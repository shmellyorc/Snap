﻿namespace Snap;

public class WindowCreationException : Exception
{
	public WindowCreationException(string message)
		: base(message) { }

	public WindowCreationException(string message, Exception inner)
		: base(message, inner) { }
}

public class Engine : IDisposable
{
	private const int TotalFpsQueueSamples = 64;

	private SFRenderWindow _window;
	private SFStyles _styles;
	private SFContext _context;
	SFVideoMode _videoMode;
	private bool _isDisposed, _initialized;
	private readonly Queue<float> _fpsQueue = new();
	private float _titleTimeout;
	private SFImage _icon;
	private bool _canApplyChanges;

	public static Engine Instance { get; private set; }
	public EngineSettings Settings { get; }
	public bool IsActive { get; private set; } = true;
	public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
	public string VersionHash => $"{HashHelpers.Hash64(Version):X8}";
	public InputMap Input { get; private set; }
	public string ApplicationFolder => FileHelpers.GetApplicationData(Settings.AppCompany, Settings.AppName);
	public string ApplicationLogFolder => Path.Combine(ApplicationFolder, Settings.LogDirectory);
	public string ApplicationSaveFolder => Path.Combine(ApplicationFolder, Settings.SaveDirectory);


	public void ApplyFullScreenChange(bool value)
	{
		if (Settings.FullScreen == value)
			return;

		Settings.FullScreen = value;
		_canApplyChanges = true;
	}

	public void ApplyWindowSizeChange(uint width, uint height)
	{
		if (width <= 0)
			return;
		if (height <= 0)
			return;
		if (Settings.Window.X == width && Settings.Window.Y == height)
			return;

		Settings.Window = new Vect2(width, height);
		_canApplyChanges = true;
	}

	public void ApplyVSyncChange(bool value)
	{
		if (Settings.VSync == value)
			return;

		Settings.VSync = value;
		_canApplyChanges = true;
	}

	public void ApplyAntialiasingChange(uint value)
	{
		if (Settings.Antialiasing == value)
			return;

		Settings.Antialiasing = (int)value;
		_canApplyChanges = true;
	}

	public void ApplyChanges()
	{
		if (!_canApplyChanges)
			return;

		if (_window != null && _window.IsInvalid)
		{
			Input.Unload();

			_window.Closed -= OnWindowClose;
			_window.GainedFocus -= OnGainedFocus;
			_window.LostFocus -= OnLostFocus;

			_window.Close();
			_window.Dispose();
		}

		_videoMode = new SFVideoMode((uint)Settings.Window.X, (uint)Settings.Window.Y);
		_context = new SFContext { MinorVersion = 3, MajorVersion = 3, AntialiasingLevel = (uint)Settings.Antialiasing };

		_styles = Settings.WindowResize
			? SFStyles.Titlebar | SFStyles.Resize | SFStyles.Close
			: SFStyles.Titlebar | SFStyles.Close;

		if (Settings.FullScreen)
			_styles |= SFStyles.Fullscreen;

		try
		{
			_window = new SFRenderWindow(_videoMode, Settings.AppTitle, _styles, _context);

			if (_window.IsInvalid || !_window.IsOpen)
			{
				throw new WindowCreationException(
					"Failed to create SNAP window. Make sure your GPU supports OpenGl 3.3 or greater."
				);
			}

			_log.Log(LogLevel.Info, "Window successfully created.");

			_window.SetIcon(_icon.Size.X, _icon.Size.Y, _icon.Pixels);
			_window.SetVerticalSyncEnabled(Settings.VSync);
			_window.SetMouseCursorVisible(Settings.Mouse);
			_window.Closed += OnWindowClose;
			_window.GainedFocus += OnGainedFocus;
			_window.LostFocus += OnLostFocus;

			Input.Load();
		}
		catch (WindowCreationException wex)
		{
			_log.Log(LogLevel.Error, wex.Message);
			_log.LogException(wex);
			throw; // re-throw so upstream knows we’re fatally broken
		}
		catch (Exception ex)
		{
			// any other unexpected issue
			_log.Log(LogLevel.Error, "Unexpected error during window creation.");
			_log.LogException(ex);

			throw new WindowCreationException("Unexpected error while creating SNAP window.", ex);
		}
	}

	private void OnLostFocus(object sender, EventArgs e) => IsActive = false;
	private void OnGainedFocus(object sender, EventArgs e) => IsActive = true;

	private void OnWindowClose(object sender, EventArgs e)
	{
		if (!_window.IsOpen)
			return;

		_window.Close();
	}

	// Systems:
	private readonly Logger _log;
	private readonly Clock _clock;
	private readonly BeaconManager _beacon;
	private readonly AssetManager _assets;
	private readonly FastRandom _rand;
	private readonly Renderer _renderer;
	private readonly SoundManager _soundManager;
	private readonly ScreenManager _screenManager;
	private readonly DebugRenderer _debugRenderer;
	private readonly ServiceManager _serviceManager;
	private readonly CoroutineManager _coroutineManager;
	private readonly TextureAtlasManager _textureAtlasManager;

	public Engine(EngineSettings settings)
	{
		if (settings is null)
			throw new ArgumentNullException(nameof(settings));
		if (!settings.Initialized)
		{
			throw new InvalidOperationException(
				"Cannot create Engine: EngineSettings must be initialized. " +
				"Make sure you call EngineSettingsBuilder.Build() before passing it in."
			);
		}

		_icon = new SFImage(EmbeddedResources.GetAppIcon());

		Instance ??= this;
		Settings = settings;

		// Before setting any folders for log, etc. Make suer they exist:
		CreateFolder(ApplicationFolder, "Application data root");
		CreateFolder(ApplicationLogFolder, "Application log folder");
		CreateFolder(ApplicationSaveFolder, "Application save folder");

		_log = new Logger(Settings.LogLevel, Settings.LogMaxRecentEntries);
		_log.AddSink(new FileLogSink(ApplicationLogFolder, Settings.LogFileSizeCap, Settings.LogMaxRecentEntries));

		_log.Log(LogLevel.Info, "────────────────────────────────────────────────────────────");
		_log.Log(LogLevel.Info, "           ███████═╗ ███══╗ ██═╗  █████══╗ ██████══╗");
		_log.Log(LogLevel.Info, "           ██ ╔════╝ ████ ╚╗██ ║ ██ ╔═██ ║ ██ ╔═██ ║");
		_log.Log(LogLevel.Info, "           ███████═╗ ██ ██ ╚██ ║ ███████ ║ ██████ ╔╝");
		_log.Log(LogLevel.Info, "            ╚═══██ ║ ██ ║██ ██ ║ ██ ╔═██ ║ ██ ╔═══╝");
		_log.Log(LogLevel.Info, "           ███████ ║ ██ ║ ████ ║ ██ ║ ██ ║ ██ ║");
		_log.Log(LogLevel.Info, "            ╚══════╝ ╚══╝  ╚═══╝ ╚══╝ ╚══╝ ╚══╝");
		_log.Log(LogLevel.Info, "────────────────────────────────────────────────────────────");
		_log.Log(LogLevel.Info, $"         Version: {Version}, Hash: {VersionHash}");
		_log.Log(LogLevel.Info, "────────────────────────────────────────────────────────────");

		_styles = Settings.WindowResize
			? SFStyles.Titlebar | SFStyles.Resize | SFStyles.Close
			: SFStyles.Titlebar | SFStyles.Close;

		if (Settings.FullScreen)
			_styles |= SFStyles.Fullscreen;

		_log.Log(LogLevel.Info, $"Initializing video mode: {Settings.Window.X}x{Settings.Window.Y}");
		_videoMode = new SFVideoMode((uint)Settings.Window.X, (uint)Settings.Window.Y);

		_context = new SFContext { MinorVersion = 3, MajorVersion = 3, AntialiasingLevel = (uint)Settings.Antialiasing };
		_log.Log(LogLevel.Info, $"Creating OpenGL context: Version {_context.MajorVersion}.{_context.MinorVersion}, Antialiasing: {_context.AntialiasingLevel}");

		try
		{
			_window = new SFRenderWindow(_videoMode, Settings.AppTitle, _styles, _context);

			if (_window.IsInvalid || !_window.IsOpen)
			{
				throw new WindowCreationException(
					"Failed to create SNAP window. Make sure your GPU supports OpenGl 3.3 or greater."
				);
			}

			_log.Log(LogLevel.Info, "Window successfully created.");

			_window.SetIcon(_icon.Size.X, _icon.Size.Y, _icon.Pixels);
		}
		catch (WindowCreationException wex)
		{
			_log.Log(LogLevel.Error, wex.Message);
			_log.LogException(wex);
			throw; // re-throw so upstream knows we’re fatally broken
		}
		catch (Exception ex)
		{
			// any other unexpected issue
			_log.Log(LogLevel.Error, "Unexpected error during window creation.");
			_log.LogException(ex);

			throw new WindowCreationException("Unexpected error while creating SNAP window.", ex);
		}

		_window.Closed += (_, _) => _window.Close();
		_window.GainedFocus += (_, _) => IsActive = true;
		_window.LostFocus += (_, _) => IsActive = false;

		// Happens only when app crashes, make sure to report:
		AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs args) =>
		{
			if (args.ExceptionObject is Exception ex)
				_log.LogException(ex);

			_log.Log(LogLevel.Warning, "SNAP force Stopped\n");
		};

		// Only triggers if app doesnt crash:
		AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs args) =>
		{
			_log.Log(LogLevel.Info, "SNAP Stopped\n");
		};

		_log.Log(LogLevel.Info, $"Vsync been set to: {settings.VSync}.");
		_window.SetVerticalSyncEnabled(settings.VSync);

		_log.Log(LogLevel.Info, $"Mouse visbility been set to: {settings.Mouse}.");
		_window.SetMouseCursorVisible(settings.Mouse);

		_log.Log(LogLevel.Info, $"Initializing SNAP core services...");

		_log.Log(LogLevel.Info, $"Initializing input mappings...");
		Input = settings.InputMap;

		_log.Log(LogLevel.Info, $"Initializing Clock...");
		_clock = new Clock();

		_log.Log(LogLevel.Info, "Initializing Beacon Manager...");
		_beacon = new BeaconManager();

		_log.Log(LogLevel.Info, "Initializing Asset Manager...");
		_assets = new AssetManager();

		_log.Log(LogLevel.Info, "Initializing FastRandom...");
		_rand = new FastRandom();

		_log.Log(LogLevel.Info, $"Initializing Renderer Manager with {Settings.DrawCallCache} cached draw calls.");
		_renderer = new Renderer(Settings.DrawCallCache);

		_log.Log(LogLevel.Info, "Initializing Sound Manager...");
		_soundManager = new SoundManager();

		_log.Log(LogLevel.Info, "Initializing Debug Renderer Manager...");
		_debugRenderer = new DebugRenderer();

		_log.Log(LogLevel.Info, "Initializing Screen Manager...");
		_screenManager = new ScreenManager();

		_log.Log(LogLevel.Info, "Initializing Service Manager...");
		_serviceManager = new ServiceManager();

		_log.Log(LogLevel.Info, "Initializing Coroutine Manager...");
		_coroutineManager = new CoroutineManager();

		_log.Log(LogLevel.Info, $"Initializing Texture Atlas manager. Page size: {Settings.AtlasPageSize} with max {Settings.MaxAtlasPages} pages");
		_textureAtlasManager = new TextureAtlasManager(512, 3);
	}
	~Engine() => Dispose(disposing: false);


	public void Quit()
	{
		if (_window == null || !_window.IsOpen)
			return;

		_window.Close();
	}


	private void CreateFolder(string path, string description)
	{
		try
		{
			Directory.CreateDirectory(path);
		}
		catch (Exception ex)
		{
			_log.LogException(ex);
			throw new IOException($"Unable to create {description} at '{path}'", ex);
		}
	}

	public void Run()
	{
		if (_window.IsInvalid)
			throw new InvalidOperationException("Window is invalid. Cannot start engine.");

		_log.Log(LogLevel.Info, "Loading InputMap...");
		Input.Load();

		// init
		if (!_initialized)
		{
			if (Settings.Services != null && Settings.Services.Length > 0)
			{
				_log.Log(LogLevel.Info, $"Adding {Settings.Services.Length} service{(Settings.Services.Length > 1 ? "s" : string.Empty)}.");
				for (int i = 0; i < Settings.Services.Length; i++)
					_serviceManager.RegisterService(Settings.Services[i]);
			}

			if (Settings.Screens != null && Settings.Screens.Length > 0)
			{
				_log.Log(LogLevel.Info, $"Adding {Settings.Screens.Length} screen{(Settings.Screens.Length > 1 ? "s" : string.Empty)}.");
				_screenManager.Add(Settings.Screens);
			}

			_initialized = true;
		}

		while (_window.IsOpen)
		{
			_window.DispatchEvents();
			_clock.Update();
			_coroutineManager.Update();

			UpdateTitle();

			_window.Clear(Settings.ClearColor);
			_screenManager.Update();
			_window.Display();
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			_assets.Clear();
			_soundManager.Clear();
			_screenManager.Clear();
			_window?.Dispose();

			_isDisposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void UpdateTitle()
	{
		if (_fpsQueue.Count >= TotalFpsQueueSamples)
			_fpsQueue.Dequeue();

		_fpsQueue.Enqueue(1f / _clock.DeltaTime);

		if (_titleTimeout >= 0.000001f)
			_titleTimeout -= _clock.DeltaTime;
		else
		{
			var sb = new StringBuilder(1024);
			var tEntity = _screenManager.Screens.Sum(x => x.Entities.Count);
			var aEntity = _screenManager.Screens.Sum(x => x.ActiveEntities.Count);

			double BytesToMib(long bytes) => bytes / 1024.0 / 1024.0;

			sb.Append($"{Settings.AppTitle} | ");

			// Fps: Fps (AvgFps)
			sb.Append($"Fps: {(1f / _clock.DeltaTime):0} ({_fpsQueue.Average():0} avg) | ");

			// Entity: ActiveEntity/total Entities
			sb.Append($"Entity: {aEntity}/{tEntity} | ");

			// Assets: Bytes, Active/Total
			sb.Append($"Assets: {BytesToMib(_assets.BytesLoaded):0.00}MB, {_assets.Count}/{_assets.TotalCount} Assets Loaded | ");

			// Rendering: Draws, Batches
			sb.Append($"Batch: Draws: {_renderer.DrawCalls}, Batches: {_renderer.Batches} | ");

			// Atlas Manager: 1/8, <percent of ratio used>
			sb.Append($"Atlas: {TextureAtlasManager.Instance.Pages}/{TextureAtlasManager.Instance.MaxPages} Pages, {(TextureAtlasManager.Instance.TotalFillRatio * 100f):0}% Filled | ");

			// // Coroutines: <number>
			sb.Append($"Routines: {CoroutineManager.Instance.Count} | ");

			// // Beacon (PubSub): <number>
			sb.Append($"Beacon: {BeaconManager.Instance.Count} | ");

			// // Sounds:
			sb.Append($"Sound: Playing: {_soundManager.PlayCount}, Banks: {_soundManager.Count}");

			_window.SetTitle(sb.ToString());

			_titleTimeout += 1.0f;
		}
	}



	public SFVideoMode CurrentMonitor => SFVideoMode.DesktopMode;

	public List<SFVideoMode> GetSupportedMonitors(int wRatio, int hRatio, float tolerance = 0.01f)
	{
		float ratio = (float)wRatio / hRatio;

		return SFVideoMode.FullscreenModes
			.Where(mode =>
			{
				float actualRatio = (float)mode.Width / mode.Height;
				return Math.Abs(actualRatio - ratio) < tolerance;
			})
			.ToList();
	}

	internal SFRenderWindow ToRenderer => _window;
}
