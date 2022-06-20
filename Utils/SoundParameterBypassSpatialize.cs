using Sonigon;
using Sonigon.Internal;
using GameModeCollection.Patches;
namespace GameModeCollection.Utils
{
    public class SoundParameterBypassSpatialize : SoundParameterBase
    {
        public SoundParameterBypassSpatialize(bool bypassSpatialize = true, UpdateMode updateMode = UpdateMode.Once)
        {
            this.bypassSpatialize = bypassSpatialize;
            this.root.updateMode = updateMode;
            this.root.type = SoundParameterTypeExt.GetSoundParameterType("BypassSpatialize");
        }

        public bool bypassSpatialize
        {
            get
            {
                return this.root.valueBool;
            }
            set
            {
                this.root.valueBool = value;
            }
        }
    }
}
