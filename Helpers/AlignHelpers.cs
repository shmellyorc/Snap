namespace Snap.Helpers;

/// <summary>
/// Provides helper methods for aligning sizes and positions of UI elements or entities
/// within a parent container or viewport.
/// </summary>
public static class AlignHelpers
{
	/// <summary>
	/// Calculates the horizontal offset needed to align a child width within a parent width,
	/// using the specified alignment and no additional offset.
	/// </summary>
	/// <param name="parent">The total width of the parent container.</param>
	/// <param name="child">The width of the child element.</param>
	/// <param name="align">The horizontal alignment (<see cref="HAlign.Center"/> or <see cref="HAlign.Right"/>).</param>
	/// <returns>
	/// The X-coordinate at which the child should be placed to achieve the requested alignment.
	/// </returns>
	public static float AlignWidth(float parent, float child, HAlign align) =>
		AlignWidth(parent, child, align, 0f);

	/// <summary>
	/// Calculates the horizontal offset needed to align a child width within a parent width,
	/// using the specified alignment and an additional offset.
	/// </summary>
	/// <param name="parent">The total width of the parent container.</param>
	/// <param name="child">The width of the child element.</param>
	/// <param name="align">The horizontal alignment (<see cref="HAlign.Center"/> or <see cref="HAlign.Right"/>).</param>
	/// <param name="offset">An extra horizontal offset to apply after alignment.</param>
	/// <returns>
	/// The X-coordinate at which the child should be placed to achieve the requested alignment, plus the offset.
	/// </returns>
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

	/// <summary>
	/// Calculates the vertical offset needed to align a child height within a parent height,
	/// using the specified alignment and no additional offset.
	/// </summary>
	/// <param name="parent">The total height of the parent container.</param>
	/// <param name="child">The height of the child element.</param>
	/// <param name="align">The vertical alignment (<see cref="VAlign.Center"/> or <see cref="VAlign.Bottom"/>).</param>
	/// <returns>
	/// The Y-coordinate at which the child should be placed to achieve the requested alignment.
	/// </returns>
	public static float AlignHeight(float parent, float child, VAlign align) =>
		AlignHeight(parent, child, align, 0f);

	/// <summary>
	/// Calculates the vertical offset needed to align a child height within a parent height,
	/// using the specified alignment and an additional offset.
	/// </summary>
	/// <param name="parent">The total height of the parent container.</param>
	/// <param name="child">The height of the child element.</param>
	/// <param name="align">The vertical alignment (<see cref="VAlign.Center"/> or <see cref="VAlign.Bottom"/>).</param>
	/// <param name="offset">An extra vertical offset to apply after alignment.</param>
	/// <returns>
	/// The Y-coordinate at which the child should be placed to achieve the requested alignment, plus the offset.
	/// </returns>
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

	/// <summary>
	/// Positions an entity within the current renderer viewport according to the given horizontal
	/// and vertical alignments, with no extra offset.
	/// </summary>
	/// <param name="entity">The entity whose <c>Position</c> and <c>Size</c> will be used.</param>
	/// <param name="hAlign">Horizontal alignment within the viewport.</param>
	/// <param name="vAlign">Vertical alignment within the viewport.</param>
	public static void AlignToRenderer(in Entity entity, HAlign hAlign, VAlign vAlign) =>
		AlignToRenderer(entity, hAlign, vAlign, Vect2.Zero);

	/// <summary>
	/// Positions an entity within the current renderer viewport according to the given horizontal
	/// and vertical alignments, with an additional offset.
	/// </summary>
	/// <param name="entity">The entity whose <c>Position</c> and <c>Size</c> will be used.</param>
	/// <param name="hAlign">Horizontal alignment within the viewport.</param>
	/// <param name="vAlign">Vertical alignment within the viewport.</param>
	/// <param name="offset">An extra positional offset to apply after alignment.</param>
	public static void AlignToRenderer(in Entity entity, HAlign hAlign, VAlign vAlign, Vect2 offset)
	{
		var v = EngineSettings.Instance.Viewport;
		var x = AlignWidth(v.X, entity.Size.X, hAlign, offset.X);
		var y = AlignHeight(v.Y, entity.Size.Y, vAlign, offset.Y);

		entity.Position = new Vect2(x, y);
	}

	/// <summary>
	/// Positions a child entity inside a parent entity according to the given horizontal and vertical alignments,
	/// with no extra offset.
	/// </summary>
	/// <param name="parent">The parent entity whose <c>Size</c> defines the container.</param>
	/// <param name="child">The child entity whose <c>Size</c> and <c>Position</c> will be updated.</param>
	/// <param name="hAlign">Horizontal alignment within the parent.</param>
	/// <param name="vAlign">Vertical alignment within the parent.</param>
	public static void AlignToEntity(in Entity parent, in Entity child, HAlign hAlign, VAlign vAlign) =>
		AlignToEntity(parent, child, hAlign, vAlign, Vect2.Zero);

	/// <summary>
	/// Positions a child entity inside a parent entity according to the given horizontal and vertical alignments,
	/// with an additional offset.
	/// </summary>
	/// <param name="parent">The parent entity whose <c>Size</c> defines the container.</param>
	/// <param name="child">The child entity whose <c>Size</c> and <c>Position</c> will be updated.</param>
	/// <param name="hAlign">Horizontal alignment within the parent.</param>
	/// <param name="vAlign">Vertical alignment within the parent.</param>
	/// <param name="offset">An extra positional offset to apply after alignment.</param>
	public static void AlignToEntity(in Entity parent, in Entity child, HAlign hAlign, VAlign vAlign, Vect2 offset)
	{
		var x = AlignWidth(parent.Size.X, child.Size.X, hAlign, offset.X);
		var y = AlignHeight(parent.Size.Y, child.Size.Y, vAlign, offset.Y);

		child.Position = new Vect2(x, y);
	}

	/// <summary>
	/// Calculates the remaining space inside a container after placing an element and applying spacing.
	/// </summary>
	/// <param name="containerSize">The total size of the container (width or height).</param>
	/// <param name="elementSize">The size of the placed element (width or height).</param>
	/// <param name="spacing">Additional spacing around or between elements.</param>
	/// <returns>
	/// The non-negative space left over in the container after subtracting <paramref name="elementSize"/> and <paramref name="spacing"/>.
	/// </returns>
	public static float Remaining(float containerSize, float elementSize, float spacing) =>
		MathF.Max(0, containerSize - elementSize - spacing);
}
