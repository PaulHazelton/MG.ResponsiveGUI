using System;
using BrokenH.MG.ResponsiveGui.Styles;

namespace BrokenH.MG.ResponsiveGui.Elements;

public class Checkbox : Button
{
	private bool _value;
	public bool Value
	{
		get => _value;
		private set => _value = value;
	}

	public Action<bool>? ValueSetter { get; set; }

	public Layout UncheckedLayout { get; set; }
	public Layout CheckedLayout { get; set; }


	public Checkbox(Layout uncheckedLayout, Layout checkedLayout, bool value, Action<bool>? valueSetter)
		: base(value ? checkedLayout : uncheckedLayout, null, string.Empty)
	{
		_value = value;
		ValueSetter = valueSetter;

		UncheckedLayout = uncheckedLayout;
		CheckedLayout = checkedLayout;
	}

	protected override void OnActivateRelease()
	{
		base.OnActivateRelease();

		Value = !Value;

		Layout = Value ? CheckedLayout : UncheckedLayout;

		ValueSetter?.Invoke(Value);
	}
}