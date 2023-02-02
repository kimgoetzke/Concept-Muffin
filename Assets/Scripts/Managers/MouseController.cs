using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CaptainHindsight
{
    public class MouseController : MonoBehaviour
    {
        public static MouseController Instance;

        [SerializeField] private LayerMask groundLayer;
        private Transform player;
        private Camera mainCamera;
        private Ray mouseRay;
        [ShowInInspector] public Vector3 MPRaw { get; private set; } = new Vector3();
        public Vector3 MPGroundLevel = new Vector3();
        public Vector3 MPChestHeight = new Vector3();
        public float ChestHeightOffset { get; private set; } = 0.5f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void FixedUpdate()
        {
            mouseRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Physics.Raycast(mouseRay, out RaycastHit mouseRayHit, 20f, groundLayer);

            // Raw mouse position
            MPRaw = mouseRayHit.point;

            // Modified, setting Y to ground level
            MPGroundLevel = MPRaw;
            MPGroundLevel.y = player.transform.position.y;

            // Modified, setting Y to player chest height
            MPChestHeight = MPRaw;
            MPChestHeight.y = player.transform.position.y + 0.5f;
        }

        private void OnEnable()
        {
            // Find main camera
            if (GameObject.Find("MainCam") == null) Debug.LogError("[MCameraTarget] Main camera not found.");
            else mainCamera = GameObject.Find("MainCam").GetComponent<Camera>();

            // Find player
            if (GameObject.Find("Player") == null) Debug.LogError("[MCameraTarget] Player transform not found.");
            else player = GameObject.Find("Player").transform;
        }
    }
}
