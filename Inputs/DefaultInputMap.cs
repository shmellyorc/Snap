namespace Snap.Inputs;

public enum DefaultInputs
{
	MoveLeft,
	MoveRight,
	MoveUp,
	MoveDown,
	Accept,
	Cancel,
	Other,
}

public sealed class DefaultInputMap : InputMap
{
	public DefaultInputMap()
	{
		AddAction(DefaultInputs.MoveLeft, KeyboardButton.A, KeyboardButton.Left, GamepadButton.DPadLeft);
		AddAction(DefaultInputs.MoveRight, KeyboardButton.D, KeyboardButton.Right, GamepadButton.DPadRight);
		AddAction(DefaultInputs.MoveUp, KeyboardButton.W, KeyboardButton.Up, GamepadButton.DPadUp);
		AddAction(DefaultInputs.MoveDown, KeyboardButton.S, KeyboardButton.Down, GamepadButton.DPadDown);
		AddAction(DefaultInputs.Accept, KeyboardButton.E, GamepadButton.AButton);
		AddAction(DefaultInputs.Cancel, KeyboardButton.F, GamepadButton.XButton);
		AddAction(DefaultInputs.Other, KeyboardButton.Q, GamepadButton.YButton);
	}
}
