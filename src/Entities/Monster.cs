using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace TearsInRain.Entities {

    [JsonObject(MemberSerialization.OptOut)]
    public class Monster : Actor {

        public Monster (Color foreground, Color background) : base(foreground, background, 'M') {
            int lootNum = GameLoop.Random.Next(1, 4);

            for (int i = 0; i < lootNum; i++) {
                Item newLoot = new Item(Color.HotPink, Color.Transparent, "Spork", 'L', 2);
                Inventory.Add(newLoot);
            }
        }
    }
}