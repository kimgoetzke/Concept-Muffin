using Sirenix.OdinInspector;
using UnityEngine;
using static CaptainHindsight.FactionManager;

namespace CaptainHindsight
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider))]
    public class MAwareness : MonoBehaviour, INotice
    {
        [Title("Configuration")]
        [SerializeField, Required] 
        private FactionIdentity factionIdentity;

        [ShowInInspector, ReadOnly] 
        private Faction myFaction = Faction.Unspecified;

        [SerializeField, PropertyRange(1, 10), OnValueChanged("UpdateAwarenessRadius")] 
        private float awarenessRadius = 1;

        private NPC npc;

        #region Unity Editor methods (Odin and OnDrawGizmos)
        private void UpdateAwarenessRadius()
        {
            GetComponent<SphereCollider>().radius = awarenessRadius;
        }
        #endregion

        private void Start()
        {
            myFaction = factionIdentity.GetFaction();
            npc = Helper.GetComponentSafely<NPC>(transform.parent.gameObject);
            if (npc != null)
            {
                SphereCollider sphereCollider = GetComponent<SphereCollider>();
                npc.InitialiseAwarenessTrigger(sphereCollider.center, sphereCollider.radius);
            }
            else
            {
                gameObject.SetActive(false);
                Helper.LogWarning("[MAwareness] " + npc.transform.name + " couldn't find parent state machine. Initialisation incomplete. AwarenessRadius disabled.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("NPC"))
            {
                Faction otherFaction = other.GetComponent<FactionIdentity>().GetFaction();
                FactionStatus factionStatus = factionIdentity.GetMyFactionStatus(otherFaction);
                //Helper.Log("[MAwareness] " + npc.transform.name + " spotted " + other.transform.name + " (FactionStatus: " + factionStatus +").");
                npc.AwarenessTrigger(true, other.transform, factionStatus); 
                return;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("NPC"))
            {
                //Helper.Log("[MAwareness] " + npc.transform.name + " lost sight of " + other.transform.name + ".");
                npc.AwarenessTrigger(false, other.transform);
                return;
            }
        }

        public void Notice(Transform target)
        {
            if (target.CompareTag("Impact"))
            {
                //Helper.Log("[MAwareness] " + transform.parent.name + " noticed " + target.tag + " at " + target.position + ".");
                npc.AwarenessTrigger(true, target);
                return;
            }
        }
    }
}
