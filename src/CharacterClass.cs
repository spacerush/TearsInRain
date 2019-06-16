using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TearsInRain.Entities;
using TearsInRain.Serializers;

namespace TearsInRain.src {

    [JsonConverter(typeof(ClassJsonConverter))]
    public class CharacterClass {
        public string ClassName = "";
        public int RanksPerLv = 4;
        public string ClassSkills = "";
        public string HitDie = "d6";
        public int LevelsInClass = 0;

        public int[] FRW_InitialSaves = { 0, 0, 0 };

        public float WillSave = 0;
        public float FortSave = 0;
        public float ReflexSave = 0;
        public float ClassAttackBonus = 0;

        public int HealthGranted = 0;


        public Dictionary<string, string> PreReqs = new Dictionary<string, string>();


        public CharacterClass(string name, int ranks, string skills, string HD, int levels, int[] initialSaves, float fort, float reflex, float will, float attackBonus, int healthGranted, Dictionary<string, string> reqs) {
            ClassName = name;
            RanksPerLv = ranks;
            ClassSkills = skills;
            HitDie = HD;
            LevelsInClass = levels;
            FRW_InitialSaves = initialSaves;
            WillSave = will;
            FortSave = fort;
            ReflexSave = reflex;
            ClassAttackBonus = attackBonus;
            HealthGranted = healthGranted;

            if (reqs != null) {
                foreach (KeyValuePair<string, string> prereq in reqs) {
                    PreReqs.Add(prereq.Key, prereq.Value);
                }
            }
        }



        public void LevelClass(Actor actor) {
            if (LevelsInClass == 0) {
                LevelsInClass++;
                actor.Level++;
                
                if (actor.Level == 1) { HealthGranted = Convert.ToInt32(HitDie.Substring(1)); }
                else if (actor.Level != 1) { HealthGranted = GoRogue.DiceNotation.Dice.Roll("1" + HitDie); }

                FortSave = FRW_InitialSaves[0];
                ReflexSave = FRW_InitialSaves[1];
                WillSave = FRW_InitialSaves[2];

                actor.ClassSkills += ClassSkills;



                actor.RecalculateHealth();
            }
        }
    }
}
