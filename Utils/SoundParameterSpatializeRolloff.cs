using Sonigon;
using Sonigon.Internal;
using GameModeCollection.Patches;
using UnityEngine;
namespace GameModeCollection.Utils
{
    public class SoundParameterSpatializeRolloff : SoundParameterBase
    {
        public SoundParameterSpatializeRolloff(AudioRolloffMode rolloff = GMCAudio.Rolloff, UpdateMode updateMode = UpdateMode.Once)
        {
            this.rolloff = rolloff;
            this.root.updateMode = updateMode;
            this.root.type = SoundParameterTypeExt.GetSoundParameterType("SpatializeRolloff");
        }

        public AudioRolloffMode rolloff
        {
            get
            {
                return (AudioRolloffMode)this.root.valueInt;
            }
            set
            {
                this.root.valueInt = (int)value;
            }
        }
    }
}
