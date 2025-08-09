namespace Snap.Engine;

public sealed class EngineSettings
{
	private const uint MinimumAtlasPageSize = 512;
	private const uint MinPages = 3;
	private const uint MaxPages = 8;
	private const int MinBatchIncrease = 32, MaxBatchIncrease = 1024;



	public static EngineSettings Instance { get; private set; }
	public bool Initialized { get; private set; }

	public EngineSettings() => Instance ??= this;




	public int BatchIncreasment { get; private set; }
	public EngineSettings WithBatchIncreasment(uint value)
	{
		if (value < MinBatchIncrease || value > MaxBatchIncrease)
			throw new Exception();

		BatchIncreasment = (int)value;

		return this;
	}




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
		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentNullException(nameof(value), "Save directory path cannot be null or empty.");

		if (value.Contains(Path.PathSeparator))
			throw new ArgumentException($"Save directory path cannot contain the path separator character '{Path.PathSeparator}'.", nameof(value));

		SaveDirectory = value;

		return this;
	}


	public string LogDirectory { get; private set; }
	public EngineSettings WithLogDirectory(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentNullException(nameof(value), "Log directory path cannot be null or whitespace.");
		if (value.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
			throw new ArgumentException("Log directory path contains invalid characters.", nameof(value));
		if (value.Contains(Path.PathSeparator))
			throw new ArgumentException($"Log directory path cannot contain the path separator character '{Path.PathSeparator}'.", nameof(value));

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

	public bool VSync { get; internal set; }
	public EngineSettings WithVSync(bool value)
	{
		VSync = value;

		return this;
	}



	public bool FullScreen { get; internal set; }
	public EngineSettings WithFullScreen(bool value)
	{
		FullScreen = value;

		return this;
	}



	public int Antialiasing { get; internal set; }
	public EngineSettings WithAntialiasing(uint value)
	{
		Antialiasing = (int)value;

		return this;
	}



	public bool WindowResize { get; private set; }
	public EngineSettings WithWindowResize(bool value)
	{
		WindowResize = value;

		return this;
	}

	public int DrawCallCache { get; private set; }
	public EngineSettings WithDrawCallCache(uint value)
	{
		const uint minimumDrawllCallCacheSize = 512;

		if (value < minimumDrawllCallCacheSize)
		{
			throw new ArgumentOutOfRangeException(nameof(value),
				$"Draw call cache size must be at least {minimumDrawllCallCacheSize}.");
		}

		DrawCallCache = (int)value;

		return this;
	}


	public int AtlasPageSize { get; private set; }
	public EngineSettings WithAtlasPageSize(uint value)
	{
		if (value < MinimumAtlasPageSize)
		{
			throw new ArgumentOutOfRangeException(nameof(value),
				$"Atlas page size must be at least {MinimumAtlasPageSize}.");
		}

		AtlasPageSize = (int)value;

		return this;
	}

	public int MaxAtlasPages { get; private set; }
	public EngineSettings WithMaxAtlasPages(uint value)
	{
		if (value < MinPages || value > MaxPages)
		{
			throw new ArgumentOutOfRangeException(nameof(value),
				$"Max atlas pages must be between {MinPages} and {MaxPages} inclusive.");
		}

		MaxAtlasPages = (int)value;

		return this;
	}




	public float DeadZone { get; private set; }
	public EngineSettings WithDeadZone(float value)
	{
		if (value < 0 || value > 1.0f)
		{
			throw new ArgumentOutOfRangeException(nameof(value),
				"Dead zone must be between 0.0 and 1.0 inclusive.");
		}

		DeadZone = (int)value;

		return this;
	}



	public InputMap InputMap { get; private set; }
	public EngineSettings WithInputMap(InputMap value)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value), "Input map cannot be null.");
		if (value._actions.Count == 0)
			throw new InvalidOperationException("Actions collection must contain at least one item.");

		InputMap = value;

		return this;
	}


	public string AppTitle { get; private set; }
	public EngineSettings WithAppTitle(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentNullException(nameof(value), "App title cannot be null.");

		AppTitle = value;

		return this;
	}

	public string AppName { get; private set; }
	public EngineSettings WithAppName(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentNullException(nameof(value), "App name cannot be null.");

		AppName = value;

		return this;
	}

	public string AppContentRoot { get; private set; }
	public EngineSettings WithAppContentRoot(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentNullException(nameof(value), "App coontent root cannot be null, empty, or whitespace.");
		if (!Directory.Exists(value))
			throw new DirectoryNotFoundException($"The specified content root directory does not exist: '{value}'.");

		AppContentRoot = value;

		return this;
	}

	public Vect2 Window { get; internal set; }
	public EngineSettings WithWindow(uint width, uint height)
	{
		if (width == 0)
			throw new ArgumentOutOfRangeException(nameof(width), "Window width must be greater than zero");
		if (height == 0)
			throw new ArgumentOutOfRangeException(nameof(width), "Window height must be greater than zero");

		Window = new Vect2(width, height);

		return this;
	}

	public Vect2 Viewport { get; private set; }
	public EngineSettings WithViewport(uint width, uint height)
	{
		if (width == 0)
			throw new ArgumentOutOfRangeException(nameof(width), "Viewport width must be greater than zero");
		if (height == 0)
			throw new ArgumentOutOfRangeException(nameof(width), "Viewport height must be greater than zero");

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
			throw new ArgumentException("Clear color must be fully opaque (Alpha = 255).", nameof(color));

		ClearColor = color;

		return this;
	}

	public Screen[] Screens { get; private set; }
	public EngineSettings WithScreens(params Screen[] values)
	{
		if (values == null)
			throw new ArgumentNullException(nameof(values), "Values cannot be null.");
		if (values.Length == 0)
			throw new ArgumentException("At least one screen must be provided");

		Screens = values;

		return this;
	}

	public GameService[] Services { get; private set; }
	public EngineSettings WithService(params GameService[] values)
	{
		if (values == null)
			throw new ArgumentNullException(nameof(values), "Values cannot be null.");
		if (values.Length == 0)
			throw new ArgumentException("At least one service must be provided");

		Services = values;

		return this;
	}


	public int LogFileSizeCap { get; private set; }
	public EngineSettings WithLogFileCap(uint value)
	{
		const uint MaxLogFileSizeBytes = 50;
		const uint BytesPerMB = 1_048_576;

		if (value == 0 || value > MaxLogFileSizeBytes)
		{
			throw new ArgumentOutOfRangeException(nameof(value),
				$"Log file size must be between 1 and {MaxLogFileSizeBytes} megabytes.");
		}

		LogFileSizeCap = (int)(value * BytesPerMB);

		return this;
	}


	public int LogMaxRecentEntries { get; private set; }
	public EngineSettings WithLogMaxRecentEntries(uint value)
	{
		const uint MaxEntries = 99;

		if (value == 0 || value > MaxEntries)
		{
			throw new ArgumentOutOfRangeException(nameof(value),
				$"Log max recent entries must be between 1 and {MaxEntries} inclusive.");
		}

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
		{
			throw new ArgumentException(
				"Company must be provided and cannot be empty or whitespace.", nameof(AppCompany));
		}

		if (string.IsNullOrWhiteSpace(AppName))
		{
			throw new ArgumentException(
				"AppName must be provided and cannot be empty or whitespace.", nameof(AppName));
		}

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
			{
				AppContentRoot = "Content";
			}
			else if (Directory.Exists("Assets"))
			{
				AppContentRoot = "Assets";
			}
			else
			{
				throw new DirectoryNotFoundException(
					"No content directory found. Expected to find either a 'Content' or 'Assets' folder.");
			}
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

		BatchIncreasment = BatchIncreasment > 0 ? BatchIncreasment : MinBatchIncrease;

		Initialized = true;

		return this;
	}
}
