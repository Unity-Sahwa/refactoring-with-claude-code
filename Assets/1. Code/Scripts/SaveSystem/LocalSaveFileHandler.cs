using System.IO;
using UnityEngine;

namespace Refactoring
{
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
