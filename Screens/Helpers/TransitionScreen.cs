using System.Collections;

using Snap.Coroutines.Routines.Animations;
using Snap.Coroutines.Routines.Conditionals;
using Snap.Coroutines.Routines.Time;
using Snap.Entities.Graphics;
using Snap.Helpers;
using Snap.Systems;

namespace Snap.Screens.Helpers;

public class TransitionScreen : Screen
{
	private readonly Screen[] _screens;
	private ColorRect _rect;

	public Color Color { get; set; } = Engine.Instance.Settings.ClearColor;
	public EaseType EaseIn { get; set; } = EaseType.Linear;
	public EaseType EaseOut { get; set; } = EaseType.Linear;
	public float EaseInTime { get; set; } = 0.5f;
	public float EaseOutTime { get; set; } = 0.5f;

	public TransitionScreen(params Screen[] screens)
	{
		_screens = screens;

		Layer = 100; // Default to 100
	}

	protected override void OnEnter()
	{
		AddEntity(_rect = new ColorRect()
		{
			Color = Color * 0f,
		});

		StartRoutine(Transition());

		base.OnEnter();
	}

	private IEnumerator Transition()
	{
		var toRemove = ScreenManager.Screens.Where(x => x != this).ToList();

		yield return new Tween<float>(0f, 1f, EaseInTime, EaseIn, MathHelpers.SmoothLerp, (f) => _rect.Color = Color * f);

		foreach (var screen in toRemove)
		{
			if (screen == this)
				continue;

			ScreenManager.Remove(screen);

			yield return new WaitUntil(() => GetScreen(screen) == null);
		}

		yield return new WaitForNextFrame();

		foreach (var screen in _screens)
		{
			AddScreen(screen);

			yield return new WaitUntil(() => GetScreen(screen) != null);
		}

		yield return new WaitForNextFrame();

		yield return new Tween<float>(1f, 0f, EaseOutTime, EaseOut, MathHelpers.SmoothLerp, (f) => _rect.Color = Color * f);

		ExitScreen();
	}
}
