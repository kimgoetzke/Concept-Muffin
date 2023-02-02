using DG.Tweening;
using UnityEngine;

namespace CaptainHindsight
{
    public class MCloudShadows : MonoBehaviour
    {
        private Light myLight;

        private void Start()
        {
            myLight = GetComponent<Light>();
            myLight.transform.DOMoveZ(25, 700).SetLoops(-1).SetEase(Ease.OutSine);
            myLight.transform.DOMoveX(35, 1000).SetLoops(-1).SetEase(Ease.OutSine);
        }
    }
}
