using CaptainHindsight.StateMachine;
using UnityEngine;

namespace CaptainHindsight
{
    public class Cooperate : BState
    {
        #region State machine reference
        protected NPC sm;

        public Cooperate(NPC stateMachine) : base("Cooperate", stateMachine)
        {
            sm = (NPC)this.stateMachine;
        }
        #endregion

        private float timer;

        #region State logic overrides
        public override void Enter()
        {
            base.Enter();

            sm.SetAnimations(false, true);
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            timer += Time.deltaTime;

            if (timer >= 1f)
                switch (sm.Behaviour)
                {
                    case NPCBehaviour.Anxious:
                        sm.SwitchState(sm.FleeState);
                        break;
                    case NPCBehaviour.Neutral:
                        sm.SwitchToDefaultMovementState();
                        break;
                    case NPCBehaviour.Aggressive:
                        sm.SwitchState(sm.AttackState);
                        break;
                    default:
                        sm.LogSwitchStateWarning(this);
                        break;
                }
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            sm.FaceTarget();
        }

        public override void Exit()
        {
            base.Exit();

            timer = 0f;

            sm.SetAnimations(false, false);
        }
        #endregion
    }
}