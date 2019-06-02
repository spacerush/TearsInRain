using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SadConsole;
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
            
            Font = Global.LoadFont("fonts/Cheepicus12.font").GetFont(GameLoop.UIManager.hold);
            Position = Position;
            IsDirty = true;
            GameLoop.UIManager.IsDirty = true;

            ID = Map.IDGenerator.UseID();
            Components.Add(new EntityViewSyncComponent());


            IsVisible = false;
        }

        public void addParts() {
            ID = Map.IDGenerator.UseID();
            Components.Add(new EntityViewSyncComponent());

            IsVisible = false;
        }
    }
}