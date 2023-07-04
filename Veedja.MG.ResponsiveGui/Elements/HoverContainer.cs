using Microsoft.Xna.Framework;
using Veedja.MG.ResponsiveGui.Styles;
using Veedja.MG.ResponsiveGui.Common;

namespace Veedja.MG.ResponsiveGui.Elements;

public class HoverContainer : Container
{
	public HoverContainer(Layout? layout = null) : base(layout) { }

	protected override void OnUpdate(GameTime deltaTime)
	{
		if (MouseIsContained)
			State = ElementStates.Hovered;

		if (!MouseIsContained)
			State = ElementStates.Neutral;
	}
}