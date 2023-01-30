using System;
using Microsoft.Xna.Framework;
using BrokenH.MG.ResponsiveGui.Elements;

namespace BrokenH.MG.ResponsiveGui.Styles
{
	public class LayoutTransitioner
	{
		private Layout _mainLayout;
		private double _elapsedTime;

		private string _currentState = ElementStates.Neutral;

		private string State
		{
			get => _currentState;
			set
			{
				if (_currentState != value)
				{
					_currentState = value;
					_elapsedTime = 0;
				}
			}
		}


		public LayoutTransitioner(Layout layout)
		{
			_mainLayout = layout;
		}

		public Layout Update(GameTime gameTime, string state, Layout oldLayout)
		{
			State = state;

			// Setup variables
			var newLayout = _mainLayout.GetLayoutFromState(_currentState);
			var transition = newLayout.Transition;

			// Conditions
			if (transition == null)
				return newLayout;

			// If there is a transition happening
			if (_elapsedTime < transition.TotalDuration)
			{
				_elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
				return LayoutTransitioner.InterpolateWithTransition(oldLayout, newLayout, transition, _elapsedTime);
			}
			else
			{
				if (newLayout.Animation == null)
					return newLayout;
				else
					return newLayout.Animation.Update(gameTime);
			}
		}

		public void Reset(string state = ElementStates.Neutral)
		{
			State = state;
			_mainLayout.GetLayoutFromState(State).Animation?.Reset();
		}

		#region Private / Internal Helper Functions

		internal static Layout InterpolateWithTransition(Layout oldLayout, Layout newLayout, Transition? transition, double timeSinceStateChange)
		{
			// Set up
			if (transition == null || oldLayout == newLayout)
				return newLayout;

			var time = timeSinceStateChange - transition.Delay;

			if (time >= transition.Duration)
				return newLayout;

			Layout resultLayout = new(newLayout);

			float t = (float)(time / transition.Duration);

			t = TransformByTimingFunction(transition, t);

			// Lerps
			resultLayout._fontScale = MathHelper.Lerp(oldLayout._fontScale, newLayout._fontScale, t);

			resultLayout.Transform = Matrix.Lerp(oldLayout.Transform, newLayout.Transform, t);

			resultLayout.BackgroundColor = Color.Lerp(oldLayout.BackgroundColor, newLayout.BackgroundColor, t);
			resultLayout.ForegroundColor = Color.Lerp(oldLayout.ForegroundColor, newLayout.ForegroundColor, t);

			resultLayout.BorderColorLeft = Color.Lerp(oldLayout.BorderColorLeft, newLayout.BorderColorLeft, t);
			resultLayout.BorderColorRight = Color.Lerp(oldLayout.BorderColorRight, newLayout.BorderColorRight, t);
			resultLayout.BorderColorTop = Color.Lerp(oldLayout.BorderColorTop, newLayout.BorderColorTop, t);
			resultLayout.BorderColorBottom = Color.Lerp(oldLayout.BorderColorBottom, newLayout.BorderColorBottom, t);

			resultLayout._left = Lerp(oldLayout._left, newLayout._left, t);
			resultLayout._right = Lerp(oldLayout._right, newLayout._right, t);
			resultLayout._top = Lerp(oldLayout._top, newLayout._top, t);
			resultLayout._bottom = Lerp(oldLayout._bottom, newLayout._bottom, t);

			resultLayout._width = Lerp(oldLayout._width, newLayout._width, t);
			resultLayout._height = Lerp(oldLayout._height, newLayout._height, t);
			resultLayout._maxWidth = Lerp(oldLayout._maxWidth, newLayout._maxWidth, t);
			resultLayout._maxHeight = Lerp(oldLayout._maxHeight, newLayout._maxHeight, t);

			resultLayout._marginLeft = MathHelper.Lerp(oldLayout._marginLeft, newLayout._marginLeft, t);
			resultLayout._marginRight = MathHelper.Lerp(oldLayout._marginRight, newLayout._marginRight, t);
			resultLayout._marginTop = MathHelper.Lerp(oldLayout._marginTop, newLayout._marginTop, t);
			resultLayout._marginBottom = MathHelper.Lerp(oldLayout._marginBottom, newLayout._marginBottom, t);

			resultLayout._gap = MathHelper.Lerp(oldLayout._gap, newLayout._gap, t);

			resultLayout._paddingLeft = MathHelper.Lerp(oldLayout._paddingLeft, newLayout._paddingLeft, t);
			resultLayout._paddingRight = MathHelper.Lerp(oldLayout._paddingRight, newLayout._paddingRight, t);
			resultLayout._paddingTop = MathHelper.Lerp(oldLayout._paddingTop, newLayout._paddingTop, t);
			resultLayout._paddingBottom = MathHelper.Lerp(oldLayout._paddingBottom, newLayout._paddingBottom, t);

			resultLayout._borderThicknessLeft = MathHelper.Lerp(oldLayout._borderThicknessLeft, newLayout._borderThicknessLeft, t);
			resultLayout._borderThicknessRight = MathHelper.Lerp(oldLayout._borderThicknessRight, newLayout._borderThicknessRight, t);
			resultLayout._borderThicknessTop = MathHelper.Lerp(oldLayout._borderThicknessTop, newLayout._borderThicknessTop, t);
			resultLayout._borderThicknessBottom = MathHelper.Lerp(oldLayout._borderThicknessBottom, newLayout._borderThicknessBottom, t);

			return resultLayout;
		}

		private static float TransformByTimingFunction(Transition transition, float t) => transition.TimingFunction switch
		{
			TimingFunction.Linear => t,
			TimingFunction.SmoothStep => MathHelper.SmoothStep(0, 1, t),
			TimingFunction.EaseInCubic => EaseInCubic(t),
			TimingFunction.EaseOutCubic => EaseOutCubic(t),
			TimingFunction.EaseInOutSine => EaseInOutSine(t),
			TimingFunction.EaseInOutCubic => EaseInOutCubic(t),
			TimingFunction.Custom => transition.CustomTimingFunction?.Invoke(t) ?? t,
			_ => t
		};
		private static float EaseInCubic(float t)
		{
			return t * t * t;
		}
		private static float EaseOutCubic(float t)
		{
			return (float)(1 - Math.Pow(1 - t, 3));
		}
		private static float EaseInOutSine(float t)
		{
			return (float)(-(Math.Cos(Math.PI * t) - 1) / 2);
		}
		private static float EaseInOutCubic(float t)
		{
			return (float)(t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2);
		}

		private static float? Lerp(float? oldFloat, float? newFloat, float t)
		{
			if (oldFloat == null || newFloat == null)
				return null;
			return MathHelper.Lerp(oldFloat.Value, newFloat.Value, t);
		}

		#endregion
	}
}