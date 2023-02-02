using UnityEngine;

namespace CaptainHindsight
{
    public class PlayerOcclusionController : MonoBehaviour
    {
        [SerializeField] [Range(0, 1f)] float objectVisibility;

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.CompareTag("Occluder"))
            {
                //Helper.Log("[PlayerOcclusionController] Player collided with " + collider.name + ".");
                collider.GetComponent<MOccludee>().StopOccluding(true, objectVisibility);
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.CompareTag("Occluder"))
            {
                //Helper.Log("[PlayerOcclusionController] Player collision with " + collider.name + " ended.");
                collider.GetComponent<MOccludee>().StopOccluding(false, 1f);
            }
        }
    }
}
