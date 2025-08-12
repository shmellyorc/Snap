namespace Snap.Engine.Entities.Panels;

/// <summary>
/// A panel that automatically centers all of its child entities within its own bounds.
/// </summary>
/// <remarks>
/// <para>
/// The centering is recalculated whenever the panel's layout becomes dirty 
/// (e.g., size changes or child modifications).
/// </para>
/// <para>
/// Child positions are set so that each child's center aligns with the center of the panel.
/// </para>
/// </remarks>
public sealed class CenterPanel : Panel
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CenterPanel"/> class
	/// with the specified child entities.
	/// </summary>
	/// <param name="entities">The child entities to add to this panel.</param>
	public CenterPanel(params Entity[] entities) : base(entities) { }

	/// <summary>
	/// Called when the panel's layout becomes dirty, recalculating child positions.
	/// </summary>
	/// <param name="state">The dirty state flags describing what changed.</param>
	/// <remarks>
	/// Iterates through all child entities and repositions them so that their 
	/// center aligns with the center of the panel.
	/// </remarks>
	protected override void OnDirty(DirtyState state)
	{
		for (int i = ChildCount - 1; i >= 0; i--)
		{
			var c = GetChild<Entity>(i);

			c.Position = Size.Center(c.Size, true);
		}

		base.OnDirty(state);
	}
}
