using System;
using Microsoft.Xna.Framework;

namespace TearsInRain.Entities { 
    public class Player : Actor {


        public Player(Color foreground, Color background) : base(foreground, background, 1) {
            Name = "Player";
            TimeLastActed = 0;
            Dexterity = 13;
            Item item = new Item(Color.White, Color.Transparent, "Book of Incredible Tales", '$', 0.5, quantity:21, plural:"Books of Incredible Tales");
            Item hoe = new Item(Color.Gray, Color.Transparent, "Shoddy Hoe", '\\', 3, slot:13);

            Inventory.Add(item);
            Inventory.Add(hoe);
        }
    }
}