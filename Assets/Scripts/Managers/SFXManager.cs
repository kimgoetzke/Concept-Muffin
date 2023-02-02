using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace CaptainHindsight
{
    public class SFXManager : MonoBehaviour
    {
        private static SFXManager instance;
        public static SFXManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<SFXManager>();

                return instance;
            }
        }

        #region Defining variables
        [Title("Configuration")]
        [SerializeField] private AudioMixer audioMixer;
        public AudioMixerGroup audioMixerGroup;

        [TitleGroup("SFX")]
        [TabGroup("SFX/SFX", "Combat")]
        [AssetList(Path = "/Data/SFX/Combat", AutoPopulate = true), ListDrawerSettings(Expanded = true)]
        public List<SFXClip> combatSFX;

        [TabGroup("SFX/SFX", "Effects")]
        [AssetList(Path = "/Data/SFX/Combat", AutoPopulate = true), ListDrawerSettings(Expanded = true)]
        public List<SFXClip> effectSFX;

        [TabGroup("SFX/SFX", "NPCs")]
        [HideLabel, LabelText("NPC SFX")]
        [AssetList(Path = "/Data/SFX/NPCs", AutoPopulate = true), ListDrawerSettings(Expanded = true)]
        public List<SFXClip> npcSFX;

        [TabGroup("SFX/SFX", "Player")]
        [AssetList(Path = "/Data/SFX/Player", AutoPopulate = true), ListDrawerSettings(Expanded = true)]
        public List<SFXClip> playerSFX;

        [HorizontalGroup("SFX/AudioSource")]
        [SerializeField]
        private AudioSource defaultAudioSource;

        [HorizontalGroup("SFX/AudioSource")]
        [ShowIf("@defaultAudioSource == null")]
        [GUIColor(1f, 0.5f, 0.5f, 1f)]
        [Button]
        private void AddAudioSource()
        {
            defaultAudioSource = this.gameObject.GetComponent<AudioSource>();

            if (defaultAudioSource == null)
            {
                defaultAudioSource = this.gameObject.AddComponent<AudioSource>();
                defaultAudioSource.outputAudioMixerGroup = audioMixerGroup;
            }
        }
        #endregion

        #region Different play methods for other scripts to call
        public static void PlaySFX(SFXClip sfx, AudioSource audioSource = null, bool waitToFinish = false)
        {
            if (audioSource == null) audioSource = SFXManager.instance.defaultAudioSource;

            if (audioSource.isPlaying == false || waitToFinish == false)
            {
                audioSource.clip = sfx.Clip;
                audioSource.volume = sfx.Volume + Random.Range(-sfx.VolumeVariation, sfx.VolumeVariation);
                audioSource.pitch = sfx.Pitch + Random.Range(-sfx.PitchVariation, sfx.PitchVariation);
                audioSource.Play();
            }
        }
        #endregion
    }
}