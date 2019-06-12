using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using TearsInRain.Entities;
using Microsoft.Xna.Framework;
using TearsInRain.Tiles;

namespace TearsInRain.Serializers {
    public class MapJsonConverter : JsonConverter<Map> {
        public override void WriteJson(JsonWriter writer, Map value, JsonSerializer serializer) {
            serializer.Serialize(writer, (MapSerialized)value);
        }

        public override Map ReadJson(JsonReader reader, Type objectType, Map existingValue,
                                        bool hasExistingValue, JsonSerializer serializer) {
            try {
                return serializer.Deserialize<MapSerialized>(reader);
            } catch (Exception e) {
                System.Console.WriteLine(e);
            }

            return null;
        }
    }

    /// <summary>
    /// Serialized instance of a <see cref="Map"/>.
    /// </summary>
    [DataContract]
    public class MapSerialized {
        [DataMember] public TileBase[] tiles;
        [DataMember] public int mapW;
        [DataMember] public int mapH;
        [DataMember] public List<Entity> entities = new List<Entity>();




        public static implicit operator MapSerialized(Map map) {
            List<Entity> ents = new List<Entity>();

            foreach (Entity entity in map.Entities) {
                ents.Add(entity);
            }


            var sObj = new MapSerialized() {
                tiles = map.Tiles,
                entities = ents,
                mapW = map.Width,
                mapH = map.Height,
            };

            return sObj;
        }

        public static implicit operator Map(MapSerialized sObj) {
            GameLoop.ReceivedEntities = new GoRogue.MultiSpatialMap<Entity>();
            GameLoop.ReceivedEntities.Clear();

            for (int i = 1; i < sObj.entities.Count; i++) {
                sObj.entities[i].IsVisible = false;
                sObj.entities[i].IsDirty = true;

                GameLoop.ReceivedEntities.Add(sObj.entities[i], sObj.entities[i].Position);
            }

            return new Map(sObj.tiles, sObj.mapW, sObj.mapH);
        }
    }
}