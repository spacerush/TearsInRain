using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using TearsInRain.Serializers;

namespace TearsInRain.Entities {

    [JsonConverter(typeof(ItemJsonConverter))]
    public class Item : Entity {
        public int Condition;
        public double Weight;
        public int Quantity;
        public string NamePlural;
        public int Slot;

        public Item(Color foreground, Color background, string name, char glyph, double weight = 1, int condition = 100, int width = 1, int height = 1, int quantity = 1, string plural = "", int slot = -1) : base(foreground, background, glyph) {
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

            Quantity = quantity;
        }

        public Item Clone() {
            Color fore = this.Animation.CurrentFrame[0].Foreground;
            Color back = this.Animation.CurrentFrame[0].Background;
            char glyph = (char) this.Animation.CurrentFrame[0].Glyph;
            Item newItem = new Item(fore, back, Name, glyph, Weight, Condition, quantity:Quantity, plural:NamePlural, slot:Slot);

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
    }
}