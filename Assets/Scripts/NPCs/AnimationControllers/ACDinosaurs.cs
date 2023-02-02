using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight
{
    public class ACDinosaurs : AnimationController
    {
        [Title("Dinosaurs")]
        [ReadOnly] public NPCType Type = NPCType.Dinosaurs;
        [SerializeField][Required] private Transform awarenessTransform;
        [SerializeField][Required] private Transform perspectiveTransform;

        public override NPCType GetNPCType()
        {
            return Type;
        }

        public override void UpdateAnimationAndRotation(float x, float y)
        {
            if (x < -0.1f)
            {
                Animator.SetBool("isRunning", true);
                if (IsFacingRight) FlipLocalScale();
            }
            else if (x > 0.1f)
            {
                Animator.SetBool("isRunning", true);
                if (IsFacingRight == false) FlipLocalScale();
            }
            else Animator.SetBool("isRunning", false);
        }

        public override void FaceTarget(Transform target)
        {
            float x = target.position.x - transform.position.x;
            if (x < 0 && IsFacingRight) FlipLocalScale();
            else if (x > 0 && IsFacingRight == false) FlipLocalScale();
        }

        private void FlipLocalScale()
        {
            if (IsFacingRight)
            {
                perspectiveTransform.localScale = new Vector3(1f, 1f, 1f);
                awarenessTransform.localScale = new Vector3(1f, 1f, 1f);
            }
            else
            {
                perspectiveTransform.localScale = new Vector3(-1f, 1f, 1f);
                awarenessTransform.localScale = new Vector3(-1f, 1f, 1f);
            }

            IsFacingRight = !IsFacingRight;
        }
    }
}
