using Sirenix.OdinInspector;
using UnityEngine;
using static CaptainHindsight.FactionManager;

namespace CaptainHindsight
{
    [DisallowMultipleComponent]
    public class FactionIdentity : MonoBehaviour
    {
        [SerializeField]
        private Faction faction;

        [ShowInInspector, ReadOnly, HideIf("faction", Faction.Player)]
        private FactionGroup factionGroup;

        private void Start()
        {
            GetMyFactionGroup();
        }

        public Faction GetFaction()
        {
            return faction;
        }

        public void GetMyFactionGroup()
        {
            factionGroup = Instance.GetFactionGroup(faction);
        }

        public FactionStatus GetMyFactionStatus(Faction otherFaction)
        {
            if (faction == Faction.Player)
            {
                Helper.LogWarning("[FactionIdentity] Player cannot request faction status. The player's actions will determine the status.");
                return FactionStatus.None;
            }

            FactionStatus factionStatus;
            switch (otherFaction)
            {
                case Faction.Player: factionStatus = factionGroup.Player;
                    break;
                case Faction.Dinosaur: factionStatus = factionGroup.Dinosaur;
                    break;
                case Faction.RIGID: factionStatus = factionGroup.RIGID;
                    break;
                case Faction.CorpX: factionStatus = factionGroup.CorpX;
                    break;
                default: factionStatus = factionGroup.Unspecified;
                    break;
            }
            return factionStatus;
        }

        private void ActionFactionChange()
        {
            //Helper.Log("[FactionIdentity] " + transform.name + " requested updated FactionGroup.");
            GetMyFactionGroup();
        }

        #region Managing events
        private void OnEnable()
        {
            EventManager.Instance.OnFactionsChange += ActionFactionChange;
        }

        private void OnDestroy()
        {
            EventManager.Instance.OnFactionsChange -= ActionFactionChange;
        }
        #endregion
    }
}
