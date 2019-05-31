using System;
using Microsoft.Xna.Framework;

namespace TearsInRain.Entities {
    public class Item : Entity {
        public int Condition;
        public double Weight;
        public int Quantity;

        public Item(Color foreground, Color background, string name, char glyph, double weight = 1, int condition = 100, int width = 1, int height = 1, int quantity = 1) : base(foreground, background, glyph) {
            Animation.CurrentFrame[0].Foreground = foreground;
            Animation.CurrentFrame[0].Background = background;
            Animation.CurrentFrame[0].Glyph = glyph;
            Weight = weight;

            Condition = condition;
            Name = name;
            Quantity = quantity;
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