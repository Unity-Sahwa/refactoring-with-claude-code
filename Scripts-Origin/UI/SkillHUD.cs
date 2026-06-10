using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

//TODO: 쿨타임 시간을 스킬 스크립트로부터 받아와서 사용. 클릭이나 입력에 따라 함수가 실행되는 방식으로 진행(ex: 버튼 실행)

public class SkillHUD : MonoBehaviour
{
    #region 외부
    [Header("외부")]
    [SerializeField] private HumanMaskSkill humanMaskSkill;
    [SerializeField] private AnimalMaskSkill animalMaskSkill;
    [SerializeField] private GhostMaskSkill ghostMaskSkill;
    [SerializeField] private PlayerState playerState;
    #endregion

    //초기에 스크립트 실행 순서때문에 생기는 문제를 bool 변수로 차단

    #region 스킬/탈 아이콘 변경
    [Space(20)]
    [Header("쿨다운 가이드 이미지")]
    [SerializeField] private GameObject battleHUD;
    [SerializeField] private Button specialAttackButton;
    [SerializeField] private Button normalAttackButton;
    [SerializeField] private Image humanSpecialAttackImage;
    [SerializeField] private Image animalSpecialAttackImage;
    [SerializeField] private Image finishAttackImage;
    [SerializeField] private Image dashImage;
    #endregion


    public void ActivateFinishHUD(bool activate)
    {//외부에서 사용 - 적탐지시, 스킬 사용시에 SkillHUD 전체가 On/Off
        finishAttackImage.gameObject.SetActive(activate);
    }

    //스킬쿨을 시각화 
    public void SkillCooldown(PlayerStateType stateType, float flowTimeRate)
    {
        Image skillGuideImage = SetImage(stateType);
        if (flowTimeRate > .99f)
        {
            skillGuideImage.fillAmount = 1;
            //skillGuideImage.color = Color.white;
        }
        else
        {
            skillGuideImage.fillAmount = flowTimeRate;
            //skillGuideImage.color = Color.grey;
        }
    }

    public void ChangeGuideHUDColor(PlayerStateType stateType, Color color)
    {
        Image skillGuideImage = SetImage(stateType);
        skillGuideImage.color = color;
    }

    //스킬 쿨타임 시각화용.
    //스킬 사용시 플레이어 상태가 변하는데 해당 이미지로 반환
    public Image SetImage(PlayerStateType stateType)
    {
        Image skillImage = null;

        if (stateType == PlayerStateType.HUMAN_INKSHAPE)
        {
            skillImage = humanSpecialAttackImage;
        }
        else if (stateType == PlayerStateType.ANIMAL_LEAPSTRIKE)
        {
            skillImage = animalSpecialAttackImage;
        }
        else if (stateType == PlayerStateType.DASH)
        {
            skillImage = dashImage;
        }
        else if (stateType == PlayerStateType.GHOST_FINISHSKILL)
        {
            skillImage = finishAttackImage;
        }

        return skillImage;
    } 

    public void ChangeIcon(MaskType maskType)
    {
        //기본공격도 적용하기

        if (maskType == MaskType.HUMAN)
        {
            humanSpecialAttackImage.transform.parent.gameObject.SetActive(true);
            animalSpecialAttackImage.transform.parent.gameObject.SetActive(false);

            specialAttackButton.targetGraphic = humanSpecialAttackImage;

        }
        else if (maskType == MaskType.ANIMAL)
        {
            humanSpecialAttackImage.transform.parent.gameObject.SetActive(false);
            animalSpecialAttackImage.transform.parent.gameObject.SetActive(true);

            specialAttackButton.targetGraphic = animalSpecialAttackImage;
        }
    }
}
