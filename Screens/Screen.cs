using System.Collections;

using Snap.Assets.Fonts;
using Snap.Assets.LDTKImporter;
using Snap.Assets.Loaders;
using Snap.Assets.Spritesheets;
using Snap.Beacons;
using Snap.Coroutines;
using Snap.Entities;
using Snap.Entities.Graphics;
using Snap.Entities.Panels;
using Snap.Graphics;
using Snap.Helpers;
using Snap.Inputs;
using Snap.Logs;
using Snap.Sounds;
using Snap.Systems;

namespace Snap.Screens;


public class Screen
{
	private int _layer;
	private bool _visible = true;
	private DirtyState _dirtyState = DirtyState.Sort | DirtyState.Update;
	private readonly List<Entity> _entities = new();
	private List<Entity> _updateEntities = new();

	public uint Id { get; internal set; }
	public IReadOnlyList<Entity> Entities => _entities;
	public IReadOnlyList<Entity> ActiveEntities => _updateEntities;
	public int EntityCount => Entities.Count;
	public Camera Camera { get; private set; }
	public bool IsExiting { get; private set; }
	public bool IsActive { get; private set; }
	public bool IsTopmostScreen { get; private set; }
	public bool IsUiScreen { get; set; }
	public bool IsActivScreen => IsActive && IsTopmostScreen && !IsExiting;

	public bool IsVisible
	{
		get => _visible;
		set
		{
			if (_visible == value)
				return;

			_visible = value;

			ScreenManager.UpdateDirtyState(DirtyState.Update);
		}
	}

	public int Layer
	{
		get => _layer;
		set
		{
			if (_layer == value)
				return;

			_layer = value;

			ScreenManager.UpdateDirtyState(DirtyState.Sort);
		}
	}





	#region Helpers
	public float SafeRegion => EngineSettings.Instance.SafeRegion;

	public Logger Logger => Logger.Instance;
	public Clock Clock => Clock.Instance;
	public Engine Engine => Engine.Instance;
	public FastRandom Rand => FastRandom.Instance;
	public Renderer Renderer => Renderer.Instance;
	public InputMap Input => Engine.Instance.Input;
	public AssetManager Assets => AssetManager.Instance;
	public BeaconManager Beacon => BeaconManager.Instance;
	public SoundManager SoundManager => SoundManager.Instance;
	public ScreenManager ScreenManager => ScreenManager.Instance;
	public CoroutineManager CoroutineManager => CoroutineManager.Instance;

	public Texture LoadTexture(string filename, bool repeat = false, bool smooth = false) => AssetManager.LoadTexture(filename, repeat, smooth);
	public SpriteFont LoadSpriteFont(string filename, float spacing = 0f, float lineSpacing = 0f) =>
		AssetManager.LoadSpriteFont(filename, spacing, lineSpacing);
	public LDTKProject LoadMap(string filename) => AssetManager.LoadMap(filename);
	public Spritesheet LoadSheet(string filename) => AssetManager.LoadSheet(filename);
	public Sound LoadSound(string filename, bool looped = false) => AssetManager.LoadSound(filename, looped);

	public Texture GetTexture(string name) => AssetManager.GetTexture(name);
	public Texture GetTexture(Enum name) => AssetManager.GetTexture(name);
	public LDTKProject GetMap(string name) => AssetManager.GetMap(name);
	public LDTKProject GetMap(Enum name) => AssetManager.GetMap(name);
	public Spritesheet GetSheet(string name) => AssetManager.GetSheet(name);
	public Spritesheet GetSheet(Enum name) => AssetManager.GetSheet(name);
	public Font GetFont(string name) => AssetManager.GetFont(name);
	public Font GetFont(Enum name) => AssetManager.GetFont(name);
	public BitmapFont GetBitmapFont(string name) => AssetManager.GetBitmapFont(name);
	public BitmapFont GetBitmapFont(Enum name) => AssetManager.GetBitmapFont(name);
	public SpriteFont GetSpriteFont(string name) => AssetManager.GetSpriteFont(name);
	public SpriteFont GetSpriteFont(Enum name) => AssetManager.GetSpriteFont(name);
	public Sound GetSound(string name) => AssetManager.GetSound(name);
	public Sound GetSound(Enum name) => AssetManager.GetSound(name);

	public void ExitScreen()
	{
		if (IsExiting)
			return;

		ScreenManager.Remove(this);
	}
	#endregion



	internal void EngineOnUpdate(bool isActive, bool isTopmostScreen)
	{
		IsActive = isActive;
		IsTopmostScreen = isTopmostScreen;

		Camera?.Update(Clock.DeltaTime);

		if (_entities.Count == 0)
		{
			OnUpdate();
			return;
		}

		if (_dirtyState != DirtyState.None)
		{
			IEnumerable<Entity> pipeline = _entities;

			if (_dirtyState.HasFlag(DirtyState.Update))
			{
				pipeline = pipeline
					.Where(x => x is not null && !x.IsExiting && (x.HasAncestorOfType<RenderTarget>()
						|| x.Bounds.Intersects(Camera.CullBounds)
						|| x.KeepAlive));
			}

			if (_dirtyState.HasFlag(DirtyState.Sort))
				pipeline = pipeline.OrderBy(x => x.Layer);

			_updateEntities = pipeline.ToList();

			_dirtyState = DirtyState.None;
		}

		for (int i = 0; i < _updateEntities.Count; i++)
		{
			var e = _updateEntities[i];

			if (!e.IsVisible)
				continue;

			e.EngineOnUpdate();
		}

		OnUpdate();
	}
	protected virtual void OnUpdate() { }



	internal void EngineOnEnter()
	{
		Camera = new Camera(this);
		Camera.Update(Clock.DeltaTime);

		OnEnter();
	}
	protected virtual void OnEnter() { }

	internal void EngineOnExit()
	{
		if (IsExiting)
			return;

		ClearBeacons();
		ClearRoutines();
		ClearEntities();

		IsExiting = true;

		OnExit();
	}
	protected virtual void OnExit() { }








	#region Screen
	public void AddScreen(params Screen[] screens) => ScreenManager.Add(screens);
	public void RemoveScreen(params Screen[] screens) => ScreenManager.Remove(screens);
	public T GetScreen<T>() where T : Screen => ScreenManager.Get<T>();
	public Screen GetScreen(Screen screen) => ScreenManager.Get(screen);
	public Screen GetScreenById(uint id) => ScreenManager.GetById(id);
	#endregion


	#region Entity
	public void AddEntity(params Entity[] entities)
	{
		if (entities == null || entities.Length == 0)
			return;

		for (int i = 0; i < entities.Length; i++)
		{
			var e = entities[i];
			if (e == null || e.IsExiting)
				continue;

			_entities.Add(e);

			e._screen = this;
			BeaconManager.Initialize(e);
			e.EngineOnEnter();
		}

		UpdateDirtyState(DirtyState.Sort | DirtyState.Update);
	}

	public void RemoveEntity(params Entity[] entities)
	{
		if (entities == null || entities.Length == 0)
			return;

		bool anyRemoved = false;

		for (int i = 0; i < entities.Length; i++)
		{
			var e = entities[i];

			if (e == null || e.IsExiting)
				continue;
			if (!_entities.Remove(e))
				continue;

			e.EngineOnExit();
			anyRemoved = true;
		}

		for (int i = 0; i < entities.Length; i++)
		{
			var e = entities[i];

			foreach (var p in e.GetAncestorsOfType<Panel>())
				p.SetDirtyState(DirtyState.Sort | DirtyState.Update);
		}

		if (anyRemoved)
			UpdateDirtyState(DirtyState.Sort | DirtyState.Update);
	}

	public T GetEntity<T>(int index) where T : Entity
		=> _entities.OfType<T>().ElementAtOrDefault(index);

	public bool TryGetEntity<T>(int index, out T entity) where T : Entity
	{
		entity = GetEntity<T>(index);

		return entity != null;
	}

	public void ClearEntities()
	{
		if (_entities.Count == 0)
			return;

		RemoveEntity([.. _entities]);
	}
	#endregion


	#region Beacons
	public void ConnectBeacon(string topic, Action<BeaconHandle> handler) => Beacon.Connect(topic, this, handler);
	public void ConnectBeacon(Enum topic, Action<BeaconHandle> handler) => Beacon.Connect(topic.ToEnumString(), this, handler);
	public void DisconnectBeacon(string topic, Action<BeaconHandle> handler) => Beacon.Disconnect(topic, this, handler);
	public void DisconnectBeacon(Enum topic, Action<BeaconHandle> handler) => Beacon.Disconnect(topic.ToEnumString(), this, handler);
	public void EmitBeacon(string topic, params object[] args) => Beacon.Emit(topic, args);
	public void EmitBeacon(Enum topic, params object[] args) => Beacon.Emit(topic, args);
	public void EmitBeaconDelayed(string topic, float seconds, params object[] args) => Beacon.EmitDelayed(this, topic, seconds, args);
	public void EmitBeaconDelayed(Enum topic, float seconds, params object[] args) => Beacon.EmitDelayed(this, topic.ToEnumString(), seconds, args);
	public void ClearBeacons() => Beacon.ClearOwner(this);
	#endregion


	#region Coroutines
	public CoroutineHandle StartRoutine(IEnumerator routine) => CoroutineManager.Start(routine, this);
	public CoroutineHandle StartRoutineDelayed(IEnumerator routine, float delay) => CoroutineManager.StartDelayed(delay, this, routine);
	public bool StopRoutine(CoroutineHandle handle) => CoroutineManager.Stop(handle);
	public bool HasRoutine(CoroutineHandle handle) => CoroutineManager.IsRunning(handle);
	public void ClearRoutines() => CoroutineManager.StopAll(this);
	#endregion

	internal void UpdateDirtyState(DirtyState state) => _dirtyState |= state;
}
