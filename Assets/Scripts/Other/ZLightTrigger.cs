using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaptainHindsight
{
    public class ZLightTrigger : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField]
        [ListDrawerSettings(Expanded = true)]
        private List<LightSettings> lights = new List<LightSettings>();

        [Title("Time")]
        [SerializeField, Tooltip("Time it takes to lerp to the new intensity value OnTriggerEnter")]
        [Min(0)]
        float lerpIn;

        [SerializeField, Tooltip("Time it takes to lerp back to the previous light intensity value OnTriggerExit")]
        [Min(0)]
        float lerpOut;

        private void Awake()
        {
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].DefaultIntensity = lights[i].Light.intensity;
                lights[i].DefaultTemperature = lights[i].Light.colorTemperature;
                lights[i].DefaultColour = lights[i].Light.color;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") == false) return;

            int killTweens = DOTween.KillAll();
            Helper.Log("[ZLightTrigger] Special light zone entered. Killed " + killTweens + " tweens to avoid conflicts.");

            for (int i = 0; i < lights.Count; i++)
            {
                if (lights[i].ChangeIntensity) lights[i].Light.DOIntensity(lights[i].Intensity, lerpIn);
                if (lights[i].ChangeColour) lights[i].Light.DOColor(lights[i].Colour, lerpIn);
                if (lights[i].ChangeTemperature) ChangeColourTemperature(i, lights[i].Temperature, lerpIn);
            }
        }

        private async void ChangeColourTemperature(int index, float temperature, float delay)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(delay));

            lights[index].Light.colorTemperature = temperature;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player") == false) return;

            int killTweens = DOTween.KillAll();
            Helper.Log("[ZLightTrigger] Special light zone exited. Killed " + killTweens + " tweens to avoid conflicts.");

            for (int i = 0; i < lights.Count; i++)
            {
                if (lights[i].ChangeIntensity) lights[i].Light.DOIntensity(lights[i].DefaultIntensity, lerpOut);
                if (lights[i].ChangeColour) lights[i].Light.DOColor(lights[i].DefaultColour, lerpOut);
                if (lights[i].ChangeTemperature) ChangeColourTemperature(i, lights[i].DefaultTemperature, lerpOut);
                
                // TO DO: Try to make this work (which may not be possible)!
                //DOTween.To(() => lights[i].Light.colorTemperature, x => lights[i].Light.colorTemperature = x, lights[i].DefaultTemperature, lerpIn);
            }
        }

        [System.Serializable]
        private class LightSettings
        {
            [SerializeField, SceneObjectsOnly, HideLabel] public Light Light;
            [HideInInspector] public float DefaultIntensity;
            [HideInInspector] public Color DefaultColour;
            [HideInInspector] public float DefaultTemperature;

            [HorizontalGroup("Split1")]
            [VerticalGroup("Split1/Left")]
            [SerializeField]
            [LabelWidth(90)]
            [LabelText("Intensity")]
            public bool ChangeIntensity;

            [VerticalGroup("Split1/Right")]
            [SerializeField, HideLabel, ShowIf("ChangeIntensity"), PropertyRange(0, 10)] 
            public float Intensity;

            [HorizontalGroup("Split2")]
            [VerticalGroup("Split2/Left")]
            [SerializeField]
            [LabelWidth(90)]
            [LabelText("Colour")]
            public bool ChangeColour;

            [VerticalGroup("Split2/Right")]
            [SerializeField, HideLabel, ShowIf("ChangeColour")] 
            public Color Colour = Color.white;

            [HorizontalGroup("Split3")]
            [VerticalGroup("Split3/Left")]
            [SerializeField]
            [LabelWidth(90)]
            [LabelText("Temperature")]
            public bool ChangeTemperature;

            [VerticalGroup("Split3/Right")]
            [SerializeField, HideLabel, ShowIf("ChangeTemperature"), PropertyRange(1500, 20000)]
            public float Temperature = 1500;
        }
    }
}
