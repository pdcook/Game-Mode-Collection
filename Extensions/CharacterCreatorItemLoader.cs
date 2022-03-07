using System.Linq;

namespace GameModeCollection.Extensions
{
    static class CharacterCreatorItemLoaderExtensions
    {
        public static int GetItemIDByName(this CharacterCreatorItemLoader instance, string name, CharacterItemType type)
        {
            CharacterItem item;
            switch (type)
            {
                case CharacterItemType.Eyes:
                    item = instance.eyes.FirstOrDefault(e => e.name.Equals(name));
                    if (item is null) { return -1; }
                    else { return System.Array.IndexOf(instance.eyes, item); }
                case CharacterItemType.Mouth:
                    item = instance.mouths.FirstOrDefault(m => m.name.Equals(name));
                    if (item is null) { return -1; }
                    else { return System.Array.IndexOf(instance.mouths, item); }
                case CharacterItemType.Detail:
                    item = instance.accessories.FirstOrDefault(a => a.name.Equals(name));
                    if (item is null) { return -1; }
                    else { return System.Array.IndexOf(instance.accessories, item); }
                default:
                    return -1;
            }
        }
    }
}
