using System; 
using Microsoft.Xna.Framework; 
using TearsInRain.Tiles;
using TearsInRain.Entities;
using System.Collections.Generic;

namespace TearsInRain {
    public class World {
        Random rndNum = new Random();
        
        private int _mapWidth = 100;
        private int _mapHeight = 100;
        private TileBase[] _mapTiles;
        private int _maxRooms = 100;
        private int _minRoomSize = 4;
        private int _maxRoomSize = 15;
        public Map CurrentMap { get; set; }

        public Dictionary<long, Player> players = new Dictionary<long, Player>();
        
        public World() {
            CreateMap();
            
            CreatePlayer(GameLoop.NetworkingManager.myUID);
            
            CreateMonsters();
            
            CreateLoot();
        }

        public World(TileBase[] tiles) {
            _mapTiles = new TileBase[tiles.Length];
            _mapTiles = tiles;

            CurrentMap = new Map(tiles.Length, 1);
            CurrentMap.Tiles = tiles; 

            CreatePlayer(GameLoop.NetworkingManager.myUID);
            CreateMonsters();
            CreateLoot();
        }

        private void CreateMap() {
            _mapTiles = new TileBase[_mapWidth * _mapHeight];
            CurrentMap = new Map(_mapWidth, _mapHeight);
            MapGenerator mapGen = new MapGenerator();
            CurrentMap = mapGen.GenerateMap(_mapWidth, _mapHeight, _maxRooms, _minRoomSize, _maxRoomSize);
        }
        
        private void CreateMonsters() {
            int numMonsters = 10;
            for (int i = 0; i < numMonsters; i++) {
                int monsterPosition = 0;
                Monster newMonster = new Monster(Color.Blue, Color.Transparent);

                while (CurrentMap.Tiles[monsterPosition].IsBlockingMove) {
                    monsterPosition = rndNum.Next(0, CurrentMap.Width * CurrentMap.Height);
                }
                
                newMonster.Defense = rndNum.Next(0, 10);
                newMonster.DefenseChance = rndNum.Next(0, 50);
                newMonster.Attack = rndNum.Next(0, 10);
                newMonster.AttackChance = rndNum.Next(0, 50);
                newMonster.Name = "a common troll";
                
                newMonster.Position = new Point(monsterPosition % CurrentMap.Width, monsterPosition / CurrentMap.Width);
                CurrentMap.Add(newMonster);
            }
        }
        
        public void CreatePlayer(long playerUID) {
            Player newPlayer = new Player(Color.Yellow, Color.Transparent);
            
            for (int i = 0; i < CurrentMap.Tiles.Length; i++) {
                if (!CurrentMap.Tiles[i].IsBlockingMove) {
                    newPlayer.Position = SadConsole.Helpers.GetPointFromIndex(i, CurrentMap.Width);
                    break;
                }
            }

            if (!players.ContainsKey(playerUID)) {
                players.Add(playerUID, newPlayer);
                CurrentMap.Add(players[playerUID]);
            }
        }
        
        private void CreateLoot() {
            int numLoot = 20;
            
            for (int i = 0; i < numLoot; i++) {
                int lootPosition = 0;
                Item newLoot = new Item(Color.Green, Color.Transparent, "fancy shirt", 'L', 2);
                
                while (CurrentMap.Tiles[lootPosition].IsBlockingMove) {
                    lootPosition = rndNum.Next(0, CurrentMap.Width * CurrentMap.Height);
                }
                
                newLoot.Position = new Point(lootPosition % CurrentMap.Width, lootPosition / CurrentMap.Width);
                
                CurrentMap.Add(newLoot);
            }

        }
    }
}
