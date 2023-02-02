using Sirenix.OdinInspector;
using UnityEngine;
using static CaptainHindsight.Quest;

namespace CaptainHindsight
{
    public class QuestGiver : QuestBehaviour, IInteractable
    {
        [Title("Configuration")]
        [SerializeField, Required]
        private Transform indicatorTransform;

        [SerializeField, PropertyRange(1, 10), OnValueChanged("UpdateInteractionRange")]
        private float interactionRange = 1;

        [SerializeField, Required]
        private string speaker;

        private MButtonInGame inGameUI;

        [Title("Quest")]
        [SerializeField, Required, InlineEditor(InlineEditorObjectFieldModes.Foldout, Expanded = false)]
        private Quest quest;

        #region Unity Editor methods (Odin and OnDrawGizmos)
        private void UpdateInteractionRange()
        {
            GetComponent<SphereCollider>().radius = interactionRange;
        }
        #endregion

        #region Awake, Start, and initialisation
        private void Awake()
        {
            // Get MButtonInGame on indicatorTransform to control interactionRange
            inGameUI = indicatorTransform.GetComponentInChildren<MButtonInGame>();
            inGameUI.InitialiseButton(interactionRange, quest.Story.name);
        }

        private void Start()
        {
            // Check if QuestManager is aware of the quest and is available
            CheckIfQuestIsAvailable();
        }

        private void CheckIfQuestIsAvailable()
        {
            if (quest.InActiveScene == false)
            {
                indicatorTransform.gameObject.SetActive(false);
                Helper.LogWarning("[QuestGiver] Interaction with this QuestGiver was disabled because the quest does not exist in the QuestManager and can therefore not be handled correctly.");
                quest = null;
                this.enabled = false;
                return;
            }

            // TO DO:
            // - None of the below should exist
            // - Ink Story should cater for all variations
            // - QuestManager/DialogueManager should work together to update DialogueVariables

            if (quest.State == QuestState.Available 
                || quest.State == QuestState.Ready)
                indicatorTransform.gameObject.SetActive(true);
            else
                indicatorTransform.gameObject.SetActive(false);
        }
        #endregion

        public void Interact(Vector3 position)
        {
            // Check interaction range (player's OverlapSphere is very larger to allow for
            // all sorts of interactions, not only with objects right in front of the player
            float distance = (position - transform.position).magnitude;
            if (distance >= (interactionRange + 0.25f)) return;

            // Update button
            inGameUI.Interact();

            // Initiate/progress/end the dialogue
            EventManager.Instance.TriggerDialogue(quest.Story, speaker);
        }

        protected override void ActionQuestUpdateEvent()
        {
            CheckIfQuestIsAvailable();
            Helper.Log("[QuestGiver] Checked if quest is now/still available.");
        }
    }
}
