using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace CaptainHindsight
{
    public class AnimationController : MonoBehaviour
    {
        [Title("Animation Controller")]
        public Animator Animator;
        [ShowInInspector, ReadOnly] public bool IsFacingRight;

        public virtual void UpdateAnimationAndRotation(float x, float y) { }
        public virtual void FaceTarget(Transform target) { }
        public virtual void ChangeAnimationLayer(int layer, float weight, bool reset = false) { }

        public virtual NPCType GetNPCType() 
        {
            return NPCType.Unspecified;
        }

        public virtual void SetTrigger(string name, bool randomInt = false, NPCAnimationTrigger trigger = NPCAnimationTrigger.Unspecified)
        {
            Animator.SetTrigger(name);
        }

        public void SetBool(string name, bool status, bool single = false)
        {
            if (single) ResetBools();

            Animator.SetBool(name, status);
        }

        public void ResetBools()
        {
            foreach (var parameter in Animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Bool)
                    Animator.SetBool(parameter.name, false);
            }
        }

        public void SetFloat(string name, float value)
        {
            Animator.SetFloat(name, value);
        }

        public void SetInteger(string name, int value)
        {
            Animator.SetInteger(name, value);
        }

        public void SetAnimatorSpeed(float speed)
        {
            Animator.speed = speed;
        }

        public bool GetBool(string name)
        {
            return Animator.GetBool(name);
        }

        public float GetCurrentAnimatorStateInfo(int layer)
        {
            return Animator.GetCurrentAnimatorStateInfo(layer).length;
        }
    }
}
