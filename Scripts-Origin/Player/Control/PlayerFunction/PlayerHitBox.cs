using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    private bool stopHitboxCoroutine = false;

    [SerializeField] private GameObject[] humanNormalAttackHitBox;
    [SerializeField] private GameObject[] animalNormalAttackHitBox;
    //[SerializeField] private GameObject animalLeapStrikeHitBox;
    public void Initialize()
    {
        stopHitboxCoroutine=true;
    }

    //타격 횟수, 간격  추가해야함
    public IEnumerator TogglePlayerHitBox(HitBoxStruct hitboxStruct)
    {
        if (!hitboxStruct.useFunction)
        {
            yield break;
        }

        stopHitboxCoroutine = false;

        float coroutineStartTime = Time.time;
        GameObject hitBox = SelectHitBox(hitboxStruct);

        hitBox.GetComponent<MeshRenderer>().enabled = hitboxStruct.showHitbox;

        #region While 변수
        bool activeHitBoxOnce = false; 
        bool isHitBoxActive = false;
        #endregion

        while (true)
        {
            if (stopHitboxCoroutine)
            {
                //끝까지 재생안한다면 코루틴 중지시 함께 중지
                if (!hitboxStruct.untilFinish)
                {
                    hitBox.SetActive(false);

                    yield break;
                }
            }
            if (Time.time >= coroutineStartTime + hitboxStruct.waitTime + hitboxStruct.duration)
            {
                hitBox.SetActive(false);
                yield break;
            }
            else if (!activeHitBoxOnce && Time.time >= coroutineStartTime + hitboxStruct.waitTime)
            {
                hitBox.SetActive(false);
                hitBox.SetActive(true);
                activeHitBoxOnce = true;
                isHitBoxActive = true;
            }

            if (isHitBoxActive)
            {
                //다음평타 히트박스 차례가 활성화됨.
                if (!hitBox.activeSelf)
                {
                    yield break;
                }
            }

            yield return null;
        }
    }

    private GameObject SelectHitBox(HitBoxStruct hitboxStruct)
    {
        GameObject hitBox = null;

        switch (hitboxStruct.hitBoxType)
        {
            case HitBoxType.HUMAN_NORMALATTACK_FIRST:
                hitBox = humanNormalAttackHitBox[0];
                break;
            case HitBoxType.HUMAN_NORMALATTACK_SECOND:
                hitBox = humanNormalAttackHitBox[1];
                break;
            case HitBoxType.HUMAN_NORMALATTACK_THIRD:
                hitBox = humanNormalAttackHitBox[2];
                break;
            case HitBoxType.ANIMAL_NORMALATTACK_FIRST:
                hitBox = animalNormalAttackHitBox[0];
                break;
            case HitBoxType.ANIMAL_NORMALATTACK_SECOND:
                hitBox = animalNormalAttackHitBox[1];
                break;
            case HitBoxType.ANIMAL_NORMALATTACK_THIRD:
                hitBox = animalNormalAttackHitBox[2];
                break;
        }

        return hitBox;
    }
}
