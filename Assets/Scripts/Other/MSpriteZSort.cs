using UnityEngine;
using UnityEngine.Rendering;

namespace CaptainHindsight
{
    public class MSpriteZSort : MonoBehaviour
    {
        [SerializeField] private SortSettings settings = SortSettings.StaticSprite;
        [SerializeField, Tooltip("Specify where th Sprite Renderer or Sorting Group component for the calculation is located")] 
            private bool findComponentInChild;
        [SerializeField, Tooltip("Use a transform that is not the pivot for the basis of the calculation")] 
            private Transform sortOffsetMarker;
        [SerializeField, Tooltip("Add an order layer after the calculation i.e. in addition to and after using any sortOffsetMarker")] 
            private int additionalLayerOffset;
        private SortingGroup sortingGroup;
        private SpriteRenderer spriteRenderer;
        private ParticleSystemRenderer particleSystemRenderer;
        private float sortingOffset;

        private void Awake()
        {
            switch (settings)
            {
                case SortSettings.StaticSprite:
                    if (findComponentInChild) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                    else spriteRenderer = GetComponent<SpriteRenderer>();
                    spriteRenderer.sortingOrder = transform.GetSortingOrder() + additionalLayerOffset;
                    this.enabled = false;
                    break;
                case SortSettings.StaticSortingGroup:
                    if (findComponentInChild) sortingGroup = GetComponentInChildren<SortingGroup>();
                    else sortingGroup = GetComponent<SortingGroup>();
                    sortingGroup.sortingOrder = transform.GetSortingOrder() + additionalLayerOffset;
                    this.enabled = false;
                    break;
                case SortSettings.StaticParticleSystem:
                    if (findComponentInChild) particleSystemRenderer = GetComponentInChildren<ParticleSystemRenderer>();
                    else particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
                    particleSystemRenderer.sortingOrder = transform.GetSortingOrder() + additionalLayerOffset;
                    this.enabled = false;
                    break;
                case SortSettings.DynamicSpriteWithNoOffset:
                    if (findComponentInChild) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                    else spriteRenderer = GetComponent<SpriteRenderer>();
                    break;
                case SortSettings.DynamicSpriteUsingMarker:
                    if (findComponentInChild) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                    else spriteRenderer = GetComponent<SpriteRenderer>();
                    sortingOffset = -sortOffsetMarker.position.y;
                    break;
                case SortSettings.DynamicSortGroupWithNoOffset:
                    if (findComponentInChild) sortingGroup = GetComponentInChildren<SortingGroup>();
                    else sortingGroup = GetComponent<SortingGroup>();
                    break;
                case SortSettings.DynamicSortGroupUsingMarker:
                    if (findComponentInChild) sortingGroup = GetComponentInChildren<SortingGroup>();
                    else sortingGroup = GetComponent<SortingGroup>();
                    sortingOffset = -sortOffsetMarker.position.y;
                    break;
                default:
                    Helper.LogError("[MSpriteZSort] " + transform.name + " (child of " + transform.parent.name + ") uses MSpriteZSort but its settings are invalid. Find the component and update its settings.");
                    this.enabled = false;
                    break;
            }
        }

        private void Update()
        {
            switch (settings)
            {
                case SortSettings.DynamicSpriteWithNoOffset:
                    spriteRenderer.sortingOrder = transform.GetSortingOrder() + additionalLayerOffset;
                    break;
                case SortSettings.DynamicSpriteUsingMarker:
                    spriteRenderer.sortingOrder = transform.GetSortingOrder(sortingOffset) + additionalLayerOffset;
                    break;
                case SortSettings.DynamicSortGroupWithNoOffset:
                    sortingGroup.sortingOrder = transform.GetSortingOrder() + additionalLayerOffset;
                    break;
                case SortSettings.DynamicSortGroupUsingMarker:
                    sortingGroup.sortingOrder = transform.GetSortingOrder(sortingOffset) + additionalLayerOffset;
                    break;
                default:
                    Helper.LogError("[MSpriteZSort] " + transform.name + " (child of " + transform.parent.name + ") uses MSpriteZSort but its settings are invalid. The component was disabled.");
                    this.enabled = false;
                    break;
            }
        }
    }

    public enum SortSettings
    {
        StaticSprite,
        StaticSortingGroup,
        StaticParticleSystem,
        DynamicSpriteWithNoOffset,
        DynamicSpriteUsingMarker,
        DynamicSortGroupWithNoOffset,
        DynamicSortGroupUsingMarker,
    }
}
