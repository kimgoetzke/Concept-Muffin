using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CaptainHindsight
{
    public class ACHumanoid : AnimationController
    {
        [Title("Humanoid")]
        [ReadOnly] public NPCType Type = NPCType.Humanoid;
        [SerializeField] private NPC stateMachine;
        [SerializeField] private Transform[] skeletons;
        private int attackAnimations = 2;
        private Transform currentSkeleton;
        private int currentDirection = 0;

        private void Awake()
        {
            // Make sure only skeleton for currentDirection is active
            foreach (var skeleton in skeletons) skeleton.gameObject.SetActive(false);
            currentSkeleton = skeletons[currentDirection];
            currentSkeleton.gameObject.SetActive(true);
        }

        public override NPCType GetNPCType()
        {
            return Type;
        }

        public override void UpdateAnimationAndRotation(float x, float y)
        {
            if (DirectionHasChanged(x)) UpdateSkeleton();
            Animator.SetFloat("moveSpeed", Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)));
        }

        public override void FaceTarget(Transform target)
        {
            float x = target.position.x - transform.position.x;
            if (DirectionHasChanged(x)) UpdateSkeleton();
        }

        public override void ChangeAnimationLayer(int layer, float weight, bool reset = false)
        {
            if (reset == true)
            {
                ResetLayers();
                return;
            }
            else Animator.SetLayerWeight(layer, weight);
        }

        private void ResetLayers()
        {
            for (int i = 0; i < Animator.layerCount; i++)
            {
                Animator.SetLayerWeight(i, 0);
            }
            Animator.SetLayerWeight(0, 1);
            Helper.Log("[ACHumanoid] " + Animator.layerCount + " animation layers were reset.");
        }

        public override void SetTrigger(string name, bool randomInt = false, NPCAnimationTrigger trigger = NPCAnimationTrigger.Unspecified)
        {
            if (randomInt)
            {
                string intName;
                int value;
                switch (trigger)
                {
                    case NPCAnimationTrigger.Attack:
                        intName = "attackSequence";
                        value = Random.Range(0, attackAnimations);
                        break;
                    default:
                        Helper.LogWarning("[ACHumanoid] Invalid SetTrigger command used. Random SetTrigger event requested but NPCAnimationTrigger does not exist.");
                        return;
                }
                Animator.SetInteger(intName, value);
            }
            base.SetTrigger(name);
        }

        private bool DirectionHasChanged(float x)
        {
            if (x < 0 && IsFacingRight)
            {
                currentDirection = 0;
                IsFacingRight = !IsFacingRight;
                return true;
            }
            else if (x > 0 && IsFacingRight == false)
            {
                currentDirection = 1;
                IsFacingRight = !IsFacingRight;
                return true;
            }
            else return false;
        }

        private void UpdateSkeleton()
        {
            // Get layer weights from current skeleton
            float[] layers = new float[Animator.layerCount];
            for (int i = 0; i < Animator.layerCount; i++)
            {
                layers[i] = Animator.GetLayerWeight(i);
                //Helper.Log("Float " + i + " set to " + layers[i]);
            }

            // Activate only skeleton for current direction
            currentSkeleton.gameObject.SetActive(false);
            skeletons[currentDirection].gameObject.SetActive(true);
            currentSkeleton = skeletons[currentDirection];

            // Update animator and animation
            Animator = skeletons[currentDirection].GetComponent<Animator>();
            Animator.enabled = true;
            
            // Set layer weights for new skeleton
            for (int i = 0; i < Animator.layerCount; i++)
            {
                if (currentDirection == 0) Animator.Play("moveW", i);
                else Animator.Play("moveE", i);
                Animator.SetLayerWeight(i, layers[i]);
                //Helper.Log("Layer " + i + " updates to " + layers[i]);
            }
        }
    }
}
