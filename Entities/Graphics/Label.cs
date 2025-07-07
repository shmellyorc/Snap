namespace Snap.Entities.Graphics;

public sealed class Label : Entity
{
	private string _text = string.Empty;
	private bool _isDirty = true;
	private readonly List<string> _displayText = [];
	private readonly Font _font;
	private int _visibleLength = -1; // Default: Show full text
	private readonly Dictionary<string, Vect2> _cachedMeasurements = [];
	private float _cachedMaxHeight = 0f;
	private RenderTarget? _rt;
	private bool _rtChecked;

	public Vect2 ShadowOffset { get; set; } = Vect2.One;
	public Vect2 Offset { get; set; } = Vect2.Zero;
	public Color Color { get; set; } = Color.White;
	public Color ShadowColor { get; set; } = Color.Black * 0.2f;
	public bool Shadow { get; set; }
	public HAlign HAlign { get; set; }
	public VAlign VAlign { get; set; }
	public int Length => _text.Length;

	public string Text
	{
		get => _text;
		set
		{
			if (_text == value)
				return;
			_text = value;
			_isDirty = true;
		}
	}

	public new Vect2 Size
	{
		get => base.Size;
		set
		{
			if (base.Size == value)
				return;
			base.Size = value;
			_isDirty = true;
		}
	}

	public int VisibleLength
	{
		get => _visibleLength;
		set
		{
			if (_visibleLength == value)
				return;

			_visibleLength = value < 0 ? -1 : Math.Max(0, value); // Keep full text if -1
			_isDirty = true;
		}
	}

	public Label(Font font) => _font = font;

	protected override void OnUpdate()
	{
		if (!_rtChecked)
		{
			this.TryGetAncestorOfType(out _rt);
			_rtChecked = true;
		}

		if (_isDirty)
		{
			_displayText.Clear();
			_cachedMeasurements.Clear(); // Clear previous measurements

			var processedText = _visibleLength == -1 ? _text : _text.TrimToLength(_visibleLength);

			var lines = _font.FormatText(processedText, (int)Size.X).Split(Environment.NewLine);

			foreach (var line in lines)
			{
				if (line.IsEmpty())
				{
					_displayText.Add("");
					_cachedMeasurements[""] = _font.Measure(" ");
					continue;
				}

				var trimmedLine = line.Trim();

				_displayText.Add(trimmedLine);
				_cachedMeasurements[trimmedLine] = _font.Measure(trimmedLine);// Cache measurement
			}

			if (_cachedMeasurements.Count == 0)
				_cachedMeasurements[""] = _font.Measure(" ");

			_cachedMaxHeight = _displayText.Sum(x => _cachedMeasurements[x].Y); // Cached total height
			_isDirty = false;
		}

		float offset = 0f;

		for (int i = 0; i < _displayText.Count; i++)
		{
			var txt = _displayText[i];
			var measure = _cachedMeasurements[txt]; // Retrieve cached measurement
			var offsetX = AlignHelpers.AlignWidth(Size.X, measure.X, HAlign);
			var offsetY = AlignHelpers.AlignHeight(Size.Y, _cachedMaxHeight, VAlign);

			if (_rt != null)
			{
				var world = this.GetGlobalPosition();
				var rtWorld = _rt.GetGlobalPosition();
				var local = world - rtWorld;
				var final = new Vect2(local.X + offsetX, local.Y + offsetY + offset);

				if (Shadow)
					_rt.DrawText(_font, txt, final + ShadowOffset + Offset, ShadowColor, Layer);

				_rt.DrawText(_font, txt, final + Offset, Color, Layer + 1);
			}
			else
			{
				var final = new Vect2(Position.X + offsetX, Position.Y + offsetY + offset);

				if (Shadow)
					Renderer.DrawText(_font, txt, final + ShadowOffset + Offset, ShadowColor, Layer);

				Renderer.DrawText(_font, txt, final + Offset, Color, Layer + 1);
			}

			offset += measure.Y;
		}

		base.OnUpdate();
	}
}
