using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffect : MonoBehaviour
{
    [Header("?????? ????")]
    [SerializeField] private PlayerHumanMaskData humanData;

    [Space(20)]
    [Header("????? ???????")]

    [Space(10)]
    [SerializeField] private GameObject[] inkHitEffect;
    public GameObject[] InkHitEffect
    {
        get
        {
            return inkHitEffect;
        }
    }

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

    //Vector3 effectPosition ?? ??????? ???? ?????? ?????? ????
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

        //?????? ??????? ??? ??????
        //?????? ???????? ????????? >>> ??????? ???
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

        //????? ????

        #region while ????
        bool activeEffectOnce = false;
        #endregion

        while (true)
        {
            //effect.followTarget == true -> ??????? ????? ??? ??????
            //effect.followTarget == false -> ??? ????, ???? followTarget = false
            
            //effectStruct.untilFinish == true -> ????? ?????? ????©£????? ????? ????
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
            
            //????©£? ???¨¨? ??????
            if (Time.time >= effectStartTime + effectStruct.duration)
            {
                skillEffect.SetActive(false);
                yield break;
            }

            //?????©£? ?? ????? ????
            else if  (Time.time >= effectStartTime)
            {
                if (!activeEffectOnce || following)
                {
                    skillEffect.transform.localScale = effectStruct.scale;

                    //??????????? ?¡À???? ??????? ???? offset ???????
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

    //????? ??? ???????? ???????(?????? ??????)
    public IEnumerator TogglePlayerEffect(EffectStruct effectStruct, GameObject[] effects, GameObject positionObject)
    {
        if (!effectStruct.useFunction)
        {
            yield break;
        }

        float coroutineStartTime = Time.time;

        stopEffectCoroutine = false;

        //?????? ??????? ??? ??????
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

        //????? ????

        #region while ????
        bool followPosition = true;
        bool activeTransformOnce = false;

        bool activeEffectOnce = false;

        //??????????? ??? ???????? ???? ???
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

            //????©£? ?????? ???? ????
            else if (Time.time >= coroutineStartTime + effectStruct.waitTime)
            {
                if (!activeEffectOnce || 
                    (effectStruct.followPosition && 
                    ((Time.time >= coroutineStartTime + effectStruct.followWaitTime) && Time.time <= coroutineStartTime + effectStruct.followWaitTime + effectStruct.followDuration)))
                {
                    skillEffect.transform.localScale = effectStruct.scale;
                    
                    positionObject.transform.localPosition = effectStruct.position;
                    skillEffect.transform.position = positionObject.transform.position;

                    //????? ????? ????? ??????? ????????? ??????????
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
        //?????? ??????? ??? ??????
        //?????? ???????? ????????? >>> ??????? ???
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

        //??????? ?? ?????????? ???? ???
        if (skillEffect == null)
        {
            yield break;
        }
        #endregion

        bool activeEffectOnce = false;

        while (true)
        {
            //effect.followTarget == true -> ??????? ????? ??? ??????
            //effect.followTarget == false -> ??? ????, ???? followTarget = false
            //effectStruct.untilFinish == true -> ????? ?????? ????©£????? ????? ????

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

            //????©£? ???¨¨? ??????
            if (Time.time >= effectStartTime + effectStruct.duration)
            {
                skillEffect.SetActive(false);
                yield break;
            }

            //?????©£? ?? ????? ????
            else if (Time.time >= effectStartTime)
            {
                if (!activeEffectOnce || following)
                {
                    skillEffect.transform.localScale = effectStruct.scale;

                    //??????????? ?¡À???? ??????? ???? offset ???????
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
