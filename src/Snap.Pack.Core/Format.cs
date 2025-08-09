using System;

namespace Snap.Pack.Core;

public enum CompType : byte
{
	None = 0,
	Brotli = 1,
	Deflate = 2
}

internal static class Format
{
	public const string Magic = "SNAPPAK"; // 8 bytes incl trailing \0 if any
}

public sealed record TocEntry(
	string Path, long Offset, long Stored, long Orginal, long MTime,
	CompType Comp, bool Encrypted, uint Crc32, int ChunkSize);

public sealed record PakHeader(ushort Version, ushort Flags, long TocOffset,
	long TocLength, Guid ArchiveId, byte[] TocSha256);
