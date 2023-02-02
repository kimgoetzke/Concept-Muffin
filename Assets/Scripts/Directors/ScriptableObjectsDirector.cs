using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CaptainHindsight
{
    public class ScriptableObjectsDirector : MonoBehaviour
    {
        public static ScriptableObjectsDirector Instance;

        [AssetList(Path = "/Data/Colours/", AutoPopulate = true)]
        public ColourPalette[] Colours;

        [AssetList(Path = "/Data/Equipment/", AutoPopulate = true)]
        public List<EquipmentItem> Equipment;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}