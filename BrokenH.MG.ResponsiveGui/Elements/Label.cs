using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BrokenH.MG.ResponsiveGui.Styles;

namespace BrokenH.MG.ResponsiveGui.Elements
{
	public class Label : GuiElement
	{
		private Line[] _lines;

		private string _text = "";
		public string Text
		{
			get => _text;
			[MemberNotNull(nameof(_lines))]
			set
			{
				_text = value;
				CreateLines(_text);
				TextSize = ComputeTotalTextSize();
			}
		}


		public Label(Layout? layout, string text = "") : base(layout)
		{
			Text = text;
		}

		[MemberNotNull(nameof(_lines))]
		private void CreateLines(string fullText)
		{
			if (CurrentLayout.Font == null)
			{
				_lines = new Line[0];
				return;
			}

			if (fullText == string.Empty)
			{
				_lines = new Line[]
				{
					new Line(fullText, CurrentLayout.Font.MeasureString(fullText) * CurrentLayout.FontScale)
				};
			}

			_lines = CurrentLayout.WordWrapMode switch
			{
				WordWrapMode.None => WordWrapNone(fullText),
				WordWrapMode.WordWrap => WordWrap(fullText),
				WordWrapMode.BreakWord => WordWrapBreakWord(fullText),
				_ => WordWrapNone(fullText),
			};
		}
		private Line[] WordWrapNone(string fullText)
		{
			if (CurrentLayout.Font == null)
				return new Line[0];
			return new Line[]
			{
				new Line(fullText, CurrentLayout.Font.MeasureString(fullText) * CurrentLayout.FontScale)
			};
		}
		private Line[] WordWrap(string fullText)
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
		private Line[] WordWrapBreakWord(string fullText)
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

		protected override void AfterRectangleCompute()
		{
			CreateLines(_text);
			TextSize = ComputeTotalTextSize();

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
		}
		private void LeftAlignLines()
		{
			float x = Position.X + CurrentLayout.PaddingLeft;
			float y = Position.Y + CurrentLayout.PaddingTop;
			foreach (var line in _lines)
			{
				line.Position = new Vector2(x, y);
				y += line.Size.Y;
			}
		}
		private void RightAlignLines()
		{
			float x = Position.X + Size.X - CurrentLayout.PaddingRight;
			float y = Position.Y + CurrentLayout.PaddingTop;
			foreach (var line in _lines)
			{
				line.Position = new Vector2(x - line.Size.X, y);
				y += line.Size.Y;
			}
		}
		private void CenterAlignLines()
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
		private void TopAlignLines()
		{
			float y = Position.Y + CurrentLayout.PaddingTop;
			foreach (var line in _lines)
			{
				line.Position.Y = y;
				y += line.Size.Y;
			}
		}
		private void MiddleAlignLines()
		{
			float y = Position.Y + ((Size.Y - TextSize!.Value.Y) / 2f);
			foreach (var line in _lines)
			{
				line.Position.Y = y;
				y += line.Size.Y;
			}
		}
		private void BottomAlignLines()
		{
			float y = Position.Y + Size.Y - CurrentLayout.PaddingBottom - TextSize!.Value.Y;
			foreach (var line in _lines)
			{
				line.Position.Y = y;
				y += line.Size.Y;
			}
		}

		private Vector2 ComputeTotalTextSize()
		{
			// Add heights, max widths
			float width = 0;
			float height = 0;
			foreach (var line in _lines)
			{
				width = MathHelper.Max(width, line.Size.X);
				height += line.Size.Y;
			}

			return new Vector2(width, height) / CurrentLayout.FontScale;
		}

		protected override void OnDraw(SpriteBatch spriteBatch)
		{
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
}