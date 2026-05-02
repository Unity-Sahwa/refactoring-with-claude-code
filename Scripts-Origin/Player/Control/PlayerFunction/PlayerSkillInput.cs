using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillInput : MonoBehaviour
{
    [SerializeField] private PlayerState playerState;
    [SerializeField] private MaskChange maskChange;

    [SerializeField] private HumanMaskSkill humanSkill;
    [SerializeField] private AnimalMaskSkill animalSkill;
    [SerializeField] private GhostMaskSkill ghostSkill;

    [SerializeField] private Stack<PlayerStateType> playerSkillInputValue = new Stack<PlayerStateType>();
    public bool canStoreInputValue { get; private set; }
    private bool stopCoroutine = false;
    private Coroutine inputCoroutine;

    #region 입력키 저장 & 실행

    public void Initialize()
    {
        canStoreInputValue = false;
        playerSkillInputValue.Clear();
        stopCoroutine = true;
        inputCoroutine = null;  
    }

    public void StoreInput(PlayerStateType inputValue)
    {
        if (canStoreInputValue)
        {
            //스킬 중간에 저장 가능할 때 Push
            playerSkillInputValue.Push(inputValue);
        }
    }
    public void ProcessInput(PlayerSkillInputStruct skillInput, float startTime)
    {
        Initialize();

        if (inputCoroutine != null)
        {
            StopCoroutine(inputCoroutine);
        }

        inputCoroutine = StartCoroutine(CoProcessInput(skillInput, startTime));
    }
    public IEnumerator CoProcessInput(PlayerSkillInputStruct skillInput, float startTime)
    {
        //(미세한 시간을 잡을 수 있는가)

        #region 변수
        stopCoroutine = false;
        #endregion

        while (!stopCoroutine)
        {
            if (Time.time >= startTime + skillInput.executeWaitTime + skillInput.executeDuration)
            {
                yield break;
            }
            else if (Time.time >= startTime + skillInput.executeWaitTime)
            {
                if (playerSkillInputValue.Count > 0)
                {
                    ExecuteStoredInput(playerSkillInputValue.Pop());
                    yield break;
                }
            }

            if (Time.time >= startTime + skillInput.storeWaitTime + skillInput.storeDuration)
            {
                //지속시간 이후에는 더 이상 키값을 받지 않음
                canStoreInputValue = false;

            }
            else if (Time.time >= startTime + skillInput.storeWaitTime)
            {
                //지속시간 동안 키를 받음
                canStoreInputValue = true;
            }


            yield return null;
        }

        Initialize();
    }

    public void ExecuteStoredInput(PlayerStateType inputValue)
    {
        if (playerState.playerCurrentState == PlayerStateType.DEAD)
        {
            if (playerSkillInputValue != null || playerSkillInputValue.Count >= 0)
            {
                playerSkillInputValue.Clear();
            }

            return;
        }

        switch (inputValue)
        {
            case PlayerStateType.HUMAN_NORMALATTACK:
                if (playerState.playerCurrentSubState == PlayerSubStateType.HUMAN_FIRSTNORMALATTACK)
                {
                    //두번째 실행
                    humanSkill.UseSkill(humanSkill.canUseSecondAttack, humanSkill.coSecondAttack, humanSkill.CoSecondAttack);
                }
                else if (playerState.playerCurrentSubState == PlayerSubStateType.HUMAN_SECONDNORMALATTACK)
                {
                    //세번째 실행
                    humanSkill.UseSkill(humanSkill.canUseThirdAttack, humanSkill.coThirdAttack, humanSkill.CoThirdAttack);
                }
                else if (playerState.playerCurrentSubState == PlayerSubStateType.HUMAN_THIRDNORMALATTACK)
                {
                    //네번째 실행
                    maskChange.ChangeCharacter();
                    maskChange.ChangeMask(MaskType.ANIMAL, true, true);
                    animalSkill.UseSkill(animalSkill.canUseLeapStrike, animalSkill.coLeapStrike, animalSkill.CoLeapStrike);
                }
                break;
            case PlayerStateType.HUMAN_INKSHAPE:
                humanSkill.UseSkill(humanSkill.canUseInkShape, humanSkill.coInkShape, humanSkill.CoInkShape);
                break;
            case PlayerStateType.HUMAN_INKFLOOR:
                if (CameraController.instance.CurrentTarget)
                {
                    humanSkill.UseSkill(humanSkill.canUseInkFloor, humanSkill.coInkFloor, humanSkill.CoInkFloor);
                }
                break;
            case PlayerStateType.DASH:
                if (maskChange.HumanMask.activeSelf)
                {
                    humanSkill.UseSkill(humanSkill.canUseDash, humanSkill.coDash, humanSkill.CoDash);
                }
                else
                {
                    animalSkill.UseSkill(animalSkill.canUseDash, animalSkill.coDash, animalSkill.CoDash)
                        ;
                }
                break;

            case PlayerStateType.ANIMAL_NORMALATTACK:
                if (playerState.playerCurrentSubState == PlayerSubStateType.ANIMAL_FIRSTNORMALATTACK)
                {
                    animalSkill.UseSkill(animalSkill.canUseSecondAttack, animalSkill.coSecondAttack, animalSkill.CoSecondAttack);
                }
                else if (playerState.playerCurrentSubState == PlayerSubStateType.ANIMAL_SECONDNORMALATTACK)
                {
                    animalSkill.UseSkill(animalSkill.canUseThirdAttack,animalSkill.coThirdAttack, animalSkill.CoThirdAttack);
                }
                else if (playerState.playerCurrentSubState == PlayerSubStateType.ANIMAL_THIRDNORMALATTACK)
                {
                    maskChange.ChangeCharacter();
                    maskChange.ChangeMask(MaskType.HUMAN, true, true);
                    humanSkill.UseSkill(humanSkill.canUseInkShape, humanSkill.coInkShape, humanSkill.CoInkShape);
                }
                break;
            case PlayerStateType.ANIMAL_LEAPSTRIKE:
                animalSkill.UseSkill(animalSkill.canUseLeapStrike, animalSkill.coLeapStrike, animalSkill.CoLeapStrike);
                break;
            case PlayerStateType.ANIMAL_ROAR:
                animalSkill.UseSkill(animalSkill.canUseRoar, animalSkill.coRoar, animalSkill.CoRoar);
                break;
        }
    }
    #endregion

    #region 원하는 콤보 즉각 실행
    public void ProcessInputDirectly(PlayerSkillInputStruct skillInput, float startTime)
    {
        Initialize();

        if (inputCoroutine != null)
        {
            StopCoroutine(inputCoroutine);
        }

        inputCoroutine = StartCoroutine(CoProcessInputDirectly(skillInput, startTime));
    }

    public IEnumerator CoProcessInputDirectly(PlayerSkillInputStruct skillInput, float startTime)
    {
        //(미세한 시간을 잡을 수 있는가)

        #region 변수
        stopCoroutine = false;
        #endregion

        while (!stopCoroutine)
        {
            if (Time.time >= startTime + skillInput.executeWaitTime + skillInput.executeDuration)
            {
                yield break;
            }
            else if (Time.time >= startTime + skillInput.executeWaitTime)
            {
                ExecuteStoredInputDirectly();
                yield break;
            }

            yield return null;
        }

        Initialize();
    }
    public void ExecuteStoredInputDirectly()
    {
        if (playerState.playerCurrentSubState == PlayerSubStateType.HUMAN_FIRSTNORMALATTACK)
        {
            //두번째 실행
            humanSkill.UseSkill(humanSkill.canUseSecondAttack, humanSkill.coSecondAttack, humanSkill.CoSecondAttack);
        }
        if (playerState.playerCurrentSubState == PlayerSubStateType.ANIMAL_FIRSTNORMALATTACK)
        {
            //두번째 실행
            animalSkill.UseSkill(animalSkill.canUseSecondAttack ,animalSkill.coSecondAttack, animalSkill.CoSecondAttack);
        }
    }

    #endregion
}
