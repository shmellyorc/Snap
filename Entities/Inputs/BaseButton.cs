namespace Snap.Entities.Inputs;

public class BaseButton : Entity
{
	private bool _disabled, _wasHovering, _wasPressed;

	public bool Disabled
	{
		get => _disabled;
		set
		{
			if (_disabled == value)
				return;
			_disabled = value;

			if (_disabled)
				OnButtonDisabled();
			else
				OnButtonUp();
		}
	}

	public Action<BaseButton> Pressed { get; set; }

	protected virtual void OnButtonUp() { }
	protected virtual void OnButtonDown() { }
	protected virtual void OnButtonHover() { }
	protected virtual void OnButtonDisabled() { }

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}
}
