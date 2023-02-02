using UnityEngine;

namespace CaptainHindsight
{
    [RequireComponent(typeof(BoxCollider))]
    public class MQuestLocation : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                //Helper.Log("[MQuestLocation] A quest location was reached.");
                QuestManager.Instance.UpdateTask(Quest.TaskType.Reach, this.transform);
            }
        }
    }
}
