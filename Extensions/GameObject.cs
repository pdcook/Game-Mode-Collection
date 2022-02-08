using UnityEngine;
namespace GameModeCollection.Extensions
{
    static class GameObjectExtensions
    {
        public static void PauseAllAnimators(this GameObject obj)
        {
            foreach (Animator anim in obj.GetComponentsInChildren<Animator>())
            {
                anim.enabled = false;
            }
        }
        public static void PlayAllAnimators(this GameObject obj)
        {
            foreach (Animator anim in obj.GetComponentsInChildren<Animator>())
            {
                anim.enabled = true;
            }
        }
    }
}
