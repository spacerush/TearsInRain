using System;
using Microsoft.Xna.Framework;

namespace TearsInRain.Entities { 
    public class Player : Actor {


        public Player(Color foreground, Color background) : base(foreground, background, 1) {
            Name = "Player";
            TimeLastActed = 0;
            Dexterity = 13;
            Item item = new Item(Color.White, Color.Transparent, "Book of Incredible Tales", '$', 20, quantity:21, plural:"Books of Incredible Tales");
            Item hoe = new Item(Color.Gray, Color.Transparent, "Shoddy Hoe", '\\', 3, slot:13);

            Inventory.Add(item);
            Inventory.Add(hoe);
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