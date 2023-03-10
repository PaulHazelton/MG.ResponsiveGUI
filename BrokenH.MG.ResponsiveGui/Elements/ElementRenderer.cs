using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BrokenH.MG.ResponsiveGui.Styles;

namespace BrokenH.MG.ResponsiveGui.Elements
{
	public class ElementRenderer
	{
		// Statics
		public static bool DrawDebugBorders { get; set; } = false;
		public static Color DebugOuterColor { get; set; } = Color.Cyan;
		public static Color DebugBoundingColor { get; set; } = Color.Magenta;
		public static Color DebugInnerColor { get; set; } = Color.Yellow;

		// Private
		private RootGuiElement _rootElement;
		private List<RenderTarget2D> RenderTargets;

		private int ScreenWidth { get; set; }
		private int ScreenHeight { get; set; }

		private GraphicsDevice GraphicsDevice { get; set; }

		private UIPrimativeDrawer _uIPrimativeDrawer;

		// Internal
		internal void AddRenderTarget() => RenderTargets.Add(new RenderTarget2D(GraphicsDevice, ScreenWidth, ScreenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents));

		public void UpdateScreenSize(int screenWidth, int screenHeight)
		{
			ScreenWidth = screenWidth;
			ScreenHeight = screenHeight;

			for (int i = 0; i < RenderTargets.Count; i++)
				RenderTargets[i] = new RenderTarget2D(GraphicsDevice, ScreenWidth, ScreenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}


		public ElementRenderer(GraphicsDevice graphicsDevice, int screenWidth, int screenHeight, RootGuiElement rootElement)
		{
			GraphicsDevice = graphicsDevice;
			ScreenWidth = screenWidth;
			ScreenHeight = screenHeight;
			_rootElement = rootElement;

			_uIPrimativeDrawer = new UIPrimativeDrawer(graphicsDevice);
			RenderTargets = new List<RenderTarget2D>();
		}

		public Texture2D Draw(SpriteBatch spriteBatch, SamplerState? samplerState)
		{
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
			Draw2(spriteBatch, _rootElement, 1, samplerState);
			spriteBatch.End();
			GraphicsDevice.SetRenderTargets(previousRTs);

			return RenderTargets[0];
		}
		private void Draw2(SpriteBatch spriteBatch, GuiElement e, int rtCount, SamplerState? samplerState)
		{
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
		}

		private void DrawThisElement(SpriteBatch spriteBatch, GuiElement e)
		{
			// Draw Background
			if (e.CurrentLayout.BackgroundColor != Color.Transparent)
				_uIPrimativeDrawer.DrawRectangle(spriteBatch, e.BoundingRectangle, e.CurrentLayout.BackgroundColor);

			// Draw Image
			if (e.CurrentLayout.NineSlice != null)
				e.CurrentLayout.NineSlice.Draw(spriteBatch, e.BoundingRectangle, e.CurrentLayout.ImageColor);
			if (e.CurrentLayout.Image != null)
				spriteBatch.Draw(e.CurrentLayout.Image, e.BoundingRectangle, null, e.CurrentLayout.ImageColor);

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
					_uIPrimativeDrawer.DrawLine(spriteBatch, tl + new Vector2(0, (thicknessTop / 2f)), tr + new Vector2(0, (thicknessTop / 2f)), thicknessTop, colorTop);
				if (colorRight != Color.Transparent && thicknessRight > 0)
					_uIPrimativeDrawer.DrawLine(spriteBatch, tr + new Vector2(-(thicknessRight / 2f), 0), br + new Vector2(-(thicknessRight / 2f), 0), thicknessRight, colorRight);
				if (colorBottom != Color.Transparent && thicknessBottom > 0)
					_uIPrimativeDrawer.DrawLine(spriteBatch, br + new Vector2(0, -(thicknessBottom / 2f)), bl + new Vector2(0, -(thicknessBottom / 2f)), thicknessBottom, colorBottom);
				if (colorLeft != Color.Transparent && thicknessLeft > 0)
					_uIPrimativeDrawer.DrawLine(spriteBatch, bl + new Vector2((thicknessLeft / 2f), 0), tl + new Vector2((thicknessLeft / 2f), 0), thicknessLeft, colorLeft);
			}

			// Debug drawing
			if (DrawDebugBorders)
			{
				_uIPrimativeDrawer.DrawRectangleOutline(spriteBatch, e.OuterRectangle, DebugOuterColor, 1, 0.5f);
				_uIPrimativeDrawer.DrawRectangleOutline(spriteBatch, e.BoundingRectangle, DebugBoundingColor, 1, 0.5f);
				_uIPrimativeDrawer.DrawRectangleOutline(spriteBatch, e.InnerRectangle, DebugInnerColor, 1, 0.5f);
			}
		}
		private void DrawChildren(SpriteBatch spriteBatch, GuiElement e, int rtCount, SamplerState? samplerState)
		{
			// Draw children
			foreach (var child in e._children)
				Draw2(spriteBatch, child, rtCount, samplerState);
		}

		[System.Obsolete]
		private void Draw(SpriteBatch spriteBatch, GuiElement e, Matrix previousTransform, Rectangle previousClipping)
		{
			// Simple draw
			if (!e.CurrentLayout.RequiresComplicatedDraw || GraphicsDevice == null)
			{
				DrawThisElement(spriteBatch, e);
				DrawChildren(spriteBatch, e, previousTransform, previousClipping);
				return;
			}

			// Stop previous spriteBatch call
			spriteBatch.End();

			// Store previous spritebatch states
			var previousRaster = GraphicsDevice.RasterizerState;
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
			GraphicsDevice.ScissorRectangle = nextClipping;
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
			GraphicsDevice.ScissorRectangle = previousClipping;
			spriteBatch.Begin(
				sortMode: SpriteSortMode.Deferred,
				blendState: BlendState.NonPremultiplied,
				samplerState: SamplerState.PointClamp,
				rasterizerState: previousRaster,
				transformMatrix: previousTransform
			);
		}
		[System.Obsolete]
		private void DrawChildren(SpriteBatch spriteBatch, GuiElement e, Matrix previousTransform, Rectangle previousClipping)
		{
			// Draw children
			foreach (var child in e._children)
				Draw(spriteBatch, child, previousTransform, previousClipping);
		}

		// Helper functions
		Rectangle GetClippingRectangle(GuiElement e)
		{
			int x = 0, y = 0, w = ScreenWidth, h = ScreenHeight;
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

		Rectangle TransformRectangle(Rectangle r, Matrix m)
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
}