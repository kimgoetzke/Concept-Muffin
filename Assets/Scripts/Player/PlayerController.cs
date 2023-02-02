using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.U2D.IK;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using Ink.Runtime;

namespace CaptainHindsight
{
    [DisallowMultipleComponent]
    public class PlayerController : GameStateBehaviour
    {
        [Header("General")]
        private Rigidbody rb;
        private Animator playerAnimator;
        private Dictionary<int, string> animationDirectionsList = new Dictionary<int, string>();

        [Title("Skeletons")]
        [SerializeField] private Transform[] playerSkeletons;
        private Transform currentSkeleton;

        [Title("Solvers")]
        [Required][SerializeField] private Solver2D[] rightHandSolvers;
        [Required][SerializeField] private Solver2D[] leftHandSolvers;
        [Required][SerializeField] private Solver2D[] rightArmSolvers;
        [Required][SerializeField] private Solver2D[] leftArmSolvers;

        [Title("Movement")]
        [Required][SerializeField] private float mSpeed;
        [ReadOnly][SerializeField] private float mMaxSpeed;
        [Required][SerializeField] private LayerMask groundLayer;
        private PlayerInputActions playerInputActions;
        private int currentDirection;
        private bool canMove;
        private bool isMoving;
        private MouseController mouseController;

        [Title("Equipment")]
        [Required][SerializeField, ReadOnly] private Transform aimTransform; // Only used for parenting and to switch on/off
        [Required][SerializeField, ReadOnly] private Transform equipmentTransform; // Used for aiming offsets to produce more realistic aim circle
        private EquipmentController equipmentController;
        private bool isEquipped;
        private bool canShoot = true;

        [Header("Equipment transform")]
        private float lastFrameXOffset;
        private float offsetX = 0f;
        private float offsetY = 0f;

        [Title("Interaction")]
        [SerializeField]
        private LayerMask interactionLayer;

        #region Awake & Start
        private void Awake()
        {
            // Deactivate all skeletons
            foreach (var skeleton in playerSkeletons) skeleton.gameObject.SetActive(false);

            // Get references
            rb = GetComponent<Rigidbody>();

            // Create list of directions and activate default skeleton
            CreateListOfDirectionalAnimations();
            currentSkeleton = playerSkeletons[currentDirection];
            UpdateSkeleton(2); // Player faces south

            // No equipment active but aimTransform not deactivated
            UpdateSolverWeights(false);
        }

        private void Start()
        {
            mouseController = MouseController.Instance;
            equipmentController = equipmentTransform.GetComponent<EquipmentController>();
        }
        #endregion

        private void FixedUpdate()
        {
            if (canMove)
            {
                Vector3 lookDirection = mouseController.MPGroundLevel - rb.position;
                int skeletalDirection = Helper.ConvertDirectionToIndex(lookDirection);

                // Use correct skeletal rig
                UpdateSkeleton(skeletalDirection);

                // Adjust equipment sprite based on look direction
                UpdateEquipment(skeletalDirection);

                // Aim weapon at mouse (used when moving with keyboard inputs)
                AimAtMouse(skeletalDirection, lookDirection, out float angle);

                // Adjust position of aim transform depending on direction, speed and status
                OffsetPositionOfEquipmentTransform(lookDirection, angle, skeletalDirection);

                // Move player using mouse input, if allowed
                if (isMoving)
                    rb.velocity = new Vector3(Mathf.Clamp(lookDirection.x * mSpeed, -mMaxSpeed, mMaxSpeed), rb.velocity.y, Mathf.Clamp(lookDirection.z * mSpeed, -mMaxSpeed, mMaxSpeed));
            }

            // Update player animator
            playerAnimator.SetFloat("moveSpeed", Mathf.Abs(rb.velocity.magnitude));
        }

        #region InputAction: Attack
        private void Attack(InputAction.CallbackContext context)
        {
            if (isEquipped == false || canShoot == false) return;
            equipmentController.Attack(transform.position);
        }


        #endregion

        #region InputAction: Interact
        private void Interact(InputAction.CallbackContext context)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 5f, interactionLayer);

            //Helper.Log("[PlayerController] Detected " + colliders.Length + " colliders.");
            for (int i = 0; i < colliders.Length; i++)
            {
                //Helper.Log(" - " + (i + 1));
                colliders[i].GetComponent<IInteractable>().Interact(transform.position);
                //Helper.Log("[PlayerController] Interacted with " + colliders[i].transform.parent.name + " (" + colliders[i].name + ").");
            }
        }
        #endregion

        #region InputAction: Pause
        private async void Pause(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                await Task.Yield();
                EventManager.Instance.RequestPauseMenu();
            }
        }
        #endregion

        #region Managing aiming
        private void AimAtMouse(int skeletalDirection, Vector3 lookDirection, out float angle)
        {
            angle = 0;
            if (skeletalDirection == 0 && lookDirection.magnitude < 1.3f)
            {
                equipmentTransform.eulerAngles = new Vector3(-20f, 0f, 90f); //new Vector3(45f, 0f, 90f);
            }
            else
            {
                Vector3 modifiedPosition = new Vector3(transform.position.x, transform.position.y + 0.75f, transform.position.z + 0.5f); // transform.position.z + 0.5f
                Vector3 aimDirection = (mouseController.MPGroundLevel - modifiedPosition).normalized;
                angle = Mathf.Atan2(aimDirection.z, aimDirection.x) * Mathf.Rad2Deg;
                equipmentTransform.eulerAngles = new Vector3(45f, 0f, angle + 180f);
            }
        }

        private void OffsetPositionOfEquipmentTransform(Vector3 lookDirection, float angle, int skeletalDirection)
        {
            // Only calculate offsets when anything is equipped
            if (isEquipped == false) return;

            // Set base variables
            float unadjustedXPosition = equipmentTransform.localPosition.x - lastFrameXOffset;
            float adjustedAngle = Mathf.Abs(angle - 90f) / 90f;
            offsetX = 0;

            // Move the transfrom further towars the player when aiming above the player
            if (skeletalDirection == 0) offsetY = 0.15f * (1 - adjustedAngle);
            else if (adjustedAngle < 1f) offsetY = 0.075f * (1 - adjustedAngle);
            else offsetY = 0;

            // Move the transform further away from the player when moving West/East
            if (isMoving)
            {
                if (currentDirection == 1) offsetX = Mathf.Clamp01(Mathf.Abs(lookDirection.magnitude) / 2) * -0.1f;
                else if (currentDirection == 3) offsetX = Mathf.Clamp01(Mathf.Abs(lookDirection.magnitude) / 2) * 0.1f;
            }

            equipmentTransform.localPosition = new Vector3(unadjustedXPosition + offsetX, 0.75f - offsetY, 0);
            lastFrameXOffset = offsetX;
            //Helper.Log("Angle: " + angle + ", offsetX: " + offsetX + ", offsetY: " + offsetY + ".");
        }
        #endregion

        #region Managing skeleton and player animations
        private void CreateListOfDirectionalAnimations()
        {
            animationDirectionsList.Add(0, "moveN");
            animationDirectionsList.Add(1, "moveW");
            animationDirectionsList.Add(2, "moveS");
            animationDirectionsList.Add(3, "moveE");

            // foreach (var item in animationDirectionsList) Helper.Log("[PlayerController] Added " + item.Key + " with value " + item.Value + " to the list.");
        }

        private void UpdateSkeleton(int direction)
        {
            // Guard clause to prevent constant updating
            if (direction == currentDirection) return;

            // Update current direction
            currentDirection = direction;

            // Update solver weights
            if (aimTransform.gameObject.activeInHierarchy) UpdateSolverWeights(true);
            else UpdateSolverWeights(false);

            // Get normalised time for current animation clip to be able to resume
            float time = 0;
            if (playerAnimator != null)
                time = playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            // Deactivate current animator and skeleton
            currentSkeleton.gameObject.SetActive(false);
            playerSkeletons[currentDirection].gameObject.SetActive(true);
            currentSkeleton = playerSkeletons[currentDirection];

            // Update animator and animation
            playerAnimator = playerSkeletons[currentDirection].GetComponent<Animator>();
            playerAnimator.enabled = true;
            playerAnimator.Play(animationDirectionsList[currentDirection], 0, time);
            //Helper.Log("[PlayerController] Changing animation to " + directionsList[currentDirection] + ".");

            // Update aim transform
            aimTransform.parent = currentSkeleton;

            // Reset equipment transform which is adjusted when looking NW, N or NE
            equipmentTransform.localPosition = new Vector3(equipmentTransform.localPosition.x, 0.75f, 0);
        }

        private void UpdateSolverWeights(bool equipmentUsed)
        {
            // Set the weight (only use arm solvers when no equipment is used)
            float weight;
            if (equipmentUsed) weight = 0.01f;
            else weight = 1f;

            // Update arms/hands
            rightArmSolvers[currentDirection].weight = weight;
            rightHandSolvers[currentDirection].weight = (1f - weight);
            leftArmSolvers[currentDirection].weight = weight;
            leftHandSolvers[currentDirection].weight = (1f - weight);
        }
        #endregion

        #region Managing equipment
        private void Equip(int equipmentSlot)
        {
            if (equipmentSlot == 1)
            {
                aimTransform.gameObject.SetActive(false);
                isEquipped = false;
                UpdateSolverWeights(false);
                EventManager.Instance.ChangeCursor(0);
            }
            else
            {
                aimTransform.gameObject.SetActive(true);
                isEquipped = true;
                equipmentController.ChangeEquipment(equipmentSlot);
                UpdateSolverWeights(true);
            }
        }

        private void UpdateEquipment(int direction)
        {
            // Guard clause to prevent constant updating
            if (isEquipped == false) return;
            equipmentController.ChangeDirection(direction, false);
        }
        #endregion

        #region Managing death
        private void Die()
        {
            Helper.Log("[PlayerController] Player dies.");
        }
        #endregion

        #region Managing events
        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            if (state == GameState.GameOver) Die();

            canMove = settings.PlayerCanMove;
        }

        private void ActionDialogueStateChange(bool state)
        {
            canShoot = !state;
            canMove = !state;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            playerInputActions = new PlayerInputActions();
            playerInputActions.Player.Enable();
            playerInputActions.Player.Attack.performed += Attack;
            playerInputActions.Player.Interact.performed += Interact;
            playerInputActions.Player.Move.started += context => isMoving = true;
            playerInputActions.Player.Move.canceled += context => isMoving = false;
            playerInputActions.Player.EquipmentSlot1.performed += context => Equip(1);
            playerInputActions.Player.EquipmentSlot2.performed += context => Equip(2);
            playerInputActions.Player.EquipmentSlot3.performed += context => Equip(3);
            playerInputActions.Player.Pause.performed += Pause;
            EventManager.Instance.OnDialogueStateChange += ActionDialogueStateChange;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            playerInputActions.Player.Attack.performed -= Attack;
            playerInputActions.Player.Interact.performed -= Interact;
            playerInputActions.Player.Pause.performed -= Pause;
            playerInputActions.Player.Disable();
            EventManager.Instance.OnDialogueStateChange -= ActionDialogueStateChange;
        }
        #endregion
    }
}
