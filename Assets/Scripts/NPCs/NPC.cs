using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using System.Threading.Tasks;
using System.Collections.Generic;
using static CaptainHindsight.FactionManager;

namespace CaptainHindsight
{
    [DisallowMultipleComponent]
    public class NPC : StateMachine.BStateMachine
    {
        #region Defining variables
        // General variables
        [FoldoutGroup("General", expanded: false)]
        [ReadOnly]
        private NPCType type; // Set by chosing an AnimationController type

        [FoldoutGroup("General", expanded: false)]
        [SerializeField, Required]
        [ChildGameObjectsOnly(IncludeSelf = false)]
        private FactionIdentity factionIdentity;

        [FoldoutGroup("General", expanded: false)]
        [ShowInInspector, ReadOnly]
        private FactionManager.Faction faction = FactionManager.Faction.Unspecified;

        [FoldoutGroup("General", expanded: false)]
        [ChildGameObjectsOnly(IncludeSelf = true)]
        public AnimationController AnimationController;

        [FoldoutGroup("General", expanded: false)]
        [ChildGameObjectsOnly(IncludeSelf = true)]
        [Required]
        public NavMeshAgent NavMeshAgent;

        [FoldoutGroup("General", expanded: false)]
        [ShowInInspector, ReadOnly]
        public bool IsCooperating;

        [FoldoutGroup("General", expanded: false)]
        [HideInInspector]
        private float countdown;

        // Awareness variables
        [FoldoutGroup("Awareness", expanded: false)]
        [ShowInInspector, ReadOnly]
        private float awarenessRadius;

        [FoldoutGroup("Awareness", expanded: false)]
        [ShowInInspector, ReadOnly]
        private Vector3 awarenessColliderCenter;

        [FoldoutGroup("Awareness", expanded: false)]
        [SerializeField]
        [Tooltip("Determines how quickly NPC reacts to awarenessTriggers by adding a random delay with a minimum of this value.")]
        private float awarenessDelayMin = 0f;

        [FoldoutGroup("Awareness", expanded: false)]
        [SerializeField]
        [Tooltip("Determines how quickly NPC reacts to awarenessTriggers by adding a random delay with a maximum of this value.")]
        private float awarenessDelayMax = 0.3f;

        [FoldoutGroup("Awareness", expanded: false)]
        [Tooltip("Determines how quickly NPC reacts to awarenessTriggers by adding a random delay with a maximum of this value.")]
        [ShowInInspector, ReadOnly]
        private float awarenessDelay;

        [FoldoutGroup("Awareness", expanded: false)]
        [Tooltip("Used in CheckForNewTargets() after CurrentTarget died/disappeared to make sure that NPC reacts quickly to nearby threats that were ignored due to StateLock.")]
        [SerializeField]
        private LayerMask searchLayers;

        // Action variables
        [BoxGroup("Action", centerLabel: true)]
        [ShowInInspector, ReadOnly]
        public NPCStateLock StateLock = NPCStateLock.Off;

        [BoxGroup("Action", centerLabel: true)]
        [ShowInInspector, ReadOnly]
        public Transform CurrentTarget;

        [BoxGroup("Action", centerLabel: true)]
        [ShowInInspector, ReadOnly]
        public string CurrentTargetTag;

        [BoxGroup("Action", centerLabel: true)]
        [SerializeField, Required]
        [PropertyRange(0.1f, 5)][Tooltip("Distance (in world units) between CurrentTarget and enemy for enemy to start attacking. The default is 1.")]
        private float actionDistance = 1f;

        [BoxGroup("Action", centerLabel: true)]
        [Required]
        [PropertyRange(1, 10)][Tooltip("Used in the attack state and defines the length of time the CurrentTarget has to be outside the Awareness Radius before returning to the default movement state.")]
        public float ActionFocus = 2f;

        // Behaviour variables
        [BoxGroup("Behaviour", centerLabel: true)]
        [GUIColor(0.3f, 0.8f, 0.8f, 1f)]
        [SerializeField]
        [Required]
        public NPCBehaviour Behaviour = NPCBehaviour.Aggressive;

        [BoxGroup("Behaviour", centerLabel: true)]
        [ShowInInspector, ReadOnly]
        private float actionRadius;

        [BoxGroup("Behaviour", centerLabel: true)]
        [Required]
        [SerializeField, PropertyRange(1, 5)]
        public float ActionSpeed = 2f;

        // - Aggressive
        [BoxGroup("Behaviour", centerLabel: true)]
        [Title("", "Aggressive", horizontalLine: false), PropertySpace(SpaceBefore = -15)]
        [ShowIf("Behaviour", NPCBehaviour.Aggressive)]
        [SerializeField]
        public float InvestigateAfter = 2f;

        // - Anxious
        [BoxGroup("Behaviour", centerLabel: true)]
        [Title("", "Anxious", horizontalLine: false), PropertySpace(SpaceBefore = -15, SpaceAfter = 0)]
        [ShowIf("Behaviour", NPCBehaviour.Anxious)]
        public bool RandomFlee = true;

        [BoxGroup("Behaviour", centerLabel: true)]
        [ShowIf("@this.RandomFlee && this.Behaviour == NPCBehaviour.Anxious"), PropertySpace(SpaceBefore = 0)]
        public float FleeDistance = 5f;

        [BoxGroup("Behaviour", centerLabel: true)]
        [HideIf("@this.RandomFlee || this.Behaviour != NPCBehaviour.Anxious"), PropertySpace(SpaceBefore = 0)]
        public Transform DefaultTarget;

        // Movement variables
        [BoxGroup("Movement", centerLabel: true)]
        [GUIColor(0.3f, 0.8f, 0.8f, 1f)]
        [SerializeField]
        [Required]
        public NPCMovement Movement = NPCMovement.Wander;

        [BoxGroup("Movement", centerLabel: true)]
        [Required]
        [SerializeField, PropertyRange(0.1f, 5), OnValueChanged("UpdateMovementSpeed")]
        public float MovementSpeed = 1f;

        // - Wandering
        [BoxGroup("Movement", centerLabel: true)]
        [Title("", "Wandering", horizontalLine: false), PropertySpace(SpaceBefore = -15)]
        [ShowIf("Movement", NPCMovement.Wander)]
        [SerializeField, PropertyRange(1, 10)]
        public float WanderRadius = 6f;

        [BoxGroup("Movement", centerLabel: true)]
        [ShowIf("Movement", NPCMovement.Wander)]
        [SerializeField, PropertyRange(1, 10)]
        public float WanderTimer = 2f;

        // - Patrolling
        [BoxGroup("Movement", centerLabel: true)]
        [Title("", "Patrolling", horizontalLine: false), PropertySpace(SpaceBefore = -15)]
        [ShowIf("Movement", NPCMovement.Patrol)]
        public Transform[] Waypoints;

        [BoxGroup("Movement", centerLabel: true)]
        [ShowIf("Movement", NPCMovement.Patrol)]
        [SerializeField, PropertyRange(0, 15)]
        public float WaitAtCheckPoint = 1f;

        #region Unity Editor methods to visualise variables
        private void UpdateMovementSpeed()
        {
            GetComponent<NavMeshAgent>().speed = MovementSpeed;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, WanderRadius);
        }
        #endregion
        #endregion

        #region Set and initialise states and types
        // - Default movement behaviour i.e no CurrentTarget / is unaware
        [HideInInspector] public Idle IdleState;
        [HideInInspector] public Wander WanderState;
        [HideInInspector] public Patrol PatrolState;
        // - Default awareness behaviour i.e. has CurrentTarget / is aware
        [HideInInspector] public Observe ObserveState;
        [HideInInspector] public Investigate InvestigateState; // NPCStateLock.Partial
        [HideInInspector] public Cooperate CooperateState;
        // - Default action behaviour i.e. has CurrentTarget / is taking action
        [HideInInspector] public Watch WatchState; // No NPCStateLock
        [HideInInspector] public Flee FleeState; // NPCStateLock.Full
        [HideInInspector] public Move MoveState; // NPCStateLock.Partial
        [HideInInspector] public Attack AttackState; // NPCStateLock.Full

        private void Awake()
        {
            IdleState = new Idle(this);
            WanderState = new Wander(this);
            PatrolState = new Patrol(this);
            WatchState = new Watch(this);
            ObserveState = new Observe(this);
            InvestigateState = new Investigate(this);
            CooperateState = new Cooperate(this);
            FleeState = new Flee(this);
            MoveState = new Move(this);
            AttackState = new Attack(this);

            faction = factionIdentity.GetFaction();
            type = AnimationController.GetNPCType();
            awarenessDelay = Random.Range(awarenessDelayMin, awarenessDelayMax);
        }
        #endregion

        protected override StateMachine.BState GetInitialState()
        {
            return IdleState;
        }

        public void LogSwitchStateWarning(object obj)
        {
            Helper.LogWarning("[NPC] Unknown state transition triggered - please investigate (" + obj.ToString() + ").");
        }

        #region Managing StateMachine wide methods to update direction/animation
        public void UpdateAnimationsAndRotation()
        {
            AnimationController.UpdateAnimationAndRotation(NavMeshAgent.velocity.x, NavMeshAgent.velocity.y);
        }

        public void FaceTarget()
        {
            AnimationController.FaceTarget(CurrentTarget);
        }

        public void SetAnimations(bool running, bool aware, bool speed = false, float speedValue = 0)
        {
            switch (type)
            {
                case NPCType.Humanoid:
                    AnimationController.SetFloat("moveSpeed", 0);
                    break;
                case NPCType.Dinosaurs:
                    AnimationController.SetBool("isRunning", running);
                    AnimationController.SetBool("isAware", aware);
                    if (speed) AnimationController.SetAnimatorSpeed(speedValue);
                    break;
                case NPCType.Unspecified:
                    break;
                default:
                    break;
            }
        }

        public void ChangeAnimationLayer(int layer, float weight)
        {
            switch (type)
            {
                case NPCType.Humanoid:
                    AnimationController.ChangeAnimationLayer(layer, weight);
                    break;
                case NPCType.Dinosaurs:
                    break;
                case NPCType.Unspecified:
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Managing StateMachine wide methods to control logic
        public void SetCurrentTarget(Transform target)
        {
            CurrentTarget = target;
            CurrentTargetTag = target.tag;
        }

        public void RemoveCurrentTarget()
        {
            CurrentTarget = null;
            CurrentTargetTag = null;
        }

        private bool StatusOfCurrentTarget()
        {
            if (CurrentTarget == null) return false;
            else return true;
        }

        public bool ObjectIsClose()
        {
            if (StatusOfCurrentTarget() == false) return false;

            if (Vector3.Distance(CurrentTarget.position, transform.position) <= actionDistance) return true;
            else return false;
        }

        public bool ObjectIsFar()
        {
            if (StatusOfCurrentTarget() == false) return true;

            // Recalculate collider center (required due to localScale -1f)
            float offset;
            if (AnimationController.IsFacingRight) offset = awarenessColliderCenter.x * -1f;
            else offset = awarenessColliderCenter.x;
            Vector3 colliderCenter = new Vector3(transform.position.x + offset, transform.position.y, transform.position.z);

            // Return true/false
            if (Vector3.Distance(colliderCenter, CurrentTarget.position) > (awarenessRadius + 0.5f)) return true;
            else return false;
        }

        public bool AgentHasReachedDestination()
        {
            // If a path is pending i.e. being calculated, the destination
            // has not beeen reached yet
            if (NavMeshAgent.pathPending) return false;

            // If the remaining distance is <= to the stopping distance, the
            // destination has been reached
            if (NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance) return true;
            else return false;
        }

        public void CheckForNewTarget()
        {
            if (searchLayers == 0)
            {
                Helper.Log("[NPC] Attempted to CheckForNewTarget() but there are no searchLayers specified.");
                return;
            }

            // Detect all colliders within awarenessRadius
            Collider[] colliders = Physics.OverlapSphere(transform.TransformPoint(awarenessColliderCenter), awarenessRadius, searchLayers);
            //Helper.Log("[NPC] Detected " + colliders.Length + " colliders in CheckForNewTargets().");

            // Guard clause and create variables
            if (colliders.Length == 0) return;
            float distance = Mathf.Infinity;
            List<SearchTargets> list = new List<SearchTargets>();

            // Calculate distance to this NPC and add to list if >0
            for (int i = 0; i < colliders.Length; i++)
            {
                distance = (colliders[i].transform.position - transform.position).sqrMagnitude;
                if (distance != 0f)
                {
                    SearchTargets target = new SearchTargets { collider = colliders[i], distance = distance };
                    list.Add(target);
                    //Helper.Log("[NPC] Added collider " + colliders[i] + " at distance " + distance + " to the list.");
                }
            }

            // Sort the list (closest object first)
            list.Sort((x, y) => x.distance.CompareTo(y.distance));

            // For trouble-shooting
            //Helper.Log("[NPC] Final, sorted list:");
            //foreach (var item in list) Helper.Log(" - " + item.collider + " / " + item.distance + ".");

            // Reduce StateLock level to be able to take immediate action
            StateLock = NPCStateLock.Partial;

            // Go through list, starting with closest object, and determine faction, then
            // use ActionTrigger or AwarenessTrigger accordingly
            // Note 1: The loop will be broken when ActionTrigger changes StateLock.Full
            // Note 2: This currently assumes some form of "rage" as hostile targets will
            // be attacked even if they are outside the actionRadius
            for (int i = 0; i < list.Count; i++)
            {
                if (StateLock == NPCStateLock.Full) return;
                Faction otherFaction = list[i].collider.GetComponent<FactionIdentity>().GetFaction();
                FactionStatus factionStatus = factionIdentity.GetMyFactionStatus(otherFaction);
                if (factionStatus == FactionStatus.Hostile)
                    ActionTrigger(list[i].collider.transform);
                else
                    AwarenessTrigger(true, list[i].collider.transform, factionStatus);
            }
        }

        private class SearchTargets
        {
            public Collider collider;
            public float distance;
        }
        #endregion

        #region Managing StateMachine wide methods to control movement
        public void SwitchToDefaultMovementState()
        {
            switch (Movement)
            {
                case NPCMovement.Idle:
                    SwitchState(IdleState);
                    break;
                case NPCMovement.Wander:
                    SwitchState(WanderState);
                    break;
                case NPCMovement.Patrol:
                    SwitchState(PatrolState);
                    break;
                default:
                    LogSwitchStateWarning(this);
                    break;
            }
        }

        public void MoveAwayFromObject(bool isFleeing)
        {
            Vector3 direction = (CurrentTarget.position - transform.position).normalized;
            if (isFleeing) NavMeshAgent.SetDestination(transform.TransformPoint(-direction * FleeDistance));
            else NavMeshAgent.SetDestination(transform.TransformPoint(-direction * actionRadius));

            // TO DO: Add logic to calculate is path is possible
            // and if not, go beyond player or check 90 degrees

            // For trouble-shooting
            //Debug.DrawLine(sm.transform.position, sm.transform.TransformPoint(direction * sm.ActionRadius), color: Color.red, 10f);
            //Debug.DrawLine(sm.transform.position, sm.transform.TransformPoint(-direction * sm.ActionRadius), color: Color.green, 10f);
        }

        public void MoveIfPushedAway()
        {
            if (NavMeshAgent.velocity.sqrMagnitude > 0)
            {
                UpdateAnimationsAndRotation();
                countdown = 0.5f;
            }
            else if (countdown > 0f) UpdateAnimationsAndRotation();
        }
        #endregion

        #region Managing external methods used outside the state machine
        public void Die()
        {
            // TO DO: This method should be updated to use an event instead

            AnimationController.ChangeAnimationLayer(0, 1, true);
            AnimationController.ResetBools();
            AnimationController.SetTrigger("isDying");
            NavMeshAgent.enabled = false;
            this.enabled = false;
        }
        #endregion

        #region Awareness trigger
        public async void AwarenessTrigger(bool detected, Transform target, FactionManager.FactionStatus factionStatus = FactionManager.FactionStatus.None)
        {
            if (StateLock == NPCStateLock.Full || IsCooperating || factionStatus != FactionManager.FactionStatus.Hostile) return;

            await Task.Delay(System.TimeSpan.FromSeconds(awarenessDelay));

            if (detected)
            {
                SetCurrentTarget(target);
                switch (Behaviour)
                {
                    case NPCBehaviour.Anxious:
                        SwitchState(ObserveState);
                        break;
                    case NPCBehaviour.Neutral:
                        SwitchState(ObserveState);
                        break;
                    case NPCBehaviour.Indifferent:
                        SwitchState(IdleState);
                        break;
                    case NPCBehaviour.Observant:
                        SwitchState(ObserveState);
                        break;
                    case NPCBehaviour.Defensive:
                        SwitchState(ObserveState);
                        break;
                    case NPCBehaviour.Aggressive:
                        SwitchState(InvestigateState);
                        break;
                    default:
                        LogSwitchStateWarning(this);
                        break;
                }
            }
            else if (detected == false)
            {
                RemoveCurrentTarget();
            }
        }

        public void InitialiseAwarenessTrigger(Vector3 center, float radius)
        {
            awarenessColliderCenter = center;
            awarenessRadius = radius;
        }
        #endregion

        #region Action trigger
        public void ActionTrigger(Transform target)
        {
            // Guard clause - Only take action if StateLock is Partial or Off
            if (StateLock == NPCStateLock.Full) return;

            SetCurrentTarget(target);
            TakeAction();
        }

        private void TakeAction()
        {
            switch (Behaviour)
            {
                case NPCBehaviour.Anxious:
                    SwitchState(FleeState);
                    break;
                case NPCBehaviour.Neutral:
                    SwitchState(MoveState);
                    break;
                case NPCBehaviour.Indifferent:
                    SwitchState(IdleState);
                    break;
                case NPCBehaviour.Observant:
                    SwitchState(WatchState);
                    break;
                case NPCBehaviour.Defensive:
                    SwitchState(AttackState);
                    break;
                case NPCBehaviour.Aggressive:
                    RequestCooperation();
                    SwitchState(AttackState);
                    break;
                default:
                    LogSwitchStateWarning(this);
                    break;
            }
        }

        public void TakeActionAfterReceivingDamage(Transform target)
        {
            // Guard clause - Only take action if StateLock is Partial or Off
            if (StateLock == NPCStateLock.Full) return;

            Helper.Log("[NPC] " + transform.name + ": Takes action after receiving damage from " + target.name + ".");

            SetCurrentTarget(target);
            switch (Behaviour)
            {
                case NPCBehaviour.Anxious:
                    SwitchState(FleeState);
                    break;
                case NPCBehaviour.Neutral:
                    SwitchState(FleeState);
                    break;
                case NPCBehaviour.Indifferent:
                    SwitchState(AttackState);
                    break;
                case NPCBehaviour.Observant:
                    SwitchState(FleeState);
                    break;
                case NPCBehaviour.Defensive:
                    SwitchState(AttackState);
                    break;
                case NPCBehaviour.Aggressive:
                    RequestCooperation();
                    SwitchState(AttackState);
                    break;
                default:
                    LogSwitchStateWarning(this);
                    break;
            }
        }

        public void InitialiseActionTrigger(float radius)
        {
            actionRadius = radius;
        }
        #endregion

        #region Cooperation trigger
        private void RequestCooperation()
        {
            EventManager.Instance.RequestCooperation(transform.position, CurrentTarget, faction);
        }

        public void CooperateWithOthers(Transform target)
        {
            if (StateLock != NPCStateLock.Off) return;
            IsCooperating = true;
            SetCurrentTarget(target);
            SwitchState(CooperateState);
            Helper.Log("[NPC] " + transform.name + " is cooperating.");
        }
        #endregion
    }
}
