using System.Reflection;
using UnityEngine;

namespace Refactoring
{
    public class SaveManager : MonoBehaviour, ISaveService
    {
        private ISaveFileHandler _fileHandler;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _fileHandler = new LocalSaveFileHandler("SaveData");
        }

        public bool Save<T>(T data) where T : ISaveData
        {
            string fileName = GetFileName<T>();
            string json = JsonUtility.ToJson(data);

            if (string.IsNullOrEmpty(json) || json == "{}")
            {
                Debug.LogWarning($"[SaveManager] {typeof(T).Name}의 직렬화 결과가 비어있습니다. [Serializable] 어트리뷰트를 확인하세요.");
                return false;
            }

            return _fileHandler.Write(fileName, json);
        }

        public T Load<T>() where T : ISaveData
        {
            string fileName = GetFileName<T>();
            string json = _fileHandler.Read(fileName);

            if (json == null)
            {
                return default;
            }

            return JsonUtility.FromJson<T>(json);
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
