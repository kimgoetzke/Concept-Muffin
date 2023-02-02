using UnityEngine;

namespace CaptainHindsight
{
    public class MSetGlobalShaderVariables : MonoBehaviour
    {
        [SerializeField] RenderTexture renderTexture;

        private void Awake()
        {
            Shader.SetGlobalTexture("_GlobalWaterTexture", renderTexture);
        }
    }
}