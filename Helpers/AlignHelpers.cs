namespace Snap.Helpers;

public static class AlignHelpers
{
	public static float AlignWidth(float parent, float child, HAlign align) =>
		AlignWidth(parent, child, align, 0f);
	public static float AlignWidth(float parent, float child, HAlign align, float offset)
	{
		var result = align switch
		{
			HAlign.Center => MathHelpers.Center(parent, child, true),
			HAlign.Right => parent - child,
			_ => 0f
		};

		return result + offset;
	}

	public static float AlignHeight(float parent, float child, VAlign align) =>
		AlignHeight(parent, child, align, 0f);
	public static float AlignHeight(float parent, float child, VAlign align, float offset)
	{
		var result = align switch
		{
			VAlign.Center => MathHelpers.Center(parent, child, true),
			VAlign.Bottom => parent - child,
			_ => 0f
		};

		return result + offset;
	}

	public static void AlignToRenderer(in Entity entity, HAlign hAlign, VAlign vAlign) =>
		AlignToRenderer(entity, hAlign, vAlign, Vect2.Zero);
	public static void AlignToRenderer(in Entity entity, HAlign hAlign, VAlign vAlign, Vect2 offset)
	{
		var v = EngineSettings.Instance.Viewport;
		var x = AlignWidth(v.X, entity.Size.X, hAlign, offset.X);
		var y = AlignHeight(v.Y, entity.Size.Y, vAlign, offset.Y);

		entity.Position = new Vect2(x, y);
	}

	public static void AlignToEntity(in Entity parent, in Entity child, HAlign hAlign, VAlign vAlign) =>
		AlignToEntity(parent, child, hAlign, vAlign, Vect2.Zero);
	public static void AlignToEntity(in Entity parent, in Entity child, HAlign hAlign, VAlign vAlign, Vect2 offset)
	{
		var x = AlignWidth(parent.Size.X, child.Size.X, hAlign, offset.X);
		var y = AlignHeight(parent.Size.Y, child.Size.Y, vAlign, offset.Y);

		child.Position = new Vect2(x, y);
	}

	public static float Remaining(float containerSize, float elementSize, float spacing) =>
		MathF.Max(0, containerSize - elementSize - spacing);
}
