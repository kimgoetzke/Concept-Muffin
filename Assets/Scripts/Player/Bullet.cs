using UnityEngine;
using System.Threading.Tasks;
using Sirenix.OdinInspector;

namespace CaptainHindsight
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float bulletSpeed = 1f;
        [ShowInInlineEditors] private int bulletDamage;
        [SerializeField] private float deactivateAfter = 2f;
        private Rigidbody rb;
        private TrailRenderer trailRenderer;
        public Transform Owner;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            trailRenderer = GetComponent<TrailRenderer>();
        }

        public void Initialise(Transform bulletOwner, Vector3 shootingDirection, bool trailOn, int damage)
        {
            Owner = bulletOwner;
            bulletDamage = damage;
            rb.AddForce(shootingDirection * bulletSpeed);
            if (trailOn == false) trailRenderer.enabled = false;
            StartTimerToDeactivate();
        }

        private void OnCollisionEnter(Collision trigger)
        {

            if (trigger.transform.CompareTag("Bullet"))
            {
                //Helper.Log("[Bullet] Collision protection: Collision with " + trigger.transform.name + " ignored.");
                return;
            }

            //Helper.Log("[Bullet] Collision with " + trigger.transform.name + ".");

            if (trigger.transform.CompareTag("NPC") || trigger.transform.CompareTag("Damageable") || trigger.transform.CompareTag("Player"))
                trigger.transform.GetComponent<IDamageable>().TakeDamage(bulletDamage, Owner);

            ObjectPoolManager.Instance.SpawnFromPool("ammo-BulletParticles", transform.position, Quaternion.identity);
            ResetBullet();
        }

        private async void StartTimerToDeactivate()
        {
            try
            {
                await Task.Delay(System.TimeSpan.FromSeconds(deactivateAfter));
                ResetBullet();
            }
            catch
            {
                //Helper.Log("[Bullet] Deactivation cancelled. Object no longer exists.");
            }
        }

        private void ResetBullet()
        {
            trailRenderer.enabled = true;
            gameObject.SetActive(false);
            gameObject.transform.position = new Vector3(0f, 0f, 0f);
            gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            rb.velocity = new Vector3(0f, 0f, 0f);
        }
    }
}