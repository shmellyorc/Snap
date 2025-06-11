using Snap.Inputs;
using Snap.Screens;
using Snap.Services;
using Snap.Systems;

namespace Snap;

public sealed class EngineSettings
{
	public static EngineSettings Instance { get; private set; }
	public bool Initialized { get; private set; }


	public EngineSettings() => Instance ??= this;

	public bool Mouse { get; private set; }
	public EngineSettings WithMouse(bool value)
	{
		Mouse = value;

		return this;
	}

	public bool DebugDraw { get; private set; }
	public EngineSettings WithDebugDraw(bool value)
	{
		DebugDraw = value;

		return this;
	}

	public bool VSync { get; private set; }
	public EngineSettings WithVSync(bool value)
	{
		VSync = value;

		return this;
	}


	public int DrawCallCache { get; private set; }
	public EngineSettings WithDrawCallCache(uint value)
	{
		if (value < 512)
			throw new Exception();

		DrawCallCache = (int)value;

		return this;
	}


	public int AtlasPageSize { get; private set; }
	public EngineSettings WithAtlasPageSize(uint value)
	{
		if (value < 512)
			throw new Exception();

		AtlasPageSize = (int)value;

		return this;
	}

	public int MaxAtlasPages { get; private set; }
	public EngineSettings WithMaxAtlasPages(uint value)
	{
		if (value < 3 || value > 8)
			throw new Exception();

		MaxAtlasPages = (int)value;

		return this;
	}




	public float DeadZone { get; private set; }
	public EngineSettings WithDeadZone(float value)
	{
		if (value < 0 || value > 1.0f)
			throw new Exception();

		DeadZone = (int)value;

		return this;
	}



	public InputMap InputMap { get; private set; }
	public EngineSettings WithInputMap(InputMap value)
	{
		if (value == null)
			throw new Exception();

		InputMap = value;

		return this;
	}


	public string AppTitle { get; private set; }
	public EngineSettings WithAppTitle(string value)
	{
		if (value.IsEmpty())
			throw new Exception();

		AppTitle = value;

		return this;
	}

	public string AppName { get; private set; }
	public EngineSettings WithAppName(string value)
	{
		if (value.IsEmpty())
			throw new Exception();

		AppName = value;

		return this;
	}

	public string AppContentRoot { get; private set; }
	public EngineSettings WithAppContentRoot(string value)
	{
		if (value.IsEmpty())
			throw new Exception();
		if (!Directory.Exists(value))
			throw new Exception();

		AppContentRoot = value;

		return this;
	}

	public Vect2 Window { get; private set; }
	public EngineSettings WithWindow(uint width, uint height)
	{
		if (width == 0 || height == 0)
			throw new Exception();

		Window = new Vect2(width, height);

		return this;
	}

	public Vect2 Viewport { get; private set; }
	public EngineSettings WithViewport(uint width, uint height)
	{
		if (width == 0 || height == 0)
			throw new Exception();

		Viewport = new Vect2(width, height);

		return this;
	}

	public uint SafeRegion { get; private set; }
	public EngineSettings WithSafeRegion(uint value)
	{
		SafeRegion = value;

		return this;
	}

	public Color ClearColor { get; private set; }
	public EngineSettings WithClearColor(Color color)
	{
		if (color.A != 255)
			throw new Exception();

		ClearColor = color;

		return this;
	}

	public Screen[] Screens { get; private set; }
	public EngineSettings WithScreens(params Screen[] screens)
	{
		if (screens == null || screens.Length == 0)
			throw new Exception();

		Screens = screens;

		return this;
	}

	public GameService[] Services { get; private set; }
	public EngineSettings WithService(params GameService[] values)
	{
		if (values == null || values.Length == 0)
			throw new Exception();

		Services = values;

		return this;
	}


	public EngineSettings Build()
	{
		if (Initialized)
			return this;

		if (Screens == null || Screens.Length == 0)
			throw new Exception();// Engine requires at least one screen

		if (AppTitle.IsEmpty())
			AppTitle = "Game";

		if (AppContentRoot.IsEmpty())
		{
			if (Directory.Exists("Content"))
				AppContentRoot = "Content";
			else if (Directory.Exists("Assets"))
				AppContentRoot = "Assets";
			else
				throw new Exception();
		}

		if (Window.X <= 0 || Window.Y <= 0)
			Window = new Vect2(1280, 720);
		if (Viewport.X <= 0 || Viewport.Y <= 0)
			Viewport = new Vect2(320, 180);
		if (ClearColor.R == 0 && ClearColor.G == 0 && ClearColor.B == 0)
			ClearColor = Color.CornFlowerBlue;
		if (InputMap == null)
			InputMap = new DefaultInputMap();
		if (MaxAtlasPages == 0)
			MaxAtlasPages = 6;
		if (AtlasPageSize == 0)
			AtlasPageSize = 512;
		if (DrawCallCache == 0)
			DrawCallCache = 512;
		if(DeadZone <= 0)
			DeadZone = 0.2f;

		Initialized = true;

		return this;
	}
}
