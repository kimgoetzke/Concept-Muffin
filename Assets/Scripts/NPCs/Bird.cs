using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace CaptainHindsight
{
    public class Bird : MonoBehaviour
    {
        [ShowInInspector, ProgressBar(1, 10)] int timeToFlyOff = 2;
        [ShowInInspector, ProgressBar(0, 60, 1, 1, 0)] int midAirDelay = 2;
        [ShowInInspector, ProgressBar(1, 20)] int timeToLandAgain = 5;

        private Animator animator;
        private Vector3 currentPosition;
        private bool isFacingRight;
        private float previouslyTriggered = 0;
        private float swarmDifferentiator;
        private Material material;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            isFacingRight = GetComponent<SpriteRenderer>().flipX;
            material = GetComponent<SpriteRenderer>().material;
            currentPosition = transform.position;
        }

        private void Start()
        {
            // Set animation to idle
            animator.SetBool("Fly", false);

            // Set random colour
            Color[] colours = ScriptableObjectsDirector.Instance.Colours[0].Colours;
            int randomColour = UnityEngine.Random.Range(0, colours.Length);
            material.SetColor("_Colour", colours[randomColour]);

            // Set random differentiator to delay/speed up actions
            swarmDifferentiator = UnityEngine.Random.Range(-0.7f, 0.7f);

            //Helper.Log("[" + transform.parent.parent.name + "] Random colour selected: " + randomColour + "/" + (colours.Length - 1) + " - swarmDif " + swarmDifferentiator + " .");
        }

        // Animation event only
        private void PlayNextIdleAnimation()
        {
            animator.SetInteger("Idle", UnityEngine.Random.Range(0, 5));
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.CompareTag("Player") || collider.gameObject.CompareTag("Bullet"))
            {
                animator.SetBool("Fly", true);
                FlyOff();
                LandAgain();
            }
        }

        private async void FlyOff()
        {
            float randomX = UnityEngine.Random.Range(-15f, 15f);
            float randomZ = UnityEngine.Random.Range(-10f, 0f);
            if (previouslyTriggered > 0f) randomX = 0f; // Don't change X, if previously triggered to avoid sprite flipping issues


            Vector3 destination = new Vector3(randomX, 15f, randomZ);
            SetSpriteDirection(randomX);
            previouslyTriggered = Time.fixedTime;

            await Task.Delay(System.TimeSpan.FromSeconds(Mathf.Abs(swarmDifferentiator / 2)));
            AudioDirector.Instance.Play("Bird-Small_flapping");
            transform.DOLocalMove(destination, timeToFlyOff + swarmDifferentiator).SetEase(Ease.InSine);
            //Helper.Log("[Bird] Flying off to " + destination + ".");
        }

        private async void LandAgain()
        {
            try
            {
                // Delay landing
                await Task.Delay(System.TimeSpan.FromSeconds(timeToFlyOff + midAirDelay + (swarmDifferentiator * 2)));

                // Flip sprite and fly back
                FlipSpriteOnXAxis();
                transform.DOMove(currentPosition, timeToLandAgain + swarmDifferentiator).SetEase(Ease.InOutSine);
                await Task.Delay(System.TimeSpan.FromSeconds(timeToLandAgain + swarmDifferentiator));

                // Switch to idle animation if landing was expected
                float expectedLandingTime = previouslyTriggered + timeToFlyOff + midAirDelay + timeToLandAgain + (swarmDifferentiator * 3);
                float actualTime = Time.fixedTime;
                if (actualTime >= expectedLandingTime - 0.5f)
                {
                    animator.SetBool("Fly", false);
                    //Helper.Log("[Bird] Landing again. Expected landing: " + expectedLandingTime + " vs actual: " + actualTime + ".");
                }
                //else Helper.Log("[" + transform.parent.parent.name + "] Landing animation requested but request ignored due. Expected landing: " + expectedLandingTime + " vs actual: " + actualTime + ".");
            }
            catch
            {
                //Helper.Log("[Bird] Object no longer exists. Landing cancelled.");
            }
        }

        private void SetSpriteDirection(float moveX)
        {
            if (moveX > 0f && !isFacingRight) FlipSpriteOnXAxis();
            else if (moveX < 0f && isFacingRight) FlipSpriteOnXAxis();
        }

        private void FlipSpriteOnXAxis()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            bool currentSettting = spriteRenderer.flipX;
            spriteRenderer.flipX = !currentSettting;
            isFacingRight = !isFacingRight;
        }
    }
}
