using System;
using Microsoft.Xna.Framework;

namespace TearsInRain.Tiles { 
    public class TileFloor : TileBase {
        public TileFloor(bool blocksMovement = false, bool blocksLOS = false, string type="floor") : base(Color.DarkGray, Color.Transparent, '.', blocksMovement, blocksLOS) {
            if (type == "floor") {
                Name = "floor";
            } else if (type == "room") {
                Name = "wood floor";
                Foreground = new Color(86, 44, 0);
                Background = new Color(71, 36, 0);
                Glyph = '_';
            } else if (type == "grass") {
                Name = "grass";
                Background = new Color(15, 109, 1);
                Foreground = new Color(21, 153, 1);

                var foliage = GameLoop.Random.Next(0, 5);

                if (foliage == 0 || foliage == 1) {
                    Glyph = '.';
                } else if (foliage == 2 || foliage == 3) {
                    Glyph = ',';
                } else {
                    Glyph = '*';
                    var color = GameLoop.Random.Next(4, 10);

                    if (color >= 0 && color <= 5) { Foreground = Color.CornflowerBlue; Name = "cornflower"; } else if (color == 6) { Foreground = Color.Red; Name = "rose"; } else if (color == 7) { Foreground = Color.Purple; Name = "violet"; } else if (color == 8) { Foreground = Color.Yellow; Name = "dandelion"; } else{ Foreground = Color.HotPink; Name = "tulip"; }

                }
            } else if (type == "just-grass") {
                Name = "grass";
                Background = new Color(15, 109, 1);
                Foreground = new Color(21, 153, 1);
                var rand = GameLoop.Random.Next(1, 2);
                if (rand == 1) { Glyph = '.';  } else { Glyph = ','; }
            } else if (type == "cornflower") {
                Name = "cornflower";
                Background = new Color(15, 109, 1);
                Foreground = Color.CornflowerBlue;
                Glyph = '*';
            } else if (type == "rose") {
                Name = "rose";
                Background = new Color(15, 109, 1);
                Foreground = Color.Red;
                Glyph = '*';
            } else if (type == "violet") {
                Name = "violet";
                Background = new Color(15, 109, 1);
                Foreground = Color.Purple;
                Glyph = '*';
            } else if (type == "dandelion") {
                Name = "dandelion";
                Background = new Color(15, 109, 1);
                Foreground = Color.Yellow;
                Glyph = '*';
            } else if (type == "tulip") {
                Name = "tulip";
                Background = new Color(15, 109, 1);
                Foreground = Color.HotPink;
                Glyph = '*';
            }
        }
    }
}