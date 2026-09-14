using System;

namespace Refactoring
{
    // 책임: 마우스 감도 창구. 설정창은 여기에 쓰고, 카메라는 여기서 읽고 알림을 듣는다.
    public interface IMouseSettings
    {
        event Action OnChanged;

        float SpeedX { get; set; }
        float SpeedY { get; set; }
    }
}
