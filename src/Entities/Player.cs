using System;
using Microsoft.Xna.Framework;

namespace TearsInRain.Entities { 
    public class Player : Actor {


        public Player(Color foreground, Color background) : base(foreground, background, '@') {
            Name = "Player";
            TimeLastActed = 0;
        }
    }
}