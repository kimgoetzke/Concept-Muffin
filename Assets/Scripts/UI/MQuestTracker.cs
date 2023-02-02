using PlasticPipe.PlasticProtocol.Messages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight
{
    public class MQuestTracker : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleMesh;

        [SerializeField]
        private Transform backgroundTransform;

        [SerializeField]
        private GameObject taskTemplatePrefab;

        private MQuestTrackerTask[] mQuestTrackerTasks;

        private void Awake()
        {
            
        }

        private void Start()
        {
        
        }

        private void Update()
        {
        
        }

        // TO DO:
        // - Think about best way to call the below methods (event?)

        public void InitialiseActiveQuest(Quest quest)
        {
            titleMesh.text = quest.Name;

            for (int i = 0; i < quest.Tasks.Count; i++)
            {
                GameObject taskHolder = Instantiate(taskTemplatePrefab);
                taskHolder.GetComponent<MQuestTrackerTask>().InitialiseTask(quest.Tasks[i].Name);
            }
        }

        public void SetQuestTask(bool complete)
        {
            for (int i = 0; i < mQuestTrackerTasks.Length; i++)
            {
                //mQuestTrackerTasks[i].
            }
        }
    }
}
