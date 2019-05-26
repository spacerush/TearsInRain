using System;
using System.Collections.Generic;
using Discord;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input; 
using SadConsole;
using SadConsole.Controls;
using SadConsole.Input;
using TearsInRain.Entities; 
using Utils = TearsInRain.Utils;

namespace TearsInRain.UI {
    public class UIManager : ContainerConsole {
        public ScrollingConsole MapConsole;
        public ScrollingConsole MultiConsole;

        public Window MapWindow;
        public MessageLogWindow MessageLog;
        public ChatLogWindow ChatLog;
        public Window MultiplayerWindow;

        public Button hostButton;
        public Button closeButton;
        public Button joinButton;
        public Button copyButton;
        public Button testButton;


        public bool chat = false;
        public long tempUID = 0;

        public string waitingForCommand = "";

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

            MessageLog = new MessageLogWindow(70, GameLoop.GameHeight / 3, "[Message Log]");
            MessageLog.Title.Align(HorizontalAlignment.Center, MessageLog.Title.Length); 
            Children.Add(MessageLog);
            MessageLog.Show();
            MessageLog.Position = new Point(0, GameLoop.GameHeight / 2);

            ChatLog = new ChatLogWindow(70, GameLoop.GameHeight / 3, "[Chat Log]");
            Children.Add(MessageLog);
            ChatLog.Show();
            ChatLog.IsVisible = false;
            ChatLog.Position = new Point((GameLoop.GameWidth / 2) - (ChatLog.Width / 2), (GameLoop.GameHeight / 2) - (ChatLog.Height / 2));


            // MessageLog.Add("Testing First");

            LoadMap(GameLoop.World.CurrentMap);

            CreateMapWindow(70, GameLoop.GameHeight / 2,"[Game Map]");
            UseMouse = true;

            CreateMultiplayerWindow(GameLoop.GameWidth / 4, GameLoop.GameHeight / 2, "[Multiplayer]");

            CenterOnActor(GameLoop.World.players[GameLoop.NetworkingManager.myUID]);
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
            
            
            MapWindow.Title = title.Align(HorizontalAlignment.Center, mapConsoleWidth, '-');
            MapWindow.Children.Add(MapConsole);

            Children.Add(MapWindow);

            MapConsole.Children.Add(GameLoop.World.players[GameLoop.NetworkingManager.myUID]);

            MapWindow.Show();
        }

        public void CreateMultiplayerWindow(int width, int height, string title) {
            MultiplayerWindow = new Window(width, height);
            MapWindow.CanDrag = true;

            int multiConsoleW = width - 2;
            int multiConsoleH = height - 2;

            MultiConsole.ViewPort = new Rectangle(0, 0, multiConsoleW, multiConsoleH);
            MultiConsole.Position = new Point(1, 1);

            closeButton = new Button(3, 1);
            closeButton.Position = new Point(1, 1);
            closeButton.Text = "X";
            closeButton.MouseButtonClicked += closeButtonClick;

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


            MultiplayerWindow.Title = title.Align(HorizontalAlignment.Center, multiConsoleW, '-');

            Children.Add(MultiplayerWindow);
            MultiplayerWindow.Show();
            MultiplayerWindow.IsVisible = false;
            MultiplayerWindow.Position = new Point((GameLoop.GameWidth / 2) - (MultiplayerWindow.Width/2), (GameLoop.GameHeight / 2) - (MultiplayerWindow.Height/2));
        }

        private void closeButtonClick(object sender, MouseEventArgs e) {
            MultiplayerWindow.IsVisible = false;
        }

        private void testButtonClick(object sender, MouseEventArgs e) {
            var lobbyManager = GameLoop.NetworkingManager.discord.GetLobbyManager();
            if (lobbyManager != null) {
                GameLoop.NetworkingManager.SendNetMessage(2, System.Text.Encoding.UTF8.GetBytes(Utils.SimpleMapString(GameLoop.World.CurrentMap.Tiles)));
            }
        }

        private void copyButtonClick(object sender, MouseEventArgs e) {
            var lobbyManager = GameLoop.NetworkingManager.discord.GetLobbyManager();
            if (lobbyManager != null) {
                TextCopy.Clipboard.SetText(lobbyManager.GetLobbyActivitySecret(lobbyManager.GetLobbyId(0)));
            }
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

                    ChatLog.IsVisible = true;
                    MultiplayerWindow.IsVisible = false;
                    chat = true;
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
                    GameLoop.NetworkingManager.SendNetMessage(2, System.Text.Encoding.UTF8.GetBytes(Utils.SimpleMapString(GameLoop.World.CurrentMap.Tiles)));

                    var playerList = "p_list";

                    GameLoop.World.CreatePlayer(userId);

                    foreach(KeyValuePair<long, Player> player in GameLoop.World.players) {
                        playerList += "|" + player.Value;
                    }
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
                    GameLoop.World.CreatePlayer(GameLoop.NetworkingManager.myUID);
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

        private void ClearWait() {
            waitingForCommand = "";
        }

        private void CheckKeyboard() {
            if (Global.KeyboardState.IsKeyReleased(Keys.Tab)) {
                if (!chat) {
                    if (MultiplayerWindow.IsVisible)
                        MultiplayerWindow.IsVisible = false;
                    else {
                        MultiplayerWindow.Position = new Point((GameLoop.GameWidth / 2) - (MultiplayerWindow.Width / 2), (GameLoop.GameHeight / 2) - (MultiplayerWindow.Height / 2));
                        MultiplayerWindow.IsVisible = true;
                    }
                } else {
                    if (ChatLog.IsVisible)
                        ChatLog.IsVisible = false;
                    else {
                        ChatLog.Position = new Point((GameLoop.GameWidth / 2) - (ChatLog.Width / 2), (GameLoop.GameHeight / 2) - (ChatLog.Height / 2));
                        ChatLog.IsVisible = true;
                    }
                }
            }

            if (!ChatLog.TextBoxFocused()) {
                if (Global.KeyboardState.IsKeyReleased(Keys.F5)) { Settings.ToggleFullScreen(); }

                if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                    Player player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];


                    if (Global.KeyboardState.IsKeyPressed(Keys.C)) {
                        waitingForCommand = "c";
                    }

                    if (Global.KeyboardState.IsKeyPressed(Keys.G)) {
                        waitingForCommand = "g";
                    }


                    if (Global.KeyboardState.IsKeyReleased(Keys.Escape)) {
                        if (waitingForCommand != "")
                            ClearWait();
                    }


                    if (player.TimeLastActed + (UInt64) player.MoveCost <= GameLoop.GameTime) {
                        if (Global.KeyboardState.IsKeyPressed(Keys.Up) || Global.KeyboardState.IsKeyPressed(Keys.NumPad9)) {
                            if (waitingForCommand == "") {
                                GameLoop.CommandManager.MoveActorBy(player, Utils.Directions["UR"]);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait();
                                GameLoop.CommandManager.CloseDoor(player, player.Position + Utils.Directions["UR"]);
                            } else if (waitingForCommand == "g") {
                                ClearWait();
                                GameLoop.CommandManager.Pickup(player, player.Position + Utils.Directions["UR"]);
                            }

                        }

                        if (Global.KeyboardState.IsKeyPressed(Keys.Up) || Global.KeyboardState.IsKeyPressed(Keys.NumPad8)) {
                            if (waitingForCommand == "") {
                                GameLoop.CommandManager.MoveActorBy(player, Utils.Directions["U"]);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait();
                                GameLoop.CommandManager.CloseDoor(player, player.Position + Utils.Directions["U"]);
                            } else if (waitingForCommand == "g") {
                                ClearWait();
                                GameLoop.CommandManager.Pickup(player, player.Position + Utils.Directions["U"]);
                            }

                        }

                        if (Global.KeyboardState.IsKeyPressed(Keys.Up) || Global.KeyboardState.IsKeyPressed(Keys.NumPad7)) {
                            if (waitingForCommand == "") {
                                GameLoop.CommandManager.MoveActorBy(player, Utils.Directions["UL"]);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait();
                                GameLoop.CommandManager.CloseDoor(player, player.Position + Utils.Directions["UL"]);
                            } else if (waitingForCommand == "g") {
                                ClearWait();
                                GameLoop.CommandManager.Pickup(player, player.Position + Utils.Directions["UL"]);
                            }

                        }

                        if (Global.KeyboardState.IsKeyPressed(Keys.Right) || Global.KeyboardState.IsKeyPressed(Keys.NumPad6)) {
                            if (waitingForCommand == "") {
                                GameLoop.CommandManager.MoveActorBy(player, Utils.Directions["R"]);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait();
                                GameLoop.CommandManager.CloseDoor(player, player.Position + Utils.Directions["R"]);
                            } else if (waitingForCommand == "g") {
                                ClearWait();
                                GameLoop.CommandManager.Pickup(player, player.Position + Utils.Directions["R"]);
                            }
                        }

                        if (Global.KeyboardState.IsKeyPressed(Keys.NumPad5)) {
                            if (waitingForCommand == "c") {
                                ClearWait();
                                GameLoop.CommandManager.CloseDoor(player, player.Position);
                            } else if (waitingForCommand == "g") {
                                ClearWait();
                                GameLoop.CommandManager.Pickup(player, player.Position + Utils.Directions["R"]);
                            }
                        }

                        if (Global.KeyboardState.IsKeyPressed(Keys.Left) || Global.KeyboardState.IsKeyPressed(Keys.NumPad4)) {
                            if (waitingForCommand == "") {
                                GameLoop.CommandManager.MoveActorBy(player, Utils.Directions["L"]);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait();
                                GameLoop.CommandManager.CloseDoor(player, player.Position + Utils.Directions["L"]);
                            } else if (waitingForCommand == "g") {
                                ClearWait();
                                GameLoop.CommandManager.Pickup(player, player.Position + Utils.Directions["L"]);
                            }
                        }

                        if (Global.KeyboardState.IsKeyPressed(Keys.Left) || Global.KeyboardState.IsKeyPressed(Keys.NumPad3)) {
                            if (waitingForCommand == "") {
                                GameLoop.CommandManager.MoveActorBy(player, Utils.Directions["DR"]);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait();
                                GameLoop.CommandManager.CloseDoor(player, player.Position + Utils.Directions["DR"]);
                            } else if (waitingForCommand == "g") {
                                ClearWait();
                                GameLoop.CommandManager.Pickup(player, player.Position + Utils.Directions["DR"]);
                            }
                        }

                        if (Global.KeyboardState.IsKeyPressed(Keys.Down) || Global.KeyboardState.IsKeyPressed(Keys.NumPad2)) {
                            if (waitingForCommand == "") {
                                GameLoop.CommandManager.MoveActorBy(player, Utils.Directions["D"]);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait();
                                GameLoop.CommandManager.CloseDoor(player, player.Position + Utils.Directions["D"]);
                            } else if (waitingForCommand == "g") {
                                ClearWait();
                                GameLoop.CommandManager.Pickup(player, player.Position + Utils.Directions["D"]);
                            }
                        }

                        if (Global.KeyboardState.IsKeyPressed(Keys.Left) || Global.KeyboardState.IsKeyPressed(Keys.NumPad1)) {
                            if (waitingForCommand == "") {
                                GameLoop.CommandManager.MoveActorBy(player, Utils.Directions["DL"]);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait();
                                GameLoop.CommandManager.CloseDoor(player, player.Position + Utils.Directions["DL"]);
                            } else if (waitingForCommand == "g") {
                                ClearWait();
                                GameLoop.CommandManager.Pickup(player, player.Position + Utils.Directions["DL"]);
                            }
                        }
                    }
                }
            } else {

                if (Global.KeyboardState.IsKeyReleased(Keys.Escape)) {
                    if (waitingForCommand == "")
                        ChatLog.Unfocus();
                    else
                        ClearWait();
                }
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
