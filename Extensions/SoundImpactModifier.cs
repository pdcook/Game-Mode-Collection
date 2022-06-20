using Sonigon;
using Sonigon.Internal;
using SoundImplementation;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
namespace GameModeCollection.Extensions
{
    /// extensions to allow SoundImpactModifiers to specify SoundParameterBase modifiers
    public class SoundImpactModifierAdditionalData
    {
        public SoundParameterBase[] soundParameterArray = null;
    }
    public static class SoundImpactModifierExtensions
    {
        private static readonly ConditionalWeakTable<SoundImpactModifier, SoundImpactModifierAdditionalData> additionalData = new ConditionalWeakTable<SoundImpactModifier, SoundImpactModifierAdditionalData>();
        public static SoundImpactModifierAdditionalData GetData(this SoundImpactModifier instance)
        {
            return additionalData.GetOrCreateValue(instance);
        }
        public static void AddSoundParameter(this SoundImpactModifier instance, SoundParameterBase soundParameter)
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
        public static void SetSoundParameterArray(this SoundImpactModifier instance, SoundParameterBase[] soundParameterArray)
        {
            instance.GetData().soundParameterArray = soundParameterArray;
        }
        public static SoundParameterBase[] GetSoundParameterArray(this SoundImpactModifier instance)
        {
            return instance.GetData().soundParameterArray ?? new SoundParameterBase[0];
        }
    }
}
