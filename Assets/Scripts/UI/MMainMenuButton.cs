using UnityEngine;

namespace CaptainHindsight
{
    public class MMainMenuButton : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            transform.gameObject.tag = "Untagged";
            AudioDirector.Instance.Play("Thud");
        }
    }
}
