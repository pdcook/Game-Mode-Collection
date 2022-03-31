using CardChoiceSpawnUniqueCardPatch.CustomCategories;
namespace GameModeCollection.GameModes.Murder.Cards
{
    public static class MurderCardCategories
    {
        public readonly static CardCategory Murder_Clue = CustomCardCategories.instance.CardCategory("Murder_Clue");
        public readonly static CardCategory Murder_Murderer = CustomCardCategories.instance.CardCategory("Murder_Murderer");
        public readonly static CardCategory Murder_Detective = CustomCardCategories.instance.CardCategory("Murder_Detective");
        public readonly static CardCategory Murder_DoNotDropOnDeath = CustomCardCategories.instance.CardCategory("Murder_DoNotDropOnDeath");
    }
}
