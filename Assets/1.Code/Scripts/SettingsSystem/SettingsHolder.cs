using System;
using UnityEngine;

namespace Refactoring
{
    // 설정값 주인들이 똑같이 하는 일만 모아둔 부모.
    // 파일에서 읽기, 파일에 쓰기, "값 바뀜" 알리기 셋뿐이고 어떤 값인지는 자식이 정한다.
    public abstract class SettingsHolder<TData> : MonoBehaviour, ISettingsHolder
        where TData : class, ISaveData, new()
    {
        // 값이 바뀌면 쏜다. 쓰는 쪽(카메라·오디오 등)이 구독해서 자기 것만 갱신한다.
        public event Action OnChanged;

        [Inject(true)] private ISaveService _saveService;

        private TData _data;

        // 처음 값을 물어볼 때 파일에서 한 번만 읽는다.
        // 미리 읽어두면 누가 먼저 깨어나느냐에 따라 빈 값을 읽는 사고가 난다.
        protected TData Data
        {
            get
            {
                if (_data == null)
                {
                    _data = _saveService?.Load<TData>() ?? new TData();
                }

                return _data;
            }
        }

        private void Awake()
        {
            // 씬이 바뀌어도 설정은 남아야 한다.
            DontDestroyOnLoad(gameObject);
        }

        public void Save()
        {
            _saveService?.Save(Data);
        }

        protected void NotifyChanged()
        {
            OnChanged?.Invoke();
        }
    }
}
