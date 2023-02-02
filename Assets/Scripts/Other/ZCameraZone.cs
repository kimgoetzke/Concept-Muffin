using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

namespace CaptainHindsight
{
    [RequireComponent(typeof(Collider))]
    public class ZCameraZone : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera virtualCamera = null;
        [SerializeField] private DepthOfField depthOfField;
        [SerializeField] private Volume volume;
        private float initialFocusDistance;


        private void Start()
        {
            virtualCamera.enabled = false;
            volume.profile.TryGet<DepthOfField>(out depthOfField);
            initialFocusDistance = depthOfField.focusDistance.value;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Helper.Log("[ZCameraZone] Entered camera zone.");
                virtualCamera.Priority = 10;
                virtualCamera.enabled = true;
                DOTween.Sequence().Append(DOTween.To(() => depthOfField.focusDistance.value, x => depthOfField.focusDistance.value = x, 12f, 3f));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                virtualCamera.enabled = false;
                virtualCamera.Priority = 9;
                DOTween.Sequence().Append(DOTween.To(() => depthOfField.focusDistance.value, x => depthOfField.focusDistance.value = x, initialFocusDistance, 3f));
            }
        }

        private void OnValidate()
        {
            GetComponent<Collider>().isTrigger = true;
        }
    }
}
