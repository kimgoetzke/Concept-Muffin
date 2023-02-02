using Sirenix.OdinInspector;
using UnityEngine;
using static CaptainHindsight.FactionManager;

namespace CaptainHindsight
{
    [DisallowMultipleComponent]
    public class MCooperation : MonoBehaviour
    {
        [Title("Configuration")]
        [SerializeField, Required]
        private FactionIdentity factionIdentity;

        [ShowInInspector, ReadOnly]
        private Faction myFaction = Faction.Unspecified;

        [SerializeField, PropertyRange(2, 20)] 
        private float cooperationRadius = 2;

        private NPC npc;

        #region Unity Editor methods (Odin and OnDrawGizmos)
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, cooperationRadius);
        }
        #endregion

        private void Start()
        {
            myFaction = factionIdentity.GetFaction();
            npc = Helper.GetComponentSafely<NPC>(transform.parent.gameObject);
            if (npc == null)
                Helper.LogWarning("[MCooperation] " + npc.transform.name + ": Cooperation was initiated but NPC state machine not found. Request incomplete.");
        }

        private void ActionCooperationRequest(Vector3 allyPosition, Transform target, Faction faction)
        {
            // Only listen to request from same faction
            // IMPORTANT: This may need to be reworked if there are cross-non-player-faction alliances
            if (faction != myFaction) return;

            // Only listen to request if distance is within cooperationRadius
            float distance = Vector3.Distance(allyPosition, transform.position);
            if (distance <= 0.1f || distance >= cooperationRadius) return;

            // Action request
            if (npc != null) npc.CooperateWithOthers(target);
            //Helper.Log("[MCooperation] " + transform.parent.name + " will cooperate (distance: " + distance + ").");
        }

        #region Managing events
        private void OnEnable()
        {
            EventManager.Instance.OnCooperationRequest += ActionCooperationRequest;
        }

        private void OnDestroy()
        {
            EventManager.Instance.OnCooperationRequest -= ActionCooperationRequest;
        }
        #endregion
    }
}
