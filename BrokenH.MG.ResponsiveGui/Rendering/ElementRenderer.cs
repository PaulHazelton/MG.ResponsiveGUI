using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BrokenH.MG.ResponsiveGui.Styles;
using BrokenH.MG.ResponsiveGui.Elements;

namespace BrokenH.MG.ResponsiveGui.Rendering;

public class ElementRenderer
{
	// Statics
	public static bool DrawDebugBorders { get; set; } = false;
	public static Color DebugOuterColor { get; set; } = Color.Cyan;
	public static Color DebugBoundingColor { get; set; } = Color.Magenta;
	public static Color DebugInnerColor { get; set; } = Color.Yellow;

	// Private
	private int _screenWidth { get; set; }
	private int _screenHeight { get; set; }

	private GraphicsDevice _graphicsDevice { get; set; }
	private IUiPrimitiveDrawer _uiPrimitiveDrawer;


	// Constructor
	public ElementRenderer(GraphicsDevice graphicsDevice, IUiPrimitiveDrawer uiPrimitiveDrawer, int screenWidth, int screenHeight)
	{
		_graphicsDevice = graphicsDevice;
		_screenWidth = screenWidth;
		_screenHeight = screenHeight;
		_uiPrimitiveDrawer = uiPrimitiveDrawer;
		// RenderTargets = new List<RenderTarget2D>();
	}

	public void UpdateScreenSize(int screenWidth, int screenHeight)
	{
		_screenWidth = screenWidth;
		_screenHeight = screenHeight;

		// for (int i = 0; i < RenderTargets.Count; i++)
		// 	RenderTargets[i] = new RenderTarget2D(GraphicsDevice, ScreenWidth, ScreenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
	}

	public void Draw(SpriteBatch spriteBatch, SamplerState? samplerState, RootGuiElement rootElement) => Draw(spriteBatch, rootElement, Matrix.Identity, new Rectangle(0, 0, _screenWidth, _screenHeight));
	private void Draw(SpriteBatch spriteBatch, GuiElement e, Matrix previousTransform, Rectangle previousClipping)
	{
		// Simple draw
		if (!e.CurrentLayout.RequiresComplicatedDraw || _graphicsDevice == null)
		{
			DrawThisElement(spriteBatch, e);
			DrawChildren(spriteBatch, e, previousTransform, previousClipping);
			return;
		}

		// Stop previous spriteBatch call
		spriteBatch.End();

		// Store previous spritebatch states
		var previousRaster = _graphicsDevice.RasterizerState;
		// Accumulate transforms
		var nextTransform = previousTransform * (e._currentTransform ?? Matrix.Identity);
		// Transform clipping rectangle
		var currentClipping = TransformRectangle(GetClippingRectangle(e), nextTransform);
		// Accumulate clippings
		var nextClipping = Rectangle.Intersect(previousClipping, currentClipping);

		var rasterState = new RasterizerState()
		{
			ScissorTestEnable = true,
		};

		spriteBatch.Begin(
			sortMode: SpriteSortMode.Deferred,
			blendState: BlendState.NonPremultiplied,
			samplerState: SamplerState.PointClamp,
			rasterizerState: rasterState,
			transformMatrix: nextTransform
		);

		DrawThisElement(spriteBatch, e);
		spriteBatch.End();
		_graphicsDevice.ScissorRectangle = nextClipping;
		spriteBatch.Begin(
			sortMode: SpriteSortMode.Deferred,
			blendState: BlendState.NonPremultiplied,
			samplerState: SamplerState.PointClamp,
			rasterizerState: rasterState,
			transformMatrix: nextTransform
		);
		DrawChildren(spriteBatch, e, nextTransform, nextClipping);

		spriteBatch.End();

		// Return to previous state
		_graphicsDevice.ScissorRectangle = previousClipping;
		spriteBatch.Begin(
			sortMode: SpriteSortMode.Deferred,
			blendState: BlendState.NonPremultiplied,
			samplerState: SamplerState.PointClamp,
			rasterizerState: previousRaster,
			transformMatrix: previousTransform
		);
	}
	private void DrawThisElement(SpriteBatch spriteBatch, GuiElement e)
	{
		// Draw Background
		if (e.CurrentLayout.BackgroundColor != Color.Transparent)
			_uiPrimitiveDrawer.DrawRectangle(spriteBatch, e.BoundingRectangle, e.CurrentLayout.BackgroundColor);

		// Draw Image
		if (e.CurrentLayout.NineSlice != null)
			e.CurrentLayout.NineSlice.Draw(spriteBatch, e.BoundingRectangle, e.CurrentLayout.ImageColor);
		if (e.CurrentLayout.Image != null)
			spriteBatch.Draw(e.CurrentLayout.Image, e.BoundingRectangle, e.CurrentLayout.SourceRectangle, e.CurrentLayout.ImageColor);

		// Custom drawing
		e.NotifyDraw(spriteBatch);

		// Draw Borders
		DrawBorders();
		void DrawBorders()
		{
			var thicknessLeft = e.CurrentLayout.BorderThicknessLeft;
			var thicknessRight = e.CurrentLayout.BorderThicknessRight;
			var thicknessTop = e.CurrentLayout.BorderThicknessTop;
			var thicknessBottom = e.CurrentLayout.BorderThicknessBottom;

			var colorLeft = e.CurrentLayout.BorderColorLeft;
			var colorRight = e.CurrentLayout.BorderColorRight;
			var colorTop = e.CurrentLayout.BorderColorTop;
			var colorBottom = e.CurrentLayout.BorderColorBottom;

			// Adjust alpha for very thin lines
			if (thicknessLeft < 1)
			{
				colorLeft.A = (byte)MathHelper.Lerp(0, colorLeft.A, thicknessLeft);
				thicknessLeft = 1;
			}
			if (thicknessRight < 1)
			{
				colorRight.A = (byte)MathHelper.Lerp(0, colorRight.A, thicknessRight);
				thicknessRight = 1;
			}
			if (thicknessTop < 1)
			{
				colorTop.A = (byte)MathHelper.Lerp(0, colorTop.A, thicknessTop);
				thicknessTop = 1;
			}
			if (thicknessBottom < 1)
			{
				colorBottom.A = (byte)MathHelper.Lerp(0, colorBottom.A, thicknessBottom);
				thicknessBottom = 1;
			}

			var tl = e.BoundingRectangle.Location.ToVector2();
			var tr = tl + new Vector2(e.BoundingRectangle.Width, 0);
			var bl = tl + new Vector2(0, e.BoundingRectangle.Height);
			var br = tl + e.BoundingRectangle.Size.ToVector2();

			if (colorTop != Color.Transparent && thicknessTop > 0)
				_uiPrimitiveDrawer.DrawLine(spriteBatch, tl + new Vector2(0, (thicknessTop / 2f)), tr + new Vector2(0, (thicknessTop / 2f)), thicknessTop, colorTop);
			if (colorRight != Color.Transparent && thicknessRight > 0)
				_uiPrimitiveDrawer.DrawLine(spriteBatch, tr + new Vector2(-(thicknessRight / 2f), 0), br + new Vector2(-(thicknessRight / 2f), 0), thicknessRight, colorRight);
			if (colorBottom != Color.Transparent && thicknessBottom > 0)
				_uiPrimitiveDrawer.DrawLine(spriteBatch, br + new Vector2(0, -(thicknessBottom / 2f)), bl + new Vector2(0, -(thicknessBottom / 2f)), thicknessBottom, colorBottom);
			if (colorLeft != Color.Transparent && thicknessLeft > 0)
				_uiPrimitiveDrawer.DrawLine(spriteBatch, bl + new Vector2((thicknessLeft / 2f), 0), tl + new Vector2((thicknessLeft / 2f), 0), thicknessLeft, colorLeft);
		}

		// Debug drawing
		if (DrawDebugBorders)
		{
			_uiPrimitiveDrawer.DrawRectangleOutline(spriteBatch, e.OuterRectangle, DebugOuterColor, 1, 0.5f);
			_uiPrimitiveDrawer.DrawRectangleOutline(spriteBatch, e.BoundingRectangle, DebugBoundingColor, 1, 0.5f);
			_uiPrimitiveDrawer.DrawRectangleOutline(spriteBatch, e.InnerRectangle, DebugInnerColor, 1, 0.5f);
		}
	}
	private void DrawChildren(SpriteBatch spriteBatch, GuiElement e, Matrix previousTransform, Rectangle previousClipping)
	{
		// Draw children
		foreach (var child in e._children)
			Draw(spriteBatch, child, previousTransform, previousClipping);
	}

	// private List<RenderTarget2D> RenderTargets;
	// internal void AddRenderTarget() => RenderTargets.Add(new RenderTarget2D(GraphicsDevice, ScreenWidth, ScreenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents));
	[System.Obsolete]
	private Texture2D DrawUsingRenderTargets(SpriteBatch spriteBatch, SamplerState? samplerState)
	{
		/*
		int rtCount = _rootElement.CountIf(e => e.Layout.RequiresComplicatedDrawEver) + 1;
		int currCount = RenderTargets.Count;
		if (rtCount > currCount)
		{
			for (int i = 0; i <= rtCount - currCount; i++)
				AddRenderTarget();
		}

		var previousRTs = GraphicsDevice.GetRenderTargets();
		GraphicsDevice.SetRenderTarget(RenderTargets[0]);
		GraphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin(
			sortMode: SpriteSortMode.Deferred,
			blendState: BlendState.NonPremultiplied,
			samplerState: samplerState,
			transformMatrix: null
		);
		DrawWithRenderTargetsHelper(spriteBatch, _rootElement, 1, samplerState);
		spriteBatch.End();
		GraphicsDevice.SetRenderTargets(previousRTs);

		return RenderTargets[0];
		*/
		return new Texture2D(null, 0, 0);
	}
	[System.Obsolete]
	private void DrawWithRenderTargetsHelper(SpriteBatch spriteBatch, GuiElement e, int rtCount, SamplerState? samplerState)
	{
		/*
		// Simple draw
		if (!e.CurrentLayout.RequiresComplicatedDraw)
		{
			DrawThisElement(spriteBatch, e);
			DrawChildren(spriteBatch, e, rtCount, samplerState);
			return;
		}

		// Pause previous spriteBatch
		spriteBatch.End();

		var previousRenderTargets = GraphicsDevice.GetRenderTargets();
		GraphicsDevice.SetRenderTarget(RenderTargets[rtCount]);
		GraphicsDevice.Clear(Color.Transparent);
		rtCount++;
		spriteBatch.Begin(
			sortMode: SpriteSortMode.Deferred,
			blendState: BlendState.NonPremultiplied,
			samplerState: samplerState,
			transformMatrix: null
		);
		DrawChildren(spriteBatch, e, rtCount, samplerState);
		spriteBatch.End();
		GraphicsDevice.SetRenderTargets(previousRenderTargets);
		rtCount--;


		spriteBatch.Begin(
			sortMode: SpriteSortMode.Deferred,
			blendState: BlendState.AlphaBlend,
			samplerState: samplerState,
			transformMatrix: e._currentTransform
		);
		DrawThisElement(spriteBatch, e);
		var clippingRectangle = GetClippingRectangle(e);
		spriteBatch.Draw(RenderTargets[rtCount], clippingRectangle, clippingRectangle, Color.White);
		spriteBatch.End();


		// Return spriteBatch to previous state
		spriteBatch.Begin(
			sortMode: SpriteSortMode.Deferred,
			blendState: BlendState.NonPremultiplied,
			samplerState: samplerState,
			transformMatrix: null
		);

		void DrawChildren(SpriteBatch spriteBatch, GuiElement e, int rtCount, SamplerState? samplerState)
		{
			// Draw children
			foreach (var child in e._children)
				DrawWithRenderTargetsHelper(spriteBatch, child, rtCount, samplerState);
		}
		*/
	}

	// Helper functions
	private Rectangle GetClippingRectangle(GuiElement e)
	{
		int x = 0, y = 0, w = _screenWidth, h = _screenHeight;
		if (e.CurrentLayout.OverflowX != Overflow.Visible)
		{
			x = e.InnerRectangle.X;
			w = e.InnerRectangle.Width;
		}
		if (e.CurrentLayout.OverflowY != Overflow.Visible)
		{
			y = e.InnerRectangle.Y;
			h = e.InnerRectangle.Height;
		}
		return new Rectangle(x, y, w, h);
	}

	private Rectangle TransformRectangle(Rectangle r, Matrix m)
	{
		var p1 = r.Location.ToVector2();
		var p2 = new Vector2(r.Right, r.Bottom);

		p1 = Vector2.Transform(p1, m);
		p2 = Vector2.Transform(p2, m);

		return new Rectangle(
			(int)(p1.X),
			(int)(p1.Y),
			(int)(p2.X - p1.X),
			(int)(p2.Y - p1.Y)
		);
	}
}