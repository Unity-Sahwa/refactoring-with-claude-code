namespace Refactoring
{
    // 히트박스 판정 영역의 모양. 크기는 데이터의 Scale로 읽는다.
    // Box    : Scale = 가로·세로·높이
    // Sphere : Scale.x = 지름 (y·z는 안 씀)
    // Capsule: Scale.x = 지름, Scale.y = 전체 높이, 축은 로컬 Y (눕히려면 Rotation으로 돌린다)
    public enum HitboxShape
    {
        Box = 0,
        Sphere,
        Capsule,
    }
}
