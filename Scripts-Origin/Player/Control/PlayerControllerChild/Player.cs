using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public partial class Player : MonoBehaviour, IDamageable
{
    public static Player instance;

    #region 외부
    [SerializeField] private PlayerController playerController;
    [SerializeField] private MaskChange maskChange;
    public HpHUD hpHUD;

    [SerializeField] private HumanMaskSkill humanSkill;
    [SerializeField] private AnimalMaskSkill animalSkill;

    private SaveManager saveManager;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerSound playerSound;
    

    [SerializeField] private PlayerCameraEffect playerCameraEffect;
    [SerializeField] private LoadingUI loadingUI;
    [SerializeField] private UIEffect UIEffect;

    //데이터
    private PlayerCommonData commonData;
    #endregion

    //현재체력
    public float currentHP;

    //피격
    private float hitStartTime;
    [SerializeField] private bool canUseHitAction;
    
    //TODO: 어쩌면 RestrictPlayer랑 중복 기능할수도
    private bool isPerformingHitAction = false;
    public bool IsPerformingHitAction
    {
        get { return isPerformingHitAction; }
    }

    private bool isPerformingHitActionAnim;
    private bool isPlayerFalling;

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
        saveManager = SaveManager.instance;
        
        SetUp();
    }

    public void SetUp()
    {
        //이전씬의 체력가져오기

        currentHP = commonData.maxHp;
        canUseHitAction = true;
        isPlayerFalling = false;

    }

    public void InitializeSkill()
    {
        isPerformingHitActionAnim = false;
        isPerformingHitAction = false;
    }

    public void FollowCharacterObject()
    {
        this.gameObject.transform.position = maskChange.CurrentMask.transform.position;
        this.gameObject.transform.rotation = maskChange.CurrentMask.transform.rotation;
    }

    //외부에서 ApplyDamage 함수를 실행함.
    public bool ApplyDamage(DamageMessage damageMessage)
    {
        #region 리턴 조건
        //플레이어가 죽은 상태, 데미지양 = 0, 리액션 쿨타임, 무적상태 -> 리턴
        if ((playerState.playerCurrentState == PlayerStateType.DEAD) 
            || (damageMessage.amount <= 0) 
            || !canUseHitAction
            || playerState.isInvincible 
            || currentHP <= 0)
        {
            return false;
        }

        if (playerState.playerCurrentState == PlayerStateType.GHOST_FINISHSKILL)
        {
            return false;
        }
        #endregion

        currentHP -= damageMessage.amount;
        hpHUD.ChangeHPStack((int)currentHP);
        canUseHitAction = false;
        hitStartTime = Time.time;

        if (currentHP <= 0) 
        {
            playerState.ChangePlayerState(PlayerStateType.DEAD);
            playerState.ChangePlayerSubState(PlayerSubStateType.DEAD_HPZERO);
            DieAction();
        }
        else if (!playerState.isSuperArmor)
        {
            playerState.ChangePlayerState(PlayerStateType.HIT);
            playerState.ChangePlayerSubState(PlayerSubStateType.NONE);
            StartCoroutine(HitAction());
        }

        return true;
    }


    #region 치트
    public void RestoreHealth(DamageMessage damageMessage)
    {
        if (currentHP + damageMessage.amount < 20)
        {
            currentHP += damageMessage.amount;
            hpHUD.ChangeHPStack((int)currentHP);
        }
        else
        {
            currentHP = 20;
            hpHUD.ChangeHPStack((int)currentHP);
        }
    }
    #endregion

    #region 버튼
    public void Loading()
    {
        loadingUI.StartCoroutine(loadingUI.Loading());
    }
    #endregion

}

#region PlayerDamageReaction(스크립트 분리시키기)
public partial class Player : MonoBehaviour, IDamageable
{
    IEnumerator HitAction()
    {
        isPerformingHitAction = true;

        if (maskChange.HumanMask.activeSelf)
        {
            humanSkill.InitializeSkill();
        }
        else
        {
            animalSkill.InitializeSkill();
        }


        maskChange.CurrentRigidbody.velocity = Vector3.zero;

        //슈퍼아머가 아니면 맞으면서 행동제약
        if (maskChange.AnimalMask) maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_Hit, 0);
        if (maskChange.HumanMask) maskChange.CurrentAnimator.CrossFade(playerAnimation.Human_Hit, 0);


        hitStartTime = Time.time;

        #region while 변수
        bool activeSoundOnce = false;
        #endregion

        while (true)
        {
            #region Animation State
            var animatorStateInfo = maskChange.CurrentAnimator.GetCurrentAnimatorStateInfo(0);
            var animationHash = animatorStateInfo.shortNameHash;

            if (animationHash == playerAnimation.Human_Hit || animationHash == playerAnimation.Animal_Hit)
            {
                isPerformingHitActionAnim = true;
            }
            else
            {
                if (isPerformingHitActionAnim)
                {
                    InitializeSkill();
                    break;
                }
            }
            #endregion

            #region Restriction
            //수정이 필요함
            //playerState.RestrictPlayer(commonData.hitRestrict, hitStartTime);
            #endregion

            #region 소리
            if (!activeSoundOnce)
            {
                playerSound.Initialize();

                playerSound.SetPlayerSound(commonData.hitSound, Player.instance.transform.position, hitStartTime);
                playerSound.SetPlayerSound(commonData.heartBeatSound, Player.instance.transform.position, hitStartTime);
                activeSoundOnce = true;
            }
            #endregion

            yield return null;
        }
    }

    public void HitCooldown()
    {
        if (canUseHitAction) return;

        if (Time.time < hitStartTime + commonData.hitCooldown)
        {
            return;
        }
        canUseHitAction = true;
    }

    public void DieAction()
    {
        StartCoroutine(CoDieAction());
    }

    public IEnumerator CoDieAction()
    {
        //PlayerCurrentSubState에 따라 일반 죽음인지, 낙사인지 구별하기 
        //코루틴으로 형성

        if (playerState.playerCurrentState != PlayerStateType.DEAD)
        {
            yield break;
        }

        if (playerState.playerCurrentSubState == PlayerSubStateType.DEAD_FALL)
        {
            if(currentHP > 3)
            {
                //Recomposer
                playerCameraEffect.StartCoroutine(playerCameraEffect.ToggleCameraRecomposer(commonData.fallDeathCameraRecomposer));
                UIEffect.ShowFadeScreen(false, 1f);

                yield return new WaitForSeconds(1f);

                //낙사 이전에 저장한 데이터 불러오기
                //세이브 포인트에서 저장할 경우 인덱스 +1
                //불러오기를 할 경우
                saveManager.MoveToPreviousIndex();
                saveManager.LoadSlotData();

                //hp감소 후 다시 저장
                currentHP -= 3;
                hpHUD.ChangeHPStack((int)currentHP);
                saveManager.SaveSloatData();
                saveManager.MoveToNextIndex();

                ////CameraUIEffect
                //if (!fadeInOnce)
                //{
                //    cameraUIEffect.StartCoroutine(cameraUIEffect.ShowFadeInOutScreen(true));
                //    fadeInOnce = true;
                //}
                yield return new WaitForSeconds(1f);

                UIEffect.ShowFadeScreen(true, 1f);
                playerState.ChangePlayerState(PlayerStateType.IDLE);
                playerState.ChangePlayerSubState(PlayerSubStateType.NONE);

                UIEffect.instance.ShowPlayerHUDFadeEffect();

                yield break;
            }

            //5보다 작으면 죽음 판정으로 넘어감
            currentHP -= 3;
            hpHUD.ChangeHPStack((int)currentHP);
        }


        //HP 0 으로 사망할 때
        if (playerState.playerCurrentSubState == PlayerSubStateType.DEAD_HPZERO || currentHP <= 0)
        {
            //체력 0
            currentHP = 0;
            hpHUD.ChangeHPStack((int)currentHP);

            MenuUI.instance.DisablePlayerControl(true);

            //물리
            maskChange.CurrentRigidbody.velocity = Vector3.zero;
            UIEffect.instance.ShowPlayerHUDFadeEffect();

            //애니메이션
            if (maskChange.AnimalMask) maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_Die, 0);
            if (maskChange.HumanMask) maskChange.CurrentAnimator.CrossFade(playerAnimation.Human_Die, 0);

            //소리
            playerSound.SetPlayerSound(commonData.dieSound, Player.instance.transform.position, Time.time);
            playerSound.SetPlayerSound(commonData.afterDeadSound, Player.instance.transform.position, Time.time);

            //Recomposer
            playerCameraEffect.StartCoroutine(playerCameraEffect.ToggleCameraRecomposer(commonData.zeroHealthDeathCameraRecomposer));
            yield return new WaitForSeconds(.5f);
            UIEffect.ShowFadeScreen(false, 1f);

            yield return new WaitForSeconds(3);
            UIEffect.StartCoroutine(UIEffect.ShowDeathScreen());

            yield return new WaitForSeconds(3);
            SceneManager.LoadScene(0);
        }

        //낙사했을 때
    }

    //죽었을 때의 기능들 
    public bool CheckDie()
    {
        return playerState.playerCurrentState == PlayerStateType.DEAD;
    }

}
#endregion