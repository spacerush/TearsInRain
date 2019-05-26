using System;
using Microsoft.Xna.Framework;
using TearsInRain.Entities;
using System.Text;
using GoRogue.DiceNotation;
using TearsInRain.Tiles;

namespace TearsInRain.Commands {
    public class CommandManager {
        public CommandManager() { }


        public bool MoveActorBy(Actor actor, Point position) {
            if (actor.MoveBy(position)) {
                actor.TimeLastActed = GameLoop.GameTime;
                return true;
            }

            return false;
        }

        public void Attack(Actor attacker, Actor defender) {
            StringBuilder attackMessage = new StringBuilder();
            StringBuilder defendMessage = new StringBuilder();

            int hits = ResolveAttack(attacker, defender, attackMessage);
            int blocks = ResolveDefense(defender, hits, attackMessage, defendMessage);

            GameLoop.UIManager.MessageLog.Add(attackMessage.ToString());

            if (!String.IsNullOrWhiteSpace(defendMessage.ToString())) {
                GameLoop.UIManager.MessageLog.Add(defendMessage.ToString());
            }

            int damage = hits - blocks;

            ResolveDamage(defender, damage);
        }

        private static int ResolveAttack(Actor attacker, Actor defender, StringBuilder attackMessage) {
            int hits = 0;
            attackMessage.AppendFormat("{0} attacks {1}, ", attacker.Name, defender.Name);

            for (int dice = 0; dice < attacker.Attack; dice++) {
                int diceOutcome = Dice.Roll("1d100");

                if (diceOutcome >= 100 - attacker.AttackChance)
                    hits++;
            }

            return hits;
        }


        private static int ResolveDefense(Actor defender, int hits, StringBuilder attackMessage, StringBuilder defendMessage) {
            int blocks = 0;
            if (hits > 0) {
                attackMessage.AppendFormat("scoring {0} hits.", hits);
                defendMessage.AppendFormat("{0} defends and rolls: ", defender.Name);

                for (int dice = 0; dice < defender.Defense; dice++) {
                    int diceOutcome = Dice.Roll("1d100");

                    if (diceOutcome >= 100 - defender.DefenseChance)
                        blocks++;
                }
            } else {
                attackMessage.Append("and misses completely!");
            }

            return blocks;
        }


        private static void ResolveDamage(Actor defender, int damage) {
            if (damage > 0) {
                defender.Health -= damage;
                GameLoop.UIManager.MessageLog.Add($"{defender.Name} was hit for {damage} damage.");
                
                if (defender.Health <= 0) {
                    ResolveDeath(defender);
                }
            } else {
                GameLoop.UIManager.MessageLog.Add($"{defender.Name} blocked all damage!");
            }
        }
        
        private static void ResolveDeath(Actor defender) {
            StringBuilder deathMessage = new StringBuilder($"{defender.Name} died");

            if (defender.Inventory.Count > 0) {
                deathMessage.Append(" and dropped");

                foreach (Item item in defender.Inventory) {
                    item.Position = defender.Position;
                    GameLoop.World.CurrentMap.Add(item);
                    deathMessage.Append(", " + item.Name);
                }

                defender.Inventory.Clear();
            } else {
                deathMessage.Append(".");
            }

            GameLoop.World.CurrentMap.Remove(defender);
            GameLoop.UIManager.MessageLog.Add(deathMessage.ToString());
        }

        public void Pickup(Actor actor, Point pos) {
            Item item = GameLoop.World.CurrentMap.GetEntityAt<Item>(pos);

            if (item != null) {
                actor.Inventory.Add(item);
                GameLoop.UIManager.MessageLog.Add($"{actor.Name} picked up {item.Name}.");
                item.Destroy();
            } else {
                GameLoop.UIManager.MessageLog.Add("Nothing to pick up there!");
            }
        }


        public void OpenDoor(Actor actor, TileDoor door, Point pos) {
            if (!door.Locked) {
                door.Open();

                GameLoop.UIManager.MapConsole.IsDirty = true;
                GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes("t_data|door|" + pos.X + "|" + pos.Y + "|open|unlock"));
            } else {
                GameLoop.UIManager.MessageLog.Add("The door is locked.");
            }
        }

        public void CloseDoor(Actor actor, Point pos) {
            TileBase tile = GameLoop.World.CurrentMap.GetTileAt<TileBase>(pos.X, pos.Y);
            Entity entity = GameLoop.World.CurrentMap.GetEntityAt<Entity>(pos);

            if (entity == null) {
                if (tile is TileDoor door) {
                    if (door.IsOpen) {
                        door.Close();

                        GameLoop.UIManager.MapConsole.IsDirty = true;

                        var data = "t_data|door|" + pos.X + "|" + pos.Y + "|close|";

                        if (door.Locked) { data += "lock"; } else if (!door.Locked) { data += "unlock"; }

                        GameLoop.NetworkingManager.SendNetMessage(0, System.Text.Encoding.UTF8.GetBytes(data));
                    } else {
                        GameLoop.UIManager.MessageLog.Add("The door is already closed.");
                    }
                } else {
                    GameLoop.UIManager.MessageLog.Add("There's nothing to close there!");
                }
            } else if (entity is Player) {
                GameLoop.UIManager.MessageLog.Add("You try to close the door, but some idiot is standing in the way!");
            } else if (entity is Monster) {
                GameLoop.UIManager.MessageLog.Add("Should have shut it before the monster walked through!");
            } else if (entity is Item) {
                if (entity.Name[0] == 'a' || entity.Name[0] == 'e' || entity.Name[0] == 'i' || entity.Name[0] == 'o' || entity.Name[0] == 'u')
                    GameLoop.UIManager.MessageLog.Add("An " + entity.Name + " is in the way!");
                else
                    GameLoop.UIManager.MessageLog.Add("A " + entity.Name + " is in the way!");
            }
        }
    }
}