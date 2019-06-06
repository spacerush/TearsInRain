using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using TearsInRain.Serializers;

namespace TearsInRain.Tiles {

    [JsonConverter(typeof(TileJsonConverter))]
    public class TileFarmland : TileBase {
        public TileFarmland(bool blocksMovement = false, bool blocksLOS = false, string seed = "empty") : base(Color.SandyBrown, Color.Brown, '#', blocksMovement, blocksLOS) {
            
        }
    }
}