using SFML.Audio;

using Snap.Logs;
using Snap.Sounds;

namespace Snap.Assets.Loaders;

public enum SoundChannelType
{
	Unknown,
	Mono,
	Stereo,
}

// Supported types: wav, mp3, ogg, flac, aiff, au, raw, paf, svx, nist, voc, ircam, w64, mat4, mat5, pvf, htk, sds, avr, sd2, caf, wve, mpc2k, rf64.
public sealed class Sound : IAsset, IEquatable<Sound>
{
	internal SFSoundBuffer Buffer;

	private readonly List<SoundInstance> _instances = new(64);

	public uint Id { get; }
	public string Tag { get; }
	public bool IsValid { get; private set; }
	public uint Handle => Id; // not really used here but required for textures

	public TimeSpan Duration => IsValid ? Buffer.Duration.ToTimeSpan() : TimeSpan.Zero;
	public uint SampleRate => IsValid ? Buffer.SampleRate : 0u;
	public SoundChannelType Channel => IsValid ? (SoundChannelType)Buffer.ChannelCount : SoundChannelType.Unknown;
	public bool IsLooped { get; }

	internal Sound(uint id, string filename, bool looped)
	{
		Id = id;
		Tag = filename;
		IsLooped = looped;
	}

	~Sound() => Dispose();

	public SoundInstance CreateInstance()
	{
		if (!IsValid)
			return null;

		// remove dead instance if any...
		int count = _instances.RemoveAll(x => x.IsValid);
		if (count > 0)
			Logger.Instance.Log(LogLevel.Info, $"Unloaded asset {Id} ({_instances.Count} instances removed), type: '{GetType().Name}'.");

		var instance = new SoundInstance(AssetManager.Id++, this, Buffer);
		_instances.Add(instance);

		return instance;
	}

	public void Unload()
	{
		if (_instances.Any(x => x.IsValid))
		{
			Logger.Instance.Log(LogLevel.Warning, $"Asset eviction canceled for ID {Id}, type: '{GetType().Name}', " +
				"because it is currently in use as a sound instance.");
			return;
		}

		if (!IsValid)
			return;

		Dispose();
	}

	public ulong Load()
	{
		if (IsValid)
			return 0u;

		if (!File.Exists(Tag))
			throw new FileNotFoundException($"The asset file '{Tag}' could not be found.");

		var bytes = File.ReadAllBytes(Tag);
		Buffer = new SFSoundBuffer(bytes);
		IsValid = true;

		return (ulong)bytes.Length;
	}

	public void Dispose()
	{
		// stop and wipe all instances...
		for (int i = _instances.Count - 1; i >= 0; i--)
		{
			var inst = _instances[i];

			inst.Stop();
			inst.Dispose();
		}
		_instances.Clear();

		Buffer?.Dispose();
		IsValid = false;

		Logger.Instance.Log(LogLevel.Info, $"Unloaded asset with ID {Id}, type: '{GetType().Name}'.");
	}



	public bool Equals(Sound other) =>
		other != null && Id.Equals(other.Id) && Tag.Equals(other.Tag);

	public override bool Equals(object obj) => obj is Sound value && Equals(value);

	public override int GetHashCode() => HashCode.Combine(Id, Tag);

	public override string ToString() => $"Sound({Id}, {Path.GetFileName(Tag)}, {IsLooped})";
}
