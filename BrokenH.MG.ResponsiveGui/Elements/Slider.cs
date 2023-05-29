using BrokenH.MG.ResponsiveGui.Rendering;
using BrokenH.MG.ResponsiveGui.Styles;
using Microsoft.Xna.Framework.Graphics;

namespace BrokenH.MG.ResponsiveGui.Elements;

public class Slider : GuiElement
{
	private float _min;
	private float _max;
	private float _value;

	// private Button _handle { get; set; }


	public Slider(Layout sliderLayout, Layout handleLayout, float min, float max, ref float value)
	: base(sliderLayout)
	{
		_min = min;
		_max = max;
		_value = value;

		// handleLayout.PositionMode = PositionMode.Relative;
		// handleLayout.LeftUnit = LayoutUnit.Percent;
		// handleLayout.Left = _value * 100;	// TODO

		// _handle = new Button(handleLayout);
		// AddChild(_handle);

		Focusable = true;
	}

	protected override void OnDraw(SpriteBatch spriteBatch)
	{
		// IUiPrimitiveDrawer uiPrimitiveDrawer = UiPrimitiveDrawer;

	}
}