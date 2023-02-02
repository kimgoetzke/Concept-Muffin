namespace CaptainHindsight
{
    public interface IQuestCollectable
    {
        CollectableQuestIdentifier collectableIdentifier { get; }

        void Collect();
    }
}