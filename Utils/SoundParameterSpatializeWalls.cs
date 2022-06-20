using Sonigon;
using Sonigon.Internal;
using GameModeCollection.Patches;
namespace GameModeCollection.Utils
{
    public class SoundParameterSpatializeWalls : SoundParameterBase
    {
        public SoundParameterSpatializeWalls(float wallPenalty = GMCAudio.WallPenaltyPercent, int maxWallsCutoff = GMCAudio.MaxWallsCutoff, UpdateMode updateMode = UpdateMode.Once)
        {
            this.wallPenalty = wallPenalty;
            this.maxWallsCutoff = maxWallsCutoff;
            this.root.updateMode = updateMode;
            this.root.type = SoundParameterTypeExt.GetSoundParameterType("SpatializeWalls");
        }

        public float wallPenalty
        {
            get
            {
                return this.root.valueFloat;
            }
            set
            {
                this.root.valueFloat = value;
            }
        }
        public int maxWallsCutoff
        {
            get
            {
                return this.root.valueInt;
            }
            set
            {
                this.root.valueInt = value;
            }
        }
    }
}
