using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight
{
    public class MQuestDefeatable : MonoBehaviour
    {
        [Title("Configuration")]
        [InfoBox("This script must be placed on the same object as the HealthManager. It will automatically get the reference and use it to identify who defeated this NPC.")]

        [SerializeField, Required]
        private NPCQuestIdentifier identifier;

        private HealthManager healthManager;

        private void Start()
        {
            healthManager = GetComponent<HealthManager>();
            healthManager.RegisterQuestDefeatable(this);
        }

        public void ProcessInformation(Transform origin)
        {
            if (origin.CompareTag("Player"))
                QuestManager.Instance.UpdateTask(Quest.TaskType.Defeat, null, NPCQuestIdentifier.Fukuiraptor);
                //QuestManager.Instance.NPCDefeated(identifier);
        }
    }
}
