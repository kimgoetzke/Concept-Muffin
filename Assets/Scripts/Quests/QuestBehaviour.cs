using UnityEngine;

namespace CaptainHindsight
{
    public abstract class QuestBehaviour : MonoBehaviour
    {
        abstract protected void ActionQuestUpdateEvent();

        protected virtual void OnEnable()
        {
            QuestManager.Instance.OnEventsUpdated += ActionQuestUpdateEvent;
        }

        protected virtual void OnDestroy()
        {
            QuestManager.Instance.OnEventsUpdated -= ActionQuestUpdateEvent;
        }
    }
}
