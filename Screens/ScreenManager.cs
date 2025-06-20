using Snap.Beacons;
using Snap.Graphics;

namespace Snap.Screens;

[Flags]
public enum DirtyState : uint // Used for Entity and ScreenManager
{
	None,
	Sort = 1 << 0, // Layer
	Update = 1 << 1, // Add or Remove
}

public sealed class ScreenManager
{
	private static uint _id;

	private List<Screen> _screens = new();
	private Dictionary<uint, Screen> _screensById = new();
	private List<Screen> _updateScreens = new();
	private DirtyState _dirtyState;

	public static ScreenManager Instance { get; private set; }
	public int Count => _screens.Count;
	public IReadOnlyList<Screen> Screens => _screens;

	internal ScreenManager() => Instance ??= this;

	internal void Update()
	{
		if (_screens.Count == 0)
			return;

		if (_dirtyState != DirtyState.None)
		{
			if (_dirtyState.HasFlag(DirtyState.Update))
			{
				_updateScreens.Clear();
				_updateScreens.EnsureCapacity(_screens.Count);

				for (int i = 0; i < _screens.Count; i++)
				{
					var s = _screens[i];

					if (s.IsExiting)
						continue;
					if (!s.IsVisible)
						continue;

					_updateScreens.Add(s);
				}
			}

			if (_dirtyState.HasFlag(DirtyState.Sort))
				_updateScreens.Sort((a, b) => a.Layer.CompareTo(b.Layer));

			_dirtyState = DirtyState.None;
		}

		var isActive = Engine.Instance.IsActive;
		var topmostScreen = _updateScreens
			.LastOrDefault(x => !x.IsUiScreen);

		foreach (var screen in _updateScreens)
		{
			if (EngineSettings.Instance.DebugDraw)
				DebugRenderer.Instance.Begin();

			Renderer.Instance.Begin(screen.Camera);

			screen.EngineOnUpdate(isActive, screen == topmostScreen);

			Renderer.Instance.End();

			if (EngineSettings.Instance.DebugDraw)
				DebugRenderer.Instance.End();
		}
	}




	public void Add(params Screen[] screens)
	{
		if (screens?.Length == 0)
			return;

		for (int i = 0; i < screens.Length; i++)
		{
			var screen = screens[i];

			screen.Id = _id++;
			BeaconManager.Initialize(screen);

			screen.EngineOnEnter();

			_screens.Add(screen);
			_screensById.Add(screen.Id, screen);
		}

		_dirtyState = DirtyState.Update | DirtyState.Sort;
	}

	public void Remove(params Screen[] screens)
	{
		if (screens?.Length == 0)
			return;

		bool anyRemoved = false;

		for (int i = screens.Length - 1; i >= 0; i--)
		{
			var screen = screens[i];

			if (_screensById.Remove(screen.Id))
				_screens.Remove(screen);
			else
				continue;

			screen.EngineOnExit();
			anyRemoved = true;
		}

		if (anyRemoved)
			_dirtyState = DirtyState.Update | DirtyState.Sort;
	}


	public T Get<T>() where T : Screen => (T)_screens.FirstOrDefault(x => x is T);

	public Screen Get(Screen screen) => _screensById.TryGetValue(screen.Id, out var s) ? s : null;
	public Screen GetById(uint id) => _screensById.TryGetValue(id, out var s) ? s : null;

	public bool TryGet<T>(out T screen) where T : Screen
	{
		screen = Get<T>();

		return screen != null;
	}

	public void Clear()
	{
		if (_screens.Count == 0)
			return;

		Remove(_screens.ToArray());

		_dirtyState = DirtyState.Update;
	}

	internal void UpdateDirtyState(DirtyState state) => _dirtyState = state;
}
