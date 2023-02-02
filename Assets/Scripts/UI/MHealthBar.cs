using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaptainHindsight
{
    public class MHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private Image image;
        [SerializeField] private Gradient gradient;
        [SerializeField] private label labelSetting;

        [SerializeField, ShowIf("@this.labelSetting == label.Health || this.labelSetting == label.Both")] 
        private TextMeshProUGUI healthMesh;

        [SerializeField, ShowIf("@this.labelSetting == label.Name || this.labelSetting == label.Both")] 
        private TextMeshProUGUI nameMesh;

        [SerializeField] private TextMeshProUGUI indicatorMesh;

        private enum label
        {
            None,
            Name,
            Health,
            Both
        }

        public void ConfigureHealthbar(int maxHealth, string name = "")
        {
            // Set colour and current/maxValue
            image.color = gradient.Evaluate(1);
            slider.maxValue = maxHealth;
            slider.value = maxHealth;

            // Set or disable healthMesh (numeric values shown on the bar)
            if (labelSetting == label.Health || labelSetting == label.Both)
                SetHealthMesh(maxHealth);
            else if (healthMesh != null) healthMesh.enabled = false;

            // Set or disable nameMesh (string shown above the bar)
            if (labelSetting == label.Name || labelSetting == label.Both)
                SetNameMesh(name);
            else if (nameMesh != null) nameMesh.enabled = false;

            // Disable indicatorMesh (used to indicate interaction options)
            indicatorMesh.enabled = false;
        }

        public void SetHealth(int health)
        {
            slider.value = health;

            // Set color of health bar
            float normalisedValue = slider.value / slider.maxValue;
            image.color = gradient.Evaluate(normalisedValue);

            if (slider.value > slider.maxValue) slider.value = slider.maxValue;

            if (labelSetting == label.Health || labelSetting == label.Both)
                SetHealthMesh(health);
        }

        public void SetNameMesh(string name)
        {
            nameMesh.text = name;
        }

        private void SetHealthMesh(int health)
        {
            healthMesh.text = health.ToString() + " / " + slider.maxValue.ToString();
        }
    }
}
