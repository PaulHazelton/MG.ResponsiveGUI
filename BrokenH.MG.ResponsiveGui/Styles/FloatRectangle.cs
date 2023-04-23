#pragma warning disable CS8524  // Simple switch expressions

using Microsoft.Xna.Framework;

namespace BrokenH.MG.ResponsiveGui.Styles;

public class FloatRectangle
{
	public float X { get; set; }
	public float Y { get; set; }
	public float W { get; set; }
	public float H { get; set; }

	public FloatRectangle(float x, float y, float w, float h)
	{
		X = x;
		Y = y;
		W = w;
		H = h;
	}

	public Rectangle ToRectangle() => new Rectangle(
		(int)(X + 0.5F),
		(int)(Y + 0.5F),
		(int)(W + 0.5F),
		(int)(H + 0.5F)
	);

	public float MainStart(FlexDirection flexDirection) => flexDirection switch
	{
		FlexDirection.Column => Y,
		FlexDirection.Row => X,
	};
	public float CrossStart(FlexDirection flexDirection) => flexDirection switch
	{
		FlexDirection.Column => X,
		FlexDirection.Row => Y,
	};

	public float MainSize(FlexDirection flexDirection) => flexDirection switch
	{
		FlexDirection.Column => H,
		FlexDirection.Row => W,
	};
	public float CrossSize(FlexDirection flexDirection) => flexDirection switch
	{
		FlexDirection.Column => W,
		FlexDirection.Row => H,
	};
}