using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TearsInRain.Tiles;

namespace TearsInRain.Entities {
    
    public abstract class Actor : Entity {
        public int _health; // current health
        public int _maxHealth; // max possible health

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int AttackChance { get; set; }
        public int Defense { get; set; }
        public int DefenseChance { get; set; }
        public int Gold { get; set; }
        public int MoveCost { get; set; }
        public UInt64 TimeLastActed { get; set; }
        public List<Item> Inventory = new List<Item>();

        protected Actor(Color foreground, Color background, int glyph, int width = 1, int height = 1) : base(foreground, background, width, height, glyph) {
            Animation.CurrentFrame[0].Foreground = foreground;
            Animation.CurrentFrame[0].Background = background;
            Animation.CurrentFrame[0].Glyph = glyph;
        }

        public bool MoveBy(Point positionChange) { 
            TileBase tile = GameLoop.World.CurrentMap.GetTileAt<TileDoor>(Position.X + positionChange.X, Position.Y + positionChange.Y);

            if (GameLoop.World.CurrentMap.IsTileWalkable(Position + positionChange) || tile is TileDoor) {
                Monster monster = GameLoop.World.CurrentMap.GetEntityAt<Monster>(Position + positionChange);
                Item item = GameLoop.World.CurrentMap.GetEntityAt<Item>(Position + positionChange);

                if (monster != null) {
                    GameLoop.CommandManager.Attack(this, monster);
                    return true;
                } else if (tile is TileDoor door && !door.IsOpen) {
                    GameLoop.CommandManager.OpenDoor(this, door, Position + positionChange);
                    return true;
                }


                Position += positionChange;

                string msg = "move_p" + "|" + GameLoop.NetworkingManager.myUID + "|" + Position.X + "|" + Position.Y;
                GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(msg));
                return true;
            } else { 
                return false;
            }
        }

        public bool MoveTo(Point newPosition) {
            Position = newPosition;
            return true;
        }
    }
}