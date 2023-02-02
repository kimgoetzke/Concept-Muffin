using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight
{
    public class MHideUI : GameStateBehaviour
    {
        [SerializeField, Tooltip("Activates/deactivates the GameObject to which the script is attached based on GameStateSettings.")] 
        private bool childObject = true;

        [SerializeField, HideIf("childObject")]
        [ChildGameObjectsOnly]
        private GameObject[] gameObjectList;

        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            if (childObject) gameObject.GetComponentInChildren<Transform>().gameObject.SetActive(settings.ShowInGameUI);
            else
            {
                foreach (var obj in gameObjectList)
                {
                    obj.SetActive(settings.ShowInGameUI);
                }
            }
        }

        private void OnDisable()
        {
            GameStateDirector.OnGameStateChange -= ActionGameStateChange;
        }
    }
}
