using System;
using Discord;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SadConsole;
using SadConsole.Controls;
using TearsInRain.Entities;

namespace TearsInRain.UI {
    public class UIManager : ContainerConsole {
        public ScrollingConsole MapConsole;
        public ScrollingConsole MultiConsole;

        public Window MapWindow;
        public MessageLogWindow MessageLog;
        public Window MultiplayerWindow;

        public UIManager() {
            IsVisible = true;
            IsFocused = true;

            Parent = SadConsole.Global.CurrentScreen;
        }

        public void checkResize(int newX, int newY) {
            this.Resize(newX, newY, false);
        }
        
        public void Init() {
            CreateConsoles();

            MessageLog = new MessageLogWindow(GameLoop.GameWidth / 2, GameLoop.GameHeight / 3, "Message Log");
            Children.Add(MessageLog);
            MessageLog.Show();
            MessageLog.Position = new Point(0, GameLoop.GameHeight / 2);
            

           // MessageLog.Add("Testing First");

            LoadMap(GameLoop.World.CurrentMap);

            CreateMapWindow(GameLoop.GameWidth / 2, GameLoop.GameHeight / 2, "Game Map");
            UseMouse = true;

            CreateMultiplayerWindow(GameLoop.GameWidth / 4, GameLoop.GameHeight / 2, "Multiplayer");

            CenterOnActor(GameLoop.World.Player);
        }

        public void CreateConsoles() {
            MapConsole = new ScrollingConsole(GameLoop.GameWidth, GameLoop.GameHeight);
            MultiConsole = new ScrollingConsole(GameLoop.GameWidth, GameLoop.GameHeight);
        } 

        public void CreateMapWindow(int width, int height, string title) {
            MapWindow = new Window(width, height);
            MapWindow.CanDrag = true;

            int mapConsoleWidth = width - 2;
            int mapConsoleHeight = height - 2;

            MapConsole.ViewPort = new Rectangle(0, 0, mapConsoleWidth, mapConsoleHeight);
            MapConsole.Position = new Point(1, 1);
            
            
            MapWindow.Title = title.Align(HorizontalAlignment.Center, mapConsoleWidth);
            MapWindow.Children.Add(MapConsole);

            Children.Add(MapWindow);

            MapConsole.Children.Add(GameLoop.World.Player);

            MapWindow.Show();
        }

        public void CreateMultiplayerWindow(int width, int height, string title) {
            MultiplayerWindow = new Window(width, height);
            MapWindow.CanDrag = true;

            int multiConsoleW = width - 2;
            int multiConsoleH = height - 2;

            MultiConsole.ViewPort = new Rectangle(0, 0, multiConsoleW, multiConsoleH);
            MultiConsole.Position = new Point(1, 1);

            Button closeButton = new Button(3, 1);
            closeButton.Position = new Point(0, 0);
            closeButton.Text = "[X]";

            Button hostButton = new Button(6, 1);
            hostButton.Position = new Point((multiConsoleW / 2) - 3, 3);
            hostButton.Text = "HOST";
            hostButton.MouseButtonClicked += hostButtonClick;

            Button joinButton = new Button(6, 1);
            joinButton.Position = new Point((multiConsoleW / 2) - 3, 5);
            joinButton.Text = "JOIN";
            joinButton.MouseButtonClicked += joinButtonClick;
            

            MultiplayerWindow.Add(closeButton);
            MultiplayerWindow.Add(hostButton);
            MultiplayerWindow.Add(joinButton);


            MultiplayerWindow.Title = title.Align(HorizontalAlignment.Center, multiConsoleW);

            Children.Add(MultiplayerWindow);
            MultiplayerWindow.Show();
            MultiplayerWindow.IsVisible = false;
        }

        private void lobbyTextBoxClicked(object sender, SadConsole.Input.MouseEventArgs e) {
            
        }

        private void hostButtonClick(object sender, SadConsole.Input.MouseEventArgs e) {
            System.Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", "0");
            GameLoop.discord = new Discord.Discord(579827348665532425, (UInt64) Discord.CreateFlags.Default);
            GameLoop.discord.RunCallbacks();


            var lobbyManager = GameLoop.discord.GetLobbyManager();
            var txn = lobbyManager.GetLobbyCreateTransaction();

            txn.SetCapacity(6);
            txn.SetType(Discord.LobbyType.Public);
            txn.SetMetadata("a", "123");

            lobbyManager.CreateLobby(txn, (Result result, ref Lobby lobby) => {
                MessageLog.Add("Created lobby! Code has been copied to clipboard, have client copy code and click JOIN.");
                TextCopy.Clipboard.SetText(lobbyManager.GetLobbyActivitySecret(lobby.Id));
               // lobbyActivSecret.Text = lobbyManager.GetLobbyActivitySecret(lobby.Id);

                lobbyManager.OnMemberConnect += onPlayerConnected;
            });
        }

        private void onPlayerConnected(long lobbyId, long userId) {
            GameLoop.discord.GetUserManager().GetUser(userId, (Result result, ref User user) => {
                if (result == Discord.Result.Ok) {
                    MessageLog.Add("User connected: " + user.Username);
                }
            });
        }

        private void joinButtonClick(object sender, SadConsole.Input.MouseEventArgs e) {
            System.Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", "1");
            GameLoop.discord = new Discord.Discord(579827348665532425, (UInt64)Discord.CreateFlags.Default);
            GameLoop.discord.RunCallbacks();

            var lobbyManager = GameLoop.discord.GetLobbyManager();
            lobbyManager.ConnectLobbyWithActivitySecret(TextCopy.Clipboard.GetText(), (Result result, ref Lobby lobby) =>
            {
                if (result == Discord.Result.Ok) {
                    MessageLog.Add("Connected to lobby successfully!");
                } else {
                    MessageLog.Add("Encountered error: " + result);
                }
            });
        }

        public void CenterOnActor(Actor actor) {
            MapConsole.CenterViewPortOnPoint(actor.Position);
        }

        public override void Update(TimeSpan timeElapsed) {
            CheckKeyboard();

            base.Update(timeElapsed);
        }

        private void SyncMapEntities(Map map) {
            MapConsole.Children.Clear();

            foreach (Entity entity in map.Entities.Items) {
                MapConsole.Children.Add(entity);
            }

            map.Entities.ItemAdded += OnMapEntityAdded; 
            map.Entities.ItemRemoved += OnMapEntityRemoved;
        }

        public void OnMapEntityAdded (object sender, GoRogue.ItemEventArgs<Entity> args) {
            MapConsole.Children.Add(args.Item);
        }

        public void OnMapEntityRemoved(object sender, GoRogue.ItemEventArgs<Entity> args) {
            MapConsole.Children.Remove(args.Item);
        }

        public void LoadMap(Map map) {
            MapConsole = new SadConsole.ScrollingConsole(GameLoop.World.CurrentMap.Width, GameLoop.World.CurrentMap.Height, Global.FontDefault, new Rectangle(0, 0, GameLoop.GameWidth, GameLoop.GameHeight), map.Tiles);
            SyncMapEntities(map);
        }

        private void CheckKeyboard() {
            if (Global.KeyboardState.IsKeyReleased(Keys.F5)) { Settings.ToggleFullScreen(); }

            if (Global.KeyboardState.IsKeyPressed(Keys.Up) || Global.KeyboardState.IsKeyPressed(Keys.NumPad8)) { GameLoop.CommandManager.MoveActorBy(GameLoop.World.Player, new Point(0, -1)); CenterOnActor(GameLoop.World.Player); }
            if (Global.KeyboardState.IsKeyPressed(Keys.Down) || Global.KeyboardState.IsKeyPressed(Keys.NumPad2)) { GameLoop.CommandManager.MoveActorBy(GameLoop.World.Player, new Point(0, 1)); CenterOnActor(GameLoop.World.Player); }
            if (Global.KeyboardState.IsKeyPressed(Keys.Left) || Global.KeyboardState.IsKeyPressed(Keys.NumPad4)) { GameLoop.CommandManager.MoveActorBy(GameLoop.World.Player, new Point(-1, 0)); CenterOnActor(GameLoop.World.Player); }
            if (Global.KeyboardState.IsKeyPressed(Keys.Right) || Global.KeyboardState.IsKeyPressed(Keys.NumPad6)) { GameLoop.CommandManager.MoveActorBy(GameLoop.World.Player, new Point(1, 0)); CenterOnActor(GameLoop.World.Player); }

            if (Global.KeyboardState.IsKeyReleased(Keys.Tab)) {
                if (GameLoop.UIManager.MultiplayerWindow.IsVisible)
                    GameLoop.UIManager.MultiplayerWindow.IsVisible = false;
                else
                    GameLoop.UIManager.MultiplayerWindow.IsVisible = true;
            }
        }
    }
}
