namespace Refactoring
{
    public interface ISaveService
    {
        bool Save<T>(T data) where T : ISaveData;
        T Load<T>() where T : ISaveData;

        // 세이브 슬롯처럼 같은 모양의 데이터를 여러 벌 두는 경우. 파일 이름 뒤에 칸 번호가 붙는다.
        bool Save<T>(T data, int slot) where T : ISaveData;
        T Load<T>(int slot) where T : ISaveData;
        bool Delete<T>(int slot) where T : ISaveData;
        bool Exists<T>(int slot) where T : ISaveData;
    }
}
