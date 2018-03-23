using OpenTK;
using osu.Game.Rulesets.UI;
using Symcol.Rulesets.Core.Multiplayer.Networking;

namespace Symcol.Rulesets.Core
{
    public class SymcolPlayfield : Playfield
    {
        public static RulesetNetworkingClientHandler RulesetNetworkingClientHandler;

        public SymcolPlayfield(float sizeX) : base(sizeX)
        {
        }
    }
}
