using UnityEngine;

namespace CaptainHindsight
{
    public interface IDamageable
    {
        public void TakeDamage(int damage, Transform origin = null);
    }
}