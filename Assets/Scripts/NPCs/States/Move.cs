using CaptainHindsight.StateMachine;
using UnityEngine;

namespace CaptainHindsight
{
    public class Move : BState
    {
        #region State machine reference
        protected NPC sm;

        public Move(NPC stateMachine) : base("Move", stateMachine)
        {
            sm = (NPC)this.stateMachine;
        }
        #endregion

        private float timer;
        private float cooldown = 2f;

        #region State logic overrides
        public override void Enter()
        {
            base.Enter();

            sm.StateLock = NPCStateLock.Partial;
            sm.NavMeshAgent.speed = sm.ActionSpeed;
            sm.MoveAwayFromObject(false);
            sm.SetAnimations(false, false, true, 2);
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            if (sm.AgentHasReachedDestination())
            {
                timer += Time.deltaTime;

                if (timer >= cooldown)
                {
                    timer = 0f;

                    // Exit state if object is not close by
                    if (sm.ObjectIsClose() == false) 
                        sm.SwitchToDefaultMovementState();
                }    
            }
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            sm.UpdateAnimationsAndRotation();

            if (sm.ObjectIsClose()) sm.MoveAwayFromObject(false);
        }

        public override void Exit()
        {
            base.Exit();

            sm.StateLock = NPCStateLock.Off;
            sm.NavMeshAgent.SetDestination(sm.transform.position);
            sm.NavMeshAgent.speed = sm.MovementSpeed;
            sm.SetAnimations(false, false, true, 1);
        }
        #endregion
    }
}