using System.Text;

namespace Snap.Helpers;

public static class HashHelpers
{
	public static uint Hash32(byte[] data)
	{
		const uint OffsetBasis = 2166136261u;
		const uint Prime = 16777619u;

		uint hash = OffsetBasis;
		for (int i = 0; i < data.Length; i++)
		{
			hash ^= data[i];
			hash *= Prime;
		}
		return hash;
	}

	public static ulong Hash64(byte[] data)
	{
		const ulong OffsetBasis = 1469598103934665603ul;
		const ulong Prime = 1099511628211ul;

		ulong hash = OffsetBasis;
		for (int i = 0; i < data.Length; i++)
		{
			hash ^= data[i];
			hash *= Prime;
		}
		return hash;
	}

	public static uint Hash32(string text) =>
		Hash32(Encoding.UTF8.GetBytes(text));

	public static ulong Hash64(string text) =>
		Hash64(Encoding.UTF8.GetBytes(text));
}
