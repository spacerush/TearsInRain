using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SadConsole.Components;
using TearsInRain.UI;

namespace TearsInRain.Entities {

    [JsonObject(MemberSerialization.OptOut)]
    public class Entity : SadConsole.Entities.Entity, GoRogue.IHasID { 
        public uint ID { get; private set; }

        protected Entity(Color foreground, Color background, int glyph, int width = 1, int height = 1) : base(width, height) {
            Animation.CurrentFrame[0].Foreground = foreground;
            Animation.CurrentFrame[0].Background = background;
            Animation.CurrentFrame[0].Glyph = glyph;

            ID = Map.IDGenerator.UseID();
            Components.Add(new EntityViewSyncComponent());
        }
    }
}