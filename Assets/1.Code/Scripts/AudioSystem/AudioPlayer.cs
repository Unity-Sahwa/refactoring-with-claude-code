using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: AudioChannel의 재생/정지 요청을 받아 실제로 소리를 낸다. (무엇을 언제 트는지는 요청하는 쪽 담당)
    // 흐름: 요청 → 카탈로그에서 정의 조회 → 창구(AudioSource) 풀에서 꺼내 재생 → 정지 요청 시 그 id 창구를 멈추고 반환
    //대원TODO: 거의 플레이어 효과음 재생하는 용도임. 배경음이나 다른 용도의 효과음은 각자가 재생함. 다음 프로젝트에서는 한곳에서 재생하도록 관리하기. 그러면 기믹이나 여러 음악 어떻게 재생되는지 모아서 기록할 필요가 있음.
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

        //대원TODO: 이벤트 함수들 순서가 어떻게 되지? Awake이전부터 자세히 알아보자. 이제 익숙해져서 괜찮을 듯. OnEnable 타이밍 까먹음
        //대원TODO: 등록을 Awake()에서 하는 곳도 있고 OnEnable()에서 하는 곳도 있음. 통일되는지 확인하고, 통일안되면 왜 그타이밍에 해야하는지 알아보자.
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
            //for문 역순으로 한 것은 _actives 요소가 중간에 삭제되어도 반납검사가 생략되지 않도록 하기 위함
            //대원TODO: 매 프레임 for문은 에바인가?? 활성화/비활성화 방식? / 시작과 동시에 타이머? / LateUpdate, Update, FixedUpdate 전문으로 하는 업데이터 클래스를 하나둘까?
            //대원TODO: 이렇게 하면만 만약 _actives 요소가 하나 사라진다면, 똑같은 요소를 한번더 시행하는 것이 되는거 아닌가
            for (int i = _actives.Count - 1; i >= 0; i--)
            {
                AudioSource source = _actives[i].Source;

                // 따라가던 대상이 파괴되면 소스도 같이 사라진다. 풀에 되돌리지 않고 버린다.
                if (source == null)
                {
                    //대원TODO: 여기다가 i 순서 바꾸고 for문을 정방향으로 하는게 가독성 높지 않을까?
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

        //대원TODO: AI는 왜 static을 좋아하는가
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

        //대원TODO: ApplyEntry에 넣고 지우기
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
