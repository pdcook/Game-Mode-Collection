using System.Linq;
using UnboundLib;
using GameModeCollection.Extensions;

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
        public static int GetRandomItemID(this CharacterCreatorItemLoader instance, CharacterItemType type, string[] bannedItemNames = null)
        {
            CharacterItem[] items;

            switch (type)
            {
                case CharacterItemType.Eyes:
                    items = instance.eyes;
                    break;
                case CharacterItemType.Mouth:
                    items = instance.mouths;
                    break;
                case CharacterItemType.Detail:
                    items = instance.accessories;
                    break;
                default:
                    return -1;
            }

            if (bannedItemNames is null)
            {
                return UnityEngine.Random.Range(0, items.Count());
            }
            else
            {
                CharacterItem[] validItems = items.Where(i => !bannedItemNames.Contains(i.name)).ToArray();
                if (validItems.Count() == 0) { return UnityEngine.Random.Range(0, items.Count()); }
                return (int)instance.InvokeMethod("GetItemID", validItems.GetRandom<CharacterItem>(), type);
            }
        }
    }
}
