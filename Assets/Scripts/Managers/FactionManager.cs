using Sirenix.OdinInspector;
using UnityEngine;

namespace CaptainHindsight
{
    public class FactionManager : MonoBehaviour
    {
        public static FactionManager Instance;

        [SerializeField]
        [BoxGroup("RIGID"), HideLabel, PropertySpace(SpaceAfter = 10)]
        private FactionGroup RIGID;

        [SerializeField]
        [BoxGroup("CorpX"), HideLabel, PropertySpace(SpaceAfter = 10)]
        private FactionGroup CorpX;

        [SerializeField]
        [BoxGroup("Dinosaur"), HideLabel, PropertySpace(SpaceAfter = 10)] 
        private FactionGroup Dinosaur;

        [SerializeField]
        [BoxGroup("Other"), HideLabel, PropertySpace(SpaceAfter = 10)] 
        private FactionGroup Unspecified;

        [HideInInspector]
        private bool firstCallBlocked;

        #region Unity Editor methods
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                if (firstCallBlocked == false)
                {
                    firstCallBlocked = true;
                    return;
                }
                EventManager.Instance.ChangeFactions();
                Helper.Log("[FactionManager] FactionsGroup(s) have/has changed during playmode so all instances of FactionIdentity were updated.");
            }
        }
        #endregion

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

        public FactionGroup GetFactionGroup(Faction faction)
        {
            FactionGroup group;
            switch (faction)
            {
                case Faction.RIGID: group = RIGID;
                    break;
                case Faction.CorpX: group = CorpX;
                    break;
                case Faction.Dinosaur: group = Dinosaur;
                    break;
                default: group = Unspecified;
                    break;
            }
            return group;
        }

        [System.Serializable]
        public class FactionGroup
        {
            [EnumToggleButtons, LabelWidth(75)]
            public FactionStatus Player;
            [EnumToggleButtons, LabelWidth(75)]
            public FactionStatus Dinosaur;
            [EnumToggleButtons, LabelWidth(75)]
            public FactionStatus RIGID;
            [EnumToggleButtons, LabelWidth(75)]
            public FactionStatus CorpX;
            [EnumToggleButtons, LabelWidth(75)]
            public FactionStatus Unspecified;
        }

        public enum Faction
        {
            Unspecified,
            Player,
            Dinosaur,
            RIGID,
            CorpX
        }

        public enum FactionStatus
        {
            Hostile,
            Neutral,
            Ally,
            None
        }
    }
}
