using Snap.Assets.Loaders;
using Snap.Systems;

namespace Snap.Entities.Graphics;

public sealed class ColorRect : Entity
{

	private Texture _texture;

	public Color Color { get; set; } = Color.White;

	public ColorRect()
	{
		// Always default to engine clear color
		Color = EngineSettings.Instance.ClearColor;
		Size = EngineSettings.Instance.Viewport;
	}

	protected override void OnEnter()
	{
		_texture = new Texture(Vect2.One); // Tint of White/Nothing

		base.OnEnter();
	}

	private RenderTarget? _rt;
	private bool _rtChecked;

	protected override void OnUpdate()
	{
		if (!_rtChecked)
		{
			this.TryGetAncestorOfType(out _rt);
			_rtChecked = true;
		}

		if (Color.A <= 0 || !IsVisible)
			return;

		if (_rt != null)
		{
			// world-space origin of the RT and into RT-local coords
			var world = this.GetGlobalPosition();
			var rtWorld = _rt.GetGlobalPosition();
			var local = world - rtWorld;

			_rt.DrawBypassAtlas(_texture, new Rect2(local, Size), Color, depth: Layer);
		}
		else
			Renderer.DrawBypassAtlas(_texture, Bounds, Color, depth: Layer);

		base.OnUpdate();
	}
}
