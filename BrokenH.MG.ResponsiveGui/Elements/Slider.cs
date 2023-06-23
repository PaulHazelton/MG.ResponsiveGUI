using System;
using BrokenH.MG.ResponsiveGui.Common;
using BrokenH.MG.ResponsiveGui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BrokenH.MG.ResponsiveGui.Elements;

public class Slider : GuiElement
{
	public static float NudgeIncrement = 0.1f;

	// Range and value
	private float _min;
	private float _max;
	private float _value;
	private float Value
	{
		get => _value;
		set
		{
			if (_value != value)
			{
				_value = value;
				_setValue?.Invoke(value);
			}
		}
	}
	private Action<float> _setValue;

	// Dragging
	private bool _isDragging;

	// Sub elements
	private Layout _handleContainerLayout;
	private Container _handleContainer;
	private Button _handle;


	public Slider(Layout sliderLayout, Layout handleLayout, float min, float max, float initialValue, Action<float> valueSetter)
	: base(sliderLayout)
	{
		_handleContainerLayout = new Layout()
		{
			PositionMode = PositionMode.Relative,
			LeftUnit = LayoutUnit.Pixels,
			Width = 0,
			Height = 0,
			AlignItems = AlignItems.Center,
			JustifyContent = JustifyContent.Center
		};

		_min = min;
		_max = max;
		_value = initialValue;
		_setValue = valueSetter;
		_handleContainer = new Container(_handleContainerLayout);
		_handle = new SliderHandle(handleLayout, ClickSliderOrHandle, NudgeSlider);

		_handleContainer.AddChild(_handle);
		AddChild(_handleContainer);
	}

	protected override void OnMouseEvent(byte button, ButtonState buttonState)
	{
		if (MouseIsContained)
			ClickSliderOrHandle(buttonState);

		if (buttonState == ButtonState.Released)
			_isDragging = false;
	}

	protected override void OnUpdate(GameTime gameTime)
	{
		if (_isDragging)
		{
			Value = GetValue();
			// Don't un-activate when mouse goes above or below slider handle
			_handle.State = ElementStates.Activated;
		}

		var handleContainerLayout = _handleContainer.Layout;
		handleContainerLayout.Left = (GetPercentage() * BoundingRectangle.Width);
	}

	protected override void OnDraw(SpriteBatch spriteBatch)
	{
		// Draw Foreground as a percentage of width
		var foregroundRec = BoundingRectangle;
		foregroundRec.Width = (int)(BoundingRectangle.Width * GetPercentage());

		if (CurrentLayout.ForegroundColor != Color.Transparent)
			UiPrimitiveDrawer.DrawRectangle(spriteBatch, foregroundRec, CurrentLayout.ForegroundColor);
	}

	private bool NudgeSlider(UIDirection d, UISide s)
	{
		if (d == UIDirection.Vertical)
			return false;

		var oldValue = Value;

		if (s == UISide.Start)
			Value -= NudgeIncrement * (_max - _min);
		else
			Value += NudgeIncrement * (_max - _min);

		Value = MathHelper.Clamp(Value, _min, _max);

		// If value changed, focus was consumed
		return (Value != oldValue);
	}

	private void ClickSliderOrHandle(ButtonState buttonState)
	{
		if (buttonState == ButtonState.Pressed)
		{
			_isDragging = true;
			Value = GetValue();
		}
	}

	private float GetPercentage()
	{
		return (Value - _min) / (_max - _min);
	}

	private float GetValue()
	{
		float x = Mouse.GetState().Position.X - BoundingRectangle.X;
		float proportion = x / BoundingRectangle.Width;
		return MathHelper.Clamp(MathHelper.Lerp(_min, _max, proportion), _min, _max);
	}

	internal class SliderHandle : Button
	{
		private Action<ButtonState> _clickCallback;
		private Func<UIDirection, UISide, bool> _focusChangeCallback;


		internal SliderHandle(Layout? layout, Action<ButtonState> clickCallback, Func<UIDirection, UISide, bool> focusChangeCallback)
			: base(layout)
		{
			_clickCallback = clickCallback;
			_focusChangeCallback = focusChangeCallback;
			OverrideFocusChange = true;
		}

		protected override void OnMouseEvent(byte mouseButton, ButtonState buttonState)
		{
			if (MouseIsContained)
				_clickCallback(buttonState);

			if (buttonState == ButtonState.Released)
			{
				if (MouseIsContained)
					State = ElementStates.Hovered;
				else
					State = ElementStates.Neutral;
			}
		}

		public override bool ConsumeFocus(UIDirection d, UISide s)
		{
			return _focusChangeCallback(d, s);
		}
	}
}