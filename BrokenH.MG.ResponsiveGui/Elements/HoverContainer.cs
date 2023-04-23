using Microsoft.Xna.Framework;
using BrokenH.MG.ResponsiveGui.Styles;

namespace BrokenH.MG.ResponsiveGui.Elements;

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