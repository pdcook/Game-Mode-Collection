using System.Runtime.CompilerServices;

namespace GameModeCollection.Extensions
{
    public class GunAdditionalData
    {
        public bool disabled = false;
        public bool disabledFromCardBar = false;
        public bool silenced = false; // when a gun is silenced, it cannot be heard through walls
        public bool pierce = false;
    }
    public static class GunExtensions
    {
        private static readonly ConditionalWeakTable<Gun, GunAdditionalData> additionalData = new ConditionalWeakTable<Gun, GunAdditionalData>();
        public static GunAdditionalData GetData(this Gun instance)
        {
            return additionalData.GetOrCreateValue(instance);
        }

        public static void DisableGun(this Gun instance)
        {
            instance.GetData().disabled = true;
        }
        public static void EnableGun(this Gun instance)
        {
            instance.GetData().disabled = false;
        }
    }
}
