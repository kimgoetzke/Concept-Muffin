using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight
{
    public class MFollowObject : MonoBehaviour
    {
        [Title("Configuration")]
        [InfoBox("This script must be placed on the base hierarchy of the prefab object. It will use the transform that is parent to it as the object to follow. As a result, it should be placed a GameObject that functions as a UI position marker for this object.")]
        [InfoBox("Use canvasLayer to determine this objects priority on the WorldCanvas. The highest priority (being drawn on top of everything else) is 1.")]

        [SerializeField, ValueDropdown("Layers")]
        private string canvasLayer = "ThirdLayer";
        private static string[] Layers = new string[] { "FirstLayer", "SecondLayer", "ThirdLayer" };

        [HideInInspector] 
        private Transform worldCanvas;

        [HideInInspector] 
        private Transform uiPositionMarker;

        private void Start()
        {
            // Find WorldCanvas
            if (GameObject.Find("UI/WorldCanvas/" + canvasLayer) == null)
                Debug.LogError("[MFollowObject] UI canvas not found.");
            else worldCanvas = GameObject.Find("UI/WorldCanvas/" + canvasLayer).transform;

            //// Set position marker as parent
            uiPositionMarker = transform.parent;

            // Change parent to WorldCanvas
            this.transform.SetParent(worldCanvas.transform);
        }

        private void Update()
        {
            transform.position = uiPositionMarker.position;
        }
    }
}
