using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight
{
    [CreateAssetMenu(fileName = "Colours-", menuName = "Scriptable Object/New colour palette")]
    public class ColourPalette : ScriptableObject
    {
        [ColorPalette] public Color[] Colours;
    }
}