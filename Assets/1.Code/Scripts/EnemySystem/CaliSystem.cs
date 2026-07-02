using UnityEngine;

namespace Refactoring
{
    public class CalliSystem : MonoBehaviour
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

        public void ResetPaint()
        {
            paintOver = 0;
            _lastColor = null;
        }
    }
}