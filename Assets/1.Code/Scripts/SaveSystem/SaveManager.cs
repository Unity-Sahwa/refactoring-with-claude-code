using System.Reflection;
using UnityEngine;

namespace Refactoring
{
    // 책임: 저장 데이터를 json으로 바꿔 파일에 읽고 쓴다. (파일 입출력 자체는 LocalSaveFileHandler 담당)
    public class SaveManager : MonoBehaviour, ISaveService
    {
        private ISaveFileHandler _fileHandler;

        // Awake에서 만들지 않는다. 같은 씬 안에서 누구 Awake가 먼저 도는지는 정해져 있지 않아서,
        // 다른 오브젝트가 자기 Awake에서 Load를 부르면 아직 비어 있어 터진다(InputHub가 그랬다).
        // 필드 자리에서도 못 만든다. 유니티가 그때는 저장 폴더 경로를 못 읽게 막는다.
        // 그래서 처음 쓸 때 만든다. 순서에 안 기대고, 언제 불려도 준비돼 있다.
        //
        // DontDestroyOnLoad를 쓰지 않는 이유:
        // 이 클래스는 씬을 넘어 기억할 값이 없다. 진짜 내용은 전부 파일에 있고, 들고 있는 건 폴더 경로뿐이다.
        // 살려두면 살아남은 옛것과 새 씬에 놓인 사본이 둘 다 주입 대상으로 등록돼서 엉뚱한 쪽이 주입될 수 있다.
        // SaveSlotManager, SlotLoadRunner도 같은 이유로 오브젝트를 안 살린다.
        private ISaveFileHandler FileHandler => _fileHandler ??= new LocalSaveFileHandler("SaveData");

        public bool Save<T>(T data) where T : ISaveData
        {
            string fileName = GetFileName<T>();
            string json = JsonUtility.ToJson(data);

            if (string.IsNullOrEmpty(json) || json == "{}")
            {
                Debug.LogWarning($"[SaveManager] {typeof(T).Name}의 직렬화 결과가 비어있습니다. [Serializable] 어트리뷰트를 확인하세요.");
                return false;
            }

            return FileHandler.Write(fileName, json);
        }

        public T Load<T>() where T : ISaveData
        {
            string fileName = GetFileName<T>();
            string json = FileHandler.Read(fileName);

            if (json == null)
            {
                return default;
            }

            return JsonUtility.FromJson<T>(json);
        }

        // 아래 넷은 세이브 슬롯용. 파일 이름 뒤에 칸 번호를 붙이는 것 말고는 위와 똑같다.
        public bool Save<T>(T data, int slot) where T : ISaveData
        {
            string json = JsonUtility.ToJson(data);

            if (string.IsNullOrEmpty(json) || json == "{}")
            {
                Debug.LogWarning($"[SaveManager] {typeof(T).Name}의 직렬화 결과가 비어있습니다. [Serializable] 어트리뷰트를 확인하세요.");
                return false;
            }

            return FileHandler.Write(GetFileName<T>() + slot, json);
        }

        public T Load<T>(int slot) where T : ISaveData
        {
            string json = FileHandler.Read(GetFileName<T>() + slot);

            if (json == null)
            {
                return default;
            }

            return JsonUtility.FromJson<T>(json);
        }

        public bool Delete<T>(int slot) where T : ISaveData
        {
            return FileHandler.Delete(GetFileName<T>() + slot);
        }

        public bool Exists<T>(int slot) where T : ISaveData
        {
            return FileHandler.Exists(GetFileName<T>() + slot);
        }

        private string GetFileName<T>() where T : ISaveData
        {
            FieldInfo field = typeof(T).GetField("FileName");

            if (field != null)
            {
                return (string)field.GetValue(null);
            }

            return typeof(T).Name;
        }
    }
}
