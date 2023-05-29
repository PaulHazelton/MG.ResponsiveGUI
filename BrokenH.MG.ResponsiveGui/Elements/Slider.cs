using System.Diagnostics;
using BrokenH.MG.ResponsiveGui.Common;
using BrokenH.MG.ResponsiveGui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BrokenH.MG.ResponsiveGui.Elements;

public class Slider : GuiElement
{
	// Range and value
	private float _min;
	private float _max;
	private float _value;

	// Dragging
	private Point _mouseStart;
	private Point _handlePosStart;
	private float _valueStart;
	private bool _isDragging;

	private Button _handle { get; set; }


	public Slider(Layout sliderLayout, Layout handleLayout, float min, float max, float value)
	: base(sliderLayout)
	{
		_min = min;
		_max = max;
		_value = value;
		_handle = new Button(handleLayout);
		AddChild(_handle);
	}

	// private void ClickyClick()
	// {
	// 	_isDragging = true;
	// 	_mouseStart = Mouse.GetState().Position;
	// 	_handlePosStart = _handle.BoundingRectangle.Location;
	// 	_valueStart = _value;
	// }

	protected override void OnMouseEvent(byte mouseButton, ButtonState buttonState)
	{
		if (buttonState == ButtonState.Pressed && (MouseIsContained || CurrentLayout.AllowScrollingFromAnywhere))
		{
			_isDragging = true;
			_mouseStart = Mouse.GetState().Position;
			_handlePosStart = _handle.BoundingRectangle.Location;
			_valueStart = _value;
		}

		if (buttonState == ButtonState.Released)
			_isDragging = false;
	}

	protected override void OnUpdate(GameTime gameTime)
	{
		if (_isDragging)
		{
			var mousePosition = Mouse.GetState().Position;
			var difference = mousePosition - _mouseStart;

			float normalDiff = ((float)difference.X) / ((float)BoundingRectangle.Width);
			_value = _valueStart + normalDiff;

			Debug.WriteLine(normalDiff);
		}

		// TODO find a better way to do this
		var handleLayout = _handle.Layout;

		handleLayout.PositionMode = PositionMode.Relative;
		handleLayout.LeftUnit = LayoutUnit.Pixels;
		handleLayout.Left = (GetPercentage() * BoundingRectangle.Width) - (_handle.BoundingRectangle.Width / 2.0f);

		foreach (var layout in handleLayout.SubLayouts)
		{
			layout.Value.PositionMode = PositionMode.Relative;
			layout.Value.LeftUnit = LayoutUnit.Pixels;
			layout.Value.Left = (GetPercentage() * BoundingRectangle.Width) - (_handle.BoundingRectangle.Width / 2.0f);
		}
	}

	protected override void OnDraw(SpriteBatch spriteBatch)
	{
		// Draw Foreground as a percentage of width
		var foregroundRec = BoundingRectangle;
		foregroundRec.Width = (int)(BoundingRectangle.Width * GetPercentage());

		if (CurrentLayout.ForegroundColor != Color.Transparent)
			UiPrimitiveDrawer.DrawRectangle(spriteBatch, foregroundRec, CurrentLayout.ForegroundColor);
	}

	private float GetPercentage()
	{
		return (_value - _min) / (_max - _min);
	}

	public class SliderHandle : Button
	{
		public SliderHandle(Layout? layout) : base(layout) { }

		// TODO Don't un-activate on drag
	}
}