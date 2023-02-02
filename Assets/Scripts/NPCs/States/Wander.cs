using CaptainHindsight.StateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace CaptainHindsight
{
    public class Wander : BState
    {
        #region State machine reference
        protected NPC sm;

        public Wander(NPC stateMachine) : base("Wander", stateMachine)
        {
            sm = (NPC)this.stateMachine;
        }
        #endregion
        
        [HideInInspector]
        private float timer;

        #region State logic overrides
        public override void Enter()
        {
            base.Enter();

            sm.RemoveCurrentTarget();
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            timer += Time.deltaTime;

            if (timer >= sm.WanderTimer)
            {
                Vector3 newPos = SetRandomPosition(sm.transform.position, sm.WanderRadius, -1);
                sm.NavMeshAgent.SetDestination(newPos);
                timer = 0;
            }

            sm.UpdateAnimationsAndRotation();
        }

        public override void Exit()
        {
            base.Exit();

            sm.NavMeshAgent.SetDestination(sm.transform.position);
            sm.SetAnimations(false, false);
        }
        #endregion

        #region State specific logic
        private static Vector3 SetRandomPosition(Vector3 origin, float dist, int layermask)
        {
            Vector3 randomDirection = Random.insideUnitSphere * dist;
            randomDirection += origin;
            NavMeshHit navMeshHit;

            // Note: This function currently targets all layers, not just the ground layer.
            // As a result, the destination can be set to a position outside the navMesh.
            // This is unlikely to be a problem but may look odd at times when the enemy
            // just stands infront of a cliff awkwardly.
            NavMesh.SamplePosition(randomDirection, out navMeshHit, dist, layermask);

            return navMeshHit.position;
        }
        #endregion
    }
}