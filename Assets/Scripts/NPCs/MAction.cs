using Sirenix.OdinInspector;
using UnityEngine;
using static CaptainHindsight.FactionManager;

namespace CaptainHindsight
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    public class MAction : MonoBehaviour
    {
        [Title("Configuration")]
        [SerializeField, Required] 
        private FactionIdentity factionIdentity;

        [Required]
        [SerializeField, PropertyRange(0.1f, 5), OnValueChanged("UpdateActionRadius")]
        private float actionRadius = 1f;

        private NPC npc;

        #region Unity Editor methods (Odin and OnDrawGizmos)
        private void UpdateActionRadius()
        {
            GetComponent<SphereCollider>().radius = actionRadius;
        }
        #endregion

        private void Start()
        {
            npc = Helper.GetComponentSafely<NPC>(transform.parent.gameObject);
            if (npc != null)
            {
                npc.InitialiseActionTrigger(actionRadius);
            }
            else
            {
                gameObject.SetActive(false);
                //Helper.LogWarning("[MAction] " + npc.transform.name + " couldn't find parent state machine. Initialisation incomplete. AwarenessRadius disabled.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bullet"))
            {
                npc.ActionTrigger(other.GetComponent<Bullet>().Owner);
            }
            else if (other.CompareTag("NPC") || other.CompareTag("Player"))
            {
                Faction otherFaction = other.GetComponent<FactionIdentity>().GetFaction();
                FactionStatus factionStatus = factionIdentity.GetMyFactionStatus(otherFaction);
                switch (factionStatus)
                {
                    case FactionStatus.Neutral:
                        //Helper.Log("[MAction] " + npc.transform.name + ": Neutral towards " + other.name + " - no action taken.");
                        return;
                    case FactionStatus.Ally:
                        //Helper.Log("[MAction] " + npc.transform.name + ": Allied to " + other.name + " - no action taken.");
                        return;
                    default:
                        break;
                }
                npc.ActionTrigger(other.transform);
            }
            else return;

            //Helper.Log("[MAction] " + npc.transform.name + ": " + other.name + " entered the actionRadius.");
        }
    }
}
