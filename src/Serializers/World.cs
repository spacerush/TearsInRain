using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using TearsInRain.Entities;
using Microsoft.Xna.Framework;

namespace TearsInRain.Serializers {
    public class WorldJsonConverter : JsonConverter<World> {
        public override void WriteJson(JsonWriter writer, World value, JsonSerializer serializer) {
            serializer.Serialize(writer, (WorldSerialized)value);
        }

        public override World ReadJson(JsonReader reader, Type objectType, World existingValue,
                                        bool hasExistingValue, JsonSerializer serializer) {
            return serializer.Deserialize<WorldSerialized>(reader);
        }
    }

    /// <summary>
    /// Serialized instance of a <see cref="World"/>.
    /// </summary>
    [DataContract]
    public class WorldSerialized { 
        [DataMember] public Map map;
        [DataMember] public string name;



        public static implicit operator WorldSerialized(World world) {
            var sObj = new WorldSerialized() {
                map = world.CurrentMap,
                name = world.WorldName,
            };

            return sObj;
        }

        public static implicit operator World(WorldSerialized sObj) {
            World world = new World();

            world.CurrentMap = sObj.map;
            world.WorldName = sObj.name;
            
            return world;
        }
    }
}