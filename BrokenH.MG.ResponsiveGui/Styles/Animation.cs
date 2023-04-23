using Microsoft.Xna.Framework;

namespace BrokenH.MG.ResponsiveGui.Styles;

public class Animation
{
	private double _elapsedTime;
	private int _currentFrame;

	private KeyFrame[] _keyFrames;
	private AnimationLoopType _animationLoopType;
	private int _loopCount;

	public int IterationsComplete { get; private set; } = 0;
	public bool IsComplete { get; private set; } = false;


	public Animation(KeyFrame[] keyFrames, AnimationLoopType animationLoopType, int loopCount = 1)
	{
		_keyFrames = keyFrames;
		_animationLoopType = animationLoopType;
		_loopCount = loopCount;
	}

	public void Reset()
	{
		_elapsedTime = 0;
		_currentFrame = 0;
		IterationsComplete = 0;
		IsComplete = false;
	}

	public Layout Update(GameTime gameTime)
	{
		_elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
		var transition = _keyFrames[_currentFrame].Transition;

		if (_elapsedTime >= transition.Delay + transition.Duration)
		{
			_elapsedTime = 0;
			_currentFrame++;

			if (_currentFrame == _keyFrames.Length)
			{
				_currentFrame = 0;
				IterationsComplete++;

				if (_animationLoopType == AnimationLoopType.LoopOnce || (_animationLoopType == AnimationLoopType.LoopCount && IterationsComplete >= _loopCount))
					IsComplete = true;
			}
		}

		var oldKf = _keyFrames[_currentFrame];
		var newKf = _keyFrames[(_currentFrame + 1) % _keyFrames.Length];
		return LayoutTransitioner.InterpolateWithTransition(oldKf.Layout, newKf.Layout, oldKf.Transition, _elapsedTime);
	}
}

public struct KeyFrame
{
	public Transition Transition;
	public Layout Layout;
	public KeyFrame(Transition transition, Layout layout)
	{
		Transition = transition;
		Layout = layout;
	}
}

public enum AnimationLoopType
{
	LoopOnce,
	LoopCount,
	LoopForever,
}