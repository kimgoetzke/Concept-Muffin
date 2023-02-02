using UnityEngine;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace CaptainHindsight
{
    [DisallowMultipleComponent]
    public class EquipmentController : MonoBehaviour
    {
        [Title("General")]
        [Required, SerializeField] private Transform playerManager;
        [Required, SerializeField] private List<GameObject> equipmentList;
        private MouseController mouseController;
        private ObjectPoolManager objectPoolManager;
        private MCameraShake cameraShake;
        private List<EquipmentItem> equipmentLedger;

        [Title("Active equipment")]
        [Required][SerializeField][AssetSelector(Paths = "Assets/Sprites/Weapons/", ExpandAllMenuItems = true)] private List<Sprite> sprites;
        [Required][SerializeField] private SpriteRenderer spriteRenderer;
        [Required][SerializeField] private Transform firePoint;

        private Equipment activeEquipmentType = Equipment.None;
        [ShowInInspector, ReadOnly] private int activeEquipment;
        [ShowInInspector, ReadOnly] private int currentDirection;
        [ShowInInspector, ReadOnly] private int damage;
        [ShowInInspector, ReadOnly] private int projectileCount;
        //[ShowInInspector, ReadOnly] private float projectileSpread;

        [Title("Offsets")]
        [SerializeField] [Required] private List<Transform> leftHandTargetList;
        [SerializeField] [Required] private List<Transform> rightHandTargetList;

        [Title("Cooldown")]
        [ShowInInspector, ReadOnly] private float attackCooldown;
        public bool IsAttacking { get; private set; }
        private float attackCooldownTimer = Mathf.Infinity;

        [Header("Recoil")]
        private float recoilIntensity;
        private float recoilTime;

        #region Start & Update
        private void Start()
        {
            mouseController = MouseController.Instance;
            objectPoolManager = ObjectPoolManager.Instance;
            cameraShake = MCameraShake.Instance;
            equipmentLedger = ScriptableObjectsDirector.Instance.Equipment;

            // Deactivate aimTransform once initialisation is complete
            transform.parent.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (attackCooldownTimer < attackCooldown) 
                attackCooldownTimer += Time.deltaTime;
        }
        #endregion

        #region Managing direction change
        public void ChangeDirection(int direction, bool priority)
        {
            if (direction == currentDirection && priority == false) return;

            //Helper.Log("[EquipmentController] Equipment (" + transform.name + ") direction changed to: " + direction);

            currentDirection = direction;
            spriteRenderer.sprite = sprites[direction];

            // Offset firePoint and equipment itself (updown)
            if (direction == 0)
            {
                equipmentList[activeEquipment].transform.localPosition = new Vector3(equipmentList[activeEquipment].transform.localPosition.x, equipmentList[activeEquipment].transform.localPosition.y, 0.002f);
                firePoint.localPosition = new Vector3(firePoint.localPosition.x, 0f, firePoint.localPosition.z);
            }
            else if (direction == 1)
            {
                equipmentList[activeEquipment].transform.localPosition = new Vector3(equipmentList[activeEquipment].transform.localPosition.x, equipmentList[activeEquipment].transform.localPosition.y, -0.0001f);
                firePoint.localPosition = new Vector3(firePoint.localPosition.x, 0.005f, firePoint.localPosition.z);
            }
            else if (direction == 2)
            {
                equipmentList[activeEquipment].transform.localPosition = new Vector3(equipmentList[activeEquipment].transform.localPosition.x, equipmentList[activeEquipment].transform.localPosition.y, -0.002f);
                firePoint.localPosition = new Vector3(firePoint.localPosition.x, 0f, firePoint.localPosition.z);
            }
            else if (direction == 3)
            {
                equipmentList[activeEquipment].transform.localPosition = new Vector3(equipmentList[activeEquipment].transform.localPosition.x, equipmentList[activeEquipment].transform.localPosition.y, 0.0001f);
                firePoint.localPosition = new Vector3(firePoint.localPosition.x, -0.02f, firePoint.localPosition.z);
            }
        }
        #endregion

        #region Managing equipment change
        public void ChangeEquipment(int input)
        {
            // Map input to equipmentList and equipmentLedger
            int slot = input - 2;

            // Update active equipment
            activeEquipmentType = equipmentLedger[slot].Type;
            activeEquipment = slot;

            // Deactivate all GameObjects except for the selected equipment
            foreach (var item in equipmentList)
            {
                item.SetActive(false);
            }
            equipmentList[slot].SetActive(true);

            // Get references for active equipment
            spriteRenderer = equipmentList[slot].GetComponent<SpriteRenderer>();
            firePoint = equipmentList[slot].transform.Find("FirePoint");

            // Update offsets for each hand and direction
            leftHandTargetList[0].localPosition = equipmentLedger[slot].LHandTargetN;
            leftHandTargetList[1].localPosition = equipmentLedger[slot].LHandTargetW;
            leftHandTargetList[2].localPosition = equipmentLedger[slot].LHandTargetS;
            leftHandTargetList[3].localPosition = equipmentLedger[slot].LHandTargetE;
            rightHandTargetList[0].localPosition = equipmentLedger[slot].RHandTargetN;
            rightHandTargetList[1].localPosition = equipmentLedger[slot].RHandTargetW;
            rightHandTargetList[2].localPosition = equipmentLedger[slot].RHandTargetS;
            rightHandTargetList[3].localPosition = equipmentLedger[slot].RHandTargetE;

            // Update sprites to current equipment
            sprites.Clear();
            sprites.Add(equipmentLedger[slot].Sprites.North);
            sprites.Add(equipmentLedger[slot].Sprites.West);
            sprites.Add(equipmentLedger[slot].Sprites.South);
            sprites.Add(equipmentLedger[slot].Sprites.East);

            // Run ActionDirectionChange to update sprite and offset sprite transform
            ChangeDirection(currentDirection, true);

            // Update recoil
            recoilIntensity = equipmentLedger[slot].RecoilIntensity;
            recoilTime = equipmentLedger[slot].RecoilTime;

            // Update other equipment stats
            damage = equipmentLedger[slot].DamageBase;
            projectileCount = equipmentLedger[slot].ProjectileCount;
            attackCooldown = equipmentLedger[slot].AttackCooldown;

            // Update cursor
            EventManager.Instance.ChangeCursor(equipmentLedger[slot].Cursor);

            // For trouble-shooting
            //Helper.Log("[EquipmentController] Slot: " + slot + " | " + activeEquipmentType + ".");
        }
        #endregion

        #region Managing attack
        public void Attack(Vector3 playerPosition)
        {
            if (attackCooldownTimer >= attackCooldown)
            {
                // TO DO: Change script to allow for different weapon types i.e. changing
                // audio, types of projectiles, and more...

                // For troubleshooting
                //Helper.Log("[EquipmentController] Attack received!");
                //Debug.DrawLine(firePoint.position, mouseController.MPRaw, Color.white, .1f);

                //Initiate attack
                attackCooldownTimer = 0;
                IsAttacking = true;

                // Shake camera
                cameraShake.ShakeCamera(recoilIntensity, recoilTime);

                // Instantiate muzzle flash
                GameObject muzzleFlashObject = objectPoolManager.SpawnFromPool("muzzleFlash", firePoint.transform.position, firePoint.transform.rotation);
                muzzleFlashObject.transform.parent = firePoint.transform;
                DeactivateMuzzleFlash(muzzleFlashObject);

                // Play shooting sound
                int randomSound = Random.Range(0, 3);
                AudioDirector.Instance.Play("Handgun_shot" + (randomSound + 1));

                // Instantiate bullet
                Vector3 modifiedFirePoint = new Vector3(firePoint.position.x, Mathf.Clamp(firePoint.position.y, playerPosition.y + 0.4f, playerPosition.y + 1f), firePoint.position.z); // Ensures collision with player collider are impossible
                GameObject bulletObject = objectPoolManager.SpawnFromPool("ammo-Bullet", modifiedFirePoint, firePoint.transform.rotation);
                Vector3 direction = mouseController.MPRaw - modifiedFirePoint;
                Vector3 normalisedDirection = direction.normalized;

                // Enable bullet trail on instantiated bullet if shooting far enough away from player
                if (direction.magnitude >= 1.3f) 
                    bulletObject.GetComponent<Bullet>().Initialise(playerManager, normalisedDirection, true, damage);
                else 
                    bulletObject.GetComponent<Bullet>().Initialise(playerManager, normalisedDirection, false, damage);

                // Instantiate additional bullets (e.g. shotgun)
                if (projectileCount > 1)
                {
                    for (int i = 1; i < projectileCount; i++)
                    {
                        bulletObject = objectPoolManager.SpawnFromPool("ammo-Bullet", modifiedFirePoint, firePoint.transform.rotation);

                        // Enable bullet trail on instantiated bullet if shooting far enough away from player
                        if (direction.magnitude >= 1.3f) 
                            bulletObject.GetComponent<Bullet>().Initialise(playerManager, normalisedDirection, true, damage);
                        else 
                            bulletObject.GetComponent<Bullet>().Initialise(playerManager, normalisedDirection, false, damage);
                    }
                }
            }
        }

        private async void DeactivateMuzzleFlash(GameObject muzzleFlash)
        {
            //await Task.Delay(System.TimeSpan.FromSeconds(1f));
            await Task.Delay(System.TimeSpan.FromSeconds(0.08f));
            muzzleFlash.SetActive(false);
        }
        #endregion
    }
}
