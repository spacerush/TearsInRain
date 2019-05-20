using System;
using Microsoft.Xna.Framework;

namespace TearsInRain.Tiles {
    public class TileWall : TileBase {
        public TileWall(bool blocksMovement=true, bool blocksLOS=true) : base(Color.LightGray, Color.Transparent, '#', blocksMovement, blocksLOS) {
            Name = "Wall";
        }
    }
}