namespace BrokenH.MG.ResponsiveGui.Styles
{
	public enum FlexDirection
	{
		Column,
		Row,
	}

	public enum JustifyContent
	{
		FlexStart,
		FlexEnd,
		Center,
		SpaceBetween
	}

	public enum AlignItems
	{
		FlexStart,
		Center,
		FlexEnd,
	}

	// TODO: change back to internal
	public static class FlexboxExtensions
	{
		public static bool IsParallelWith(this FlexDirection flexDirection, UIDirection d) => d switch
		{
			UIDirection.Horizontal => flexDirection == FlexDirection.Row,
			UIDirection.Vertical => flexDirection == FlexDirection.Column,
			_ => false,
		};
	}
}