#ifndef SEE_THROUGH_GLOBALS_INCLUDED
#define SEE_THROUGH_GLOBALS_INCLUDED

// 셰이더 그래프 프로퍼티는 Exposed를 꺼도 머티리얼이 값을 소유해서 Shader.SetGlobal이 먹지 않는다.
// 그래서 머티리얼이 개입할 수 없는 전역 변수를 여기서 직접 선언해 읽는다.
float _SeeThroughSize;
float _SeeThroughOpacity;
float _SeeThroughEdgeSoftness;
float4 _SeeThroughPosition;

// Position: 플레이어의 화면 좌표(0~1). Depth: 카메라에서 플레이어까지의 거리.
void GetSeeThrough_float(out float Size, out float2 Position, out float Depth, out float Opacity, out float EdgeSoftness)
{
    Size = _SeeThroughSize;
    Position = _SeeThroughPosition.xy;
    Depth = _SeeThroughPosition.z;
    Opacity = _SeeThroughOpacity;
    EdgeSoftness = _SeeThroughEdgeSoftness;
}

#endif
