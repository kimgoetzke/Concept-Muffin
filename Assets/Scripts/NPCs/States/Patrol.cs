using CaptainHindsight.StateMachine;
using UnityEngine;

namespace CaptainHindsight
{
    public class Patrol : BState
    {
        #region State machine reference
        protected NPC sm;

        public Patrol(NPC stateMachine) : base("Patrol", stateMachine)
        {
            sm = (NPC)this.stateMachine;
        }
        #endregion

        private float timer;
        private bool isWaiting;
        private int currentWaypointIndex;

        #region State logic overrides
        public override void Enter()
        {
            base.Enter();

            sm.RemoveCurrentTarget();
            sm.NavMeshAgent.SetDestination(sm.Waypoints[currentWaypointIndex].position);
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            if (isWaiting)
            {
                timer += Time.deltaTime;
                if (timer >= sm.WaitAtCheckPoint)
                {
                    isWaiting = false;
                    sm.NavMeshAgent.SetDestination(sm.Waypoints[currentWaypointIndex].position);
                }
            }
            else if (sm.AgentHasReachedDestination())
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % sm.Waypoints.Length;
                timer = 0f;
                isWaiting = true;
            }

            sm.UpdateAnimationsAndRotation();
        }

        public override void Exit()
        {
            base.Exit();

            sm.SetAnimations(false, false);
            sm.NavMeshAgent.SetDestination(sm.transform.position);
        }
        #endregion
    }
}