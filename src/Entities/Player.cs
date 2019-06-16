using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace TearsInRain.Entities {

    [JsonObject(MemberSerialization.OptOut)]
    public class Player : Actor {


        public Player(Color foreground, Color background) : base(foreground, background, 1) {
            Name = "Player";
            TimeLastActed = 0;
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
            Constitution = actor.Constitution;
            Intelligence = actor.Intelligence;
            Wisdom = actor.Wisdom;
            Charisma = actor.Charisma;

            Level = actor.Level;

            Health = actor.Health;
            MaxHealth = actor.MaxHealth;


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

            Skills = actor.Skills;
            ClassSkills = actor.ClassSkills;
            RanksPerLvl = actor.RanksPerLvl;
            MiscRanksMod = actor.MiscRanksMod;

            Equipped = actor.Equipped;

            Position = pos;
            
        }
    }
}