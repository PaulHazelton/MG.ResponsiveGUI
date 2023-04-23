#pragma warning disable CS8524	// Simple switch expressions

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using BrokenH.MG.ResponsiveGui.Styles;

namespace BrokenH.MG.ResponsiveGui.Elements;

public abstract class GuiElement : IDisposable
{
	#region Statics

	// Global initialization
	public static void Initialize(int screenWidth, int screenHeight)
	{
		ScreenWidth = screenWidth;
		ScreenHeight = screenHeight;
	}

	public static void UpdateSize(int screenWidth, int screenHeight)
	{
		ScreenWidth = screenWidth;
		ScreenHeight = screenHeight;
	}

	// Behavior tuning
	public static bool HoldShiftToScrollHorizontally = true;
	public static float ScrollScale { get; set; } = 0.3f;
	public static float SmoothScrollLerpSpeed { get; set; } = 12f;

	// Global state
	private static int ScreenWidth { get; set; }
	private static int ScreenHeight { get; set; }

	// Helper functions
	private static int ScreenSize(UIDirection d) => d switch
	{
		UIDirection.Horizontal => ScreenWidth,
		UIDirection.Vertical => ScreenHeight,
		_ => 0,
	};
	private static (float main, float gap) CalcMainAndGap(List<GuiElement> _children, Layout Layout, FloatRectangle InnerRectangle)
	{
		FlexDirection fd = Layout.FlexDirection;
		float main = InnerRectangle.MainStart(fd);
		float sum = 0;
		float gap = Layout.Gap;
		switch (Layout.JustifyContent)
		{
			case JustifyContent.FlexStart: break;
			case JustifyContent.Center:
				sum = -Layout.Gap;
				Sum();
				main += ((InnerRectangle.MainSize(fd) - sum) / 2.0f);
				break;
			case JustifyContent.FlexEnd:
				sum = -Layout.Gap;
				Sum();
				main += InnerRectangle.MainSize(fd) - sum;
				break;
			case JustifyContent.SpaceBetween:
				Sum(false);
				gap = _children.Count > 1 ? (InnerRectangle.MainSize(fd) - sum) / (_children.Count - 1) : 0;
				break;
		}

		void Sum(bool includeGap = true)
		{
			foreach (var child in _children)
			{
				if (child.Layout.PositionMode.NormalFlow())
					sum += child.CalculatedMainSize(fd) + (includeGap ? Layout.Gap : 0);
			}
		}

		return (main, gap);
	}
	private static float CalcCrossAxis(FlexDirection flexDirection, AlignItems alignItems, FloatRectangle innerRectangle, GuiElement child)
	{
		float start = innerRectangle.CrossStart(flexDirection);
		float leftover = innerRectangle.CrossSize(flexDirection) - child.CalculatedCrossSize(flexDirection);
		switch (alignItems)
		{
			case AlignItems.FlexStart: return start;
			case AlignItems.FlexEnd: return start + leftover;
			case AlignItems.Center: return start + leftover / 2;
			default: return start;
		}
	}

	#endregion

	#region Private variables

	private LayoutTransitioner _layoutTransitioner;

	internal Matrix? _currentTransform;

	// Positioning
	private float? _calculatedLeft;
	private float? _calculatedRight;
	private float? _calculatedTop;
	private float? _calculatedBottom;

	// Sizing
	private float? _specifiedWidth;
	private float? _specifiedHeight;

	private float? _specifiedMaxWidth;
	private float? _specifiedMaxHeight;

	/// <summary>
	/// The sum of the bounding rectangle width and the x margins
	/// </summary>
	private float _calculatedOuterWidth { get; set; }
	/// <summary>
	/// The sum of the bounding rectangle height and the y margins
	/// </summary>
	private float _calculatedOuterHeight { get; set; }

	// Scrolling
	private float _scrollX;
	private float _scrollY;

	private int _maxScrollWidth;
	private int _maxScrollHeight;

	private float _scrollTargetX;
	private float _scrollTargetY;

	#endregion

	#region Private switch getters and setters (these are basically just the properties, but with direction context for simplicity)

	private float? CalculatedPosition(UIDirection d, UISide s) => d switch
	{
		UIDirection.Horizontal => s == UISide.Start ? _calculatedLeft : _calculatedRight,
		UIDirection.Vertical => s == UISide.Start ? _calculatedTop : _calculatedBottom,
	};
	private float? SpecifiedSize(UIDirection d) => d switch
	{
		UIDirection.Horizontal => _specifiedWidth,
		UIDirection.Vertical => _specifiedHeight,
	};
	private float? SpecifiedMaxSize(UIDirection d) => d switch
	{
		UIDirection.Horizontal => _specifiedMaxWidth,
		UIDirection.Vertical => _specifiedMaxHeight,
	};
	private float CalculatedOuterSize(UIDirection d) => d switch
	{
		UIDirection.Horizontal => _calculatedOuterWidth,
		UIDirection.Vertical => _calculatedOuterHeight,
	};
	private float CalculatedMainSize(FlexDirection flexDirection) => flexDirection switch
	{
		FlexDirection.Column => _calculatedOuterHeight,
		FlexDirection.Row => _calculatedOuterWidth,
	};
	private float CalculatedCrossSize(FlexDirection flexDirection) => flexDirection switch
	{
		FlexDirection.Column => _calculatedOuterWidth,
		FlexDirection.Row => _calculatedOuterHeight,
	};

	private void SetMaxScrollSize(UIDirection d, int value)
	{
		switch (d)
		{
			case UIDirection.Horizontal: _maxScrollWidth = value; return;
			case UIDirection.Vertical: _maxScrollHeight = value; return;
		}
	}

	// Scroll set helpers
	private float ScrollClamp(float value, UIDirection d) => d switch
	{
		UIDirection.Horizontal => CurrentLayout.FlexDirection == FlexDirection.Row
					? Math.Clamp(value, -_maxScrollWidth, 0)
					: ScrollClamp(value, _maxScrollWidth),
		UIDirection.Vertical => CurrentLayout.FlexDirection == FlexDirection.Column
			? Math.Clamp(value, -_maxScrollHeight, 0)
			: ScrollClamp(value, _maxScrollHeight),
	};
	private float ScrollClamp(float value, int max) => CurrentLayout.AlignItems switch
	{
		AlignItems.FlexStart => Math.Clamp(value, -max, 0),
		AlignItems.FlexEnd => Math.Clamp(value, 0, max),
		AlignItems.Center => Math.Clamp(value, -max / 2, max / 2),
	};

	#endregion

	#region Public/Protected properties (grouped with private backing fields)

	// Structure
	internal List<GuiElement> _children;
	public IReadOnlyList<GuiElement> Children => _children.AsReadOnly();
	public GuiElement? ParentElement { get; private set; }
	public RootGuiElement Root => ParentElement?.Root ?? (RootGuiElement)this;

	// Focus
	private int _focusableElementsInTree = 0;
	public bool HasFocusableElementsInTree => _focusableElementsInTree > 0;

	private bool _focusable = false;
	public bool Focusable
	{
		get => _focusable;
		set
		{
			if (value == _focusable)
				return;

			if (value)
				IncrementFocusableDescendants();
			else
				DecrementFocusableDescendants();

			_focusable = value;
		}
	}
	public bool OverrideFocusChange { get; set; } = false;

	// State
	private string _state = ElementStates.Neutral;
	public string State
	{
		get => _state;
		set
		{
			if (_state != value)
			{
				_oldLayout = _currentLayout;
				_state = value;
				OnStateChange();
			}
		}
	}

	// Mouse state
	protected Point TransformedMouse { get; private set; }
	public bool MouseIsContained { get; private set; }
	public bool DidMouseMove { get; private set; }

	// Layout
	public Layout Layout { get; set; }
	private Layout _oldLayout;
	private Layout _currentLayout;
	public Layout CurrentLayout => _currentLayout;

	// Computed size and position
	public Vector2 Position { get; private set; }
	public Vector2 Size { get; private set; }
	public Rectangle OuterRectangle { get; private set; }
	public Rectangle BoundingRectangle { get; private set; }
	public Rectangle InnerRectangle { get; private set; }

	public float? MaxWidth => _specifiedMaxWidth;

	// Text size (if width is not set use text size)
	private Vector2? _textSize;
	protected Vector2? TextSize
	{
		get
		{
			if (_textSize == null)
				return null;
			return new Vector2(_textSize.Value.X * CurrentLayout.FontScale, _textSize.Value.Y * CurrentLayout.FontScale);
		}
		set => _textSize = value;
	}
	private float? GetTextSize(UIDirection d) => d == UIDirection.Horizontal ? TextSize?.X : TextSize?.Y;

	// Scrolling
	public float ScrollX
	{
		get => _scrollX;
		set
		{
			if (CurrentLayout.OverflowX == Overflow.Scroll)
			{
				_scrollX = ScrollClamp(value, UIDirection.Horizontal);
				_scrollTargetX = _scrollX;
			}
		}
	}
	public float ScrollY
	{
		get => _scrollY;
		set
		{
			if (CurrentLayout.OverflowY == Overflow.Scroll)
			{
				_scrollY = ScrollClamp(value, UIDirection.Vertical);
				_scrollTargetY = _scrollY;
			}
		}
	}

	public float ScrollTargetX
	{
		get => _scrollTargetX;
		set => _scrollTargetX = ScrollClamp(value, UIDirection.Horizontal);
	}
	public float ScrollTargetY
	{
		get => _scrollTargetY;
		set => _scrollTargetY = ScrollClamp(value, UIDirection.Vertical);
	}

	#endregion


	// Element creation
	public GuiElement(Layout? layout = null)
	{
		_children = new List<GuiElement>();
		Layout = layout ?? new Layout();
		_currentLayout = Layout;
		_oldLayout = Layout;

		_layoutTransitioner = new LayoutTransitioner(Layout);
	}

	// Adding and removing
	public GuiElement AddChild(GuiElement child)
	{
		if (child is RootGuiElement)
			throw new InvalidOperationException($"Cannot add a {nameof(RootGuiElement)} as a child.");

		_children.Add(child);
		child.ParentElement = this;

		if (child.HasFocusableElementsInTree)
			this.IncrementFocusableDescendants(child._focusableElementsInTree);

		return this;
	}
	public void RemoveChild(GuiElement child)
	{
		child.Dispose();
		_children.Remove(child);

		if (child.HasFocusableElementsInTree)
			this.DecrementFocusableDescendants(child._focusableElementsInTree);
	}
	public void Dispose()
	{
		// Remove all children
		for (int i = _children.Count - 1; i >= 0; i--)
			RemoveChild(_children[i]);

		OnDispose();
	}

	// Game Loop
	internal void PropogateUpdate(GameTime gameTime, Point transformedMouse, bool mouseVisibleToThis, bool didMouseMove)
	{
		// Transition layouts
		_currentLayout = _layoutTransitioner.Update(gameTime, State, _oldLayout);

		// Handle Transforms
		if (CurrentLayout.Transform != Matrix.Identity)
		{
			// _currentTransform = _currentLayout.Transform;
			_currentTransform
				= Matrix.CreateTranslation(new Vector3(-(new Vector2(
					Position.X + Size.X / 2f,
					Position.Y + Size.Y / 2f
				)), 0f))
				* _currentLayout.Transform
				* Matrix.CreateTranslation(new Vector3((new Vector2(
					Position.X + Size.X / 2f,
					Position.Y + Size.Y / 2f
				)), 0f))
			;
		}
		else
			_currentTransform = null;

		// Handle mouse and scrolling
		if (_currentTransform.HasValue)
		{
			// transformedMouse = BoundingRectangle
			transformedMouse = Vector2.Transform(
				transformedMouse.ToVector2(),
				Matrix.Invert(_currentTransform.Value)
			).ToPoint();
		}
		TransformedMouse = transformedMouse;

		// Smooth scroll behavior
		if ((CurrentLayout.OverflowX == Overflow.Scroll || CurrentLayout.OverflowY == Overflow.Scroll)
			&& (ScrollX != ScrollTargetX || ScrollY != ScrollTargetY))
		{
			// Smoothly approach scroll target
			_scrollX = MathHelper.Lerp(_scrollX, _scrollTargetX, (float)(SmoothScrollLerpSpeed * gameTime.ElapsedGameTime.TotalSeconds));
			_scrollY = MathHelper.Lerp(_scrollY, _scrollTargetY, (float)(SmoothScrollLerpSpeed * gameTime.ElapsedGameTime.TotalSeconds));

			// Snap to end of scroll if close
			if (Math.Abs(_scrollTargetX - _scrollX) < 1)
				_scrollX = _scrollTargetX;
			if (Math.Abs(_scrollTargetY - _scrollY) < 1)
				_scrollY = _scrollTargetY;

			UpdateChildRectangles();
		}

		// Determine if mouse is visible to children
		if (CurrentLayout.OverflowX != Overflow.Visible || CurrentLayout.OverflowY != Overflow.Visible)
		{
			bool xContained = transformedMouse.X >= BoundingRectangle.X && transformedMouse.X <= BoundingRectangle.X + BoundingRectangle.Width;
			bool yContained = transformedMouse.Y >= BoundingRectangle.Y && transformedMouse.Y <= BoundingRectangle.Y + BoundingRectangle.Height;

			if (CurrentLayout.OverflowX != Overflow.Visible && !xContained)
				mouseVisibleToThis = false;
			if (CurrentLayout.OverflowY != Overflow.Visible && !yContained)
				mouseVisibleToThis = false;
		}

		MouseIsContained = mouseVisibleToThis && BoundingRectangle.Contains(transformedMouse);
		DidMouseMove = didMouseMove;

		OnUpdate(gameTime);

		if (Focusable && DidMouseMove)
		{
			var root = Root;
			if (MouseIsContained && State == ElementStates.Neutral)
			{
				State = ElementStates.Hovered;
				root.FocusedElement = this;
			}

			if (!MouseIsContained && State == ElementStates.Hovered)
			{
				State = ElementStates.Neutral;
				if (root.FocusedElement == this)
					root.FocusedElement = null;
			}
		}

		_children.ForEach(c => c.PropogateUpdate(gameTime, transformedMouse, mouseVisibleToThis, didMouseMove));
	}

	// Events
	public void MouseEvent(byte mouseButton, ButtonState state)
	{
		_children.ForEach(c => c.MouseEvent(mouseButton, state));
		OnMouseEvent(mouseButton, state);
	}
	public void ScrollEvent(UIDirection direction, int distance)
	{
		ScrollElementTree(direction, distance);
	}
	private bool ScrollElementTree(UIDirection direction, int distance)
	{
		// Scroll children
		foreach (var child in _children)
		{
			if (child.ScrollElementTree(direction, distance))
				return true;
		}

		// Alter direction if shift is held
		if (HoldShiftToScrollHorizontally && direction == UIDirection.Vertical
			&& (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift)))
		{
			direction = UIDirection.Horizontal;
			distance = -distance;
		}

		// Do Scrolling
		if (CurrentLayout.AllowScrollingFromAnywhere || MouseIsContained)
			return DoScrolling(direction, (int)(distance * ScrollScale));

		return false;
	}
	protected bool DoScrolling(UIDirection direction, int distance)
	{
		var prevScrollTargetX = ScrollTargetX;
		var prevScrollTargetY = ScrollTargetY;
		var consumed = true;
		if (direction == UIDirection.Vertical && CurrentLayout.OverflowY == Overflow.Scroll)
			ScrollTargetY += distance;
		else if (direction == UIDirection.Horizontal && CurrentLayout.OverflowX == Overflow.Scroll)
			ScrollTargetX -= distance;
		else
			consumed = false;

		// Did we actually scroll?
		if (ScrollTargetX == prevScrollTargetX && ScrollTargetY == prevScrollTargetY)
			consumed = false;

		OnScrollEvent(direction, distance);

		return consumed;
	}

	public void ActivatePress() => OnActivatePress();
	public void ActivateRelease() => OnActivateRelease();

	// Element tree functions
	public void ToAll(Action<GuiElement> action)
	{
		action(this);
		foreach (var child in _children)
			child.ToAll(action);
	}
	public int CountIf(Func<GuiElement, bool> predicate)
	{
		int count = predicate(this) ? 1 : 0;

		foreach (var child in _children)
			count += child.CountIf(predicate);

		return count;
	}
	private GuiElement? NearestAncestorWhere(Func<GuiElement, bool> predicate)
	{
		if (ParentElement == null)
			return null;

		if (predicate(ParentElement))
			return ParentElement;

		return ParentElement.NearestAncestorWhere(predicate);
	}

	// Other
	public void ResetTransitions(string state = ElementStates.Neutral)
	{
		State = state;
		_oldLayout = Layout.GetLayoutFromState(state);
		_layoutTransitioner.Reset(state);
	}

	#region Focus

	private void IncrementFocusableDescendants(int count = 1)
	{
		_focusableElementsInTree += count;
		ParentElement?.IncrementFocusableDescendants(count);
	}
	private void DecrementFocusableDescendants(int count = 1)
	{
		_focusableElementsInTree -= count;
		ParentElement?.DecrementFocusableDescendants(count);
	}

	internal GuiElement? GetFirstFocusableElementInTree()
	{
		if (Focusable)
			return this;

		if (!HasFocusableElementsInTree)
			return null;

		foreach (var child in _children)
		{
			if (child.HasFocusableElementsInTree)
				return child.GetFirstFocusableElementInTree();
		}

		return null;
	}

	internal GuiElement? GetNextFocusableElement(UIDirection d, UISide s, Vector2 posOfFocusedElement)
	{
		return GetNextFocusableSibling(d, s, posOfFocusedElement) ?? ParentElement?.GetNextFocusableElement(d, s, posOfFocusedElement);
	}
	internal GuiElement? GetNextFocusableSibling(UIDirection d, UISide s, Vector2 posOfFocusedElement)
	{
		if (ParentElement == null)
			return null;

		if (!ParentElement.CurrentLayout.FlexDirection.IsParallelWith(d))
			return null;

		// TODO: This is getting next sibling and seeing if they are focusable. Should loop until next focusable sibling is found
		// Is there a sibling in that direction
		int myIndex = ParentElement._children.IndexOf(this);
		if ((s == UISide.Start && myIndex - 1 < 0) || (s == UISide.End && myIndex + 1 >= ParentElement._children.Count))
			return null;

		// Get next sibling
		GuiElement sibling;
		sibling = ParentElement._children[(s == UISide.Start ? myIndex - 1 : myIndex + 1)];

		if (sibling._focusable)
			return sibling;

		if (sibling._focusableElementsInTree > 0)
		{
			// focus direction is parallel
			// if (d == UIDirection.Vertical && sibling.CurrentLayout.FlexDirection == FlexDirection.Column)
			if (sibling.CurrentLayout.FlexDirection.IsParallelWith(d))
				return sibling.GetFurthestFocusableElememt(s.Invert());
			else
				return sibling.GetClosestFocusableElememtTo(posOfFocusedElement);
		}

		return null;
	}
	// "GetSouthernmostFocusableElement": If direction is Side.End, returns the "southernmost" element that is focusable
	internal GuiElement? GetFurthestFocusableElememt(UISide s)
	{
		if (_focusable)
			return this;

		if (s == UISide.Start)
		{
			foreach (var child in _children)
			{
				if (child.HasFocusableElementsInTree)
					return child.GetFurthestFocusableElememt(s);
			}
		}
		else
		{
			for (int i = _children.Count - 1; i >= 0; i--)
			{
				if (_children[i].HasFocusableElementsInTree)
					return _children[i].GetFurthestFocusableElememt(s);
			}
		}

		return null;
	}
	internal GuiElement? GetClosestFocusableElememtTo(Vector2 pos)
	{
		if (_focusable)
			return this;

		var fd = CurrentLayout.FlexDirection;

		// Loop through each child, find child with min x distance
		GuiElement closestChild = null!;
		float min = float.MaxValue;
		foreach (var child in _children)
		{
			float distance = Math.Abs(child.Position.RelaventCoordinate(fd) - pos.RelaventCoordinate(fd));
			if (child._focusableElementsInTree > 0 && distance < min)
			{
				closestChild = child;
				min = distance;
			}
		}
		return closestChild.GetClosestFocusableElememtTo(pos);
	}

	internal void ScrollElementIntoView()
	{
		// Find nearest parent who can scroll, make them scroll me into view. Then for that parent, repeat the process, make sure that parent is scrolled into view

		// Element? parentWithScrolling = NearestAncestorWhere(e => e.CurrentLayout.OverflowX == Overflow.Scroll || e.CurrentLayout.OverflowY == Overflow.Scroll);

		// Get the immediate child to the parentWithScrolling, make the 'parentWithScrolling' scroll the child into view
		(var parent, var child) = this.GetNearestParentWithScrollingAndRelaventChild(this);
		if (parent == null)
			return;

		ScrollIntoView(parent, child, this);

		parent?.ScrollElementIntoView();
	}
	(GuiElement?, GuiElement) GetNearestParentWithScrollingAndRelaventChild(GuiElement relaventChild)
	{
		if (ParentElement == null)
			return (null, this);

		if (ParentElement.CurrentLayout.OverflowX == Overflow.Scroll || ParentElement.CurrentLayout.OverflowY == Overflow.Scroll)
			return (ParentElement, this);
		else
			return ParentElement.GetNearestParentWithScrollingAndRelaventChild(this);
	}
	internal static void ScrollIntoView(GuiElement parent, GuiElement child, GuiElement toBeSeen)
	{
		var parentBox = parent.InnerRectangle;
		var toBeSeenBox = toBeSeen.BoundingRectangle;
		int scrollPadding = (int)parent.CurrentLayout.ScrollPadding;

		if (parent.CurrentLayout.OverflowY == Overflow.Scroll)
		{
			var differenceTop = parentBox.Top - toBeSeenBox.Top;
			var differenceBottom = toBeSeenBox.Bottom - parentBox.Bottom;

			// If toBeSeen is above parent, scroll it down
			if (differenceTop > 0)
				parent.DoScrolling(UIDirection.Vertical, (differenceTop + scrollPadding));
			// Else, scroll it up
			else if (differenceBottom > 0)
				parent.DoScrolling(UIDirection.Vertical, -(differenceBottom + scrollPadding));

		}
		if (parent.CurrentLayout.OverflowX == Overflow.Scroll)
		{
			var differenceLeft = parentBox.Left - toBeSeenBox.Left;
			var differenceRight = toBeSeenBox.Right - parentBox.Right;

			// If toBeSeen is above parent, scroll it down
			if (differenceLeft > 0)
				parent.DoScrolling(UIDirection.Horizontal, -(differenceLeft + scrollPadding));
			// Else, scroll it up
			else if (differenceRight > 0)
				parent.DoScrolling(UIDirection.Horizontal, (differenceRight + scrollPadding));
		}
	}

	#endregion

	#region Resolve position and size of element tree

	internal void ComputeDimensions()
	{
		_calculatedLeft = CalcPosition(UIDirection.Horizontal, UISide.Start);
		_calculatedRight = CalcPosition(UIDirection.Horizontal, UISide.End);
		_calculatedTop = CalcPosition(UIDirection.Vertical, UISide.Start);
		_calculatedBottom = CalcPosition(UIDirection.Vertical, UISide.End);

		_specifiedWidth = CalcSpecifiedSize(UIDirection.Horizontal);
		_specifiedHeight = CalcSpecifiedSize(UIDirection.Vertical);

		_specifiedMaxWidth = CalcSpecifiedMaxSize(UIDirection.Horizontal);
		_specifiedMaxHeight = CalcSpecifiedMaxSize(UIDirection.Vertical);

		foreach (var child in _children)
			child.ComputeDimensions();

		var marginsX = CurrentLayout.GetMargin(UIDirection.Horizontal, UISide.Start) + CurrentLayout.GetMargin(UIDirection.Horizontal, UISide.End);
		var marginsY = CurrentLayout.GetMargin(UIDirection.Vertical, UISide.Start) + CurrentLayout.GetMargin(UIDirection.Vertical, UISide.End);

		var width = CalcSize(UIDirection.Horizontal);
		var height = CalcSize(UIDirection.Vertical);

		Size = new Vector2(width, height);

		_calculatedOuterWidth = width + marginsX;
		_calculatedOuterHeight = height + marginsY;
	}
	private float? CalcPosition(UIDirection d, UISide s)
	{
		var position = CurrentLayout.GetPosition(d, s);
		if (position == null)
			return null;
		switch (CurrentLayout.GetPositionUnit(d, s))
		{
			case LayoutUnit.Pixels: return position;
			case LayoutUnit.Percent:
				if (ParentElement == null)
					return (ScreenSize(d) * position) / 100.0f;
				var size = ParentElement.SpecifiedActualSize(d);
				if (size == null)
					return 0;
				return (size * position) / 100.0f;
			default:
				return 0;
		}
	}
	private float? CalcSpecifiedSize(UIDirection d)
	{
		var myMargins = CurrentLayout.GetMargin(d, UISide.Start) + CurrentLayout.GetMargin(d, UISide.End);
		var myPadding = CurrentLayout.GetPadding(d, UISide.Start) + CurrentLayout.GetPadding(d, UISide.End);

		var size = CurrentLayout.GetSize(d) ?? GetTextSize(d) + myPadding;
		var sizeUnit = CurrentLayout.GetSizeUnit(d);
		var start = CalculatedPosition(d, UISide.Start);
		var end = CalculatedPosition(d, UISide.End);
		var screenSize = ScreenSize(d);
		var parentSize = ParentElement?.SpecifiedActualSize(d);
		var parentPadding = ParentElement?.CurrentLayout.GetPadding(d, UISide.Start) + ParentElement?.CurrentLayout.GetPadding(d, UISide.End);

		// If width is null, try calculating by left and right
		if (size == null && start.HasValue && end.HasValue)
		{
			switch (CurrentLayout.PositionMode)
			{
				case PositionMode.Static:
					return null;
				case PositionMode.Relative:
					return null;
				case PositionMode.RelativeToParent:
					if (ParentElement == null)
						return screenSize - start - end - myMargins;
					if (parentSize.HasValue)
						return parentSize - start - end - myMargins - parentPadding;
					return null;
				case PositionMode.Fixed:
					return screenSize - start - end - myMargins;
			}
		}
		// Otherwise, just go by width (and calculate percentage if neccessary)
		switch (sizeUnit)
		{
			case LayoutUnit.Pixels:
				return size;
			case LayoutUnit.Percent:
				if (size == null)
					return null;

				if (ParentElement == null)
					return ((screenSize - myMargins) * size) / 100.0f;

				if (parentSize == null)
					return null;

				return ((parentSize - myMargins - parentPadding!) * size) / 100.0f;
			default:
				return 0;
		}
	}
	private float? CalcSpecifiedMaxSize(UIDirection d)
	{
		var maxSize = CurrentLayout.GetMaxSize(d);
		var maxSizeUnit = CurrentLayout.GetMaxSizeUnit(d);
		var screenSize = ScreenSize(d);
		var parentSpecifiedSize = ParentElement?.SpecifiedSize(d);
		if (parentSpecifiedSize.HasValue)
			parentSpecifiedSize = Math.Min(parentSpecifiedSize.Value, ParentElement?.SpecifiedMaxSize(d) ?? parentSpecifiedSize.Value);
		var myMargins = CurrentLayout.GetMargin(d, UISide.Start) + CurrentLayout.GetMargin(d, UISide.End);
		var myPadding = CurrentLayout.GetPadding(d, UISide.Start) + CurrentLayout.GetPadding(d, UISide.End);
		var parentPadding = ParentElement?.CurrentLayout.GetPadding(d, UISide.Start) + ParentElement?.CurrentLayout.GetPadding(d, UISide.End);

		switch (maxSizeUnit)
		{
			case LayoutUnit.Pixels:
				return maxSize;
			case LayoutUnit.Percent:
				if (maxSize == null)
					return null;

				if (ParentElement == null)
					return (screenSize * maxSize) / 100.0f;

				if (parentSpecifiedSize == null)
					return null;

				return ((parentSpecifiedSize - parentPadding!) * maxSize) / 100.0f;
			default:
				return 0;
		}
	}
	private float? SpecifiedActualSize(UIDirection d)
	{
		float? specifiedSize = SpecifiedSize(d);
		float? specifiedMaxSize = SpecifiedMaxSize(d);

		if (specifiedSize == null)
			return null;

		if (specifiedMaxSize == null)
			return specifiedSize;

		return Math.Min(specifiedSize.Value, specifiedMaxSize.Value);
	}
	private float CalcSize(UIDirection d)
	{
		var myPadding = CurrentLayout.GetPadding(d, UISide.Start) + CurrentLayout.GetPadding(d, UISide.End);

		var sizeOfChildren = computeSizeOfChildren();
		var computedSize = compute();
		var specifiedMaxSize = SpecifiedMaxSize(d);

		var finalSize = Math.Min(computedSize, specifiedMaxSize ?? computedSize);

		SetMaxScrollSize(d, (int)(sizeOfChildren - (finalSize - myPadding)));
		return finalSize;

		float compute()
		{
			var specifiedSize = SpecifiedActualSize(d);

			if (specifiedSize.HasValue)
				return specifiedSize.Value;

			return myPadding + sizeOfChildren;
		}
		float computeSizeOfChildren()
		{
			float sum = 0;
			if (_children.Count == 0)
				return sum;

			if (CurrentLayout.FlexDirection.IsParallelWith(d))
			{
				_children.ForEach(c =>
					sum += c.Layout.PositionMode.NormalFlow() ? c.CalculatedOuterSize(d) + CurrentLayout.Gap : 0
				);
				sum -= CurrentLayout.Gap;
			}
			else
			{
				float maxChildSize = 0;
				_children.ForEach(c =>
					maxChildSize = c.Layout.PositionMode.NormalFlow() ? MathHelper.Max(maxChildSize, c.CalculatedOuterSize(d)) : maxChildSize
				);
				sum += maxChildSize;
			}

			return sum;
		}
	}

	protected void UpdateRectangles() => UpdateRectangles(0, 0, FlexDirection.Row);
	private void UpdateRectangles(float main, float cross, FlexDirection parentFlexDirection)
	{
		float x = 0, y = 0;
		if (parentFlexDirection == FlexDirection.Row)
		{
			x = main;
			y = cross;
		}
		else
		{
			y = main;
			x = cross;
		}
		switch (CurrentLayout.PositionMode)
		{
			case PositionMode.Static:
				break;
			case PositionMode.Relative:
				x = x + (_calculatedLeft ?? 0) - (_calculatedRight ?? 0);
				y = y + (_calculatedTop ?? 0) - (_calculatedBottom ?? 0);
				break;
			case PositionMode.RelativeToParent:
				if (ParentElement == null)
					SetPosition(0, 0, ScreenWidth, ScreenHeight);
				else
					SetPosition(ParentElement.InnerRectangle.X, ParentElement.InnerRectangle.Y, ParentElement.InnerRectangle.Width, ParentElement.InnerRectangle.Height);
				break;
			case PositionMode.Fixed:
				SetPosition(0, 0, ScreenWidth, ScreenHeight);
				break;
		}

		void SetPosition(float xStart, float yStart, float width, float height)
		{
			if (_calculatedLeft.HasValue)
				x = xStart + _calculatedLeft.Value;
			else if (_calculatedRight.HasValue)
				x = xStart + width - _calculatedRight.Value - _calculatedOuterWidth;
			else
				x = xStart;

			if (_calculatedTop.HasValue)
				y = yStart + _calculatedTop.Value;
			else if (_calculatedBottom.HasValue)
				y = yStart + height - _calculatedBottom.Value - _calculatedOuterHeight;
			else
				y = yStart;
		}

		Position = new Vector2(x + CurrentLayout.MarginLeft, y + CurrentLayout.MarginTop);
		OuterRectangle = new Rectangle((int)(x + 0.5f), (int)(y + 0.5f), (int)(_calculatedOuterWidth + 0.5f), (int)(_calculatedOuterHeight + 0.5f));
		BoundingRectangle = new Rectangle(
			(int)(OuterRectangle.X + CurrentLayout.MarginLeft),
			(int)(OuterRectangle.Y + CurrentLayout.MarginTop),
			(int)(OuterRectangle.Width - CurrentLayout.MarginLeft - CurrentLayout.MarginRight),
			(int)(OuterRectangle.Height - CurrentLayout.MarginTop - CurrentLayout.MarginBottom)
		);
		InnerRectangle = new Rectangle(
			(int)(BoundingRectangle.X + CurrentLayout.PaddingLeft),
			(int)(BoundingRectangle.Y + CurrentLayout.PaddingTop),
			(int)(BoundingRectangle.Width - CurrentLayout.PaddingLeft - CurrentLayout.PaddingRight),
			(int)(BoundingRectangle.Height - CurrentLayout.PaddingTop - CurrentLayout.PaddingBottom)
		);

		AfterRectangleCompute();

		// Position children
		UpdateChildRectangles();
	}
	protected void UpdateChildRectangles()
	{
		if (_children.Count == 0) return;

		float x = Position.X + CurrentLayout.PaddingLeft;
		float y = Position.Y + CurrentLayout.PaddingTop;
		float w = Size.X - CurrentLayout.PaddingLeft - CurrentLayout.PaddingRight;
		float h = Size.Y - CurrentLayout.PaddingTop - CurrentLayout.PaddingBottom;
		var psuedoInnerRec = new FloatRectangle(x + ScrollX, y + ScrollY, w, h);

		float main;
		float cross;
		float gap;
		(main, gap) = CalcMainAndGap(_children, CurrentLayout, psuedoInnerRec);
		foreach (var child in _children)
		{
			cross = CalcCrossAxis(CurrentLayout.FlexDirection, CurrentLayout.AlignItems, psuedoInnerRec, child);
			child.UpdateRectangles(main, cross, CurrentLayout.FlexDirection);
			main += child.CalculatedMainSize(CurrentLayout.FlexDirection) + gap;
		}
	}

	#endregion

	#region Hooks

	// OnDraw should be protected, but also accessable from the ElementRenderer class
	internal void NotifyDraw(SpriteBatch spriteBatch) => OnDraw(spriteBatch);

	protected virtual void OnUpdate(GameTime gameTime) { }
	protected virtual void OnDraw(SpriteBatch spriteBatch) { }
	protected virtual void OnStateChange() { }
	protected virtual void OnMouseEvent(byte mouseButton, ButtonState buttonState) { }
	protected virtual void OnActivatePress() { }
	protected virtual void OnActivateRelease() { }
	protected virtual void OnScrollEvent(UIDirection direction, int distance) { }
	protected virtual void AfterRectangleCompute() { }
	protected virtual void OnDispose() { }

	public virtual bool ConsumeFocus(UIDirection d, UISide s) => false;

	#endregion
}