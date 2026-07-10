using UnityEngine;
using UnityEngine.Audio;

namespace Refactoring
{
    // 역할: 볼륨값을 외부에 주거나, 받아서 AudioMixer에 설정한다. 0~1의 볼륨값을 데시벨로 변환
    public class VolumeController : MonoBehaviour, IVolumeControl
    {
        // 오디오 믹서에서 노출시킨 파라미터 이름과 정확히 같아야 함.
        private const string MasterParam = "MasterVolume";
        private const string BgmParam = "BgmVolume";
        private const string SfxParam = "SfxVolume";

        private const float MinDb = -80f; // 믹서 최소값 = 완전 무음

        [SerializeField] private AudioMixer mixer;

        public void SetVolume(VolumeCategory category, float volume01)
        {
            volume01 = Mathf.Clamp01(volume01);
            mixer.SetFloat(ParamOf(category), LinearToDb(volume01));
        }

        public float GetVolume(VolumeCategory category)
        {
            if (mixer.GetFloat(ParamOf(category), out float db))
            {
                return DbToLinear(db);
            }
            return 0f;
        }

        private string ParamOf(VolumeCategory category) 
        {
            switch (category)
            {
                case VolumeCategory.Master: return MasterParam;
                case VolumeCategory.Bgm: return BgmParam;
                case VolumeCategory.Sfx: return SfxParam;
                default: return MasterParam;
            }
        }

        // 볼륨 범위는 -80dB ~ 0dB. 1을 넣으면 0dB(원본 소리), 0.0001을 넣으면 -80dB이 나옴. 
        private float LinearToDb(float v)
        {
            if (v <= 0.0001f) return MinDb; // 0 근처는 log 무한대라 최저로 표현
            return Mathf.Log10(v) * 20f;
        }

        private float DbToLinear(float db)
        {
            if (db <= MinDb) return 0f;
            return Mathf.Pow(10f, db / 20f);
        }
    }
}
