using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight
{
    [CreateAssetMenu(fileName = "GameState-", menuName = "Scriptable Object/New game state settings")]
    public class GameStateSettings : ScriptableObject
    {
        [Title("State")]
        public GameState Name;

        [Title("UI settings")]
        public bool ShowInGameUI;

        [Title("Time settings")]
        public bool FreezeTime;

        [Title("Player settings")]
        public bool PlayerCanMove;
        public bool PlayerCanInteract;
        public bool PlayerIsAffected;
    }
}