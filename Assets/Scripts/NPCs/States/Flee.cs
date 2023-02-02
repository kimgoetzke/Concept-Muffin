using CaptainHindsight.StateMachine;

namespace CaptainHindsight
{
    public class Flee : BState
    {
        #region State machine reference
        protected NPC sm;

        public Flee(NPC stateMachine) : base("Flee", stateMachine)
        {
            sm = (NPC)this.stateMachine;
        }
        #endregion

        #region State logic overrides
        public override void Enter()
        {
            base.Enter();

            sm.StateLock = NPCStateLock.Full;
            sm.NavMeshAgent.speed = sm.ActionSpeed;
            sm.SetAnimations(false, false, true, 2);

            // Set destination for fleeing based on settings
            if (sm.RandomFlee) sm.MoveAwayFromObject(true);
            else sm.NavMeshAgent.SetDestination(sm.DefaultTarget.position);
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            sm.UpdateAnimationsAndRotation();

            if (sm.AgentHasReachedDestination())
                sm.SwitchToDefaultMovementState();
        }

        public override void Exit()
        {
            base.Exit();

            sm.StateLock = NPCStateLock.Off;
            sm.SetAnimations(false, false, true, 1);
            sm.NavMeshAgent.SetDestination(sm.transform.position);
            sm.NavMeshAgent.speed = sm.MovementSpeed; 
        }
        #endregion
    }
}