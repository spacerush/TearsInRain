using System;
using Microsoft.Xna.Framework;

namespace TearsInRain.Tiles { 
    public class TileFloor : TileBase {
        public TileFloor(bool blocksMovement = false, bool blocksLOS = false, string type="floor") : base(Color.DarkGray, Color.Transparent, '.', blocksMovement, blocksLOS) {
            if (type == "floor") {
                Name = "Floor";
            } else if (type == "room") {
                Name = "Floor";
                Foreground = new Color(86, 44, 0);
                Background = new Color(71, 36, 0);
                Glyph = '_';
            } else if (type == "grass") {
                Name = "Grass";
                this.Background = new Color(15, 109, 1);
                this.Foreground = new Color(21, 153, 1);

                var foliage = GameLoop.Random.Next(0, 5);

                if (foliage == 0 || foliage == 1) {
                    this.Glyph = '.';
                } else if (foliage == 2 || foliage == 3) {
                    this.Glyph = ',';
                } else {
                    this.Glyph = '*';
                    var color = GameLoop.Random.Next(4, 10);

                    if (color >= 0 && color <= 5) { Foreground = Color.CornflowerBlue; Name = "Cornflower"; } else if (color == 6) { Foreground = Color.Red; Name = "Rose"; } else if (color == 7) { Foreground = Color.Purple; Name = "Violet"; } else if (color == 8) { Foreground = Color.Yellow; Name = "Dandelion"; } else{ Foreground = Color.HotPink; Name = "Tulip"; }

                }
            }
        }
    }
}