using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TearsInRain.src;

namespace TearsInRain.Serializers {
    public class ClassJsonConverter : JsonConverter<CharacterClass> {
        public override void WriteJson(JsonWriter writer, CharacterClass value, JsonSerializer serializer) {
            serializer.Serialize(writer, (ClassSerialized)value);
        }

        public override CharacterClass ReadJson(JsonReader reader, Type objectType, CharacterClass existingValue,
                                        bool hasExistingValue, JsonSerializer serializer) {
            return serializer.Deserialize<ClassSerialized>(reader);
        }
    }

    /// <summary>
    /// Serialized instance of a <see cref="Skill"/>.
    /// </summary>
    [DataContract]
    public class ClassSerialized {
        [DataMember] public string ClassName;
        [DataMember] public int RanksPerLv;
        [DataMember] public string ClassSkills;
        [DataMember] public string HitDie;
        [DataMember] public int LevelsInClass;

        [DataMember] public int[] FRW_InitialSaves;
        [DataMember] public float WillSave;
        [DataMember] public float FortSave;
        [DataMember] public float ReflexSave;
        [DataMember] public float ClassAttackBonus;
        [DataMember] public int HealthGranted;
        [DataMember] public Dictionary<string, string> PreReqs = new Dictionary<string, string>();

        public static implicit operator ClassSerialized(CharacterClass obj) {
            var serializedObject = new ClassSerialized() {
                ClassName = obj.ClassName,
                RanksPerLv = obj.RanksPerLv,
                ClassSkills = obj.ClassSkills,
                HitDie = obj.HitDie,
                LevelsInClass = obj.LevelsInClass,
                FRW_InitialSaves = obj.FRW_InitialSaves,
                WillSave = obj.WillSave,
                FortSave = obj.FortSave,
                ReflexSave = obj.ReflexSave,
                ClassAttackBonus = obj.ClassAttackBonus,
                PreReqs = obj.PreReqs,
            };

            return serializedObject;
        }

        public static implicit operator CharacterClass(ClassSerialized sObj) {
            return new CharacterClass(sObj.ClassName, sObj.RanksPerLv, sObj.ClassSkills, sObj.HitDie, sObj.LevelsInClass, sObj.FRW_InitialSaves, sObj.FortSave, sObj.ReflexSave, sObj.WillSave, sObj.ClassAttackBonus, sObj.HealthGranted, sObj.PreReqs); 
        }
    }
}