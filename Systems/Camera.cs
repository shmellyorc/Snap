namespace Snap.Systems;

public class Camera
{

	private Vect2 _viewCenter, _viewport;
	private readonly SFView _view;
	private readonly Screen _screen;
	private Vect2 _position, _lastDirtyPosition;

	// Shake:
	private float _shakeDuration;         // total seconds of shake
	private float _shakeTimeRemaining;    // seconds left to shake
	private float _shakeMagnitude;        // initial magnitude (in world‐units)

	// Camera‐fly tween state:
	private bool _isFlying;
	private Vect2 _flyStart;
	private Vect2 _flyTarget;
	private float _flyDuration;
	private float _flyElapsed;
	private EaseType _flyEase;
	private Entity _followTarget;

	public Vect2 Offset { get; set; }
	public Vect2 Position
	{
		get => _position;
		set
		{
			if (_position == value)
				return;
			_position = value;

			CullBounds = Rect2.FromCenter(_position, _viewport);
			_view.Center = _position;
			_screen.UpdateDirtyState(DirtyState.Update);
		}
	}

	public Rect2 CullBounds { get; private set; }
	public Rect2 Clamp { get; set; }

	public Camera(Screen screen)
	{
		_isFlying = false;
		_shakeDuration = 0f;
		_shakeTimeRemaining = 0f;
		_shakeMagnitude = 0f;

		_screen = screen;

		var s = EngineSettings.Instance;

		_viewport = s.Viewport;          // e.g. (1280, 720)
		_viewCenter = _viewport / 2f;    // e.g. (640, 360)

		_view = new SFView(new SFRectF(0f, 0f, _viewport.X, _viewport.Y));

		Position = _viewport / 2f;     // e.g. (160, 90)
									   // (Position setter will set CullBounds and _view.Center)

		// Don't remove or change. This makes it so stacking
		// LDTK tiles appear properly. Don't touch or mess with.
		_lastDirtyPosition = Position;
	}

	public void StopFollow() => _followTarget = null;

	public void Follow(Vect2 target, float duration, EaseType ease)
	{
		_followTarget = null;

		if (duration <= 0f)
		{
			// Instant snap, no tween
			Position = target;
			_isFlying = false;
			return;
		}

		_isFlying = true;
		_flyStart = Position;
		_flyTarget = target;
		_flyDuration = duration;
		_flyElapsed = 0f;
		_flyEase = ease;
	}

	public void Follow(Entity entity, bool teleport)
	{
		if (entity == null)
		{
			// No entity to fly to—snap to origin or just do nothing.
			return;
		}

		if (teleport)
			Position = entity.Position;

		_followTarget = entity;
		_screen.UpdateDirtyState(DirtyState.Update);
	}

	private Vect2 _orginalOffset;

	public void StartShake(float duration, float magnitude)
	{
		if (duration <= 0f || magnitude <= 0f)
		{
			// No shake if inputs are invalid.
			_shakeTimeRemaining = 0f;
			return;
		}

		_shakeDuration = duration;
		_shakeTimeRemaining = duration;
		_shakeMagnitude = magnitude;
		_orginalOffset = Position;
	}

	internal void Update(float dt)
	{
		if (_isFlying)
		{
			UpdateFly(dt);
			return;
		}

		Vect2 targetPos = _followTarget != null
			? _followTarget.Position
			: Position;

		float tau = 0.3f; // ~0.3s time constant
		float rawT = 1f - MathF.Exp(-dt / tau);
		float t = Easing.Ease(EaseType.Linear, rawT);

		Vect2 desired = Vect2.Lerp(Position, targetPos, t);

		if (!Clamp.IsZero)
		{
			desired.X = Math.Clamp(
				desired.X,
				Clamp.Left + _viewCenter.X,
				Clamp.Right - _viewCenter.X
			);
			desired.Y = Math.Clamp(
				desired.Y,
				Clamp.Top + _viewCenter.Y,
				Clamp.Bottom - _viewCenter.Y
			);
		}

		if (_shakeTimeRemaining > 0f)
		{
			_shakeTimeRemaining -= dt;
			if (_shakeTimeRemaining < 0f)
				_shakeTimeRemaining = 0f;

			float normalized = _shakeTimeRemaining / _shakeDuration; // 1→0
			float currentMag = _shakeMagnitude * normalized;
			float offsetX = FastRandom.Instance.RangeFloat(-1, 1f) * currentMag;
			float offsetY = FastRandom.Instance.RangeFloat(-1, 1f) * currentMag;
			desired += new Vect2(offsetX, offsetY);

			if(_shakeTimeRemaining <= 0)
				desired = _orginalOffset;
		}

		Position = desired;

		const float pixelThreshold = 1.0f;
		if ((desired - _lastDirtyPosition).Length() >= pixelThreshold)
		{
			_screen.UpdateDirtyState(DirtyState.Update);
		}
	}

	private void UpdateFly(float dt)
	{
		_flyElapsed += dt;
		if (_flyElapsed >= _flyDuration)
		{
			// Tween is done
			Position = _flyTarget;
			_isFlying = false;
			return;
		}

		float tNorm = _flyElapsed / _flyDuration;
		float tEase = Easing.Ease(_flyEase, tNorm);

		Vect2 newPos = Vect2.Lerp(_flyStart, _flyTarget, tEase);

		Position = newPos;
	}


	internal SFView ToEngine => _view;
}
