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
        public Console StatusConsole;
        public Console InventoryConsole;
        public Console EquipmentConsole;
        public Console SplashConsole;
        public Console SplashRain;
        public Console MenuConsole;
        
        public Window MapWindow;
        public MessageLogWindow MessageLog;
        public ChatLogWindow ChatLog;
        public Window StatusWindow;
        public Window InventoryWindow;
        public Window EquipmentWindow;

        public Window ContextWindow;
        public Console ContextConsole;

        public Button hostButton;
        public Button closeButton;
        public Button joinButton;

        public ControlsConsole joinPrompt;
        public TextBox joinBox;
        public Window WorldCreateWindow;
        public ControlsConsole WorldCreateConsole;
        public TextBox WorldNameBox;
        public Button CreateWorldButton;

        public Window LoadWindow;
        public Console LoadConsole;
        public string selectedWorldName = "";
        public bool tryDelete = false;

        public Point Mouse;
        public string STATE = "MENU";
        public string JOIN_CODE = "";


        public int currentZoomLV = 1; // Half, One, Two, Three, Four


        public bool chat = false;
        public long tempUID = 0;
        public int invContextIndex = -1;

        public List<Entity> rain = new List<Entity>();
        public int raindrops = 0;

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
            UseMouse = true;
            LoadSplash();


            joinPrompt = new ControlsConsole(8, 1, Global.FontDefault);
            joinPrompt.IsVisible = true;
            joinPrompt.Position = new Point(42, 53);

            joinBox = new TextBox(5);
            joinBox.Position = new Point(0,0);
            joinBox.UseKeyboard = true;
            joinBox.IsVisible = true;
 
            joinPrompt.Add(joinBox);
            
            Children.Add(joinPrompt);

            joinPrompt.FocusOnMouseClick = true;
            CreateWorld();
            LoadWorldDialogue();
        }

        public void CreateConsoles() {
            MapConsole = new ScrollingConsole(60, (GameLoop.GameHeight / 3) * 2); 
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

            StatusWindow.Title = title.Align(HorizontalAlignment.Center, statusConsoleWidth, (char) 205);
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

            ContextWindow.Title = title.Align(HorizontalAlignment.Center, contextConsoleWidth, (char) 205);
            ContextWindow.Position = new Point(60, 40);


            ContextWindow.Children.Add(ContextConsole);
            Children.Add(ContextWindow);

            ContextWindow.MoveToFrontOnMouseClick = true;
            
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
                    ContextConsole.Print(0, 2, (("" + (char)205).Align(HorizontalAlignment.Center, 18, (char)205)).CreateColored(new Color(51, 153, 255)));

                    ContextConsole.Print(0, 3, "Drop 01".Align(HorizontalAlignment.Center, 18, ' '));
                    ContextConsole.Print(0, 4, "Drop 05".Align(HorizontalAlignment.Center, 18, ' '));
                    ContextConsole.Print(0, 5, "Drop 10".Align(HorizontalAlignment.Center, 18, ' '));
                    ContextConsole.Print(0, 6, "Drop **".Align(HorizontalAlignment.Center, 18, ' '));
                    
                    if (item.Slot != -1) {
                        ContextConsole.Print(0, 8, "Equip".Align(HorizontalAlignment.Center, 18, ' '));
                    } 
                } else {
                    ContextConsole.Print(0, 0, "No Item Selected".Align(HorizontalAlignment.Center, 18, ' '));
                    ContextConsole.Print(0, 2, (("" + (char)205).Align(HorizontalAlignment.Center, 18, (char)205)).CreateColored(new Color(51, 153, 255)));
                    invContextIndex = -1;
                }
            }

            ContextWindow.IsDirty = true;
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
            

            StatusConsole.Print(0, 3, (("" + (char) 205).Align(HorizontalAlignment.Center, 18, (char) 205)).CreateColored(new Color(51, 153, 255)));


            if (player != null) {
                ColoredString heldTIR = new ColoredString(player.HeldGold.ToString() + " TIR", Color.Yellow, Color.Transparent);
                StatusConsole.Print(StatusConsole.Width - heldTIR.Count - 2, 2, heldTIR);


                StatusConsole.Print(0, 4, " STR: " + player.Strength.ToString());
                StatusConsole.Print(8, 4, "   DEX: " + player.Dexterity.ToString());
                StatusConsole.Print(0, 5, " INT: " + player.Intelligence.ToString());
                StatusConsole.Print(8, 5, "   VIT: " + player.Vitality.ToString());

                StatusConsole.Print(0, 6, "WILL: " + player.Will.ToString());
                StatusConsole.Print(8, 6, "   PER: " + player.Perception.ToString());
                StatusConsole.Print(0, 7, (("" + (char)205).Align(HorizontalAlignment.Center, 18, (char)205)).CreateColored(new Color(51, 153, 255)));

                ColorGradient wgtGrad = new ColorGradient(Color.Green, Color.Red);
                player.RecalculateWeight(); 
                Color wgtColor = wgtGrad.Lerp((float) player.Carrying_Weight / (player.BasicLift * 10));


                ColoredString wgt = new ColoredString((player.Carrying_Weight.ToString() + " / " + (player.BasicLift * 10).ToString() + " kg"), wgtColor, Color.Transparent);
                ColoredString spd = new ColoredString("SPD: " + player.Speed, wgtColor, Color.Transparent);
                ColoredString dodge = new ColoredString("DODGE: " + player.Dodge, wgtColor, Color.Transparent);

                StatusConsole.Print(0, 8, "WGT: ", wgtColor);
                StatusConsole.Print(StatusConsole.Width - wgt.Count - 2, 8, wgt);
                StatusConsole.Print(0, 9, spd);
                StatusConsole.Print(10, 9, dodge);
                StatusConsole.Print(0, 10, (("" + (char)205).Align(HorizontalAlignment.Center, 18, (char)205)).CreateColored(new Color(51, 153, 255)));


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


                Point Mouse = GameLoop.MouseLoc.PixelLocationToConsole(12, 12);
                Mouse = Mouse - player.PositionOffset - new Point(1, 1) ;
                TileBase hovered = GameLoop.World.CurrentMap.GetTileAt<TileBase>(Mouse.X, Mouse.Y);

                StatusConsole.Print(0, 19, "HOVERED TILE".Align(HorizontalAlignment.Center, 18, (char)205).CreateColored(new Color(51, 153, 255)));
                if (hovered != null && hovered.Background.A == 255 && hovered.IsVisible) {
                    ColoredString hovName = new ColoredString(hovered.Name.Align(HorizontalAlignment.Center, 18, ' '), hovered.Foreground, Color.Transparent);
                    StatusConsole.Print(0, 20, hovName);

                    int drawLineAt = 21;

                    List<Actor> monstersAtTile = GameLoop.World.CurrentMap.GetEntitiesAt<Actor>(Mouse);

                    if (monstersAtTile.Count > 0) {
                        drawLineAt++;
                        StatusConsole.Print(0, drawLineAt, "ENTITIES".Align(HorizontalAlignment.Center, 18, (char)205).CreateColored(new Color(51, 153, 255)));
                        drawLineAt++;
                    }

                    for (int i = 0; i < monstersAtTile.Count; i++) {
                        ColorGradient enHealthGrad = new ColorGradient(Color.Red, Color.Green);
                        ColoredString enHealth = new ColoredString(monstersAtTile[i].Name.Align(HorizontalAlignment.Center, 18, ' '), enHealthGrad.Lerp((float)monstersAtTile[i].Health / monstersAtTile[i].MaxHealth), Color.Transparent);
                        StatusConsole.Print(0, drawLineAt, enHealth);
                        drawLineAt++;
                    }


                    List<Item> itemsAtTile = GameLoop.World.CurrentMap.GetEntitiesAt<Item>(Mouse);

                    if (itemsAtTile.Count > 0) {
                        drawLineAt++;
                        StatusConsole.Print(0, drawLineAt, "ITEMS".Align(HorizontalAlignment.Center, 18, (char)205).CreateColored(new Color(51, 153, 255)));
                        drawLineAt++;
                    }

                    for (int i = 0; i < itemsAtTile.Count; i++) {
                        string itemName = itemsAtTile[i].Name;
                        if (itemName.Length > 16) { itemName = itemName.Substring(0, 13) + "..."; }
                        StatusConsole.Print(2, drawLineAt, itemName.Align(HorizontalAlignment.Center, 16, ' '));
                        StatusConsole.Print(0, drawLineAt, itemsAtTile[i].Quantity.ToString());
                        drawLineAt++;
                    }
                }
            }
        }

        public void CreateInventoryWindow(int width, int height, string title) {
            InventoryWindow = new Window(width, height);

            int invConsoleW = width - 2;
            int invConsoleH = height - 2;

            InventoryConsole.Position = new Point(1, 1);


            InventoryWindow.Title = title.Align(HorizontalAlignment.Center, invConsoleW, (char) 205);
            InventoryWindow.Children.Add(InventoryConsole);
            InventoryWindow.Position = new Point((GameLoop.GameWidth/2) - InventoryWindow.Width/2, (GameLoop.GameHeight/2) - InventoryWindow.Height/2);

            Children.Add(InventoryWindow);

            InventoryWindow.CanDrag = true;
            InventoryWindow.IsVisible = false;
            InventoryConsole.IsVisible = false;
            InventoryWindow.FocusOnMouseClick = true;

            UpdateInventory();

            InventoryConsole.MouseButtonClicked += invMouseClick;
        }

        public void CreateEquipmentWindow(int width, int height, string title) {
            EquipmentWindow = new Window(width, height);

            int eqpConsoleW = width - 2;
            int eqpConsoleH = height - 2;

            EquipmentConsole.Position = new Point(1, 1);


            EquipmentWindow.Title = title.Align(HorizontalAlignment.Center, eqpConsoleW, (char) 205);
            EquipmentWindow.Children.Add(EquipmentConsole);
            EquipmentWindow.Position = new Point((GameLoop.GameWidth / 2) - EquipmentWindow.Width / 2, (GameLoop.GameHeight / 2) - EquipmentWindow.Height / 2);

            Children.Add(EquipmentWindow);

            EquipmentWindow.CanDrag = true;
            EquipmentWindow.IsVisible = false;
            EquipmentConsole.IsVisible = false;
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
            InventoryConsole.Print(0, 1, (("" + (char)205).Align(HorizontalAlignment.Center, InventoryConsole.Width - 2, (char)205)).CreateColored(new Color(51, 153, 255)));

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
            
            
            MapWindow.Title = title.Align(HorizontalAlignment.Center, mapConsoleWidth, (char) 205);
            MapWindow.Children.Add(MapConsole);
            

            Children.Add(MapWindow);
            
            MapConsole.MouseButtonClicked += mapClick;

            MapWindow.CanDrag = true;
            MapWindow.IsVisible = true;

            
        }
        
        private void mapClick(object sender, MouseEventArgs e) {
            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                Player player = GameLoop.World.players[GameLoop.NetworkingManager.myUID];
                Point offset = player.PositionOffset;
                Point modifiedClick = e.MouseState.ConsoleCellPosition - offset;

                int range = (int) Distance.CHEBYSHEV.Calculate(player.Position, modifiedClick);
                Monster monster = GameLoop.World.CurrentMap.GetEntityAt<Monster>(modifiedClick); 

                if (monster != null) {
                    if ((player.Equipped[0] != null && range <= Convert.ToInt32(player.Equipped[0].Properties["range"])) || range <= 1) {
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
                        
                        if (new List<string> { "cornflower", "rose", "violet", "dandelion", "tulip" }.Contains(tile.Name)) {
                            Item flower = GameLoop.ItemLibrary[tile.Name].Clone();
                            flower.Position = modifiedClick;
                            GameLoop.World.CurrentMap.Add(flower);

                            GameLoop.World.CurrentMap.Tiles[modifiedClick.ToIndex(GameLoop.World.CurrentMap.Width)] = GameLoop.TileLibrary["grass"].Clone();

                            string serialFlower = JsonConvert.SerializeObject(flower, Formatting.Indented, new ItemJsonConverter());

                            string itemDrop = "i_data|drop|" + flower.Position.X + "|" + flower.Position.Y + "|" + serialFlower;
                            GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(itemDrop));

                            string tileUpdate = "t_data|flower_picked|" + flower.Position.X + "|" + flower.Position.Y;
                            GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(tileUpdate));
                                    
                            player.PickupItem(flower); 
                            RefreshMap();
                        }

                        if (player.Equipped[13] != null) {
                            player.Equipped[13].UseItem(player, modifiedClick);
                        }
                    }
                    
                }
            }
        }

        
        

        
        private void hostButtonClick(object sender, SadConsole.Input.MouseEventArgs e) {
            GameLoop.NetworkingManager.changeClientTarget("0"); // HAS TO BE DISABLED ON LIVE BUILD, ONLY FOR TESTING TWO CLIENTS ON ONE COMPUTER

            var lobbyManager = GameLoop.NetworkingManager.discord.GetLobbyManager(); 
            string possibleChars = "ABCDEFGHIJKLMNPQRSTUVWXYZ0123456789";
            string code = "";
            
            code = "";

            for (int i = 0; i < 4; i++) {
                code += possibleChars[GameLoop.Random.Next(0, possibleChars.Length)];
            }



            var txn = lobbyManager.GetLobbyCreateTransaction();
            txn.SetCapacity(6);
            txn.SetType(Discord.LobbyType.Public);
            txn.SetMetadata("code", code);


            lobbyManager.CreateLobby(txn, (Result result, ref Lobby lobby) => {
                if (result == Result.Ok) {
                    MessageLog.Add("Created lobby! Code has been copied to clipboard.");
                    

                    GameLoop.NetworkingManager.InitNetworking(lobby.Id);
                    lobbyManager.OnMemberConnect += onPlayerConnected;
                    lobbyManager.OnMemberDisconnect += onPlayerDisconnected;
                    
                    ChatLog.IsVisible = true;
                    JOIN_CODE = code;

                    ChatLog.Title = ("[CHAT: " + code + "]").Align(HorizontalAlignment.Center, ChatLog.Width, (char) 205);
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

                    GameLoop.World.CreatePlayer(userId, new Player(Color.Yellow, Color.Transparent));
                    SyncMapEntities(true);
                    GameLoop.NetworkingManager.SendNetMessage(2, System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(GameLoop.World, Formatting.Indented, new WorldJsonConverter())));

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

            joinBox.IsFocused = false;
            

            var lobbyManager = GameLoop.NetworkingManager.discord.GetLobbyManager();
            LobbySearchQuery searchQ = lobbyManager.GetSearchQuery();
            searchQ.Filter("metadata.code", LobbySearchComparison.Equal, LobbySearchCast.String, joinBox.Text);

            string acSec = "";

            lobbyManager.Search(searchQ, (resultSearch) => {
                if (resultSearch == Discord.Result.Ok) {
                    var count = lobbyManager.LobbyCount();

                    acSec = lobbyManager.GetLobbyActivitySecret(lobbyManager.GetLobbyId(0));



                    lobbyManager.ConnectLobbyWithActivitySecret(acSec, (Result result, ref Lobby lobby) => {
                        if (result == Discord.Result.Ok) {
                            MessageLog.Add("Connected to lobby successfully!");
                            GameLoop.NetworkingManager.InitNetworking(lobby.Id);
                            kickstartNet();
                            GameLoop.World.CreatePlayer(GameLoop.NetworkingManager.myUID, new Player(Color.Yellow, Color.Transparent));
                        } else {
                            MessageLog.Add("Encountered error: " + result);
                        }
                    });
                }
            });
        }

        public void CenterOnActor(Actor actor) {
            MapConsole.CenterViewPortOnPoint(actor.Position);
        }

        public override void Update(TimeSpan timeElapsed) {
            base.Update(timeElapsed); 
            CheckKeyboard();
            if (STATE == "GAME") { 
                GameLoop.World.CalculateFov(GameLoop.CommandManager.lastPeek);
                UpdateStatusWindow();
                UpdateContextWindow();
                UpdateEquipment();
                UpdateInventory();
                
            }

            if (STATE == "MENU") {
                SplashAnim();
            }
        }


        public void RefreshMap() {
            MapConsole.SetSurface(GameLoop.World.CurrentMap.Tiles, GameLoop.World.CurrentMap.Width, GameLoop.World.CurrentMap.Height);
            MapConsole.IsDirty = true; 
        }

        public void SyncMapEntities(bool keepOldView = false) {
            MapConsole.Children.Clear();

            foreach (Entity entity in GameLoop.World.CurrentMap.Entities.Items) {
                MapConsole.Children.Add(entity);
            }

            foreach (KeyValuePair<long, Player> player in GameLoop.World.players) {
                MapConsole.Children.Add(player.Value);
            }

            GameLoop.World.CurrentMap.Entities.ItemAdded += OnMapEntityAdded;
            GameLoop.World.CurrentMap.Entities.ItemRemoved += OnMapEntityRemoved;

            GameLoop.World.ResetFOV(keepOldView);
        }

        public void OnMapEntityAdded (object sender, GoRogue.ItemEventArgs<Entity> args) {
            MapConsole.Children.Add(args.Item);
        }

        public void OnMapEntityRemoved(object sender, GoRogue.ItemEventArgs<Entity> args) {
            MapConsole.Children.Remove(args.Item);
        }

        public void LoadMap(Map map) {
            GameLoop.World.CurrentMap = map;

            MapConsole.Clear();
            MapConsole.Children.Clear();
            

            MapConsole = new ScrollingConsole(map.Width, map.Height, Global.FontDefault, new Rectangle(0, 0, MapWindow.Width - 2, MapWindow.Height - 2), GameLoop.World.CurrentMap.Tiles);
            MapConsole.Parent = MapWindow;
            MapConsole.Position = new Point(1, 1);
            MapConsole.MouseButtonClicked += mapClick;

            SyncMapEntities(false);
        }

        public void InitNewMap(string worldName = "Unnamed", bool justInit = false) {
            GameLoop.World.WorldName = worldName;


            CreateConsoles();

            MessageLog = new MessageLogWindow(60, (GameLoop.GameHeight / 3), "[MESSAGE LOG]");
            MessageLog.Title.Align(HorizontalAlignment.Center, MessageLog.Title.Length);
            Children.Add(MessageLog);
            MessageLog.IsVisible = true;
            MessageLog.Position = new Point(0, (GameLoop.GameHeight / 3) * 2);

            ChatLog = new ChatLogWindow(60, GameLoop.GameHeight / 3, "[CHAT LOG]");
            Children.Add(MessageLog);
            ChatLog.Show();
            ChatLog.IsVisible = false;
            ChatLog.Position = new Point((GameLoop.GameWidth / 2) - (ChatLog.Width / 2), (GameLoop.GameHeight / 2) - (ChatLog.Height / 2)); 

            
            CreateMapWindow(60, (GameLoop.GameHeight / 3) * 2, "[GAME MAP]");
            CreateStatusWindow(20, (GameLoop.GameHeight / 3) * 2, "[PLAYER INFO]"); 
            CreateContextWindow(20, GameLoop.GameHeight / 3, "[ITEM MENU]"); 
            ContextConsole.MouseButtonClicked += contextClick;

            CreateInventoryWindow(59, 28, "[INVENTORY]");
            CreateEquipmentWindow(30, 16, "[EQUIPMENT]");


            if (!justInit) {
                GameLoop.World.CreatePlayer(GameLoop.NetworkingManager.myUID, new Player(Color.Yellow, Color.Transparent));
                LoadMap(GameLoop.World.CurrentMap);
                MapConsole.Font = Global.LoadFont("fonts/Cheepicus12.font").GetFont(Font.FontSizes.One);
                CenterOnActor(GameLoop.World.players[GameLoop.NetworkingManager.myUID]);
            }
        }

        public void SplashAnim() {
            if (SplashConsole.IsVisible) {
                if (raindrops < 100 && GameLoop.Random.Next(0, 10) < 7) {
                    raindrops++;

                    Entity newRaindrop = new Entity(Color.Blue, Color.Transparent, '\\');
                    int hori = GameLoop.Random.Next(0, 4);
                    if (hori == 0 || hori == 1) {
                        newRaindrop.Position = new Point(0, GameLoop.Random.Next(0, SplashConsole.Height - 14));
                    } else {
                        newRaindrop.Position = new Point(GameLoop.Random.Next(0, SplashConsole.Width - 2), 0);
                    }
                    rain.Add(newRaindrop);

                    SplashRain.Children.Add(newRaindrop);
                }


                for (int i = rain.Count - 1; i >= 0; i--) {
                    if (rain[i].Position.X + 1 < SplashConsole.Width - 2 && rain[i].Position.Y + 1 < SplashConsole.Height - 14) {
                        rain[i].Position += new Point(1, 1);
                    } else {
                        int hori = GameLoop.Random.Next(0, 4);
                        if (hori == 0 || hori == 1) {
                            rain[i].Position = new Point(0, GameLoop.Random.Next(0, SplashConsole.Height - 14));
                        } else {
                            rain[i].Position = new Point(GameLoop.Random.Next(0, SplashConsole.Width - 2), 0);
                        }
                    }
                }
            }
        }


        public void LoadSplash() {

            SadRex.Image splashScreen = SadRex.Image.Load(new FileStream("./data/splash.xp", FileMode.Open));
            Cell[] splashCells = new Cell[splashScreen.Width * splashScreen.Height];
            int logoCount = 0;
            int lastY = 2;

            for (int i = 0; i < splashScreen.Layers[0].Cells.Count; i++) {
                SadRex.Cell temp = splashScreen.Layers[0].Cells[i];
                Color fore = new Color(temp.Foreground.R, temp.Foreground.G, temp.Foreground.B);
                Color back = new Color(temp.Background.R, temp.Background.G, temp.Background.B);

                if (fore == new Color(17, 17, 17) || fore == new Color(26, 26, 26)) { fore.A = 150; } 
                if (fore == new Color(35, 35, 35) || fore == new Color(48, 48, 48)) { fore.A = 170; } 
                if (fore == new Color(69, 69, 69) || fore == new Color(78, 78, 78)) { fore.A = 190; } 
                if (fore == new Color(255, 0, 255)) { fore.A = 0; } 
                if (back == new Color(255, 0, 255)) {  back.A = 0; }

                ColorGradient splashGrad = new ColorGradient(Color.DeepPink, Color.Cyan);
                Point cellPos = i.ToPoint(splashScreen.Width);

                if (fore == new Color(178, 0, 178)) {
                    if (lastY < cellPos.Y) {
                        logoCount++;
                        lastY = cellPos.Y;
                    }
                    fore = splashGrad.Lerp((float)logoCount / 5);
                }

                splashCells[i] = new Cell(fore, back, temp.Character);
            }

            SplashConsole = new Console(splashScreen.Width, splashScreen.Height, Global.FontDefault, splashCells);
            SplashConsole.IsVisible = true;
            Children.Add(SplashConsole);

            Cell[] rain = new Cell[(SplashConsole.Width - 2) * (SplashConsole.Height - 14)];

            for (int i = 0; i < rain.Length; i++) {
                rain[i] = new Cell(Color.Transparent, Color.Transparent, ' ');
            }

            SplashRain = new Console(SplashConsole.Width - 2, SplashConsole.Height - 14, Global.FontDefault, rain);
            SplashRain.Position = new Point(1, 1);
            SplashConsole.Children.Add(SplashRain);


            Cell[] menu = new Cell[(SplashConsole.Width - 2) * 12];

            for (int i = 0; i < menu.Length; i++) {
                menu[i] = new Cell(Color.White, Color.Transparent, ' ');
            }

            MenuConsole = new Console(SplashConsole.Width - 2, 12, Global.FontDefault, menu);
            SplashConsole.Children.Add(MenuConsole);

            MenuConsole.Position = new Point(1, SplashConsole.Height - 12);
            MenuConsole.Print(1, 1, "CREATE NEW WORLD".Align(HorizontalAlignment.Center, 76, ' ').CreateColored(new Color(51, 153, 255), Color.Transparent));
            MenuConsole.Print(1, 2, "LOAD WORLD".Align(HorizontalAlignment.Center, 76, ' ').CreateColored(new Color(51, 153, 255), Color.Transparent));
             
            MenuConsole.Print(1, 5, "JOIN:    ".Align(HorizontalAlignment.Center, 76, ' ').CreateColored(new Color(51, 153, 255), Color.Transparent));
            MenuConsole.Print(1, 7, "HOST".Align(HorizontalAlignment.Center, 76, ' ').CreateColored(new Color(51, 153, 255), Color.Transparent));

            MenuConsole.Print(1, 10, "EXIT GAME".Align(HorizontalAlignment.Center, 76, ' ').CreateColored(new Color(51, 153, 255), Color.Transparent));

            MenuConsole.MouseButtonClicked += menuClick; 
        }

        private void CreateWorld() {
            WorldCreateWindow = new Window(20, 10, Global.FontDefault);
            WorldCreateWindow.Position = new Point(GameLoop.GameWidth / 2 - 10, GameLoop.GameHeight / 2 - 5);
            WorldCreateWindow.CanDrag = true;
            WorldCreateWindow.IsVisible = false;
            WorldCreateWindow.UseMouse = true;
            WorldCreateWindow.Title = "[CREATE WORLD]".Align(HorizontalAlignment.Center, 18, (char)205); 
            
            WorldCreateConsole = new ControlsConsole(18, 8, Global.FontDefault);
            WorldCreateConsole.Position = new Point(1, 1);
            WorldCreateConsole.IsVisible = false;
            WorldCreateConsole.FocusOnMouseClick = true;

            WorldNameBox = new TextBox(16);
            WorldNameBox.Position = new Point(1, 1);
            WorldNameBox.IsVisible = true;
            WorldNameBox.UseKeyboard = true;
            WorldNameBox.TextAlignment = HorizontalAlignment.Center;


            CreateWorldButton = new Button(18, 1);
            CreateWorldButton.Text = "CREATE".Align(HorizontalAlignment.Center, 18, ' ');
            CreateWorldButton.Position = new Point(0, 7);
            CreateWorldButton.IsVisible = true;
            CreateWorldButton.MouseButtonClicked += createWorldButtonClicked;

            WorldCreateWindow.Children.Add(WorldCreateConsole);
            WorldCreateConsole.Add(WorldNameBox);
            WorldCreateConsole.Add(CreateWorldButton);
            Children.Add(WorldCreateWindow); 
        }

        private void createWorldButtonClicked(object sender, MouseEventArgs e) {
            WorldNameBox.Text = WorldNameBox.EditingText;
            if (WorldNameBox.Text != "") {
                WorldCreateConsole.IsVisible = false;
                WorldCreateWindow.IsVisible = false;
                WorldNameBox.IsVisible = false;
                CreateWorldButton.IsVisible = false;


                InitNewMap(WorldNameBox.Text);
                ChangeState("GAME");

                hostButtonClick(null, null);
            }
        }

        private void SaveEverything() {
            Directory.CreateDirectory(@"./saves/" + GameLoop.World.WorldName);

            // string map = Utils.SimpleMapString(GameLoop.World.CurrentMap.Tiles);

            string map = JsonConvert.SerializeObject(GameLoop.World, Formatting.Indented, new WorldJsonConverter());

            File.WriteAllText(@"./saves/" + GameLoop.World.WorldName + "/map.json", map);
        }

        private void LoadMapFromFile(string path) {
            InitNewMap("", true);
            string map = File.ReadAllText(path + "/map.json");
            
            GameLoop.World = JsonConvert.DeserializeObject<World>(map, new WorldJsonConverter());
            
            LoadMap(GameLoop.World.CurrentMap);
        }


        public void LoadWorldDialogue() {
            LoadWindow = new Window(40, 20, Global.FontDefault);
            LoadWindow.Position = new Point(GameLoop.GameWidth / 2 - 20, GameLoop.GameHeight / 2 - 10);
            LoadWindow.CanDrag = true;
            LoadWindow.IsVisible = false;
            LoadWindow.UseMouse = true;
            LoadWindow.Title = "[CHOOSE WORLD]".Align(HorizontalAlignment.Center, 38, (char)205);

            LoadConsole = new Console(38, 18, Global.FontDefault);
            LoadConsole.Position = new Point(1, 1);
            LoadConsole.IsVisible = false;

            LoadConsole.MouseButtonClicked += loadWorldClick;



            Directory.CreateDirectory(@"./saves"); 
            string[] savelist = Directory.GetDirectories(@"./saves");
            
            int saveCount = 0;

            foreach (string dir in savelist) {
                string[] split = dir.Split('\\');

                Color selected;

                System.Console.WriteLine(selectedWorldName);
                System.Console.WriteLine(dir);

                if (dir == selectedWorldName) {
                    selected = Color.Lime;
                } else {
                    selected = Color.DarkSlateGray;
                }

                LoadConsole.Print(0, saveCount, (split[split.Length - 1].Align(HorizontalAlignment.Center, 38, ' ')).CreateColored(selected));
                saveCount++;
            }


            LoadWindow.Children.Add(LoadConsole);
            Children.Add(LoadWindow);
        }

        private void UpdateLoadWindow() {
            LoadConsole.Clear();

            Directory.CreateDirectory(@"./saves");
            string[] savelist = Directory.GetDirectories(@"./saves");

            int saveCount = 0;

            string selectedName = "";

            foreach (string dir in savelist) {
                string[] split = dir.Split('\\');

                Color selected;

                if (dir == selectedWorldName) {
                    selected = Color.Lime;
                    selectedName = split[split.Length - 1];
                } else {
                    selected = Color.DarkSlateGray;
                }

                LoadConsole.Print(0, saveCount, (split[split.Length - 1].Align(HorizontalAlignment.Center, 38, ' ')).CreateColored(selected));
                saveCount++;
            }


            if (selectedWorldName != "") {
                LoadConsole.Print(0, 14, ("Load " + selectedName).Align(HorizontalAlignment.Center, 38, ' '));
                LoadConsole.Print(0, 15, (("Delete " + selectedName).Align(HorizontalAlignment.Center, 38, ' ')).CreateColored(Color.DarkRed));

                if (tryDelete) {
                    LoadConsole.Print(0, 16, (("Hold SHIFT and press Y to Confirm").Align(HorizontalAlignment.Center, 38, ' ')).CreateColored(Color.Red));
                }
            }

            LoadConsole.Print(0, 17, "BACK TO MENU".Align(HorizontalAlignment.Center, 38, ' '));
        }

        private void loadWorldClick(object sender, MouseEventArgs e) {
            Directory.CreateDirectory(@"./saves");
            string[] savelist = Directory.GetDirectories(@"./saves");

            if (e.MouseState.ConsoleCellPosition.Y < savelist.Length) {
                //LoadMapFromFile(savelist[e.MouseState.ConsoleCellPosition.Y]);
                //ChangeState("GAME");
                selectedWorldName = savelist[e.MouseState.ConsoleCellPosition.Y];
            }


            if (e.MouseState.ConsoleCellPosition.Y == 14) {
                LoadMapFromFile(selectedWorldName);
                ChangeState("GAME");
            }

            if (e.MouseState.ConsoleCellPosition.Y == 15) {
                if (!tryDelete) {
                    tryDelete = true;
                }
            }

            if (e.MouseState.ConsoleCellPosition.Y == 17) {
                LoadWindow.IsVisible = false;
                LoadConsole.IsVisible = false;

                selectedWorldName = "";
                tryDelete = false;
            }

           
            if (e.MouseState.ConsoleCellPosition.Y != 15 && e.MouseState.ConsoleCellPosition.Y != 16) {
                tryDelete = false;
            }

            UpdateLoadWindow();
        }

        private void menuClick(object sender, MouseEventArgs e) {
            if (e.MouseState.ConsoleCellPosition.Y == 1) { // Should open world creation dialogue
                WorldCreateWindow.IsVisible = true;
                WorldCreateConsole.IsVisible = true;
            }

            else if (e.MouseState.ConsoleCellPosition.Y == 2) { // Should open world loading dialogue
                LoadWindow.IsVisible = true;
                LoadConsole.IsVisible = true;

                UpdateLoadWindow();
            }

            else if (e.MouseState.ConsoleCellPosition.Y == 5) { // This one is probably fine like this, but should be switched so it doesn't make its own world before joining.
                InitNewMap("", true);
                ChangeState("GAME");

                joinButtonClick(null, null);
            }

            else if (e.MouseState.ConsoleCellPosition.Y == 7) { // Should open a dialogue that lets the player specify whether to use a new world or load an existing world
                InitNewMap();
                ChangeState("GAME");

                hostButtonClick(null, null);
            }
        }

        private void ClearWait(Actor actor) {
            waitingForCommand = "";
            GameLoop.CommandManager.ResetPeek(actor);
        }

        private void ChangeState(string newState) {
            STATE = newState;
            if (STATE == "MENU") {
                MapConsole.IsVisible = false;
                MapWindow.IsVisible = false;

                InventoryConsole.IsVisible = false;
                InventoryWindow.IsVisible = false;

                StatusConsole.IsVisible = false;
                StatusWindow.IsVisible = false;

                EquipmentConsole.IsVisible = false;
                EquipmentWindow.IsVisible = false;


                SplashConsole.IsVisible = true;
                SplashRain.IsVisible = true;
            }

            if (STATE == "GAME") {
                MapConsole.IsVisible = true;
                MapWindow.IsVisible = true;
                
                StatusConsole.IsVisible = true;
                StatusWindow.IsVisible = true;
                
                SplashConsole.IsVisible = false;
                SplashRain.IsVisible = false;

                LoadWindow.IsVisible = false;
                LoadConsole.IsVisible = false;
            }
        }


        private void CheckKeyboard() {
            if (STATE == "MENU") {
                if (Global.KeyboardState.IsKeyDown(Keys.LeftShift) && Global.KeyboardState.IsKeyReleased(Keys.Y) && tryDelete) {
                    Directory.Delete(selectedWorldName, true);
                    selectedWorldName = "";
                    UpdateLoadWindow();
                }
            }

            if (STATE == "GAME") { 
                if (Global.KeyboardState.IsKeyReleased(Keys.Tab)) {
                    if (ChatLog.IsVisible)
                        ChatLog.IsVisible = false;
                    else {
                        ChatLog.Position = new Point((GameLoop.GameWidth / 2) - (ChatLog.Width / 2), (GameLoop.GameHeight / 2) - (ChatLog.Height / 2));
                        ChatLog.IsVisible = true;
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

                        if (Global.KeyboardState.IsKeyReleased(Keys.OemTilde)) {
                            SaveEverything();
                        }
                        


                        if (Global.KeyboardState.IsKeyPressed(Keys.P)) {
                            System.Console.WriteLine(player.Position);
                        }

                        

                        if (Global.KeyboardState.IsKeyPressed(Keys.E)) {
                            if (EquipmentWindow.IsVisible) {
                                EquipmentWindow.IsVisible = false;
                                EquipmentConsole.IsVisible = false;
                            } else {
                                EquipmentWindow.IsVisible = true;
                                EquipmentConsole.IsVisible = true;
                            }
                        }

                        if (Global.KeyboardState.IsKeyPressed(Keys.I)) {
                            if (InventoryWindow.IsVisible) {
                                InventoryWindow.IsVisible = false;
                                InventoryConsole.IsVisible = false;
                                ContextWindow.IsVisible = false;
                                ContextConsole.IsVisible = false;
                                invContextIndex = -1;
                            } else {
                                InventoryWindow.IsVisible = true;
                                InventoryConsole.IsVisible = true;
                                ContextWindow.IsVisible = true;
                                ContextConsole.IsVisible = true;
                                invContextIndex = -1;
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



                        if (player.TimeLastActed + (UInt64)player.Speed <= GameLoop.GameTime) {
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

                            } else if (Global.KeyboardState.IsKeyPressed(Keys.W) || Global.KeyboardState.IsKeyPressed(Keys.NumPad8)) {
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
}
