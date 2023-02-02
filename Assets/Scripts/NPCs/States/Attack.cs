using CaptainHindsight.StateMachine;
using UnityEngine;

namespace CaptainHindsight
{
    public class Attack : BState
    {
        #region State machine reference
        protected NPC sm;

        public Attack(NPC stateMachine) : base("Attack", stateMachine)
        {
            sm = (NPC)this.stateMachine;
        }
        #endregion

        private float cooldown;
        private float timer;
        private float facingTargetTimer;
        private float newTargetCooldown;

        #region State logic overrides
        public override void Enter()
        {
            base.Enter();

            sm.NavMeshAgent.SetDestination(sm.CurrentTarget.position);
            sm.StateLock = NPCStateLock.Full;
            sm.NavMeshAgent.speed = sm.ActionSpeed;
            sm.ChangeAnimationLayer(1, 1);
            sm.SetAnimations(false, false, true, 2);
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            if (cooldown > 0)
            {
                cooldown -= Time.deltaTime;
                return;
            }

            sm.UpdateAnimationsAndRotation();

            if (sm.ObjectIsClose())
            {
                //if (sm.NavMeshAgent.velocity.x == 0f) sm.FaceTarget();
                facingTargetTimer += Time.deltaTime;
                if (facingTargetTimer <= 0.5f)
                {
                    sm.FaceTarget();
                    facingTargetTimer = 0f;
                }
                sm.NavMeshAgent.SetDestination(sm.transform.position);
                sm.AnimationController.SetTrigger("Attack", true, NPCAnimationTrigger.Attack);
                cooldown = sm.AnimationController.GetCurrentAnimatorStateInfo(0);
            }
            else if (sm.IsCooperating == false && sm.ObjectIsFar())
            {
                timer += Time.deltaTime;

                // Switch back to default movement state after (ActionFocus) seconds
                if (timer >= sm.ActionFocus)
                    sm.SwitchToDefaultMovementState();

                // Check for new targets during this time
                if (newTargetCooldown <= 0)
                {
                    sm.CheckForNewTarget();
                    newTargetCooldown = 0.75f;
                }
                newTargetCooldown -= Time.deltaTime;
            }
            else
            {
                if (sm.AgentHasReachedDestination())
                {
                    sm.IsCooperating = false;
                    sm.NavMeshAgent.SetDestination(sm.CurrentTarget.position);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();

            cooldown = 0f;
            timer = 0f;
            newTargetCooldown = 0f;
            sm.IsCooperating = false;
            sm.StateLock = NPCStateLock.Off;
            sm.ChangeAnimationLayer(1, 0);
            sm.SetAnimations(false, false, true, 1);
            sm.NavMeshAgent.speed = sm.MovementSpeed;
            sm.NavMeshAgent.SetDestination(sm.transform.position);
        }
        #endregion
    }
}