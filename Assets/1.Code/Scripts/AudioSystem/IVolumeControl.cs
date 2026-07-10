namespace Refactoring
{
    // 외부(설정, 저장 시스템 등)에서 볼륨 값을 받거나 설정할 수 있는 유일한 통로 
    public interface IVolumeControl
    {
        void SetVolume(VolumeCategory category, float volume01);
        float GetVolume(VolumeCategory category);
    }
}
