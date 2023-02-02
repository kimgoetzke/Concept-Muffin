using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight
{
    [DisallowMultipleComponent]
    public class PlayerManager : GameStateBehaviour, IDamageable, IHealable
    {
        public static PlayerManager Instance;

        [ShowInInspector, ReadOnly, ProgressBar(0, "maxHealth", ColorGetter = "GetHealthBarColor", Height = 25), PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
        private int currentHealth;

        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField] private Transform effectPopup;
        [SerializeField] private Transform spawnPosition;
        private MHealthBar healthBar;
        private bool canBeAffected = true;

        #region Awake & Start
        private void Awake()
        {
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

        private void Start()
        {
            // Find health bar
            if (GameObject.Find("UI").GetComponentInChildren<MHealthBar>(true) == null) Debug.LogError("[PlayerManager] UI canvas not found.");
            else healthBar = GameObject.Find("UI").GetComponentInChildren<MHealthBar>(true);

            // Initial configuration
            currentHealth = maxHealth;
            healthBar.ConfigureHealthbar(maxHealth);
        }
        #endregion

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            healthBar.SetHealth(currentHealth);
        }

        #region Managing damage/health
        public void TakeDamage(int damage, Transform origin)
        {
            // Guard clause to prevent damage in certain GameStates
            if (canBeAffected == false) return;

            // Calculate and set new health
            int newHealth = currentHealth - damage;
            currentHealth = newHealth;

            // Update health bar
            healthBar.SetHealth(Mathf.Clamp(newHealth, 0, maxHealth));

            // Instantiate effect popup showing amount of damage taken
            Transform popup = Instantiate(effectPopup, transform);
            popup.position = spawnPosition.position;
            popup.GetComponent<MEffectPopup>().Initialisation(ActionType.Damage, "-" + damage.ToString());

            // Initiate death, if health <= 0
            if (newHealth <= 0) Die();
        }

        public void AddHealth(int health)
        {
            // Guard clause to prevent healing in certain GameStates
            if (canBeAffected == false) return;

            // Calculate and set new health
            int newHealth = Mathf.Clamp(currentHealth + health, 0, maxHealth);
            currentHealth = newHealth;

            // Update health bar
            healthBar.SetHealth(newHealth);

            // Instantiate effect popup showing amount of damage taken
            Transform popup = Instantiate(effectPopup, transform);
            popup.position = spawnPosition.position;
            popup.GetComponent<MEffectPopup>().Initialisation(ActionType.Health, "+" + health.ToString());

        }
        #endregion

        private void Die()
        {
            healthBar.gameObject.SetActive(false);
            GameStateDirector.Instance.SwitchState(GameState.GameOver);
        }

        #region Managing events
        protected override void ActionGameStateChange(GameState state, GameStateSettings settings)
        {
            canBeAffected = settings.PlayerIsAffected;
        }
        #endregion
    }
}
