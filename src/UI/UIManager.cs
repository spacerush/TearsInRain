using System;
using Discord;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Input;
using TearsInRain.Entities;
using TearsInRain.Tiles;

namespace TearsInRain.UI {
    public class UIManager : ContainerConsole {
        public ScrollingConsole MapConsole;
        public ScrollingConsole MultiConsole;

        public Window MapWindow;
        public MessageLogWindow MessageLog;
        public ChatLogWindow ChatLog;
        public Window MultiplayerWindow;

        public Button hostButton;
        public Button joinButton;
        public Button copyButton;
        public Button testButton;

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

            ChatLog = new ChatLogWindow(GameLoop.GameWidth / 2, GameLoop.GameHeight / 3, "Chat Log");
            Children.Add(MessageLog);
            ChatLog.Show();
            ChatLog.IsVisible = false;
            ChatLog.Position = new Point(0, GameLoop.GameHeight / 2);


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
            closeButton.MouseButtonClicked += exitButtonClick;

            hostButton = new Button(6, 1);
            hostButton.Position = new Point((multiConsoleW / 2) - 3, 3);
            hostButton.Text = "HOST";
            hostButton.MouseButtonClicked += hostButtonClick;

            joinButton = new Button(6, 1);
            joinButton.Position = new Point((multiConsoleW / 2) - 3, 5);
            joinButton.Text = "JOIN";
            joinButton.MouseButtonClicked += joinButtonClick;

            copyButton = new Button(10, 1);
            copyButton.Position = new Point((multiConsoleW / 2) - 5, 3);
            copyButton.Text = "GET CODE";
            copyButton.MouseButtonClicked += copyButtonClick;
            copyButton.IsVisible = false;

            testButton = new Button(6, 1);
            testButton.Position = new Point((multiConsoleW / 2) - 3, 9);
            testButton.Text = "TEST";
            testButton.MouseButtonClicked += testButtonClick;

            MultiplayerWindow.Add(closeButton);
            MultiplayerWindow.Add(hostButton);
            MultiplayerWindow.Add(joinButton);
            MultiplayerWindow.Add(copyButton);
            MultiplayerWindow.Add(testButton);


            MultiplayerWindow.Title = title.Align(HorizontalAlignment.Center, multiConsoleW);

            Children.Add(MultiplayerWindow);
            MultiplayerWindow.Show();
            MultiplayerWindow.IsVisible = false;
        }

        private void testButtonClick(object sender, MouseEventArgs e) {
            var lobbyManager = GameLoop.NetworkingManager.discord.GetLobbyManager();
            if (lobbyManager != null) {
                GameLoop.NetworkingManager.SendNetMessage(2, System.Text.Encoding.UTF8.GetBytes("fuck"));
            }
        }

        private void copyButtonClick(object sender, MouseEventArgs e) {
            var lobbyManager = GameLoop.NetworkingManager.discord.GetLobbyManager();
            if (lobbyManager != null) {
                TextCopy.Clipboard.SetText(lobbyManager.GetLobbyActivitySecret(lobbyManager.GetLobbyId(0)));
            }
        }

        private void exitButtonClick(object sender, MouseEventArgs e) {
            MultiplayerWindow.IsVisible = false;
        }

        private void hostButtonClick(object sender, SadConsole.Input.MouseEventArgs e) {
            GameLoop.NetworkingManager.changeClientTarget("0"); // HAS TO BE DISABLED ON LIVE BUILD, ONLY FOR TESTING TWO CLIENTS ON ONE COMPUTER

            var lobbyManager = GameLoop.NetworkingManager.discord.GetLobbyManager();
            var txn = lobbyManager.GetLobbyCreateTransaction();

            txn.SetCapacity(6);
            txn.SetType(Discord.LobbyType.Public);
            txn.SetMetadata("a", "123");


            lobbyManager.CreateLobby(txn, (Result result, ref Lobby lobby) => {
                if (result == Result.Ok) {
                    MessageLog.Add("Created lobby! Code has been copied to clipboard.");
                    TextCopy.Clipboard.SetText(lobbyManager.GetLobbyActivitySecret(lobby.Id));

                    GameLoop.NetworkingManager.InitNetworking(lobby.Id);
                    lobbyManager.OnMemberConnect += onPlayerConnected;
                    lobbyManager.OnMemberDisconnect += onPlayerDisconnected;

                    hostButton.IsVisible = false;
                    joinButton.IsVisible = false;
                    copyButton.IsVisible = true;
                } else {
                    MessageLog.Add("Error: " + result);
                }
            });
        }

        private void onPlayerDisconnected(long lobbyId, long userId) {
            var userManager = GameLoop.NetworkingManager.discord.GetUserManager();
            userManager.GetUser(userId, (Result result, ref User user) => {
                if (result == Discord.Result.Ok) {
                    ChatLog.Add("User disconnected: " + user.Username);
                }
            });
        }

        private void onPlayerConnected(long lobbyId, long userId) {
            var userManager = GameLoop.NetworkingManager.discord.GetUserManager();
            userManager.GetUser(userId, (Result result, ref User user) => {
                if (result == Discord.Result.Ok) {
                    ChatLog.Add("User connected: " + user.Username);
                    kickstartNet();
                }
            });
        }

        private void kickstartNet() {
            GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes("a"));
            GameLoop.NetworkingManager.SendNetMessage(1, System.Text.Encoding.UTF8.GetBytes("a"));
            GameLoop.NetworkingManager.SendNetMessage(2, System.Text.Encoding.UTF8.GetBytes("a"));
        }

        private void joinButtonClick(object sender, SadConsole.Input.MouseEventArgs e) {
            GameLoop.NetworkingManager.changeClientTarget("1"); // HAS TO BE DISABLED ON LIVE BUILD, ONLY FOR TESTING TWO CLIENTS ON ONE COMPUTER


            var lobbyManager = GameLoop.NetworkingManager.discord.GetLobbyManager();
            lobbyManager.ConnectLobbyWithActivitySecret(TextCopy.Clipboard.GetText(), (Result result, ref Lobby lobby) => {
                if (result == Discord.Result.Ok) {
                    MessageLog.Add("Connected to lobby successfully!");
                    GameLoop.NetworkingManager.InitNetworking(lobby.Id);
                    kickstartNet();
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

            foreach (TIREntity entity in map.Entities.Items) {
                MapConsole.Children.Add(entity);
            }

            map.Entities.ItemAdded += OnMapEntityAdded; 
            map.Entities.ItemRemoved += OnMapEntityRemoved;
        }

        public void OnMapEntityAdded (object sender, GoRogue.ItemEventArgs<TIREntity> args) {
            MapConsole.Children.Add(args.Item);
        }

        public void OnMapEntityRemoved(object sender, GoRogue.ItemEventArgs<TIREntity> args) {
            MapConsole.Children.Remove(args.Item);
        }

        public void LoadMap(Map map) {
            TileBase[] mapArr = new TileBase[map._TileDict.Count];
            map._TileDict.Values.CopyTo(mapArr, 0);
            MapConsole = new SadConsole.ScrollingConsole(GameLoop.World.CurrentMap.Width, GameLoop.World.CurrentMap.Height, Global.FontDefault, new Rectangle(0, 0, GameLoop.GameWidth, GameLoop.GameHeight), mapArr);
            SyncMapEntities(map);
        }

        private void CheckKeyboard() {
            if (!ChatLog.TextBoxFocused()) {
                if (Global.KeyboardState.IsKeyReleased(Keys.F5)) { Settings.ToggleFullScreen(); }

                if (Global.KeyboardState.IsKeyPressed(Keys.Up) || Global.KeyboardState.IsKeyPressed(Keys.NumPad8)) { GameLoop.CommandManager.MoveActorBy(GameLoop.World.Player, new Point(0, -1)); CenterOnActor(GameLoop.World.Player); }
                if (Global.KeyboardState.IsKeyPressed(Keys.Down) || Global.KeyboardState.IsKeyPressed(Keys.NumPad2)) { GameLoop.CommandManager.MoveActorBy(GameLoop.World.Player, new Point(0, 1)); CenterOnActor(GameLoop.World.Player); }
                if (Global.KeyboardState.IsKeyPressed(Keys.Left) || Global.KeyboardState.IsKeyPressed(Keys.NumPad4)) { GameLoop.CommandManager.MoveActorBy(GameLoop.World.Player, new Point(-1, 0)); CenterOnActor(GameLoop.World.Player); }
                if (Global.KeyboardState.IsKeyPressed(Keys.Right) || Global.KeyboardState.IsKeyPressed(Keys.NumPad6)) { GameLoop.CommandManager.MoveActorBy(GameLoop.World.Player, new Point(1, 0)); CenterOnActor(GameLoop.World.Player); }

                if (Global.KeyboardState.IsKeyReleased(Keys.Tab)) {
                    if (MultiplayerWindow.IsVisible)
                        MultiplayerWindow.IsVisible = false;
                    else
                        MultiplayerWindow.IsVisible = true;
                }


                if (Global.KeyboardState.IsKeyReleased(Keys.C)) {
                    if (ChatLog.IsVisible)
                        ChatLog.IsVisible = false;
                    else
                        ChatLog.IsVisible = true;
                }
            } else {
                if (Global.KeyboardState.IsKeyReleased(Keys.Escape)) { ChatLog.Unfocus(); }
                if (Global.KeyboardState.IsKeyReleased(Keys.Enter) && GameLoop.NetworkingManager.discord.GetLobbyManager() != null) {
                    if (ChatLog.GetText() != "") {
                        var assembled = GameLoop.NetworkingManager.userManager.GetCurrentUser().Username + ": " + ChatLog.GetText();

                        GameLoop.NetworkingManager.SendNetMessage(1, System.Text.Encoding.UTF8.GetBytes(assembled));

                        ChatLog.Add(assembled);
                        ChatLog.ClearText();
                        
                    } else {
                        ChatLog.Refocus();
                    }
                }
            }
        }
    }
}
