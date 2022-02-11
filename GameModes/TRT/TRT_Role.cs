using UnityEngine;
using GameModeCollection.Extensions;

namespace GameModeCollection.GameModes.TRT
{
    public abstract class TRT_Role : MonoBehaviour, ITRT_Role
    {
        public abstract TRT_Role_Appearance Appearance { get; }
        public abstract Alignment Alignment { get; }
        public abstract int MaxCards { get; }
        public abstract int StartingCards { get; }
        public abstract float BaseHealth { get; }
        public abstract bool CanDealDamage { get; }
        public abstract bool AlertAlignment(Alignment alignment);
        public abstract TRT_Role_Appearance AppearToAlignment(Alignment alignment);
        public abstract void OnCorpseInteractedWith(Player player);
        public abstract void OnInteractWithCorpse(TRT_Corpse corpse);
        public abstract void OnKilledByPlayer(Player killingPlayer);
        public abstract void OnKilledPlayer(Player killedPlayer);
        public abstract bool WinConditionMet(Player[] playersRemaining);

        protected virtual void Start()
        {
            this.GetComponent<CharacterData>()?.SetMaxCards(this.MaxCards);
        }
        protected virtual void OnDestroy()
        {
            this.GetComponent<CharacterData>()?.SetMaxCards(int.MaxValue);
        }
    }
}
