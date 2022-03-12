using UnityEngine;
using GameModeCollection.Extensions;
using UnboundLib;
using ItemShops.Extensions;

namespace GameModeCollection.GameModes.TRT
{
    public abstract class TRT_Role : MonoBehaviour, ITRT_Role
    {
        public abstract TRT_Role_Appearance Appearance { get; }
        public abstract Alignment Alignment { get; }
        public virtual int MaxCards { get; } = GM_TRT.BaseMaxCards;
        public abstract float BaseHealth { get; }
        public abstract bool CanDealDamageAndTakeEnvironmentalDamage { get; }
        public abstract float KarmaChange { get; protected set; }
        public abstract int StartingCredits { get; }
        public abstract bool AlertAlignment(Alignment alignment);
        public abstract TRT_Role_Appearance AppearToAlignment(Alignment alignment);
        public abstract void OnCorpseInteractedWith(Player player);
        public abstract void OnInteractWithCorpse(TRT_Corpse corpse, bool interact);
        public virtual void OnAnyPlayerDied(Player deadPlayer, ITRT_Role[] rolesRemaining)
        {
            
        }
        public abstract void OnKilledByPlayer(Player killingPlayer);
        public abstract void OnKilledPlayer(Player killedPlayer);
        public abstract bool WinConditionMet(Player[] playersRemaining);
        public abstract void TryShop();

        protected virtual void Start()
        {
            CharacterData data = this.GetComponent<CharacterData>();
            if (data != null && !data.dead)
            {
                data.SetMaxCards(this.MaxCards);
                data.maxHealth = this.BaseHealth;
                data.healthHandler.Revive(true);
            }
            if (data.view.IsMine)
            {
                UIHandler.instance.roundCounterSmall.UpdateText(1, this.Appearance.Name.ToUpper(), this.Appearance.Color, 30, Vector3.one, GM_TRT.DisplayBackgroundColor);
            }
            this.GetComponent<Player>().GetAdditionalData().bankAccount.Deposit(TRTShopHandler.TRT_Currency, this.StartingCredits);
            this.ExecuteAfterFrames(5, BetterChat.BetterChat.EvaluateCanSeeGroup);
        }
        protected virtual void OnDestroy()
        {
            this.GetComponent<CharacterData>()?.SetMaxCards(GM_TRT.BaseMaxCards);
            this.GetComponent<Player>().GetAdditionalData().bankAccount.RemoveAllMoney();
        }
    }
}
