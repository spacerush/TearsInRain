using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace TearsInRain.Entities {

    [JsonObject(MemberSerialization.OptOut)]
    public class Monster : Actor {

        public Monster (Color foreground, Color background) : base(foreground, background, 'M') {
            int lootNum = GameLoop.Random.Next(1, 4);

            for (int i = 0; i < lootNum; i++) {
                Item newLoot = GameLoop.ItemLibrary["book_incredible_tales"].Clone();
                Inventory.Add(newLoot);
            }
        }
    }
}