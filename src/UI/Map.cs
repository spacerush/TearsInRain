using System;
using System.Linq;
using Microsoft.Xna.Framework;
using TearsInRain.Tiles;
using TearsInRain.Entities;
using SadConsole;
using System.Collections.Generic;

namespace TearsInRain.UI { 
    public class Map {
        //public TileBase[] _tiles;

        public Dictionary<Point, TileBase> _TileDict = new Dictionary<Point, TileBase>();

        public int _width;
        public int _height;

       // public TileBase[] Tiles { get { return _tiles; } set { _tiles = value; } }
        public int Width { get { return _width; } set { _width = value; } }
        public int Height { get { return _height; } set { _height = value; } }

        // public TIRSpatial Entities;
        public GoRogue.MultiSpatialMap<TIREntity> Entities;

        public static GoRogue.IDGenerator IDGenerator = new GoRogue.IDGenerator();

        public Map (int width, int height) {
            _width = width;
            _height = height;
           // _Tile = new TileBase[width * height];
            //Entities = new TIRSpatial(width, height);
            Entities = new GoRogue.MultiSpatialMap<TIREntity>();
        }

        public Map() {

        }

        //public void postCreateInit(int width, int height, TIRSpatial entities, TileBase[] tiles) {
        //    _width = width;
        //    _height = height;
        //    Tiles = new TileBase[width * height];
        //    Entities = entities;
        //    Tiles = tiles;
        //}

        public bool IsTileWalkable(Point location) {
            if (!_TileDict.ContainsKey(location))
                return false;

            return !_TileDict[location].IsBlockingMove;

        }

        public T GetEntityAt<T>(Point location) where T : TIREntity {
            return Entities.GetItems(location).OfType<T>().FirstOrDefault();
        }

        public void Remove(TIREntity entity) {
            Entities.Remove(entity);
            entity.Moved -= OnEntityMoved;
        }

        public void Add(TIREntity entity) {
            Entities.Add(entity, entity.Position);
            entity.Moved += OnEntityMoved;
        }

        private void OnEntityMoved(object sender, TIREntity.EntityMovedEventArgs args) {
            Entities.Move(args.Entity as TIREntity, args.Entity.Position);
        }

        //public T GetTileAt<T>(int x, int y) where T : TileBase {
        //    int locationIndex = Helpers.GetIndexFromPoint(x, y, Width);

        //    if (locationIndex <= Width * Height && locationIndex >= 0) {
        //        if (Tiles[locationIndex] is T)
        //            return (T)Tiles[locationIndex];
        //        else return null;
        //    } else
        //        return null;
        //}



        // Change to using a Dictionary?
        public T GetTileAt<T>(Point location) where T : TileBase {
            //int locationIndex = Helpers.GetIndexFromPoint(x, y, Width); 
            if (_TileDict.ContainsKey(location)) {
                if (_TileDict[location] is T) {
                    return (T)_TileDict[location];
                } else return null;
            } else return null;

        }
    }
}