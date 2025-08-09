namespace Snap.Engine.Inputs;

/// <summary>
/// Represents the built-in input actions supported by the engine's default input map.
/// Used when no custom <see cref="InputMap"/> is defined by the developer.
/// </summary>
public enum DefaultInputs
{
	/// <summary>Move the character or cursor left.</summary>
	MoveLeft,

	/// <summary>Move the character or cursor right.</summary>
	MoveRight,

	/// <summary>Move the character or cursor up.</summary>
	MoveUp,

	/// <summary>Move the character or cursor down.</summary>
	MoveDown,

	/// <summary>Triggers the primary action or confirmation.</summary>
	Accept,

	/// <summary>Triggers the cancel or back action.</summary>
	Cancel,

	/// <summary>Represents any other extra or context-specific action.</summary>
	Other,
}

/// <summary>
/// Default input map used when the developer does not define a custom <see cref="InputMap"/>.
/// Maps <see cref="DefaultInputs"/> to common keyboard and gamepad buttons.
/// </summary>
/// <remarks>
/// This ensures basic movement and action bindings are always available for prototyping or fallback control schemes.
/// </remarks>
public sealed class DefaultInputMap : InputMap
{
	/// <summary>
	/// Initializes the default input map with standard keyboard and gamepad bindings for core actions.
	/// </summary>
	/// <remarks>
	/// This constructor sets up default bindings for <see cref="DefaultInputs"/>:
	/// <list type="bullet">
	///   <item><description><b>MoveLeft:</b> A, Left Arrow, DPadLeft</description></item>
	///   <item><description><b>MoveRight:</b> D, Right Arrow, DPadRight</description></item>
	///   <item><description><b>MoveUp:</b> W, Up Arrow, DPadUp</description></item>
	///   <item><description><b>MoveDown:</b> S, Down Arrow, DPadDown</description></item>
	///   <item><description><b>Accept:</b> E, A Button</description></item>
	///   <item><description><b>Cancel:</b> F, X Button</description></item>
	///   <item><description><b>Other:</b> Q, Y Button</description></item>
	/// </list>
	/// This setup ensures that common input actions work out-of-the-box for both keyboard and gamepad.
	/// </remarks>
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
