using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;

public class MaskChange : MonoBehaviour
{
    public static MaskChange instance;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private OrbitPartnerMask partner;

    [SerializeField] private SkillHUD skillHUD;
    [SerializeField] private HumanMaskSkill humanSkill;
    [SerializeField] private AnimalMaskSkill animalSkill;
    
    [SerializeField] private PlayerSound playerSound;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerEffect playerEffect;

    private PlayerCommonData commonData;
    private PlayerHumanMaskData humanData;
    private PlayerAnimalMaskData animalData;

    #region 탈 이펙트
    [SerializeField] private GameObject humanMaskOnCharacter;
    [SerializeField] private GameObject ghostMaskOnHumanCharacter;
    [SerializeField] private GameObject animalMaskOnCharacter;
    [SerializeField] private GameObject ghostMaskOnAnimalCharacter;

    [SerializeField] private GameObject[] humanMaskRadialEffect;
    [SerializeField] private GameObject[] animalMaskRadialEffect;
    [SerializeField] private GameObject[] ghostMaskRadialEffect;
    [SerializeField] private Transform maskEffectTR;
    #endregion

    private bool canUseChangeMask;
    private float lastTimeCastedChangeMask;

    #region Human
    private GameObject humanCharacter;
    private Animator humanAnimator;
    private Rigidbody humanRigidbody;
    public GameObject HumanMask { get { return humanCharacter; } }
    public Animator HumanAnimator { get { return humanAnimator; } }
    public Rigidbody HumanRigidbody { get { return humanRigidbody; } }
    #endregion

    #region Animal
    private GameObject animalCharacter;
    private Animator animalAnimator;
    private Rigidbody animalRigidbody;
    public GameObject AnimalMask { get { return animalCharacter; } }
    public Animator AnimalAnimator { get { return animalAnimator; } }
    public Rigidbody AnimalRigidbody { get { return animalRigidbody; } }
    #endregion

    #region current
    private GameObject currentCharacter;
    private Animator currentAnimator;
    private Rigidbody currentRigidbody;
    
    public GameObject CurrentMask { get { return currentCharacter; } }
    public Animator CurrentAnimator { get { return currentAnimator; } }
    public Rigidbody CurrentRigidbody { get { return currentRigidbody; } }

    #endregion

    private void Awake()
    {
        #region 싱글톤
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);
        #endregion
    }

    private void Start()
    {
        commonData = PlayerCommonData.Instance;
        humanData = PlayerHumanMaskData.Instance;
        animalData = PlayerAnimalMaskData.Instance;

        canUseChangeMask = true;

        InitialSetUp();
    }

    public void InitialSetUp()
    {
        humanCharacter = transform.GetChild(0).gameObject;
        humanAnimator = humanCharacter.GetComponent<Animator>();
        humanRigidbody = humanCharacter.GetComponent<Rigidbody>();
        
        animalCharacter = transform.GetChild(1).gameObject;
        animalAnimator = animalCharacter.GetComponent<Animator>();
        animalRigidbody = animalCharacter.GetComponent<Rigidbody>();

        //player = transform.GetChild(3).gameObject;
        //playerRigidbody = player.GetComponent<Rigidbody>();
        //playerCollider = player.GetComponent<CapsuleCollider>();

        currentCharacter = humanCharacter;
        currentAnimator = humanAnimator;
        currentRigidbody = humanRigidbody;

        ChangeMask(MaskType.HUMAN, false, false);
    }

    //캐릭터만 교체
    public void ChangeCharacter()
    {
        if (!canUseChangeMask) return;

        canUseChangeMask = false;
        lastTimeCastedChangeMask = Time.time;

        if (humanCharacter.activeSelf)
        {
            animalCharacter.transform.position = humanCharacter.transform.position; 
            animalCharacter.transform.rotation = humanCharacter.transform.rotation;

            currentCharacter = animalCharacter;
            currentAnimator = animalAnimator;
            currentRigidbody = animalRigidbody;

            humanSkill.InitializeSkill();

            animalCharacter.SetActive(true);
            humanCharacter.SetActive(false);
            
            if (CameraController.instance.CurrentTarget)
            {
                currentAnimator.SetBool("isFocused", true);
            }
            else
            {
                currentAnimator.SetBool("isFocused", false);
            }
        }
        else if (animalCharacter.activeSelf)
        {
            humanCharacter.transform.position = animalCharacter.transform.position;
            humanCharacter.transform.rotation = animalCharacter.transform.rotation;

            currentCharacter = humanCharacter;
            currentAnimator = humanAnimator;
            currentRigidbody=humanRigidbody;

            animalSkill.InitializeSkill();

            animalCharacter.SetActive(false);
            humanCharacter.SetActive(true);

            if (CameraController.instance.CurrentTarget)
            {
                currentAnimator.SetBool("isFocused", true);
            }
            else
            {
                currentAnimator.SetBool("isFocused", false);
            }
        }
    } 

    //탈을 교체
    public void ChangeMask(MaskType maskType, bool useFunction, bool useSound)
    {
        //HUD 바꾸기
        skillHUD.ChangeIcon(maskType);

        //탈교체
        if (maskType == MaskType.HUMAN)
        {
            humanMaskOnCharacter.SetActive(true);
            ghostMaskOnHumanCharacter.SetActive(false);
        }
        else if (maskType == MaskType.ANIMAL)
        {
            animalMaskOnCharacter.SetActive(true);
            ghostMaskOnAnimalCharacter.SetActive(false);
        }
        else
        {
            if (currentCharacter == humanCharacter)
            {
                humanMaskOnCharacter.SetActive(false);
                ghostMaskOnHumanCharacter.SetActive(true);
            }
            else
            {
                animalMaskOnCharacter.SetActive(false);
                ghostMaskOnAnimalCharacter.SetActive(true);
            }
        }

        if (!useFunction)
        {
            return;
        }
        
        GameObject[] maskObject;
        EffectStruct effectStruct;
        if (maskType == MaskType.HUMAN)
        {
            maskObject = humanMaskRadialEffect;
            effectStruct = commonData.humanMaskEffect;

            partner.StartCoroutine(partner.CollideWithPlayer(MaskType.HUMAN));
        }
        else if (maskType == MaskType.ANIMAL)
        {
            maskObject = animalMaskRadialEffect;
            effectStruct = commonData.animalMaskEffect;

            partner.StartCoroutine(partner.CollideWithPlayer(MaskType.ANIMAL));
        }

        else

        {
            maskObject = ghostMaskRadialEffect;
            effectStruct = commonData.ghostMaskEffect;

            partner.StartCoroutine(partner.CollideWithPlayer(MaskType.GHOST));
        }

        Vector3 cameraDirection = Camera.main.transform.position - maskEffectTR.position;
        Vector3 cameraForwardPosition = maskEffectTR.position + cameraDirection.normalized * 0.5f;

        //히트이펙트는 아니지만... 수정해야함
        playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect(commonData.partnerCollisionEffect, maskObject, cameraForwardPosition));
        
        if (useSound)
        {
            playerSound.SetPlayerSound(commonData.maskChangeSound, Player.instance.transform.position, Time.time);
        }
    }

    //상황에 따라 탈교체 소리 안나거나, 데이터 불러올때 캐릭터만 교체하는걸로 하기

    public void ChangeMaskCooldown()
    {
        if (canUseChangeMask) return;

        if (Time.time < lastTimeCastedChangeMask + commonData.changeMaskCooldown) //탈교체 쿨타임이 지나면 반환
        {
            return;
        }
        canUseChangeMask = true;
    }

    //public void FollowPlayerObject()
    //{
    //    if ( humanMask.activeSelf)
    //    {
    //        humanMask.transform.position = player.transform.position;
    //        humanMask.transform.rotation = player.transform.rotation;
    //    }
    //    else
    //    {
    //        animalMask.transform.position = player.transform.position;
    //        animalMask.transform.rotation = player.transform.rotation;
    //    }
    //}
}


