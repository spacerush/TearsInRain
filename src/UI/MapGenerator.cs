﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using TearsInRain.Tiles;

namespace TearsInRain.UI {
    class MapGenerator {
        public MapGenerator() { }
        Random randNum = new Random();

        Map _map;

        public Map GenerateMap(int mapWidth, int mapHeight, int maxRooms, int minRoomSize, int maxRoomSize) {
            _map = new Map(mapWidth, mapHeight);
            List<Rectangle> Rooms = new List<Rectangle>();

            for (int i = 0; i < maxRooms; i++) {
                int newRoomWidth = randNum.Next(minRoomSize, maxRoomSize);
                int newRoomHeight = randNum.Next(minRoomSize, maxRoomSize);

                int newRoomX = randNum.Next(0, mapWidth - newRoomWidth - 1);
                int newRoomY = randNum.Next(0, mapHeight - newRoomHeight - 1);

                Rectangle newRoom = new Rectangle(newRoomX, newRoomY, newRoomWidth, newRoomHeight);

                bool newRoomIntersects = Rooms.Any(room => newRoom.Intersects(room));

                if (!newRoomIntersects)
                    Rooms.Add(newRoom);
            }

            FloodWalls();

            foreach (Rectangle room in Rooms)
                CreateRoom(room);

            for (int r = 1; r < Rooms.Count; r++) {
                Point previousRoomCenter = Rooms[r - 1].Center;
                Point currentRoomCenter = Rooms[r].Center;

                if (randNum.Next(1, 2) == 1) {
                    CreateHorizontalTunnel(previousRoomCenter.X, currentRoomCenter.X, previousRoomCenter.Y);
                    CreateVerticalTunnel(previousRoomCenter.Y, currentRoomCenter.Y, currentRoomCenter.X); 
                } else {
                    CreateVerticalTunnel(previousRoomCenter.Y, currentRoomCenter.Y, previousRoomCenter.X);
                    CreateHorizontalTunnel(previousRoomCenter.X, currentRoomCenter.X, currentRoomCenter.Y);
                }
            }

            foreach (Rectangle room in Rooms) {
                CreateDoor(room);
            }

            return _map;
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

        private void CreateRoom(Rectangle room) {
            for (int x = room.Left + 1; x < room.Right - 1; x++) {
                for (int y = room.Top + 1; y < room.Bottom - 1; y++) {
                    CreateFloor(new Point(x, y));
                }
            }

            List<Point> perimeter = GetBorderCellLocations(room);
            foreach (Point location in perimeter)
                CreateWall(location);
        }


        private void CreateFloor(Point location) {
            _map._TileDict[location] = new TileFloor();
        }

        private void CreateWall(Point location) {
            _map._TileDict[location] = new TileWall();
        }

        private void FloodWalls() {
            for(int x = 0; x < _map.Width; x++) {
                for (int y = 0; y < _map.Height; y++) {
                    Point location = new Point(x, y);
                    if (!_map._TileDict.ContainsKey(location)) {
                        _map._TileDict.Add(location, new TileWall());
                    }
                }
            }
                _map._TileDict[new Point()] = new TileWall();
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
                if (xOrigin == xDestination && yOrigin == yDestination)
                    break;

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

        private void CreateDoor(Rectangle room) {
            List<Point> borderCells = GetBorderCellLocations(room);

            foreach (Point location in borderCells) {
                int locationIndex = location.ToIndex(_map.Width);
                if (IsPotentialDoor(location)) {
                    TileDoor newDoor = new TileDoor(false, false);
                    _map._TileDict[location] = newDoor;
                }
            }
        }

        private bool IsPotentialDoor(Point location) {
            int locationIndex = location.ToIndex(_map.Width);
            //if (_map.Tiles[locationIndex] != null && _map.Tiles[locationIndex] is TileWall)
            //    return false;

            if (_map._TileDict[location] != null && _map._TileDict[location] is TileWall)
                return false;

            Point right = new Point(location.X + 1, location.Y);
            Point left = new Point(location.X - 1, location.Y);
            Point top = new Point(location.X, location.Y - 1);
            Point bottom = new Point(location.X, location.Y + 1);

            //if (_map.GetTileAt<TileDoor>(location.X, location.Y) != null ||  _map.GetTileAt<TileDoor>(right.X, right.Y) != null ||  _map.GetTileAt<TileDoor>(left.X, left.Y) != null || _map.GetTileAt<TileDoor>(top.X, top.Y) != null || _map.GetTileAt<TileDoor>(bottom.X, bottom.Y) != null ) {
            //    return false;
            //}

            //if ((!_map.Tiles[right.ToIndex(_map.Width)].IsBlockingMove && !_map.Tiles[left.ToIndex(_map.Width)].IsBlockingMove) && _map.Tiles[top.ToIndex(_map.Width)].IsBlockingMove && _map.Tiles[bottom.ToIndex(_map.Width)].IsBlockingMove) {
            //    return true;
            //}

            //if (_map.Tiles[right.ToIndex(_map.Width)].IsBlockingMove && _map.Tiles[left.ToIndex(_map.Width)].IsBlockingMove && !_map.Tiles[top.ToIndex(_map.Width)].IsBlockingMove && !_map.Tiles[bottom.ToIndex(_map.Width)].IsBlockingMove) {
            //    return true;
            //}

            if (_map.GetTileAt<TileDoor>(location) != null || _map.GetTileAt<TileDoor>(right) != null || _map.GetTileAt<TileDoor>(left) != null || _map.GetTileAt<TileDoor>(top) != null || _map.GetTileAt<TileDoor>(bottom) != null) {
                return false;
            }

            if (_map._TileDict.ContainsKey(left) && _map._TileDict.ContainsKey(right) && _map._TileDict.ContainsKey(top) && _map._TileDict.ContainsKey(bottom)) {
                if ((!_map._TileDict[right].IsBlockingMove && !_map._TileDict[left].IsBlockingMove) &&  _map._TileDict[top].IsBlockingMove && _map._TileDict[bottom].IsBlockingMove) {
                    return true;
                }
            }

            if (_map._TileDict.ContainsKey(left) && _map._TileDict.ContainsKey(right) && _map._TileDict.ContainsKey(top) && _map._TileDict.ContainsKey(bottom)) {
                if (_map._TileDict[right].IsBlockingMove && _map._TileDict[left].IsBlockingMove && !_map._TileDict[top].IsBlockingMove && !_map._TileDict[bottom].IsBlockingMove) {
                    return true;
                }
            }

            return false;
        }
    }
}
