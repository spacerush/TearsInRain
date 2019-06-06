using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using TearsInRain.Serializers;
using TearsInRain.Tiles;

namespace TearsInRain.Entities {

    [JsonConverter(typeof(ItemJsonConverter))]
    public class Item : Entity {
        public int Condition;
        public double Weight;
        public int Quantity;
        public string NamePlural;
        public int Slot;
        public string ID;
        public Dictionary<string, int> Properties;

        public Item(Color foreground, Color background, string name, char glyph, double weight = 1, int condition = 100, int width = 1, int height = 1, int quantity = 1, string plural = "", int slot = -1, Dictionary<string, int> properties = null, string id = "") : base(foreground, background, glyph) {
            Animation.CurrentFrame[0].Foreground = foreground;
            Animation.CurrentFrame[0].Background = background;
            Animation.CurrentFrame[0].Glyph = glyph;
            Weight = weight;

            Condition = condition;
            Name = name;
            Slot = slot;

            if (plural == "") {
                NamePlural = name + "s";
            } else {
                NamePlural = plural;
            }

            if (id == "") {
                ID = name;
            } else {
                ID = id;
            }

            Quantity = quantity;
            Properties = properties;
        }

        public Item Clone() {
            Color fore = this.Animation.CurrentFrame[0].Foreground;
            Color back = this.Animation.CurrentFrame[0].Background;
            char glyph = (char) this.Animation.CurrentFrame[0].Glyph;
            Item newItem = new Item(fore, back, Name, glyph, Weight, Condition, quantity:Quantity, plural:NamePlural, slot:Slot, properties:Properties, id:ID);

            return newItem;
        }


        public void DamageItem(int dmg) {
            Condition -= dmg;

            if (Condition <= 0) {
                Destroy();
            }
        }


        public void Destroy() {
            GameLoop.World.CurrentMap.Remove(this);
        }


        public double StackWeight() {
            return Weight * Quantity;
        }


        public void UseItem(Player player, Point pos) {
            if ((player.Equipped[0] != null && player.Equipped[0].Properties.ContainsKey("hoe")) || (player.Equipped[13] != null && player.Equipped[13].Properties.ContainsKey("hoe"))) {
                if (GameLoop.World.CurrentMap.GetTileAt<TileBase>(pos.X, pos.Y).Name == "grass") {
                    TileBase tile = GameLoop.World.CurrentMap.GetTileAt<TileBase>(pos.X, pos.Y);
                    
                    GameLoop.World.CurrentMap.Tiles[pos.ToIndex(GameLoop.World.CurrentMap.Width)] = new TileFloor(false, false, "farmland");
                    GameLoop.UIManager.MapConsole.SetSurface(GameLoop.World.CurrentMap.Tiles, GameLoop.World.CurrentMap.Width, GameLoop.World.CurrentMap.Height);

                    player.CurrentStamina -= 5;

                    string tileUpdate = "t_data|farmland|" + pos.X + "|" + pos.Y;
                    GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(tileUpdate));
                    
                }
            }
        }
    }
}