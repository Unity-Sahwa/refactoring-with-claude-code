using System;
using System.Collections.Generic;
using UnityEngine;

// PlayerStateManager.Initialize() 대신 Awake/Start로 초기화 타이밍을 분리한다.
// 이벤트 구독은 Start()에서 수행 (설계 문제점: 외부 컴포넌트 초기화 순서 의존 참고)
namespace Refactoring
{
    public class PlayerStateManager : StateManager<PlayerStateType>, IInjectRequester, IStateContext
    {
        public List<Type> TargetTypes => new List<Type> { typeof(IStateDataProvider), typeof(InputEventBroadcaster) };

        public IStateDataProvider StateDataManager {get; private set;}
        private InputEventBroadcaster _inputBroadcaster;

        public Animator HAnimator => hAnimator;
        [SerializeField] private Animator hAnimator;
        public Animator AAnimator => aAnimator;
        [SerializeField] private Animator aAnimator;

        public PlayerStateEventChannel EventChannel => eventChannel;
        [SerializeField] PlayerStateEventChannel eventChannel;

        private void Awake()
        {
            foreach (PlayerStateType key in Enum.GetValues(typeof(PlayerStateType)))
            {
                states[key] = null;
            }
        }

        protected override void Start()
        {
            BaseState<PlayerStateType>.BindContext(this);

            CreateStatesFromReflection();
            
            //테스트
            _inputBroadcaster.OnInputPressed += Test_OnInputPressed;
            if(states[PlayerStateType.HIdle] != null) CurrentState = states[PlayerStateType.HIdle];

            base.Start();
        }

        public void Inject(Dictionary<Type, List<object>> targets)
        {
            if (targets.TryGetValue(typeof(IStateDataProvider), out var list))
            {
                StateDataManager = list[0] as IStateDataProvider;
            }

            if (targets.TryGetValue(typeof(InputEventBroadcaster), out var broadcasters))
                _inputBroadcaster = broadcasters[0] as InputEventBroadcaster;
        }
        
        private void CreateStatesFromReflection()
        {
            var baseType = typeof(BaseState<PlayerStateType>);

            foreach (var type in AssemblyCache.AllTypes)
            {
                //BaseState<EPlayerState>를 상속한 구체 클래스만 통과시키는 필터
                if (type.IsClass && //클래스만
                    !type.IsAbstract && //추상 클래스 제외
                    !type.IsGenericTypeDefinition && //타입인자가 확정된 제네릭인가
                    baseType.IsAssignableFrom(type)) //type이 baseType을 상속/구현했는가?
                {
                    var instance = (BaseState<PlayerStateType>)Activator.CreateInstance(type);
                    states[instance.StateKey] = instance;
                }
            }
        }

        private void Test_OnInputPressed(InputActionType actionType)
        {
            if(actionType == InputActionType.NormalAttack)
            {
                if(CurrentState == states[PlayerStateType.HNormalAttack1])
                {
                    nextStateKey = PlayerStateType.HNormalAttack2;
                }
                else if(CurrentState == states[PlayerStateType.HNormalAttack2])
                {
                    nextStateKey = PlayerStateType.HNormalAttack3;
                }
                else
                {
                    nextStateKey = PlayerStateType.HNormalAttack1;
                }
            }
            else if (actionType == InputActionType.SpecialAttack)
            {
                CurrentState = states[ PlayerStateType.HSpecialAttack];
                nextStateKey = PlayerStateType.HSpecialAttack;
            }
            else if (actionType == InputActionType.FinishAttack)
            {
                CurrentState = states[ PlayerStateType.HFinishAttack];
                nextStateKey = PlayerStateType.HFinishAttack;
            }
        }
    }
}