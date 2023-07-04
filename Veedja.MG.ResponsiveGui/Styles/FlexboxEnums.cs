using Microsoft.Xna.Framework;

namespace Veedja.MG.ResponsiveGui.Styles;

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

internal static class FlexboxExtensions
{
	internal static bool IsParallelWith(this FlexDirection flexDirection, UIDirection d) => d switch
	{
		UIDirection.Horizontal => flexDirection == FlexDirection.Row,
		UIDirection.Vertical => flexDirection == FlexDirection.Column,
		_ => false,
	};

	internal static float RelaventCoordinate(this Vector2 v, FlexDirection d) => d switch
	{
		FlexDirection.Row => v.X,
		FlexDirection.Column => v.Y,
		_ => 0,
	};
}