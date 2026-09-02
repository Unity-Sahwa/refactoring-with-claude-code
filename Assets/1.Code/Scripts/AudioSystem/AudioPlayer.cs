using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: AudioChannel의 재생/정지 요청을 받아 실제로 소리를 낸다. (무엇을 언제 트는지는 요청하는 쪽 담당)
    // 흐름: 요청 → 카탈로그에서 정의 조회 → 창구(AudioSource) 풀에서 꺼내 재생 → 정지 요청 시 그 id 창구를 멈추고 반환
    public class AudioPlayer : MonoBehaviour
    {
        [Preserve, Inject] private AudioChannel _audioChannel;
        [Preserve, Inject] private AudioCatalog _audioCatalog;
        private const int _initialVoices = 8;

        private readonly List<AudioSource> _pool = new();

        // 재생 중인 창구. 정지 요청이 오면 여기서 그 id를 찾는다.
        private readonly List<ActiveVoice> _actives = new();
        private IDisposable _audioEventDisposable;

        private void Awake()
        {
            for (int i = 0; i < _initialVoices; i++)
            {
                _pool.Add(CreateVoice());
            }
        }

        private void OnEnable()
        {
            _audioEventDisposable = _audioChannel.Register(HandlePlay, HandleStop);
        }

        private void OnDisable()
        {
            _audioEventDisposable?.Dispose();
        }

        private void Update()
        {
            for (int i = _actives.Count - 1; i >= 0; i--)
            {
                AudioSource source = _actives[i].Source;

                // 따라가던 대상이 파괴되면 소스도 같이 사라진다. 풀에 되돌리지 않고 버린다.
                if (source == null)
                {
                    _actives.RemoveAt(i);
                    continue;
                }

                if (source.isPlaying)
                {
                    continue;
                }

                ReturnVoice(source);
                _actives.RemoveAt(i);
            }
        }

        private void HandlePlay(AudioPlayRequest request)
        {
            if (!_audioCatalog.TryGet(request.Id, out AudioCatalogEntry entry))
            {
                Debug.LogWarning($"[AudioPlayer] 카탈로그에 {request.Id} 항목이 없음", this);
                return;
            }

            if (entry.Clips == null || entry.Clips.Length == 0)
            {
                Debug.LogWarning($"[AudioPlayer] {request.Id} 항목에 클립이 비어 있음", this);
                return;
            }

            AudioSource audioSource = RentVoice();
            ApplyEntry(audioSource, entry);
            audioSource.outputAudioMixerGroup = entry.Output;

            PlaceVoice(audioSource, request);

            audioSource.Play();
            _actives.Add(new ActiveVoice { Id = request.Id, Source = audioSource });
        }

        private void HandleStop(SoundType id)
        {
            for (int i = _actives.Count - 1; i >= 0; i--)
            {
                if (_actives[i].Id != id)
                {
                    continue;
                }

                ReturnVoice(_actives[i].Source);
                _actives.RemoveAt(i);
            }
        }

        // 따라갈 대상이 있으면 그 자식으로 붙이고, 없으면 요청 지점에 둔다.
        private void PlaceVoice(AudioSource audioSource, AudioPlayRequest request)
        {
            Transform voiceTransform = audioSource.transform;

            if (request.Follow != null)
            {
                voiceTransform.SetParent(request.Follow, false);
                voiceTransform.localPosition = Vector3.zero;
                return;
            }

            voiceTransform.SetParent(transform, false);

            if (request.HasPosition)
            {
                voiceTransform.position = request.Position;
            }
        }

        private static void ApplyEntry(AudioSource source, AudioCatalogEntry entry)
        {
            source.clip = PickClip(entry.Clips);
            source.volume = entry.Volume;
            source.pitch = entry.Pitch;
            source.spatialBlend = entry.SpatialBlend;
            source.minDistance = entry.MinDistance;
            source.maxDistance = entry.MaxDistance;
            source.loop = entry.Loop;
        }

        private static AudioClip PickClip(AudioClip[] clips)
        {
            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }

        private AudioSource RentVoice()
        {
            if (_pool.Count == 0)
            {
                return CreateVoice();
            }

            AudioSource audioSource = _pool[_pool.Count - 1];
            _pool.RemoveAt(_pool.Count - 1);

            return audioSource;
        }

        private void ReturnVoice(AudioSource audioSource)
        {
            //소스가 파괴된 경우 바로 종료
            if (audioSource == null) 
            {
                return;
            }

            audioSource.Stop();
            audioSource.clip = null;

            // 위치를 제자리로 옮겨 대상과 함께 파괴되는 것을 방지
            audioSource.transform.SetParent(transform, false);
            _pool.Add(audioSource);
        }

        private AudioSource CreateVoice()
        {
            GameObject voice = new GameObject("SFXVoice");
            voice.transform.SetParent(transform, false);

            AudioSource source = voice.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        // 재생 중인 창구 하나. private 중첩 타입이라 필드를 그대로 노출한다.
        private class ActiveVoice
        {
            public SoundType Id;
            public AudioSource Source;
        }
    }
}
