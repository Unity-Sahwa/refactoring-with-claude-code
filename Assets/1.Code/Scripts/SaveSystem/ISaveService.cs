namespace Refactoring
{
    public interface ISaveService
    {
        bool Save<T>(T data) where T : ISaveData;
        T Load<T>() where T : ISaveData;
    }
}
