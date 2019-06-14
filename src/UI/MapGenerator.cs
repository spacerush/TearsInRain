using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TearsInRain.Entities;
using TearsInRain.Tiles;

namespace TearsInRain {
    public class MapGenerator {
        Random randNum = new Random();

        Map _map;
        public MapGenerator() {
        }


        public Map GenerateMap(int mapWidth, int mapHeight, int maxRooms, int minRoomSize, int maxRoomSize) {
            _map = new Map(mapWidth, mapHeight);
            
            //List<Rectangle> Rooms = new List<Rectangle>();

            //for (int i = 0; i < maxRooms; i++) {
            //    int newRoomWidth = randNum.Next(minRoomSize, maxRoomSize);
            //    int newRoomHeight = randNum.Next(minRoomSize, maxRoomSize);
                
            //    int newRoomX = randNum.Next(0, mapWidth - newRoomWidth - 1);
            //    int newRoomY = randNum.Next(0, mapHeight - newRoomHeight - 1);
                
            //    Rectangle newRoom = new Rectangle(newRoomX, newRoomY, newRoomWidth, newRoomHeight);
                
            //    bool newRoomIntersects = Rooms.Any(room => newRoom.Intersects(room));

            //    if (!newRoomIntersects) {
            //        Rooms.Add(newRoom);
            //    }
            //}
            
            FloodFloor();

            //foreach (Rectangle room in Rooms) {
            //    CreateRoom(room);
            //}

            //for (int r = 1; r < Rooms.Count; r++) {
            //    Point previousRoomCenter = Rooms[r - 1].Center;
            //    Point currentRoomCenter = Rooms[r].Center;

            //    if (randNum.Next(1, 2) == 1) {
            //        CreateHorizontalTunnel(previousRoomCenter.X, currentRoomCenter.X, previousRoomCenter.Y);
            //        CreateVerticalTunnel(previousRoomCenter.Y, currentRoomCenter.Y, currentRoomCenter.X);
            //    } else {
            //        CreateVerticalTunnel(previousRoomCenter.Y, currentRoomCenter.Y, previousRoomCenter.X);
            //        CreateHorizontalTunnel(previousRoomCenter.X, currentRoomCenter.X, currentRoomCenter.Y);
            //    }
            //}

            //foreach (Rectangle room in Rooms) {
            //    CreateDoor(room);
            //}
            
            return _map;
        }
        
        private void FloodFloor() {
            for (int i = 0; i < _map.Tiles.Length; i++) {
                int foliage = GameLoop.Random.Next(0, 5);
                if (foliage != 4) {
                    _map.Tiles[i] = GameLoop.TileLibrary["grass"].Clone();
                } else {
                    int flowerType = GameLoop.Random.Next(0, 5);

                    switch(flowerType) {
                        case 0:
                            _map.Tiles[i] = GameLoop.TileLibrary["cornflower"].Clone();
                            break;
                        case 1:
                            _map.Tiles[i] = GameLoop.TileLibrary["rose"].Clone();
                            break;
                        case 2:
                            _map.Tiles[i] = GameLoop.TileLibrary["violet"].Clone();
                            break;
                        case 3:
                            _map.Tiles[i] = GameLoop.TileLibrary["dandelion"].Clone();
                            break;
                        default: 
                            _map.Tiles[i] = GameLoop.TileLibrary["tulip"].Clone();
                            break;
                    }
                }
            }
        }
        
        private void CreateRoom(Rectangle room) {
            for (int x = room.Left + 1; x < room.Right; x++) {
                for (int y = room.Top + 1; y < room.Bottom; y++) {
                    CreateFloor(new Point(x, y));
                }
            }
            
            List<Point> perimeter = GetBorderCellLocations(room);
            foreach (Point location in perimeter) {
                CreateWall(location);
            }
        }
        
        private void CreateFloor(Point location) {
            _map.Tiles[location.ToIndex(_map.Width)] = GameLoop.TileLibrary["wood floor"].Clone();
        }
        
        private void CreateWall(Point location) {
            _map.Tiles[location.ToIndex(_map.Width)] = GameLoop.TileLibrary["wall"].Clone();
        }
        
        private List<Point> GetBorderCellLocations(Rectangle room) {
            int xMin = room.Left;
            int xMax = room.Right;
            int yMin = room.Top;
            int yMax = room.Bottom;
            
            List<Point> borderCells = GetTileLocationsAlongLine(xMin, yMin, xMax, yMin).ToList();
            borderCells.AddRange(GetTileLocationsAlongLine(xMin, yMin, xMin, yMax));
            borderCells.AddRange(GetTileLocationsAlongLine(xMin, yMax, xMax, yMax));
            borderCells.AddRange(GetTileLocationsAlongLine(xMax, yMin, xMax, yMax));

            return borderCells;
        }
        
        private void CreateHorizontalTunnel(int xStart, int xEnd, int yPosition) {
            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++) {
                CreateFloor(new Point(x, yPosition));
            }
        }
        
        private void CreateVerticalTunnel(int yStart, int yEnd, int xPosition) {
            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++) {
                CreateFloor(new Point(xPosition, y));
            }
        }
        
        private void CreateDoor(Rectangle room) {
            List<Point> borderCells = GetBorderCellLocations(room);
            
            foreach (Point location in borderCells) {
                int locationIndex = location.ToIndex(_map.Width);
                if (IsPotentialDoor(location)) {
                    TileDoor newDoor = new TileDoor(false, false);
                    _map.Tiles[locationIndex] = newDoor;

                }
            }
        }
        
        private bool IsPotentialDoor(Point location) {
            int locationIndex = location.ToIndex(_map.Width);
            if (_map.Tiles[locationIndex] != null && _map.Tiles[locationIndex].Name == "wall") {
                return false;
            }
            
            Point right = new Point(location.X + 1, location.Y);
            Point left = new Point(location.X - 1, location.Y);
            Point top = new Point(location.X, location.Y - 1);
            Point bottom = new Point(location.X, location.Y + 1);
            
            if (_map.GetTileAt<TileDoor>(location.X, location.Y) != null ||
                _map.GetTileAt<TileDoor>(right.X, right.Y) != null ||
                _map.GetTileAt<TileDoor>(left.X, left.Y) != null ||
                _map.GetTileAt<TileDoor>(top.X, top.Y) != null ||
                _map.GetTileAt<TileDoor>(bottom.X, bottom.Y) != null
               ) {
                return false;
            }
            
            if (!_map.Tiles[right.ToIndex(_map.Width)].IsBlockingMove && !_map.Tiles[left.ToIndex(_map.Width)].IsBlockingMove && _map.Tiles[top.ToIndex(_map.Width)].IsBlockingMove && _map.Tiles[bottom.ToIndex(_map.Width)].IsBlockingMove) {
                return true;
            }

            if (_map.Tiles[right.ToIndex(_map.Width)].IsBlockingMove && _map.Tiles[left.ToIndex(_map.Width)].IsBlockingMove && !_map.Tiles[top.ToIndex(_map.Width)].IsBlockingMove && !_map.Tiles[bottom.ToIndex(_map.Width)].IsBlockingMove) {
                return true;
            }
            return false;
        }
        
        public IEnumerable<Point> GetTileLocationsAlongLine(int xOrigin, int yOrigin, int xDestination, int yDestination) {
            
            xOrigin = ClampX(xOrigin);
            yOrigin = ClampY(yOrigin);
            xDestination = ClampX(xDestination);
            yDestination = ClampY(yDestination);

            int dx = Math.Abs(xDestination - xOrigin);
            int dy = Math.Abs(yDestination - yOrigin);

            int sx = xOrigin < xDestination ? 1 : -1;
            int sy = yOrigin < yDestination ? 1 : -1;
            int err = dx - dy;

            while (true) {

                yield return new Point(xOrigin, yOrigin);
                if (xOrigin == xDestination && yOrigin == yDestination) {
                    break;
                }
                int e2 = 2 * err;
                if (e2 > -dy) {
                    err = err - dy;
                    xOrigin = xOrigin + sx;
                }
                if (e2 < dx) {
                    err = err + dx;
                    yOrigin = yOrigin + sy;
                }
            }
        }

        
        private int ClampX(int x) {
            if (x < 0)
                x = 0;
            else if (x > _map.Width - 1)
                x = _map.Width - 1;
            return x;
        }
        
        private int ClampY(int y) {
            if (y < 0)
                y = 0;
            else if (y > _map.Height - 1)
                y = _map.Height - 1;
            return y;
        }
    }
}
