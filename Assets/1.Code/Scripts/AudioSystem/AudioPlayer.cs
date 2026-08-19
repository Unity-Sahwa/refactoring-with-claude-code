using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Refactoring
{
    // 역할: 오디오 채널의 재생/정지 요청을 받아 실제로 소리를 낸다.
    //       창구(AudioSource) 풀에서 꺼내 재생하고, 정지 요청 시 그 id 창구를 멈춘다.
    //       카탈로그에서 소리 정의를 찾을 뿐, 무엇을 언제 왜 트는지는 모른다(요청받을 뿐).
    public class AudioPlayer : MonoBehaviour
    {
        [Inject] private AudioChannel _audioChannel;
        [Inject] private AudioCatalog _audioCatalog;
        [SerializeField] private AudioMixerGroup _sfxGroup;
        [SerializeField] private int _initialVoices = 8;

        private readonly List<AudioSource> _pool = new();    // 사용 가능한 오디오 소스
        private readonly List<ActiveVoice> _actives = new(); // 재생 중인 창구(정지를 위한 추적용 i로 쓰임)
        private IDisposable _audioEventDiposable;

        private class ActiveVoice
        {
            public AudioId Id;
            public AudioSource Source;
        }

        private void Awake()
        {
            for (int i = 0; i < _initialVoices; i++)
            {
                _pool.Add(CreateVoice("SFXVoice"));
            }
        }

        private void OnEnable() 
        {
            _audioEventDiposable = _audioChannel.Register(HandlePlay, HandleStop);
        }
        private void OnDisable() 
        {
            _audioEventDiposable?.Dispose();
        }

        private void Update()
        {
            // 활성화된 오디오 소스 중 플레이 끝난 것은 제거
            for (int i = _actives.Count - 1; i >= 0; i--)
            {
                if (_actives[i].Source.isPlaying) continue;
                ReturnVoice(_actives[i].Source);
                _actives.RemoveAt(i);
            }
        }

        private void HandlePlay(AudioPlayRequest request)
        {
            if (!_audioCatalog.TryGet(request.Id, out AudioCatalogEntry entry) || entry.Clips == null || entry.Clips.Length == 0) //단락평가
            {
                return;
            } 

            AudioSource audioSource = RentVoice();
            Apply(audioSource, entry);
            audioSource.outputAudioMixerGroup = entry.Output != null ? entry.Output : _sfxGroup;

            Transform t = audioSource.transform;
            if (request.Follow != null)
            {
                t.SetParent(request.Follow, false);
                t.localPosition = Vector3.zero;
            }
            else
            {
                t.SetParent(transform, false);
                if (request.HasPosition) t.position = request.Position;
            }

            audioSource.Play();
            _actives.Add(new ActiveVoice { Id = request.Id, Source = audioSource });
        }

        // 그 id로 울리는 창구를 모두 멈춘다
        private void HandleStop(AudioId id)
        {
            for (int i = _actives.Count - 1; i >= 0; i--)
            {
                if (_actives[i].Id != id) continue;
                ReturnVoice(_actives[i].Source);
                _actives.RemoveAt(i);
            }
        }

        private static void Apply(AudioSource source, AudioCatalogEntry entry)
        {
            source.clip = PickClip(entry.Clips);
            source.volume = entry.Volume;
            source.pitch = entry.Pitch;
            source.spatialBlend = entry.SpatialBlend;
            source.minDistance = entry.MinDistance;
            source.maxDistance = entry.MaxDistance;
            source.loop = entry.Loop;
        }

        // 클립이 여럿이면 그 중 랜덤 하나
        private static AudioClip PickClip(AudioClip[] clips)
        {
            if (clips.Length == 1) return clips[0];
            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }

        private AudioSource RentVoice()
        {
            AudioSource audioSource;
            if (_pool.Count > 0)
            {
                //pool에서 가져오고, pool에서 삭제
                audioSource = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
            }
            else
            {
                // 모자라면 오디오소스 오브젝트 늘림
                audioSource = CreateVoice("SFXVoice"); 
            }
            return audioSource;
        }

        private void ReturnVoice(AudioSource audioSource)
        {
            //사용했던 오디오소스 원위치 시키고, pool에 넣어 사용 가능하게 만들기
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.transform.SetParent(transform, false);
            _pool.Add(audioSource);
        }

        private AudioSource CreateVoice(string voiceName)
        {
            var gameObject = new GameObject(voiceName);
            gameObject.transform.SetParent(transform, false);
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }
    }
}
