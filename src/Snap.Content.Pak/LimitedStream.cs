using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snap.Content.Pak;

internal sealed class LimitedStream : Stream
{
	private readonly Stream _base;
	private readonly long _start;
	private readonly long _length;
	private long _position;

	public LimitedStream(Stream @base, long start, long length)
	{
		_base = @base; _start = start; _length = length; _position = 0;
		_base.Position = _start;
	}

	public override bool CanRead => true;
	public override bool CanSeek => true;
	public override bool CanWrite => false;
	public override long Length => _length;
	public override long Position { get => _position; set => Seek(value, SeekOrigin.Begin); }

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_position >= _length) return 0;
		var tol = (int)Math.Min(count, _length - _position);
		var read = _base.Read(buffer, offset, tol);
		_position += read;
		return read;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		long target = origin switch
		{
			SeekOrigin.Begin => offset,
			SeekOrigin.Current => _position + offset,
			SeekOrigin.End => _length + offset,
			_ => throw new ArgumentOutOfRangeException(nameof(origin))
		};
		if (target < 0 || target > _length) throw new IOException("Seek out of bounds");
		_position = target;
		_base.Position = _start + _position;
		return _position;
	}

	public override void Flush() { }
	public override void SetLength(long value) => throw new NotSupportedException();
	public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
