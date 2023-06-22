using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BrokenH.MG.ResponsiveGui.Rendering;

public interface IUiPrimitiveDrawer
{
	void DrawRectangle(SpriteBatch spriteBatch, Vector2 position, Vector2 size, float angle, Color color, float layerDepth = 0);
	void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color);
	void DrawLine(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, float thickness, Color color);
	void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rectangle, Color color, float thickness, float inset);
}