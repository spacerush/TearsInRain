using System; 
using Microsoft.Xna.Framework; 
using TearsInRain.Tiles;
using TearsInRain.Entities;
using System.Collections.Generic;
using System.Linq;
using SadConsole.Effects;

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

        public List<Point> SeenTiles = new List<Point>();

        public Dictionary<long, Player> players = new Dictionary<long, Player>();
        
        public World() {
            CreateMap();
            
            CreatePlayer(GameLoop.NetworkingManager.myUID);
            
            CreateMonsters();
            
            CreateLoot();

            SeenTiles.Add(players[GameLoop.NetworkingManager.myUID].Position);
        }

        public World(TileBase[] tiles) {
            _mapTiles = new TileBase[tiles.Length];
            _mapTiles = tiles;

            CurrentMap = new Map(tiles.Length, 1);
            CurrentMap.Tiles = tiles; 

            CreatePlayer(GameLoop.NetworkingManager.myUID);
            CreateMonsters();
            CreateLoot();

            SeenTiles.Add(players[GameLoop.NetworkingManager.myUID].Position); 
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
                int monsterPosition = rndNum.Next(0, CurrentMap.Width * CurrentMap.Height);
                Monster newMonster = new Monster(Color.Blue, Color.Transparent);

                while (CurrentMap.Tiles[monsterPosition].IsBlockingMove) {
                    monsterPosition = rndNum.Next(0, CurrentMap.Width * CurrentMap.Height);
                }
                
               
                newMonster.Name = "the common troll";
                
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
                int lootPosition = rndNum.Next(0, CurrentMap.Width * CurrentMap.Height);
                Item newLoot = new Item(Color.Green, Color.Transparent, "fancy shirt", 'L', 2);
                
                while (CurrentMap.Tiles[lootPosition].IsBlockingMove) {
                    lootPosition = rndNum.Next(0, CurrentMap.Width * CurrentMap.Height);
                }
                
                newLoot.Position = new Point(lootPosition % CurrentMap.Width, lootPosition / CurrentMap.Width);
                
                CurrentMap.Add(newLoot);
            }

        }



        public void CalculateFov() {
            // Use a GoRogue class that creates a map view so that the IsTransparent function is called whenever FOV asks for the value of a position
            var fovMap = new GoRogue.MapViews.LambdaMapView<bool>(CurrentMap.Width, CurrentMap.Height, CurrentMap.IsTransparent);
            GoRogue.FOV fov = new GoRogue.FOV(fovMap);

            if (GameLoop.World.players.ContainsKey(GameLoop.NetworkingManager.myUID)) {
                Point start = GameLoop.World.players[GameLoop.NetworkingManager.myUID].Position;
                fov.Calculate(start, 10);

                foreach (var spot in fov.NewlySeen) {
                    TileBase tile = CurrentMap.GetTileAt<TileBase>(spot.X, spot.Y);
                    tile.IsVisible = true;
                    if (!SeenTiles.Contains(spot)) {
                        SeenTiles.Add(new Point(spot.X, spot.Y));
                    }
                }


                for (int i = SeenTiles.Count - 1; i > 0; i--) {
                    var spot = SeenTiles[i];

                    if (!fov.CurrentFOV.Contains(new GoRogue.Coord(spot.X, spot.Y))) {
                        TileBase tile = CurrentMap.GetTileAt<TileBase>(spot.X, spot.Y);
                        tile.Grayscale(true);


                        SeenTiles.Remove(spot);
                    } else {
                        TileBase tile = CurrentMap.GetTileAt<TileBase>(spot.X, spot.Y);
                        tile.Grayscale(false);

                        GameLoop.UIManager.MapConsole.ClearDecorators(spot.X, spot.Y, 1);
                    }
                }

                GameLoop.UIManager.MapConsole.IsDirty = true;
            }
        }
    }
}
