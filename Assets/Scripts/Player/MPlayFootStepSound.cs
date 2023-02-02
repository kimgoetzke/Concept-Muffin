using UnityEngine;

namespace CaptainHindsight
{
    public class MPlayFootStepSound : MonoBehaviour
    {
        [SerializeField]
        private SFX footstepGrass;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = AudioDirector.Instance.AddAudioSourceForSFX(gameObject, footstepGrass);
        }

        // Used as animator event only
        public void PlayFootstepSound()
        {
            footstepGrass.Play(audioSource);
        }
    }
}
