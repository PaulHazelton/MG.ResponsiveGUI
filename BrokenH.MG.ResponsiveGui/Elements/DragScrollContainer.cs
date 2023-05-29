using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BrokenH.MG.ResponsiveGui.Styles;

namespace BrokenH.MG.ResponsiveGui.Elements;

public class DragScrollContainer : Container
{
	private Point _mouseStart;
	private bool _isDragging;
	private Point _scrollStart;
	public static int ScrollDistance = 100;


	public DragScrollContainer(Layout? layout = null, bool focusable = false)
		: base(layout)
	{
		Focusable = focusable;
		if (Focusable)
			OverrideFocusChange = true;
	}

	protected override void OnUpdate(GameTime gameTime)
	{
		if (_isDragging)
		{
			var mousePosition = Mouse.GetState().Position;
			var difference = mousePosition - _mouseStart + _scrollStart;

			ScrollX = difference.X;
			ScrollY = difference.Y;
			// UpdateChildRectangles();
		}
	}

	protected override void OnMouseEvent(byte mouseButton, ButtonState buttonState)
	{
		if (buttonState == ButtonState.Pressed && (MouseIsContained || CurrentLayout.AllowScrollingFromAnywhere))
		{
			_isDragging = true;
			_mouseStart = Mouse.GetState().Position;
			_scrollStart = new Point((int)ScrollX, (int)ScrollY);
		}

		if (buttonState == ButtonState.Released)
			_isDragging = false;
	}

	public override bool ConsumeFocus(UIDirection d, UISide s)
	{
		// Scrolling vertically
		if (CurrentLayout.OverflowY == Overflow.Scroll && d == UIDirection.Vertical)
			return DoScrolling(UIDirection.Vertical, (s == UISide.Start ? 1 : -1) * ScrollDistance);
		else if (CurrentLayout.OverflowX == Overflow.Scroll && d == UIDirection.Horizontal)
			return DoScrolling(UIDirection.Horizontal, (s == UISide.Start ? 1 : -1) * ScrollDistance);

		return false;
	}
}