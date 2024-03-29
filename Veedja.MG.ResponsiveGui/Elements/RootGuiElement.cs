using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Veedja.MG.ResponsiveGui.Styles;
using Veedja.MG.ResponsiveGui.Common;

namespace Veedja.MG.ResponsiveGui.Elements;

public class RootGuiElement : GuiElement
{
	private GuiElement? _focusedElement;
	public GuiElement? FocusedElement
	{
		get => _focusedElement;
		internal set
		{
			if (_focusedElement == value)
				return;

			_focusedElement = value;
		}
	}

	private Point _previousAbsoluteMouse;


	// Constructor
	public RootGuiElement(Layout? layout = null) : base(layout) { }

	// Game Loop
	public void Draw(SpriteBatch spriteBatch, SamplerState? samplerState = null) => GuiElement.ElementRenderer.Draw(spriteBatch, samplerState, this);
	public void Update(GameTime gameTime)
	{
		var mouse = Mouse.GetState().Position;

		// Did mouse move
		bool didMouseMove = true;
		var absoluteMouse = Mouse.GetState().Position;
		var moveDifference = absoluteMouse - _previousAbsoluteMouse;
		_previousAbsoluteMouse = absoluteMouse;
		if (Math.Abs(moveDifference.X) == 0 && Math.Abs(moveDifference.Y) == 0)
			didMouseMove = false;

		PropogateUpdate(gameTime, mouse, true, didMouseMove);
		Refresh();
	}
	public void Refresh()
	{
		ComputeDimensions();
		UpdateRectangles();
	}

	// Input
	public void ChangeFocusLeft() => ChangeFocus(UIDirection.Horizontal, UISide.Start);
	public void ChangeFocusRight() => ChangeFocus(UIDirection.Horizontal, UISide.End);
	public void ChangeFocusUp() => ChangeFocus(UIDirection.Vertical, UISide.Start);
	public void ChangeFocusDown() => ChangeFocus(UIDirection.Vertical, UISide.End);

	public void ActivateFocused(ButtonState eventType)
	{
		if (eventType == ButtonState.Pressed)
			FocusedElement?.ActivatePress();
		else
			FocusedElement?.ActivateRelease();
	}

	private void ChangeFocus(UIDirection d, UISide s)
	{
		GuiElement? newFocus;
		if (FocusedElement == null)
			newFocus = GetFirstFocusableElementInTree();
		else
		{
			if (FocusedElement.OverrideFocusChange && FocusedElement.ConsumeFocus(d, s))
				return;

			newFocus = FocusedElement.GetNextFocusableElement(d, s, FocusedElement.Position);
		}

		if (newFocus == null || newFocus == FocusedElement)
			return;

		if (FocusedElement != null)
			FocusedElement.State = ElementStates.Neutral;
		FocusedElement = newFocus;
		FocusedElement.State = ElementStates.Hovered;

		FocusedElement.ScrollElementIntoView();
	}

	public void Unfocus()
	{
		if (FocusedElement == null)
			return;
		FocusedElement.State = ElementStates.Neutral;
		FocusedElement = null;
	}

	// Other
	public void ResetAllTransitions()
	{
		this.ToAll(e => e.ResetTransitions(e == FocusedElement ? ElementStates.Hovered : ElementStates.Neutral));
	}
}