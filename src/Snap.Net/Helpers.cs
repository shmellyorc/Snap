using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snap.Net;

public static class Helpers
{
	public static byte[] TakeFromStart(ref byte[] source, int count)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (count <= 0) return Array.Empty<byte>();

		if (count >= source.Length)
		{
			var all = source;
			source = Array.Empty<byte>();
			return all;
		}

		// bytes being removed
		var removed = new byte[count];
		Buffer.BlockCopy(source, 0, removed, 0, count);

		// remaining bytes become the new source
		var remaining = new byte[source.Length - count];
		Buffer.BlockCopy(source, count, remaining, 0, remaining.Length);

		source = remaining;
		return removed;
	}
}
