using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight
{
    public class MOccludee : MonoBehaviour
    {
        [SerializeField] private Material transparencyMaterial;
        [SerializeField] private bool overrideTransparency;
        [SerializeField, ShowIf("overrideTransparency"), PropertyRange(0f, 1f)] private float overrideValue;
        private Material defaultMaterial;
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            defaultMaterial = spriteRenderer.material;
        }

        public void StopOccluding(bool status, float transparency)
        {
            if (status == false)
            {

                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                spriteRenderer.material = defaultMaterial;
            }
            else
            {
                if (overrideTransparency) transparency = overrideValue;
                spriteRenderer.material = transparencyMaterial;
                spriteRenderer.color = new Color(1f, 1f, 1f, transparency);
            }
        }
    }
}
