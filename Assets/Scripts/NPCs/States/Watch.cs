using CaptainHindsight.StateMachine;

namespace CaptainHindsight
{
    public class Watch : BState
    {
        #region State machine reference
        protected NPC sm;

        public Watch(NPC stateMachine) : base("Watch", stateMachine)
        {
            sm = (NPC)this.stateMachine;
        }
        #endregion

        private float countdown;

        #region State logic overrides
        public override void Enter()
        {
            base.Enter();

            sm.SetAnimations(false, true);
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            if (sm.CurrentTarget == null && sm.StateLock == NPCStateLock.Off)
                sm.SwitchToDefaultMovementState();
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            if (sm.NavMeshAgent.velocity.x != 0 || sm.NavMeshAgent.velocity.y != 0)
            {
                countdown = 0.5f;
                sm.UpdateAnimationsAndRotation();
            }
            else if (countdown > 0f) sm.UpdateAnimationsAndRotation();
            else sm.FaceTarget();
        }

        public override void Exit()
        {
            base.Exit();

            countdown = 0f;
            sm.SetAnimations(false, false);
        }
        #endregion
    }
}