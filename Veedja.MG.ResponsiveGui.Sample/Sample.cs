using System.IO;
using System.Text.Json;
using Veedja.MG.ResponsiveGui.Common;
using Veedja.MG.ResponsiveGui.Elements;
using Veedja.MG.ResponsiveGui.Rendering;
using Veedja.MG.ResponsiveGui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;

namespace Veedja.MG.ResponsiveGui.Sample;

public class SampleGame : Game
{
	// MonoGame stuff
	private GraphicsDeviceManager _graphics;
	private SpriteBatch _spriteBatch;

	// Input
	private KeyboardState _newKeyState;
	private KeyboardState _oldKeyState;

	private MouseState _newMouseState;
	private MouseState _oldMouseState;

	// GUI Elements
	private RootGuiElement _body;
	private RootGuiElement _title;
	private RootGuiElement _about;
	private RootGuiElement _settings;
	private RootGuiElement _grid;

	// Elements to keep track of to reference later
	private Label _volumeIndicator;
	private Label _drawDebugBordersIndicator;


	public SampleGame()
	{
		_graphics = new GraphicsDeviceManager(this)
		{
			PreferredBackBufferWidth = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width * 0.8f),
			PreferredBackBufferHeight = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height * 0.8f),
		};
		Window.AllowUserResizing = true;
		Window.ClientSizeChanged += (sender, e) => WindowSizeChanged();
		Content.RootDirectory = "Content";
		TargetElapsedTime = System.TimeSpan.FromSeconds(1.0d / 144.0d);
		IsMouseVisible = true;
		SoundEffect.MasterVolume = 0.5f;
	}
	protected override void Initialize()
	{
		base.Initialize();
		_spriteBatch = new SpriteBatch(GraphicsDevice);
	}
	protected override void LoadContent()
	{
		var aboutText
			= "Welcome to MGResponsiveGui! This package lets you create GUIs in a similar fashion to using HTML and CSS to create web pages."
			+ " The Layout class allows you to position elements using most of the same functionality availible in css, allowing you to easily create game menus that work on many different size screens."
			+ " You can use Transitions and Animations make your Menus just like you can with css!"
			+ " Basic web style concepts are supported such as scrolling (not just text, but entire element subtrees), text wrap, interacting using a gamepad (try the arrow keys and enter!), and even sounds!"
			+ " Some basic form elements are built in: containers, labels, buttons, checkboxes, and sliders."
			+ " I plan on creating more basic elements in the future, such as: text boxes, drop downs, and rich text in labels."
			+ "\n\n"
			+ "As of now, this has only been tested with SamplerState.PointClamp and a pixel art font and style, and BlendState cannot be specified."
			+ "\n\n"
			+ "Allowing more flexibility with SamplerState and BlendState, is planned for the future."
			+ " To see other plans for this package, check out the issues page on github!"
		;

		// Load content
		var pixelFont = Content.Load<SpriteFont>("pixelFont");
		// var titleFont = Content.Load<SpriteFont>("TitleFont");
		// var arialFont = Content.Load<SpriteFont>("Arial");
		Layout.Initialize(pixelFont);
		GuiElement.Initialize(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

		var itemFrame = new NineSliceAsset(Content.Load<Texture2D>("MenuItemFrame"), LoadJson<NineSliceSpec>("MenuItemFrame.json"));

		// Sound effects
		var hoverSound = Content.Load<SoundEffect>("Hover");
		var backSound = Content.Load<SoundEffect>("Click_Back");
		var forwardSound = Content.Load<SoundEffect>("Click_Enter");

		#region Layouts

		// Set up layout (like css)
		var darkGray = Color.DarkSlateGray;

		// This bodyLayout is the layout I give to RootGuiElements.
		// It's fullscreen and children will be centered
		var bodyLayout = new Layout()
		{
			FlexDirection = FlexDirection.Column,
			JustifyContent = JustifyContent.Center,
			AlignItems = AlignItems.Center,
			Width_ = "100%",
			Height_ = "100%",
			Padding = 80,
		};
		var menuFrameLayout = new Layout()
		{
			AlignItems = AlignItems.Center,

			Width_ = "100%",

			BorderColor = Color.White,
			BorderThickness = 4,
			Padding = 12,
		};
		var headingLayout = new Layout()
		{
			FontScale = 12,
			ForegroundColor = Color.White,
			TextAlign = TextAlign.Center,
			Padding2 = (-30, 8),
		};
		var scrollingButtonContainer = new Layout()
		{
			AlignItems = AlignItems.Center,
			Width_ = "100%",
			MaxHeight = 488,
			Gap = 8,
			ScrollPadding = 56,
			MarginBottom = 8,
			OverflowY = Overflow.Scroll,
			AllowScrollingFromAnywhere = true,
		};

		var rowLayout = new Layout()
		{
			FlexDirection = FlexDirection.Row,
			JustifyContent = JustifyContent.SpaceBetween,
			Width_ = "100%",
			Height = 280,
			Padding = 40
		};
		var gridButtonLayout = new Layout()
		{
			ForegroundColor = Color.Cyan,
			BackgroundColor = new Color(0, 0, 0),
			BorderColor = Color.Cyan,
			BorderThickness = 4,

			FontScale = 8,

			Width = 200,
			Height = 200,

			Transition = new Transition(0.4d, TimingFunction.EaseOutCubic),
		};
		gridButtonLayout[ElementStates.Hovered] = new Layout(gridButtonLayout)
		{
			BackgroundColor = new Color(40, 40, 40),

			Transform = Matrix.CreateScale(1.2f),
			Transition = new Transition(0.4d, TimingFunction.EaseOutCubic),
		};
		gridButtonLayout[ElementStates.Activated] = new Layout(gridButtonLayout[ElementStates.Hovered])
		{
			BackgroundColor = Color.Cyan,
			ForegroundColor = Color.Black,
			Transform = Matrix.Identity,
			Transition = new Transition(0.1d, TimingFunction.EaseInOutCubic),
		};

		var buttonLayout = new Layout()
		{
			NineSlice = new NineSlice(itemFrame),
			ImageColor = Color.Cyan,
			ForegroundColor = Color.White,
			FontScale = 6,
			Width_ = "80%",
			Height = 72,
			Transition = new Transition(0.4d, TimingFunction.EaseOutCubic),
			ActivateSound = forwardSound,
			HoverSound = hoverSound
		};
		buttonLayout[ElementStates.Hovered] = new Layout(buttonLayout)
		{
			BackgroundColor = darkGray,
			Width_ = "100%",
		};
		buttonLayout[ElementStates.Activated] = new Layout(buttonLayout[ElementStates.Hovered])
		{
			Width_ = "90%",
			BackgroundColor = Color.Cyan,
			ForegroundColor = Color.Black,
			Transition = new Transition(0.1d, TimingFunction.EaseOutCubic),
		};

		var backButtonLayout = new Layout(buttonLayout)
		{
			ActivateSound = backSound
		};

		var textScrollBox = new Layout()
		{
			AlignItems = AlignItems.Center,
			Width_ = "100%",
			MaxHeight = 64 * 5 + 8 * 4,
			Padding = 8,
			OverflowY = Overflow.Scroll,
			AllowScrollingFromAnywhere = true,
		};
		textScrollBox[ElementStates.Hovered] = new Layout(textScrollBox)
		{
			BorderColor = Color.Cyan,
			BorderThickness = 2,
		};
		var wordWrapLayout = new Layout()
		{
			ForegroundColor = Color.White,
			Width_ = "100%",
			MaxWidth_ = "100%",

			BorderColor = new Color(80, 80, 80),
			BorderThickness = 4,
			Padding = 8,

			FontScale = 4,
			TextAlign = TextAlign.Left,
			WordWrapMode = WordWrapMode.WordWrap,
		};

		var formRowLayout = new Layout()
		{
			FlexDirection = FlexDirection.Row,
			JustifyContent = JustifyContent.FlexStart,
			AlignItems = AlignItems.Center,
			Width_ = "100%",
			Height = 90,
		};
		var formLabel = new Layout()
		{
			ForegroundColor = Color.White,
			FontScale = 6,
			Width_ = "45%",
			PaddingLeft = 20,
			TextAlign = TextAlign.Left,
		};

		var formSlider = new Layout()
		{
			Width_ = "40%",
			MarginLeft = 20,
			Height = 20,
			ForegroundColor = Color.Cyan,
			BackgroundColor = darkGray,

			FlexDirection = FlexDirection.Row,
			JustifyContent = JustifyContent.FlexStart,
			AlignItems = AlignItems.Center,
		};
		var sliderHandle = new Layout()
		{
			Width = 40,
			Height = 40,

			// BackgroundColor = Color.Black,
			BackgroundColor = Color.Transparent,
			BorderColor = Color.White,
			BorderThickness = 4,
			Transition = new Transition(0.1d, TimingFunction.EaseOutCubic),
		};
		sliderHandle[ElementStates.Hovered] = new Layout(sliderHandle)
		{
			// BackgroundColor = new Color(80, 80, 80),
			// BorderColor = new Color(80, 80, 80),
			BorderColor = Color.Cyan,
			Transform = Matrix.CreateScale(1.2f),
			Transition = new Transition(0.2d, TimingFunction.EaseOutCubic),
		};
		sliderHandle[ElementStates.Activated] = new Layout(sliderHandle[ElementStates.Hovered])
		{
			BorderColor = Color.Cyan,
			BackgroundColor = Color.Cyan,
			Transform = Matrix.Identity,
			Transition = new Transition(0.1d, TimingFunction.EaseOutCubic),
		};

		var checkbox = new Layout()
		{
			Width = 40,
			Height = 40,
			BackgroundColor = Color.Transparent,
			BorderColor = Color.White,
			BorderThickness = 4,
			Transition = new Transition(0.1d, TimingFunction.EaseOutCubic),
		};
		checkbox[ElementStates.Hovered] = new Layout(checkbox)
		{
			BorderColor = Color.Cyan,
			Transform = Matrix.CreateScale(1.2f),
			Transition = new Transition(0.2d, TimingFunction.EaseOutCubic),
		};
		checkbox[ElementStates.Activated] = new Layout(checkbox)
		{
			BorderColor = Color.Cyan,
			BackgroundColor = Color.Cyan,
			Transform = Matrix.Identity,
			Transition = new Transition(0.1d, TimingFunction.EaseOutCubic),
		};
		var checkboxChecked = new Layout(checkbox)
		{
			BorderColor = Color.White,
			BackgroundColor = Color.White,
			Transition = new Transition(0.1d, TimingFunction.EaseOutCubic),
		};
		checkboxChecked[ElementStates.Hovered] = new Layout(checkboxChecked)
		{
			BorderColor = Color.Cyan,
			BackgroundColor = Color.Cyan,
			Transform = Matrix.CreateScale(1.2f),
			Transition = new Transition(0.2d, TimingFunction.EaseOutCubic),
		};
		checkboxChecked[ElementStates.Activated] = new Layout(checkboxChecked)
		{
			BorderColor = Color.Cyan,
			BackgroundColor = Color.Transparent,
			Transform = Matrix.Identity,
			Transition = new Transition(0.1d, TimingFunction.EaseOutCubic),
		};

		var valueIndicator = new Layout()
		{
			ForegroundColor = Color.White,
			FontScale = 4,
			Width_ = "10%",
			Height_ = "100%",
			Right = 0,
			PositionMode = PositionMode.RelativeToParent
		};

		#endregion

		// Build element tree (like html)
		_title		= new RootGuiElement(bodyLayout);
		_about		= new RootGuiElement(bodyLayout);
		_settings	= new RootGuiElement(bodyLayout);
		_grid		= new RootGuiElement(bodyLayout);

		#region Title
		_title
			.AddChild(new Container(menuFrameLayout)
				.AddChild(new Label(headingLayout, "Responsive GUI\nfor MonoGame"))
				.AddChild(new DragScrollContainer(scrollingButtonContainer)
					.AddChild(new Button(buttonLayout, () => SwitchBody(_about), "About"))
					.AddChild(new Button(buttonLayout, () => SwitchBody(_settings), "Settings"))
					.AddChild(new Button(buttonLayout, () => SwitchBody(_grid), "Grid Example"))
					.AddChild(new Button(backButtonLayout, async () => await Quit(), "Quit"))
				)
			)
		;
		#endregion

		#region About
		_about
			.AddChild(new Container(menuFrameLayout)
				.AddChild(new Label(headingLayout, "About page"))
				.AddChild(new DragScrollContainer(textScrollBox, true)
					.AddChild(new Label(wordWrapLayout, aboutText))
				)
				.AddChild(new DragScrollContainer(scrollingButtonContainer)
					.AddChild(new Button(backButtonLayout, () => SwitchBody(_title), "Back"))
				)
			)
		;
		#endregion

		#region Settings
		_volumeIndicator = new Label(valueIndicator, SoundEffect.MasterVolume.ToString("P0"));
		_drawDebugBordersIndicator = new Label(valueIndicator, ElementRenderer.DrawDebugBorders.ToString());

		_settings
			.AddChild(new Container(menuFrameLayout)
				.AddChild(new Label(headingLayout, "Settings"))
				.AddChild(new DragScrollContainer(scrollingButtonContainer)
					.AddChild(new Container(formRowLayout)
						.AddChild(new Label(formLabel, "Volume"))
						.AddChild(new Slider(formSlider, sliderHandle, 0, 1, SoundEffect.MasterVolume) { OnValueTargetChange = OnVolumeChange })
						.AddChild(_volumeIndicator)
					)
					.AddChild(new Container(formRowLayout)
						.AddChild(new Label(formLabel, "Debug Borders"))
						.AddChild(new Checkbox(checkbox, checkboxChecked, ElementRenderer.DrawDebugBorders) { OnValueChange = OnDrawDebugBorderChange })
						.AddChild(_drawDebugBordersIndicator)
					)
				)
				.AddChild(new Button(backButtonLayout, () => SwitchBody(_title), "Back"))
			)
		;
		#endregion

		#region Grid
		_grid
			.AddChild(new Container(menuFrameLayout)
				.AddChild(new Label(headingLayout, "Grid Example"))
				.AddChild(new DragScrollContainer(scrollingButtonContainer)
					.AddChild(new Container(rowLayout)
						.AddChild(new Button(gridButtonLayout, null, "1"))
						.AddChild(new Button(gridButtonLayout, null, "2"))
						.AddChild(new Button(gridButtonLayout, null, "3"))
					)
					.AddChild(new Container(rowLayout)
						.AddChild(new Button(gridButtonLayout, null, "4"))
						.AddChild(new Button(gridButtonLayout, null, "5"))
						.AddChild(new Button(gridButtonLayout, null, "6"))
					)
					.AddChild(new Container(rowLayout)
						.AddChild(new Button(gridButtonLayout, null, "7"))
						.AddChild(new Button(gridButtonLayout, null, "8"))
						.AddChild(new Button(gridButtonLayout, null, "9"))
					)
					.AddChild(new Container(rowLayout)
						.AddChild(new Button(gridButtonLayout, null, "10"))
						.AddChild(new Button(gridButtonLayout, null, "11"))
						.AddChild(new Button(gridButtonLayout, null, "12"))
					)
				)
				.AddChild(new DragScrollContainer(scrollingButtonContainer)
					.AddChild(new Button(backButtonLayout, () => SwitchBody(_title), "Back"))
				)
			)
		;
		#endregion

		_body = _title;
		WindowSizeChanged();
	}

	private void OnVolumeChange(float value)
	{
		SoundEffect.MasterVolume = value;
		_volumeIndicator.Text = value.ToString("P0");
	}
	private void OnDrawDebugBorderChange(bool value)
	{
		ElementRenderer.DrawDebugBorders = value;
		_drawDebugBordersIndicator.Text = value.ToString();
	}

	protected override void Update(GameTime gameTime)
	{
		HandleInput();

		_body.Update(gameTime);

		base.Update(gameTime);
	}

	private void HandleInput()
	{
		_newKeyState = Keyboard.GetState();
		_newMouseState = Mouse.GetState();

		// Escape key
		if (WasPressed(Keys.Escape))
		{
			if (_body == _title)
			{
				Exit();
				return;
			}
			else
				SwitchBody(_title);
		}

		// Changing focus and activating elements (hitting the arrow keys and enter) to interact with the GUI via keyboard
		// The process can also work with a gamepad
		if (WasPressed(Keys.Enter))
			_body.FocusedElement?.ActivatePress();
		else if (WasReleased(Keys.Enter))
			_body.FocusedElement?.ActivateRelease();

		if (WasPressed(Keys.Left))
			_body.ChangeFocusLeft();
		if (WasPressed(Keys.Right))
			_body.ChangeFocusRight();
		if (WasPressed(Keys.Up))
			_body.ChangeFocusUp();
		if (WasPressed(Keys.Down))
			_body.ChangeFocusDown();

		// Left click
		if (_newMouseState.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Released)
			_body.MouseEvent(1, ButtonState.Pressed);
		if (_newMouseState.LeftButton == ButtonState.Released && _oldMouseState.LeftButton == ButtonState.Pressed)
			_body.MouseEvent(1, ButtonState.Released);

		// Middle click
		if (_newMouseState.MiddleButton == ButtonState.Pressed && _oldMouseState.MiddleButton == ButtonState.Released)
			_body.MouseEvent(2, ButtonState.Pressed);
		if (_newMouseState.MiddleButton == ButtonState.Released && _oldMouseState.MiddleButton == ButtonState.Pressed)
			_body.MouseEvent(2, ButtonState.Released);

		// Right click
		if (_newMouseState.RightButton == ButtonState.Pressed && _oldMouseState.RightButton == ButtonState.Released)
			_body.MouseEvent(3, ButtonState.Pressed);
		if (_newMouseState.RightButton == ButtonState.Released && _oldMouseState.RightButton == ButtonState.Pressed)
			_body.MouseEvent(3, ButtonState.Released);

		if (_newMouseState.ScrollWheelValue != _oldMouseState.ScrollWheelValue)
			_body.ScrollEvent(UIDirection.Vertical, _newMouseState.ScrollWheelValue - _oldMouseState.ScrollWheelValue);
		if (_newMouseState.HorizontalScrollWheelValue != _oldMouseState.HorizontalScrollWheelValue)
			_body.ScrollEvent(UIDirection.Horizontal, _newMouseState.HorizontalScrollWheelValue - _oldMouseState.HorizontalScrollWheelValue);

		_oldKeyState = _newKeyState;
		_oldMouseState = _newMouseState;
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.Black);

		_spriteBatch.Begin(
			sortMode: SpriteSortMode.Deferred,
			blendState: BlendState.AlphaBlend,
			samplerState: SamplerState.PointClamp
		);
		_body.Draw(_spriteBatch, SamplerState.PointClamp);
		_spriteBatch.End();

		base.Draw(gameTime);
	}

	private void SwitchBody(RootGuiElement newElement)
	{
		if (_body != _title)
			_body.Unfocus();
		_body.ResetAllTransitions();
		_body = newElement;
		_body.Refresh();
	}

	private void WindowSizeChanged()
	{
		int w = GraphicsDevice.Viewport.Width;
		int h = GraphicsDevice.Viewport.Height;

		GuiElement.UpdateSize(w, h);
	}

	private bool WasPressed(Keys key) => _newKeyState.IsKeyDown(key) && _oldKeyState.IsKeyUp(key);
	private bool WasReleased(Keys key) => _newKeyState.IsKeyUp(key) && _oldKeyState.IsKeyDown(key);

	private T LoadJson<T>(string fileName) => JsonSerializer.Deserialize<T>(LoadText(fileName));
	private string LoadText(string fileName)
	{
		string path = Path.Combine("Content", fileName);
		using (var stream = TitleContainer.OpenStream(path))
		using (var reader = new StreamReader(stream))
			return reader.ReadToEnd();
	}

	private async Task Quit()
	{
		await Task.Delay(300);
		Exit();
	}
}