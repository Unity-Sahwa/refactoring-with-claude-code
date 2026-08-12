using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Refactoring
{
    // 역할: UIEffectTarget을 받아서 그 수치대로 효과를 돌리는 실행기. 누가 언제 부를지는 모름.
    public class UIEffectRunner : MonoBehaviour
    {
        /// <summary> 대원_STUDY: Array를 Dictionary로 바꾸기. ToDictionary(...) 정의 이해하기
        /// 매개변수 this IEnumerable<TSource> source : AAA.ToDictionary(...)에서 AAA에 해당. 자동으로 가져감
        /// 매개변수 Func<TSource, TKey> keySelector : key를 찾는 방법. element => element.ConfigType 을 통해 key(logic은 source의 요소 중 하나)
        /// Dictionary의 value는 따로 안적으면 element가 들어감
        /// </summary>
        private readonly Dictionary<Type, IUIEffectLogic> _logics =
            new IUIEffectLogic[] { new Shake(), new FadeInOut(), new FadeIn(), new FadeOut() }
                .ToDictionary(element => element.ConfigType);
        private readonly List<Playing> _playing = new();

        private struct Playing
        {
            public UIEffectTarget Target;
            public float Elapsed;
            public IUIEffectLogic Logic;
        }

        private void OnDisable()
        {
            // 그냥 비우면 UI가 어긋난 자리에 굳고 로직에도 항목이 남음. 하나씩 끝내서 원위치로 되돌림.
            for (int i = _playing.Count - 1; i >= 0; i--)
            {
                Finish(i);
            }
        }

        public void Play(UIEffectTarget target)
        {
            if (!TryGetLogic(target, out IUIEffectLogic logic))
            {
                return;
            }
            if (target.Config.Duration <= 0f)
            {
                Debug.LogWarning($"{target.name}: Duration이 0 이하라 재생하지 않음. 인스펙터 확인 요망");
                return;
            }

            var playing = new Playing { Target = target, Elapsed = 0f, Logic = logic };

            int index = FindPlaying(target);
            if (index < 0) // 아직 안 돌고 있음
            {
                target.Target.SetActive(true); // 시작할 때 무조건 활성화.
                logic.Begin(target.Target, target.Config);
                _playing.Add(playing);
                return;
            }
            _playing[index] = playing; // 이미 도는 중이면 처음부터 다시
        }

        public void Stop(UIEffectTarget target)
        {
            int index = FindPlaying(target);
            if (index < 0)
            {
                return;
            }

            Finish(index);
        }

        private void Update()
        {
            // 뒤에서부터 읽기 때문에 도중에 끝난 효과를 list에서 지워도 for 문은 문제없이 돌아감.
            for (int i = _playing.Count - 1; i >= 0; i--)
            {
                Playing playing = _playing[i];
                playing.Elapsed += Time.unscaledDeltaTime;

                float duration = playing.Target.Config.Duration;
                if (playing.Elapsed >= duration)
                {
                    Finish(i);
                    continue;
                }

                _playing[i] = playing; // Elapsed가 변했기 때문에 다시 할당
                playing.Logic.Tick(playing.Target.Target, playing.Target.Config, playing.Elapsed / duration);
            }
        }

        private int FindPlaying(UIEffectTarget target)
        {
            for (int i = 0; i < _playing.Count; i++)
            {
                if (_playing[i].Target == target) return i;
            }
            return -1;
        }

        private void Finish(int index)
        {
            Playing playing = _playing[index];
            UIEffectConfig config = playing.Target.Config;

            playing.Logic.End(playing.Target.Target, config);
            if (config.DeactivateOnStop)
            {
                playing.Target.Target.SetActive(false);
            }
            _playing.RemoveAt(index);
        }

        // 부르는 쪽이 전부 "있어야 정상"인 상황이라 실패하면 무조건 배선 오류로 보고 경고함.
        private bool TryGetLogic(UIEffectTarget target, out IUIEffectLogic logic)
        {
            logic = null;
            if (target == null)
            {
                Debug.LogWarning("UIEffectRunner: 대상이 비어있음. 인스펙터 확인 요망");
                return false;
            }
            if (target.Config == null)
            {
                Debug.LogWarning($"{target.name}: config가 비어있음. 인스펙터에서 효과 종류 선택 요망");
                return false;
            }
            if (!_logics.TryGetValue(target.Config.GetType(), out logic))
            {
                Debug.LogWarning($"{target.name}: {target.Config.GetType().Name}을 처리할 로직이 없음. UIEffectRunner의 _logics에 추가 요망");
                return false;
            }
            return true;
        }
    }
}
