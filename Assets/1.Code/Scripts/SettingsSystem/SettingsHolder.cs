using System;
using UnityEngine;

namespace Refactoring
{
    // 역할 : 데이터를 불러오기 / 저장하기 / 데이터 변경시 알림을 해주는 중간다리 역할
    public abstract class SettingsHolder<TData> : MonoBehaviour, ISettingsHolder where TData : class, ISaveData, new()
    {
        public event Action OnChanged; //값이 바뀔 때 호출됨. 외부에서 구독알림받음

        // AttributeInjector가 자식 클래스(MouseSettings 등)의 필드를 훑기 때문에, private로 할 경우 안보여서 주입 안됨 
        [Inject(true)] protected ISaveService _saveService;

        private TData _data;

        // 처음 값을 물어볼 때 파일에서 한 번만 읽는다.
        // 미리 읽어두면 누가 먼저 깨어나느냐에 따라 빈 값을 읽는 사고가 난다.
        protected TData Data
        {
            get
            {
                //데이터 없으면 불러오거나 새로 생성
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
