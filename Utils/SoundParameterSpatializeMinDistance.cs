using Sonigon;
using Sonigon.Internal;
using GameModeCollection.Patches;
namespace GameModeCollection.Utils
{
    public class SoundParameterSpatializeMinDistance : SoundParameterBase
    {
        public SoundParameterSpatializeMinDistance(float minDistance = GMCAudio.MinDistance, UpdateMode updateMode = UpdateMode.Once)
        {
            this.minDistance = minDistance;
            this.root.updateMode = updateMode;
            this.root.type = SoundParameterTypeExt.GetSoundParameterType("SpatializeMinDistance");
        }

        public float minDistance
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
