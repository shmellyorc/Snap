using Snap.Assets.Loaders;
using Snap.Enums;
using Snap.Helpers;
using Snap.Systems;

namespace Snap.Entities.Graphics;

public sealed class Animation
{
	public string Name { get; }
	public Texture Texture { get; }
	public IReadOnlyList<Rect2> Frames { get; }
	public float Speed { get; }
	public bool Looped { get; }
	public bool IsEmpty => Name.IsEmpty();
	public float FrameDuration { get; }

	internal Animation(string name, Texture texture, Rect2[] frames, float speed, bool looped)
	{
		Name = name;
		Texture = texture;
		Frames = frames;
		Speed = speed;
		Looped = looped;

		FrameDuration = 1f / Speed;
	}
}

public sealed class AnimatedSprite : Entity
{
	private const float MinSpeed = 0.0001f;

	private readonly Dictionary<uint, Animation> _animations = new(16);
	private float _frameDelta;
	private int _frame;
	private RenderTarget? _rt;
	private bool _rtChecked;

	private uint _currentHash;

	// Micro-opt:
	// No Diretory or property lookup in OnUpdate for frames and duration;
	// Single array lookup instead of Current.Frames[Frame] (two property calls)
	// FrameDuration Fetched once per Play, not devided every frame.
	private Rect2[] _currentFrames;
	private float _currentDuration;

	public bool IsPlaying { get; private set; }
	public bool IsFinished => !IsPlaying && !Current.Looped;
	public float SpeedScale { get; set; } = 1f;
	public Animation Current { get; private set; }
	public int Frame => Current.IsEmpty ? 0 : Math.Clamp(_frame, 0, Current.Frames.Count - 1);
	public Color Color { get; set; } = Color.White;
	public Vect2 Origin { get; set; } = Vect2.Zero;
	public float Rotation { get; set; }
	public Vect2 Scale { get; set; } = Vect2.One;
	public TextureEffects Effects { get; set; }
	public HAlign HAlign { get; set; }
	public VAlign VAlign { get; set; }

	public AnimatedSprite() { }

	public AnimatedSprite AddAnimation(Enum name, Texture texture, Vect2 grid, int frame, float speed, bool looped) =>
		AddAnimation(name.ToEnumString(), texture, grid, new[] { frame }, speed, looped);
	public AnimatedSprite AddAnimation(string name, Texture texture, Vect2 grid, int frame, float speed, bool looped) =>
		AddAnimation(name, texture, grid, new[] { frame }, speed, looped);

	public AnimatedSprite AddAnimation(Enum name, Texture texture, Vect2 grid, int startFrame, int endFrame,
	float speed, bool looped) => AddAnimation(name.ToEnumString(), texture, grid, startFrame, endFrame, speed, looped);
	public AnimatedSprite AddAnimation(string name, Texture texture, Vect2 grid, int startFrame, int endFrame,
	float speed, bool looped)
	{
		if (grid.X <= 0 || grid.Y <= 0) throw new ArgumentOutOfRangeException(
			nameof(grid), grid, "Grid cell with and height must both be greaer than zero.");

		int step = endFrame >= startFrame ? 1 : -1;
		int length = Math.Abs(endFrame - startFrame) + 1;

		var frames = Enumerable
			.Range(0, length)
			.Select(i => startFrame + i * step)
			.ToArray();

		AddAnimation(name, texture, grid, frames, speed, looped);
		return this;
	}

	public AnimatedSprite AddAnimation(Enum name, Texture texture, Vect2 grid, int[] frames, float speed, bool looped) =>
		AddAnimation(name.ToEnumString(), texture, grid, frames, speed, looped);
	public AnimatedSprite AddAnimation(string name, Texture texture, Vect2 grid, int[] frames, float speed, bool looped)
	{
		if (frames == null || frames.Length == 0)
			throw new ArgumentException("Must supply at least one frame index", nameof(frames));
		if (speed < 0)
			throw new ArgumentOutOfRangeException(nameof(speed));
		if (grid.X <= 0 || grid.Y <= 0) throw new ArgumentOutOfRangeException(
			nameof(grid), grid, "Grid cell with and height must both be greaer than zero.");

		var tilesPerRow = (int)(texture.Size.X / grid.X);

		var rects = frames.Select(idx =>
		{
			var tile = MathHelpers.To2D(idx, tilesPerRow);
			var pixelPos = new Vect2(tile.X * grid.X, tile.Y * grid.Y);
			return new Rect2(pixelPos, grid);
		})
		.ToArray();

		var clampSpeed = MathF.Max(MinSpeed, speed);
		var anim = new Animation(name, texture, rects, clampSpeed, looped);
		var key = HashHelpers.Hash32(name);

		_animations[key] = anim;
		return this;
	}

	public AnimatedSprite Stop()
	{
		if (!IsPlaying)
			return this;

		IsPlaying = false;
		return this;
	}

	public AnimatedSprite Play(Enum name) => Play(name.ToEnumString(), false, true);
	public AnimatedSprite Play(string name) => Play(name, false, true);
	public AnimatedSprite Play(Enum name, bool repeat) => Play(name.ToEnumString(), repeat, true);
	public AnimatedSprite Play(string name, bool repeat) => Play(name, repeat, true);
	public AnimatedSprite Play(Enum name, bool repeat, bool reset) => Play(name.ToEnumString(), repeat, reset);
	public AnimatedSprite Play(string name, bool repeat, bool reset)
	{
		var hash = HashHelpers.Hash32(name);
		if (!_animations.TryGetValue(hash, out var anim))
			throw new KeyNotFoundException($"Unable to find animation '{name}'.");

		if (_currentHash == hash && repeat)
			return this;

		if (reset)
		{
			_frame = 0;
			_frameDelta = 0;
		}
		else
		{
			if (!Current.IsEmpty)
			{
				_frame = Math.Clamp(_frame, 0, Current.Frames.Count - 1);
				_frameDelta = 0;
			}
		}

		Size = anim.Frames.First().Size;

		Current = anim;
		_currentHash = hash;
		_currentFrames = anim.Frames as Rect2[] ?? anim.Frames.ToArray();
		_currentDuration = anim.FrameDuration;
		IsPlaying = true;

		return this;
	}

	public AnimatedSprite Pause()
	{
		if (!Current.IsEmpty)
			IsPlaying = false;
		return this;
	}

	public AnimatedSprite Resume()
	{
		if (!Current.IsEmpty)
			IsPlaying = false;
		return this;
	}

	protected override void OnUpdate()
	{
		if (IsPlaying && !Current.IsEmpty)
		{
			var FrameDuration = _currentDuration;
			var delta = Clock.DeltaTime * SpeedScale;
			_frameDelta += delta;

			while (_frameDelta >= FrameDuration)
			{
				_frameDelta -= FrameDuration;
				_frame++;

				if (_frame >= _currentFrames.Length)
				{
					if (Current.Looped)
						_frame = 0;
					else
					{
						_frame = _currentFrames.Length - 1;
						IsPlaying = false;
						break;
					}
				}
			}
		}

		if (!Current.IsEmpty)
			UpdateDraw();

		base.OnUpdate();
	}

	private void UpdateDraw()
	{
		if (!_rtChecked)
		{
			this.TryGetAncestorOfType(out _rt);
			_rtChecked = true;
		}

		if (Color.A <= 0 || !IsVisible)
			return;

		var idx = Frame;
		var frame = _currentFrames[idx];
		var size = frame.Size;
		var offsetX = AlignHelpers.AlignWidth(Size.X, size.X, HAlign);
		var offsetY = AlignHelpers.AlignHeight(Size.Y, size.Y, VAlign);

		if (_rt != null)
		{
			// world-space origin of the RT and into RT-local coords
			var world = this.GetGlobalPosition();
			var rtWorld = _rt.GetGlobalPosition();
			var local = world - rtWorld;
			var final = new Vect2(local.X + offsetX, local.Y + offsetY);

			_rt.Draw(Current.Texture, final, frame, Color, Origin, Scale, Rotation, Effects, Layer);
		}
		else
			Renderer.Draw(Current.Texture, Position, frame, Color, Origin, Scale, Rotation, Effects, Layer);
	}
}
