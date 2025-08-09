namespace Snap.Engine.Inputs;

/// <summary>
/// Represents a keyboard button binding within an <see cref="InputMap"/> entry.
/// </summary>
/// <remarks>
/// Wraps a <see cref="KeyboardButton"/> enum value as an input source tied to a specific action.
/// </remarks>
public sealed class KeyboardInputState : InputMapEntry
{
	/// <summary>
	/// Initializes a new <see cref="KeyboardInputState"/> with the specified keyboard button.
	/// </summary>
	/// <param name="value">The keyboard button to associate with this input state.</param>
	public KeyboardInputState(KeyboardButton value) : base(value) { }
}

/// <summary>
/// Represents a mouse button binding within an <see cref="InputMap"/> entry.
/// </summary>
/// <remarks>
/// Wraps a <see cref="MouseButton"/> enum value as an input source tied to a specific action.
/// </remarks>
public sealed class MouseInputState : InputMapEntry
{
	/// <summary>
	/// Initializes a new <see cref="MouseInputState"/> with the specified mouse button.
	/// </summary>
	/// <param name="value">The mouse button to associate with this input state.</param>
	public MouseInputState(MouseButton value) : base(value) { }
}

/// <summary>
/// Represents a gamepad button binding within an <see cref="InputMap"/> entry.
/// </summary>
/// <remarks>
/// Wraps a <see cref="GamepadButton"/> enum value as an input source tied to a specific action.
/// </remarks>
public sealed class GamepadInputState : InputMapEntry
{
	/// <summary>
	/// Initializes a new <see cref="GamepadInputState"/> with the specified gamepad button.
	/// </summary>
	/// <param name="value">The gamepad button to associate with this input state.</param>
	public GamepadInputState(GamepadButton value) : base(value) { }
}