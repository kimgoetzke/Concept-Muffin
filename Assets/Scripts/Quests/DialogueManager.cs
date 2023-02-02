using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

namespace CaptainHindsight
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance;

        [Title("Configuration")]
        [SerializeField, Required]
        private TextAsset globalVariablesJSON;

        [ShowInInspector, ReadOnly]
        private List<Stories> listOfStories = new List<Stories>();
        private Stories currentActiveStory;

        [Title("For trouble-shooting...")]
        [ShowInInspector, ReadOnly]
        private DialogueCanvasController dialogueCanvasController;

        public Story CurrentStory { get; private set; }

        public bool inProgress;
        private bool canContinue = true;
        private DialogueVariables dialogueVariables;

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

            // Find DialogueUI, the canvas for all dialogue-related UI
            if (GameObject.Find("UI/DialogueUI").GetComponent<DialogueCanvasController>() == null)
                Debug.LogError("[QuestGiver] Dialogue UI canvas not found.");
            dialogueCanvasController = GameObject.Find("UI/DialogueUI").GetComponent<DialogueCanvasController>();

            // Get instance of dialogue variables class
            dialogueVariables = new DialogueVariables(globalVariablesJSON);
            EventManager.Instance.ShareDialogueVariables(dialogueVariables);
        }

        #region Managing dialogue progression
        public void TriggerDialogue(TextAsset inkJSON, string speaker)
        {
            EventManager.Instance.ChangeDialogueState(true);
            if (inProgress == false)
            {
                inProgress = true;
                StartDialogue(inkJSON, speaker);
            }
            else ContinueDialogue();
        }

        private void StartDialogue(TextAsset inkJSON, string speaker)
        {
            // Set currentStory based on input
            CurrentStory = new Story(inkJSON.text);
            if (OnCreateStory != null) OnCreateStory(CurrentStory);

            // Add story to list of active stories
            Stories thisNewStory = new Stories { Speaker = speaker, Story = CurrentStory, InkJSON = inkJSON };
            currentActiveStory = thisNewStory;
            listOfStories.Add(thisNewStory);

            // Update globalVariables in currentStory and subscribe to changes going forward
            dialogueVariables.StartListening(CurrentStory);

            // Set DialogueCanvas active and set speaker
            dialogueCanvasController.SetCanvasActive(true);
            dialogueCanvasController.Icon.enabled = true;
            dialogueCanvasController.SpeakerMesh.text = speaker;

            ContinueDialogue();
        }

        private void ContinueDialogue()
        {
            if (dialogueCanvasController.GetCanvasStatus() == false)
            {
                dialogueCanvasController.SetCanvasActive(true);
                return;
            }

            if (CurrentStory.canContinue)
            {
                dialogueCanvasController.TextMesh.text = CurrentStory.Continue();
                DisplayChoicesOrIcon();
            }
            else EndDialogue();
        }

        private void DisplayChoicesOrIcon()
        {
            List<Choice> currentChoices = CurrentStory.currentChoices;

            if (currentChoices.Count == 0)
            {
                dialogueCanvasController.Icon.enabled = true;
                dialogueCanvasController.SetChoicesGroupActive(false);
            }
            else
            {
                dialogueCanvasController.Icon.enabled = false;
                dialogueCanvasController.SetChoicesGroupActive(true);
                dialogueCanvasController.DisplayChoices(currentChoices);
            }
        }

        private void MakeChoice(int choiceIndex)
        {
            //Helper.Log("[DialogueManager] Received choice " + choiceIndex + ".");
            if (canContinue)
            {
                CurrentStory.ChooseChoiceIndex(choiceIndex);
                dialogueCanvasController.SetChoicesGroupActive(false);
                ContinueDialogue();
            }
        }

        private void PauseDialogue(string storyName)
        {
            Helper.Log("[DialogueManager] Dialogue paused. Canvas deactivated.");
            EventManager.Instance.ChangeDialogueState(false);
            dialogueCanvasController.SetCanvasActive(false);
            inProgress = false;
        }

        private void EndDialogue()
        {
            inProgress = false;
            EventManager.Instance.ChangeDialogueState(false);
            dialogueVariables.StopListening(CurrentStory);
            dialogueCanvasController.SetCanvasActive(false);
            dialogueCanvasController.Icon.enabled = true;
            dialogueCanvasController.SpeakerMesh.text = "Name";
            dialogueCanvasController.TextMesh.text = "...";
            currentActiveStory.CanContinue = false;
            CurrentStory = null;
        }
        #endregion

        #region Managing global story variables
        // Used by InlineButton in Unity Inspector
        [Button("Print Dialogue Variable Values", ButtonSizes.Large), PropertyOrder(0)]
        public void PrintVariableValues()
        {
            if (Application.isPlaying == false)
            {
                Helper.LogWarning("[DialogueManager] This function only works in Play mode.");
                return;
            }
            else if (dialogueVariables == null)
            {
                Helper.LogWarning("[DialogueManager] There are no dialogue variables. This is unexpected. Please investigate why this has happened.");
                return;
            }

            Helper.Log("[DialogueManager] List of current global Ink variables (public version):");
            foreach (KeyValuePair<string, Ink.Runtime.Object> kvp in dialogueVariables.Variables)
                Helper.Log(" - " + kvp.Key + " = " + kvp.Value);
        }

        // No longer used, should possibly be deleted
        public Ink.Runtime.Object GetVariable(string variableName)
        {
            Ink.Runtime.Object variableValue = null;
            dialogueVariables.Variables.TryGetValue(variableName, out variableValue);
            if (variableValue == null)
            {
                Debug.LogWarning("[DialogueManager] Ink Variable was found to be null: " + variableName);
            }
            return variableValue;
        }
        #endregion

        #region Managing active stories master list
        public class Stories
        {
            public string Speaker;
            public TextAsset InkJSON;
            public Story Story;
            public bool CanContinue;
        }

        //private bool StoryIsActive(string speaker, TextAsset inkJSON)
        //{
        //    bool state = false;
        //    for (int i = 0; i < listOfStories.Count; i++)
        //    {
        //        if (listOfStories[i].Speaker == speaker)
        //        {
        //            Helper.Log("[DialogueManager] Found speaker for " + speaker + " in listOfStories.");
        //            //if (listOfStories[i].CanContinue == false)
        //            //{
        //            //    Helper.Log("[DialogueManager] CanContinue = false, so resetting story and restarting.");

        //            //    //CurrentStory.ResetState();

        //            //    // Update globalVariables in currentStory and subscribe to changes going forward
        //            //    dialogueVariables.StartListening(CurrentStory);

        //            //    // Set DialogueCanvas active and set speaker
        //            //    dialogueCanvasController.SetCanvasActive(true);
        //            //    dialogueCanvasController.Icon.enabled = true;
        //            //    dialogueCanvasController.SpeakerMesh.text = speaker;

        //            //    ContinueDialogue();
        //            //}
        //            state = true;
        //        }
        //    }
        //    return state;
        //}
        #endregion

        #region Managing events
        public static event Action<Story> OnCreateStory;

        private void OnEnable()
        {
            EventManager.Instance.OnDialogueTrigger += TriggerDialogue;
            EventManager.Instance.OnInteractionTriggerRadiusExit += PauseDialogue;
            EventManager.Instance.OnDialogueChoiceSubmission += MakeChoice;
        }

        private void OnDestroy()
        {
            EventManager.Instance.OnDialogueTrigger -= TriggerDialogue;
            EventManager.Instance.OnInteractionTriggerRadiusExit -= PauseDialogue;
            EventManager.Instance.OnDialogueChoiceSubmission -= MakeChoice;
        }
        #endregion
    }
}
