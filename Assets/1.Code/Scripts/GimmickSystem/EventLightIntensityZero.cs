using UnityEngine;

namespace Refactoring
{
    // 역할: 이벤트 발생 시 지정한 Light를 끄지 않고 intensity만 0으로 만든다.
    // (완전히 끄면 셰이더 keyword가 바뀌어 GPU program 재컴파일이 발생해 오히려 렉 유발됨)
    public class EventLightIntensityZero : EventData
    {
        [SerializeField] private Light _light;

        public override void Execute()
        {
            if (_light == null) return;

            _light.intensity = 0f;
        }
    }
}
