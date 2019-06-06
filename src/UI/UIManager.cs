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
using Entity = TearsInRain.Entities.Entity;
using TearsInRain.Serializers;
using TearsInRain.Tiles;
using Console = SadConsole.Console;
using Utils = TearsInRain.Utils;
using GoRogue;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using System.IO;

namespace TearsInRain.UI {
    public class UIManager : ContainerConsole {
        public ScrollingConsole MapConsole;
        public ScrollingConsole MultiConsole;
        public Console StatusConsole;
        public Console InventoryConsole;
        public Console EquipmentConsole;
        
        public Window MapWindow;
        public MessageLogWindow MessageLog;
        public ChatLogWindow ChatLog;
        public Window MultiplayerWindow;
        public Window StatusWindow;
        public Window InventoryWindow;
        public Window EquipmentWindow;

        public Window ContextWindow;
        public Console ContextConsole;
        public Button DropButton;
        public Button DropButton5;
        public Button DropButton10;
        public Button DropButtonAll;
        public Button EquipButton;

        public Button hostButton;
        public Button closeButton;
        public Button joinButton;
        public Button copyButton;


        public int currentZoomLV = 1; // Half, One, Two, Three, Four


        public bool chat = false;
        public long tempUID = 0;
        public int invContextIndex = -1;

        public string waitingForCommand = "";
        public Point viewOffset = new Point(0, 0);
        public Font.FontSizes hold = Font.FontSizes.One;

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

            MessageLog = new MessageLogWindow(60, (GameLoop.GameHeight / 3), "[MESSAGE LOG]");
            MessageLog.Title.Align(HorizontalAlignment.Center, MessageLog.Title.Length); 
            Children.Add(MessageLog);
            MessageLog.Show();
            MessageLog.Position = new Point(0, (GameLoop.GameHeight / 3) * 2);

            ChatLog = new ChatLogWindow(60, GameLoop.GameHeight / 3, "[CHAT LOG]");
            Children.Add(MessageLog);
            ChatLog.Show();
            ChatLog.IsVisible = false;
            ChatLog.Position = new Point((GameLoop.GameWidth / 2) - (ChatLog.Width / 2), (GameLoop.GameHeight / 2) - (ChatLog.Height / 2));
            
            // MessageLog.Add("Testing First");

            LoadMap(GameLoop.World.CurrentMap);

            CreateMapWindow(60, (GameLoop.GameHeight/3) * 2,"[GAME MAP]");
            UseMouse = true;

            CreateMultiplayerWindow(GameLoop.GameWidth / 4, GameLoop.GameHeight / 2, "[MULTIPLAYER]");

            CreateStatusWindow(20, (GameLoop.GameHeight / 3) * 2 , "[PLAYER INFO]");

            CreateContextWindow(20, GameLoop.GameHeight / 3, "[ITEM MENU]");

            ContextConsole.MouseButtonClicked += contextClick;

            CreateInventoryWindow(59, 28, "[INVENTORY]");
            CreateEquipmentWindow(30, 16, "[EQUIPMENT]");
            
            MapConsole.Font = Global.LoadFont("fonts/Cheepicus12.font").GetFont(Font.FontSizes.One);


            CenterOnActor(GameLoop.World.players[GameLoop.NetworkingManager.myUID]);
            
            
        }
        public void CreateConsoles() {
            MapConsole = new ScrollingConsole(60, (GameLoop.GameHeight / 3) * 2);  
            MultiConsole = new ScrollingConsole(GameLoop.GameWidth / 4, GameLoop.GameHeight / 2);
            StatusConsole = new Console(20, 40);
            InventoryConsole = new Console(59, 28);
            ContextConsole = new Console(20, 20);
            EquipmentConsole = new Console(30, 16);
        }


        public void CreateStatusWindow(int width, int height, string title) {
            StatusWindow = new Window(width, height);

            int statusConsoleWidth = width - 2;
            int statusConsoleHeight = height - 2;

            StatusConsole.Position = new Point(1, 1);

            StatusWindow.Title = title.Align(HorizontalAlignment.Center, statusConsoleWidth, '-');
            StatusWindow.Children.Add(StatusConsole);
            StatusWindow.Position = new Point(60, 0);

            Children.Add(StatusWindow);

            StatusWindow.CanDrag = true;
            StatusWindow.IsVisible = true;
        }

        public void CreateContextWindow(int width, int height, string title) {
            ContextWindow = new Window(width, height);

            int contextConsoleWidth = width - 2;
            int contextConsoleHeight = height - 2;

            ContextConsole.Position = new Point(1, 1);

            ContextWindow.Title = title.Align(HorizontalAlignment.Center, contextConsoleWidth, '-');
            ContextWindow.Position = new Point(60, 40);


            ContextWindow.Children.Add(ContextConsole);
            Children.Add(ContextWindow);

            ContextWindow.MoveToFrontOnMouseClick = true;

            DropButton = new Button(20, 1);
            DropButton.Text = "Drop 01".Align(HorizontalAlignment.Center, 20, ' ');
            DropButton.Position = new Point(0, 4);
            DropButton.MouseButtonClicked += contextClick;

            DropButton5 = new Button(20, 1);
            DropButton5.Text = "Drop 05".Align(HorizontalAlignment.Center, 20, ' ');
            DropButton5.Position = new Point(0, 5);
            DropButton5.MouseButtonClicked += contextClick;

            DropButton10 = new Button(20, 1);
            DropButton10.Text = "Drop 10".Align(HorizontalAlignment.Center, 20, ' ');
            DropButton10.Position = new Point(0, 6);
            DropButton10.MouseButtonClicked += contextClick;

            DropButtonAll = new Button(20, 1);
            DropButtonAll.Text = "Drop **".Align(HorizontalAlignment.Center, 20, ' ');
            DropButtonAll.Position = new Point(0, 7);
            DropButtonAll.MouseButtonClicked += contextClick;

            EquipButton = new Button(20, 1);
            EquipButton.Text = "Equip".Align(HorizontalAlignment.Center, 20, ' ');
            EquipButton.Position = new Point(0, 9);
            EquipButton.MouseButtonClicked += contextClick;

            ContextWindow.Add(DropButton);
            ContextWindow.Add(DropButton5);
            ContextWindow.Add(DropButton10);
            ContextWindow.Add(DropButtonAll);
            ContextWindow.Add(EquipButton);
            ContextWindow.CanDrag = true;
            ContextWindow.IsVisible = false;
        }

        public void UpdateContextWindow() {
            ContextWindow.UseMouse = true;
            ContextConsole.Clear();

            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                Player player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];
                 
                if (invContextIndex != -1 && invContextIndex < player.Inventory.Count) {
                    Item item = player.Inventory[invContextIndex];
                    string itemName = item.Name;

                    if (itemName.Length > 18) { itemName = itemName.Substring(0, 14) + "...";  }

                    ContextConsole.Print(0, 0, itemName.Align(HorizontalAlignment.Center, 18, ' '));
                    ContextConsole.Print(0, 1, ("QTY: " + item.Quantity.ToString()).Align(HorizontalAlignment.Center, 18, ' '));
                    ContextConsole.Print(0, 2, "-".Align(HorizontalAlignment.Center, 18, '-'));

                    DropButton.IsVisible = true;
                    DropButton5.IsVisible = true;
                    DropButton10.IsVisible = true;
                    DropButtonAll.IsVisible = true;

                    if (item.Slot != -1) {
                        EquipButton.IsVisible = true;
                    } else {
                        EquipButton.IsVisible = false;
                    }
                } else {
                    ContextConsole.Print(0, 0, "No Item Selected".Align(HorizontalAlignment.Center, 18, ' '));
                    ContextConsole.Print(0, 2, "-".Align(HorizontalAlignment.Center, 18, '-')); 
                    invContextIndex = -1;

                    DropButton.IsVisible = false;
                    DropButton5.IsVisible = false;
                    DropButton10.IsVisible = false;
                    DropButtonAll.IsVisible = false;
                    EquipButton.IsVisible = false;
                }

                ContextWindow.IsDirty = true;
            }
        }

        private void contextClick(object sender, MouseEventArgs e) {
            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID) && ContextWindow.IsVisible) {
                Player player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];
                if (e.MouseState.ConsoleCellPosition.Y == 3) {
                    if (invContextIndex != -1 && invContextIndex < player.Inventory.Count) {
                        player.DropItem(invContextIndex, 1);
                    } else {
                        invContextIndex = -1;
                    } 
                } else if (e.MouseState.ConsoleCellPosition.Y == 4) {
                    if (invContextIndex != -1 && invContextIndex < player.Inventory.Count) {
                        player.DropItem(invContextIndex, 5);
                    } else {
                        invContextIndex = -1;
                    }
                } else if (e.MouseState.ConsoleCellPosition.Y == 5) {
                    if (invContextIndex != -1 && invContextIndex < player.Inventory.Count) {
                        player.DropItem(invContextIndex, 10);
                    } else {
                        invContextIndex = -1;
                    }
                } else if (e.MouseState.ConsoleCellPosition.Y == 6) {
                    if (invContextIndex != -1 && invContextIndex < player.Inventory.Count) {
                        player.DropItem(invContextIndex, 0);
                    } else {
                        invContextIndex = -1;
                    }
                } else if (e.MouseState.ConsoleCellPosition.Y == 8) {
                    if (invContextIndex != -1 && invContextIndex < player.Inventory.Count) {
                        player.Equip(invContextIndex);
                    } else {
                        invContextIndex = -1;
                    }
                }

                if (player.Inventory.Count == 0) {
                    invContextIndex = -1;
                    ContextWindow.IsVisible = false;
                }
            } 
        }

        public void UpdateStatusWindow() { 
            StatusConsole.Clear();

            Player player = null;

            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];
            }

            TimeManager time = GameLoop.TimeManager;

            ColoredString season = time.ColoredSeason();
            ColoredString timeString = new ColoredString(time.GetTimeString(), Color.White, Color.TransparentBlack);
            string dayYear = time.Day.ToString();
            if (time.Day < 10) { dayYear = " " + dayYear; }

            string year = "Year " + time.Year.ToString();
            
            StatusConsole.Print(0, 1, season);
            StatusConsole.Print(0 + season.Count, 1, dayYear);
            StatusConsole.Print(0, 2, year);
            StatusConsole.Print(StatusConsole.Width-timeString.Count - 2, 1, timeString);
            
            StatusConsole.Print(0, 3, "------------------");


            if (player != null) {
                ColoredString heldTIR = new ColoredString(player.HeldGold.ToString() + " TIR", Color.Yellow, Color.Transparent);
                StatusConsole.Print(StatusConsole.Width - heldTIR.Count - 2, 2, heldTIR);


                StatusConsole.Print(0, 4, " STR: " + player.Strength.ToString());
                StatusConsole.Print(8, 4, "   DEX: " + player.Dexterity.ToString());
                StatusConsole.Print(0, 5, " INT: " + player.Intelligence.ToString());
                StatusConsole.Print(8, 5, "   VIT: " + player.Vitality.ToString());

                StatusConsole.Print(0, 6, "WILL: " + player.Will.ToString());
                StatusConsole.Print(8, 6, "   PER: " + player.Perception.ToString());
                StatusConsole.Print(0, 7, "------------------");

                Color wgtColor;

                switch (player.EncumbranceLv) {
                    case 0:
                        wgtColor = Color.CornflowerBlue;
                        break;
                    case 1:
                        wgtColor = Color.Green;
                        break;
                    case 2:
                        wgtColor = Color.Yellow;
                        break;
                    case 3:
                        wgtColor = Color.Orange;
                        break;
                    case 4:
                        wgtColor = Color.Red;
                        break;
                    default:
                        wgtColor = Color.CornflowerBlue;
                        break;
                }


                ColoredString wgt = new ColoredString((player.Carrying_Weight.ToString() + " / " + (player.BasicLift * 10).ToString() + " kg"), wgtColor, Color.Transparent);
                ColoredString spd = new ColoredString("SPD: " + player.Speed, wgtColor, Color.Transparent);
                ColoredString dodge = new ColoredString("DODGE: " + player.Dodge, wgtColor, Color.Transparent);

                StatusConsole.Print(0, 8, "WGT: ", wgtColor);
                StatusConsole.Print(StatusConsole.Width - wgt.Count - 2, 8, wgt);
                StatusConsole.Print(0, 9, spd);
                StatusConsole.Print(10, 9, dodge);
                StatusConsole.Print(0, 10, "------------------");


                StatusConsole.Print(0, 11, "HEALTH: "); 
                float percent = (float) player.Health /  (float) player.MaxHealth; 
                ColorGradient hpgradient = new ColorGradient(Color.Red, Color.Green);
                string hpbar = "";
                for (int i = 0; i < 10; i++) {
                    if (percent >= ((float) i * 0.1f)) {
                        hpbar += "#";
                    }
                }
                ColoredString health = new ColoredString(hpbar, hpgradient.Lerp(percent), Color.Transparent);
                StatusConsole.Print(8, 11, health);


                StatusConsole.Print(0, 12, "  STAM: ");
                float stamPercent = (float)player.CurrentStamina / (float)player.MaxStamina;
                ColorGradient stamGradient = new ColorGradient(Color.Red, Color.Yellow);
                string stamBar = "";
                for (int i = 0; i < 10; i++) {
                    if (stamPercent >= ((float)i * 0.1f)) {
                        stamBar += "#";
                    }
                }
                ColoredString stamina = new ColoredString(stamBar, stamGradient.Lerp(stamPercent), Color.Transparent);
                StatusConsole.Print(8, 12, stamina);


                StatusConsole.Print(0, 13, "ENERGY: ");
                float energyPercent = (float)player.CurrentEnergy / (float)player.MaxEnergy;
                ColorGradient energyGradient = new ColorGradient(Color.Red, Color.Aqua);
                string energyBar = "";
                for (int i = 0; i < 10; i++) {
                    if (energyPercent >= ((float)i * 0.1f)) {
                        energyBar += "#";
                    }
                }
                ColoredString energy = new ColoredString(energyBar, energyGradient.Lerp(energyPercent), Color.Transparent);
                StatusConsole.Print(8, 13, energy);
            }
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
            InventoryWindow.IsVisible = false;
            InventoryWindow.FocusOnMouseClick = true;

            UpdateInventory();

            InventoryConsole.MouseButtonClicked += invMouseClick;
        }

        public void CreateEquipmentWindow(int width, int height, string title) {
            EquipmentWindow = new Window(width, height);

            int eqpConsoleW = width - 2;
            int eqpConsoleH = height - 2;

            EquipmentConsole.Position = new Point(1, 1);


            EquipmentWindow.Title = title.Align(HorizontalAlignment.Center, eqpConsoleW, '-');
            EquipmentWindow.Children.Add(EquipmentConsole);
            EquipmentWindow.Position = new Point((GameLoop.GameWidth / 2) - EquipmentWindow.Width / 2, (GameLoop.GameHeight / 2) - EquipmentWindow.Height / 2);

            Children.Add(EquipmentWindow);

            EquipmentWindow.CanDrag = true;
            EquipmentWindow.IsVisible = true;
            EquipmentWindow.FocusOnMouseClick = true;

            UpdateEquipment();

            EquipmentConsole.MouseButtonClicked += eqpMouseClick;
        }

        private void eqpMouseClick(object sender, MouseEventArgs e) {
            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID) && EquipmentWindow.IsVisible) {
                Player player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];
                if (e.MouseState.ConsoleCellPosition.Y < 14 && player.Equipped[e.MouseState.ConsoleCellPosition.Y] != null) {
                    player.Unequip(e.MouseState.ConsoleCellPosition.Y);
                }
            }
        }

        public void UpdateEquipment() {
            EquipmentConsole.Clear();
            
            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                Player player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];
                for (int i = 0; i < player.Equipped.Length; i++) {
                    if (player.Equipped[i] != null) {
                        EquipmentConsole.Print(0, i, player.Equipped[i].Name);
                    } else {
                        switch(i) {
                            case 0:
                                EquipmentConsole.Print(0, i, new ColoredString("(MELEE)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 1:
                                EquipmentConsole.Print(0, i, new ColoredString("(RANGED)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 2:
                                EquipmentConsole.Print(0, i, new ColoredString("(AMMO)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 3:
                                EquipmentConsole.Print(0, i, new ColoredString("(RING)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 4:
                                EquipmentConsole.Print(0, i, new ColoredString("(RING)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 5:
                                EquipmentConsole.Print(0, i, new ColoredString("(NECK)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 6:
                                EquipmentConsole.Print(0, i, new ColoredString("(LIGHTING)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 7:
                                EquipmentConsole.Print(0, i, new ColoredString("(BODY)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 8:
                                EquipmentConsole.Print(0, i, new ColoredString("(CLOAK)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 9:
                                EquipmentConsole.Print(0, i, new ColoredString("(SHIELD)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 10:
                                EquipmentConsole.Print(0, i, new ColoredString("(HEAD)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 11:
                                EquipmentConsole.Print(0, i, new ColoredString("(HANDS)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 12:
                                EquipmentConsole.Print(0, i, new ColoredString("(FEET)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            case 13:
                                EquipmentConsole.Print(0, i, new ColoredString("(TOOL)", Color.DarkSlateGray, Color.Transparent));
                                break;
                            default:
                                EquipmentConsole.Print(0, i, new ColoredString("(ERROR)", Color.DarkSlateGray, Color.Transparent));
                                break;
                        }
                    }
                    
                    
                }
            }


        }

        private void invMouseClick(object sender, MouseEventArgs e) {
            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID) && InventoryWindow.IsVisible) {
                Player player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];
                if (player.Inventory.Count >= e.MouseState.ConsoleCellPosition.Y - 2 && e.MouseState.ConsoleCellPosition.Y - 2 >= 0) {
                    invContextIndex = e.MouseState.ConsoleCellPosition.Y - 2;
                    ContextWindow.IsVisible = false;
                    ContextWindow.IsVisible = true;
                    ContextWindow.IsDirty = true;
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


            InventoryConsole.IsDirty = true;
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
            MapConsole.MouseButtonClicked += mapClick;

            MapWindow.CanDrag = true;
            MapWindow.Show();

            
        }

        private void mapClick(object sender, MouseEventArgs e) {
            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                Player player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];
                Point offset = player.PositionOffset;
                Point modifiedClick = e.MouseState.ConsoleCellPosition - offset;

                int range = (int) Distance.CHEBYSHEV.Calculate(player.Position, modifiedClick);
                Monster monster = GameLoop.World.CurrentMap.GetEntityAt<Monster>(modifiedClick);

                GameLoop.UIManager.MessageLog.Add("Click: " + modifiedClick + ", Offset: " + offset + ", Player: " + player.Position);

                if (monster != null) {
                    if ((player.Equipped[0] != null && range <= player.Equipped[0].Properties["melee"]) || range <= 1) {
                        GameLoop.CommandManager.Attack(player, monster);
                    }
                } else {
                    if (range <= 1) {
                        TileBase tile = GameLoop.World.CurrentMap.GetTileAt<TileBase>(modifiedClick.X, modifiedClick.Y); 
                                

                        if (tile is TileDoor door) {
                            if (!door.IsOpen)
                                GameLoop.CommandManager.OpenDoor(player, door, modifiedClick);
                            else
                                GameLoop.CommandManager.CloseDoor(player, modifiedClick, true);

                        }

                        if (tile is TileFloor floor) {
                            if (new List<string> { "cornflower", "rose", "violet", "dandelion", "tulip" }.Contains(tile.Name)) {
                                Item flower = new Item(tile.Foreground, Color.Transparent, tile.Name, (char) tile.Glyph, 0.01, 100);
                                flower.Position = modifiedClick;
                                GameLoop.World.CurrentMap.Add(flower);

                                GameLoop.World.CurrentMap.Tiles[modifiedClick.ToIndex(GameLoop.World.CurrentMap.Width)] = new TileFloor(false, false, "just-grass");

                                string serialFlower = JsonConvert.SerializeObject(flower, Formatting.Indented, new ItemJsonConverter());

                                string itemDrop = "i_data|drop|" + flower.Position.X + "|" + flower.Position.Y + "|" + serialFlower;
                                GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(itemDrop));

                                string tileUpdate = "t_data|flower_picked|" + flower.Position.X + "|" + flower.Position.Y;
                                GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(tileUpdate));
                                    
                                player.PickupItem(flower); 
                                GameLoop.UIManager.RefreshMap();
                            }
                        }

                        if (player.Equipped[13] != null) {
                            player.Equipped[13].UseItem(player, modifiedClick);
                        }
                    }
                    
                }
            }
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

            MultiplayerWindow.Add(closeButton);
            MultiplayerWindow.Add(hostButton);
            MultiplayerWindow.Add(joinButton);
            MultiplayerWindow.Add(copyButton);


            MultiplayerWindow.Title = title.Align(HorizontalAlignment.Center, multiConsoleW, '-');

            Children.Add(MultiplayerWindow);
            MultiplayerWindow.Show();
            MultiplayerWindow.IsVisible = false;
            MultiplayerWindow.Position = new Point((GameLoop.GameWidth / 2) - (MultiplayerWindow.Width/2), (GameLoop.GameHeight / 2) - (MultiplayerWindow.Height/2));
        }

        private void closeButtonClick(object sender, MouseEventArgs e) {
            MultiplayerWindow.IsVisible = false;
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
                    var itemList = "i_data|list";
                    foreach (Entity entity in GameLoop.World.CurrentMap.Entities.Items) {
                        if (entity is Monster && !(entity is Item)) {
                            monsterList += "|" + JsonConvert.SerializeObject((Actor)entity, Formatting.Indented, new ActorJsonConverter()) + "~" + entity.Position.X + "~" + entity.Position.Y;
                        } else if (entity is Item && !(entity is Monster)) {
                            itemList += "|" + JsonConvert.SerializeObject((Item) entity, Formatting.Indented, new ItemJsonConverter()) + "~" + entity.Position.X + "~" + entity.Position.Y;
                        }
                    }

                    GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(monsterList));
                    GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(itemList));

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
            UpdateEquipment();
            UpdateInventory();
        }


        public void RefreshMap() {
            MapConsole.SetSurface(GameLoop.World.CurrentMap.Tiles, GameLoop.World.CurrentMap.Width, GameLoop.World.CurrentMap.Height);
        }

        public void SyncMapEntities(Map map) {
            MapConsole.Children.Clear();

            foreach (Entity entity in map.Entities.Items) {
                if (!(entity is Player))
                    MapConsole.Children.Add(entity);
            }

            foreach (KeyValuePair<long, Player> player in GameLoop.World.players) {
                MapConsole.Children.Add(player.Value);
            }

            map.Entities.ItemAdded += OnMapEntityAdded; 
            map.Entities.ItemRemoved += OnMapEntityRemoved;

            GameLoop.World.ResetFOV();
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

                    if (Global.KeyboardState.IsKeyPressed(Keys.C)) {
                        waitingForCommand = "c";
                    }

                    if (Global.KeyboardState.IsKeyPressed(Keys.E)) {
                        if (EquipmentWindow.IsVisible) {
                            EquipmentWindow.IsVisible = false;
                        } else {
                            EquipmentWindow.IsVisible = true;
                        }
                    }

                    if (Global.KeyboardState.IsKeyPressed(Keys.I)) {
                        if (InventoryWindow.IsVisible) {
                            InventoryWindow.IsVisible = false;
                            ContextWindow.IsVisible = false;
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

                    if (Global.KeyboardState.IsKeyReleased(Keys.H)) {
                        player.Health--;
                    }

                    if (Global.KeyboardState.IsKeyReleased(Keys.Escape)) {
                        if (waitingForCommand != "")
                            ClearWait(player);
                        ContextWindow.IsVisible = false;
                    }

                    if (Global.KeyboardState.IsKeyReleased(Keys.OemPlus)) {
                        if (hold != Font.FontSizes.Four) {
                            switch (MapConsole.Font.SizeMultiple) {
                                case Font.FontSizes.One:
                                    MapConsole.Font = Global.LoadFont("fonts/Cheepicus12.font").GetFont(Font.FontSizes.Two);
                                    hold = Font.FontSizes.Two;
                                    MapConsole.ViewPort = new Rectangle(0, 0, 29, 19); 
                                    break;
                                case Font.FontSizes.Two:
                                    MapConsole.Font = Global.LoadFont("fonts/Cheepicus12.font").GetFont(Font.FontSizes.Four);
                                    hold = Font.FontSizes.Four;
                                    MapConsole.ViewPort = new Rectangle(0, 0, 15, 9);
                                    break;
                            }

                            foreach (Entity entity in GameLoop.World.CurrentMap.Entities.Items) {
                                entity.Font = Global.LoadFont("fonts/Cheepicus12.font").GetFont(hold);
                                entity.Position = entity.Position;
                                entity.IsDirty = true;
                            }

                            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                                CenterOnActor(GameLoop.World.players[GameLoop.NetworkingManager.myUID]);
                            }

                            MapConsole.IsDirty = true;
                        }
                    }

                    if (Global.KeyboardState.IsKeyReleased(Keys.OemMinus)) {
                        if (hold != Font.FontSizes.One) {
                            switch (MapConsole.Font.SizeMultiple) {
                                case Font.FontSizes.Two:
                                    MapConsole.Font = Global.LoadFont("fonts/Cheepicus12.font").GetFont(Font.FontSizes.One);
                                    hold = Font.FontSizes.One;
                                    MapConsole.ViewPort = new Rectangle(0, 0, 58, 38);
                                    break;
                                case Font.FontSizes.Four:
                                    MapConsole.Font = Global.LoadFont("fonts/Cheepicus12.font").GetFont(Font.FontSizes.Two);
                                    hold = Font.FontSizes.Two;
                                    MapConsole.ViewPort = new Rectangle(0, 0, 29, 19);
                                    break;
                            }

                            foreach (Entity entity in GameLoop.World.CurrentMap.Entities.Items) {
                                entity.Font = Global.LoadFont("fonts/Cheepicus12.font").GetFont(hold);
                                entity.Position = entity.Position;
                                entity.IsDirty = true;
                            }


                            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                                CenterOnActor(GameLoop.World.players[GameLoop.NetworkingManager.myUID]);
                            }

                            MapConsole.IsDirty = true;

                        }
                    }



                    if (player.TimeLastActed + (UInt64) player.Speed <= GameLoop.GameTime) {
                        if (Global.KeyboardState.IsKeyPressed(Keys.NumPad9)) {
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

                        } else if (Global.KeyboardState.IsKeyPressed(Keys.NumPad7)) {
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
                        } else if (Global.KeyboardState.IsKeyPressed(Keys.NumPad3)) {
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
                        } else if (Global.KeyboardState.IsKeyPressed(Keys.NumPad1)) {
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
