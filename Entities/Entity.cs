namespace Snap.Entities;

public class Entity
{
	internal Screen _screen;
	internal Entity _parent;
	internal Vect2 _position;

	private bool _keepAlive, _visible = true;
	private int _layer;
	private Color _color = Color.White;
	private readonly List<Entity> _children = new(64); // Used for referencing.

	public Screen Screen => _screen;
	public Entity Parent => _parent;

	public IReadOnlyList<Entity> Children => _children;
	public int ChildCount => Children.Count;
	public bool IsParent => _parent == null;
	public bool IsReady { get; private set; }
	public bool IsChild => _parent != null;
	public bool IsExiting { get; private set; }
	public Camera Camera => _screen.Camera;
	public bool IsActive => _screen?.IsActive ?? false;
	public bool IsTopmostScreen => _screen?.IsTopmostScreen ?? false;
	public bool IsActiveScreen => _screen?.IsActivScreen ?? false;
	public int ChildIndex => Children.IndexOf(this);

	public bool KeepAlive
	{
		get
		{
			if (IsChild)
				return _parent.KeepAlive || _keepAlive;

			return _keepAlive;
		}
		set
		{
			if (_keepAlive == value)
				return;

			_keepAlive = value;

			if (_screen == null)
			{
				CoroutineManager.Start(WaitForNullScreen(() => _screen.UpdateDirtyState(DirtyState.Sort)));
				// CoroutineManager.Start(CoroutineHelpers.WaitWhileThan(() => _screen == null,
				// 	() => _screen.UpdateDirtyState(DirtyState.Update)));
			}
			else
				_screen.UpdateDirtyState(DirtyState.Update);
		}
	}

	public bool IsVisible
	{
		get
		{
			if (IsChild)
				return _parent.IsVisible && _visible;

			return _visible;
		}
		set => _visible = value;
	}


	public Color Color
	{
		get
		{
			if (IsChild)
				// TODO: Fix, it must blend colors not multi
				return _color;

			return _color;
		}
		set => _color = value;
	}


	public int Layer
	{
		get
		{
			if (IsChild)
				return _parent.Layer + _layer;

			return _layer;
		}
		set
		{
			if (_layer == value)
				return;
			_layer = value;

			if (_screen == null)
			{
				// CoroutineManager.Start(CoroutineHelpers.WaitWhileThan(() => _screen == null,
				CoroutineManager.Start(WaitForNullScreen(() => _screen.UpdateDirtyState(DirtyState.Sort)));
			}
			else
				_screen.UpdateDirtyState(DirtyState.Sort);
		}
	}



	public Vect2 Position
	{
		get
		{
			if (IsChild)
				return _parent.Position + _position;

			return _position;
		}
		set
		{
			if (_position == value)
				return;

			_position = value;
		}
	}

	public Vect2 LocalPosition
	{
		get => _position;
		set => _position = value;
	}

	private Vect2 _size = Vect2.One; // <-- Leave as one not zero, will cause the engine to skip these entities

	public Vect2 Size
	{
		get => _size;
		set
		{
			if (_size == value)
				return;
			_size = value;

			foreach (var e in this.GetAncestorsOfType<Panel>())
				e.SetDirtyState(DirtyState.Update | DirtyState.Sort);
		}
	}

	public Rect2 Bounds
	{
		get
		{
			// Fetch hte entity's gobal position regardless.
			var worldPos = this.GetGlobalPosition();

			// Build the rectangle at world position
			return new(worldPos, Size);
		}
	}


	public Entity() { }


	#region Helpers
	public float SafeRegion => EngineSettings.Instance.SafeRegion;

	public TParent EntityAs<TParent>(Entity entity) where TParent : Entity
	{
		if (entity == null)
			throw new InvalidOperationException("Cannot get parent: no parent is set.");

		if (entity is TParent parent)
			return parent;

		throw new InvalidCastException(
			$"Parent is of type {entity.GetType().Name}, not {typeof(TParent).Name}");
	}
	public TEntity ParentAs<TEntity>() where TEntity : Entity =>
		EntityAs<TEntity>(_parent);

	public Logger Logger => Logger.Instance;
	public Clock Clock => Clock.Instance;
	public Engine Engine => Engine.Instance;
	public FastRandom Rand => FastRandom.Instance;
	public Renderer Renderer => Renderer.Instance;
	public InputMap Input => Engine.Instance.Input;
	public AssetManager Assets => AssetManager.Instance;
	public BeaconManager Beacon => BeaconManager.Instance;
	public SoundManager SoundManager => SoundManager.Instance;
	public ScreenManager ScreenManager => ScreenManager.Instance;
	public CoroutineManager CoroutineManager => CoroutineManager.Instance;

	public Texture GetTexture(string name) => AssetManager.GetTexture(name);
	public Texture GetTexture(Enum name) => AssetManager.GetTexture(name);
	public bool TryGetTexture(string name, out Texture texture) => AssetManager.TryGetTexture(name, out texture);
	public bool TryGetTexture(Enum name, out Texture texture) => AssetManager.TryGetTexture(name, out texture);

	public LDTKProject GetMap(string name) => AssetManager.GetMap(name);
	public LDTKProject GetMap(Enum name) => AssetManager.GetMap(name);
	public bool TryGetMap(string name, out LDTKProject texture) => AssetManager.TryGetMap(name, out texture);
	public bool TryGetMap(Enum name, out LDTKProject texture) => AssetManager.TryGetMap(name, out texture);

	public Spritesheet GetSheet(string name) => AssetManager.GetSheet(name);
	public Spritesheet GetSheet(Enum name) => AssetManager.GetSheet(name);
	public bool TryGetSheet(string name, out Spritesheet texture) => AssetManager.TryGetSheet(name, out texture);
	public bool TryGetSheet(Enum name, out Spritesheet texture) => AssetManager.TryGetSheet(name, out texture);

	public Font GetFont(string name) => AssetManager.GetFont(name);
	public Font GetFont(Enum name) => AssetManager.GetFont(name);
	public bool TryGetFont(string name, out Font texture) => AssetManager.TryGetFont(name, out texture);
	public bool TryGetFont(Enum name, out Font texture) => AssetManager.TryGetFont(name, out texture);

	public BitmapFont GetBitmapFont(string name) => AssetManager.GetBitmapFont(name);
	public BitmapFont GetBitmapFont(Enum name) => AssetManager.GetBitmapFont(name);
	public bool TryGetBitmapFont(string name, out BitmapFont texture) => AssetManager.TryGetBitmapFont(name, out texture);
	public bool TryGetBitmapFont(Enum name, out BitmapFont texture) => AssetManager.TryGetBitmapFont(name, out texture);

	public SpriteFont GetSpriteFont(string name) => AssetManager.GetSpriteFont(name);
	public SpriteFont GetSpriteFont(Enum name) => AssetManager.GetSpriteFont(name);
	public bool TryGetSpriteFont(string name, out SpriteFont texture) => AssetManager.TryGetSpriteFont(name, out texture);
	public bool TryGetSpriteFont(Enum name, out SpriteFont texture) => AssetManager.TryGetSpriteFont(name, out texture);

	public Sound GetSound(string name) => AssetManager.GetSound(name);
	public Sound GetSound(Enum name) => AssetManager.GetSound(name);
	public bool TryGetSound(string name, out Sound sound) => AssetManager.TryGetSound(name, out sound);
	public bool TryGetSound(Enum name, out Sound sound) => AssetManager.TryGetSound(name, out sound);

	public void Destroy()
	{
		if (IsExiting)
			return;

		Screen.RemoveEntity(this);
	}

	#endregion



	internal void EngineOnEnter()
	{
		OnEnter();
	}
	protected virtual void OnEnter() { }

	internal void EngineOnExit()
	{
		if (IsExiting)
			return;

		ClearBeacons();
		ClearRoutines();
		ClearChildren();

		IsExiting = true;

		foreach (var p in this.GetAncestorsOfType<Panel>())
		{
			p._children.Remove(this);
			p.SetDirtyState(DirtyState.Sort | DirtyState.Update);
		}

		OnExit();
	}
	protected virtual void OnExit() { }

	internal void EngineOnUpdate()
	{
		if (EngineSettings.Instance.DebugDraw)
		{
			if (this.TryGetAncestorOfType<RenderTarget>(out var rt))
			{
				var world = this.GetGlobalPosition();
				var box = new Rect2(world, Size);

				DebugRenderer.Instance.DrawRect(box - rt.Offset, Color.Red);
			}
			else
				DebugRenderer.Instance.DrawRect(Bounds, Color.Red);
		}

		OnUpdate();
	}

	protected virtual void OnUpdate() { }












	#region Children
	public void AddChild(params Entity[] children)
	{
		if (children == null || children.Length == 0)
			return;

		IEnumerator WaitRoutine()
		{
			if (_screen == null)
				yield return new WaitWhile(() => _screen == null);

			var list = new List<Entity>(children.Length);
			for (int i = 0; i < children.Length; i++)
			{
				var c = children[i];
				if (c == null || c.IsExiting)
					continue;

				c._parent = this;
				_children.Add(c);

				list.Add(c);
			}

			if (list.Count > 0)
				_screen.AddEntity([.. list]);
		}

		CoroutineManager.Start(WaitRoutine());
	}

	public TParent AddToParent<TParent>(params Entity[] children) where TParent : Entity
	{
		if (children == null || children.Length == 0)
			return (TParent)this;

		for (int i = children.Length - 1; i >= 0; i--)
		{
			var c = children[i];
			if (c == null || c.IsExiting)
				continue;

			if (c._layer >= 0)
				c._layer++;
		}

		AddChild(children);

		return (TParent)this;
	}

	public bool RemoveChild(params Entity[] children)
	{
		if (children == null || children.Length == 0)
			return false;

		var list = new List<Entity>(children.Length);
		for (int i = 0; i < children.Length; i++)
		{
			var c = children[i];

			if (c == null || c.IsExiting)
				continue;

			if (_children.Remove(c))
			{
				c._parent = null;
				list.Add(c);
			}

			list.Add(c);
		}

		if (list.Count > 0)
		{
			_screen.RemoveEntity([.. list]);
			return true;
		}

		return false;
	}

	public TEntity GetChild<TEntity>(int index) where TEntity : Entity
	{
		if (index < 0)
			return null;

		int count = 0;
		for (int i = 0; i < _children.Count; i++)
		{
			var c = _children[i];
			if (c is TEntity match)
			{
				if (count == index)
					return match;
				count++;
			}
		}

		return null!;
	}

	public bool TryGetChild<TEntity>(int index, out TEntity child) where TEntity : Entity
	{
		child = GetChild<TEntity>(index);

		return child != null;
	}

	public void ClearChildren()
	{
		if (_children.Count == 0)
			return;

		var toRemove = _children.ToArray();
		_children.Clear();

		for (int i = toRemove.Length - 1; i >= 0; i--)
			toRemove[i]._parent = null;

		_screen.RemoveEntity(toRemove);
	}
	#endregion



	private IEnumerator WaitForNullScreen(Action onReady)
	{
		yield return new WaitWhile(() => Screen == null);

		onReady?.Invoke();
	}




	#region Beacons
	public void ConnectBeacon(string topic, Action<BeaconHandle> handler) => Beacon.Connect(topic, this, handler);
	public void ConnectBeacon(Enum topic, Action<BeaconHandle> handler) => Beacon.Connect(topic.ToEnumString(), this, handler);
	public void DisconnectBeacon(string topic, Action<BeaconHandle> handler) => Beacon.Disconnect(topic, this, handler);
	public void DisconnectBeacon(Enum topic, Action<BeaconHandle> handler) => Beacon.Disconnect(topic.ToEnumString(), this, handler);
	public void EmitBeacon(string topic, params object[] args) => Beacon.Emit(topic, args);
	public void EmitBeacon(Enum topic, params object[] args) => Beacon.Emit(topic, args);
	public void EmitBeaconDelayed(string topic, float seconds, params object[] args) => Beacon.EmitDelayed(this, topic, seconds, args);
	public void EmitBeaconDelayed(Enum topic, float seconds, params object[] args) => Beacon.EmitDelayed(this, topic.ToEnumString(), seconds, args);
	public void ClearBeacons() => Beacon.ClearOwner(this);
	#endregion


	#region Coroutines
	public CoroutineHandle StartRoutine(IEnumerator routine) => CoroutineManager.Start(routine, this);
	public CoroutineHandle StartRoutineDelayed(IEnumerator routine, float delay) => CoroutineManager.StartDelayed(delay, this, routine);
	public bool StopRoutine(CoroutineHandle handle) => CoroutineManager.Stop(handle);
	public bool HasRoutine(CoroutineHandle handle) => CoroutineManager.IsRunning(handle);
	public void ClearRoutines() => CoroutineManager.StopAll(this);
	#endregion


	#region Screen
	public void AddScreen(params Screen[] screens) => ScreenManager.Add(screens);
	public void RemoveScreen(params Screen[] screens) => ScreenManager.Remove(screens);
	public T GetScreen<T>() where T : Screen => ScreenManager.Get<T>();
	public Screen GetScreen(Screen screen) => ScreenManager.Get(screen);
	public Screen GetScreenById(uint id) => ScreenManager.GetById(id);
	#endregion
}