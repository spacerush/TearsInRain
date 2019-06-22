using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using TearsInRain.Serializers;

namespace TearsInRain.src {

    [JsonConverter(typeof(RaceJsonConverter))]
    public class CharacterRace {
        public string RaceName;
        public int[] AbilityScoreMods = { 0, 0, 0, 0, 0, 0 };
        public Color SkinColor;

        public CharacterRace(string name, int[] abilityMods, Color skin) {
            RaceName = name;
            AbilityScoreMods = abilityMods;
        }
    }
}
