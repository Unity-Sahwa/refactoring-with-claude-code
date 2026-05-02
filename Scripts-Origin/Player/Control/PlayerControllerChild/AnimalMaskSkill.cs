using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalMaskSkill : PlayerSkill
{
    #region 외부
    [SerializeField] private SkillHUD skillHUD;

    //스킬 기능
    [SerializeField] private PlayerCameraEffect playerCameraEffect;
    [SerializeField] private PlayerSkillMove playerSkillMove;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerEffect playerEffect;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerSound playerSound;
    [SerializeField] private PlayerSkillInput playerSkillInput;
    [SerializeField] private PlayerHitBox playerHitBox;
    [SerializeField] private GameTimeScale gameTimeScale;

    #endregion

    #region 사용가능 여부
    public bool canUseFirstAttack { get; private set; }
    public bool canUseSecondAttack { get; private set; }
    public bool canUseThirdAttack { get; private set; }
    public bool canUseLeapStrike {get; private set;}
    public bool canUseRoar {get; private set;}
    public bool canUseDash { get;  private set;}
    #endregion

    #region 스킬 수행중 여부
    private bool isPerformingLeapStrike;
    private bool isPerformingRoar;
    private bool isPerformingDash;

    //애니 수행
    private bool isPerformingFirstAttackAnim;
    private bool isPerformingSecondAttackAnim;
    private bool isPerformingThirdAttackAnim;
    private bool isPerformingLeapStrikeAnim;
    private bool isPerformingRoarAnim;
    private bool isPerformingDashAnim;
    #endregion

    #region 시전시간
    private float firstAttackStartTime;
    private float secondAttackStartTime;
    private float thirdAttackStartTime;
    private float leapStrikeStartTime;
    private float roarStartTime;
    private float dashStartTime;
    #endregion

    #region 스킬 코루틴
    public Coroutine coFirstAttack { get; private set; }
    public Coroutine coSecondAttack { get; private set; }
    public Coroutine coThirdAttack { get; private set; }
    public Coroutine coLeapStrike { get; private set; }
    public Coroutine coRoar { get; private set; }
    public Coroutine coDash { get; private set; }
    #endregion

    #region 무기 형상
    [SerializeField] private GameObject rightHandWeapon;
    [SerializeField] private GameObject leftHandWeapon;
    
    private MeshRenderer rightHandWeaponMesh;
    private MeshRenderer leftHandWeaponMesh;

    //[SerializeField] private GameObject[] weaponShapes;
    #endregion

    #region NoarmalAttack
    [Space(20)]
    [SerializeField] private GameObject firstAttackEffectPosition;
    [SerializeField] private GameObject secondAttackEffectPosition;
    [SerializeField] private GameObject thirdAttackEffectPosition;

    [SerializeField ] private GameObject[] firstAttackEffect;
    [SerializeField] private GameObject[] secondAttackEffect;
    [SerializeField] private GameObject[] thirdAttackEffect;
    #endregion

    #region Leap Strike
    [Space(20)]
    private int leapStrikeHitCount = 0;

    [SerializeField] private GameObject leapStrikeHitBox;
    [SerializeField] private GameObject leapStrikeEffectPosition;
    [SerializeField] private GameObject[] leapStrikeEffect;

    //Position SO 데이터로 넣어놓고 실시간으로 변경하는게 편할듯. 테스트할때는 
    [SerializeField] private GameObject leapStrikeTrailPosition;
    [SerializeField] private GameObject[] leapStrikeTrail;
    #endregion

    #region Roar
    [Space(20)]
    private int roarHitCount = 0;
    [SerializeField] private GameObject roarHitBox;
    
    [SerializeField] private GameObject roarEffectPosition;

    [SerializeField] private GameObject[] roarTrailEffect;
    [SerializeField] private GameObject[] roarChargeEffect;
    [SerializeField] private GameObject[] roarDisChargeEffect;
    #endregion

    public delegate IEnumerator CoroutineDelegate();

    private void Start()
    {
        StartSet();
        AnimalMaskStartSet();
        this.gameObject.SetActive(false);
    }

    #region Initialize
    public void InitializeSkill()
    {
        InitializeCoroutine();
        InitializeWeapon();
        InitializeHitBox();
        InitializeState();

        //스킬기능 
        //playerTimeScale.Initialize();
        playerSkillMove.Initialize();
        playerEffect.Initialize();
        playerState.Initialize();
        playerSound.Initialize();
        playerSkillInput.Initialize();
        gameTimeScale.Initialize();
    }

    public void InitializeCoroutine()
    {
        if (coFirstAttack != null) StopCoroutine(coFirstAttack);
        if (coSecondAttack != null) StopCoroutine(coSecondAttack);
        if (coThirdAttack != null) StopCoroutine(coThirdAttack);
        if (coLeapStrike != null) StopCoroutine(coLeapStrike);
        if (coRoar != null) StopCoroutine(coRoar);
        if (coDash != null) StopCoroutine(coDash);
    }

    public void InitializeWeapon()
    {
        rightHandWeaponMesh.enabled = false;
        leftHandWeaponMesh.enabled = false;
    }

    public void InitializeHitBox()
    {
        leapStrikeHitBox.SetActive(false);
        roarHitBox.SetActive(false);

        //도약공격 해당
        //로어 해당
    }

    public void InitializeState()
    {
        #region 기본공격
        isPerformingFirstAttackAnim = false;
        isPerformingSecondAttackAnim = false;
        isPerformingThirdAttackAnim = false;

        canUseFirstAttack = true; //평타는 쿨타임 없음
        canUseSecondAttack = false; //평타1 함수에서 true로 바뀜
        canUseThirdAttack = false; //평타2 함수에서 true로 바뀜
        #endregion

        #region 스킬
        isPerformingLeapStrike = false;
        isPerformingRoar = false;
        isPerformingDash = false;

        isPerformingLeapStrikeAnim = false;
        isPerformingRoarAnim = false;
        isPerformingDashAnim = false;
        #endregion
    }
    #endregion

    private void AnimalMaskStartSet()
    {
        #region canUse
        canUseFirstAttack = true;
        canUseSecondAttack = false;
        canUseThirdAttack = false;
        canUseLeapStrike = true;
        canUseRoar = true;
        canUseDash = true;
        #endregion

        #region perform
        isPerformingLeapStrike = false;
        isPerformingRoar = false;
        isPerformingDash = false;

        isPerformingFirstAttackAnim = false;
        isPerformingSecondAttackAnim = false;
        isPerformingThirdAttackAnim = false;
        isPerformingLeapStrikeAnim = false;
        isPerformingRoarAnim = false;
        isPerformingDashAnim = false;
        #endregion

        #region coroutine
        coFirstAttack = null;
        coSecondAttack = null;
        coThirdAttack = null;
        coLeapStrike = null;
        coRoar = null;
        coDash = null;
        #endregion

        #region 게임오브젝트
        leapStrikeHitBox.SetActive(false);

        for (int i = 0; i < firstAttackEffect.Length; i++)
        {
            firstAttackEffect[i].SetActive(false);
        }
        for (int i = 0; i < secondAttackEffect.Length; i++)
        {
            secondAttackEffect[i].SetActive(false);
        }
        for (int i = 0; i < leapStrikeEffect.Length; i++)
        {
            leapStrikeEffect[i].SetActive(false);
        }
        for (int i = 0; i < leapStrikeTrail.Length; i++)
        {
            leapStrikeTrail[i].SetActive(false);
        }
        for (int i = 0; i < roarChargeEffect.Length; i++)
        {
            roarChargeEffect[i].SetActive(false);
        }
        for (int i = 0; i < roarDisChargeEffect.Length; i++)
        {
            roarDisChargeEffect[i].SetActive(false);
        }

        rightHandWeaponMesh = rightHandWeapon.GetComponent<MeshRenderer>();
        leftHandWeaponMesh = leftHandWeapon.GetComponent<MeshRenderer>();

        rightHandWeaponMesh.enabled = false;
        leftHandWeaponMesh.enabled = false;
        #endregion

        leapStrikeHitCount = 0;
        roarHitCount = 0;
    }

    public void UseSkill(bool canUse, Coroutine coroutine, CoroutineDelegate coroutineMethod)
    {
        if (canUse)
        {
            InitializeSkill();

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            UIEffect.ShowPlayerHUDFadeEffect();
            coroutine = StartCoroutine(coroutineMethod());

            canUse = false;
        }
    }

    //public void ActivateWeaponShape()
    //{
    //    //자동으로 꺼지는 이펙트
    //    for (int i = 0; i < weaponShapes.Length; i++)
    //    {
    //        weaponShapes[i].SetActive(true);
    //    }
    //}


    #region Normal Attack
    public void NormalAttack()
    {
        UseSkill(canUseFirstAttack,coFirstAttack, CoFirstAttack);
    }

    public IEnumerator CoFirstAttack()
    {
        maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_FirstNormalAttack, 0.1f);
        
        playerState.ChangePlayerState(PlayerStateType.ANIMAL_NORMALATTACK);
        playerState.ChangePlayerSubState(PlayerSubStateType.ANIMAL_FIRSTNORMALATTACK);

        canUseFirstAttack = false;
        firstAttackStartTime = Time.time;

        #region while 변수
        bool activeMoveOnce = false;
        bool activeEffectOnce = false;
        bool activeHitBoxOnce = false;
        bool inactiveHitBoxOnce = false;
        bool activeSoundOnce = false;
        bool activeCameraShakeOnce = false;
        #endregion

        //playerSkillInput.ProcessInputDirectly(animalData.firstNormalAttackInput, firstAttackStartTime);
        playerSkillInput.ProcessInput(animalData.firstNormalAttackInput, firstAttackStartTime);
        while (playerState.playerCurrentSubState == PlayerSubStateType.ANIMAL_FIRSTNORMALATTACK)
        {
            #region Animation State
            var animatorStateInfo = maskChange.CurrentAnimator.GetCurrentAnimatorStateInfo(0);
            var animationHash = animatorStateInfo.shortNameHash;
            if (animationHash == playerAnimation.Animal_FirstNormalAttack)
            {
                isPerformingFirstAttackAnim = true;
            }
            else if ((animationHash == playerAnimation.Animal_Hit) || (animationHash == playerAnimation.Animal_Die))
            {
                InitializeSkill();
                yield break;
            }
            else
            {
                if (isPerformingFirstAttackAnim)
                {
                    InitializeSkill();
                    yield break;
                }
            }
            #endregion

            #region Move
            if (!activeMoveOnce)
            {
                for (int i = 0; i < animalData.firstNormalAttackMove.Length; i++)
                {
                    playerSkillMove.StartCoroutine(playerSkillMove.SkillMove(animalData.firstNormalAttackMove[i]));
                }
                activeMoveOnce = true;
            }
            #endregion

            #region Restriction
            playerState.RestrictPlayer(animalData.firstNormalAttackRestrict, firstAttackStartTime);

            //DoNotAct의 duration까지 끝난 상황에서 움직이면 스킬 중지 
            if (isPerformingFirstAttackAnim && (Time.time >= firstAttackStartTime + animalData.firstNormalAttackRestrict.actRestrictWaitTime))
            {
                canUseSecondAttack = true;

                if (maskChange.CurrentAnimator.GetFloat("moveAmount") > .4f) // 움직이면 초기화
                {
                    InitializeSkill();
                    yield break;
                }
            }
            #endregion

            #region Effect
            if (!activeEffectOnce)
            {
                playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                    (animalData.firstNormalAttackSkillEffect, firstAttackEffect, firstAttackEffectPosition));
                activeEffectOnce = true;
            }   
            #endregion

            #region Weapon Shape
            if (Time.time >= firstAttackStartTime + animalData.firstNormalAttackWeaponWaitTime + animalData.firstNormalAttackWeaponDuration)
            {
                rightHandWeaponMesh.enabled = false;
            }
            else if (Time.time >= firstAttackStartTime + animalData.firstNormalAttackWeaponWaitTime)
            {
                rightHandWeaponMesh.enabled = true;
            }
            #endregion

            #region HitBox
            if (!activeHitBoxOnce)
            {
                playerHitBox.StartCoroutine(playerHitBox.TogglePlayerHitBox(animalData.firstNormalAttackHitBox));
                activeHitBoxOnce = true;
            }
            #endregion

            #region Audio
            if (!activeSoundOnce)
            {
                playerSound.SetPlayerSound(animalData.firstNormalAttackSound, player.transform.position,firstAttackStartTime);
                activeSoundOnce = true;
            }
            #endregion

            yield return null;
        }
    }
    public IEnumerator CoSecondAttack()
    {
        playerState.ToggleSuperArmorState(true);

        maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_SecondNormalAttack, 0.1f);

        playerState.ChangePlayerState(PlayerStateType.ANIMAL_NORMALATTACK);
        playerState.ChangePlayerSubState(PlayerSubStateType.ANIMAL_SECONDNORMALATTACK);

        canUseSecondAttack = false;
        secondAttackStartTime = Time.time;

        #region while 변수
        bool activeMoveOnce = false;
        bool activeEffectOnce = false;
        bool activeHitBoxOnce = false;
        bool inactiveHitBoxOnce = false;
        bool activeSoundOnce = false;
        bool activeCameraShakeOnce = false;
        #endregion

        playerSkillInput.ProcessInput(animalData.secondNormalAttackInput, secondAttackStartTime);

        while (playerState.playerCurrentSubState == PlayerSubStateType.ANIMAL_SECONDNORMALATTACK)
        {
            #region Animation State
            var animatorStateInfo = maskChange.CurrentAnimator.GetCurrentAnimatorStateInfo(0);
            var animationHash = animatorStateInfo.shortNameHash;

            if (animationHash == playerAnimation.Animal_SecondNormalAttack)
            {
                isPerformingSecondAttackAnim = true;
            }
            else if ((animationHash == playerAnimation.Animal_Hit) || (animationHash == playerAnimation.Animal_Die))
            {
                InitializeSkill();
                yield break;
            }
            else
            {
                if (isPerformingSecondAttackAnim)
                {
                    InitializeSkill();
                    yield break;
                }
            }
            #endregion

            #region Move
            if (!activeMoveOnce)
            {
                for (int i = 0; i < animalData.secondNormalAttackMove.Length; i++)
                {
                    playerSkillMove.StartCoroutine(playerSkillMove.SkillMove(animalData.secondNormalAttackMove[i]));
                }
                activeMoveOnce = true;
            }
            #endregion

            #region Restrict
            playerState.RestrictPlayer(animalData.secondNormalAttackRestrict, secondAttackStartTime);

            //DoNotAct의 duration까지 끝난 상황에서 움직이면 스킬 중지
            if (isPerformingSecondAttackAnim && Time.time >= secondAttackStartTime + animalData.secondNormalAttackRestrict.actRestrictWaitTime)
            {
                canUseThirdAttack = true;

                if (maskChange.CurrentAnimator.GetFloat("moveAmount") > .9f) // 움직이면 초기화
                {
                    InitializeSkill();
                    yield break;
                }
            }
            #endregion

            #region Effect
            if (!activeEffectOnce)
            {
                playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                    (animalData.secondNormalAttackSkillEffect, secondAttackEffect, secondAttackEffectPosition));
                activeEffectOnce = true;
            }
            #endregion

            #region Weapon Shape
            if (Time.time >= secondAttackStartTime + animalData.secondNormalAttackWeaponWaitTime + animalData.secondNormalAttackWeaponDuration)
            {
                leftHandWeaponMesh.enabled = false;
            }
            else if (Time.time >= secondAttackStartTime + animalData.secondNormalAttackWeaponWaitTime)
            {
                leftHandWeaponMesh.enabled = true;
            }
            #endregion

            #region HitBox
            if (!activeHitBoxOnce)
            {
                playerHitBox.StartCoroutine(playerHitBox.TogglePlayerHitBox(animalData.secondNormalAttackHitBox));
                activeHitBoxOnce = true;
            }
            #endregion

            #region Sound
            if (!activeSoundOnce)
            {
                playerSound.SetPlayerSound(animalData.secondNormalAttackSound, player.transform.position,secondAttackStartTime);
                activeSoundOnce = true;
            }
            #endregion

            #region Camera Shake
            if (!activeCameraShakeOnce && (Time.time >= secondAttackStartTime + animalData.secondNormalAttackCameraShake.waitTime))
            {
                playerCameraEffect.ShakeCamera(animalData.secondNormalAttackCameraShake);
                activeCameraShakeOnce = true;
            }
            #endregion

            yield return null;
        }
    }
    public IEnumerator CoThirdAttack()
    {
        playerState.ToggleSuperArmorState(true);

        maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_ThirdNormalAttack, 0.1f);

        playerState.ChangePlayerState(PlayerStateType.ANIMAL_NORMALATTACK);
        playerState.ChangePlayerSubState(PlayerSubStateType.ANIMAL_THIRDNORMALATTACK);

        canUseThirdAttack = false;
        thirdAttackStartTime = Time.time;

        #region while 변수
        bool activeMoveOnce = false;
        bool activeEffectOnce = false;
        bool activeHitBoxOnce = false;
        bool inactiveHitBoxOnce = false;
        bool activeSoundOnce = false;
        bool activeCameraShakeOnce = false;
        #endregion

        playerSkillInput.ProcessInput(animalData.thirdNormalAttackInput, thirdAttackStartTime);

        while (playerState.playerCurrentSubState == PlayerSubStateType.ANIMAL_THIRDNORMALATTACK)
        {
            #region Animation State
            var animatorStateInfo = maskChange.CurrentAnimator.GetCurrentAnimatorStateInfo(0);
            var animationHash = animatorStateInfo.shortNameHash;

            if (animationHash == playerAnimation.Animal_ThirdNormalAttack)
            {
                isPerformingThirdAttackAnim = true;
            }
            else if ((animationHash == playerAnimation.Animal_Hit) || (animationHash == playerAnimation.Animal_Die))
            {
                InitializeSkill();
                yield break;
            }
            else
            {
                if (isPerformingThirdAttackAnim)
                {
                    InitializeSkill();
                    yield break;
                }
            }
            #endregion

            #region Move
            if (!activeMoveOnce)
            {
                for (int i = 0; i < animalData.thirdNormalAttackMove.Length; i++)
                {
                    playerSkillMove.StartCoroutine(playerSkillMove.SkillMove(animalData.thirdNormalAttackMove[i]));
                }
                activeMoveOnce = true;
            }
            #endregion

            #region Restrict
            playerState.RestrictPlayer(animalData.thirdNormalAttackRestrict, thirdAttackStartTime);

            //DoNotAct의 duration까지 끝난 상황에서 움직이면 스킬 중지
            if (isPerformingThirdAttackAnim && Time.time >= thirdAttackStartTime + animalData.thirdNormalAttackRestrict.actRestrictWaitTime)
            {
                if (maskChange.CurrentAnimator.GetFloat("moveAmount") > .9f) // 움직이면 초기화
                {
                    InitializeSkill();
                    yield break;
                }
            }
            #endregion

            #region Effect
            if (!activeEffectOnce)
            {
                playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                    (animalData.thirdNormalAttackSkillEffect, thirdAttackEffect, thirdAttackEffectPosition));
                activeEffectOnce = true;
            }
            #endregion

            #region Weapon Shape
            if (Time.time >= thirdAttackStartTime + animalData.thirdNormalAttackWeaponWaitTime + animalData.thirdNormalAttackWeaponDuration)
            {
                leftHandWeaponMesh.enabled = false;
            }
            else if (Time.time >= thirdAttackStartTime + animalData.thirdNormalAttackWeaponWaitTime)
            {
                leftHandWeaponMesh.enabled = true;

            }
            #endregion

            #region HitBox
            if (!activeHitBoxOnce)
            {
                playerHitBox.StartCoroutine(playerHitBox.TogglePlayerHitBox(animalData.thirdNormalAttackHitBox));
                activeHitBoxOnce = true;
            }
            #endregion

            #region Sound
            if (!activeSoundOnce)
            {
                playerSound.SetPlayerSound(animalData.thirdNormalAttackSound, player.transform.position, thirdAttackStartTime);
                activeSoundOnce = true;
            }
            #endregion

            #region Camera Shake
            if (!activeCameraShakeOnce && (Time.time >= secondAttackStartTime + animalData.secondNormalAttackCameraShake.waitTime))
            {
                playerCameraEffect.ShakeCamera(animalData.secondNormalAttackCameraShake);
                activeCameraShakeOnce = true;
            }
            #endregion

            #region TimeScale

            #endregion

            yield return null;
        }
    }
    #endregion

    #region Leap Strike
    public void LeapStrike()
    {
        UseSkill(canUseLeapStrike, coLeapStrike, CoLeapStrike);
    }

    public IEnumerator CoLeapStrike()
    {
        playerState.ToggleSuperArmorState(true);
        playerState.ChangePlayerState(PlayerStateType.ANIMAL_LEAPSTRIKE);

        if (playerMovement.Movement != Vector3.zero && !cameraController.CurrentTarget)
        {
            maskChange.CurrentMask.transform.forward = playerMovement.Movement;
        }

        isPerformingLeapStrike = true;
        canUseLeapStrike = false;
        leapStrikeStartTime = Time.time;

        playerSkillMove.GetOriginHeight();

        maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_LeapStrike, 0.1f);


        #region while 변수
        bool activeMoveOnce = false;
        bool activeEffectOnce = false;
        bool activeHitBoxOnce = false;
        bool activeSoundOnce = false;
        bool activeCameraShakeOnce = false;
        bool activeTimeScaleOnce = false;
        #endregion

        playerSkillInput.ProcessInput(animalData.leapStrikeInput, leapStrikeStartTime);

        while (isPerformingLeapStrike)
        {
            //다음에는 애니메이션 하나만 사용하는 방향으로
            #region 애니메이션 상태
            var animatorStateInfo = maskChange.CurrentAnimator.GetCurrentAnimatorStateInfo(0);
            var animationHash = animatorStateInfo.shortNameHash;
            if (animationHash == playerAnimation.Animal_LeapStrike)
            {
                isPerformingLeapStrikeAnim = true; //애니 실행중
            }
            else if (animationHash == playerAnimation.Animal_Die)
            {
                InitializeSkill();
                yield break;
            }
            else
            {
                if (isPerformingLeapStrikeAnim)
                {
                    InitializeSkill();
                    yield break;
                }
            }
            #endregion


            #region 물리 이동
            if (!activeMoveOnce)
            {
                for (int i = 0; i < animalData.leapStrikeSkillMove.Length; i++)
                {
                    playerSkillMove.StartCoroutine(playerSkillMove.SkillMove(animalData.leapStrikeSkillMove[i]));
                }
                activeMoveOnce = true;
            }
            #endregion

            #region Restrict
            playerState.RestrictPlayer(animalData.leapStrikeRestrict, leapStrikeStartTime);
            #endregion

            #region 이펙트
            if (!activeEffectOnce)
            {
                //트레일
                playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                    (animalData.leapStrikeTrailEffect, leapStrikeTrail, leapStrikeTrailPosition));

                //슬래쉬
                playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                    (animalData.leapStrikeSlashEffect, leapStrikeEffect, leapStrikeEffectPosition));

                activeEffectOnce = true;
            }
            #endregion

            #region 무기 형상
            if (Time.time >= leapStrikeStartTime + animalData.leapStrikeWeaponWaitTime + animalData.leapStrikeWeaponDuration)
            {
                rightHandWeaponMesh.enabled = false;
                leftHandWeaponMesh.enabled = false;

            }
            else if (Time.time >= leapStrikeStartTime + animalData.leapStrikeWeaponWaitTime)
            {
                rightHandWeaponMesh.enabled = true;
                leftHandWeaponMesh.enabled = true;
                //ActivateWeaponShape();
            }
            #endregion

            #region 히트박스
            //Invoke 활용. 예약걸어놓고 취소하려면 Initialize 에다가 bool값으로 Invoke 제어하도록 만들기

            if (!activeHitBoxOnce)
            {
                if (Time.time >= leapStrikeStartTime + animalData.leapStrikeHitBoxWaitTime)
                {
                    {
                        activeHitBoxOnce = true;
                        leapStrikeHitBox.SetActive(true);
                        rightHandWeapon.SetActive(true);
                        leftHandWeapon.SetActive(true);
                            
                        Invoke("LeapStrikeHitBoxOff", animalData.leapStrikeHitBoxDuration);
                    }
                }
            }
            #endregion

            #region 소리
            if (!activeSoundOnce)
            {
                playerSound.SetPlayerSound(animalData.leapStrikeJumpSound, player.transform.position,leapStrikeStartTime);
                playerSound.SetPlayerSound(animalData.leapStrikeFloatSound, player.transform.position, leapStrikeStartTime);
                playerSound.SetPlayerSound(animalData.leapStrikeSlashSound, player.transform.position, leapStrikeStartTime);

                activeSoundOnce = true;
            }
            #endregion

            #region 카메라 쉐이크
            if (!activeCameraShakeOnce)
            {
                if (Time.time >= leapStrikeStartTime + animalData.leapStrikeCameraShake.waitTime)
                {
                    playerCameraEffect.ShakeCamera(animalData.leapStrikeCameraShake);
                    activeCameraShakeOnce = true;
                }
            }
            #endregion

            #region 타임 스케일
            if (!activeTimeScaleOnce)
            {
                gameTimeScale.StartCoroutine(gameTimeScale.CoSetTimeScale(animalData.leapStrikeGameTimeScale));
                activeTimeScaleOnce = true;
            }
            #endregion

            yield return null;
        }
    }
    public void LeapStrikeCooldown()
    {
        if (canUseLeapStrike) return;

        float flowTimeRate = (Time.time - leapStrikeStartTime) / animalData.leapStrikeStat.cooldown;
        skillHUD.SkillCooldown(PlayerStateType.ANIMAL_LEAPSTRIKE, flowTimeRate);

        if (Time.time > leapStrikeStartTime + animalData.leapStrikeStat.cooldown)
        {
            canUseLeapStrike = true;
        }
    }
    public void LeapStrikeHitBoxOff()
    {
        if (isPerformingLeapStrike)
        {
            leapStrikeHitBox.SetActive(false);
        }
    }
    #endregion

    #region Roar
    public void Roar()
    {
        UseSkill(canUseRoar, coRoar, CoRoar);
    }
    public IEnumerator CoRoar()
    {
        playerState.ToggleSuperArmorState(true);
        playerState.ChangePlayerState(PlayerStateType.ANIMAL_ROAR);
        playerState.ChangePlayerSubState(PlayerSubStateType.NONE);

        if (playerMovement.Movement != Vector3.zero && !cameraController.CurrentTarget)
        {
            maskChange.CurrentMask.transform.forward = playerMovement.Movement;
        }

        isPerformingRoar = true;
        canUseRoar = false;
        roarStartTime = Time.time;

        maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_Roar, 0.1f);

        #region while 변수
        bool activeMoveOnce = false;
        bool activeEffectOnce = false;
        bool activeHitBoxOnce = false;
        bool activeSoundOnce = false;
        bool activeCameraShakeOnce = false;
        bool activeTimeScaleOnce = false;
        #endregion

        playerSkillInput.ProcessInput(animalData.roarInput, roarStartTime);

        while (isPerformingRoar)
        {
            #region 애니메이션 상태
            var animatorStateInfo = maskChange.CurrentAnimator.GetCurrentAnimatorStateInfo(0);
            var animationHash = animatorStateInfo.shortNameHash;
            if (animationHash == playerAnimation.Animal_Roar)
            {
                isPerformingRoarAnim = true; //애니 실행중
            }
            else if (animationHash == playerAnimation.Animal_Die)
            {
                InitializeSkill();
                yield break;
            }
            else
            {
                if (isPerformingRoarAnim)
                {
                    InitializeSkill();
                    yield break;
                }
            }
            #endregion

            #region Restrict
            playerState.RestrictPlayer(animalData.roarRestrict, roarStartTime);
            #endregion

            #region 이펙트
            if (!activeEffectOnce)
            {
                //트레일
                playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                    (animalData.roarTrailEffect, roarTrailEffect, rightHandWeapon));
                playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                    (animalData.roarTrailEffect, roarTrailEffect, leftHandWeapon));

                //차징
                playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                    (animalData.roarChargeEffect, roarChargeEffect, roarEffectPosition));

                //차징
                playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                    (animalData.roardisChargeEffect, roarDisChargeEffect, roarEffectPosition));
                activeEffectOnce = true;
            }
            #endregion

            #region 무기 형상
            if (Time.time >= roarStartTime + animalData.roarWeaponWaitTime + animalData.roarWeaponDuration)
            {
                rightHandWeaponMesh.enabled = false;
                leftHandWeaponMesh.enabled = false;
            }
            else if (Time.time >= roarStartTime + animalData.roarWeaponWaitTime)
            {
                rightHandWeaponMesh.enabled = true;
                leftHandWeaponMesh.enabled = true;
            }
            #endregion

            #region 히트박스
            if (Time.time >= roarStartTime + animalData.roarHitBoxWaitTime)
            {
                if (!activeHitBoxOnce)
                {
                    activeHitBoxOnce = true;

                    roarHitBox.SetActive(true);
                    roarHitBox.transform.position = roarEffectPosition.transform.position;
                    roarHitBox.transform.localScale = animalData.roarHitBoxScale;

                    //Invoke에 시간을 0으로 두면 다음번 Updata 함수 사이클에 실행됨
                    InvokeRepeating("RoarHitBoxOn", 0.01f, animalData.roarHitInterval);
                }
            }
            #endregion

            #region 소리
            if (!activeSoundOnce)
            {
                playerSound.SetPlayerSound(animalData.roarChargeSound, player.transform.position, roarStartTime);
                playerSound.SetPlayerSound(animalData.roarDischargeSound, player.transform.position, roarStartTime);
                playerSound.SetPlayerSound(animalData.roarSound, player.transform.position, roarStartTime);

                activeSoundOnce = true;
            }
            #endregion

            #region 카메라 쉐이크
            if (!activeCameraShakeOnce && (Time.time >= roarStartTime + animalData.roarCameraShake.waitTime))
            {
                playerCameraEffect.ShakeCamera(animalData.roarCameraShake);
                activeCameraShakeOnce = true;
            }
            #endregion

            #region 타임 스케일
            if (!activeTimeScaleOnce)
            {
                gameTimeScale.StartCoroutine(gameTimeScale.CoSetTimeScale(animalData.roarGameTimeScale));
                activeTimeScaleOnce = true;
            }
            #endregion

            yield return null;
        }
    }
    public void roarCooldown()
    {
        if (canUseRoar) return;

        //skillHUD.SkillIconCooldown(SkillCooldown.ROAR, Time.time - roarStartTime);

        if (Time.time > roarStartTime + animalData.roarStat.cooldown)
        {
            canUseRoar = true;
        }
    }
    public void RoarHitBoxOn()
    {
        roarHitCount++;

        roarHitBox.SetActive(true);

        Invoke("RoarHitBoxOff", animalData.roarHitInterval - 0.1f);

        if (roarHitCount >= animalData.roarHitCount)
        {
            CancelInvoke("RoarHitBoxOn");
            roarHitCount = 0;
        }
    }
    public void RoarHitBoxOff()
    {
        roarHitBox.SetActive(false);
    }


    #endregion

    #region Dash
    public void Dash()
    {
        UseSkill(canUseDash, coDash, CoDash);
    }
    public IEnumerator CoDash()
    {
        playerState.ChangePlayerState(PlayerStateType.DASH);
        playerState.ChangePlayerSubState(PlayerSubStateType.NONE);

        if (playerMovement.Movement != Vector3.zero && !cameraController.CurrentTarget)
        {
            maskChange.CurrentMask.transform.forward = playerMovement.Movement;
        }

        isPerformingDash = true;
        dashStartTime = Time.time;
        canUseDash = false;

        bool isFrontDash = false;

        if (cameraController.CurrentTarget)
        {
            isFrontDash = false;
            maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_BackDash, 0.1f);
            playerSkillInput.ProcessInput(commonData.backDashInput, dashStartTime);
        }
        else
        {
            isFrontDash = true;
            maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_FrontDash, 0.1f);
            playerSkillInput.ProcessInput(commonData.dashInput, dashStartTime);

            //if (playerMovement.Movement != Vector3.zero) //키를 누른방향으로 
            //{
            //    maskChange.CurrentMask.transform.forward = playerMovement.Movement;
            //}
        }

        #region 변수
        bool activeSoundOnce = false;

        bool activeMoveOnce = false;

        bool activeRestrictOnce = false;
        #endregion

        while (isPerformingDash)
        {
            #region 애니메이션 상태
            var animatorStateInfo = maskChange.CurrentAnimator.GetCurrentAnimatorStateInfo(0);
            var animationHash = animatorStateInfo.shortNameHash;

            if ((animationHash == playerAnimation.Animal_FrontDash) || (animationHash == playerAnimation.Animal_BackDash))
            {
                isPerformingDashAnim = true; //애니 실행중
            }
            else if ((animationHash == playerAnimation.Animal_Hit) || (animationHash == playerAnimation.Animal_Die))
            {
                InitializeSkill();
                yield break;
            }
            else
            {
                if (isPerformingDashAnim)
                {
                    InitializeSkill();
                    yield break;
                }
            }
            #endregion

            #region 물리 이동
            if (!isFrontDash)
            {
                if (!activeMoveOnce)
                {
                    for (int i = 0; i < commonData.backDashMove.Length; i++)
                    {
                        playerSkillMove.StartCoroutine(playerSkillMove.SkillMove(commonData.backDashMove[i]));
                    }
                    activeMoveOnce = true;
                }
            }
            else
            {
                if (!activeMoveOnce)
                {
                    for (int i = 0; i < commonData.dashMove.Length; i++)
                    {
                        playerSkillMove.StartCoroutine(playerSkillMove.SkillMove(commonData.dashMove[i]));
                    }
                    activeMoveOnce = true;
                }
            }
            #endregion

            #region 제한
            if (!activeRestrictOnce)
            {
                if (isFrontDash)
                {
                    playerState.RestrictPlayer(commonData.dashRestrict, dashStartTime);

                }
                else
                {
                    playerState.RestrictPlayer(commonData.backDashRestrict, dashStartTime);
                }

                activeRestrictOnce = true;
            }
            #endregion

            #region 소리
            if (!activeSoundOnce)
            {
                if (isFrontDash)
                {
                    playerSound.SetPlayerSound(commonData.dashSound, player.transform.position, dashStartTime);
                }
                else
                {
                    playerSound.SetPlayerSound(commonData.backDashSound, player.transform.position, dashStartTime);
                }

                activeSoundOnce = true;
            }
            #endregion

            yield return null;
        }
    }
    public void DashCooldown()
    {
        if (canUseDash) return;

        float flowTimeRate = (Time.time - dashStartTime) / commonData.dashCooldown;
        skillHUD.SkillCooldown(PlayerStateType.DASH, flowTimeRate);

        if (Time.time > dashStartTime + commonData.dashCooldown)
        {
            canUseDash = true;
            return;
        }
    }
    #endregion
}
