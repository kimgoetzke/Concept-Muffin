using UnityEngine;

namespace CaptainHindsight
{
    public class ZRenderTextureZone : MonoBehaviour
    {
        [SerializeField] private Camera renderCamera = null;
        [SerializeField] private string playerParticleName;
        private Transform particleHolder;
        private Transform particleTransform;

        private void Start()
        {
            renderCamera.enabled = false;

            // Find player
            if (GameObject.Find("Player") == null) Debug.LogError("[ZRenderTextureZone] Player transform not found.");
            else particleHolder = GameObject.Find("Player").transform.Find("Particles").transform;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") == false) return;
            Helper.Log("[ZRenderTextureZone] Entered render texture zone " + transform.parent.name + ".");
            renderCamera.enabled = true;
            particleTransform = particleHolder.Find(playerParticleName);
            particleTransform.gameObject.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") == false) return;
            renderCamera.enabled = false;
            particleTransform.gameObject.SetActive(false);
            Helper.Log("[ZRenderTextureZone] Left render texture zone.");
        }

        private void OnValidate()
        {
            GetComponent<Collider>().isTrigger = true;
        }
    }
}
