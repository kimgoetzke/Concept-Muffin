using CaptainHindsight.StateMachine;

namespace CaptainHindsight
{
    public class Observe : BState
    {
        #region State machine reference
        protected NPC sm;

        public Observe(NPC stateMachine) : base("Observe", stateMachine)
        {
            sm = (NPC)this.stateMachine;
        }
        #endregion

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

            sm.FaceTarget();
        }

        public override void Exit()
        {
            base.Exit();

            sm.SetAnimations(false, false);
        }
        #endregion
    }
}