using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using TearsInRain.Entities;
using Microsoft.Xna.Framework;

namespace TearsInRain.Serializers {
    public class ActorJsonConverter : JsonConverter<Actor> {
        public override void WriteJson(JsonWriter writer, Actor value, JsonSerializer serializer) {
            serializer.Serialize(writer, (ActorSerialized)value);
        }

        public override Actor ReadJson(JsonReader reader, Type objectType, Actor existingValue,
                                        bool hasExistingValue, JsonSerializer serializer) {
            return serializer.Deserialize<ActorSerialized>(reader);
        }
    }

    /// <summary>
    /// Serialized instance of a <see cref="Entity"/>.
    /// </summary>
    [DataContract]
    public class ActorSerialized {
        // Visuals
        [DataMember] public Color FG; // Foreground
        [DataMember] public Color BG; // Background
        [DataMember] public int G; // Glyph

        [DataMember] public string Name;

        //Primary Stats
        [DataMember] public int ST; // Strength
        [DataMember] public int DX; // Dexterity
        [DataMember] public int IQ; // Intelligence
        [DataMember] public int VT; // Vitality

        // Secondary Direct
        [DataMember] public int HP; // Health
        [DataMember] public int Will;
        [DataMember] public int PER; // Perception
        [DataMember] public int STA; // Stamina

        [DataMember] public int SPD; // Speed
        [DataMember] public int Dodge;


        public static implicit operator ActorSerialized(Actor actor) {
            var serializedObject = new ActorSerialized() {
                FG = actor.Animation.CurrentFrame[0].Foreground,
                BG = actor.Animation.CurrentFrame[0].Background,
                G = actor.Animation.CurrentFrame[0].Glyph,


                Name = actor.Name,
                ST = actor.Strength,
                DX = actor.Dexterity,
                IQ = actor.Intelligence,
                VT = actor.Vitality,

                HP = actor.Health,
                Will = actor.Will,
                PER = actor.Perception,
                STA = actor.Stamina,

                SPD = actor.Speed,
                Dodge = actor.Dodge,
            };

            return serializedObject;
        }

        public static implicit operator Actor(ActorSerialized serializedObject) {
            var entity = new Actor(serializedObject.FG, serializedObject.BG, serializedObject.G);

            entity.Name = serializedObject.Name;

            entity.Strength = serializedObject.ST;
            entity.Dexterity = serializedObject.DX;
            entity.Intelligence = serializedObject.IQ;
            entity.Vitality = serializedObject.VT;

            entity.Health = serializedObject.HP;
            entity.MaxHealth = serializedObject.HP;
            entity.Will = serializedObject.Will;
            entity.Perception = serializedObject.PER;
            entity.Stamina = serializedObject.STA;

            entity.Speed = serializedObject.SPD;
            entity.BaseDodge = serializedObject.Dodge;
            entity.Dodge = serializedObject.Dodge;


            return entity;
        }
    }
}