using CaptainHindsight.StateMachine;
using UnityEngine;

namespace CaptainHindsight
{
    public class Investigate : BState
    {
        #region State machine reference
        protected NPC sm;

        public Investigate(NPC stateMachine) : base("Investigate", stateMachine)
        {
            sm = (NPC)this.stateMachine;
        }
        #endregion

        private float investigationTimer;
        private float investigationCountdown;
        private float switchStateTimer;
        private float switchStateCountdown;
        private bool isInvestigating;

        #region State logic overrides
        public override void Enter()
        {
            base.Enter();

            investigationCountdown = sm.InvestigateAfter;
            switchStateCountdown = Mathf.Max(sm.InvestigateAfter * 2, 2f);
            sm.FaceTarget();
            sm.StateLock = NPCStateLock.Partial;
            sm.SetAnimations(false, true);
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            investigationTimer += Time.deltaTime;

            if (investigationTimer >= investigationCountdown)
            {
                // Set new target if CurrentTarget is close enough
                if (sm.ObjectIsFar() == false)
                {
                    sm.AnimationController.SetBool("isAware", false);
                    sm.NavMeshAgent.SetDestination(sm.CurrentTarget.position);
                    isInvestigating = true;
                    investigationTimer = 0f;
                    switchStateTimer = 0f;
                    Helper.Log("[NPC] " + sm.transform.name + ": SetDestination for investigation at " + sm.CurrentTarget.position + ".");
                }
                else isInvestigating = false;

                // Initiate switchState if isInvestigating is false
                if (isInvestigating == false && sm.AgentHasReachedDestination())
                {
                    if (sm.NavMeshAgent.velocity.magnitude > 0) return;

                    switchStateTimer += Time.deltaTime;

                    if (sm.AnimationController.GetBool("isAware") == false)
                    {
                        Helper.Log("[NPC] " + sm.transform.name + ": Investigation is over. CurrentTarget is too far away. SwitchState countdown has begun...");
                        sm.AnimationController.SetBool("isAware", true);
                    }

                    if (switchStateTimer >= switchStateCountdown)
                        sm.SwitchToDefaultMovementState();
                }
            }

            sm.UpdateAnimationsAndRotation();
        }

        public override void Exit()
        {
            base.Exit();

            investigationTimer = 0f;
            switchStateTimer = 0f;
            isInvestigating = false;
            sm.StateLock = NPCStateLock.Off;
            sm.SetAnimations(false, false);
            sm.NavMeshAgent.SetDestination(sm.transform.position);
        }
        #endregion
    }
}