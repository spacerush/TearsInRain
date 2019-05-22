using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TearsInRain.Entities {
    
    public abstract class Actor : TIREntity {
        public int _health; // current health
        public int _maxHealth; // max possible health

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int AttackChance { get; set; }
        public int Defense { get; set; }
        public int DefenseChance { get; set; }
        public int Gold { get; set; }
        public List<Item> Inventory = new List<Item>();

        protected Actor(Color foreground, Color background, int glyph, int width = 1, int height = 1) : base(foreground, background, width, height, glyph) {
            Animation.CurrentFrame[0].Foreground = foreground;
            Animation.CurrentFrame[0].Background = background;
            Animation.CurrentFrame[0].Glyph = glyph;
        }

        public bool MoveBy(Point positionChange) {
            if (GameLoop.World.CurrentMap.IsTileWalkable(Position + positionChange)) {
                Monster monster = GameLoop.World.CurrentMap.GetEntityAt<Monster>(Position + positionChange);
                Item item = GameLoop.World.CurrentMap.GetEntityAt<Item>(Position + positionChange);

                if (monster != null) {
                    GameLoop.CommandManager.Attack(this, monster);
                    return true;
                } else if (item != null) {
                    GameLoop.CommandManager.Pickup(this, item);
                    return true;
                }


                Position += positionChange;
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