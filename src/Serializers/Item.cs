using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TearsInRain.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace TearsInRain.Serializers {
    public class ItemJsonConverter : JsonConverter<Item> {
        public override void WriteJson(JsonWriter writer, Item value, JsonSerializer serializer) {
            serializer.Serialize(writer, (ItemSerialized)value);
        }

        public override Item ReadJson(JsonReader reader, Type objectType, Item existingValue, 
                                        bool hasExistingValue, JsonSerializer serializer) {
            return serializer.Deserialize<ItemSerialized>(reader);
        }
    }

    /// <summary>
    /// Serialized instance of a <see cref="Item"/>.
    /// </summary>
    [DataContract]
    public class ItemSerialized {
        [DataMember] public string ID; // Item ID for referencing
        [DataMember] public string Name; // Item Name
        [DataMember] public string NamePlural; // Item Name, Plural

        [DataMember] public string FG; // Foreground
        [DataMember] public string BG; // Background
        [DataMember] public int Glyph; // Glyph

        [DataMember] public double Weight; // Item Weight in kilograms
        [DataMember] public int Condition; // Item Condition (out of 100)
        [DataMember] public int Slot; // Slot that item can be equipped to. If no applicable slot, this should be -1
        [DataMember] public int Quantity; // Number of items

        [DataMember] public Dictionary<string, int> Properties;

        public static implicit operator ItemSerialized(Item item) {
            Color tempFG = item.Animation.CurrentFrame[0].Foreground;
            Color tempBG = item.Animation.CurrentFrame[0].Background;

            var sObj = new ItemSerialized() {
                FG = tempFG.R.ToString() + "," + tempFG.G.ToString() + "," + tempFG.B.ToString() + "," + tempFG.A.ToString(),
                BG = tempBG.R.ToString() + "," + tempBG.G.ToString() + "," + tempBG.B.ToString() + "," + tempBG.A.ToString(),
                Glyph = item.Animation.CurrentFrame[0].Glyph,
                ID = item.ID,
                Name = item.Name,
                NamePlural = item.NamePlural,
                Weight = item.Weight,
                Condition = item.Condition,
                Slot = item.Slot,
                Quantity = item.Quantity,
                Properties = item.Properties,
            };

            return sObj;
        }

        public static implicit operator Item(ItemSerialized sObj) {
            string[] sFG = sObj.FG.Split(',');
            string[] sBG = sObj.BG.Split(',');
            Color FG = new Color(Convert.ToInt32(sFG[0]), Convert.ToInt32(sFG[1]), Convert.ToInt32(sFG[2]), Convert.ToInt32(sFG[3]));
            Color BG = new Color(Convert.ToInt32(sBG[0]), Convert.ToInt32(sBG[1]), Convert.ToInt32(sBG[2]), Convert.ToInt32(sBG[3]));

            return new Item(FG, BG, sObj.Name, (char)sObj.Glyph, sObj.Weight, sObj.Condition, quantity: sObj.Quantity, plural: sObj.NamePlural, slot: sObj.Slot, properties: sObj.Properties);
        }
    }
}