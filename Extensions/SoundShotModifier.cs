using Sonigon;
using Sonigon.Internal;
using SoundImplementation;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
namespace GameModeCollection.Extensions
{
    /// extensions to allow SoundShotModifiers to specify SoundParameterBase modifiers
    public class SoundShotModifierAdditionalData
    {
        public SoundParameterBase[] soundParameterArray = null;
    }
    public static class SoundShotModifierExtensions
    {
        private static readonly ConditionalWeakTable<SoundShotModifier, SoundShotModifierAdditionalData> additionalData = new ConditionalWeakTable<SoundShotModifier, SoundShotModifierAdditionalData>();
        public static SoundShotModifierAdditionalData GetData(this SoundShotModifier instance)
        {
            return additionalData.GetOrCreateValue(instance);
        }
        public static void AddSoundParameter(this SoundShotModifier instance, SoundParameterBase soundParameter)
        {
            if (instance.GetData().soundParameterArray is null)
            {
                instance.GetData().soundParameterArray = new SoundParameterBase[] { soundParameter };
            }
            else
            {
                instance.GetData().soundParameterArray = instance.GetData().soundParameterArray.Concat(new SoundParameterBase[] { soundParameter }).ToArray();
            }
        }
        public static void SetSoundParameterArray(this SoundShotModifier instance, SoundParameterBase[] soundParameterArray)
        {
            instance.GetData().soundParameterArray = soundParameterArray;
        }
        public static SoundParameterBase[] GetSoundParameterArray(this SoundShotModifier instance)
        {
            return instance.GetData().soundParameterArray ?? new SoundParameterBase[0];
        }
    }
}
