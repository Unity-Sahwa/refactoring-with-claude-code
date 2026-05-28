<!-- 피드백
[제목]
- 수정된 본문부터 작성하고 제목 작성할 것

[본문]
- (무엇을)인터페이스 교체. IInjectRequester, IInjectTarget > IInterfaceInjectable | (왜)해당 클래스는 DI에 의존하고, DI 방식이 교체되었기 때문
- (무엇을)입력신호 이벤트를 보유하는 IInputEventProvider을 구현 | (왜) 외부에서 의존성 주입으로 가져가도록 하기 위함.
- (무엇을)Awake()에서 인스턴스가 해당 객체가 아니면 파괴되는 조건 추가 | (왜)기존의 instance를 유지하기 위해

- Convention 추가: 위와 같이 '무엇을', '왜'를 괄호 구조로 나타내기. 또한 괄호로 표시하는게 가독성에 괜찮은지 평가가 필요함.
- Convention 추가: 메서드 언급이 필요할 경우 작성(메서드명과 괄호만 작성. 반환타입,제네릭, 매개변수 등 작성 X). 가급적 코드는 적지말고 무엇을 했는지 작성
- 문제:IInputEventProvider은 의존성 주입과는 관련이 없음. 관련없는 내용을 관련있는 것처럼 작성하는 문제가 보임.
- 문제: 의존성 주입 문제는 의존성 주입끼리 따로 commit 해야하지 않을까?

-->

refactor(InputEventBroadcaster_cs): DI 방식을 IInterfaceInjectable로 교체

IInjectRequester·IInjectTarget 기반 주입 방식을 IInterfaceInjectable·IInputEventProvider 기반으로 교체.
Inject() 메서드를 제거하고 injectedImplements 딕셔너리로 의존성을 직접 보유하도록 변경.
싱글톤 중복 체크 조건을 `_instance != this`로 보강.
