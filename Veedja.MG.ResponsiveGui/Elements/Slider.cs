using System;
using Veedja.MG.ResponsiveGui.Common;
using Veedja.MG.ResponsiveGui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Veedja.MG.ResponsiveGui.Elements;

public class Slider : GuiElement
{
	private readonly Layout _handleContainerLayout;
	private readonly Container _handleContainer;
	private readonly Button _handle;

	// Backing fields
	private float _value;
	private float _valueTarget;

	// Properties
	public float NudgeLerpSpeed { get; set; } = 12f;
	public float NudgeIncrement { get; set; } = 0.1f;

	public float Min { get; set; }
	public float Max { get; set; }
	public float Value
	{
		get => _value;
		set
		{
			var newVal = MathHelper.Clamp(value, Min, Max);
			if (_value != newVal)
			{
				_value = newVal;
				OnValueChange?.Invoke(newVal);
			}
		}
	}
	public float ValueTarget
	{
		get => _valueTarget;
		set
		{
			var newVal = MathHelper.Clamp(value, Min, Max);
			if (_valueTarget != newVal)
			{
				_valueTarget = newVal;
				OnValueTargetChange?.Invoke(newVal);
			}
		}
	}
	public Action<float>? OnValueChange { get; set; }
	public Action<float>? OnValueTargetChange { get; set; }

	public bool IsDragging { get; set; }


	public Slider(Layout sliderLayout, Layout handleLayout, float min, float max, float value)
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
		_handleContainer = new Container(_handleContainerLayout);
		_handle = new SliderHandle(handleLayout, ClickSliderOrHandle, NudgeSlider);
		_handleContainer.AddChild(_handle);
		AddChild(_handleContainer);

		_value = value;
		Min = min;
		Max = max;
		ValueTarget = value;
	}

	protected override void OnMouseEvent(byte button, ButtonState buttonState)
	{
		if (MouseIsContained)
			ClickSliderOrHandle(buttonState);

		if (buttonState == ButtonState.Released)
			IsDragging = false;
	}

	protected override void OnUpdate(GameTime gameTime)
	{
		if (IsDragging)
		{
			ComputeValue();
			// Don't un-activate when mouse goes above or below slider handle
			_handle.State = ElementStates.Activated;
		}
		else
		{
			// Lerp value to target
			Value = MathHelper.Lerp(Value, ValueTarget, (float)(NudgeLerpSpeed * gameTime.ElapsedGameTime.TotalSeconds));

			// Snap to end if close
			if (Math.Abs(ValueTarget - Value) < (Max - Min) * 0.001f)
				Value = ValueTarget;
		}

		var handleContainerLayout = _handleContainer.Layout;

		var proportion = GetPercentage();

		if (Layout.JustifyContent == JustifyContent.FlexEnd)
			proportion = -proportion;

		if (Layout.FlexDirection == FlexDirection.Row)
			handleContainerLayout.Left = proportion * BoundingRectangle.Width;
		else
			handleContainerLayout.Top = proportion * BoundingRectangle.Height;
	}

	protected override void OnDraw(SpriteBatch spriteBatch)
	{
		var foregroundRec = BoundingRectangle;

		var proportion = GetPercentage();

		if (Layout.FlexDirection == FlexDirection.Row)
			foregroundRec.Width = (int)(BoundingRectangle.Width * proportion);
		else
			foregroundRec.Height = (int)(BoundingRectangle.Height * proportion);

		if (Layout.JustifyContent == JustifyContent.FlexEnd)
		{
			if (Layout.FlexDirection == FlexDirection.Row)
				foregroundRec.X += BoundingRectangle.Width - foregroundRec.Width;
			else
				foregroundRec.Y += BoundingRectangle.Height - foregroundRec.Height;
		}

		if (CurrentLayout.ForegroundColor != Color.Transparent)
			UiPrimitiveDrawer.DrawRectangle(spriteBatch, foregroundRec, CurrentLayout.ForegroundColor);
	}

	private bool NudgeSlider(UIDirection d, UISide s)
	{
		if (!Layout.FlexDirection.IsParallelWith(d))
			return false;

		var oldValue = ValueTarget;
		float difference;

		if (s == UISide.End)
			difference = NudgeIncrement * (Max - Min);
		else
			difference = -NudgeIncrement * (Max - Min);

		if (Layout.JustifyContent == JustifyContent.FlexEnd)
			difference *= -1;

		ValueTarget = MathHelper.Clamp(ValueTarget + difference, Min, Max);

		// If value changed, focus was consumed
		return ValueTarget != oldValue;
	}

	private void ClickSliderOrHandle(ButtonState buttonState)
	{
		if (buttonState == ButtonState.Pressed)
		{
			IsDragging = true;
			ComputeValue();
		}
	}

	private float GetPercentage()
	{
		return (Value - Min) / (Max - Min);
	}

	private void ComputeValue()
	{
		float proportion = Layout.FlexDirection switch
		{
			FlexDirection.Row => (Mouse.GetState().Position.X - BoundingRectangle.X) / (float)BoundingRectangle.Width,
			FlexDirection.Column => (Mouse.GetState().Position.Y - BoundingRectangle.Y) / (float)BoundingRectangle.Height,
			_ => 0
		};

		if (Layout.JustifyContent == JustifyContent.FlexEnd)
			proportion = 1 - proportion;

		Value = MathHelper.Lerp(Min, Max, proportion);
		ValueTarget = Value;
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

		public override bool ConsumeFocus(UIDirection d, UISide s) => _focusChangeCallback(d, s);
	}
}