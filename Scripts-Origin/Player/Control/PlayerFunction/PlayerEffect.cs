using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffect : MonoBehaviour
{
    [Header("데이터 수정")]
    [SerializeField] private PlayerHumanMaskData humanData;

    [Space(20)]
    [Header("이펙트 오브젝트")]
    //[SerializeField] private GameObject[] humanFirstNormalAttackEffect;
    //[SerializeField] private GameObject[] humanSecondNormalAttackEffect;
    //[SerializeField] private GameObject[] humanThirdNormalAttackEffect;
    //[SerializeField] private GameObject[] humanInkshapeEffect;
    //[SerializeField] private GameObject[] humanInkFloorEffect;
    
    //[Space(10)]
    //[SerializeField] private GameObject[] animalNormalAttackEffect;
    //[SerializeField] private GameObject[] animalLeapStrikeEffect;
    //[SerializeField] private GameObject[] animalRoarEffect;

    [Space(10)]
    [SerializeField] private GameObject[] inkHitEffect;
    public GameObject[] InkHitEffect
    {
        get
        {
            return inkHitEffect;
        }
    }


    //[Space(20)]
    //[Header("이펙트 포지션")]
    //[SerializeField] private Transform playerPosition;
    //public Transform PlayerPosition
    //{
    //    get
    //    {
    //        return playerPosition; 
    //    }
    //}

    //[SerializeField] private Transform humanWeaponPosition;
    //public Transform HumanWeaponPosition
    //{
    //    get
    //    {
    //        return humanWeaponPosition;
    //    }
    //}

    //[SerializeField] private Transform animalRightHandPosition;
    //public Transform AnimalRightHandPosition
    //{
    //    get
    //    {
    //        return animalRightHandPosition;
    //    }
    //}

    //[SerializeField] private Transform animalLeftHandPosition;
    //public Transform AnimalLeftHandPosition
    //{
    //    get
    //    {
    //        return animalLeftHandPosition;
    //    }
    //}

    //이펙트 코루틴 멈추기(untilFinish == true 제외)


    [SerializeField] private GameObject[] MaskEffect;


    private bool stopEffectCoroutine = false;



    private void Start()
    {
        humanData = PlayerHumanMaskData.Instance;
    }


    public void Initialize()
    {
        stopEffectCoroutine = true;
    }

    //Vector3 effectPosition 로 받으니까 벡터 값에서 변동은 없음
    public IEnumerator TogglePlayerHitEffect(EffectStruct effectStruct, GameObject[] effects, Vector3 hitPosition)
    {
        #region UseFuction
        if (!effectStruct.useFunction)
        {
            yield break;
        }
        #endregion

        float coroutineStartTime = Time.time;
        stopEffectCoroutine = false;

        //비활성화 오브젝트 하나 고르기
        //프리팹 생성으로 교체해야할듯 >>> 오브젝트 풀링
        GameObject skillEffect = null;

        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i].activeSelf)
            {
                continue;
            }
            else
            {
                skillEffect = effects[i];
                break;
            }
        }

        if (skillEffect == null)
        {
            yield break;
        }

        //이펙트 활성화

        #region while 변수
        bool activeEffectOnce = false;
        #endregion

        while (true)
        {
            //effect.followTarget == true -> 이펙트가 타겟을 계속 따라다님
            //effect.followTarget == false -> 한번 실행, 이후 followTarget = false
            
            //effectStruct.untilFinish == true -> 스킬이 끝나도 지속시간까지 이펙트 활성화
            if (stopEffectCoroutine)
            {
                if (!effectStruct.untilFinish)
                {
                    skillEffect.SetActive(false);
                    yield break;
                }
            }

            float effectStartTime = coroutineStartTime + effectStruct.waitTime;
            float followStartTime = coroutineStartTime + effectStruct.followWaitTime;
            bool following = effectStruct.followPosition && (Time.time >= followStartTime && Time.time <= followStartTime + effectStruct.followDuration);
            
            //지속시간 이후에 비활성화
            if (Time.time >= effectStartTime + effectStruct.duration)
            {
                skillEffect.SetActive(false);
                yield break;
            }

            //지연시간 후 이펙트 활성화
            else if  (Time.time >= effectStartTime)
            {
                if (!activeEffectOnce || following)
                {
                    skillEffect.transform.localScale = effectStruct.scale;

                    //히트지점에서 플레이어 정면방향 기준 offset 가능하게
                    skillEffect.transform.position =
                        hitPosition + Player.instance.transform.forward * effectStruct.position.x +
                        Player.instance.transform.up * effectStruct.position.y + 
                        Player.instance.transform.right * effectStruct.position.z;
                    

                    Vector3 totalRotation = Player.instance.transform.rotation.eulerAngles + effectStruct.rotation;
                    skillEffect.transform.rotation = Quaternion.Euler(totalRotation.x, totalRotation.y, totalRotation.z);

                    skillEffect.SetActive(true);

                    activeEffectOnce = true;
                }
            }

            yield return null;
        }
    }

    //이펙트 위치 변동에도 대처가능(출시하고 바꿔야함)
    public IEnumerator TogglePlayerEffect(EffectStruct effectStruct, GameObject[] effects, GameObject positionObject)
    {
        if (!effectStruct.useFunction)
        {
            yield break;
        }

        float coroutineStartTime = Time.time;

        stopEffectCoroutine = false;

        //비활성화 오브젝트 하나 고르기
        GameObject skillEffect = null;
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i].activeSelf)
            {
                continue;
            }
            else
            {
                skillEffect = effects[i];
                break;
            }
        }

        if (skillEffect == null)
        {
            yield break;
        }

        //이펙트 활성화

        #region while 변수
        bool followPosition = true;
        bool activeTransformOnce = false;

        bool activeEffectOnce = false;

        //로컬포지션에 계속 더해지는 문제 발생
        positionObject.transform.localPosition += effectStruct.position;
        positionObject.transform.localRotation = Quaternion.Euler(effectStruct.rotation.x, effectStruct.rotation.y, effectStruct.rotation.z);
        #endregion

        while (true)
        {
            if (stopEffectCoroutine)
            {
                if (!effectStruct.untilFinish)
                {
                    skillEffect.SetActive(false);
                    yield break;
                }
            }

            if (Time.time >= coroutineStartTime + effectStruct.waitTime + effectStruct.duration)
            {
                skillEffect.SetActive(false);
                yield break;
            }

            //지속시간 지나면 코루틴 중지
            else if (Time.time >= coroutineStartTime + effectStruct.waitTime)
            {
                if (!activeEffectOnce || 
                    (effectStruct.followPosition && 
                    ((Time.time >= coroutineStartTime + effectStruct.followWaitTime) && Time.time <= coroutineStartTime + effectStruct.followWaitTime + effectStruct.followDuration)))
                {
                    skillEffect.transform.localScale = effectStruct.scale;
                    
                    positionObject.transform.localPosition = effectStruct.position;
                    skillEffect.transform.position = positionObject.transform.position;

                    //이펙트 회전은 위치를 나타내는 오브젝트의 회전좌표에서
                    //Vector3 totalRotation = positionObject.transform.rotation.eulerAngles + effectStruct.rotaion;
                    //skillEffect.transform.rotation = Quaternion.Euler(totalRotation.x, totalRotation.y, totalRotation.z);
                    positionObject.transform.localRotation= Quaternion.Euler(effectStruct.rotation.x, effectStruct.rotation.y, effectStruct.rotation.z);
                    skillEffect.transform.rotation = positionObject.transform.rotation;

                    skillEffect.SetActive(true);

                    activeEffectOnce = true;
                }
            }
            
            yield return null;
        }
    }

    public IEnumerator TogglePlayerEffect(EffectStruct effectStruct, GameObject[] effects, Vector3 effectPosition)
    {
        #region UseFuction
        if (!effectStruct.useFunction)
        {
            yield break;
        }
        #endregion

        float coroutineStartTime = Time.time;
        stopEffectCoroutine = false;

        #region Select Inactive Effect
        //비활성화 오브젝트 하나 고르기
        //프리팹 생성으로 교체해야할듯 >>> 오브젝트 풀링
        GameObject skillEffect = null;
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i].activeSelf)
            {
                continue;
            }
            else
            {
                skillEffect = effects[i];
                break;
            }
        }

        //이펙트가 다 켜져있으면 코루틴 취소
        if (skillEffect == null)
        {
            yield break;
        }
        #endregion

        bool activeEffectOnce = false;

        while (true)
        {
            //effect.followTarget == true -> 이펙트가 타겟을 계속 따라다님
            //effect.followTarget == false -> 한번 실행, 이후 followTarget = false
            //effectStruct.untilFinish == true -> 스킬이 끝나도 지속시간까지 이펙트 활성화

            if (stopEffectCoroutine)
            {
                if (!effectStruct.untilFinish)
                {
                    skillEffect.SetActive(false);
                    yield break;
                }
            }

            float effectStartTime = coroutineStartTime + effectStruct.waitTime;
            float followStartTime = coroutineStartTime + effectStruct.followWaitTime;
            bool following = effectStruct.followPosition && (Time.time >= followStartTime && Time.time <= followStartTime + effectStruct.followDuration);

            //지속시간 이후에 비활성화
            if (Time.time >= effectStartTime + effectStruct.duration)
            {
                skillEffect.SetActive(false);
                yield break;
            }

            //지연시간 후 이펙트 활성화
            else if (Time.time >= effectStartTime)
            {
                if (!activeEffectOnce || following)
                {
                    skillEffect.transform.localScale = effectStruct.scale;

                    //히트지점에서 플레이어 정면방향 기준 offset 가능하게
                    skillEffect.transform.position = effectPosition;
                    skillEffect.SetActive(true);

                    activeEffectOnce = true;
                }
            }

            yield return null;
        }
    }

    #region PostProcessing
    public IEnumerator ShowHitVignette()
    {
        yield return null; 
    }
    #endregion
}
