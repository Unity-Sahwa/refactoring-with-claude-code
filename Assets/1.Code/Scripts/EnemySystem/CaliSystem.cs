using UnityEngine;

namespace Refactoring
{
    public class CaliSystem : MonoBehaviour
    {
        [SerializeField] private int maxPaintOver = 3;
        public int MaxPaintOver => maxPaintOver;
        public float paintOver { get; private set; }

        private InkColor? _lastColor;

        // 색이 바뀔 때마다 스택을 쌓는다(같은 색 연타는 무시). 덧칠 처형 게이지.
        public void Painting(InkColor color, float value)
        {
            if (_lastColor != null && _lastColor != color)
                paintOver = Mathf.Min(paintOver + value, maxPaintOver);

            _lastColor = color;
        }

        // 덧칠이 최대치인가. 최대치 설정이 0이면(작동 안 함) 무조건 false.
        public bool IsPaintOverMax() => maxPaintOver > 0 && paintOver >= maxPaintOver;

        public void ResetPaint()
        {
            paintOver = 0;
            _lastColor = null;
        }
    }
}