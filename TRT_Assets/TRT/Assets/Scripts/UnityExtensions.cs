using UnityEngine;
namespace Assets
{
    public static class GameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T: MonoBehaviour
        {
            T comp = gameObject.GetComponent<T>();
            if (comp is null)
            {
                comp = gameObject.AddComponent<T>();
            }
            return comp;

        }
    }
    public static class ComponentExtensions
    {
        public static T GetOrAddComponent<T>(this Component component) where T: MonoBehaviour
        {
            T comp = component.GetComponent<T>();
            if (comp is null)
            {
                comp = component.gameObject.AddComponent<T>();
            }
            return comp;

        }
    }
    public static class TransformExtensions
    {
        public static T GetOrAddComponent<T>(this Transform transform) where T: MonoBehaviour
        {
            T comp = transform.GetComponent<T>();
            if (comp is null)
            {
                comp = transform.gameObject.AddComponent<T>();
            }
            return comp;
        }
        public static Transform GetOrCreateChild(this Transform transform, string childName)
        {
            Transform child = transform.Find(childName);
            if (child is null)
            {
                child = new GameObject(childName).transform;
                child.SetParent(transform);
            }
            return child;
        }
    }
}
