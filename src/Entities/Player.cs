using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace TearsInRain.Entities {

    [JsonObject(MemberSerialization.OptOut)]
    public class Player : Actor {


        public Player(Color foreground, Color background) : base(foreground, background, 1) {
            Name = "Player";
            TimeLastActed = 0;
            Dexterity = 13;
            Dictionary<string, string> props = new Dictionary<string, string>();
            props.Add("qualities", "tilling");

            Item hoe = GameLoop.ItemLibrary["hoe_shoddy"].Clone();
            Item book = GameLoop.ItemLibrary["book_incredible_tales"].Clone();
            Item seed = GameLoop.ItemLibrary["seed_potato"].Clone();

            Inventory.Add(book);
            Inventory.Add(hoe);
            Inventory.Add(seed);
        }

        public Player(Color foreground, Color background, Actor actor) : base(foreground, background, 1) {
            Name = actor.Name;

            Strength = actor.Strength;
            Dexterity = actor.Dexterity;
            Intelligence = actor.Intelligence;
            Vitality = actor.Vitality;

            Health = actor.Health;
            MaxHealth = actor.MaxHealth;
            Will = actor.Will;
            Perception = actor.Perception;
            CurrentStamina = actor.CurrentStamina;
            MaxStamina = actor.MaxStamina;
            CurrentEnergy = actor.CurrentEnergy;
            MaxEnergy = actor.MaxEnergy;

            Speed = actor.Speed;
            BaseDodge = actor.BaseDodge;
            Dodge = actor.Dodge;

            Inventory = actor.Inventory;
            Equipped = actor.Equipped;
        }
    }
}