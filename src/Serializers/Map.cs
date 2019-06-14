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
        [DataMember] public List<Actor> entities;
        [DataMember] public List<Item> items;
        [DataMember] public List<TerrainFeature> terrain;



        public static implicit operator MapSerialized(Map map) {
            List<Actor> ents = new List<Actor>();
            List<Item> items = new List<Item>();
            List<TerrainFeature> TFs = new List<TerrainFeature>();

            foreach (Entity entity in map.Entities.Items) {
                if (entity is Item item) {
                    items.Add(item);
                } else if (entity is TerrainFeature terr) {
                    TFs.Add(terr);
                } else if (entity is Actor actor && !(entity is Player)) {
                    ents.Add(actor);
                } 
            }




            var sObj = new MapSerialized() {
                tiles = map.Tiles,
                entities = ents,
                items = items,
                terrain = TFs,
                mapW = map.Width,
                mapH = map.Height,
            };

            return sObj;
        }

        public static implicit operator Map(MapSerialized sObj) {
            Map map = new Map(sObj.tiles, sObj.mapW, sObj.mapH);

            map.Entities = new GoRogue.MultiSpatialMap<Entity>();

            for (int i = 0; i < sObj.entities.Count; i++) {
                map.Add(sObj.entities[i]);
            }

            for (int i = 0; i < sObj.items.Count; i++) {
                map.Add(sObj.items[i]);
            }

            for (int i = 0; i < sObj.terrain.Count; i++) {
                map.Add(sObj.terrain[i]);
            }

            return map;
        }
    }
}