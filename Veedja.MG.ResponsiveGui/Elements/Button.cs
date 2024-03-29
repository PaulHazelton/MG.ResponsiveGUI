using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Veedja.MG.ResponsiveGui.Styles;
using Veedja.MG.ResponsiveGui.Common;

namespace Veedja.MG.ResponsiveGui.Elements;

public class Button : Label
{
	public static int DragForgiveness = 8;
	public static byte PrimaryClick = 1;

	protected Action? ClickAction;
	private Point _mouseDragStart;


	public Button(Layout? layout, Action? clickAction = null, string text = "")
	: base(layout, text)
	{
		ClickAction = clickAction;
		Focusable = true;
	}

	protected override void OnMouseEvent(byte button, ButtonState state)
	{
		if (button == PrimaryClick && state == ButtonState.Released && State == ElementStates.Activated)
			ActivateRelease();
		if (button == PrimaryClick && state == ButtonState.Pressed && State == ElementStates.Hovered)
			ActivatePress();
	}

	protected override void OnActivatePress()
	{
		State = ElementStates.Activated;
		_mouseDragStart = TransformedMouse;
	}
	protected override void OnActivateRelease()
	{
		State = ElementStates.Hovered;
		ClickAction?.Invoke();
	}

	protected override void OnUpdate(GameTime deltaTime)
	{
		// If the user dragged, un-activate
		// TODO: Don't use DidMouseMove, try to remove this weird variable
		if (DidMouseMove && State == ElementStates.Activated)
		{
			var dragDifference = TransformedMouse - _mouseDragStart;

			if (Math.Abs(dragDifference.X) > DragForgiveness || Math.Abs(dragDifference.Y) > DragForgiveness)
				State = ElementStates.Hovered;
		}
	}

	protected override void OnStateChange()
	{
		foreach (var child in Children)
			child.State = State;
	}
}