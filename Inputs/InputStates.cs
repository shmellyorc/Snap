using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Snap.Enums;

namespace Snap.Inputs.Types;

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