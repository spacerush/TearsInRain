using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TearsInRain.Tiles;

namespace TearsInRain.Serializers {
    public class TileJsonConverter : JsonConverter<TileBase> {
        public override void WriteJson(JsonWriter writer, TileBase value, JsonSerializer serializer) {
            serializer.Serialize(writer, (TileSerialized)value);
        }

        public override TileBase ReadJson(JsonReader reader, Type objectType, TileBase existingValue, 
                                        bool hasExistingValue, JsonSerializer serializer) {
            return serializer.Deserialize<TileSerialized>(reader);
        }
    }

    /// <summary>
    /// Serialized instance of a <see cref="TileBase"/>.
    /// </summary>
    [DataContract]
    public class TileSerialized {
        // Visuals
        [DataMember] public string FG; // Foreground
        [DataMember] public string BG; // Background
        [DataMember] public int Glyph; // Glyph

        [DataMember] public string Name; // Item Name
        [DataMember] public bool blocksLOS; // Whether or not this tile blocks Line of Sight
        [DataMember] public bool blocksMove; // Whether or not this tile can be walked on

        [DataMember] public bool isLocked;
        [DataMember] public bool isOpen;

        public static implicit operator TileSerialized(TileBase tile) {
            Color tempFG = tile.Foreground;
            Color tempBG = tile.Background;

            var sObj = new TileSerialized() {
                FG = tempFG.R.ToString() + "," + tempFG.G.ToString() + "," + tempFG.B.ToString() + "," + tempFG.A.ToString(),
                BG = tempBG.R.ToString() + "," + tempBG.G.ToString() + "," + tempBG.B.ToString() + "," + tempBG.A.ToString(),
                Glyph = tile.Glyph,
                Name = tile.Name,
                blocksLOS = tile.IsBlockingLOS,
                blocksMove = tile.IsBlockingMove,
            };


            if (tile is TileDoor door) {
                sObj.isLocked = door.IsLocked;
                sObj.isOpen = door.IsOpen;
            }

            return sObj;
        }

        public static implicit operator TileBase(TileSerialized sObj) {
            string[] sFG = sObj.FG.Split(',');
            string[] sBG = sObj.BG.Split(',');
            Color FG = new Color(Convert.ToInt32(sFG[0]), Convert.ToInt32(sFG[1]), Convert.ToInt32(sFG[2]), Convert.ToInt32(sFG[3]));
            Color BG = new Color(Convert.ToInt32(sBG[0]), Convert.ToInt32(sBG[1]), Convert.ToInt32(sBG[2]), Convert.ToInt32(sBG[3]));

            TileBase newTile = new TileBase(FG, BG, sObj.Glyph, sObj.blocksMove, sObj.blocksLOS, sObj.Name);
            
            newTile.IsLocked = sObj.isLocked;
            newTile.IsOpen = sObj.isOpen;

            return newTile;
        }
    }
}