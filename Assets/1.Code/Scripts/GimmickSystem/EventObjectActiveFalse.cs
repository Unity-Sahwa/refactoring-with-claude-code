namespace Refactoring
{
    public class EventObjectActiveFalse : EventData
    {
        public override void Execute()
        {
            gameObject.SetActive(false);
        }
    }
}

