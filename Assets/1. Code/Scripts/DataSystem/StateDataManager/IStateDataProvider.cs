namespace Refactoring
{
    public interface IStateDataProvider
    {
        T GetData<T>(PlayerStateType stateType) where T : class;
    }
}