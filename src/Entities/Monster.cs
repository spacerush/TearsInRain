using System;
using Microsoft.Xna.Framework;

namespace TearsInRain.Entities {
    public class Monster : Actor {
        Random rndNum = new Random();

        public Monster (Color foreground, Color background) : base(foreground, background, 'M') {
            int lootNum = rndNum.Next(1, 4);

            for (int i = 0; i < lootNum; i++) {
                Item newLoot = new Item(Color.HotPink, Color.Transparent, "Spork", 'L', 2);
                Inventory.Add(newLoot);
            }
        }
    }
}