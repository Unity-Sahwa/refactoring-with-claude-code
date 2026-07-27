using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Refactoring
{
    // 역할: 외부에서 UI 효과 id를 받으면 그에 맞는 효과를 정해진 수치만큼 동작하는 실행기.
    public class UIEffectRunner : MonoBehaviour
    {
        [Inject] private UIEffectChannel _channel;
        [Inject(true)] private UIEffectCatalog _catalog;
        [Inject(true)] private List<UIEffectTarget> _targets;
        [SerializeField] private Transform _canvas; // UI 프리팹이 생성되는 캔버스
        private readonly Dictionary<UIEffectId, (GameObject target, UIEffectConfig config)> _effects = new();

        /// <summary> 대원_STUDY: Array를 Dictionary로 바꾸기. ToDictionary(...) 정의 이해하기
        /// 매개변수 this IEnumerable<TSource> source : AAA.ToDictionary(...)에서 AAA에 해당. 자동으로 가져감
        /// 매개변수 Func<TSource, TKey> keySelector : key를 찾는 방법. element => element.ConfigType 을 통해 key(logic은 source의 요소 중 하나)
        /// Dictionary의 value는 따로 안적으면 element가 들어감
        /// </summary>
        private readonly Dictionary<Type, IUIEffectLogic> _logics =
            new IUIEffectLogic[] { new Shake(), new FadeInOut() }.ToDictionary(element => element.ConfigType);
        private readonly List<Playing> _playing = new();

        private struct Playing
        {
            public UIEffectId Id;
            public float Elapsed;
            public GameObject Target;
            public UIEffectConfig Config;
            public IUIEffectLogic Logic;
        }
        private IDisposable _effectDisposable; //알림 등록이면서 등록 해제가능한 타입

        private void Awake()
        {
            CreateCatalogUI();
            RegisterTargets();
        }
        private void OnEnable()
        {
            if (_channel == null)
            {
                Debug.LogError("UIEffectRunner에 UIEffectChannel 주입 실패");
                return;
            }

            _effectDisposable = _channel.Register(HandlePlay, HandleStop);
        }
        private void OnDisable()
        {
            _effectDisposable?.Dispose();
            _effectDisposable = null;

            // 그냥 비우면 UI가 어긋난 자리에 굳고 로직에도 항목이 남음. 하나씩 끝내서 원위치로 되돌림.
            for (int i = _playing.Count - 1; i >= 0; i--)
            {
                Finish(i);
            }
        }
        private void RegisterTargets()
        {
            if (_targets == null) 
            {
                return;
            }

            foreach (UIEffectTarget target in _targets)
            {
                if (target == null || target.Id == UIEffectId.None) 
                {
                    continue;
                }

                AddEffect(target.Id, target.Target, target.Config);
            }
        }
        private void AddEffect(UIEffectId id, GameObject target, UIEffectConfig config)
        {
            if (_effects.ContainsKey(id))
            {
                Debug.LogWarning($"{id}: 이미 등록된 id. 나중 것으로 덮어씀");
            }

            _effects[id] = (target, config);
        }
        private void CreateCatalogUI()
        {
            if (_catalog == null || _catalog.Entries == null || _canvas == null)
            {
                Debug.LogWarning("UIEffectRunner: 카탈로그 UI 프리팹을 생성하지 못함");
                return;
            }

            // 캔버스에 카탈로그에 있는 UI 프리팹 생성
            foreach (UIEffectCatalog.Entry e in _catalog.Entries)
            {
                if (e == null || e.Id == UIEffectId.None || e.Prefab == null) continue;
                GameObject instance = Instantiate(e.Prefab, _canvas);
                instance.SetActive(false);
                AddEffect(e.Id, instance, e.Config);
            }
        }
        private void Update()
        {
            // 뒤에서부터 읽기 때문에 도중에 끝난 효과를 list에서 지워도 for 문은 문제없이 돌아감.
            for (int i = _playing.Count - 1; i >= 0; i--)
            {
                Playing playing = _playing[i];
                playing.Elapsed += Time.unscaledDeltaTime;

                if (playing.Elapsed >= playing.Config.Duration)
                {
                    Finish(i);
                    continue;
                }

                _playing[i] = playing; // Elapsed가 변했기 때문에 다시 할당
                float progress = playing.Elapsed / playing.Config.Duration;
                playing.Logic.Tick(playing.Target, playing.Config, progress);
            }
        }

        private void HandlePlay(UIEffectId id)
        {
            if (!TryGetEffect(id, out (GameObject target, UIEffectConfig config) entry, out IUIEffectLogic logic))
            {
                return;
            }
            if (entry.config.Duration <= 0f)
            {
                Debug.LogWarning($"{id}: Duration이 0 이하라 재생하지 않음. 인스펙터 확인 요망");
                return;
            }

            var playing = new Playing
            {
                Id = id,
                Elapsed = 0f,
                Target = entry.target,
                Config = entry.config,
                Logic = logic
            };

            int index = FindPlaying(id);
            if (index < 0) //id가 플레이되고 있지 않음.
            {
                entry.target.SetActive(true); // 시작할 때 무조건 활성화.
                logic.Begin(entry.target, entry.config);
                _playing.Add(playing);
                return;
            }
            _playing[index] = playing; // 이미 도는 중이면 처음부터 다시
        }

        private void HandleStop(UIEffectId id) 
        {
            int index = FindPlaying(id);
            if (index >= 0) 
            {
                return;
            }

            Finish(index);
        }

        private int FindPlaying(UIEffectId id)
        {
            for (int i = 0; i < _playing.Count; i++)
            {
                if (_playing[i].Id == id) return i;
            }
            return -1;
        }

        private void Finish(int index)
        {
            Playing playing = _playing[index];

            playing.Logic.End(playing.Target, playing.Config);
            if (playing.Config.DeactivateOnStop)
            {
                playing.Target.SetActive(false);
            }
            _playing.RemoveAt(index);
        }

        // 부르는 쪽이 전부 "있어야 정상"인 상황이라 실패하면 무조건 배선 오류로 보고 경고함.
        private bool TryGetEffect(UIEffectId id, out (GameObject target, UIEffectConfig config) entry, out IUIEffectLogic logic)
        {
            logic = null;
            if (!_effects.TryGetValue(id, out entry))
            {
                Debug.LogWarning($"{id}: 등록되지 않은 효과. 카탈로그나 씬의 UIEffectTarget 확인 요망");
                return false;
            }
            if (entry.config == null)
            {
                Debug.LogWarning($"{id}: config가 비어있음. 인스펙터에서 효과 종류 선택 요망");
                return false;
            }
            if (!_logics.TryGetValue(entry.config.GetType(), out logic))
            {
                Debug.LogWarning($"{id}: {entry.config.GetType().Name}을 처리할 로직이 없음. UIEffectRunner의 _logics에 추가 요망");
                return false;
            }
            return true;
        }
    }
}
