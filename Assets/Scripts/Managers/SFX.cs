using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CaptainHindsight
{
    [System.Serializable]
    public class SFX
    {
        #region Defining variables
        [LabelText("Category")]
        [LabelWidth(100)]
        [OnValueChanged("SFXChange")]
        public SFXCategory sfxCategory = SFXCategory.Player;

        [LabelText("$sfxLabel")]
        [LabelWidth(100)]
        [ValueDropdown("Category", DropdownWidth = 400)]
        [OnValueChanged("SFXChange")]
        [InlineButton("Preview")]
        [InlineButton("Select")]
        public SFXClip sfxToPlay;

        private string sfxLabel = "SFX";
        #pragma warning disable
        [SerializeField]
        private bool showFile = false;

        [ShowIf("showFile")]
        [SerializeField]
        private bool editFile = false;
        #pragma warning enable

        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [ShowIf("showFile")]
        [EnableIf("editFile")]
        [SerializeField]
        private SFXClip sfxBase;
        #endregion

        #region Methods used in Unity Inspector
        private void SFXChange()
        {
            // Keep the label up to date
            sfxLabel = sfxCategory.ToString() + " SFX";

            // Keep the displayed "SFX clip" up to date
            sfxBase = sfxToPlay;
        }

        private void Select()
        {
            UnityEditor.Selection.activeObject = sfxToPlay;
        }

        private void Preview()
        {
            SFXManager.PlaySFX(sfxToPlay);
        }

        // Get list of SFX from AudioDirector, used in the Unity Inspector
        private List<SFXClip> Category()
        {
            List<SFXClip> sfxList;

            switch (sfxCategory)
            {
                case SFXCategory.Combat:
                    sfxList = SFXManager.Instance.combatSFX;
                    break;
                case SFXCategory.Effects:
                    sfxList = SFXManager.Instance.combatSFX;
                    break;
                case SFXCategory.NPC:
                    sfxList = SFXManager.Instance.npcSFX;
                    break;
                case SFXCategory.Player:
                    sfxList = SFXManager.Instance.playerSFX;
                    break;
                default:
                    sfxList = SFXManager.Instance.playerSFX;
                    break;
            }

            return sfxList;
        }
        #endregion

        public void Play(AudioSource audioSource, bool waitToFinish = false)
        {
            SFXManager.PlaySFX(sfxToPlay, audioSource, waitToFinish);
        }

        public enum SFXCategory
        {
            Combat,
            Effects,
            NPC,
            Player
        }
    }
}