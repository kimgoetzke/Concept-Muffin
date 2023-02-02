using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace CaptainHindsight
{
    public class TransitionManager : MonoBehaviour
    {
        public static TransitionManager Instance;

        [SerializeField][Required] 
        private Image blackImage;

        [SerializeField]
        private float lengthOfFadeOut = 1f;

        [SerializeField]
        private float lengthOfFadeIn = 3f;

        private void Awake()
        {
            blackImage.gameObject.SetActive(true);

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public void FadeToNextScene(string levelName) => LoadLevelAfterFade(levelName, 999);

        public void FadeToNextScene(int levelNumber) => LoadLevelAfterFade("", levelNumber);

        private async void OnEnable()
        {
            // Fade out black image
            blackImage.DOFade(0f, lengthOfFadeIn).SetEase(Ease.InSine).SetUpdate(UpdateType.Normal, true);

            await Task.Delay(System.TimeSpan.FromSeconds(lengthOfFadeIn));

            // Switch to Play state
            // NOTE: I'll have to do some work here to when implementing Menu/Tutorial state
            GameStateDirector.Instance.SwitchState(GameState.Play);
        }

        private async void LoadLevelAfterFade(string name, int number)
        {
            // Fade in black overlay
            blackImage.DOFade(1f, lengthOfFadeOut).SetUpdate(UpdateType.Normal, true);

            // Switch state
            GameStateDirector.Instance.SwitchState(GameState.Transition);

            await Task.Delay(System.TimeSpan.FromSeconds(lengthOfFadeOut + 1f));

            // Kill all DOTweens to prevent errors/warnings
            DOTween.KillAll();

            // Load next scene by number or, if set to 999 (loaded by name), load by name
            if (number == 999) SceneManager.LoadScene(name);
            else SceneManager.LoadScene(number);
        }
    }
}