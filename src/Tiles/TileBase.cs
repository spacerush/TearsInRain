using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SadConsole;

namespace TearsInRain.Tiles {

    [JsonObject(MemberSerialization.OptOut)]
    public class TileBase : Cell {
        public bool IsBlockingMove;
        public bool IsBlockingLOS;
        public string Name;
        public bool IsExplored = false; 

        public TileBase(Color foreground, Color background, int glyph, bool blockingMove=false, bool blockingLOS=false, String name="") : base(foreground, background, glyph) {
            IsBlockingMove = blockingMove;
            IsBlockingLOS = blockingLOS;
            Name = name;
            IsVisible = false;
        } 

        public void Darken(bool isGray) {
            if (isGray) {
                Foreground.A = 200;
                Background.A = 200;
            } else {
                Foreground.A = 255;
                Background.A = 255;
            }
        }
    }
}