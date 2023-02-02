using UnityEngine;
using Cinemachine;

namespace CaptainHindsight
{
    public class MCameraTarget : MonoBehaviour
    {
        [SerializeField] private Transform cmTargetGroup;
        private MouseController mouseController;
        private Transform player;
        private CinemachineVirtualCamera cinemachineCamera;

        private void Start() => mouseController = MouseController.Instance;

        private void Update()
        {
            transform.position = mouseController.MPGroundLevel;
        }

        private void OnDisable()
        {
            try
            {
                cinemachineCamera.Follow = player.transform;
            }
            catch
            {
                Helper.Log("[MCameraTarget] Cinemachine or player no longer exist. Cinemachine target not updated.");
            }
        }

        private void OnEnable()
        {
            // Find player
            if (GameObject.Find("Player") == null) Debug.LogError("[MCameraTarget] Player transform not found.");
            else player = GameObject.Find("Player").transform;

            // Find main Cinemachine camera
            if (GameObject.Find("Cinemachine") == null) Debug.LogError("[MCameraTarget] Cinemachine not found.");
            else cinemachineCamera = GameObject.Find("Cinemachine").GetComponent<CinemachineVirtualCamera>();

            // Update main Cinemachine camera to use target group
            cinemachineCamera.Follow = cmTargetGroup;
        }
    }
}
