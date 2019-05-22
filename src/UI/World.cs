using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TearsInRain.Entities;
using TearsInRain.Tiles;

namespace TearsInRain.UI {
    public class World {
        private int _mapWidth = 32;
        private int _mapHeight = 32;
        private TileBase[] _mapTiles;
        private int _maxRooms = 20;
        private int _minRoomSize = 4;
        private int _maxRoomSize = 15;

        Random rndNum = new Random();

        public Map CurrentMap { get; set; } 
        public Player Player { get; set; }

        public World() {
            CreateMap();
            CreatePlayer();
            CreateMonsters();
            CreateLoot();
        }

        private void CreateMap() {
            _mapTiles = new TileBase[_mapWidth * _mapHeight];
            CurrentMap = new Map(_mapWidth, _mapHeight);
            MapGenerator mapGen = new MapGenerator();
            CurrentMap = mapGen.GenerateMap(_mapWidth, _mapHeight, _maxRooms, _minRoomSize, _maxRoomSize);
        }

        private void CreatePlayer() {
            Player = new Player(Color.Yellow, Color.Transparent);

            //for (int i = 0; i < CurrentMap.Tiles.Length; i++) {
            //    if (!CurrentMap.Tiles[i].IsBlockingMove) {
            //        Player.Position = SadConsole.Helpers.GetPointFromIndex(i, CurrentMap.Width);
            //        break;
            //    }
            //}

            foreach(KeyValuePair<Point, TileBase> tile in CurrentMap._TileDict) {
                if (!tile.Value.IsBlockingMove) {
                    Player.Position = tile.Key;
                }
            }

            CurrentMap.Add(Player);
        }

        private void CreateMonsters() {
            int numMonsters = 10;

            for (int i = 0; i < numMonsters; i++) {
                Point monsterPosition = new Point(0, 0);
                Monster newMonster = new Monster(Color.Blue, Color.Transparent);

                while (!CurrentMap._TileDict.ContainsKey(monsterPosition) || CurrentMap._TileDict[monsterPosition].IsBlockingMove) {
                    monsterPosition = new Point(rndNum.Next(CurrentMap.Width), rndNum.Next(CurrentMap.Height));
                }

                newMonster.Defense = rndNum.Next(0, 10);
                newMonster.DefenseChance = rndNum.Next(0, 50);
                newMonster.Attack = rndNum.Next(0, 10);
                newMonster.AttackChance = rndNum.Next(0, 50);
                newMonster.Name = "a common troll";

                newMonster.Position = monsterPosition;
                CurrentMap.Add(newMonster);
            }
        }

        private void CreateLoot() {
            int numLoot = 20;

            for (int i = 0; i < numLoot; i++) {
                Point lootPosition = new Point(0, 0);
                Item newLoot = new Item(Color.Green, Color.Transparent, "fancy shirt", 'L', 2);

                while (!CurrentMap._TileDict.ContainsKey(lootPosition) || CurrentMap._TileDict[lootPosition].IsBlockingMove) {
                    lootPosition = new Point(rndNum.Next(CurrentMap.Width), rndNum.Next(CurrentMap.Height));
                }

                newLoot.Position = lootPosition;
                CurrentMap.Add(newLoot);
            }
        }
    }
}