using System.Runtime.CompilerServices;
using System.Linq;

namespace GameModeCollection.Extensions
{
    public class CharacterDataAdditionalData
    {
        public int maxAllowedCards = int.MaxValue;
    }
    public static class CharacterDataExtensions
    {
        private static readonly ConditionalWeakTable<CharacterData, CharacterDataAdditionalData> additionalData = new ConditionalWeakTable<CharacterData, CharacterDataAdditionalData>();
        public static CharacterDataAdditionalData GetData(this CharacterData instance)
        {
            return additionalData.GetOrCreateValue(instance);
        }
        public static bool CanHaveMoreCards(this CharacterData instance)
        {
            return instance.currentCards.Count() < instance.GetData().maxAllowedCards;
        }
    }
}
