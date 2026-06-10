using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GhostMaskSkill : MonoBehaviour
{
    #region 외부
    private PlayerController playerController;
    private Player player;

    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private CameraController cameraController;
    [SerializeField] private MaskChange maskChange;
    [SerializeField] private HumanMaskSkill humanSkill;

    [SerializeField] private SkillHUD skillHUD;

    [SerializeField] private PlayerCameraEffect playerCameraEffect;
    [SerializeField] private PlayerSkillMove playerSkillMove;
    [SerializeField] private GameTimeScale playerTimeScale;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerEffect playerEffect;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerSound playerSound;

    //데이터
    private PlayerCommonData commonData;
    private PlayerGhostMaskData ghostData;
    #endregion

    #region 처형 스킬 오브젝트
    [Space(20)]
    [SerializeField] private GameObject humanWeapon;
    [SerializeField] private GameObject[] animalWeapon;
    [SerializeField] private GameObject ghostWeaponForHuman;
    [SerializeField] private GameObject ghostWeaponForAnimal;
    [SerializeField] private GameObject[] finishDomeEffect;
    [SerializeField] private GameObject[] finishCutEffect;
    [SerializeField] private GameObject finishDomeEffectPosition;
    [SerializeField] private GameObject finishCutEffectPosition;

    private List<Transform> finishTargetList;
    #endregion

    public bool canUseFinishSkill {get; private set;}   
    
    private float finishSkillStartTime;
    private bool isPerformingFinish = false;
    private bool isPerformingFinishAnim = false;

    private Coroutine coFinishSkill;

    [SerializeField] private CinemachineVirtualCamera skillCamera;
    private Animator skillCameraAnimator;


    private void Start()
    {
        playerController = PlayerController.instance;
        player = Player.instance;

        commonData = PlayerCommonData.Instance;
        ghostData = PlayerGhostMaskData.Instance;
        cameraController = CameraController.instance;

        skillCameraAnimator = skillCamera.GetComponent<Animator>();

        finishTargetList = new();
        canUseFinishSkill = true;
        coFinishSkill = null;

        ghostWeaponForAnimal.SetActive(false);
        ghostWeaponForHuman.SetActive(false);
    }

    public void InitializeSkill()
    {
        //코루틴
        if (coFinishSkill != null) StopCoroutine(coFinishSkill);

        //상태
        isPerformingFinish = false;
        isPerformingFinishAnim = false;

        //기능 초기화
        //playerTimeScale.Initialize();
        playerSkillMove.Initialize();
        playerEffect.Initialize();
        playerState.Initialize();
        playerSound.Initialize();

        //카메라
        cameraController.ChangeCamera(CameraType.DEFAULT);
    }

    //상시탐색
    public void DetectTargetToFinish()
    {
        if (!canUseFinishSkill)
        { 
            skillHUD.ActivateFinishHUD(false);
            return;
        }

        //리스트 클리어
        finishTargetList.Clear();

        //적 레이어의 타겟 감지
        Collider[] colliders = Physics.OverlapSphere
            (maskChange.CurrentMask.transform.position, CameraData.Instance.detectRange, CameraData.Instance.enemyLayer);

        int maxInkStack = 0;
        Collider targetCenter; //타겟기준(스택이 제일 높은 타겟) //이름 다시 짓기

        #region 타겟에 추가되는 조건
        for (int i = 0; i < colliders.Length; i++)
        {
            //적이 아니라면 + 죽었다면 패스
            if (!colliders[i].gameObject.GetComponent<Enemy>()) continue;
            if (colliders[i].gameObject.GetComponent<Enemy>().isDead) continue;

            //거리 밖이면 패스(타겟감지랑 중복됨)
            //float distance = Vector3.Distance(maskChange.CurrentMask.transform.position, colliders[i].transform.position);
            //if (distance > CameraData.Instance.detectRange) continue;

            #region 앞에 장애물이 있으면 패스
            //Vector3 direction = (colliders[i].transform.position - cameraController.MainCamera.transform.position).normalized; //타켓방향 벡터
            //if (Physics.Raycast(maskChange.CurrentMask.transform.position, direction, distance, CameraData.Instance.obstacleLayer))
            //{
            //    continue;
            //}
            #endregion

            //덧칠 풀스택이 존재하면 그 몬스터의 한계스택수 저장(더 큰게 나올수록 갱신)
            //그 외에는 컨티뉴
            if (!colliders[i].gameObject.GetComponent<CalliSystem>()) continue;
            if (!colliders[i].gameObject.GetComponent<CalliSystem>().IsPaintOverMax()) continue;

            int maxPaintOver = colliders[i].gameObject.GetComponent<CalliSystem>().MaxPaintOver;
            if (maxInkStack < maxPaintOver)
            {
                maxInkStack = maxPaintOver;
            }
        }

        //다시 포문을 돌려서 적들중에
        //해당 스택이랑 같거나 낮은 상대 모두 처형리스트에 추가
        for (int i = 0; i < colliders.Length; i++)
        {
            if (!colliders[i].gameObject.GetComponent<Enemy>()) continue;
            if (colliders[i].gameObject.GetComponent<Enemy>().isDead) continue;
            if (!colliders[i].gameObject.GetComponent<CalliSystem>()) continue;
            
            int enemyMaxPaintOver = colliders[i].gameObject.GetComponent<CalliSystem>().MaxPaintOver;
            if (maxInkStack >= enemyMaxPaintOver)
            {
                finishTargetList.Add(colliders[i].gameObject.transform);
            }
        }

        //타겟 감지 상태에 따라 아이콘 활성화
        if (finishTargetList.Count == 0 || (finishTargetList == null) || !canUseFinishSkill)
        {
            skillHUD.ActivateFinishHUD(false);
        }
        else
        {
            skillHUD.ActivateFinishHUD(true);
        }
        #endregion
    }

    //입력시 실행 함수
    public void Finish()
    {
        //조건 만족시 스킬 진행
        if (!CheckEnableFinish())
        {
            return;
        }

        InitializeSkill();

        if (coFinishSkill != null)
        {
            StopCoroutine(coFinishSkill);
        }

        canUseFinishSkill = false;
        coFinishSkill = StartCoroutine(CoFinish());
    }

    //카메라 정면과 카메라와 적 방향과의 각도 계산해서 처형가능한지 계산
    public bool CheckEnableFinish()
    {
        if (!canUseFinishSkill) return false;
        if (finishTargetList == null) return false;
        if (finishTargetList.Count <= 0) return false;

        //시야각 내에 타겟이 있다면 실행
        bool startSkill = false;
        for (int i = 0; i < finishTargetList.Count; i++)
        {
            Vector3 direction = (finishTargetList[i].transform.position - cameraController.MainCamera.transform.position).normalized; //타켓방향 벡터

            if (Vector3.Angle(cameraController.MainCamera.transform.forward, direction) < (ghostData.viewAngle * 0.5f))
            {
                startSkill = true;
                break;
            }
        }

        if (!startSkill)
        {
            return false;
        }

        //모든 조건 만족시 실행
        return true;
    }

    public IEnumerator CoFinish()
    {
        //무적
        playerState.ToggleInvincibleState(true);

        //if (playerMovement.Movement != Vector3.zero && !cameraController.CurrentTarget)
        //{
        //    maskChange.CurrentMask.transform.forward = playerMovement.Movement;
        //}

        //카메라
        cameraController.ChangeCamera(CameraType.FINISHSKILL);
        SkillCameraAnimation();

        //시간
        finishSkillStartTime = Time.time;
        canUseFinishSkill = false;

        //구분
        bool isHumanMask = false;

        //상태
        playerState.ChangePlayerState(PlayerStateType.GHOST_FINISHSKILL);
        playerState.ChangePlayerSubState(PlayerSubStateType.NONE);

        //스킬 사용중임을 알림
        playerSound.StopLoopingAudio();

        //적 정지
        Collider[] colliders = Physics.OverlapSphere
            (maskChange.CurrentMask.transform.position, CameraData.Instance.detectRange, CameraData.Instance.enemyLayer);

        if (colliders.Length > 0 )
        {
            foreach (var target in colliders)
            {
                if (!target) continue;
                if (!target.gameObject.GetComponent<Enemy>()) continue;
                if (!target.gameObject.GetComponent<NavMeshAgent>()) continue;
                if (!target.gameObject.GetComponent<NavMeshAgent>().enabled) continue;
                target.GetComponent<Enemy>().MotionStop(7);
            }
        }
        
        //애니메이션 출력
        if (maskChange.CurrentMask == maskChange.HumanMask)
        {
            maskChange.CurrentAnimator.CrossFade(playerAnimation.Ghost_HumanHitGround, 0);
            isHumanMask = true;
        }
        else
        {
            maskChange.CurrentAnimator.CrossFade(playerAnimation.Ghost_AnimalSweap, 0);
            
            for (int i = 0; i < animalWeapon.Length; i++)
            {
                animalWeapon[i].SetActive(true);
            }

            isHumanMask = false;
            
        }

        #region while 변수
        bool setGhostWeaponOnce = false;
        bool setOriginalWeaponOnce = false;
        bool killTargetsOnce = false;

        bool activeMoveOnce = false;
        bool activeEffectOnce = false;
        bool activeSoundOnce = false;
        bool activeCameraShake1Once = false;
        bool activeCameraShake2Once = false;
        bool activeTimeScaleOnce = false;
        #endregion

        while (true)
        {
            if (isHumanMask)
            {
                #region 셋팅
                if (!setGhostWeaponOnce && (Time.time >= finishSkillStartTime + ghostData.humanSetGhostWeaponTime))
                {
                    maskChange.ChangeMask(MaskType.GHOST,true,false);
                    humanWeapon.SetActive(false);
                    ghostWeaponForHuman.SetActive(true);
                    
                    setGhostWeaponOnce = true;
                }
                if (!setOriginalWeaponOnce && (Time.time >= finishSkillStartTime + ghostData.humanSetOriginalWeaponTime))
                {
                    maskChange.ChangeMask(MaskType.HUMAN,false,false);
                    humanWeapon.SetActive(true);
                    ghostWeaponForHuman.SetActive(false);
                    
                    setOriginalWeaponOnce = true;
                }
                if (!killTargetsOnce && (Time.time >= finishSkillStartTime + ghostData.humanKillTargetTime))
                {
                    if (finishTargetList != null || finishTargetList.Count != 0)
                    {
                        foreach (var target in finishTargetList)
                        {
                            //while문 
                            if (target.GetComponent<Enemy>() || !target.GetComponent<Enemy>().isDead)
                            {
                                target.GetComponent<Enemy>().Execution();
                            }
                        }

                        if (cameraController.CurrentTarget != null)
                        {
                            cameraController.LockOnTarget();
                        }

                        player.currentHP += 2;
                        if (player.currentHP >= 20)
                        {
                            player.currentHP = 20;
                        }
                        HpHUD.instance.ChangeHPStack((int)player.currentHP);

                        killTargetsOnce = true;
                    }
                }
                #endregion

                #region 애니메이션 상태
                var animatorStateInfo = maskChange.CurrentAnimator.GetCurrentAnimatorStateInfo(0);
                var animationHash = animatorStateInfo.shortNameHash;

                if (animationHash == playerAnimation.Ghost_HumanHitGround || animationHash == playerAnimation.Ghost_HumanSwing)
                {
                    isPerformingFinishAnim = true;
                }
                else
                {
                    if (isPerformingFinishAnim)
                    {
                        InitializeSkill();
                        yield break;
                    }
                }
                #endregion

                #region 물리이동
                if (!activeMoveOnce)
                {
                    for (int i = 0; i < ghostData.humanSkillMove.Length; i++)
                    {
                        playerSkillMove.StartCoroutine(playerSkillMove.SkillMove(ghostData.humanSkillMove[i]));
                    }
                    activeMoveOnce = true;
                }
                #endregion

                #region 제한
                    playerState.RestrictPlayer(ghostData.humanRestrict, finishSkillStartTime);
                #endregion

                #region 이펙트
                if (!activeEffectOnce)
                {
                    playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                        (ghostData.humanCutEffect, finishCutEffect, finishCutEffectPosition ));
                    playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                        (ghostData.humanDomeEffect, finishDomeEffect, finishDomeEffectPosition));
                    activeEffectOnce = true;
                }
                #endregion

                #region 소리
                if (!activeSoundOnce)
                {
                    //waitTime 값이 큰 struct부터 위에 배치
                    playerSound.SetPlayerSound(ghostData.humanHitGroundSound, player.transform.position, finishSkillStartTime);
                    playerSound.SetPlayerSound(ghostData.humanSwingSound, humanWeapon.transform.position, finishSkillStartTime);
                    playerSound.SetPlayerSound(ghostData.humanAfterSwingSound, player.transform.position, finishSkillStartTime);
                    activeSoundOnce = true;
                }
                
                #endregion

                #region 카메라 쉐이크
                if (!activeCameraShake1Once && (Time.time >= finishSkillStartTime + ghostData.humanFinishHitGroundCameraShake.waitTime))
                {
                    playerCameraEffect.ShakeCamera(ghostData.humanFinishHitGroundCameraShake);
                    activeCameraShake1Once = true;
                }
                if (!activeCameraShake2Once && (Time.time >= finishSkillStartTime + ghostData.humanFinishSwingCameraShake.waitTime))
                {
                    playerCameraEffect.ShakeCamera(ghostData.humanFinishSwingCameraShake);
                    activeCameraShake2Once = true;
                }
                #endregion

                #region 타임 스케일
                if (!activeTimeScaleOnce)
                {
                    playerTimeScale.StartCoroutine(playerTimeScale.CoSetTimeScale(ghostData.humanFinishTimeScale));
                    activeTimeScaleOnce = true;
                }
                #endregion
            }
            else
            {
                #region 셋팅
                if (!setGhostWeaponOnce && (Time.time >= finishSkillStartTime + ghostData.animalSetGhostWeaponTime))
                {
                    maskChange.ChangeMask(MaskType.GHOST, true, false);

                    for (int i = 0; i < animalWeapon.Length; i++)
                    {
                        animalWeapon[i].SetActive(false);
                    }

                    ghostWeaponForAnimal.SetActive(true);

                    setGhostWeaponOnce = true;
                }
                if (!setOriginalWeaponOnce && (Time.time >= finishSkillStartTime + ghostData.animalSetOriginalWeaponTime))
                {
                    maskChange.ChangeMask(MaskType.ANIMAL, false,false);

                    //동물 무기형상은 평소에 비활성화

                    ghostWeaponForAnimal.SetActive(false);
                    setOriginalWeaponOnce = true;
                }
                if (!killTargetsOnce && (Time.time >= finishSkillStartTime + ghostData.animalKillTargetTime))
                {
                    if (finishTargetList != null || finishTargetList.Count != 0)
                    {
                        foreach (var target in finishTargetList)
                        {
                            target.GetComponent<Enemy>().Execution();
                        }
                        if (cameraController.CurrentTarget != null)
                        {
                            cameraController.LockOnTarget();
                        }

                        player.currentHP += 2;
                        if (player.currentHP >= 20)
                        {
                            player.currentHP = 20;
                        }
                        HpHUD.instance.ChangeHPStack((int)player.currentHP);

                        killTargetsOnce = true;
                    }
                }
                #endregion

                #region 애니메이션 상태
                var animatorStateInfo = maskChange.CurrentAnimator.GetCurrentAnimatorStateInfo(0);
                var animationHash = animatorStateInfo.shortNameHash;

                if (animationHash == playerAnimation.Ghost_AnimalSweap || animationHash == playerAnimation.Ghost_AnimalSwing)
                {
                    isPerformingFinishAnim = true;
                }
                else
                {
                    if (isPerformingFinishAnim)
                    {
                        InitializeSkill();
                        yield break;
                    }
                }
                #endregion

                #region 물리이동
                if (!activeMoveOnce)
                {
                    for (int i = 0; i < ghostData.animalSkillMove.Length; i++)
                    {
                        playerSkillMove.StartCoroutine(playerSkillMove.SkillMove(ghostData.animalSkillMove[i]));
                    }
                    activeMoveOnce = true;
                }
                #endregion

                #region 제한
                playerState.RestrictPlayer(ghostData.animalRestrict, finishSkillStartTime);
                #endregion

                #region 이펙트
                if (!activeEffectOnce)
                {
                    playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                        (ghostData.animalCutEffect, finishCutEffect, finishCutEffectPosition));
                    playerEffect.StartCoroutine(playerEffect.TogglePlayerEffect
                        (ghostData.animalDomeEffect, finishDomeEffect, finishDomeEffectPosition));
                    activeEffectOnce = true;
                }
                #endregion

                #region 소리
                if (!activeSoundOnce)
                {
                    playerSound.SetPlayerSound(ghostData.animalSweapSound, player.transform.position,finishSkillStartTime);
                    playerSound.SetPlayerSound(ghostData.animalSwingSound, player.transform.position, finishSkillStartTime);
                    playerSound.SetPlayerSound(ghostData.animalAfterSwingSound, player.transform.position, finishSkillStartTime);
                    activeSoundOnce = true;
                }
                #endregion

                #region 카메라 쉐이크
                if (!activeCameraShake1Once && (Time.time >= finishSkillStartTime + ghostData.animalFinishSweapCameraShake.waitTime))
                {
                    playerCameraEffect.ShakeCamera(ghostData.animalFinishSweapCameraShake);
                    activeCameraShake1Once = true;
                }
                if (!activeCameraShake2Once && (Time.time >= finishSkillStartTime + ghostData.animalFinishSwingCameraShake.waitTime))
                {
                    playerCameraEffect.ShakeCamera(ghostData.animalFinishSwingCameraShake);
                    activeCameraShake2Once = true;
                }
                #endregion

                #region 타임 스케일
                if (!activeTimeScaleOnce)
                {
                    playerTimeScale.StartCoroutine(playerTimeScale.CoSetTimeScale(ghostData.animalFinishTimeScale));
                    activeTimeScaleOnce = true;
                }
                #endregion
            }

            yield return null;
        }
    }

    public void FinishSkillCooldown() //처형 쿨타임
    {
        if (canUseFinishSkill) return;

        float flowTimeRate = (Time.time - finishSkillStartTime) / ghostData.cooldown;
        skillHUD.SkillCooldown(PlayerStateType.GHOST_FINISHSKILL, flowTimeRate);

        if (Time.time > finishSkillStartTime + ghostData.cooldown)
        {
            canUseFinishSkill = true;
        }
    }

    public void SkillCameraAnimation()
    {
        if (maskChange.HumanMask.activeSelf)
        {
            skillCameraAnimator.CrossFade("HumanMaskFinish", 0);
        }
        else
        {
            skillCameraAnimator.CrossFade("AnimalMaskFinish", 0);
        }
    }
}
