using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight
{
    [DisallowMultipleComponent]
    public class HealthManager : MonoBehaviour, IDamageable
    {
        [BoxGroup("Name")]
        [SerializeField, HideLabel, PropertySpace(SpaceBefore = 5, SpaceAfter = 5)] private string npcName = "Unspecified";

        [BoxGroup("Health")]
        [Title("Current Health", bold: false, horizontalLine: false, titleAlignment:TitleAlignments.Centered)]
        [GUIColor(0.3f, 0.8f, 0.8f, 1f)]
        [HideLabel]
        [ShowInInspector, ReadOnly, ProgressBar(0, "maxHealth", ColorGetter = "GetHealthBarColor", Height = 25), PropertySpace(SpaceBefore = -10, SpaceAfter = 10)] 
        private int currentHealth;

        [BoxGroup("Health")]
        [SerializeField, Min(1), PropertySpace(SpaceAfter = 5)] private int maxHealth = 100;

        [TabGroup("Particles")][Required][SerializeField]
        [AssetSelector(Paths = "Assets/Prefabs/Combat", DropdownWidth = 300)]
        private GameObject meatParticles;
        [TabGroup("Particles")][Required][SerializeField]
        [AssetSelector(Paths = "Assets/Prefabs/Combat", DropdownWidth = 300)]
        private GameObject bloodParticles;
        [TabGroup("Particles")][Required][SerializeField]
        [AssetSelector(Paths = "Assets/Prefabs/Combat", DropdownWidth = 300)]
        private GameObject bloodSplashParticles;

        [TabGroup("UI")][Required][SerializeField]
        private MHealthBar healthBar;
        [TabGroup("UI")][Required, SerializeField]
        private Transform uiPositionMarker;
        [TabGroup("UI")][Required][SerializeField]
        [AssetSelector(Paths = "Assets/Prefabs/UI", DropdownWidth = 300)]
        private Transform effectPopup;
        [TabGroup("UI")][Required][SerializeField]
        private Transform spawnPosition;

        [FoldoutGroup("Deactivate OnDeath", expanded: false)]
        [SerializeField][Required] private Animator animator;

        [FoldoutGroup("Deactivate OnDeath", expanded: false)]
        [SerializeField][Required] private NPC stateMachine;

        [FoldoutGroup("Deactivate OnDeath", expanded: false)]
        [SerializeField][Required] private Transform awarenessTransform;

        [FoldoutGroup("Deactivate OnDeath", expanded: false)]
        [SerializeField][Required] private Transform actionTransform;

        [FoldoutGroup("Deactivate OnDeath", expanded: false)]
        [SerializeField][Required] private Transform particleTransform;

        [FoldoutGroup("Quest Defeatable", expanded: true)]
        [ShowInInspector, ReadOnly] private bool questDefeatable;

        [FoldoutGroup("Quest Defeatable", expanded: true)]
        [ShowIf("questDefeatable")] private MQuestDefeatable mQuestDefeatable;

        private void Awake()
        {
            currentHealth = maxHealth;
            healthBar.ConfigureHealthbar(maxHealth, npcName);
        }

        public void TakeDamage(int damage, Transform origin)
        {
            // Guard clause to ensure no damage is recognised after the killing blow
            if (currentHealth <= 0) return;

            // Calculate and set new health
            int newHealth = currentHealth - damage;
            currentHealth = newHealth;

            // Update health bar
            healthBar.SetHealth(Mathf.Clamp(newHealth, 0, maxHealth));

            // Instantiate effect popup showing amount of damage taken
            Transform popup = Instantiate(effectPopup, transform);
            popup.position = spawnPosition.position;
            popup.GetComponent<MEffectPopup>().Initialisation(ActionType.Positive, "-" + damage.ToString());

            // Instantiate blood effect
            Instantiate(meatParticles, transform.position, Quaternion.identity);
            Instantiate(bloodParticles, transform.position, Quaternion.identity);
            Instantiate(bloodSplashParticles, particleTransform.position, Quaternion.identity);

            // Initiate death if health <= 0 or consider taking action
            if (newHealth <= 0) Die(origin);
            else stateMachine.TakeActionAfterReceivingDamage(origin);
        }

        public void AddHealth(int health)
        {
            // Calculate and set new health
            int newHealth = Mathf.Clamp(currentHealth + health, 0, maxHealth);
            currentHealth = newHealth;

            // Update health bar
            healthBar.SetHealth(newHealth);

            // Instantiate effect popup showing amount of damage taken
            Transform popup = Instantiate(effectPopup, transform);
            popup.position = spawnPosition.position;
            popup.GetComponent<MEffectPopup>().Initialisation(ActionType.Negative, "+" + health.ToString());
        }

        public void RegisterQuestDefeatable(MQuestDefeatable component)
        {
            questDefeatable = true;
            mQuestDefeatable = component;
        }

        private void Die(Transform origin)
        {
            if (questDefeatable)
                mQuestDefeatable.ProcessInformation(origin);

            healthBar.gameObject.SetActive(false);
            stateMachine.Die();
            actionTransform.gameObject.SetActive(false);
            awarenessTransform.gameObject.SetActive(false);
            particleTransform.gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
