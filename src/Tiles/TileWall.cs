using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using TearsInRain.Serializers;

namespace TearsInRain.Tiles {

    [JsonConverter(typeof(TileJsonConverter))]
    public class TileWall : TileBase {
        public TileWall(bool blocksMovement=true, bool blocksLOS=true) : base(Color.LightGray, Color.Transparent, 264, blocksMovement, blocksLOS) {
            Name = "wall";
            Foreground = new Color(120, 120, 120);
            Background = new Color(100, 100, 100);
        }
    }
}