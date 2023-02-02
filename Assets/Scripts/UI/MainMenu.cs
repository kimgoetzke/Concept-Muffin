using UnityEngine;

namespace CaptainHindsight
{
    public class MainMenu : MonoBehaviour
    {
        public void NewGame()
        {
            TransitionManager.Instance.FadeToNextScene(1);
        }

        public void QuitGame()
        {
            Helper.Log("Application closed.");
            Application.Quit();
        }
    }
}