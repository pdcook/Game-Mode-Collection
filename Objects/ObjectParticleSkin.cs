using UnityEngine;
using UnboundLib;
using System.Reflection;
namespace GameModeCollection.Objects
{
    static class ObjectParticleSkin
    {
        public static int Layer => LayerMask.NameToLayer("Player");
        public static int SortingLayerID => SortingLayer.NameToID("Player0");
        public static void AddObjectParticleSkin(Transform parent, Sprite sprite, PlayerSkin skinColors)
        {
            GameObject particleSkinSprite = GameObject.Instantiate(PlayerAssigner.instance.playerPrefab.transform.GetChild(0).GetChild(0).gameObject, parent);
            particleSkinSprite.name = "Sprite";
            particleSkinSprite.gameObject.layer = Layer;
            particleSkinSprite.transform.localScale = Vector3.one;
            particleSkinSprite.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            particleSkinSprite.GetComponent<SpriteRenderer>().sprite = sprite;
            particleSkinSprite.GetComponent<SpriteMask>().sprite = sprite;

            PlayerSkin skin = ((PlayerSkinBank)typeof(PlayerSkinBank).GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, null)).skins[0].currentPlayerSkin;
            PlayerSkin newSkin = GameObject.Instantiate(skin, parent).gameObject.GetComponent<PlayerSkin>();
            newSkin.gameObject.layer = Layer;
            newSkin.gameObject.name = "ObjectParticleSkin";
            newSkin.transform.localScale = Vector3.one;
            //UnityEngine.GameObject.DontDestroyOnLoad(newSkin);
            newSkin.color = skinColors.color;
            newSkin.backgroundColor = skinColors.backgroundColor;
            newSkin.winText = skinColors.winText;
            newSkin.particleEffect = skinColors.particleEffect;
            PlayerSkinParticle newSkinPart = newSkin.GetComponentInChildren<PlayerSkinParticle>();
            ParticleSystem part = newSkinPart.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = part.main;
            ParticleSystem.MinMaxGradient startColor = main.startColor;
            startColor.colorMin = skinColors.backgroundColor;
            startColor.colorMax = skinColors.color;
            main.startColor = startColor;

            newSkinPart.SetFieldValue("startColor1", skinColors.backgroundColor);
            newSkinPart.SetFieldValue("startColor2", skinColors.color);

            parent.gameObject.AddComponent<SetObjectSpriteLayer>();

        }
    }
    class SetObjectSpriteLayer : MonoBehaviour
    {
        void Start()
        {
            int layerID = ObjectParticleSkin.SortingLayerID;
            this.SetSpriteLayerOfChildren(this.gameObject, layerID);
            this.InitParticles(this.gameObject.GetComponentsInChildren<PlayerSkinParticle>(), layerID);
        }
        private void SetSpriteLayerOfChildren(GameObject obj, int layer)
        {
            SpriteMask[] sprites = obj.GetComponentsInChildren<SpriteMask>();
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].frontSortingLayerID = layer;
                sprites[i].backSortingLayerID = layer;
            }
        }
        private void InitParticles(PlayerSkinParticle[] parts, int layer)
        {
            foreach (PlayerSkinParticle skinpart in parts)
            {
                ParticleSystem part = skinpart.GetComponent<ParticleSystem>();
                skinpart.SetFieldValue("part", part);
                part.GetComponent<ParticleSystemRenderer>().sortingLayerID = layer;
                ParticleSystem.MainModule main = part.main;
                skinpart.SetFieldValue("main", main);
                skinpart.SetFieldValue("startColor1", main.startColor.colorMin);
                skinpart.SetFieldValue("startColor2", main.startColor.colorMax);
                part.Play();
            }
        }
    }
}
