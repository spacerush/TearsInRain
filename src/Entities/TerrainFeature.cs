using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SadConsole.Components;
using TearsInRain.Serializers;
using TearsInRain.Tiles;

namespace TearsInRain.Entities {

    [JsonConverter(typeof(TerrainJsonConverter))]
    public class TerrainFeature : Entity {
        public string madeBy = "";
        public double Weight;
        public int Condition;
        public bool IsBlockingLOS;
        public bool IsBlockingMove;
        public Color DecoColor;
        public int DecoGlyph;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();

        public TerrainFeature(Color foreground, Color background, string name, char glyph, bool blocksLOS = false, bool blocksMove = false, double weight = 1, int condition = 100, int width = 1, int height = 1, Color decoColor = new Color(), char decoGlyph = ' ', Dictionary<string, string> properties = null) : base(foreground, background, glyph) {
            Animation.CurrentFrame[0].Foreground = foreground;
            Animation.CurrentFrame[0].Background = background;
            Animation.CurrentFrame[0].Glyph = glyph;
            Weight = weight;

            IsBlockingLOS = blocksLOS;
            IsBlockingMove = blocksMove;

            Condition = condition;
            Name = name;
            Properties = properties;

            if (decoGlyph != ' ') {
                Animation.AddDecorator(Position.X, Position.Y, 1, new SadConsole.CellDecorator(decoColor, decoGlyph, Microsoft.Xna.Framework.Graphics.SpriteEffects.None));

                DecoColor = decoColor;
                DecoGlyph = decoGlyph;

                Animation.IsDirty = true;
                
            }

            Components.Add(new EntityViewSyncComponent());
        }

        public TerrainFeature Clone() {
            Color fore = Animation.CurrentFrame[0].Foreground;
            Color back = Animation.CurrentFrame[0].Background;
            char glyph = (char) Animation.CurrentFrame[0].Glyph;

            Dictionary<string, string> newProps = new Dictionary<string, string>();

            if (Properties != null) {
                foreach (KeyValuePair<string, string> prop in Properties) {
                    newProps.Add(prop.Key, prop.Value);
                }
            }


            TerrainFeature newTF = new TerrainFeature(fore, back, Name, glyph, IsBlockingLOS, IsBlockingMove, Weight, Condition, properties:newProps);
            newTF.DecoColor = DecoColor;
            newTF.DecoGlyph = DecoGlyph;
            newTF.Animation.AddDecorator(Position.X, Position.Y, 1, new SadConsole.CellDecorator(newTF.DecoColor, newTF.DecoGlyph, Microsoft.Xna.Framework.Graphics.SpriteEffects.None));

            return newTF;
        }

        public void Destroy() {
            GameLoop.World.CurrentMap.Remove(this);
        }
        
    }
}