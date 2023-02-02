namespace CaptainHindsight
{
    public enum ActionType
    {
        Damage,
        Health,
        Positive,
        Negative,
        Other
    }

    public enum GameState
    {
        Tutorial,
        Play,
        Pause,
        GameOver,
        Win,
        Transition,
        Menu,
        Error
    }

    public enum CollectableQuestIdentifier
    {
        // Intended to be usef by quest system to mark items as collectables
    }
}