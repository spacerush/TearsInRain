using System;
using Microsoft.Xna.Framework;
using TearsInRain.Entities;
using System.Text;
using GoRogue.DiceNotation;

namespace TearsInRain.Commands {
    public class CommandManager {
        public CommandManager() { }


        public bool MoveActorBy(Actor actor, Point position) {
            return actor.MoveBy(position);
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

        public void Pickup(Actor actor, Item item) {
            actor.Inventory.Add(item);
            GameLoop.UIManager.MessageLog.Add($"{actor.Name} picked up {item.Name}.");
            item.Destroy();
        }
    }
}