using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TearsInRain.src;

namespace TearsInRain.Serializers {
    public class SkillJsonConverter : JsonConverter<Skill> {
        public override void WriteJson(JsonWriter writer, Skill value, JsonSerializer serializer) {
            serializer.Serialize(writer, (SkillSerialized)value);
        }

        public override Skill ReadJson(JsonReader reader, Type objectType, Skill existingValue,
                                        bool hasExistingValue, JsonSerializer serializer) {
            return serializer.Deserialize<SkillSerialized>(reader);
        }
    }

    /// <summary>
    /// Serialized instance of a <see cref="Skill"/>.
    /// </summary>
    [DataContract]
    public class SkillSerialized {
        [DataMember] public string Name;
        [DataMember] public string ControllingAttribute;
        [DataMember] public int Ranks;

        public static implicit operator SkillSerialized(Skill skill) {
            var serializedObject = new SkillSerialized() { 
                Name = skill.Name, 
                ControllingAttribute = skill.ControllingAttribute,
                Ranks = skill.Ranks, 
            };

            return serializedObject;
        }

        public static implicit operator Skill(SkillSerialized sObj) {
            return new Skill(sObj.Name, sObj.ControllingAttribute, sObj.Ranks); 
        }
    }
}