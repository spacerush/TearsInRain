using System;
using Microsoft.Xna.Framework;

namespace TearsInRain.Entities { 
    public class Player : Actor {


        public Player(Color foreground, Color background) : base(foreground, background, '@') {
            Name = "Player";
            TimeLastActed = 0;
            Dexterity = 13;
            Item item = new Item(Color.White, Color.Transparent, "The Book of Incredible Tales", '$', 0.5);

            Inventory.Add(item);
        }
    }
}