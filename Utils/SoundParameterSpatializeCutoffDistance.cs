using Sonigon;
using Sonigon.Internal;
using GameModeCollection.Patches;
namespace GameModeCollection.Utils
{
    public class SoundParameterSpatializeCutoffDistance : SoundParameterBase
    {
        public SoundParameterSpatializeCutoffDistance(float cutoffDistance = GMCAudio.CutoffDistance, UpdateMode updateMode = UpdateMode.Once)
        {
            this.cutoffDistance = cutoffDistance;
            this.root.updateMode = updateMode;
            this.root.type = SoundParameterTypeExt.GetSoundParameterType("SpatializeCutoffDistance");
        }

        public float cutoffDistance
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
    }
}
