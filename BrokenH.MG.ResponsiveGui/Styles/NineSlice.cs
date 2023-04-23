using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BrokenH.MG.ResponsiveGui.Styles;

public class NineSlice
{
	/* Note rectangle indices:
		0 1 2
		3 4 5
		6 7 8
	*/

	private int _left;
	private int _right;
	private int _top;
	private int _bottom;

	// Cached values
	private NineSliceAsset _asset;
	private int _rightX;
	private int _bottomY;
	private Rectangle _previousDestination;

	// Properties
	public Rectangle[] SourceRectangles { get; private set; }
	public Rectangle[] DestinationRectangles { get; private set; }


	public NineSlice(NineSliceAsset asset)
	{
		_asset = asset;

		_left = asset.Spec.Left;
		_right = asset.Spec.Right;
		_top = asset.Spec.Top;
		_bottom = asset.Spec.Bottom;

		var width = asset.Spec.Width;
		var height = asset.Spec.Height;

		_rightX = width - _right;
		_bottomY = height - _bottom;
		var midW = width - _left - _right;
		var midH = height - _top - _bottom;

		SourceRectangles = new Rectangle[]
		{
			new Rectangle(0,		0, _left,	_top),
			new Rectangle(_left,	0, midW,	_top),
			new Rectangle(_rightX,	0, _right,	_top),

			new Rectangle(0,		_top, _left,	midH),
			new Rectangle(_left,	_top, midW,		midH),
			new Rectangle(_rightX,	_top, _right,	midH),

			new Rectangle(0,		_bottomY, _left,	_bottom),
			new Rectangle(_left,	_bottomY, midW,		_bottom),
			new Rectangle(_rightX,	_bottomY, _right,	_bottom),
		};

		DestinationRectangles = new Rectangle[9];
	}

	public void SetDestinationRectangle(Rectangle destinationRectangle)
	{
		_previousDestination = destinationRectangle;
		var r = destinationRectangle;

		var s = _asset.Spec.Scale;
		var top = _top * s;
		var bottom = _bottom * s;
		var left = _left * s;
		var right = _right * s;

		var x = r.X;
		var y = r.Y;
		var rightX = r.Width - right;
		var bottomY = r.Height - bottom;
		var dmidW = r.Width - left - right;
		var dmidH = r.Height - top - bottom;

		DestinationRectangles[0] = new Rectangle(x,				y, left,   top);
		DestinationRectangles[1] = new Rectangle(x + left,		y, dmidW,   top);
		DestinationRectangles[2] = new Rectangle(x + rightX,	y, right,  top);

		DestinationRectangles[3] = new Rectangle(x + 0,			y + top,   left,  dmidH);
		DestinationRectangles[4] = new Rectangle(x + left,		y + top,   dmidW,  dmidH);
		DestinationRectangles[5] = new Rectangle(x + rightX,	y + top,   right, dmidH);

		DestinationRectangles[6] = new Rectangle(x + 0,			y + bottomY,   left,    bottom);
		DestinationRectangles[7] = new Rectangle(x + left,		y + bottomY,   dmidW,    bottom);
		DestinationRectangles[8] = new Rectangle(x + rightX,	y + bottomY,   right,   bottom);
	}

	public void Draw(
		SpriteBatch spriteBatch,
		Rectangle destinationRectangle,
		Color color)
	{
		if (_previousDestination != destinationRectangle)
			SetDestinationRectangle(destinationRectangle);

		for (int i = 0; i < 9; i++)
			spriteBatch.Draw(
				texture: _asset.Texture,
				destinationRectangle: DestinationRectangles[i],
				sourceRectangle: SourceRectangles[i],
				color: color);
	}
}

public class NineSliceAsset
{
	public Texture2D Texture { get; private set; }
	public NineSliceSpec Spec { get; private set; }

	public NineSliceAsset(Texture2D texture, NineSliceSpec spec)
	{
		Texture = texture;
		Spec = spec;
	}
}

public class NineSliceSpec
{
	public int Width { get; set; }
	public int Height { get; set; }
	public int Left { get; set; }
	public int Right { get; set; }
	public int Top { get; set; }
	public int Bottom { get; set; }

	public int Scale { get; set; }
}