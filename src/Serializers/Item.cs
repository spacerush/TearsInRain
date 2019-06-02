using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TearsInRain.Entities;
using Microsoft.Xna.Framework;

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
        // Visuals
        [DataMember] public Color FG; // Foreground
        [DataMember] public Color BG; // Background
        [DataMember] public int G; // Glyph

        [DataMember] public string Name; // Item Name
        [DataMember] public string NamePlural; // Item Name, Plural
        [DataMember] public double Weight; // Item Weight in kilograms
        [DataMember] public int Condition; // Item Condition (out of 100)
        [DataMember] public int Slot; // Slot that item can be equipped to. If no applicable slot, this should be -1
        [DataMember] public int Quantity; // Number of items



        public static implicit operator ItemSerialized(Item item) {
            var sObj = new ItemSerialized() {
                FG = item.Animation.CurrentFrame[0].Foreground,
                BG = item.Animation.CurrentFrame[0].Background,
                G = item.Animation.CurrentFrame[0].Glyph,
                Name = item.Name,
                NamePlural = item.NamePlural,
                Weight = item.Weight,
                Condition = item.Condition,
                Slot = item.Slot,
                Quantity = item.Quantity,
            };

            return sObj;
        }

        public static implicit operator Item(ItemSerialized sObj) {
            return new Item(sObj.FG, sObj.BG, sObj.Name, (char)sObj.G, sObj.Weight, sObj.Condition, quantity: sObj.Quantity, plural: sObj.NamePlural, slot: sObj.Slot);
        }
    }
}