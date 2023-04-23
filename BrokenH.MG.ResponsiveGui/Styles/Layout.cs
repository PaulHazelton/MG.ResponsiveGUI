#pragma warning disable CS8524  // Simple switch expressions

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BrokenH.MG.ResponsiveGui.Elements;

namespace BrokenH.MG.ResponsiveGui.Styles
{
	public class Layout
	{
		#region Statics

		private static SpriteFont? DefaultFont;

		public static float UIScale { get; set; } = 1.0f;

		public static bool ScaleFontWithUIScale { get; set; } = true;

		public static void Initialize(SpriteFont defaultFont)
		{
			DefaultFont = defaultFont;
		}

		private static float? GetValueByUnit(float? value, LayoutUnit unit) => unit switch
		{
			LayoutUnit.Pixels => value * UIScale,
			LayoutUnit.Percent => value, // The Element class is in charge of computing width percent.
		};

		#endregion

		#region Private / Internal Backing fields

		internal float _fontScale = 1;

		internal float? _top;
		internal float? _bottom;
		internal float? _left;
		internal float? _right;

		internal float? _width;
		internal float? _height;

		internal float? _maxWidth;
		internal float? _maxHeight;

		internal float _marginLeft;
		internal float _marginRight;
		internal float _marginTop;
		internal float _marginBottom;

		internal float _gap;

		internal float _paddingLeft;
		internal float _paddingRight;
		internal float _paddingTop;
		internal float _paddingBottom;

		internal float _borderThicknessLeft;
		internal float _borderThicknessRight;
		internal float _borderThicknessTop;
		internal float _borderThicknessBottom;

		internal float _scrollPadding;

		#endregion

		#region Main Layout Properties

		public Transition? Transition { get; set; }
		public Animation? Animation { get; set; }
		public Matrix Transform { get; set; } = Matrix.Identity;

		public Color BackgroundColor { get; set; } = Color.Transparent;
		public Color ForegroundColor { get; set; } = Color.Transparent;

		public Color BorderColorLeft { get; set; } = Color.Transparent;
		public Color BorderColorRight { get; set; } = Color.Transparent;
		public Color BorderColorTop { get; set; } = Color.Transparent;
		public Color BorderColorBottom { get; set; } = Color.Transparent;

		public Texture2D? Image { get; set; }
		public Color ImageColor { get; set; } = Color.White;
		public Rectangle? SourceRectangle { get; set; } = null;

		public NineSlice? NineSlice { get; set; }

		public SpriteFont Font { get; set; }
		public float FontScale { get => ScaleFontWithUIScale ? _fontScale * UIScale : _fontScale; set => _fontScale = value; }

		public TextAlign TextAlign = TextAlign.Center;
		public TextAlignVertical TextAlignVertical = TextAlignVertical.Middle;
		public WordWrapMode WordWrapMode = WordWrapMode.None;

		public Overflow OverflowX { get; set; } = Overflow.Visible;
		public Overflow OverflowY { get; set; } = Overflow.Visible;
		public bool AllowScrollingFromAnywhere = false;

		public FlexDirection FlexDirection { get; set; } = FlexDirection.Column;
		public JustifyContent JustifyContent { get; set; } = JustifyContent.FlexStart;
		public AlignItems AlignItems { get; set; } = AlignItems.FlexStart;

		public PositionMode PositionMode { get; set; } = PositionMode.Static;

		public float? Left { get => GetValueByUnit(_left, LeftUnit); set => _left = value; }
		public float? Right { get => GetValueByUnit(_right, RightUnit); set => _right = value; }
		public float? Top { get => GetValueByUnit(_top, TopUnit); set => _top = value; }
		public float? Bottom { get => GetValueByUnit(_bottom, BottomUnit); set => _bottom = value; }

		public float? Width { get => GetValueByUnit(_width, WidthUnit); set => _width = value; }
		public float? Height { get => GetValueByUnit(_height, HeightUnit); set => _height = value; }

		public float? MaxWidth { get => GetValueByUnit(_maxWidth, WidthUnit); set => _maxWidth = value; }
		public float? MaxHeight { get => GetValueByUnit(_maxHeight, HeightUnit); set => _maxHeight = value; }

		public float MarginLeft { get => _marginLeft * UIScale; set => _marginLeft = value; }
		public float MarginRight { get => _marginRight * UIScale; set => _marginRight = value; }
		public float MarginTop { get => _marginTop * UIScale; set => _marginTop = value; }
		public float MarginBottom { get => _marginBottom * UIScale; set => _marginBottom = value; }

		public float Gap { get => _gap * UIScale; set => _gap = value; }

		public float PaddingLeft { get => _paddingLeft * UIScale; set => _paddingLeft = value; }
		public float PaddingRight { get => _paddingRight * UIScale; set => _paddingRight = value; }
		public float PaddingTop { get => _paddingTop * UIScale; set => _paddingTop = value; }
		public float PaddingBottom { get => _paddingBottom * UIScale; set => _paddingBottom = value; }

		public float BorderThicknessLeft { get => _borderThicknessLeft * Layout.UIScale; set => _borderThicknessLeft = value; }
		public float BorderThicknessRight { get => _borderThicknessRight * Layout.UIScale; set => _borderThicknessRight = value; }
		public float BorderThicknessTop { get => _borderThicknessTop * Layout.UIScale; set => _borderThicknessTop = value; }
		public float BorderThicknessBottom { get => _borderThicknessBottom * Layout.UIScale; set => _borderThicknessBottom = value; }

		public float ScrollPadding { get => _scrollPadding * UIScale; set => _scrollPadding = value; }

		public LayoutUnit LeftUnit { get; set; } = LayoutUnit.Pixels;
		public LayoutUnit RightUnit { get; set; } = LayoutUnit.Pixels;
		public LayoutUnit TopUnit { get; set; } = LayoutUnit.Pixels;
		public LayoutUnit BottomUnit { get; set; } = LayoutUnit.Pixels;

		public LayoutUnit WidthUnit { get; set; } = LayoutUnit.Pixels;
		public LayoutUnit HeightUnit { get; set; } = LayoutUnit.Pixels;

		public LayoutUnit MaxWidthUnit { get; set; } = LayoutUnit.Pixels;
		public LayoutUnit MaxHeightUnit { get; set; } = LayoutUnit.Pixels;

		#endregion

		// Sub-layouts
		public Dictionary<string, Layout> SubLayouts { get; set; }
		public Layout this[string key] { get => SubLayouts[key]; set => SubLayouts[key] = value; }
		public Layout GetLayoutFromState(string state) => state switch
		{
			ElementStates.Neutral => this,
			_ => this.SubLayouts.GetValueOrDefault(state) ?? this,
		};

		// Constructors
		public Layout()
		{
			SubLayouts = new();

			Font = DefaultFont ?? throw new System.InvalidOperationException($"Must call {nameof(Initialize)} before creating a layout object");
		}
		public Layout(Layout toCopy) : this(toCopy, true) { }
		private Layout(Layout toCopy, bool copySublayouts)
		{
			if (toCopy.Transition != null)
				Transition = new Transition(toCopy.Transition);

			Animation = toCopy.Animation;

			Transform = toCopy.Transform;

			OverflowX = toCopy.OverflowX;
			OverflowY = toCopy.OverflowY;
			AllowScrollingFromAnywhere = toCopy.AllowScrollingFromAnywhere;

			Image = toCopy.Image;
			ImageColor = toCopy.ImageColor;
			SourceRectangle = toCopy.SourceRectangle;

			NineSlice = toCopy.NineSlice;

			Font = toCopy.Font;
			FontScale = toCopy.FontScale;

			TextAlign = toCopy.TextAlign;
			TextAlignVertical = toCopy.TextAlignVertical;
			WordWrapMode = toCopy.WordWrapMode;

			TextAlign = toCopy.TextAlign;
			WordWrapMode = toCopy.WordWrapMode;

			BackgroundColor = toCopy.BackgroundColor;
			ForegroundColor = toCopy.ForegroundColor;

			_borderThicknessLeft = toCopy._borderThicknessLeft;
			_borderThicknessRight = toCopy._borderThicknessRight;
			_borderThicknessTop = toCopy._borderThicknessTop;
			_borderThicknessBottom = toCopy._borderThicknessBottom;

			_scrollPadding = toCopy._scrollPadding;

			BorderColorLeft = toCopy.BorderColorLeft;
			BorderColorRight = toCopy.BorderColorRight;
			BorderColorTop = toCopy.BorderColorTop;
			BorderColorBottom = toCopy.BorderColorBottom;

			FlexDirection = toCopy.FlexDirection;
			JustifyContent = toCopy.JustifyContent;
			AlignItems = toCopy.AlignItems;

			_top = toCopy._top;
			_bottom = toCopy._bottom;
			_left = toCopy._left;
			_right = toCopy._right;

			_width = toCopy._width;
			_height = toCopy._height;

			_maxWidth = toCopy._maxWidth;
			_maxHeight = toCopy._maxHeight;

			PositionMode = toCopy.PositionMode;

			TopUnit = toCopy.TopUnit;
			BottomUnit = toCopy.BottomUnit;
			LeftUnit = toCopy.LeftUnit;
			RightUnit = toCopy.RightUnit;

			WidthUnit = toCopy.WidthUnit;
			HeightUnit = toCopy.HeightUnit;

			MaxWidthUnit = toCopy.MaxWidthUnit;
			MaxHeightUnit = toCopy.MaxHeightUnit;

			MarginLeft = toCopy.MarginLeft;
			MarginRight = toCopy.MarginRight;
			MarginTop = toCopy.MarginTop;
			MarginBottom = toCopy.MarginBottom;

			Gap = toCopy.Gap;

			PaddingLeft = toCopy.PaddingLeft;
			PaddingRight = toCopy.PaddingRight;
			PaddingTop = toCopy.PaddingTop;
			PaddingBottom = toCopy.PaddingBottom;

			SubLayouts = copySublayouts ? toCopy.SubLayouts.ToDictionary(kvp => kvp.Key, kvp => new Layout(kvp.Value, false)) : new();
		}

		public Layout Copy() => new Layout(this);

		#region Additional Setters

		public float BorderThickness { set => SetBorderThickness(value); }
		public Color BorderColor { set => SetBorderColor(value); }
		// public int BorderRadius { get; set; }

		public string Top_ { set => (Top, TopUnit) = ParseLiteral(value); }
		public string Bottom_ { set => (Bottom, BottomUnit) = ParseLiteral(value); }
		public string Left_ { set => (Left, LeftUnit) = ParseLiteral(value); }
		public string Right_ { set => (Right, RightUnit) = ParseLiteral(value); }

		/// <summary>
		/// Set Width and WidthUnit at the same time with one string
		/// The acceptable units are "px" and "%"
		/// </summary>
		public string Width_ { set => (Width, WidthUnit) = ParseLiteral(value); }
		/// <summary>
		/// Set Height and HeightUnit at the same time with one string
		/// The acceptable units are "px" and "%"
		/// </summary>
		public string Height_ { set => (Height, HeightUnit) = ParseLiteral(value); }

		public string MaxWidth_ { set => (MaxWidth, MaxWidthUnit) = ParseLiteral(value); }
		public string MaxHeight_ { set => (MaxHeight, MaxHeightUnit) = ParseLiteral(value); }

		public int Margin { set => SetMargins(value); }
		public (int, int) Margin2 { set => SetMargins(value.Item1, value.Item2); }
		public (int, int, int, int) Margin4 { set => SetMargins(value.Item1, value.Item2, value.Item3, value.Item4); }

		public int Padding { set => SetPadding(value); }
		public (int, int) Padding2 { set => SetPadding(value.Item1, value.Item2); }
		public (int, int, int, int) Padding4 { set => SetPadding(value.Item1, value.Item2, value.Item3, value.Item4); }

		private (int, LayoutUnit) ParseLiteral(string literal)
		{
			try
			{
				if (literal.Contains("px"))
					return (int.Parse(literal.Substring(0, literal.Length - 2)), LayoutUnit.Pixels);
				else if (literal.Contains("%"))
					return (int.Parse(literal.Substring(0, literal.Length - 1)), LayoutUnit.Percent);
				else
					return (int.Parse(literal), LayoutUnit.Pixels);
			}
			catch (System.Exception)
			{
				throw new System.ArgumentException($"The value provided {literal} is not a valid value");
			}
		}

		private void SetBorderThickness(float thickness)
		{
			BorderThicknessLeft = thickness;
			BorderThicknessRight = thickness;
			BorderThicknessTop = thickness;
			BorderThicknessBottom = thickness;
		}
		private void SetBorderColor(Color color)
		{
			BorderColorLeft = color;
			BorderColorRight = color;
			BorderColorTop = color;
			BorderColorBottom = color;
		}

		private void SetPadding(int padding)
		{
			PaddingLeft = padding;
			PaddingRight = padding;
			PaddingTop = padding;
			PaddingBottom = padding;
		}
		private void SetPadding(int verticalPadding, int horizontalPadding)
		{
			PaddingTop = verticalPadding;
			PaddingBottom = verticalPadding;
			PaddingLeft = horizontalPadding;
			PaddingRight = horizontalPadding;
		}
		private void SetPadding(int top, int right, int bottom, int left)
		{
			PaddingTop = top;
			PaddingRight = right;
			PaddingBottom = bottom;
			PaddingLeft = left;
		}

		private void SetMargins(int margin)
		{
			MarginLeft = margin;
			MarginRight = margin;
			MarginTop = margin;
			MarginBottom = margin;
		}
		private void SetMargins(int verticalMargin, int horizontalMargin)
		{
			MarginTop = verticalMargin;
			MarginBottom = verticalMargin;
			MarginLeft = horizontalMargin;
			MarginRight = horizontalMargin;
		}
		private void SetMargins(int top, int right, int bottom, int left)
		{
			MarginTop = top;
			MarginRight = right;
			MarginBottom = bottom;
			MarginLeft = left;
		}

		#endregion

		#region Convenient Getters

		internal bool RequiresComplicatedDraw
			=> OverflowX != Overflow.Visible
			|| OverflowY != Overflow.Visible
			|| Transform != Matrix.Identity;

		internal Overflow GetOverflow(UIDirection d) => d switch
		{
			UIDirection.Horizontal => OverflowX,
			UIDirection.Vertical => OverflowY,
		};
		internal float? GetPosition(UIDirection d, UISide s) => d switch
		{
			UIDirection.Horizontal => s == UISide.Start ? Left : Right,
			UIDirection.Vertical => s == UISide.Start ? Top : Bottom,
		};
		internal LayoutUnit GetPositionUnit(UIDirection d, UISide s) => d switch
		{
			UIDirection.Horizontal => s == UISide.Start ? LeftUnit : RightUnit,
			UIDirection.Vertical => s == UISide.Start ? TopUnit : BottomUnit,
		};
		internal float? GetSize(UIDirection d) => d switch
		{
			UIDirection.Horizontal => Width,
			UIDirection.Vertical => Height,
		};
		internal LayoutUnit GetSizeUnit(UIDirection d) => d switch
		{
			UIDirection.Horizontal => WidthUnit,
			UIDirection.Vertical => HeightUnit,
		};
		internal float? GetMaxSize(UIDirection d) => d switch
		{
			UIDirection.Horizontal => MaxWidth,
			UIDirection.Vertical => MaxHeight,
		};
		internal LayoutUnit GetMaxSizeUnit(UIDirection d) => d switch
		{
			UIDirection.Horizontal => MaxWidthUnit,
			UIDirection.Vertical => MaxHeightUnit,
		};
		internal float GetMargin(UIDirection d, UISide s) => d switch
		{
			UIDirection.Horizontal => s == UISide.Start ? MarginLeft : MarginRight,
			UIDirection.Vertical => s == UISide.Start ? MarginTop : MarginBottom,
		};
		internal float GetPadding(UIDirection d, UISide s) => d switch
		{
			UIDirection.Horizontal => s == UISide.Start ? PaddingLeft : PaddingRight,
			UIDirection.Vertical => s == UISide.Start ? PaddingTop : PaddingBottom,
		};

		#endregion
	}
}