namespace CaptainHindsight
{
    public enum NPCType
    {
        Unspecified,
        Dinosaurs,
        Humanoid
    }

    public enum NPCQuestIdentifier
    {
        Unspecified,
        Fukuiraptor
    }

    public enum NPCMovement
    {
        Idle,
        Wander,
        Patrol
    }

    public enum NPCBehaviour
    {
        Anxious, // Observe -> Flee
        Neutral, // Observe -> Move
        Indifferent, // Idle -> Idle
        Observant, // Observe -> Watch
        Defensive, // Observe -> Attack
        Aggressive // Investigate -> Attack
    }

    public enum NPCStateLock
    {
        Full,
        Partial,
        Off
    }

    public enum NPCAnimationTrigger
    {
        Attack,
        Unspecified
    }
}