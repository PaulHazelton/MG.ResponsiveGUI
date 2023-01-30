using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BrokenH.MG.ResponsiveGui.Elements
{
	internal class UIPrimativeDrawer
	{
		private Texture2D _rectangleSprite;
		private static Vector2 _rectangleOrigin;

		internal UIPrimativeDrawer(GraphicsDevice graphicsDevice)
		{
			// Make a 1 x 1 pixel to draw rectangles and lines
			_rectangleSprite = new Texture2D(graphicsDevice, 1, 1);
			_rectangleSprite.SetData(new Color[] { Color.White });
			_rectangleOrigin = new Vector2(0.5f, 0.5f);
		}

		internal void DrawRectangle(SpriteBatch spriteBatch, Vector2 position, Vector2 size, float angle, Color color, float layerDepth = 0)
		{
			spriteBatch.Draw(_rectangleSprite, position, null, color, angle, _rectangleOrigin, size, SpriteEffects.None, layerDepth);
		}
		internal void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
		{
			spriteBatch.Draw(_rectangleSprite, rectangle, null, color, 0, Vector2.Zero, SpriteEffects.None, 0);
		}
		internal void DrawLine(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, float thickness, Color color)
		{
			Vector2 midpoint = Midpoint(p1, p2);
			Vector2 size = new Vector2((p1 - p2).Length(), thickness);
			float angle = (float)Math.Atan2((p2 - p1).Y, (p2 - p1).X);

			DrawRectangle(spriteBatch, midpoint, size, angle, color);
		}
		internal void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rectangle, Color color, float thickness, float inset)
		{
			if (thickness == 0) return;
			// Adjust alpha for very thin lines
			if (thickness < 1)
			{
				color.A = (byte)MathHelper.Lerp(0, color.A, thickness);
				thickness = 1;
			}

			var tl = rectangle.Location.ToVector2();
			var tr = tl + new Vector2(rectangle.Width, 0);
			var bl = tl + new Vector2(0, rectangle.Height);
			var br = tl + rectangle.Size.ToVector2();

			// Inset
			tl += new Vector2(inset, inset);
			tr += new Vector2(-inset, inset);
			bl += new Vector2(inset, -inset);
			br += new Vector2(-inset, -inset);

			DrawLine(spriteBatch, tl - new Vector2(thickness / 2f, 0), tr + new Vector2(thickness / 2f, 0), thickness, color);
			DrawLine(spriteBatch, tr, br, thickness, color);
			DrawLine(spriteBatch, br + new Vector2(thickness / 2f, 0), bl - new Vector2(thickness / 2f, 0), thickness, color);
			DrawLine(spriteBatch, bl, tl, thickness, color);
		}

		private Vector2 Midpoint(Vector2 a, Vector2 b) => new Vector2((a.X + b.X) / 2, (a.Y + b.Y) / 2);
	}
}