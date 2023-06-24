using System.IO;
using System.Text.Json;
using BrokenH.MG.ResponsiveGui.Common;
using BrokenH.MG.ResponsiveGui.Elements;
using BrokenH.MG.ResponsiveGui.Rendering;
using BrokenH.MG.ResponsiveGui.Styles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GuiSample;

public class ResponsiveGuiSample : Game
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

	// Settings
	private float _masterVolume = 0.3f;

	// Property to test slider
	private float Volume = 0.3f;


	public ResponsiveGuiSample()
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
		SoundEffect.MasterVolume = _masterVolume;
	}
	protected override void Initialize()
	{
		base.Initialize();
		_spriteBatch = new SpriteBatch(GraphicsDevice);
	}
	protected override void LoadContent()
	{
		var aboutText = "What the heck did you just frickin' say about me, you little whiner? I'll have you know I graduated top of my class in the Navy Seals, and I've been involved in numerous secret raids on Al-Quaeda, and I have over 300 confirmed kills."
			+ " I am trained in gorilla warfare and I'm the top sniper in the entire US armed forces.You are nothing to me but just another target. I will wipe you the frick out with precision the likes of which has never been seen before on this Earth, mark my frickin' words.You think you can get away with saying that crap to me over the Internet?"
			+ " Think again, buddy. As we speak I am contacting my secret network of spies across the USA and your IP is being traced right now so you better prepare for the storm, buddy."
		;

		// Load content
		var font = Content.Load<SpriteFont>("pixelFont");
		Layout.Initialize(font);
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
			ForegroundColor = Color.White,
			// Height = 64,
			TextAlign = TextAlign.Center,
			Padding2 = (-30, 8),
			FontScale = 12,
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
			BackgroundColor = new Color(80, 80, 80),
			Width_ = "100%",
		};
		buttonLayout[ElementStates.Activated] = new Layout(buttonLayout[ElementStates.Hovered])
		{
			BackgroundColor = Color.White,
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
			JustifyContent = JustifyContent.SpaceBetween,
			AlignItems = AlignItems.Center,
			Width_ = "100%",
		};
		var formLabel = new Layout()
		{
			ForegroundColor = Color.White,
			FontScale = 6,
			Width_ = "35%",
			PaddingLeft = 20,
			TextAlign = TextAlign.Left,
		};
		var formSlider = new Layout()
		{
			Width_ = "55%",
			Height = 200,
			ForegroundColor = new Color(250, 120, 0),
			BackgroundColor = darkGray,

			FlexDirection = FlexDirection.Column,
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
		};
		sliderHandle[ElementStates.Hovered] = new Layout(sliderHandle)
		{
			// BackgroundColor = new Color(80, 80, 80),
			// BorderColor = new Color(80, 80, 80),\
			BorderColor = Color.Cyan,
			Transition = new Transition(0.4d, TimingFunction.EaseOutCubic),
		};
		sliderHandle[ElementStates.Activated] = new Layout(sliderHandle[ElementStates.Hovered])
		{
			BorderColor = Color.OrangeRed,
			Transition = new Transition(0.1d, TimingFunction.EaseOutCubic),
		};
		var sliderNumber = new Layout()
		{
			Width_ = "10%",
			ForegroundColor = Color.White,
			FontScale = 4,
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
					.AddChild(new Button(buttonLayout, ToggleDebugBorders, "Toggle debug borders"))
					.AddChild(new Button(backButtonLayout, Exit, "Quit"))
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
					.AddChild(new Button(buttonLayout, null, "Button"))
					.AddChild(new Button(backButtonLayout, () => SwitchBody(_title), "Back"))
				)
			)
		;
		#endregion

		#region Settings
		_settings
			.AddChild(new Container(menuFrameLayout)
				.AddChild(new Label(headingLayout, "Settings"))
				.AddChild(new DragScrollContainer(scrollingButtonContainer)
					.AddChild(new Container(formRowLayout)
						.AddChild(new Label(formLabel, "Volume"))
						.AddChild(new Slider(formSlider, sliderHandle, 0, 1, Volume, (value) => Volume = value))
						.AddChild(new Label(sliderNumber, () => Volume.ToString("P0")))
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

		// var menuTexture =_body.Draw(_spriteBatch, SamplerState.PointClamp);

		_spriteBatch.Begin(
			sortMode: SpriteSortMode.Deferred,
			blendState: BlendState.AlphaBlend,
			samplerState: SamplerState.PointClamp
		);
		_body.Draw(_spriteBatch, SamplerState.PointClamp);
		_spriteBatch.End();

		base.Draw(gameTime);
	}

	private void ToggleDebugBorders() => ElementRenderer.DrawDebugBorders = !ElementRenderer.DrawDebugBorders;

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

	private bool WasPressed(Keys key) => (_newKeyState.IsKeyDown(key) && _oldKeyState.IsKeyUp(key));
	private bool WasReleased(Keys key) => (_newKeyState.IsKeyUp(key) && _oldKeyState.IsKeyDown(key));

	private T LoadJson<T>(string fileName) => JsonSerializer.Deserialize<T>(LoadText(fileName));
	private string LoadText(string fileName)
	{
		string path = Path.Combine("Content", fileName);
		using (var stream = TitleContainer.OpenStream(path))
		using (var reader = new StreamReader(stream))
			return reader.ReadToEnd();
	}
}
