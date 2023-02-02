using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight
{
    public class MQuestTrackerTask : MonoBehaviour
    {
        [Title("General")]

        [SerializeField, Required]
        private Image indicator;

        [SerializeField, Required]
        private TextMeshProUGUI textMesh;

        [Title("Complete settings")]

        [SerializeField, Required, LabelText("Indicator")]
        private Sprite taskCompleteIndicator;

        [SerializeField, Required, LabelText("Text colour")]
        private Color taskCompleteColour;

        [SerializeField, Required, LabelText("Text alpha")]
        private float taskIncompleteAlpha;

        [Title("Incomplete settings")]

        [SerializeField, Required, LabelText("Indicator")]
        private Sprite taskIncompleteIndicator;

        [SerializeField, Required, LabelText("Text colour")]
        private Color taskIncompleteColour;

        [SerializeField, Required, LabelText("Text alpha")]
        private float transparencyIncomplete;


        public void InitialiseTask(string text)
        {
            textMesh.text = text;
            SetTaskStatus(false);
        }

        public void SetTaskStatus(bool complete)
        { 
            if (complete)
            {
                textMesh.color = taskCompleteColour;
                textMesh.alpha = taskIncompleteAlpha;
                indicator.sprite = taskCompleteIndicator;
            }
            else
            {
                textMesh.color = taskIncompleteColour;
                textMesh.alpha = taskIncompleteAlpha;
                indicator.sprite = taskIncompleteIndicator;
            }
        }

        //public string GetName()
        //{

        //}
    }
}
