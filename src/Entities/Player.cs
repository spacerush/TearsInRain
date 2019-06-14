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

        public Player(Actor actor, Point pos) : base(actor.Animation.CurrentFrame[0].Foreground, actor.Animation.CurrentFrame[0].Background, 1) {
            Name = actor.Name;

            Strength = actor.Strength;
            Dexterity = actor.Dexterity;
            Intelligence = actor.Intelligence;
            Vitality = actor.Vitality;

            Health = actor.Health;
            MaxHealth = actor.MaxHealth;
            Will = actor.Will;
            Perception = actor.Perception;


            MaxStamina = actor.MaxStamina;
            CurrentStamina = actor.CurrentStamina;

            MaxEnergy = actor.MaxEnergy;
            CurrentEnergy = actor.CurrentEnergy;
            Carrying_Weight = actor.Carrying_Weight;
            Carrying_Volume = actor.Carrying_Volume;
            MaxCarriedVolume = actor.MaxCarriedVolume;

            BaseSpeed = actor.BaseSpeed;
            Speed = actor.Speed;

            EncumbranceLv = actor.EncumbranceLv;
            BasicLift = actor.BasicLift;
            HeldGold = actor.HeldGold;

            BaseDodge = actor.BaseDodge;
            Dodge = actor.Dodge;

            IsStealthing = actor.IsStealthing;
            FailedStealth = actor.FailedStealth;
            StealthResult = actor.StealthResult;
            BaseStealthResult = actor.BaseStealthResult;

            Inventory = actor.Inventory;

            Equipped = actor.Equipped;

            Position = pos;
            
        }
    }
}