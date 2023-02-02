using CaptainHindsight.StateMachine;

namespace CaptainHindsight
{
    public class Idle : BState
    {
        #region State machine reference
        protected NPC sm;

        public Idle(NPC stateMachine) : base("Idle", stateMachine)
        {
            sm = (NPC)this.stateMachine;
        }
        #endregion

        #region State logic overrides
        public override void Enter()
        {
            base.Enter();

            sm.RemoveCurrentTarget();

            switch (sm.Movement)
            {
                case NPCMovement.Wander:
                    stateMachine.SwitchState(sm.WanderState);
                    break;
                case NPCMovement.Idle:
                    break;
                case NPCMovement.Patrol:
                    stateMachine.SwitchState(sm.PatrolState);
                    break;
                default:
                    sm.LogSwitchStateWarning(this);
                    break;
            }
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            sm.MoveIfPushedAway();
        }
        #endregion
    }
}