using System;
using SadConsole;
using Console = SadConsole.Console;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SadConsole.Components;
using TearsInRain.UI;
using TearsInRain.Commands;

namespace TearsInRain {
    class GameLoop {

        public const int GameWidth = 120;
        public const int GameHeight = 80;

        public static UIManager UIManager;
        public static World World;
        public static CommandManager CommandManager;

        static int oldWindowPixelWidth;
        static int oldWindowPixelHeight;

        public static Discord.Discord discord;


        static void Main(string[] args) {
            SadConsole.Game.Create(GameWidth, GameHeight);
            
            SadConsole.Game.OnInitialize = Init;
            SadConsole.Game.OnUpdate = Update;
            

            SadConsole.Game.Instance.Run();
            SadConsole.Game.Instance.Dispose();
            
        }

        private static void Update(GameTime time) {
            discord.RunCallbacks();
        }

        private static void Init() {
            Global.FontDefault = Global.LoadFont("fonts/Cheepicus12.font").GetFont(Font.FontSizes.One);
            Global.FontDefault.ResizeGraphicsDeviceManager(SadConsole.Global.GraphicsDeviceManager, 100, 75, 0, 0);
            Global.ResetRendering();

            SadConsole.Themes.Library.Default.WindowTheme = new GlobalTheme();
            SadConsole.Themes.Library.Default.WindowTheme.RefreshTheme(SadConsole.Themes.Library.Default.Colors);

            Settings.AllowWindowResize = true;

            UIManager = new UIManager();
            CommandManager = new CommandManager();
            World = new World();
            UIManager.Init();
            SadConsole.Game.Instance.Window.ClientSizeChanged += Window_ClientSizeChanged;

            discord = new Discord.Discord(579827348665532425, (UInt64)Discord.CreateFlags.Default);
        }

        private static void Window_ClientSizeChanged(object sender, EventArgs e) {
            int windowPixelsWidth = SadConsole.Game.Instance.Window.ClientBounds.Width;
            int windowPixelsHeight = SadConsole.Game.Instance.Window.ClientBounds.Height;

            // If this is getting called because of the ApplyChanges, exit.
            if (windowPixelsWidth == oldWindowPixelWidth && windowPixelsHeight == oldWindowPixelHeight)
                return;

            // Store for later
            oldWindowPixelWidth = windowPixelsWidth;
            oldWindowPixelHeight = windowPixelsHeight;

            // Get the exact pixels we can fit in that window based on a font.
            int fontPixelsWidth = (windowPixelsWidth / SadConsole.Global.FontDefault.Size.X) * SadConsole.Global.FontDefault.Size.X;
            int fontPixelsHeight = (windowPixelsHeight / SadConsole.Global.FontDefault.Size.Y) * SadConsole.Global.FontDefault.Size.Y;

            // Resize the monogame rendering to match
            SadConsole.Global.GraphicsDeviceManager.PreferredBackBufferWidth = windowPixelsWidth;
            SadConsole.Global.GraphicsDeviceManager.PreferredBackBufferHeight = windowPixelsHeight;
            SadConsole.Global.GraphicsDeviceManager.ApplyChanges();

            // Tell sadconsole how much to render to the screen.
            Global.RenderWidth = fontPixelsWidth;
            Global.RenderHeight = fontPixelsHeight;
            Global.ResetRendering();

            // Get the total cells you can fit
            int totalCellsX = fontPixelsWidth / SadConsole.Global.FontDefault.Size.X;
            int totalCellsY = fontPixelsHeight / SadConsole.Global.FontDefault.Size.Y;

            UIManager.checkResize(totalCellsX, totalCellsY);
        }
    }
}