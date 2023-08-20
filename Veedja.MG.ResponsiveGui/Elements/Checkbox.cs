using System;
using Veedja.MG.ResponsiveGui.Styles;

namespace Veedja.MG.ResponsiveGui.Elements;

public class Checkbox : Button
{
	private bool _value;
	public bool Value
	{
		get => _value;
		set
		{
			_value = value;
			OnValueSet();
		}
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
	}

	private void OnValueSet()
	{
		Layout = Value ? CheckedLayout : UncheckedLayout;

		ValueSetter?.Invoke(Value);
	}
}