using System;
using System.Collections.Generic;
using Discord;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using RogueSharp.DiceNotation;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Input;
using TearsInRain.Entities;
using TearsInRain.Serializers;
using TearsInRain.Tiles;
using Console = SadConsole.Console;
using Utils = TearsInRain.Utils;

namespace TearsInRain.UI {
    public class UIManager : ContainerConsole {
        public ScrollingConsole MapConsole;
        public ScrollingConsole MultiConsole;
        public Console StatusConsole;
        public Console InventoryConsole;
        
        public Window MapWindow;
        public MessageLogWindow MessageLog;
        public ChatLogWindow ChatLog;
        public Window MultiplayerWindow;
        public Window StatusWindow;
        public Window InventoryWindow;

        public Window ContextWindow;
        public Console ContextConsole;

        public Button hostButton;
        public Button closeButton;
        public Button joinButton;
        public Button copyButton;
        public Button testButton;




        public bool chat = false;
        public long tempUID = 0;
        public int invContextIndex = -1;

        public string waitingForCommand = "";
        public Point viewOffset = new Point(0, 0);

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

            MessageLog = new MessageLogWindow(70, (GameLoop.GameHeight / 3), "[MESSAGE LOG]");
            MessageLog.Title.Align(HorizontalAlignment.Center, MessageLog.Title.Length); 
            Children.Add(MessageLog);
            MessageLog.Show();
            MessageLog.Position = new Point(0, (GameLoop.GameHeight / 3) * 2);

            ChatLog = new ChatLogWindow(70, GameLoop.GameHeight / 3, "[CHAT LOG]");
            Children.Add(MessageLog);
            ChatLog.Show();
            ChatLog.IsVisible = false;
            ChatLog.Position = new Point((GameLoop.GameWidth / 2) - (ChatLog.Width / 2), (GameLoop.GameHeight / 2) - (ChatLog.Height / 2));
            
            // MessageLog.Add("Testing First");

            LoadMap(GameLoop.World.CurrentMap);

            CreateMapWindow(70, (GameLoop.GameHeight / 3) * 2,"[GAME MAP]");
            UseMouse = true;

            CreateMultiplayerWindow(GameLoop.GameWidth / 4, GameLoop.GameHeight / 2, "[MULTIPLAYER]");

            CreateStatusWindow(30, (GameLoop.GameHeight/3)*2 , "[PLAYER INFO]");

            CreateContextWindow(30, GameLoop.GameHeight / 3, "[ITEM MENU]");

            ContextConsole.MouseButtonClicked += contextClick;

            CreateInventoryWindow(59, 28, "[INVENTORY]");

            CenterOnActor(GameLoop.World.players[GameLoop.NetworkingManager.myUID]);
            
            
        }
        public void CreateConsoles() {
            MapConsole = new ScrollingConsole(GameLoop.GameWidth, GameLoop.GameHeight);  
            MultiConsole = new ScrollingConsole(GameLoop.GameWidth, GameLoop.GameHeight);
            StatusConsole = new Console(GameLoop.GameWidth, GameLoop.GameHeight);
            InventoryConsole = new Console(GameLoop.GameWidth, GameLoop.GameHeight);
            ContextConsole = new Console(GameLoop.GameWidth, GameLoop.GameHeight);
        }


        public void CreateStatusWindow(int width, int height, string title) {
            StatusWindow = new Window(width, height);

            int statusConsoleWidth = width - 2;
            int statusConsoleHeight = height - 2;

            StatusConsole.Position = new Point(1, 1);

            StatusWindow.Title = title.Align(HorizontalAlignment.Center, statusConsoleWidth, '-');
            StatusWindow.Children.Add(StatusConsole);
            StatusWindow.Position = new Point(70, 0);

            Children.Add(StatusWindow);

            StatusWindow.CanDrag = true;
            StatusWindow.Show();
        }

        public void CreateContextWindow(int width, int height, string title) {
            ContextWindow = new Window(width, height);

            int contextConsoleWidth = width - 2;
            int contextConsoleHeight = height - 2;

            ContextConsole.Position = new Point(1, 1);

            ContextWindow.Title = title.Align(HorizontalAlignment.Center, contextConsoleWidth, '-');
            ContextWindow.Children.Add(ContextConsole);
            ContextWindow.Position = new Point(70, GameLoop.GameHeight / 3 * 2);
            
            Children.Add(ContextWindow);

            ContextWindow.CanDrag = true;
            ContextWindow.Show();
        }

        public void UpdateContextWindow() {
            ContextConsole.Clear();
            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                Player player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];
                 
                if (player.Inventory.Count >= invContextIndex + 1 && invContextIndex >= 0) {
                    Item item = player.Inventory[invContextIndex];

                    ContextConsole.Print(0, 0, item.Name);
                    ContextConsole.Print(0, 1, "-".Align(HorizontalAlignment.Center, 28, '-'));

                    ContextConsole.Print(0, 2, "Drop".Align(HorizontalAlignment.Center, 28, ' '));
                } else {
                    ContextConsole.Print(0, 0, "No Item Selected");
                    ContextConsole.Print(0, 1, "-".Align(HorizontalAlignment.Center, 28, '-'));
                }
            }
        }

        private void contextClick(object sender, MouseEventArgs e) {
            if (e.MouseState.Mouse.LeftClicked) {
                if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                    Player player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];

                    if (invContextIndex != -1 && invContextIndex < player.Inventory.Count) {
                        if (e.MouseState.ConsoleCellPosition.Y != 99) {
                            System.Console.WriteLine("RIGHT Y");
                            player.DropItem(invContextIndex);
                            invContextIndex = -1;
                        }
                    }
                }
            }
        }

        public void UpdateStatusWindow() { 
            StatusConsole.Clear();

            TimeManager time = GameLoop.TimeManager;

            ColoredString season = time.ColoredSeason();
            ColoredString timeString = new ColoredString(time.GetTimeString(), Color.White, Color.TransparentBlack);
            string dayYear = time.Day.ToString();
            if (time.Day < 10) { dayYear = " " + dayYear; }

            dayYear += ", Year " + time.Year.ToString();

            StatusConsole.Print(1, 1, season);
            StatusConsole.Print(1 + season.Count, 1, dayYear);
            StatusConsole.Print(22, 1, timeString);
            StatusConsole.Print(0, 3, "----------------------------");
        }

        public void CreateInventoryWindow(int width, int height, string title) {
            InventoryWindow = new Window(width, height);

            int invConsoleW = width - 2;
            int invConsoleH = height - 2;

            InventoryConsole.Position = new Point(1, 1);


            InventoryWindow.Title = title.Align(HorizontalAlignment.Center, invConsoleW, '-');
            InventoryWindow.Children.Add(InventoryConsole);
            InventoryWindow.Position = new Point((GameLoop.GameWidth/2) - InventoryWindow.Width/2, (GameLoop.GameHeight/2) - InventoryWindow.Height/2);

            Children.Add(InventoryWindow);

            InventoryWindow.CanDrag = true;
            InventoryWindow.Show();
            InventoryWindow.IsVisible = false;
            InventoryWindow.FocusOnMouseClick = true;

            UpdateInventory();




            InventoryConsole.MouseButtonClicked += invMouseClick;
        }




        private void invMouseClick(object sender, MouseEventArgs e) {
            if (e.MouseState.Mouse.LeftClicked) {
                if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                    Player player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];
                    if (player.Inventory.Count >= e.MouseState.ConsoleCellPosition.Y - 2 && e.MouseState.ConsoleCellPosition.Y - 2 >= 0) {
                        invContextIndex = e.MouseState.ConsoleCellPosition.Y - 2;
                    }
                }
            }
        } 

        public void UpdateInventory() {
            InventoryConsole.Clear();
            
            InventoryConsole.Print(29, 0, "Item Name | QTY | WEIGHT");
            InventoryConsole.Print(0, 1, "---------------------------------------------------------");

            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                Player player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];
                for (int i = 0; i < player.Inventory.Count; i++) {
                    InventoryConsole.Print(38 - player.Inventory[i].Name.Length, i+2, player.Inventory[i].Name);

                    string qty = player.Inventory[i].Quantity.ToString();
                    if (player.Inventory[i].Quantity < 100) { qty = " " + qty; }
                    if (player.Inventory[i].Quantity < 10) { qty = " " + qty; }

                    var space = "";

                    string wgt = string.Format("{0:N2}", player.Inventory[i].StackWeight());
                    //string wgt = player.Inventory[i].StackWeight().ToString();
                    ColoredString weight = new ColoredString(string.Format("{0:N2}", player.Inventory[i].StackWeight(), Color.White, Color.Transparent));
                    if (player.Inventory[i].StackWeight() < 100) { space = " "; }
                    if (player.Inventory[i].StackWeight() < 10) { space = "  "; }
                    if (weight[weight.Count-1].Glyph == '0') { weight[weight.Count - 1].Foreground = Color.DarkSlateGray; }
                    if (weight[weight.Count - 2].Glyph == '0' && weight[weight.Count - 1].Glyph == '0') { weight[weight.Count - 2].Foreground = Color.DarkSlateGray; weight[weight.Count - 3].Foreground = Color.DarkSlateGray; }

                    InventoryConsole.Print(39, i + 2, "| " + qty + " | " + space + weight);
                    InventoryConsole.Print(54, i + 2, "kg");
                }
            }
            

        }

        public void CreateMapWindow(int width, int height, string title) {
            MapWindow = new Window(width, height);

            int mapConsoleWidth = width - 2;
            int mapConsoleHeight = height - 2;

            MapConsole.ViewPort = new Rectangle(0, 0, mapConsoleWidth, mapConsoleHeight);
            MapConsole.Position = new Point(1, 1);
            
            
            MapWindow.Title = title.Align(HorizontalAlignment.Center, mapConsoleWidth, '-');
            MapWindow.Children.Add(MapConsole);

            Children.Add(MapWindow);

            MapConsole.Children.Add(GameLoop.World.players[GameLoop.NetworkingManager.myUID]);

            MapWindow.CanDrag = true;
            MapWindow.Show();

            
        }

        public void CreateMultiplayerWindow(int width, int height, string title) {
            MultiplayerWindow = new Window(width, height);
            MultiplayerWindow.CanDrag = true;

            int multiConsoleW = width - 2;
            int multiConsoleH = height - 2;

            int center = multiConsoleW / 2;

            MultiConsole.ViewPort = new Rectangle(0, 0, multiConsoleW, multiConsoleH);
            MultiConsole.Position = new Point(1, 1);

            closeButton = new Button(3, 1);
            closeButton.Position = new Point(1, 1);
            closeButton.Text = "X";
            closeButton.MouseButtonClicked += closeButtonClick;

            hostButton = new Button(6, 1);
            hostButton.Position = new Point(center - (hostButton.Width - 2)/2, 3);
            hostButton.Text = "HOST";
            hostButton.MouseButtonClicked += hostButtonClick;

            joinButton = new Button(6, 1);
            joinButton.Position = new Point(center - (joinButton.Width - 2)/2, 5);
            joinButton.Text = "JOIN";
            joinButton.MouseButtonClicked += joinButtonClick;

            copyButton = new Button(10, 1);
            copyButton.Position = new Point(center - (copyButton.Width - 2)/2, 3);
            copyButton.Text = "GET CODE";
            copyButton.MouseButtonClicked += copyButtonClick;
            copyButton.IsVisible = false;

            testButton = new Button(6, 1);
            testButton.Position = new Point(center - (testButton.Width - 2)/2, 9);
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
                    GameLoop.World.CurrentMap.Remove(GameLoop.World.players[user.Id]);
                    GameLoop.World.players.Remove(user.Id);
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
                    foreach (KeyValuePair<long, Player> player in GameLoop.World.players) {
                        playerList += "|" + player.Key + ";" + player.Value.Position.X + ";" + player.Value.Position.Y + ";" + player.Value.Animation.CurrentFrame[0].Foreground.A;
                    } 
                    GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(playerList));
                    
                    var monsterList = "m_list"; 
                    foreach (Entity entity in GameLoop.World.CurrentMap.Entities.Items) {
                        if (entity is Monster) {
                            monsterList += "|" + JsonConvert.SerializeObject((Actor) entity, Formatting.Indented, new ActorJsonConverter()) + "~" + entity.Position.X + "~" + entity.Position.Y;
                        }
                    } 
                    GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(monsterList));

                    var timeString = "time|" + GameLoop.TimeManager.Year + "|" + GameLoop.TimeManager.Season + "|" + GameLoop.TimeManager.Day + "|" + GameLoop.TimeManager.Hour + "|" + GameLoop.TimeManager.Minute;
                    GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(timeString));




                    MapConsole.IsDirty = true;
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
            GameLoop.World.CalculateFov(GameLoop.CommandManager.lastPeek);
            UpdateStatusWindow();
            UpdateContextWindow();
        }

        public void SyncMapEntities(Map map) {
            MapConsole.Children.Clear();

            foreach (Entity entity in map.Entities.Items) {
                MapConsole.Children.Add(entity);
            }

            foreach(KeyValuePair<long, Player> player in GameLoop.World.players) {
                MapConsole.Children.Add(player.Value);
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

        private void ClearWait(Actor actor) {
            waitingForCommand = "";
            GameLoop.CommandManager.ResetPeek(actor);
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
                    if (Global.KeyboardState.IsKeyPressed(Keys.G)) {
                        waitingForCommand = "g";
                    }

                    if (Global.KeyboardState.IsKeyPressed(Keys.X)) {
                        if (GameLoop.CommandManager.lastPeek == new Point(0, 0)) {
                            waitingForCommand = "x";
                        } else {
                            ClearWait(player);
                        }
                    }

                    if (Global.KeyboardState.IsKeyPressed(Keys.C) && Global.KeyboardState.IsKeyDown(Keys.LeftShift)) {
                        waitingForCommand = "c";
                    }

                    if (Global.KeyboardState.IsKeyPressed(Keys.I)) {
                        if (InventoryWindow.IsVisible) {
                            InventoryWindow.IsVisible = false;
                        } else {
                            InventoryWindow.IsVisible = true;
                        }
                    }

                    if (Global.KeyboardState.IsKeyPressed(Keys.S) && Global.KeyboardState.IsKeyDown(Keys.LeftShift)) {
                        if (!player.IsStealthing) {
                            int skillCheck = Dice.Roll("3d6");
                            player.Stealth(skillCheck, true);
                            GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes("stealth|yes|" + GameLoop.NetworkingManager.myUID + "|" + skillCheck));
                        } else {
                            player.Unstealth();
                            GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes("stealth|no|" + GameLoop.NetworkingManager.myUID + "|0"));
                        }
                    }


                    if (Global.KeyboardState.IsKeyReleased(Keys.Escape)) {
                        if (waitingForCommand != "")
                            ClearWait(player);
                    }


                    if (player.TimeLastActed + (UInt64) player.Speed <= GameLoop.GameTime) {
                        if (Global.KeyboardState.IsKeyPressed(Keys.E) || Global.KeyboardState.IsKeyPressed(Keys.NumPad9)) {
                            Point thisDir = Utils.Directions["UR"];
                            if (waitingForCommand == "") {
                                ClearWait(player);
                                GameLoop.CommandManager.MoveActorBy(player, thisDir);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait(player);
                                GameLoop.CommandManager.CloseDoor(player, thisDir);
                            } else if (waitingForCommand == "g") {
                                ClearWait(player);
                                GameLoop.CommandManager.Pickup(player, thisDir);
                            } else if (waitingForCommand == "x") {
                                ClearWait(player);
                                GameLoop.CommandManager.Peek(player, thisDir);
                            }

                        }

                        else if (Global.KeyboardState.IsKeyPressed(Keys.W) || Global.KeyboardState.IsKeyPressed(Keys.NumPad8)) {
                            Point thisDir = Utils.Directions["U"];
                            if (waitingForCommand == "") {
                                ClearWait(player);
                                GameLoop.CommandManager.MoveActorBy(player, thisDir);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait(player);
                                GameLoop.CommandManager.CloseDoor(player, thisDir);
                            } else if (waitingForCommand == "g") {
                                ClearWait(player);
                                GameLoop.CommandManager.Pickup(player, thisDir);
                            } else if (waitingForCommand == "x") {
                                ClearWait(player);
                                GameLoop.CommandManager.Peek(player, thisDir);
                            }

                        } else if (Global.KeyboardState.IsKeyPressed(Keys.Q) || Global.KeyboardState.IsKeyPressed(Keys.NumPad7)) {
                            Point thisDir = Utils.Directions["UL"];
                            if (waitingForCommand == "") {
                                ClearWait(player);
                                GameLoop.CommandManager.MoveActorBy(player, thisDir);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait(player);
                                GameLoop.CommandManager.CloseDoor(player, thisDir);
                            } else if (waitingForCommand == "g") {
                                ClearWait(player);
                                GameLoop.CommandManager.Pickup(player, thisDir);
                            } else if (waitingForCommand == "x") {
                                ClearWait(player);
                                GameLoop.CommandManager.Peek(player, thisDir);
                            }

                        } else if (Global.KeyboardState.IsKeyPressed(Keys.D) || Global.KeyboardState.IsKeyPressed(Keys.NumPad6)) {
                            Point thisDir = Utils.Directions["R"];
                            if (waitingForCommand == "") {
                                ClearWait(player);
                                GameLoop.CommandManager.MoveActorBy(player, thisDir);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait(player);
                                GameLoop.CommandManager.CloseDoor(player, thisDir);
                            } else if (waitingForCommand == "g") {
                                ClearWait(player);
                                GameLoop.CommandManager.Pickup(player, thisDir);
                            } else if (waitingForCommand == "x") {
                                ClearWait(player);
                                GameLoop.CommandManager.Peek(player, thisDir);
                            }
                        } else if (Global.KeyboardState.IsKeyPressed(Keys.NumPad5)) {
                            Point thisDir = Utils.Directions["C"];
                            if (waitingForCommand == "") {
                                ClearWait(player);
                                GameLoop.CommandManager.MoveActorBy(player, thisDir);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait(player);
                                GameLoop.CommandManager.CloseDoor(player, thisDir);
                            } else if (waitingForCommand == "g") {
                                ClearWait(player);
                                GameLoop.CommandManager.Pickup(player, thisDir);
                            } else if (waitingForCommand == "x") {
                                ClearWait(player);
                                GameLoop.CommandManager.Peek(player, thisDir);
                            }
                        } else if (Global.KeyboardState.IsKeyPressed(Keys.A) || Global.KeyboardState.IsKeyPressed(Keys.NumPad4)) {
                            Point thisDir = Utils.Directions["L"];
                            if (waitingForCommand == "") {
                                ClearWait(player);
                                GameLoop.CommandManager.MoveActorBy(player, thisDir);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait(player);
                                GameLoop.CommandManager.CloseDoor(player, thisDir);
                            } else if (waitingForCommand == "g") {
                                ClearWait(player);
                                GameLoop.CommandManager.Pickup(player, thisDir);
                            } else if (waitingForCommand == "x") {
                                ClearWait(player);
                                GameLoop.CommandManager.Peek(player, thisDir);
                            }
                        } else if ((Global.KeyboardState.IsKeyPressed(Keys.C) && !Global.KeyboardState.IsKeyDown(Keys.LeftShift)) || Global.KeyboardState.IsKeyPressed(Keys.NumPad3)) {
                            Point thisDir = Utils.Directions["DR"];
                            if (waitingForCommand == "") {
                                ClearWait(player);
                                GameLoop.CommandManager.MoveActorBy(player, thisDir);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait(player);
                                GameLoop.CommandManager.CloseDoor(player, thisDir);
                            } else if (waitingForCommand == "g") {
                                ClearWait(player);
                                GameLoop.CommandManager.Pickup(player, thisDir);
                            } else if (waitingForCommand == "x") {
                                ClearWait(player);
                                GameLoop.CommandManager.Peek(player, thisDir);
                            }
                        } else if ((Global.KeyboardState.IsKeyPressed(Keys.S) && !Global.KeyboardState.IsKeyDown(Keys.LeftShift)) || Global.KeyboardState.IsKeyPressed(Keys.NumPad2)) {
                            Point thisDir = Utils.Directions["D"];
                            if (waitingForCommand == "") {
                                ClearWait(player);
                                GameLoop.CommandManager.MoveActorBy(player, thisDir);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait(player);
                                GameLoop.CommandManager.CloseDoor(player, thisDir);
                            } else if (waitingForCommand == "g") {
                                ClearWait(player);
                                GameLoop.CommandManager.Pickup(player, thisDir);
                            } else if (waitingForCommand == "x") {
                                ClearWait(player);
                                GameLoop.CommandManager.Peek(player, thisDir);
                            }
                        } else if (Global.KeyboardState.IsKeyPressed(Keys.Z) || Global.KeyboardState.IsKeyPressed(Keys.NumPad1)) {
                            Point thisDir = Utils.Directions["DL"];
                            if (waitingForCommand == "") {
                                ClearWait(player);
                                GameLoop.CommandManager.MoveActorBy(player, thisDir);
                                CenterOnActor(player);
                            } else if (waitingForCommand == "c") {
                                ClearWait(player);
                                GameLoop.CommandManager.CloseDoor(player, thisDir);
                            } else if (waitingForCommand == "g") {
                                ClearWait(player);
                                GameLoop.CommandManager.Pickup(player, thisDir);
                            } else if (waitingForCommand == "x") {
                                ClearWait(player);
                                GameLoop.CommandManager.Peek(player, thisDir);
                            }
                        }

                    }
                }
            } else {

                if (Global.KeyboardState.IsKeyReleased(Keys.Escape)) {
                    ChatLog.Unfocus();
                }
                if (Global.KeyboardState.IsKeyReleased(Keys.Enter) && GameLoop.NetworkingManager.discord.GetLobbyManager() != null) {
                    if (ChatLog.GetText() != "") {
                        var assembled = GameLoop.NetworkingManager.userManager.GetCurrentUser().Username + ": " + ChatLog.GetText();

                        GameLoop.NetworkingManager.SendNetMessage(1, System.Text.Encoding.UTF8.GetBytes(assembled));

                        ChatLog.Add(assembled);
                        ChatLog.ClearText();
                        ChatLog.Refocus();
                    } else {
                        ChatLog.Refocus();
                    }
                }
            }
        }
    }
}
