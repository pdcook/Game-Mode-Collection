using SoundImplementation;
using System.Collections.Generic;
using UnboundLib;
namespace GameModeCollection.Extensions
{
    static class SoundGunExtensions
    {
        public static void RemoveSoundShotModifier(this SoundGun instance, SoundShotModifier soundShotModifier)
        {
            if (soundShotModifier != null)
            {
                List<SoundShotModifier> soundShotModifierAllList = (List<SoundShotModifier>)instance.GetFieldValue("soundShotModifierAllList");
                soundShotModifierAllList.Remove(soundShotModifier);
                instance.SetFieldValue("soundShotModifierAllList", soundShotModifierAllList);
                instance.RefreshSoundModifiers();
            }
        }
        public static void RemoveSoundImpactModifier(this SoundGun instance, SoundImpactModifier soundImpactModifier)
        {
            if (soundImpactModifier != null)
            {
                List<SoundImpactModifier> soundImpactModifierAllList = (List<SoundImpactModifier>)instance.GetFieldValue("soundImpactModifierAllList");
                soundImpactModifierAllList.Remove(soundImpactModifier);
                instance.SetFieldValue("soundImpactModifierAllList", soundImpactModifierAllList);
                instance.RefreshSoundModifiers();
            }
        }
    }
}
