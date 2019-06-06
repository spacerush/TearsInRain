using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using TearsInRain.Serializers;

namespace TearsInRain.Tiles {

    [JsonConverter(typeof(TileJsonConverter))]
    public class TileFloor : TileBase {
        public TileFloor(bool blocksMovement = false, bool blocksLOS = false, string type="floor") : base(Color.DarkGray, Color.Transparent, 262, blocksMovement, blocksLOS) {
            if (type == "floor") {
                Name = "floor";
            } else if (type == "room") {
                Name = "wood floor";
                Foreground = new Color(86, 44, 0);
                Background = new Color(71, 36, 0);
                Glyph = 265;
            } else if (type == "farmland") {
                Name = "farmland";
                Background = new Color(163, 108, 37);
                Foreground = new Color(209, 137, 43);
                Glyph = '#';
            } else if (type == "grass") {
                Name = "grass";
                Background = new Color(15, 109, 1);
                Foreground = new Color(21, 153, 1);

                var foliage = GameLoop.Random.Next(0, 5);

                if (foliage == 0 || foliage == 1) {
                    Glyph = 262;
                } else if (foliage == 2 || foliage == 3) {
                    Glyph = 263;
                } else {
                    Glyph = 266;
                    var color = GameLoop.Random.Next(4, 10);

                    if (color >= 0 && color <= 5) {
                        Foreground = Color.CornflowerBlue; Name = "cornflower";
                    } else if (color == 6) {
                        Foreground = Color.Red; Name = "rose";
                        Glyph = 268;
                    } else if (color == 7) {
                        Foreground = Color.Purple; Name = "violet";
                        Glyph = 268;
                    } else if (color == 8) {
                        Foreground = Color.Yellow; Name = "dandelion";
                        Glyph = 267;
                    } else {
                        Foreground = Color.HotPink; Name = "tulip";
                    }

                }
            } else if (type == "just-grass") {
                Name = "grass";
                Background = new Color(15, 109, 1);
                Foreground = new Color(21, 153, 1);
                var rand = GameLoop.Random.Next(1, 2);
                if (rand == 1) { Glyph = 262; } else { Glyph = 263; }
            } else if (type == "cornflower") {
                Name = "cornflower";
                Background = new Color(15, 109, 1);
                Foreground = Color.CornflowerBlue;
                Glyph = 266;
            } else if (type == "rose") {
                Name = "rose";
                Background = new Color(15, 109, 1);
                Foreground = Color.Red;
                Glyph = 268;
            } else if (type == "violet") {
                Name = "violet";
                Background = new Color(15, 109, 1);
                Foreground = Color.Purple;
                Glyph = 268;
            } else if (type == "dandelion") {
                Name = "dandelion";
                Background = new Color(15, 109, 1);
                Foreground = Color.Yellow;
                Glyph = 267;
            } else if (type == "tulip") {
                Name = "tulip";
                Background = new Color(15, 109, 1);
                Foreground = Color.HotPink;
                Glyph = 266;
            }
        }
    }
}