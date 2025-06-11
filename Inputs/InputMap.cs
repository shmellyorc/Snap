using Snap.Enums;
using Snap.Helpers;
using Snap.Inputs.Types;
using Snap.Logs;
using Snap.Screens;
using Snap.Systems;

namespace Snap.Inputs;

public enum ActiveInput
{
	Keyboard,
	Gamepad
}
public class InputMap
{

	private uint _joyCount;
	private bool _mouseJustPressed, _keyJustPressed, _joystickJustPressed;
	private readonly Dictionary<uint, bool> _joysticks = new();
	private Dictionary<uint, List<InputMapEntry>> _actions = new(20);

	public ActiveInput Current { get; private set; }
	public Vect2 MousePosition => SFMouse.GetPosition();


	#region Constructor
	public InputMap()
	{
		SFJoystick.Update();

		_joyCount = SFJoystick.Count;
		_joysticks = new Dictionary<uint, bool>((int)_joyCount);

		for (uint i = 0; i < _joyCount; i++)
			_joysticks[i] = SFJoystick.IsConnected(i);
	}
	#endregion


	#region Keyboard
	public bool IsKeyPressed(KeyboardButton button)
	{
		if (!Engine.Instance.IsActive)
			return false;

		var result = SFKeyboard.IsKeyPressed((SFKey)button);

		if (result)
			Current = ActiveInput.Keyboard;

		return result;
	}
	public bool IsKeyReleased(KeyboardButton button) => !IsKeyPressed(button);
	public bool IsKeyJustPressed(KeyboardButton button)
	{
		if (_keyJustPressed)
			return false;

		if (IsKeyPressed(button))
		{
			_keyJustPressed = true;


			return true;
		}

		return false;
	}
	#endregion


	#region Mouse
	public bool IsMousePressed(MouseButton button)
	{
		if (!Engine.Instance.IsActive)
			return false;

		return SFMouse.IsButtonPressed((SFMouseButton)button);
	}
	public bool IsMouseReleased(MouseButton button) => !IsMousePressed(button);
	public bool IsMouseJustPressed(MouseButton button)
	{

		if (_mouseJustPressed)
			return false;

		if (IsMouseJustPressed(button))
		{
			_mouseJustPressed = true;

			return true;
		}

		return false;
	}
	#endregion


	#region Gamepad
	public bool IsGamepadReleased(GamepadButton button)
	{
		if (!Engine.Instance.IsActive)
			return false;

		return !IsGamepadPressed(button);
	}
	public bool IsGamepadJustPressed(GamepadButton button)
	{
		if (!Engine.Instance.IsActive)
			return false;
		if (_joystickJustPressed)
			return false;

		if (IsGamepadPressed(button))
		{
			_joystickJustPressed = true;
			return true;
		}

		return false;
	}

	public float GetGamepadForce(GamepadButton button)
	{
		if (!Engine.Instance.IsActive)
			return 0f;

		SFJoystick.Update();

		var joys = _joysticks
			.Where(x => x.Value)
			.Select(x => x.Key);

		foreach (var joyId in joys)
		{
			float result = button switch
			{
				GamepadButton.AButton => SFJoystick.IsButtonPressed(joyId, 0) ? 1f : 0f,
				GamepadButton.BButton => SFJoystick.IsButtonPressed(joyId, 1) ? 1f : 0f,
				GamepadButton.XButton => SFJoystick.IsButtonPressed(joyId, 2) ? 1f : 0f,
				GamepadButton.YButton => SFJoystick.IsButtonPressed(joyId, 3) ? 1f : 0f,
				GamepadButton.LeftBumper => SFJoystick.IsButtonPressed(joyId, 4) ? 1f : 0f,
				GamepadButton.RightBumper => SFJoystick.IsButtonPressed(joyId, 5) ? 1f : 0f,
				GamepadButton.Back => SFJoystick.IsButtonPressed(joyId, 6) ? 1f : 0f,
				GamepadButton.Start => SFJoystick.IsButtonPressed(joyId, 7) ? 1f : 0f,
				GamepadButton.LeftStick => SFJoystick.IsButtonPressed(joyId, 8) ? 1f : 0f,
				GamepadButton.RightStick => SFJoystick.IsButtonPressed(joyId, 9) ? 1f : 0f,
				GamepadButton.DPadLeft => GetPovX(joyId) < 0f ? 1f : 0f,
				GamepadButton.DPadRight => GetPovX(joyId) > 0f ? 1f : 0f,
				GamepadButton.DPadDown => GetPovY(joyId) < 0f ? 1f : 0f,
				GamepadButton.DPadUp => GetPovY(joyId) > 0f ? 1f : 0f,
				GamepadButton.LeftStickLeft => MathF.Max(0f, GetAxis(joyId, SFJoystickAxis.X)),
				GamepadButton.LeftStickRight => MathF.Max(0f, GetAxis(joyId, SFJoystickAxis.X)),
				GamepadButton.LeftStickUp => MathF.Max(0f, GetAxis(joyId, SFJoystickAxis.Y)),
				GamepadButton.LeftStickDown => MathF.Max(0f, GetAxis(joyId, SFJoystickAxis.Y)),
				GamepadButton.RightStickLeft => MathF.Max(0f, GetAxis(joyId, SFJoystickAxis.U)),
				GamepadButton.RightStickRight => MathF.Max(0f, GetAxis(joyId, SFJoystickAxis.U)),
				GamepadButton.RightStickUp => MathF.Max(0f, GetAxis(joyId, SFJoystickAxis.V)),
				GamepadButton.RightStickDown => MathF.Max(0f, GetAxis(joyId, SFJoystickAxis.V)),
				GamepadButton.LeftTrigger => MathF.Max(0f, GetAxis(joyId, SFJoystickAxis.Z)),
				GamepadButton.RightTrigger => MathF.Max(0f, GetAxis(joyId, SFJoystickAxis.Z)),
				_ => 0f
			};

			if (result > 0f)
			{
				Current = ActiveInput.Gamepad;

				return result;
			}
		}

		return 0f;
	}

	public bool IsGamepadPressed(GamepadButton button)
	{
		if (!Engine.Instance.IsActive)
			return false;

		SFJoystick.Update();

		var joys = _joysticks
			.Where(x => x.Value)
			.Select(x => x.Key);

		foreach (var joyId in joys)
		{
			bool result = button switch
			{
				GamepadButton.AButton => SFJoystick.IsButtonPressed(joyId, 0),
				GamepadButton.BButton => SFJoystick.IsButtonPressed(joyId, 1),
				GamepadButton.XButton => SFJoystick.IsButtonPressed(joyId, 2),
				GamepadButton.YButton => SFJoystick.IsButtonPressed(joyId, 3),
				GamepadButton.LeftBumper => SFJoystick.IsButtonPressed(joyId, 4),
				GamepadButton.RightBumper => SFJoystick.IsButtonPressed(joyId, 5),
				GamepadButton.Back => SFJoystick.IsButtonPressed(joyId, 6),
				GamepadButton.Start => SFJoystick.IsButtonPressed(joyId, 7),
				GamepadButton.LeftStick => SFJoystick.IsButtonPressed(joyId, 8),
				GamepadButton.RightStick => SFJoystick.IsButtonPressed(joyId, 9),

				// Axis:
				GamepadButton.DPadLeft => GetPovX(joyId) < -DeadZone,
				GamepadButton.DPadRight => GetPovX(joyId) > DeadZone,
				GamepadButton.DPadDown => GetPovY(joyId) < -DeadZone,
				GamepadButton.DPadUp => GetPovY(joyId) > DeadZone,
				GamepadButton.LeftStickLeft => GetAxis(joyId, SFJoystickAxis.X) < -DeadZone,
				GamepadButton.LeftStickRight => GetAxis(joyId, SFJoystickAxis.X) > DeadZone,
				GamepadButton.LeftStickUp => GetAxis(joyId, SFJoystickAxis.Y) < -DeadZone,
				GamepadButton.LeftStickDown => GetAxis(joyId, SFJoystickAxis.Y) > DeadZone,
				GamepadButton.RightStickLeft => GetAxis(joyId, SFJoystickAxis.U) < -DeadZone,
				GamepadButton.RightStickRight => GetAxis(joyId, SFJoystickAxis.U) > DeadZone,
				GamepadButton.RightStickUp => GetAxis(joyId, SFJoystickAxis.V) < -DeadZone,
				GamepadButton.RightStickDown => GetAxis(joyId, SFJoystickAxis.V) > DeadZone,
				GamepadButton.LeftTrigger => GetAxis(joyId, SFJoystickAxis.Z) < -DeadZone,
				GamepadButton.RightTrigger => GetAxis(joyId, SFJoystickAxis.Z) > DeadZone,

				_ => false
			};

			if (result)
			{
				Current = ActiveInput.Gamepad;

				return true;
			}
		}

		return false;
	}
	#endregion


	#region Transform
	public static Vect2 Transform(Vect2 position, Camera camera)
	{
		var w = Engine.Instance.ToRenderer;

		return w.MapPixelToCoords(position, camera.ToEngine);
	}
	public static Vect2 Transform(Vect2 position, Screen screen) =>
		Transform(position, screen.Camera);
	#endregion


	#region Initialization/De-Initializaiton
	internal void Load()
	{
		var w = Engine.Instance.ToRenderer;

		w.JoystickButtonReleased += OnJoystickButtonReleased;
		w.JoystickConnected += OnJoystickConnected;
		w.JoystickDisconnected += OnJoystickDisconnected;
		w.MouseButtonReleased += OnMouseButtonReleased;
		w.KeyReleased += OnKeyReleased;
	}

	internal void Unload()
	{
		var w = Engine.Instance.ToRenderer;

		w.JoystickButtonReleased -= OnJoystickButtonReleased;
		w.JoystickConnected -= OnJoystickConnected;
		w.JoystickDisconnected -= OnJoystickDisconnected;
		w.MouseButtonReleased -= OnMouseButtonReleased;
		w.KeyReleased -= OnKeyReleased;
	}

	private void OnKeyReleased(object sender, SFKeyEventArgs e) => _keyJustPressed = false;
	private void OnMouseButtonReleased(object sender, SFMouseButtonEventArgs e) => _mouseJustPressed = false;
	private void OnJoystickButtonReleased(object sender, SFJoystickButtonEventArgs e) => _joystickJustPressed = false;
	private void OnJoystickDisconnected(object sender, SFJoystickConnectEventArgs e)
	{
		SFJoystick.Update();
		_joysticks[e.JoystickId] = SFJoystick.IsConnected(e.JoystickId);

		Logger.Instance.Log(LogLevel.Info, $"Gamepad disconnected on ID: {e.JoystickId}, joysticks ID is now: {_joysticks[e.JoystickId]}");
	}
	private void OnJoystickConnected(object sender, SFJoystickConnectEventArgs e)
	{
		SFJoystick.Update();
		_joysticks[e.JoystickId] = SFJoystick.IsConnected(e.JoystickId);

		Logger.Instance.Log(LogLevel.Info, $"Gamepad cconnected on ID: {e.JoystickId}, joysticks ID is now: {_joysticks[e.JoystickId]}");
	}
	#endregion


	#region Actions
	public bool IsActionPressed(Enum name) => IsActionPressed(name.ToEnumString());
	public bool IsActionPressed(string name)
	{
		var hash = HashHelpers.Hash32(name);

		if (!_actions.TryGetValue(hash, out var actions))
			return false;

		foreach (var action in actions)
		{
			if (action is KeyboardInputState keyboard)
			{
				var result = IsKeyPressed(keyboard.ValueAs<KeyboardButton>());

				if (result)
					return true;
			}

			if (action is MouseInputState mouse)
			{
				var result = IsMousePressed(mouse.ValueAs<MouseButton>());

				if (result)
					return true;
			}

			if (action is GamepadInputState gamepad)
			{
				var result = IsGamepadPressed(gamepad.ValueAs<GamepadButton>());

				if (result)
					return true;
			}
		}

		return false;
	}

	public bool TryGetActionForce(Enum name, out float output) => 
		TryGetActionForce(name.ToEnumString(), out output);
	public bool TryGetActionForce(string name, out float output)
	{
		output = GetActionForce(name);
		return output > 0f;
	}

	public float GetActionForce(Enum name) => GetActionForce(name.ToEnumString());
	public float GetActionForce(string name)
	{
		var hash = HashHelpers.Hash32(name);

		if (!_actions.TryGetValue(hash, out var actions))
			return 0f;

		foreach (var action in actions)
		{
			if (action is KeyboardInputState keyboard)
			{
				var result = IsKeyPressed(keyboard.ValueAs<KeyboardButton>());

				if (result)
					return 1f;
			}

			if (action is MouseInputState mouse)
			{
				var result = IsMousePressed(mouse.ValueAs<MouseButton>());

				if (result)
					return 1f;
			}

			if (action is GamepadInputState gamepad)
			{
				var result = GetGamepadForce(gamepad.ValueAs<GamepadButton>());

				if (result > 0f)
					return result;
			}
		}

		return 0f;
	}


	public bool IsActionJustPressed(Enum name) => IsActionJustPressed(name.ToEnumString());
	public bool IsActionJustPressed(string name)
	{
		var hash = HashHelpers.Hash32(name);

		if (!_actions.TryGetValue(hash, out var actions))
			return false;

		foreach (var action in actions)
		{
			if (action is KeyboardInputState keyboard)
			{
				var result = IsKeyJustPressed(keyboard.ValueAs<KeyboardButton>());

				if (result)
					return true;
			}

			if (action is MouseInputState mouse)
			{
				var result = IsMouseJustPressed(mouse.ValueAs<MouseButton>());

				if (result)
					return true;
			}

			if (action is GamepadInputState gamepad)
			{
				var result = IsGamepadJustPressed(gamepad.ValueAs<GamepadButton>());

				if (result)
					return true;
			}
		}

		return false;
	}

	public bool IsActionReleased(Enum name) => IsActionReleased(name.ToEnumString());
	public bool IsActionReleased(string name)
	{
		var hash = HashHelpers.Hash32(name);

		if (!_actions.TryGetValue(hash, out var actions))
			return false;

		foreach (var action in actions)
		{
			if (action is KeyboardInputState keyboard)
			{
				var result = IsKeyReleased(keyboard.ValueAs<KeyboardButton>());

				if (result)
					return true;
			}

			if (action is MouseInputState mouse)
			{
				var result = IsMouseReleased(mouse.ValueAs<MouseButton>());

				if (result)
					return true;
			}

			if (action is GamepadInputState gamepad)
			{
				var result = IsGamepadReleased(gamepad.ValueAs<GamepadButton>());

				if (result)
					return true;
			}
		}

		return false;
	}

	public void AddAction(Enum name, params object[] inputs) => AddAction(name.ToEnumString(), inputs);
	public void AddAction(string name, params object[] inputs)
	{
		var hash = HashHelpers.Hash32(name);

		if (!_actions.TryGetValue(hash, out var i))
			_actions[hash] = new List<InputMapEntry>(GetInputs(inputs));
		else
			i.AddRange(GetInputs(inputs));
	}
	#endregion


	#region Private Methods
	private List<InputMapEntry> GetInputs(object[] inputs)
	{
		if (inputs == null || inputs.Length == 0)
			return new();

		var result = new List<InputMapEntry>(inputs.Length);

		for (int i = 0; i < inputs.Length; i++)
		{
			var current = inputs[i];

			if (current is KeyboardButton keyboard)
				result.Add(new KeyboardInputState(keyboard));
			else if (current is MouseButton mouse)
				result.Add(new MouseInputState(mouse));
			else if (current is GamepadButton gamepad)
				result.Add(new GamepadInputState(gamepad));
		}

		return result;
	}

	private float GetAxis(uint joyId, SFJoystickAxis axis)
	{
		var raw = SFJoystick.GetAxisPosition(joyId, axis);
		var norm = Math.Clamp(raw / 100f, -1f, 1f);
		return MathF.Abs(norm) > EngineSettings.Instance.DeadZone ? norm : 0f;

	}

	private float GetPovX(uint joyId) => GetAxis(joyId, SFJoystickAxis.PovX);
	private float GetPovY(uint joyUd) => GetAxis(joyUd, SFJoystickAxis.PovY);
	private float DeadZone => EngineSettings.Instance.DeadZone;
	#endregion
}
