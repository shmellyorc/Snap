using System.Runtime.CompilerServices;

using Microsoft.VisualBasic;

using Snap.Logs;
using Snap.Paths;
using Snap.Systems;

namespace Snap.Graphics;

public sealed class DebugRenderer
{
	private SFPrimitiveType _primitive = SFPrimitiveType.Lines;
	private SFVertexBuffer _buffer;
	private int _capcity, _vertexCount;
	private SFVertex[] _vertexes;

	public static DebugRenderer Instance { get; private set; }

	internal DebugRenderer(int initialCapacity = 256)
	{
		Instance ??= this;

		_capcity = initialCapacity;
		_buffer = new SFVertexBuffer((uint)_capcity, SFPrimitiveType.Lines, SFVertexBuffer.UsageSpecifier.Stream);
		_vertexes = new SFVertex[_capcity];
		_vertexCount = 0;
	}

	public void DrawLine(Vect2 a, Vect2 b, Color color)
	{
		if (_vertexCount + 2 > _vertexes.Length)
			GrowVertexArray(_vertexCount + 2);

		_vertexes[_vertexCount++] = new SFVertex(a, color, new(0, 0));
		_vertexes[_vertexCount++] = new SFVertex(b, color, new(0, 0));
	}

	public void DrawRect(Rect2 rect, Color color)
	{
		var topLeft = rect.TopLeft;
		var topRight = rect.TopRight;
		var bottomRight = rect.BottomRight;
		var bottomLeft = rect.BottomLeft;

		DrawLine(topLeft, topRight, color);
		DrawLine(topRight, bottomRight, color);
		DrawLine(bottomRight, bottomLeft, color);
		DrawLine(bottomLeft, topLeft, color);
	}

	public void DrawCircle(Vect2 center, float radius, Color color, int segmentCount = 16)
	{
		if (segmentCount < 3)
			segmentCount = 3;

		float angleStep = (MathF.PI * 2f) / segmentCount;
		Vect2 prev = new(
			center.X + MathF.Cos(0f) * radius,
			center.Y + MathF.Sin(0f) * radius
		);
		Vect2 start = prev;

		for (int i = 0; i < segmentCount; i++)
		{
			float angle = angleStep * i;
			Vect2 next = new Vect2(
				center.X + MathF.Cos(angle) * radius,
				center.Y + MathF.Sin(angle) * radius
			);

			DrawLine(prev, next, color);
			prev = next;
		}

		DrawLine(prev, start, color);
	}

	internal void Begin() => _vertexCount = 0;

	internal void End()
	{
		if (_vertexCount == 0)
			return;

		int needed = _vertexCount;

		if (needed > _capcity)
		{
			var newCap = _capcity;
			while (newCap < needed)
				newCap *= 2;

			_buffer.Dispose();
			_buffer = new SFVertexBuffer((uint)newCap, _primitive, SFVertexBuffer.UsageSpecifier.Stream);
			_capcity = newCap;

			Logger.Instance.Log(LogLevel.Info, $"[Debug Renderer]: Vertex buffer increased to '{newCap}'.");
		}

		_buffer.Update(_vertexes, (uint)needed, 0);

		int tailCount = _capcity - needed;
		if (tailCount > 0)
		{
			var blanks = new SFVertex[tailCount];

			FillBlanks(blanks, tailCount);

			_buffer.Update(blanks, (uint)tailCount, (uint)needed);
		}

		var states = new SFRenderStates
		{
			Texture = null,
			Transform = SFTransform.Identity,
			BlendMode = SFBlendMode.Alpha
		};

		Engine.Instance.ToRenderer.Draw(_buffer, states);

		_vertexCount = 0;
	}

	private void FillBlanks(SFVertex[] blanks, int tailCount)
	{
		unsafe
		{
			fixed (SFVertex* p = blanks)
			{
				uint byteCount = (uint)(tailCount * sizeof(SFVertex));

				Unsafe.InitBlock(p, 0, byteCount);
			}
		}
	}

	private void GrowVertexArray(int newSize)
	{
		int cap = _vertexes.Length;
		while (cap < newSize)
			cap *= 2;

		Array.Resize(ref _vertexes, cap);

		Logger.Instance.Log(LogLevel.Info, $"[Debug Renderer]: Vertex array increased to '{cap}'.");
	}
}
