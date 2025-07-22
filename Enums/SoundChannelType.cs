namespace Snap.Enums;

/// <summary>
/// Defines the channel configuration for audio playback.
/// </summary>
public enum SoundChannelType
{
	/// <summary>
	/// The channel type is not specified or unrecognized.
	/// </summary>
	Unknown,

	/// <summary>
	/// Single-channel (mono) audio.
	/// </summary>
	Mono,

	/// <summary>
	/// Two-channel (stereo) audio.
	/// </summary>
	Stereo,
}
