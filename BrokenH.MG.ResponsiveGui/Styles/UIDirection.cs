#pragma warning disable CS8524  // Simple switch expressions

namespace BrokenH.MG.ResponsiveGui.Styles;

public enum UIDirection
{
	Vertical,
	Horizontal,
}

public enum UISide
{
	Start,
	End,
}

internal static class DirectionSideExtensions
{
	internal static UISide Invert(this UISide s) => s switch
	{
		UISide.Start => UISide.End,
		UISide.End => UISide.Start,
	};
}