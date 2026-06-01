namespace Refactoring
{
    public interface ISaveFileHandler
    {
        bool Write(string fileName, string data);
        string Read(string fileName);
        bool Delete(string fileName);
        bool Exists(string fileName);
    }
}
