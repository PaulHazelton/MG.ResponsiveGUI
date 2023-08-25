using System;
using Veedja.MG.ResponsiveGui.Styles;

namespace Veedja.MG.ResponsiveGui.Elements;

public class Checkbox : Button
{
	// Backing fields
	private bool _value;

	// Properties
	public bool Value
	{
		get => _value;
		set
		{
			if (_value != value)
			{
				_value = value;
				OnValueSet();
			}
		}
	}
	public Action<bool>? OnValueChange { get; set; }

	public Layout UncheckedLayout { get; set; }
	public Layout CheckedLayout { get; set; }


	public Checkbox(Layout uncheckedLayout, Layout checkedLayout, bool value)
		: base(value ? checkedLayout : uncheckedLayout, null, string.Empty)
	{
		_value = value;

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

		OnValueChange?.Invoke(Value);
	}
}