using System;

namespace BrokenH.MG.ResponsiveGui.Styles
{
	public class Transition
	{
		public double Duration { get; set; }
		public double Delay { get; set; }
		public TimingFunction TimingFunction { get; set; }
		public Func<float, float>? CustomTimingFunction { get; set; }

		public double TotalDuration => Duration + Delay;


		public Transition(Transition toCopy)
		{
			Duration = toCopy.Duration;
			Delay = toCopy.Delay;
			TimingFunction = toCopy.TimingFunction;
			CustomTimingFunction = toCopy.CustomTimingFunction;
		}
		public Transition(double duration, TimingFunction timingFunction = TimingFunction.Linear, double delay = 0, Func<float, float>? customTimingFunction = null)
		{
			Duration = duration;
			Delay = delay;
			TimingFunction = timingFunction;
			CustomTimingFunction = customTimingFunction;
		}
	}
}