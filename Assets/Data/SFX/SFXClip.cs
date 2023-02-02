using UnityEngine;
using Sirenix.OdinInspector;

namespace CaptainHindsight
{
    [CreateAssetMenu(menuName = "Scriptable Object/New SFX clip", fileName = "SFX-")]
    public class SFXClip : ScriptableObject
    {
        [Space]
        [Title("File")]
        [Required, OnValueChanged("SetName")]
        public AudioClip Clip;

        [ReadOnly]
        public string Label;

        [Title("Settings")]
        [Range(0f, 1f)]
        public float Volume = 0.95f;

        [Range(0f, 0.2f)]
        public float VolumeVariation = 0.05f;

        [Range(0f, 2f)]
        public float Pitch = 1f;

        [Range(0f, 0.2f)]
        public float PitchVariation = 0.05f;

        private void SetName()
        {
            Label = Clip.name;
        }
    }
}