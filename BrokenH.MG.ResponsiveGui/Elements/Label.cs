using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BrokenH.MG.ResponsiveGui.Styles;
using System;

namespace BrokenH.MG.ResponsiveGui.Elements;

public class Label : GuiElement
{
	private Line[] _lines;

	private string _text = "";
	public string Text
	{
		get => _text;
		set
		{
			if (_text != value)
				_text = value;
		}
	}
	public Func<string>? TextGetter { get; set; }


	public Label(Layout? layout, string text = "") : base(layout)
	{
		Text = text;
		_lines = new Line[0];
	}
	public Label(Layout? layout, Func<string> textGetter) : base(layout)
	{
		TextGetter = textGetter;
		Text = textGetter();
		_lines = new Line[0];
	}

	protected override void OnUpdate(GameTime gameTime)
	{
		if (TextGetter != null)
			Text = TextGetter();
	}

	protected override void AfterRectangleCompute()
	{
		if (string.IsNullOrEmpty(Text))
			return;

		// TODO:
		// If: text has changed ||
		// current layout has meaningfully changed* (font, font scale, padding, wrap mode, text align, any positioning) ||
		// bounding box (position or size) has changed
		// Then: call the next three functions
		// Note*: If the layout has changed, then RectangleCompute will get called again, so this consideration will not be relevant when that happens.

		CreateLines();
		ComputeTotalTextSize();
		AlignLines();
	}

	private void CreateLines()
	{
		if (CurrentLayout.Font == null || Text == string.Empty)
		{
			_lines = new Line[0];
			return;
		}

		_lines = CurrentLayout.WordWrapMode switch
		{
			WordWrapMode.None => WordWrapNone(Text),
			WordWrapMode.WordWrap => WordWrap(Text),
			WordWrapMode.BreakWord => WordWrapBreakWord(Text),
			_ => WordWrapNone(Text),
		};

		Line[] WordWrapNone(string fullText)
		{
			if (CurrentLayout.Font == null)
				return new Line[0];
			return new Line[]
			{
			new Line(fullText, CurrentLayout.Font.MeasureString(fullText) * CurrentLayout.FontScale)
			};
		}
		Line[] WordWrap(string fullText)
		{
			if (CurrentLayout.Font == null)
				return new Line[0];

			if (Size.X == 0)
				return WordWrapNone(fullText);

			List<Line> lines = new();
			float maxLineWidth = (Size.X) - CurrentLayout.PaddingLeft - CurrentLayout.PaddingRight;
			var characters = fullText.ToCharArray();

			int currentLineStartIndex = 0;
			int lastWhitespaceIndex = 0;
			Vector2 lineSize = Vector2.Zero;
			Vector2 previousLineSize = Vector2.Zero;
			string subString;

			for (int i = 0; i < characters.Length; i++)
			{
				if (char.IsWhiteSpace(characters[i]))
				{
					previousLineSize = lineSize;
					lastWhitespaceIndex = i;
				}

				subString = fullText.Substring(currentLineStartIndex, i + 1 - currentLineStartIndex);
				lineSize = CurrentLayout.Font.MeasureString(subString) * CurrentLayout.FontScale;
				if (lineSize.X > maxLineWidth)
				{
					if (lastWhitespaceIndex > currentLineStartIndex)
						subString = fullText.Substring(currentLineStartIndex, lastWhitespaceIndex - currentLineStartIndex);
					else
					{
						subString = fullText.Substring(currentLineStartIndex, i - currentLineStartIndex);
						lastWhitespaceIndex = i;
					}
					lines.Add(new Line(subString, previousLineSize));
					currentLineStartIndex = lastWhitespaceIndex + 1;
				}
			}
			subString = fullText.Substring(currentLineStartIndex);
			previousLineSize = CurrentLayout.Font.MeasureString(subString) * CurrentLayout.FontScale;
			lines.Add(new Line(subString, previousLineSize));

			return lines.ToArray();
		}
		Line[] WordWrapBreakWord(string fullText)
		{
			if (CurrentLayout.Font == null)
				return new Line[0];

			List<Line> lines = new();
			if (Size.X == 0)
				return WordWrapNone(fullText);

			float maxLineWidth = Size.X - CurrentLayout.PaddingLeft - CurrentLayout.PaddingRight;
			var characters = fullText.ToCharArray();

			int currentLineStartIndex = 0;
			Vector2 lineSize = Vector2.Zero;
			Vector2 previousLineSize = Vector2.Zero;
			string subString;

			for (int i = 0; i < characters.Length; i++)
			{
				previousLineSize = lineSize;

				subString = fullText.Substring(currentLineStartIndex, i + 1 - currentLineStartIndex);
				lineSize = CurrentLayout.Font.MeasureString(subString) * CurrentLayout.FontScale;
				if (lineSize.X > maxLineWidth)
				{
					subString = fullText.Substring(currentLineStartIndex, i - currentLineStartIndex);
					lines.Add(new Line(subString, previousLineSize));
					currentLineStartIndex = i;
				}
			}
			subString = fullText.Substring(currentLineStartIndex);
			previousLineSize = CurrentLayout.Font.MeasureString(subString) * CurrentLayout.FontScale;
			lines.Add(new Line(subString, previousLineSize));

			return lines.ToArray();
		}
	}
	private void ComputeTotalTextSize()
	{
		// Add heights, max widths
		float width = 0;
		float height = 0;
		foreach (var line in _lines)
		{
			width = MathHelper.Max(width, line.Size.X);
			height += line.Size.Y;
		}

		TextSize = new Vector2(width, height) / CurrentLayout.FontScale;
	}
	private void AlignLines()
	{
		switch (CurrentLayout.TextAlign)
		{
			case TextAlign.Left: LeftAlignLines(); break;
			case TextAlign.Center: CenterAlignLines(); break;
			case TextAlign.Right: RightAlignLines(); break;
		}
		switch (CurrentLayout.TextAlignVertical)
		{
			case TextAlignVertical.Top: TopAlignLines(); break;
			case TextAlignVertical.Middle: MiddleAlignLines(); break;
			case TextAlignVertical.Bottom: BottomAlignLines(); break;
		}

		void LeftAlignLines()
		{
			float x = Position.X + CurrentLayout.PaddingLeft;
			float y = Position.Y + CurrentLayout.PaddingTop;
			foreach (var line in _lines)
			{
				line.Position = new Vector2(x, y);
				y += line.Size.Y;
			}
		}
		void RightAlignLines()
		{
			float x = Position.X + Size.X - CurrentLayout.PaddingRight;
			float y = Position.Y + CurrentLayout.PaddingTop;
			foreach (var line in _lines)
			{
				line.Position = new Vector2(x - line.Size.X, y);
				y += line.Size.Y;
			}
		}
		void CenterAlignLines()
		{
			float y = Position.Y + CurrentLayout.PaddingTop;
			float w = 0;
			foreach (var line in _lines)
			{
				w = (Size.X - line.Size.X) / 2f;
				line.Position = new Vector2(Position.X + w, y);
				y += line.Size.Y;
			}
		}
		void TopAlignLines()
		{
			float y = Position.Y + CurrentLayout.PaddingTop;
			foreach (var line in _lines)
			{
				line.Position.Y = y;
				y += line.Size.Y;
			}
		}
		void MiddleAlignLines()
		{
			float y = Position.Y + ((Size.Y - TextSize!.Value.Y) / 2f);
			foreach (var line in _lines)
			{
				line.Position.Y = y;
				y += line.Size.Y;
			}
		}
		void BottomAlignLines()
		{
			float y = Position.Y + Size.Y - CurrentLayout.PaddingBottom - TextSize!.Value.Y;
			foreach (var line in _lines)
			{
				line.Position.Y = y;
				y += line.Size.Y;
			}
		}
	}

	protected override void OnDraw(SpriteBatch spriteBatch)
	{
		if (string.IsNullOrEmpty(Text))
			return;

		if (CurrentLayout.ForegroundColor == Color.Transparent)
			return;

		foreach (var line in _lines)
		{
			spriteBatch.DrawString(
				spriteFont: CurrentLayout.Font,
				text: line.Text,
				position: line.Position,
				color: CurrentLayout.ForegroundColor,
				rotation: 0,
				origin: Vector2.Zero,
				scale: CurrentLayout.FontScale,
				effects: SpriteEffects.None,
				layerDepth: 0.5f);
		}
	}
}

internal class Line
{
	internal string Text;
	internal Vector2 Size;
	internal Vector2 Position = Vector2.Zero;

	internal Line(string text, Vector2 size)
	{
		Text = text;
		Size = size;
	}
}