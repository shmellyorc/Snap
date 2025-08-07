namespace Snap.Entities.Graphics;

public enum ProgressDirection
{
	LeftToRight,
	RightToLeft,
	TopToBottom,
	BottomToTop
}

public class Progress : Entity
{
	private RenderTarget? _rt;
	private bool _rtChecked;
	private Texture _bg, _fg;

	public ProgressDirection Direction { get; set; } = ProgressDirection.LeftToRight;
	public Color BgColor { get; set; } = Color.White;
	public Color FgColor { get; set; } = Color.Blue;
	public float Min { get; set; } = 0f;
	public float Max { get; set; } = 1f;
	public float Value { get; set; } = 0f;
	public bool Rounded = false;

	protected override void OnEnter()
	{
		_bg = new Texture(Vect2.One, Color.White);
		_fg = new Texture(Vect2.One, Color.White);

		base.OnEnter();
	}

	protected override void OnUpdate()
	{
		if (!_rtChecked)
		{
			this.TryGetAncestorOfType(out _rt);
			_rtChecked = true;
		}

		// Ensure Min is not greater than Max
		if (Min > Max)
			(Min, Max) = (Max, Min);

		float clampedValue = Math.Clamp(Value, Min, Max);
		float range = Max - Min;
		float percent = range > 0f ? (clampedValue - Min) / range : 0f;

		Vect2 fillSize;
		Vect2 fgPos = Position;

		switch (Direction)
		{
			case ProgressDirection.LeftToRight:
				fillSize = Rounded
					? new Vect2(MathF.Round(Size.X * percent), Size.Y)
					: new Vect2(Size.X * percent, Size.Y);
				break;

			case ProgressDirection.RightToLeft:
				fillSize = Rounded
					? new Vect2(MathF.Round(Size.X * percent), Size.Y)
					: new Vect2(Size.X * percent, Size.Y);
				fgPos.X += Size.X - fillSize.X;
				break;

			case ProgressDirection.TopToBottom:
				fillSize = Rounded
					? new Vect2(Size.X, MathF.Round(Size.Y * percent))
					: new Vect2(Size.X, Size.Y * percent);
				break;

			case ProgressDirection.BottomToTop:
				fillSize = Rounded
					? new Vect2(Size.X, MathF.Round(Size.Y * percent))
					: new Vect2(Size.X, Size.Y * percent);
				fgPos.Y += Size.Y - fillSize.Y;
				break;

			default:
				fillSize = Size;
				break;
		}

		Rect2 bgRect = new(Position, Size);
		Rect2 fgRect = new(fgPos, fillSize);

		if (_rt != null)
		{
			Vect2 world = this.GetGlobalPosition();
			Vect2 rtWorld = _rt.GetGlobalPosition();
			Vect2 local = world - rtWorld;

			bgRect.Position = local;
			fgRect.Position = local + (fgPos - Position);

			_rt.DrawBypassAtlas(_bg, bgRect, BgColor, Layer);
			_rt.DrawBypassAtlas(_fg, fgRect, FgColor, Layer);
		}
		else
		{
			Renderer.DrawBypassAtlas(_bg, bgRect, BgColor, Layer);
			Renderer.DrawBypassAtlas(_fg, fgRect, FgColor, Layer);
		}

		base.OnUpdate();
	}
}
