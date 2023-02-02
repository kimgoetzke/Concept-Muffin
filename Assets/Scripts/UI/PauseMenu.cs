using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using TMPro;
using Sirenix.OdinInspector;

namespace CaptainHindsight
{
    public class PauseMenu : GameStateBehaviour
    {
        [Title("General configuration")]
        [SerializeField] public GameObject pauseMenuUI;

        [SerializeField] private GraphicRaycaster raycaster;

        [ShowInInspector, ReadOnly] private bool allowButtonPress = true;

        [SerializeField, ListDrawerSettings(Expanded = true)]
        [ChildGameObjectsOnly]
        private RectTransform[] buttons;

        [HideInInspector] private bool gameIsPaused = false;

        [Title("Game Over menu")]
        [SerializeField] private TextMeshProUGUI titleMesh;
        [SerializeField] private GameObject resumeButton;

        #region Game Over menu
        private async void SwitchToAndTriggerGameOverMenu()
        {
            // Lock pause menu so it cannot be triggered during death animation and
            // it doesn't need to be unlocked later as there will be a change in scene
            LockPauseMenu(true);

            // Change pause menu title to game over menu title
            titleMesh.text = " -GAME OVER-";

            // Deactivate resume button
            resumeButton.GetComponent<UnityEngine.UI.Button>().interactable = false;
            resumeButton.GetComponentInChildren<TextMeshProUGUI>().alpha = 0.2f;

            // Wait 2 seconds, then trigger menu
            await Task.Delay(System.TimeSpan.FromSeconds(2f));
            PauseGame();
        }
        #endregion

        #region Managing button functionality
        private void PauseGame()
        {
            // Ensure that the below, esp. animations, cannot be interrupted/messed
            // up by going out of the pause menu
            LockPauseMenu(true);

            // Enable raycaster so that player can press buttons
            raycaster.enabled = true;

            // Play sound effect
            PlayButtonPressSound();

            // Fade in background and title
            pauseMenuUI.SetActive(true);
            pauseMenuUI.GetComponent<Image>().DOFade(0.8f, 0.4f).SetUpdate(UpdateType.Normal, true).OnComplete(() => { titleMesh.gameObject.SetActive(true); });

            // Animations that move each button into the visible screen
            var sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
            foreach (var button in buttons)
            {
                sequence.Append(button.DOAnchorPosY(25, 0.2f).SetEase(Ease.Linear));
            }

            // Allowing user to use Escape again
            LockPauseMenu(false);
        }

        private async Task ResumeGame()
        {
            // Ensure that the below, esp. animations, cannot be interrupted/messed
            // up by going out of the pause menu
            LockPauseMenu(true);

            // Disable raycaster again so it doesn't interfere
            raycaster.enabled = false;

            // Play sound effect
            PlayButtonPressSound();

            // Animations that move each button out of the visible screen
            var sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
            foreach (var button in buttons)
            {
                sequence.Append(button.DOAnchorPosY(-60, 0.2f).SetEase(Ease.Linear));
            }

            // Hide the title and fade out background
            titleMesh.gameObject.SetActive(false);
            pauseMenuUI.GetComponent<Image>().DOFade(0f, 0.7f).SetUpdate(UpdateType.Normal, true);

            // Add a delay to keep menu locked during animation
            await Task.Delay(System.TimeSpan.FromSeconds(0.7f));

            pauseMenuUI.SetActive(false);

            // Allowing user to use Escape again
            LockPauseMenu(false);

            await Task.Yield();
        }

        public void RestartLevel() => TransitionManager.Instance.FadeToNextScene(SceneManager.GetActiveScene().buildIndex);

        public void MainMenu() => TransitionManager.Instance.FadeToNextScene("MainMenu");
        #endregion

        #region Managing audio
        public void PlayButtonHighlightSound()
        {
            AudioDirector.Instance.Play("Select");
        }

        public void PlayButtonPressSound()
        {
            AudioDirector.Instance.Play("Click");
        }
        #endregion

        private void LockPauseMenu(bool status) => allowButtonPress = !status;

        #region Managing events and game states
        // Method used by EventManager (to block escape key when in other menus, for example)
        // and while menu fade in animations are playing
        public async void AttemptToPauseOrUnpause()
        {
            if (allowButtonPress)
            {
                if (gameIsPaused)
                {
                    await ResumeGame();
                    gameIsPaused = false;
                    GameStateDirector.Instance.SwitchState(GameState.Play);
                }
                else if (gameIsPaused == false)
                {
                    PauseGame();
                    gameIsPaused = true;
                    GameStateDirector.Instance.SwitchState(GameState.Pause);
                }
            }
            else Helper.LogWarning("PauseMenu: Menu is locked. Try again later.");
        }

        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            if (state == GameState.GameOver)
            {
                SwitchToAndTriggerGameOverMenu();
            }
            else if (state == GameState.Tutorial || state == GameState.Transition)
            {
                LockPauseMenu(true);
            }
            else if (state == GameState.Play)
            {
                LockPauseMenu(false);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.OnPauseMenuRequest += AttemptToPauseOrUnpause;
            raycaster.enabled = false; // Without this the pause menu will block interactions with other menus (such as an end of level menu) and possible a touch controller
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventManager.Instance.OnPauseMenuRequest -= AttemptToPauseOrUnpause;
        }
        #endregion
    }
}
