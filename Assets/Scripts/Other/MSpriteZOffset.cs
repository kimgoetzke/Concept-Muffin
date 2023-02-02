using UnityEngine;

namespace CaptainHindsight
{
    public class MSpriteZOffset : MonoBehaviour
    {

        private SpriteRenderer[] renderers;

        private void Start()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var item in renderers)
            {
                float offset = (item.sortingOrder / 100000f);
                float newZPosition = item.transform.localPosition.z + offset;
                Helper.Log("(" + item.transform.parent.name + ") " + item.transform.name + ", order: " + item.sortingOrder + ", Z: " + item.transform.localPosition.z + " - Offset by " + offset + " to " + newZPosition + ".");
                item.transform.position = new Vector3(item.transform.position.x, item.transform.position.y, newZPosition);
            }
        }
    }
}
