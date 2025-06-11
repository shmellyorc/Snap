using System.Reflection;
using System.Text;

using Coroutines;

using Snap.Assets.Loaders;
using Snap.Beacons;
using Snap.Coroutines;
using Snap.Graphics;
using Snap.Helpers;
using Snap.Inputs;
using Snap.Logs;
using Snap.Screens;
using Snap.Services;
using Snap.Sounds;
using Snap.Systems;

namespace Snap;

public class Engine : IDisposable
{
	private const int TotalFpsQueueSamples = 32;

	private SFRenderWindow _window;
	private SFStyles _styles;
	private SFContext _context;
	SFVideoMode _videoMode;
	private bool _isDisposed, _initialized;
	private readonly Queue<float> _fpsQueue = new();
	private float _titleTimeout;

	public static Engine Instance { get; private set; }
	public EngineSettings Settings { get; }
	public bool IsActive { get; private set; } = true;
	public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
	public string VersionHash => $"{HashHelpers.Hash64(Version):X8}";

	public InputMap Input { get; private set; }

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
		if (!settings.Initialized)
			throw new ArgumentException(nameof(settings));

		Instance ??= this;
		Settings = settings;

		_log = new Logger();
		_log.Log(LogLevel.Info, "────────────────────────────────────────────────────────────");
		_log.Log(LogLevel.Info, "           ███████╗ ███╗   ██╗  █████╗  ██████╗");
		_log.Log(LogLevel.Info, "           ██╔════╝ ████╗  ██║ ██╔══██╗ ██╔══██╗");
		_log.Log(LogLevel.Info, "           ███████╗ ██╔██╗ ██║ ███████║ ██████╔╝");
		_log.Log(LogLevel.Info, "           ╚════██║ ██║╚██╗██║ ██╔══██║ ██╔═══╝");
		_log.Log(LogLevel.Info, "           ███████║ ██║ ╚████║ ██║  ██║ ██║");
		_log.Log(LogLevel.Info, "           ╚══════╝ ╚═╝  ╚═══╝ ╚═╝  ╚═╝ ╚═╝");
		_log.Log(LogLevel.Info, "────────────────────────────────────────────────────────────");
		_log.Log(LogLevel.Info, $"         Version: {Version}, Hash: {VersionHash}");
		_log.Log(LogLevel.Info, "────────────────────────────────────────────────────────────");

		_styles = SFStyles.Titlebar | SFStyles.Close;
		_videoMode = new SFVideoMode((uint)Settings.Window.X, (uint)Settings.Window.Y);
		_log.Log(LogLevel.Info, $"Initializing video mode: {Settings.Window.X}x{Settings.Window.Y}");
		_context = new SFContext { MinorVersion = 3, MajorVersion = 3, AntialiasingLevel = 0 };
		_log.Log(LogLevel.Info, $"Creating OpenGL context: Version {_context.MajorVersion}.{_context.MinorVersion}, Antialiasing: {_context.AntialiasingLevel}");
		_window = new SFRenderWindow(_videoMode, "Game", _styles, _context);

		if (!_window.IsInvalid)
			_log.Log(LogLevel.Info, "Window seccessfully created.");
		else
		{
			_log.Log(LogLevel.Error, "Window creation failed! Check settings or intialization parameters.");
			return;
		}

		_window.Closed += (_, _) => _window.Close();
		_window.GainedFocus += (_, _) => IsActive = true;
		_window.LostFocus += (_, _) => IsActive = false;

		// Happens only when app crashes, make sure to report:
		AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs args) =>
		{
			if (args.ExceptionObject is Exception ex)
			{
				var msg = new StringBuilder();
				msg.AppendLine($"Crash detected: {ex.Message}");
				msg.AppendLine($"Stack Trace:\n{ex.StackTrace}");

				_log.Log(LogLevel.Error, msg.ToString().Trim());
			}

			_log.Log(LogLevel.Info, "SNAP Stopped\n");
			_log.Stop();
		};

		// Only triggers if app doesnt crash:
		AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs args) =>
		{
			_log.Log(LogLevel.Info, "SNAP Stopped\n");
			_log.Stop();
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

	public void Run()
	{
		if (_window.IsInvalid)
			return;

		Input.Load();

		// init
		if (!_initialized)
		{
			if (Settings.Services != null && Settings.Services.Length > 0)
			{
				for (int i = 0; i < Settings.Services.Length; i++)
					_serviceManager.RegisterService(Settings.Services[i]);
			}

			if (Settings.Screens != null && Settings.Screens.Length > 0)
				_screenManager.Add(Settings.Screens);

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

		if (_titleTimeout >= 0)
			_titleTimeout -= _clock.DeltaTime;
		else
		{
			var sb = new StringBuilder();
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
			sb.Append($"Atlas: {TextureAtlasManager.Instance.Pages}/{TextureAtlasManager.Instance.MaxPages} Pages, {(0.0D * 100D):0.00}% Filled | ");

			// // Coroutines: <number>
			sb.Append($"Routines: {CoroutineManager.Instance.Count} | ");

			// // Beacon (PubSub): <number>
			sb.Append($"Beacon: {BeaconManager.Instance.Count}");

			// // Sounds:
			sb.Append($"Sound: Playing: {_soundManager.PlayCount}, Banks: {_soundManager.Count}");

			_window.SetTitle(sb.ToString());

			_titleTimeout += 1.0f;
		}
	}



	internal SFRenderWindow ToRenderer => _window;
}
