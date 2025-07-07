namespace Snap.Inputs;

public sealed class KeyboardInputState : InputMapEntry
{
    public KeyboardInputState(KeyboardButton value) : base(value) { }
}

public sealed class MouseInputState : InputMapEntry
{
    public MouseInputState(MouseButton value) : base(value) { }
}

public sealed class GamepadInputState : InputMapEntry
{
    public GamepadInputState(GamepadButton value) : base(value) { }
}