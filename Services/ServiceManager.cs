namespace Snap.Services;

public class GameService
{
	public virtual void Iniitialize() { }
}

public sealed class ServiceManager
{
	private readonly Dictionary<Type, GameService> _services = new(10);

	public static ServiceManager Instance { get; private set; }
	public int Count => _services.Count;

	public ServiceManager() => Instance ??= this;

	public void RegisterService(GameService service)
	{
		if (service == null)
			throw new ArgumentNullException(nameof(service));
		if (_services.ContainsKey(service.GetType()))
			throw new ArgumentException(nameof(service));

		_services[service.GetType()] = service;

		BeaconManager.Initialize(service);
		service?.Iniitialize();
	}

	public T GetService<T>() where T : GameService
	{
		if (!_services.TryGetValue(typeof(T), out var service))
			throw new Exception();

		return (T)service;
	}

	public bool TryGetService<T>(out T service) where T : GameService
	{
		service = GetService<T>();

		return service != null;
	}
}
