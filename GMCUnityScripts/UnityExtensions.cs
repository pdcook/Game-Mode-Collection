using UnityEngine;
namespace GMCUnityScripts
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
        public static void DestroyAllChildren(this GameObject gameObject)
        {
            if (gameObject?.transform != null)
            {
                if (gameObject is null)
                {
                    UnityEngine.Debug.Log("GameObject is null");
                }
                if (gameObject.transform is null)
                {
                    UnityEngine.Debug.Log("GameObject.transform is null");
                }
                gameObject.transform.DestroyAllChildren();
            }
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
        public static void DestroyAllChildren(this Transform transform)
        {
            Transform[] children = transform?.GetComponentsInChildren<Transform>(true);
            if (children is null) { return; }
            foreach (Transform child in children)
            {
                if (child?.gameObject != null && child != transform) { GameObject.Destroy(child.gameObject); }
            }
        }
    }
}
