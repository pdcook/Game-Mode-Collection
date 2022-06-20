using Sonigon;
using Sonigon.Internal;
using GameModeCollection.Patches;
namespace GameModeCollection.Utils
{
    public class SoundParameterSpatializeMaxDistance : SoundParameterBase
    {
        public SoundParameterSpatializeMaxDistance(float maxDistance = GMCAudio.MaxDistance, UpdateMode updateMode = UpdateMode.Once)
        {
            this.maxDistance = maxDistance;
            this.root.updateMode = updateMode;
            this.root.type = SoundParameterTypeExt.GetSoundParameterType("SpatializeMaxDistance");
        }

        public float maxDistance
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
