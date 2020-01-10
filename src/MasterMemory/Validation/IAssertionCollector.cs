namespace MasterMemory.Validation
{
    public interface IAssertionCollector
    {
        void AddFailed(string message);
    }
}
