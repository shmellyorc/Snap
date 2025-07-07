namespace Snap;

public sealed class EngineSettings
{
	public static EngineSettings Instance { get; private set; }
	public bool Initialized { get; private set; }

	public EngineSettings() => Instance ??= this;





	public bool HalfTexelOffset { get; private set; }
	public EngineSettings WithHalfTexelOffset(bool value)
	{
		HalfTexelOffset = value;

		return this;
	}


	public LogLevel LogLevel { get; private set; }
	public EngineSettings WithLogLevel(LogLevel value)
	{
		LogLevel = value;

		return this;
	}


	public string SaveDirectory { get; private set; }
	public EngineSettings WithSaveDirectory(string value)
	{
		if (value.IsEmpty())
			throw new Exception();
		if (value.Contains(Path.PathSeparator))
			throw new Exception();

		SaveDirectory = value;

		return this;
	}


	public string LogDirectory { get; private set; }
	public EngineSettings WithLogDirectory(string value)
	{
		if (value.IsEmpty())
			throw new Exception();
		if (value.Contains(Path.PathSeparator))
			throw new Exception();

		LogDirectory = value;

		return this;
	}


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


	public string AppCompany { get; private set; }
	public EngineSettings WithAppCompany(string value)
	{
		AppCompany = value;

		return this;
	}


	// Allow partial paths from pathfinding...
	public bool AllowPartialPaths { get; private set; }
	public EngineSettings WithAllowPartialPaths(bool value)
	{
		AllowPartialPaths = value;

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


	public int LogFileSizeCap { get; private set; }
	public EngineSettings WithLogFileCap(uint value)
	{
		if (value == 0 || value >= 50_000_000)
			throw new Exception();

		LogFileSizeCap = (int)value;

		return this;
	}


	public int LogMaxRecentEntries { get; private set; }
	public EngineSettings WithLogMaxRecentEntries(uint value)
	{
		if (value == 0 || value >= 100)
			throw new Exception();

		LogMaxRecentEntries = (int)value;

		return this;
	}


	public EngineSettings Build()
	{
		if (Initialized)
			return this;

		// Screens
		if (Screens is null || Screens.Length == 0)
			throw new InvalidOperationException("At least one screen must be configured.");

		// Company & AppName
		if (string.IsNullOrWhiteSpace(AppCompany))
			throw new ArgumentException(
				"Company must be provided and cannot be empty or whitespace.", nameof(AppCompany));
		if (string.IsNullOrWhiteSpace(AppName))
			throw new ArgumentException(
				"AppName must be provided and cannot be empty or whitespace.", nameof(AppName));

		// AppTitle
		AppTitle = string.IsNullOrWhiteSpace(AppTitle)
			? "Game" : AppTitle.Trim();
		LogDirectory = string.IsNullOrWhiteSpace(LogDirectory)
			? "Logs" : LogDirectory.Trim();
		SaveDirectory = string.IsNullOrWhiteSpace(SaveDirectory)
			? "Saves" : SaveDirectory.Trim();

		// Content root
		if (string.IsNullOrWhiteSpace(AppContentRoot))
		{
			if (Directory.Exists("Content"))
				AppContentRoot = "Content";
			else if (Directory.Exists("Assets"))
				AppContentRoot = "Assets";
			else
				throw new DirectoryNotFoundException(
					"No content directory found. Expected to find either a 'Content' or 'Assets' folder.");
		}

		// Window & Viewport defaults
		if (Window.X <= 0 || Window.Y <= 0)
			Window = new Vect2(1280, 720);
		if (Viewport.X <= 0 || Viewport.Y <= 0)
			Viewport = new Vect2(320, 180);

		// ClearColor default
		if (ClearColor == Color.Transparent)
			ClearColor = Color.CornFlowerBlue;

		// InputMap
		InputMap ??= new DefaultInputMap();

		// Atlas & cache defaults
		MaxAtlasPages = MaxAtlasPages > 0 ? MaxAtlasPages : 6;
		AtlasPageSize = AtlasPageSize > 0 ? AtlasPageSize : 512;
		DrawCallCache = DrawCallCache > 0 ? DrawCallCache : 512;
		DeadZone = DeadZone > 0 ? DeadZone : 0.2f;

		// Logfiles:
		LogFileSizeCap = LogFileSizeCap > 0 ? LogFileSizeCap : 1_000_000;
		LogMaxRecentEntries = LogMaxRecentEntries > 0 ? LogMaxRecentEntries : 100;

		SafeRegion = SafeRegion > 0 ? SafeRegion : 8;

		Initialized = true;
		return this;
	}
}
