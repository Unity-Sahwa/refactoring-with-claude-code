using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: 상태 이벤트 구독자에게 전달될 데이터를 한곳에 모아 카테고리별로 꺼내준다.
    //       (한곳에 모은 이유는 상태마다 연출을 조정하기 쉬우려고. 안 쓰는 항목은 Editor에서 가린다)
    [CreateAssetMenu(fileName = "StateData", menuName = "Data/StateData")]
    public class StateData : ScriptableObject
    {
        [Tooltip("상태 정체성. 자식 클래스 override 대신 인스펙터에서 지정")]
        [SerializeField] private PlayerStateType _stateType;
        [SerializeField] private bool _isLooping;
        [Tooltip("이 상태를 다시 사용하려면 마지막 진입 후 최소 이 시간(초)이 지나야 한다. 스킬 연타 방지용")]
        [SerializeField] private float _cooldown;

        [Space(10f)]
        [SerializeField] private IntervalDataEntry[] _inputBlock;
        [SerializeField] private IntervalDataEntry[] _inputBuffer;
        [SerializeField] private IntervalDataEntry[] _moveControl;
        [SerializeField] private IntervalDataEntry[] _rotateControl;
        [SerializeField] private IntervalDataEntry[] _superArmor;
        [SerializeField] private IntervalDataEntry[] _invincible;
        [SerializeField] private IntervalDataEntry[] _cameraLock;
        [SerializeField] private SkillMoveDataEntry[] _skillMove;

        [SerializeField] private SkillEffectDataEntry[] _effect;
        [SerializeField] private HitboxDataEntry[] _hitbox;
        [SerializeField] private AudioDataEntry[] _audio;
        [SerializeField] private CameraShakeDataEntry[] _cameraShake;
        [SerializeField] private CameraZoomDataEntry[] _cameraZoom;
        [SerializeField] private FinishDataEntry[] _finish;
        [SerializeField] private ObjectToggleDataEntry[] _objectToggle;

        private Dictionary<StateEventCategory, Array> _dataMap;

        public PlayerStateType StateType => _stateType;
        public bool IsLooping => _isLooping;
        public float Cooldown => _cooldown;

        private void OnEnable() => BuildDataMap();

#if UNITY_EDITOR
        private void OnValidate() => BuildDataMap();
#endif

        // OnEnable보다 GetData가 먼저 불릴 수 있어 두 곳에서 호출한다.
        private void BuildDataMap()
        {
            _dataMap = new()
            {
                [StateEventCategory.InputBlock] = _inputBlock,
                [StateEventCategory.InputBuffer] = _inputBuffer,
                [StateEventCategory.SkillMove] = _skillMove,
                [StateEventCategory.MoveControl] = _moveControl,
                [StateEventCategory.RotateControl] = _rotateControl,
                [StateEventCategory.Effect] = _effect,
                [StateEventCategory.Hitbox] = _hitbox,
                [StateEventCategory.Audio] = _audio,
                [StateEventCategory.CameraShake] = _cameraShake,
                [StateEventCategory.CameraZoom] = _cameraZoom,
                [StateEventCategory.SuperArmor] = _superArmor,
                [StateEventCategory.Invincible] = _invincible,
                [StateEventCategory.CameraLock] = _cameraLock,
                [StateEventCategory.Finish] = _finish,
                [StateEventCategory.ObjectToggle] = _objectToggle,
            };
        }

        public Dictionary<StateEventCategory, T[]> GetData<T>()
        {
            // OnEnable이 아직 안 불렸을 때를 대비한다.
            if (_dataMap == null)
            {
                BuildDataMap();
            }

            var result = new Dictionary<StateEventCategory, T[]>();

            foreach (var (category, array) in _dataMap)
            {
                if (array == null || array.Length == 0)
                {
                    continue;
                }

                if (array is T[] typed)
                {
                    result[category] = typed;
                }
            }
            return result;
        }
    }
}