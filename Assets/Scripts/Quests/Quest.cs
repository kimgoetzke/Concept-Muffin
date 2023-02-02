using Ink.Runtime;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CaptainHindsight
{
    [CreateAssetMenu(fileName = "Quest-", menuName = "Scriptable Object/New quest")]
    public class Quest : ScriptableObject
    {
        [Title("Settings")]
        [LabelWidth(90)]
        public string Name;

        [LabelWidth(90), PropertySpace(SpaceAfter = 5, SpaceBefore = 0)]
        [InlineButton("Reset")]
        public QuestState State = QuestState.Unavailable;

        [BoxGroup("Only populated when in Play mode")]
        [HorizontalGroup("Only populated when in Play mode/Split")]
        [VerticalGroup("Only populated when in Play mode/Split/Left")]
        [ShowInInspector, ReadOnly]
        [LabelText("Temp. ID"), LabelWidth(60)]
        public int TemporaryID;

        [BoxGroup("Only populated when in Play mode")]
        [HorizontalGroup("Only populated when in Play mode/Split")]
        [VerticalGroup("Only populated when in Play mode/Split/Right")]
        [ShowInInspector, ReadOnly]
        [LabelText("In Active Scene")]
        public bool InActiveScene;

        [Title("Description", bold: false, horizontalLine: false)]
        [HideLabel]
        [MultiLineProperty]
        public string Description;

        [TitleGroup("Requirements")]
        [HorizontalGroup("Requirements/Split")]
        [VerticalGroup("Requirements/Split/Left")]
        [SerializeField, HideLabel, LabelText("Quest"), LabelWidth(90)]
        private bool questRequirement;

        [VerticalGroup("Requirements/Split/Right")]
        [SerializeField, HideLabel]
        [ShowIf("questRequirement")]
        private Quest questToComplete;

        [VerticalGroup("Requirements/Split/Right")]
        [ShowInInspector, ReadOnly]
        [HideLabel, LabelText("Fulfilled?")]
        public bool QuestRequirementStatus
        {
            get
            {
                if (questToComplete == null) return true;
                else if (questToComplete.State == QuestState.Completed) return true;
                else return false;
            }
        }

        [Space]
        [Title("Story")]
        [LabelWidth(90), LabelText("Story JSON")]
        [AssetSelector(Paths = "Assets/Data/Stories")]
        public TextAsset Story;

        public List<QuestVariable> QuestVariables = new List<QuestVariable>();

        [Space]
        [Title("Tasks")]
        [ListDrawerSettings(Expanded = true)]
        public List<QuestTask> Tasks = new List<QuestTask>();

        #region Class: QuestTask
        [System.Serializable]
        public class QuestTask
        {
            public string Name;

            [ShowInInspector]
            public bool State
            {
                get 
                { 
                    switch (Type)
                    {
                        case TaskType.Do:
                            if (Done) return true;
                            else return false;
                        case TaskType.Defeat:
                            if (Defeated >= ToDefeat) return true;
                            else return false;
                        case TaskType.Collect:
                            if (Collected >= ToCollect) return true;
                            else return false;
                        case TaskType.Reach:
                            if (LocationReached) return true;
                            else return false;
                        default: return false;
                    }
                }
            }

            public TaskType Type;

            [MultiLineProperty]
            public string Description;

            #region Class: QuestType - Type specific properties
            [ShowIf("Type", TaskType.Do)]
            public bool Done;

            [ShowIf("Type", TaskType.Defeat), MinValue(1)]
            public int ToDefeat;
            [ShowIf("Type", TaskType.Defeat), MinValue(0)]
            public int Defeated;
            [ShowIf("Type", TaskType.Defeat)]
            public NPCQuestIdentifier NPCIdentifier;

            [ShowIf("Type", TaskType.Collect)]
            public int ToCollect;
            [ShowIf("Type", TaskType.Collect)]
            public int Collected;
            [ShowIf("Type", TaskType.Collect)]
            public int TypeToCollect;

            [ShowIf("Type", TaskType.Reach)]
            public Transform Location;

            [ShowIf("Type", TaskType.Reach)]
            [SerializeField]
            private bool LocationReached;

            public bool AtLocation(Transform reachedObject)
            {
                if (LocationReached == true) return true;

                if (reachedObject == Location)
                {
                    Helper.Log("[Quest] Reach task: Location for a task was reached.");
                    LocationReached = true;
                    return true;
                }
                else return false;
            }
            #endregion

            public void ResetTask()
            {
                Done = false;
                Defeated = 0;
                Collected = 0;
                LocationReached = false;
            }
        }
        #endregion

        #region Class: QuestVariable
        [System.Serializable]
        public class QuestVariable
        {
            public string Name;
            public VariableCategory Category;

            // TO DO: Add method here to automatically set the bool below
            public bool SetByInk;

            [EnumToggleButtons]
            public VariableType Type;

            [ShowIf("Type", VariableType.Bool), DisableIf("SetByInk"), LabelText("Value")]
            public bool BoolValue;

            [ShowIf("Type", VariableType.Int), DisableIf("SetByInk"), LabelText("Value")]
            public int IntValue;

            [ShowIf("Type", VariableType.String), DisableIf("SetByInk"), LabelText("Value")]
            public string StringValue;

            public void ResetValues()
            {
                BoolValue = false;
                IntValue = 0;
                StringValue = "";
            }
        }
        #endregion

        public void Reset()
        {
            // Only reset ID if in Editor mode
            if (Application.isPlaying == false)
                TemporaryID = 0;
            else Helper.Log("[Quest] Quest reset, except for field 'TemporaryID' which is only reset when in Editor mode.");

            for (int i = 0; i < Tasks.Count; i++)
            {
                Tasks[i].ResetTask();
            }

            for (int i = 0; i < QuestVariables.Count; i++)
            {
                QuestVariables[i].ResetValues();
            }

            // NOTE: This will only work if quest dependencies are higher in the list because
            // otherwise the QuestRequirementStatus will be requested before the dependency
            // has been reset
            if (QuestRequirementStatus == true) State = QuestState.Available;
            else State = QuestState.Unavailable;
        }

        public void UpdateQuestState()
        {
            bool thereAreOutstandingTasksLeft = false;
            for (int j = 0; j < Tasks.Count; j++)
            {
                if (Tasks[j].State == false)
                    thereAreOutstandingTasksLeft = true;
            }

            if (thereAreOutstandingTasksLeft) return;

            // Set State to QuestState.Ready if there's a story (i.e. NPC) or
            // QuestState.Completed if there isn't
            if (QuestVariables.Count == 0)
                State = Quest.QuestState.Completed;
            else State = Quest.QuestState.Ready;
        }

        public enum QuestState
        {
            Unavailable,    // Set by QuestManager (SetAllQuestStates)
            Available,      // Set by Quest (if questRequirements true) or QuestManager (SetAllQuestStates)
            Active,         // Set by QuestManager through DialogueVariables
            Ready,          // Set by QuestManager when all tasks are completed
            Completed       // Set by QuestManager through DialogueVariables
        }

        public enum TaskType
        {
            Do,
            Defeat,
            Collect,
            Reach
        }
        public enum VariableCategory
        {
            Unspecified,    // Set by QuestManager
            QuestAccepted,  // Set by DialogueVariable
            QuestReady,     // Set by QuestManager
            QuestComplete,  // Set by DialogueVariable
            KnowsNPC        // Set by DialogueVariable
        }

        public enum VariableType
        {
            String,
            Bool,
            Int
        }
    }
}
