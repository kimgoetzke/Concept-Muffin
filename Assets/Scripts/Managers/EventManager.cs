using Ink.Runtime;
using System;
using UnityEngine;

namespace CaptainHindsight
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance;

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

        #region Managing player & player input-related events
        public event Action<int> OnEquipmentDirectionChange;
        public event Action<Equipment, Vector3> OnAttack;
        public event Action OnPauseMenuRequest;

        // Used by PlayerController when relevant input is detected
        public void ChangeEquipmentDirection(int direction)
        {
            OnEquipmentDirectionChange?.Invoke(direction);
        }

        // Used by PlayerController when relevant input is detected (e.g. Escape key pressed)
        public void RequestPauseMenu()
        {
            OnPauseMenuRequest?.Invoke();
        }

        // Was used by EquipmentController to action attack, can possibly be deleted now
        public void Attack(Equipment equipment, Vector3 playerPosition)
        {
            OnAttack?.Invoke(equipment, playerPosition);
        }
        #endregion

        #region Managing general events
        public event Action<int> OnCursorChange;
        public event Action<Vector3, Transform, FactionManager.Faction> OnCooperationRequest;
        public event Action OnFactionsChange;

        // Used by MouseController to change cursors
        public void ChangeCursor(int cursor)
        {
            OnCursorChange?.Invoke(cursor);
        }

        // Used by MCooperation to process cooperation requests
        public void RequestCooperation(Vector3 position, Transform target, FactionManager.Faction faction)
        {
            OnCooperationRequest?.Invoke(position, target, faction);
        }

        // Used by various classes to request updated FactionGroups from FactionManager
        public void ChangeFactions()
        {
            OnFactionsChange?.Invoke();
        }
        #endregion

        #region Managing dialogue
        public event Action<TextAsset, string> OnDialogueTrigger;
        public event Action<bool> OnDialogueStateChange;
        public event Action<string> OnInteractionTriggerRadiusExit;
        public event Action<int> OnDialogueChoiceSubmission;
        public event Action<DialogueVariables> OnDialogueVariablesShare;


        // Used by QuestGiver to initiate/continue/stop dialogue
        public void TriggerDialogue(TextAsset inkJSON, string speaker)
        {
            OnDialogueTrigger?.Invoke(inkJSON, speaker);
        }

        // Used to get PlayerController to react on dialogue state by e.g. disable shooting
        public void ChangeDialogueState(bool status)
        {
            OnDialogueStateChange?.Invoke(status);
        }

        // Used by buttons on the dialogueCanvas (e.g. the Dialogue-Choice-Button prefab
        // to inform DialogueManager about a dialogue choice that has been made
        public void SubmitDialogueChoice(int choice)
        {
            OnDialogueChoiceSubmission?.Invoke(choice);
        }

        // Used by MButtonInGame to notify DialogueManager that dialogue has to be paused
        // because player left the interactionRadius so the button is no longer visible
        // and the player can no longer interact with the button/object/NPC - it is also
        // used to notify the QuestManager _which_ story the event relates to, so that
        // all variables in the related quest can be updated
        public void InteractionTriggerRadiusExited(string nameOfStory)
        {
            OnInteractionTriggerRadiusExit?.Invoke(nameOfStory);
        }

        // Used to share global Ink dialogue variables between QuestManager and
        // DialogueManager which is required to track QuestState, progress, and more...
        public void ShareDialogueVariables(DialogueVariables variables)
        {
            OnDialogueVariablesShare?.Invoke(variables);
        }
        #endregion
    }
}