using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TearsInRain.Tiles;

namespace TearsInRain {
    class Utils {
        public static Dictionary<string, Point> Directions = new Dictionary<string, Point>();


        public static void InitDirections() {
            Directions.Add("UR", new Point(1, -1)); // Num9
            Directions.Add("U", new Point(0, -1)); // Num8, UpArrow
            Directions.Add("UL", new Point(-1, -1)); // Num7
            Directions.Add("R", new Point(1, 0)); // Num6
            Directions.Add("L", new Point(-1, 0)); // Num4
            Directions.Add("DR", new Point(1, 1)); // Num3
            Directions.Add("D", new Point(0, 1)); // Num2
            Directions.Add("DL", new Point(-1, 1)); // Num1
        }


        public static string CalculateMD5Hash(string input) {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++) {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string SimpleMapString(TileBase[] tiles) {
            var simpleMap = tiles.Length.ToString() + "|";


            for (int i = 0; i < tiles.Length; i++) {
                if (tiles[i].Name == "wood floor") { simpleMap += "F"; }
                if (tiles[i].Name == "floor") { simpleMap += "f"; }
                else if (tiles[i].Name == "grass") { simpleMap += "G"; }
                else if (tiles[i].Name == "cornflower") { simpleMap += "1"; }
                else if (tiles[i].Name == "rose") { simpleMap += "2"; } 
                else if (tiles[i].Name == "violet") { simpleMap += "3"; } 
                else if (tiles[i].Name == "dandelion") { simpleMap += "4"; } 
                else if (tiles[i].Name == "tulip") { simpleMap += "5"; } 
                else if (tiles[i] is TileDoor door && door.IsOpen && !door.Locked) { simpleMap += "d"; }
                else if (tiles[i] is TileDoor door1 && !door1.IsOpen && !door1.Locked) { simpleMap += "D"; } 
                else if (tiles[i] is TileDoor door2 && door2.IsOpen && door2.Locked) { simpleMap += "l"; } 
                else if (tiles[i] is TileDoor door3 && !door3.IsOpen && door3.Locked) { simpleMap += "L"; } 
                else if (tiles[i] is TileWall) { simpleMap += "W"; }
            }

            return simpleMap;
        }

        public static TileBase[] GetMapFromString(string tileString) {
            TileBase[] tiles = new TileBase[0];

            for (int i = 0; i < tileString.Length; i++) {
                if (tileString[i].ToString() == "|") {
                    tiles = new TileBase[System.Convert.ToInt32(tileString.Substring(0, i))];
                    tileString = tileString.Substring(i + 1);
                    break;
                }
            }

            for (int j = 0; j < tileString.Length; j++) {

                if (tileString[j].ToString() == "F") {
                    tiles[j] = new TileFloor(type: "room");
                } else if (tileString[j].ToString() == "f") {
                    tiles[j] = new TileFloor(type: "floor");
                } else if (tileString[j].ToString() == "G") {
                    tiles[j] = new TileFloor(type: "just-grass");
                } else if (tileString[j].ToString() == "1") {
                    tiles[j] = new TileFloor(type: "cornflower");
                } else if (tileString[j].ToString() == "2") {
                    tiles[j] = new TileFloor(type: "rose");
                } else if (tileString[j].ToString() == "3") {
                    tiles[j] = new TileFloor(type: "violet");
                } else if (tileString[j].ToString() == "4") {
                    tiles[j] = new TileFloor(type: "dandelion");
                } else if (tileString[j].ToString() == "5") {
                    tiles[j] = new TileFloor(type: "tulip");
                } else if (tileString[j].ToString() == "d") {
                    tiles[j] = new TileDoor(false, true);
                } else if (tileString[j].ToString() == "D") {
                    tiles[j] = new TileDoor(false, false);
                } else if (tileString[j].ToString() == "l") {
                    tiles[j] = new TileDoor(true, true);
                } else if (tileString[j].ToString() == "L") {
                    tiles[j] = new TileDoor(true, false);
                } else if (tileString[j].ToString() == "W") {
                    tiles[j] = new TileWall();
                }
            }

            return tiles; 
        }
    }
}
