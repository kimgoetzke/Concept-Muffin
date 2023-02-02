using UnityEngine;

namespace CaptainHindsight
{
    public abstract class GameStateBehaviour : MonoBehaviour
    {
        abstract protected void ActionGameStateChange(GameState state, GameStateSettings settings);

        protected virtual void OnEnable() => GameStateDirector.OnGameStateChange += ActionGameStateChange;

        protected virtual void OnDestroy() => GameStateDirector.OnGameStateChange -= ActionGameStateChange;
    }
}