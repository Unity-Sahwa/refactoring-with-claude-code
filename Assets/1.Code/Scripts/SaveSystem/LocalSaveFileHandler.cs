using System.IO;
using UnityEngine;

namespace Refactoring
{
    // 책임: 저장 폴더 안의 json 파일 하나를 읽고 쓰고 지운다.
    public class LocalSaveFileHandler : ISaveFileHandler
    {
        private readonly string _directory;

        public LocalSaveFileHandler(string subDirectory)
        {
            _directory = Path.Combine(Application.persistentDataPath, subDirectory);

            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
        }

        public bool Write(string fileName, string data)
        {
            string filePath = GetFilePath(fileName);
            string tempPath = filePath + ".tmp";

            // 쓰는 도중에 게임이 꺼져도 원본이 반토막 나지 않도록, 임시 파일에 다 쓴 뒤 이름만 바꾼다.
            try
            {
                File.WriteAllText(tempPath, data);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.Move(tempPath, filePath);
                return true;
            }
            catch
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                return false;
            }
        }

        public string Read(string fileName)
        {
            string filePath = GetFilePath(fileName);

            if (!File.Exists(filePath))
            {
                return null;
            }

            return File.ReadAllText(filePath);
        }

        public bool Delete(string fileName)
        {
            string filePath = GetFilePath(fileName);

            if (!File.Exists(filePath))
            {
                return false;
            }

            File.Delete(filePath);
            return true;
        }

        public bool Exists(string fileName) => File.Exists(GetFilePath(fileName));

        private string GetFilePath(string fileName) => Path.Combine(_directory, fileName + ".json");
    }
}
