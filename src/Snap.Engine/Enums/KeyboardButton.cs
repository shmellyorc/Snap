namespace Snap.Engine.Enums;

/// <summary>
/// Represents all supported keyboard keys for input handling.
/// </summary>
public enum KeyboardButton
{
	/// <summary>
	/// An unrecognized or unspecified key.
	/// </summary>
	Unknown = -1,

	/// <summary>
	/// The A key.
	/// </summary>
	A = 0,

	/// <summary>
	/// The B key.
	/// </summary>
	B = 1,

	/// <summary>
	/// The C key.
	/// </summary>
	C = 2,

	/// <summary>
	/// The D key.
	/// </summary>
	D = 3,

	/// <summary>
	/// The E key.
	/// </summary>
	E = 4,

	/// <summary>
	/// The F key.
	/// </summary>
	F = 5,

	/// <summary>
	/// The G key.
	/// </summary>
	G = 6,

	/// <summary>
	/// The H key.
	/// </summary>
	H = 7,

	/// <summary>
	/// The I key.
	/// </summary>
	I = 8,

	/// <summary>
	/// The J key.
	/// </summary>
	J = 9,

	/// <summary>
	/// The K key.
	/// </summary>
	K = 10,

	/// <summary>
	/// The L key.
	/// </summary>
	L = 11,

	/// <summary>
	/// The M key.
	/// </summary>
	M = 12,

	/// <summary>
	/// The N key.
	/// </summary>
	N = 13,

	/// <summary>
	/// The O key.
	/// </summary>
	O = 14,

	/// <summary>
	/// The P key.
	/// </summary>
	P = 15,

	/// <summary>
	/// The Q key.
	/// </summary>
	Q = 16,

	/// <summary>
	/// The R key.
	/// </summary>
	R = 17,

	/// <summary>
	/// The S key.
	/// </summary>
	S = 18,

	/// <summary>
	/// The T key.
	/// </summary>
	T = 19,

	/// <summary>
	/// The U key.
	/// </summary>
	U = 20,

	/// <summary>
	/// The V key.
	/// </summary>
	V = 21,

	/// <summary>
	/// The W key.
	/// </summary>
	W = 22,

	/// <summary>
	/// The X key.
	/// </summary>
	X = 23,

	/// <summary>
	/// The Y key.
	/// </summary>
	Y = 24,

	/// <summary>
	/// The Z key.
	/// </summary>
	Z = 25,

	/// <summary>
	/// The 0 key.
	/// </summary>
	Num0 = 26,

	/// <summary>
	/// The 1 key.
	/// </summary>
	Num1 = 27,

	/// <summary>
	/// The 2 key.
	/// </summary>
	Num2 = 28,

	/// <summary>
	/// The 3 key.
	/// </summary>
	Num3 = 29,

	/// <summary>
	/// The 4 key.
	/// </summary>
	Num4 = 30,

	/// <summary>
	/// The 5 key.
	/// </summary>
	Num5 = 31,

	/// <summary>
	/// The 6 key.
	/// </summary>
	Num6 = 32,

	/// <summary>
	/// The 7 key.
	/// </summary>
	Num7 = 33,

	/// <summary>
	/// The 8 key.
	/// </summary>
	Num8 = 34,

	/// <summary>
	/// The 9 key.
	/// </summary>
	Num9 = 35,

	/// <summary>
	/// The Escape key.
	/// </summary>
	Escape = 36,

	/// <summary>
	/// The left Control key.
	/// </summary>
	LControl = 37,

	/// <summary>
	/// The left Shift key.
	/// </summary>
	LShift = 38,

	/// <summary>
	/// The left Alt key.
	/// </summary>
	LAlt = 39,

	/// <summary>
	/// The left system (Windows/Apple) key.
	/// </summary>
	LSystem = 40,

	/// <summary>
	/// The right Control key.
	/// </summary>
	RControl = 41,

	/// <summary>
	/// The right Shift key.
	/// </summary>
	RShift = 42,

	/// <summary>
	/// The right Alt key.
	/// </summary>
	RAlt = 43,

	/// <summary>
	/// The right system (Windows/Apple) key.
	/// </summary>
	RSystem = 44,

	/// <summary>
	/// The Menu key.
	/// </summary>
	Menu = 45,

	/// <summary>
	/// The [ key.
	/// </summary>
	LBracket = 46,

	/// <summary>
	/// The ] key.
	/// </summary>
	RBracket = 47,

	/// <summary>
	/// The ; key.
	/// </summary>
	Semicolon = 48,

	/// <summary>
	/// DEPRECATED: Alias for <see cref="Semicolon"/>.
	/// </summary>
	SemiColon = 48,

	/// <summary>
	/// The , key.
	/// </summary>
	Comma = 49,

	/// <summary>
	/// The . key.
	/// </summary>
	Period = 50,

	/// <summary>
	/// The ' key.
	/// </summary>
	Apostrophe = 51,

	/// <summary>
	/// DEPRECATED: Alias for <see cref="Apostrophe"/>.
	/// </summary>
	Quote = 51,

	/// <summary>
	/// The / key.
	/// </summary>
	Slash = 52,

	/// <summary>
	/// The \ key.
	/// </summary>
	Backslash = 53,

	/// <summary>
	/// DEPRECATED: Alias for <see cref="Backslash"/>.
	/// </summary>
	BackSlash = 53,

	/// <summary>
	/// The ~ (grave accent) key.
	/// </summary>
	Grave = 54,

	/// <summary>
	/// DEPRECATED: Alias for <see cref="Grave"/>.
	/// </summary>
	Tilde = 54,

	/// <summary>
	/// The = key.
	/// </summary>
	Equal = 55,

	/// <summary>
	/// The - key.
	/// </summary>
	Hyphen = 56,

	/// <summary>
	/// DEPRECATED: Alias for <see cref="Hyphen"/>.
	/// </summary>
	Dash = 56,

	/// <summary>
	/// The Space key.
	/// </summary>
	Space = 57,

	/// <summary>
	/// The Enter (Return) key.
	/// </summary>
	Enter = 58,

	/// <summary>
	/// DEPRECATED: Alias for <see cref="Enter"/>.
	/// </summary>
	Return = 58,

	/// <summary>
	/// The Backspace key.
	/// </summary>
	Backspace = 59,

	/// <summary>
	/// DEPRECATED: Alias for <see cref="Backspace"/>.
	/// </summary>
	BackSpace = 59,

	/// <summary>
	/// The Tab key.
	/// </summary>
	Tab = 60,

	/// <summary>
	/// The Page Up key.
	/// </summary>
	PageUp = 61,

	/// <summary>
	/// The Page Down key.
	/// </summary>
	PageDown = 62,

	/// <summary>
	/// The End key.
	/// </summary>
	End = 63,

	/// <summary>
	/// The Home key.
	/// </summary>
	Home = 64,

	/// <summary>
	/// The Insert key.
	/// </summary>
	Insert = 65,

	/// <summary>
	/// The Delete key.
	/// </summary>
	Delete = 66,

	/// <summary>
	/// The + (Add) key on the keypad.
	/// </summary>
	Add = 67,

	/// <summary>
	/// The - (Subtract) key on the keypad.
	/// </summary>
	Subtract = 68,

	/// <summary>
	/// The * (Multiply) key on the keypad.
	/// </summary>
	Multiply = 69,

	/// <summary>
	/// The / (Divide) key on the keypad.
	/// </summary>
	Divide = 70,

	/// <summary>
	/// The Left Arrow key.
	/// </summary>
	Left = 71,

	/// <summary>
	/// The Right Arrow key.
	/// </summary>
	Right = 72,

	/// <summary>
	/// The Up Arrow key.
	/// </summary>
	Up = 73,

	/// <summary>
	/// The Down Arrow key.
	/// </summary>
	Down = 74,

	/// <summary>
	/// The numpad 0 key.
	/// </summary>
	Numpad0 = 75,

	/// <summary>
	/// The numpad 1 key.
	/// </summary>
	Numpad1 = 76,

	/// <summary>
	/// The numpad 2 key.
	/// </summary>
	Numpad2 = 77,

	/// <summary>
	/// The numpad 3 key.
	/// </summary>
	Numpad3 = 78,

	/// <summary>
	/// The numpad 4 key.
	/// </summary>
	Numpad4 = 79,

	/// <summary>
	/// The numpad 5 key.
	/// </summary>
	Numpad5 = 80,

	/// <summary>
	/// The numpad 6 key.
	/// </summary>
	Numpad6 = 81,

	/// <summary>
	/// The numpad 7 key.
	/// </summary>
	Numpad7 = 82,

	/// <summary>
	/// The numpad 8 key.
	/// </summary>
	Numpad8 = 83,

	/// <summary>
	/// The numpad 9 key.
	/// </summary>
	Numpad9 = 84,

	/// <summary>
	/// The F1 function key.
	/// </summary>
	F1 = 85,

	/// <summary>
	/// The F2 function key.
	/// </summary>
	F2 = 86,

	/// <summary>
	/// The F3 function key.
	/// </summary>
	F3 = 87,

	/// <summary>
	/// The F4 function key.
	/// </summary>
	F4 = 88,

	/// <summary>
	/// The F5 function key.
	/// </summary>
	F5 = 89,

	/// <summary>
	/// The F6 function key.
	/// </summary>
	F6 = 90,

	/// <summary>
	/// The F7 function key.
	/// </summary>
	F7 = 91,

	/// <summary>
	/// The F8 function key.
	/// </summary>
	F8 = 92,

	/// <summary>
	/// The F9 function key.
	/// </summary>
	F9 = 93,

	/// <summary>
	/// The F10 function key.
	/// </summary>
	F10 = 94,

	/// <summary>
	/// The F11 function key.
	/// </summary>
	F11 = 95,

	/// <summary>
	/// The F12 function key.
	/// </summary>
	F12 = 96,

	/// <summary>
	/// The F13 function key.
	/// </summary>
	F13 = 97,

	/// <summary>
	/// The F14 function key.
	/// </summary>
	F14 = 98,

	/// <summary>
	/// The F15 function key.
	/// </summary>
	F15 = 99,

	/// <summary>
	/// The Pause/Break key.
	/// </summary>
	Pause = 100,

	/// <summary>
	/// The total count of keyboard keys defined.
	/// </summary>
	KeyCount = 101
}
