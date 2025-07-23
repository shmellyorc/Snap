namespace Snap.Entities.Inputs;

public enum BaseButtonPressedState
{
	None,
	Pressed,
	Released,
}

public class BaseButton : Entity
{
	private bool _disabled, _wasHovering, _wasPressed, _isDirty;

	public BaseButtonPressedState ButtonState { get; set; } = BaseButtonPressedState.Released;

	public bool Disabled
	{
		get => _disabled;
		set
		{
			if (_disabled == value)
				return;
			_disabled = value;
			_isDirty = true;
		}
	}

	public Action<BaseButton> Pressed { get; set; }

	protected virtual void OnButtonUp() { }
	protected virtual void OnButtonDown() { }
	protected virtual void OnButtonHover() { }
	protected virtual void OnButtonDisabled() { }

	protected override void OnUpdate()
	{
		if (_isDirty)
		{
			if (_disabled)
				OnButtonDisabled();
			else
				OnButtonUp();

			_isDirty = false;
		}

		if (_disabled)
			return;

		var rect = Bounds;
		var pos = Input.Transform(Input.MousePosition, Camera);

		if (Input.IsMousePressed(MouseButton.Left))
		{
			if (rect.Contains(pos))
			{
				if (!_wasPressed)
				{
					_wasPressed = true;
					_wasHovering = false;
					OnButtonDown();

					if (ButtonState == BaseButtonPressedState.Pressed)
						Pressed?.Invoke(this);
				}
			}
		}
		else
		{
			if (rect.Contains(pos))
			{
				if (_wasPressed)
				{
					_wasPressed = false;
					OnButtonUp();

					if (ButtonState == BaseButtonPressedState.Released)
						Pressed?.Invoke(this);
				}
				else if (!_wasHovering)
				{
					_wasHovering = true;
					OnButtonHover();
				}
			}
			else if (_wasHovering || _wasPressed)
			{
				_wasHovering = false;
				_wasPressed = false;
				OnButtonUp();
			}
		}

		base.OnUpdate();
	}
}
