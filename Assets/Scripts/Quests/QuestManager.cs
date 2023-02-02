using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static CaptainHindsight.Quest;

namespace CaptainHindsight
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance;

        #region Defining variables
        [Title("Available Quests")]
        [Space]
        [InfoBox("If you update any tasks in the Unity Inspector, you need to update QuestStates manually by pressing 'Update All Quest States' above. Note that QuestState.Completed quests will not be updated when pressing the button.")]
        [Space]
        [SerializeField, PropertyOrder(2)]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        private List<Quest> quests = new List<Quest>();

        [ShowInInspector, ReadOnly]
        private DialogueVariables dialogueVariables;
        #endregion

        #region Configuring Unity Inspector-only buttons
        [VerticalGroup("General/Split/Right")]
        [Button("Reset All Quests & Tasks", ButtonSizes.Large), GUIColor(1, 0.2f, 0)]
        private void ResetAllQuests()
        {
            for (int i = 0; i < quests.Count; i++)
            {
                quests[i].Reset();
            }

            if (Application.isPlaying)
            {
                ConfigureActiveQuestsForRuntime();
                RequestListenersToGetUpdate();
            }
        }

        [Title("Dialogue Variables")]
        [Button("Print All Ink Dialogue Variables & Their Values", ButtonSizes.Large), PropertyOrder(0), PropertySpace(SpaceBefore = 10)]
        private void PrintVariableValues()
        {
            if (Application.isPlaying == false)
            {
                Helper.LogWarning("[QuestManager] This function only works in Play mode.");
                return;
            }
            else if (dialogueVariables == null)
            {
                Helper.LogWarning("[QuestManager] There are no dialogue variables. This is unexpected. Please investigate why this has happened.");
                return;
            }

            Helper.Log("[QuestManager] List of current global Ink variables (public version):");
            foreach (KeyValuePair<string, Ink.Runtime.Object> kvp in dialogueVariables.Variables)
                Helper.Log(" - " + kvp.Key + " = " + kvp.Value);
        }
        #endregion

        #region Awake & initialisation
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

            ConfigureActiveQuestsForRuntime();
            RefreshAllIncompleteQuestStates();
        }
        #endregion

        #region Updating tasks (public, used by MQuestDefeatable/MQuestLocation/etc.)
        public void UpdateTask(TaskType type, Transform reachedObject = null, NPCQuestIdentifier identifier = NPCQuestIdentifier.Unspecified)
        {
            for (int i = 0; i < quests.Count; i++)
            {
                // Only go through QuestState.Active
                if (quests[i].State != QuestState.Active) continue;

                // Loop through all tasks within the quest and update them, if applicable
                for (int j = 0; j < quests[i].Tasks.Count; j++)
                {
                    switch (type)
                    {
                        case TaskType.Do:
                            // TO DO: Implement when required
                            break;
                        case TaskType.Defeat:
                            if (quests[i].Tasks[j].NPCIdentifier != identifier) continue;
                            quests[i].Tasks[j].Defeated++;
                            Helper.Log("[QuestManager] Defeat quest: Count = " + quests[i].Tasks[j].Defeated + ".");
                            break;
                        case TaskType.Collect:
                            // TO DO: Implement when required
                            break;
                        case TaskType.Reach:
                            Helper.Log("[QuestManager] Reach quest: Evaluating location.");
                            quests[i].Tasks[j].AtLocation(reachedObject);
                            break;
                        default:
                            Helper.Log("[QuestManager] Task type (" + type + ") to be updated does not exist.");
                            break;
                    }
                }

                // Loop through all tasks within the same quest again and check whether
                // all tasks have been comleted
                bool thereAreOutstandingTasksLeft = false;
                for (int j = 0; j < quests[i].Tasks.Count; j++)
                {
                    if (quests[i].Tasks[j].State == false)
                        thereAreOutstandingTasksLeft = true;
                }

                // If yes, set QuestState.Ready
                if (thereAreOutstandingTasksLeft == false)
                {
                    SetQuestStateToReady(quests[i]);
                    SetQuestAndDialogueVariableToReady(quests[i]);
                }
            }
        }
        #endregion

        #region Managing temporaryID for all quests
        private void ConfigureActiveQuestsForRuntime()
        {
            for (int i = 0; i < quests.Count; i++)
            {
                quests[i].TemporaryID = i;
                quests[i].InActiveScene = true;
            }
        }

        private void ResetActiveQuestsRuntimeConfiguration()
        {
            for (int i = 0; i < quests.Count; i++)
            {
                quests[i].TemporaryID = 0;
                quests[i].InActiveScene = false;
            }
        }
        #endregion

        #region Managing QuestState
        [TitleGroup("General")]
        [HorizontalGroup("General/Split"), PropertyOrder(1)]
        [VerticalGroup("General/Split/Left")]
        [Button("Update All Quest States", ButtonSizes.Large)]
        private void RefreshAllIncompleteQuestStates()
        {
            for (int i = 0; i < quests.Count; i++)
            {
                if (quests[i].State == Quest.QuestState.Completed) continue;

                // Loop through all tasks in a quest to find out if 1) any and/or 2) all
                // tasks have been completed
                bool thereAreOutstandingTasksLeft = false;
                bool someTasksHaveBeenCompleted = false;
                for (int j = 0; j < quests[i].Tasks.Count; j++)
                {
                    if (quests[i].Tasks[j].State == false)
                        thereAreOutstandingTasksLeft = true;
                    else someTasksHaveBeenCompleted = true;
                }

                // Set QuestState if...
                if (thereAreOutstandingTasksLeft && someTasksHaveBeenCompleted == false)
                {
                    // ...no tasks have been completed yet
                    if (quests[i].QuestRequirementStatus == true)
                        quests[i].State = Quest.QuestState.Available; // ...but requirements fulfilled
                    else quests[i].State = Quest.QuestState.Unavailable; // ...and requirements not fulfilled
                }
                else if (thereAreOutstandingTasksLeft && someTasksHaveBeenCompleted)
                {
                    // ...some task have been completed
                    quests[i].State = Quest.QuestState.Active;
                }
                else if (thereAreOutstandingTasksLeft == false)
                {
                    // ...all tasks have been completed
                    if (quests[i].State != Quest.QuestState.Ready
                        && quests[i].State != Quest.QuestState.Completed)
                    {
                        SetQuestStateToReady(quests[i]);
                        SetQuestAndDialogueVariableToReady(quests[i]);
                    }
                }
            }

            if (Application.isPlaying) RequestListenersToGetUpdate();
        }

        private void SetQuestStateToReady(Quest quest)
        {
            quest.State = Quest.QuestState.Ready;
        }

        private void UpdateQuestStateAfterVariableChange(Quest quest)
        {
            if (quest.State == QuestState.Active || quest.State == QuestState.Completed)
            {
                Helper.Log("[QuestManager] Request ignored. Cannot change State for '" + quest.Name + "' because it is " + quest.State + " which is set through dialogue only i.e. by Ink.");
                return;
            }

            // Loop through all QuestVariables to look for VariableCategory.QuestComplete
            // and VariableCategory.QuestAccepted, and update QuestState - break loop if
            // the former is true
            for (int i = 0; i < quest.QuestVariables.Count; i++)
            {
                if (quest.QuestVariables[i].Category == VariableCategory.QuestComplete
                    && quest.QuestVariables[i].BoolValue == true
                    && quest.State == QuestState.Ready)
                {
                    quest.State = QuestState.Completed;
                    return;
                }

                if (quest.QuestVariables[i].Category == VariableCategory.QuestAccepted
                    && quest.QuestVariables[i].BoolValue == true
                    && quest.State != QuestState.Ready
                    && quest.State != QuestState.Completed)
                {
                    quest.State = QuestState.Active;
                }
            }
        }
        #endregion

        #region Managing QuestVariables & DialogueVariables
        private void SetDialogueVariables(DialogueVariables variables)
        {
            dialogueVariables = variables;
        }

        private async void SetQuestAndDialogueVariableToReady(Quest quest)
        {
            if (quest.Story == null) return;

            await Task.Delay(System.TimeSpan.FromSeconds(0.25f));

            for (int i = 0; i < quest.QuestVariables.Count; i++)
            {
                if (quest.QuestVariables[i].Category == VariableCategory.QuestReady)
                {
                    quest.QuestVariables[i].BoolValue = true;
                    Ink.Runtime.Object obj = new Ink.Runtime.BoolValue(true);
                    dialogueVariables.UpdateDictionary(quest.QuestVariables[i].Name, obj);
                }
            }
        }

        // NOTE: The below are not implemented yet as they were not needed but they will
        // be critical for any non-completion state QuestVariable
        private void SetQuestAndDialogueVariable(Quest quest, VariableCategory category, string value)
        {
            if (category == VariableCategory.QuestAccepted 
                || category == VariableCategory.QuestComplete)
            {
                Helper.LogWarning("[QuestManager] Invalid request. You're trying to set a variable that is controlled by Ink. Request ignored.");
                return;
            }

            for (int i = 0; i < quest.QuestVariables.Count; i++)
            {
                if (quest.QuestVariables[i].Category == category)
                {
                    // Set QuestVariable
                    quest.QuestVariables[i].StringValue = value;

                    // Set DialogueVariable
                    Ink.Runtime.Object obj = new Ink.Runtime.StringValue(value);
                    dialogueVariables.UpdateDictionary(quest.QuestVariables[i].Name, obj);
                }
            }
        }

        private void SetQuestAndDialogueVariable(Quest quest, VariableCategory category, bool value)
        {
            if (category == VariableCategory.QuestAccepted
                || category == VariableCategory.QuestComplete)
            {
                Helper.LogWarning("[QuestManager] Invalid request. You're trying to set a variable that is controlled by Ink. Request ignored.");
                return;
            }

            for (int i = 0; i < quest.QuestVariables.Count; i++)
            {
                if (quest.QuestVariables[i].Category == category)
                {
                    // Set QuestVariable
                    quest.QuestVariables[i].BoolValue = value;

                    // Set DialogueVariable
                    Ink.Runtime.Object obj = new Ink.Runtime.BoolValue(value);
                    dialogueVariables.UpdateDictionary(quest.QuestVariables[i].Name, obj);
                }
            }
        }

        private void SetQuestAndDialogueVariable(Quest quest, VariableCategory category, int value)
        {
            if (category == VariableCategory.QuestAccepted
                || category == VariableCategory.QuestComplete)
            {
                Helper.LogWarning("[QuestManager] Invalid request. You're trying to set a variable that is controlled by Ink. Request ignored.");
                return;
            }

            for (int i = 0; i < quest.QuestVariables.Count; i++)
            {
                if (quest.QuestVariables[i].Category == category)
                {
                    // Set QuestVariable
                    quest.QuestVariables[i].IntValue = value;

                    // Set DialogueVariable
                    Ink.Runtime.Object obj = new Ink.Runtime.IntValue(value);
                    dialogueVariables.UpdateDictionary(quest.QuestVariables[i].Name, obj);
                }
            }
        }
        #endregion

        #region Managing events
        public event Action OnEventsUpdated;

        private void RequestListenersToGetUpdate()
        {
            Helper.Log("[QuestManager] All QuestGivers were requested to update themselves.");
            OnEventsUpdated?.Invoke();
        }

        // This function is triggered by MButtonInGame through an event when the
        // player leaves the interactionRadius of the MButtonInGame - its purpose
        // is to make sure that the game is aware of any updates to variables in
        // Ink which are relevant to the game
        private void UpdateQuestVariables(string storyName)
        {
            if (dialogueVariables == null)
            {
                Helper.Log("[QuestManager] dialogueVariables = null, mate. WTF?");
                return;
            }

            Helper.Log("[QuestManager] QuestVariables are being updated because player left interactionRadius of MButtonInGame.");

            // Loop through all quests and their variables to find the quest with storyName
            for (int i = 0; i < quests.Count; i++)
            {
                if (quests[i].name == storyName)
                {
                    Helper.Log("[QuestManager] Quest '" + storyName + "':");

                    // Update all variables for the quest
                    for (int j = 0; j < quests[i].QuestVariables.Count; j++)
                    {
                        switch (quests[i].QuestVariables[j].Type)
                        {
                            case VariableType.String:
                                quests[i].QuestVariables[j].StringValue = dialogueVariables.ReturnString(quests[i].QuestVariables[j].Name);
                                Helper.Log(" - " + quests[i].QuestVariables[j].Name + " updated to " + quests[i].QuestVariables[j].StringValue + ".");
                                break;
                            case VariableType.Bool:
                                quests[i].QuestVariables[j].BoolValue = dialogueVariables.ReturnBool(quests[i].QuestVariables[j].Name);
                                Helper.Log(" - " + quests[i].QuestVariables[j].Name + " updated to " + quests[i].QuestVariables[j].BoolValue + ".");
                                break;
                            case VariableType.Int:
                                quests[i].QuestVariables[j].IntValue = dialogueVariables.ReturnInt(quests[i].QuestVariables[j].Name);
                                Helper.Log(" - " + quests[i].QuestVariables[j].Name + " updated to " + quests[i].QuestVariables[j].IntValue + ".");
                                break;
                            default:
                                break;
                        }
                    }

                    UpdateQuestStateAfterVariableChange(quests[i]);
                }
            }
        }

        private void OnEnable()
        {
            EventManager.Instance.OnDialogueVariablesShare += SetDialogueVariables;
            EventManager.Instance.OnInteractionTriggerRadiusExit += UpdateQuestVariables;
        }

        private void OnDestroy()
        {
            EventManager.Instance.OnDialogueVariablesShare -= SetDialogueVariables;
            EventManager.Instance.OnInteractionTriggerRadiusExit -= UpdateQuestVariables;

            // Reset all quest IDs as they are scene dependent
            ResetActiveQuestsRuntimeConfiguration();
        }
        #endregion
    }
}