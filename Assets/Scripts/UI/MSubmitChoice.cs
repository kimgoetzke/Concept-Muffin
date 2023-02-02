using UnityEngine;

namespace CaptainHindsight
{
    public class MSubmitChoice : MonoBehaviour
    {
        public int ChoiceIndex;
        public void SubmitChoice()
        {
            EventManager.Instance.SubmitDialogueChoice(ChoiceIndex);
        }
    }
}
