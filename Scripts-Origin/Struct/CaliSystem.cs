using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Enemy;

public class CalliSystem : MonoBehaviour
{
    private List<char> paint = new List<char>();
    [SerializeField] private int maxPaintOver = 10;
    public int MaxPaintOver {  get { return maxPaintOver; } }
    private char lastColor = ' '; 

    public float paintWhite { get; private set; }
    public float paintBlack { get; private set; }
    public float paintOver { get; private set; }

    private void Awake()
    {
        // 적의 종류에 따라 maxPaintOver를 설정. 사실상 EnemyData에서 정해짐
        Enemy enemy = GetComponent<Enemy>();
        maxPaintOver = 3;
        //if (enemy != null)
        //{
        //    switch (enemy.enemyData)
        //    {
        //        case EnemyDataMelee meleeData:
        //            maxPaintOver = 3;
        //            break;
        //        case EnemyDataRange rangeData:
        //            maxPaintOver = 5;
        //            break;
        //        case EnemyDataHybird hybridData:
        //            maxPaintOver = 5;
        //            break;
        //        case EnemyDataBuffer bufferData:
        //            maxPaintOver = 5;
        //            break;
        //    }
        //}
        /* 수정전
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            switch (enemy.enemyType)
            {
                case EnemyType.Normal:
                    maxPaintOver = 5;
                    break;
                case EnemyType.Elite:
                    maxPaintOver = 10;
                    break;
                case EnemyType.Epic:
                    maxPaintOver = 20;
                    break;
                case EnemyType.MiddleBoss:
                    maxPaintOver = 30;
                    break;
                case EnemyType.MainBoss:
                    maxPaintOver = 30;
                    break;
            }
        }
         */
        
    }

    public void Painting(char color, float value)
    {
        //Debug.Log("이전 색 : " + (paint.Count > 0 ? paint[paint.Count - 1].ToString() : "없음") + " / 이후 색 : " + color);

        // 같은 색이 들어올 경우 paint에 추가만 함
        if (paint.Count > 0 && paint[paint.Count - 1] == color)
        {
            for (var i = 0; i < value; i++)
            {
                paint.Add(color);
            }
        }
        else
        {
            // 다른 색이 들어올 경우 덧칠 스택을 계산함
            float previousValue = paint.Count > 0 ? paint[paint.Count - 1] : 0;

            // 이전 색과 다른 색이 연속으로 들어올 때만 paintOver 증가
            if (paint.Count > 0 && lastColor != ' ' && lastColor != color)
            {
                paintOver += value;
                paintOver = Mathf.Min(paintOver, maxPaintOver);
            }

            for (var i = 0; i < value; i++)
            {
                paint.Add(color);
                if (paint.Count > maxPaintOver)
                {
                    paint.RemoveAt(0); // 큐처럼 FIFO 방식으로 제거
                }
            }
        }

        lastColor = color;
        UpdatePaint();
    }

    private void UpdatePaint()
    {
        paintWhite = 0;
        paintBlack = 0;

        foreach (var type in paint)
        {
            if (type == 'W')
            {
                paintWhite++;
            }
            if (type == 'B')
            {
                paintBlack++;
            }
        }
    }

    public float StackRatio() // (쌓인 스택 / 최대 스택)
    {
        return (float)paintOver / maxPaintOver;
    }

    public void ResetPaint()
    {
        paint.Clear();
        paintOver = 0;
        lastColor = ' ';
    }

    public bool IsPaintOverMax()
    {
        return paintOver >= maxPaintOver;
    }
}
