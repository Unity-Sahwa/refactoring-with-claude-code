namespace Refactoring
{
    public interface IAudioCatalog
    {
        bool TryGet(SoundType id, out AudioCatalogEntry entry);
    }
}
