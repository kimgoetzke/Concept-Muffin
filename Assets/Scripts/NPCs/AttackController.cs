using UnityEngine;

namespace CaptainHindsight
{
    public class AttackController : MonoBehaviour
    {
        [SerializeField, Min(1)] private int damage = 1;

        [SerializeField] private Transform attackOrigin;
        [SerializeField, Min(0.1f)] private float attackRange = 0.35f;
        [SerializeField] private LayerMask attackLayer;
        [SerializeField] private Transform healthManager;

        // Triggered by animation event only
        public void Attack()
        {
            Collider[] colliders = Physics.OverlapSphere(attackOrigin.position, attackRange, attackLayer);

            for (int i = 0; i < colliders.Length; i++)
            {
                float distance = (colliders[i].transform.position - transform.position).sqrMagnitude;
                //Helper.Log("[AttackManager] " + transform.parent.parent.name + ": Distance to " + colliders[i].transform.name + " = " + distance + ".");
                if (distance > 0) colliders[i].GetComponent<IDamageable>().TakeDamage(damage, healthManager);
            }
        }

        public void Die()
        {
            gameObject.GetComponent<Animator>().enabled = false;
        }
    }
}
