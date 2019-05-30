using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using RogueSharp.DiceNotation;
using TearsInRain.Serializers;
using TearsInRain.Tiles;

namespace TearsInRain.Entities {
    
    [JsonConverter(typeof(ActorJsonConverter))]
    public class Actor : Entity { 
        public UInt64 TimeLastActed { get; set; }

        // Primary Attributes
        
        public int Strength = 10;
        public int Dexterity = 10;
        public int Intelligence = 10;
        public int Vitality = 10;

        // Secondary Attributes (Direct)
        public int Health = 10;  // Starts equal to MaxHealth
        public int MaxHealth = 10; // Starts equal to Strength
        public int Will = 10; // Starts equal to Intelligence
        public int Perception = 10; // Starts equal to Intelligence
        public int Stamina = 10; // Starts equal to Vitality

        // Secondary Attributes (Indirect)
        public double Carrying_Weight = 0;
        public double Carrying_Volume = 0;
        public double MaxCarriedVolume = 0;

        public int Speed = 10;

        public int EncumbranceLv = 0;

        public int BaseDodge = 8;
        public int Dodge = 8;

        public bool IsStealthing = false;
        public bool FailedStealth = false; 
        public int StealthResult = 0;
        public int BaseStealthResult = 0;

        public int Gold { get; set; }
        public List<Item> Inventory = new List<Item>();

        public Actor(Color foreground, Color background, int glyph, int width = 1, int height = 1) : base(foreground, background, width, height, glyph) {
            Animation.CurrentFrame[0].Foreground = foreground;
            Animation.CurrentFrame[0].Background = background;
            Animation.CurrentFrame[0].Glyph = glyph;
        }

        public bool MoveBy(Point positionChange) { 
            TileBase tile = GameLoop.World.CurrentMap.GetTileAt<TileDoor>(Position.X + positionChange.X, Position.Y + positionChange.Y);

            Point justVert = new Point(0, positionChange.Y);
            Point justHori = new Point(positionChange.X, 0);

            if (GameLoop.World.CurrentMap.IsTileWalkable(Position + positionChange) || tile is TileDoor) {
                Monster monster = GameLoop.World.CurrentMap.GetEntityAt<Monster>(Position + positionChange);
                Item item = GameLoop.World.CurrentMap.GetEntityAt<Item>(Position + positionChange);

                if (monster != null) {
                    GameLoop.CommandManager.Attack(this, monster);
                    return true;
                } else if (tile is TileDoor door && !door.IsOpen) {
                    GameLoop.CommandManager.OpenDoor(this, door, Position + positionChange);
                    return true;
                }


                Position += positionChange;

                string msg = "move_p" + "|" + GameLoop.NetworkingManager.myUID + "|" + Position.X + "|" + Position.Y;
                GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(msg));
                return true;
            } else if (GameLoop.World.CurrentMap.IsTileWalkable(Position + justVert) || tile is TileDoor) {
                Monster monster = GameLoop.World.CurrentMap.GetEntityAt<Monster>(Position + justVert);
                Item item = GameLoop.World.CurrentMap.GetEntityAt<Item>(Position + justVert);

                if (monster != null) {
                    GameLoop.CommandManager.Attack(this, monster);
                    return true;
                } else if (tile is TileDoor door && !door.IsOpen) {
                    GameLoop.CommandManager.OpenDoor(this, door, Position + justVert);
                    return true;
                }


                Position += justVert;

                string msg = "move_p" + "|" + GameLoop.NetworkingManager.myUID + "|" + Position.X + "|" + Position.Y;
                GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(msg));
                return true;
            } else if (GameLoop.World.CurrentMap.IsTileWalkable(Position + justHori) || tile is TileDoor) {
                Monster monster = GameLoop.World.CurrentMap.GetEntityAt<Monster>(Position + justHori);
                Item item = GameLoop.World.CurrentMap.GetEntityAt<Item>(Position + justHori);

                if (monster != null) {
                    GameLoop.CommandManager.Attack(this, monster);
                    return true;
                } else if (tile is TileDoor door && !door.IsOpen) {
                    GameLoop.CommandManager.OpenDoor(this, door, Position + justHori);
                    return true;
                }


                Position += justHori;

                string msg = "move_p" + "|" + GameLoop.NetworkingManager.myUID + "|" + Position.X + "|" + Position.Y;
                GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(msg));
                return true;
            } else { 
                return false;
            }
        }

        public bool MoveTo(Point newPosition) {
            Position = newPosition;
            return true;
        }


        public void Stealth(int stealthResult, bool IsLocal) {
            IsStealthing = true;

            if (stealthResult > Dexterity) {
                FailedStealth = true;
            }

            if (IsLocal) {
                Animation.CurrentFrame[0].Foreground = Color.DarkGray;
                Animation.IsDirty = true;
            } else {
                UpdateStealth(0);
            }

            StealthResult = stealthResult;
            Speed += 5;
        }

        public void Unstealth() {
            IsStealthing = false;
            FailedStealth = false;
            StealthResult = 0;

            Animation.CurrentFrame[0].Foreground.A = 255;
            Animation.CurrentFrame[0].Foreground = Color.Yellow;
            Animation.IsDirty = true;

            Speed -= 5;
        }


        public void UpdateStealth(int mod) {
            if (!IsStealthing) { return; }

            TileBase tile = GameLoop.World.CurrentMap.GetTileAt<TileBase>(Position.X, Position.Y);
            Color tColor = tile.Background;
            Animation.CurrentFrame[0].Foreground = Color.Lerp(Color.Yellow, tColor, 0.05f * Dexterity);
            Animation.CurrentFrame[0].Foreground.SetHSL(tColor.GetHue(), tColor.GetSaturation(), tColor.GetBrightness());

            if (!FailedStealth) {
                if (Dexterity >= (StealthResult - mod)) {
                    int successBy = (Dexterity - (StealthResult - mod));
                    int newA = 255 - (successBy * 10);
                    if (newA < 75) { newA = 75; }
                    if (newA > 255) { newA = 255; }
                    Animation.CurrentFrame[0].Foreground.A = (byte)newA;
                }
            }

            Animation.IsDirty = true;
        }


        public string GetMeleeDamage(string type) {
            double diceFormula;
            int dice;
            int add;

            if (type == "swing") {
                diceFormula = ((Strength - 10) + 4) / 4;
            } else {
                diceFormula = ((Strength - 10) + 4) / 8;
            }

            string[] splitDice = new string[2];
            System.Console.WriteLine(diceFormula.ToString());
            splitDice = diceFormula.ToString().Split('.');

            dice = Convert.ToInt32(splitDice[0]);

            int tempAdd = 0;
            if (splitDice.Length == 2) {
                tempAdd = Convert.ToInt32(splitDice[1]);
            }

            if (tempAdd <= 0.25) {
                add = 1;
            } else if (0.25 < tempAdd && tempAdd <= 0.5) {
                add = 2;
            } else if (0.5 < tempAdd && tempAdd <= 0.75) {
                add = -1;
                dice++;
            } else {
                add = 0;
                dice++;
            }


            return dice + "d6+" + add;
        }


        public void CalculateEncumbrance() {
            int BasicLift = (int) Math.Round((double) (Strength * Strength) / 5, 0); // Weight you can lift above your head with one hand comfortably.

            if (Carrying_Weight <= BasicLift) { EncumbranceLv = 0; } // Not Encumbered at all
            if (BasicLift < Carrying_Weight && Carrying_Weight <= BasicLift * 2) { EncumbranceLv = 1; } // Light
            if (BasicLift * 2 < Carrying_Weight && Carrying_Weight <= BasicLift * 3) { EncumbranceLv = 2; } // Medium
            if (BasicLift * 3 < Carrying_Weight && Carrying_Weight <= BasicLift * 6) { EncumbranceLv = 3; } // Heavy
            if (BasicLift * 6 < Carrying_Weight && Carrying_Weight <= BasicLift * 10) { EncumbranceLv = 4; } // Extra Heavy

            if (BasicLift * 10 < Carrying_Weight) {
                Speed = 2;
            } else {
                Speed = 1;
            }

            Dodge = BaseDodge - EncumbranceLv;
        }


        public void AddWeight(double added) {
            Carrying_Weight += added;
            CalculateEncumbrance();
        }

        public void RemoveWeight(double removed) {
            Carrying_Weight -= removed;
            CalculateEncumbrance();
        }


        public void PickupItem(Item item) {
            if (Carrying_Volume + item.Volume <= MaxCarriedVolume) {
                AddWeight(item.Weight);

                Inventory.Add(item);
                GameLoop.UIManager.MessageLog.Add($"{Name} picked up {item.Name}.");
                item.Destroy();
            }
        }
    }
}