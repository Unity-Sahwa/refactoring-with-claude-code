namespace Refactoring
{
    // 세이브 포인트 하나하나의 이름표. 앞 숫자는 씬, 뒤 숫자는 그 씬 안에서의 순서.
    // 세이브 포인트 기믹이 인스펙터에서 이 값을 고르고, 같은 지점이면 같은 슬롯에 덮어쓴다.
    //
    // 이름을 번역 표의 UI_Env_ 뒤 글자와 똑같이 맞춰 뒀다.
    // 그래서 화면에 지역 이름을 찍을 때 "UI_Env_" + 이 이름으로 표에서 바로 꺼내 쓴다.
    // 지점이 늘면 표에 한 줄 넣고 여기에 같은 이름을 한 줄 더 적으면 된다.
    public enum SavePointType
    {
        Area0_0,
        Area0_1,
        Area0_2,

        Area1_0,
        Area1_1,

        Area2_0,
        Area2_1,
        Area2_2,
        Area2_3,
        Area2_4,

        Area3_0,
        Area3_1,
        Area3_2,
        Area3_3,
        Area3_4,

        Area4_0,
    }
}
