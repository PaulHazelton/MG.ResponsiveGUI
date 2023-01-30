namespace BrokenH.MG.ResponsiveGui.Styles
{
	public enum PositionMode
	{
		/// <summary>
		/// An element with Position.Static is not positioned in any special way;
		/// it is always positioned according to the normal flow of the parent element.
		/// Left, Right, Top, and Bottom do not have an affect with Position.Static.
		/// </summary>
		Static,
		/// <summary>
		/// An element with Position.Relative; is positioned relative to its normal position.
		/// Setting the top, right, bottom, and left properties of a relatively-positioned element will cause it to be adjusted away from its normal position.
		/// Other content will not be adjusted to fit into any gap left by the element.
		/// </summary>
		Relative,
		/// <summary>
		/// An element with Position.RelativeToParent is positioned relative to the parent element.
		/// Note: Elements with this position mode are removed from the normal flow, and can overlap elements.
		/// </summary>
		RelativeToParent,
		/// <summary>
		/// An element with Position.Absolute is positioned relative to the screen
		/// Note: Elements with this position mode are removed from the normal flow, and can overlap elements.
		/// </summary>
		Fixed,

		/*
		/// <summary>
		/// An element with Position.Absolute is positioned relative to the nearest positioned ancestor (instead of positioned relative to the screen, like Fixed).
		/// However; if an absolute positioned element has no ancestors with Position.Relative, it is relative to the screen, and moves along with scrolling.
		/// Note: Absolute positioned elements are removed from the normal flow, and can overlap elements.
		/// </summary>
		Absolute,
		*/
	}

	public static class PositionModeExtensions
	{
		public static bool NormalFlow(this PositionMode pm) => pm switch
		{
			PositionMode.Static => true,
			PositionMode.Relative => true,
			_ => false
		};
	}
}