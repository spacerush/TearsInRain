using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TearsInRain.src;

namespace TearsInRain.Serializers {
    public class RaceJsonConverter : JsonConverter<CharacterRace> {
        public override void WriteJson(JsonWriter writer, CharacterRace value, JsonSerializer serializer) {
            serializer.Serialize(writer, (RaceSerialized)value);
        }

        public override CharacterRace ReadJson(JsonReader reader, Type objectType, CharacterRace existingValue,
                                        bool hasExistingValue, JsonSerializer serializer) {
            return serializer.Deserialize<RaceSerialized>(reader);
        }
    }

    /// <summary>
    /// Serialized instance of a <see cref="CharacterRace"/>.
    /// </summary>
    [DataContract]
    public class RaceSerialized {
        [DataMember] public string RaceName;
        [DataMember] public int[] AbilityScoreMods;
        [DataMember] public uint SkinColor;

        public static implicit operator RaceSerialized(CharacterRace obj) {
            var serializedObject = new RaceSerialized() {
                RaceName = obj.RaceName,
                AbilityScoreMods = obj.AbilityScoreMods,
                SkinColor = obj.SkinColor.PackedValue,
            };

            return serializedObject;
        }

        public static implicit operator CharacterRace(RaceSerialized sObj) {
            return new CharacterRace(sObj.RaceName, sObj.AbilityScoreMods, new Microsoft.Xna.Framework.Color(sObj.SkinColor)); 
        }
    }
}