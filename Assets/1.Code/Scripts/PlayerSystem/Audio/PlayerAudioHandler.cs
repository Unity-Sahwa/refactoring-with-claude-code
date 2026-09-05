using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 플레이어 이벤트(상태 및 타격 발생)를 받아 오디오 채널에 소리를 요청함.
    public class PlayerAudioHandler : MonoBehaviour
    {
        [Preserve, Inject] private IPlayerStateEventSubscriber _eventSubscriber;
        // 소리 재생·정지를 요청하기 위함
        [Preserve, Inject] private AudioChannel _audioChannel;
        // 타격 이벤트를 받기 위함
        [Preserve, Inject] private HitChannel _hitChannel;

        private readonly List<IPlayerAudio> _started = new();
        private IDisposable _audioEventDisposable;
        private IDisposable _hitDisposable;

        private void Awake()
        {
            _audioEventDisposable = _eventSubscriber.Register(StateEventCategory.Audio, HandlePlay, HandleReset);
            _hitDisposable = _hitChannel.Register(HandleHit);
        }

        private void HandlePlay(IStartData data)
        {
            if (data is not IPlayerAudio audio)
            {
                Debug.LogError($"[PlayerAudioHandler] IPlayerAudio가 필요한데 {data?.GetType().Name ?? "null"}을 받음");
                return;
            }

            SoundType id = audio.Id;
            if (id == SoundType.None)
            {
                return;
            }

            _audioChannel.RaisePlay(AudioPlayRequest.Create(id));
            // 나중에 정지시키기 위해 시작한 소리를 추적한다.
            _started.Add(audio);
        }

        private void HandleReset(CloseEventType reason)
        {
            for (int i = 0; i < _started.Count; i++)
            {
                if (!_started[i].UntilFinish)
                {
                    _audioChannel.RaiseStop(_started[i].Id);
                }
            }
            _started.Clear();
        }

        // 타격 성공 시 그 지점에서 타격음 재생(한 번 나고 마니 정지 추적 안 함).
        private void HandleHit(HitReport hitReport)
        {
            if (hitReport.Sound == SoundType.None)
            {
                return;
            }
            _audioChannel.RaisePlay(AudioPlayRequest.CreateAt(hitReport.Sound, hitReport.Point));
        }

        private void OnDestroy()
        {
            _audioEventDisposable?.Dispose();
            _hitDisposable?.Dispose();
        }
    }
}
