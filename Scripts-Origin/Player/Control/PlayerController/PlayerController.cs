using UnityEngine;

public class PlayerController : MonoBehaviour 
{
    //주의점: 자식오브젝트가 비활성화인 상태에서 시작하면 자식오브젝트으 스크립트의 Awake, Start 함수가 호출되지 않음

    public static PlayerController instance;

    private SaveManager saveManager;
    [SerializeField] private MenuUI menuUI;
    [SerializeField] private PlayerSkillInput playerSkillInput;

    #region 외부
    [SerializeField] public PlayerMovement playerMovement;
    [SerializeField] public MaskChange maskChange;
    [SerializeField] public HumanMaskSkill humanMaskSkill;
    [SerializeField] public AnimalMaskSkill animalMaskSkill;
    [SerializeField] public GhostMaskSkill ghostMaskSkill;
    [SerializeField] public Player player;

    [SerializeField] private PlayerSkillMove playerSkillMove;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerAnimation playerAnimation;

    //데이터
    private PlayerCommonData commonData;
    private PlayerHumanMaskData humanData;
    private PlayerAnimalMaskData animalData;
    #endregion

    private void Awake()
   {
        #region 싱글톤
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);
        #endregion

        commonData = PlayerCommonData.Instance;
        humanData = PlayerHumanMaskData.Instance;
        animalData = PlayerAnimalMaskData.Instance;

        maskChange = GetComponent<MaskChange>();
        playerMovement = GetComponent<PlayerMovement>();
        
        humanMaskSkill = GetComponentInChildren<HumanMaskSkill>();
        animalMaskSkill = GetComponentInChildren<AnimalMaskSkill>();
        ghostMaskSkill = GetComponentInChildren<GhostMaskSkill>(    );

        player = GetComponentInChildren<Player>();

        maskChange.InitialSetUp();
    }
    private void Start()
    {
        saveManager = SaveManager.instance;
    }
    private void Update()
    {
        #region 셋팅 입력

        if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.MENU]))
        {
            menuUI.MenuSwitch();
        }

        if (menuUI.isPlayerControlDisabled)
        {
            return;
        }

        
        #endregion

        if (playerState.playerCurrentState == PlayerStateType.DEAD) return;

        #region 플레이어 상태
        //애니메이션 상태
        playerAnimation.UpdateAnimationState();

        //전투상태
        if (!humanMaskSkill.canUseDash ||
            !humanMaskSkill.canUseInkShape ||
            !animalMaskSkill.canUseDash ||
            !animalMaskSkill.canUseLeapStrike ||
            !ghostMaskSkill.canUseFinishSkill ||
            CameraController.instance.isTargetDetected)
        {
            UIEffect.instance.IsPlayerHUDFading(true);
        }
        else
        {
            UIEffect.instance.IsPlayerHUDFading(false);
        }
        #endregion

        #region 쿨타임
        maskChange.ChangeMaskCooldown();
        player.HitCooldown();

        //에러나는 이유 비활성화되어서 작동이 안됨
        if (maskChange.CurrentMask == maskChange.HumanMask)
        {
            humanMaskSkill.InkFloorCooldown();
            humanMaskSkill.InkShapeCooldown();
            humanMaskSkill.DashCooldown();
        }
        else
        {
            animalMaskSkill.LeapStrikeCooldown();
            animalMaskSkill.roarCooldown();
            animalMaskSkill.DashCooldown();
        }

        ghostMaskSkill.FinishSkillCooldown();

        #endregion

        if (playerState.playerCurrentState == PlayerStateType.GHOST_FINISHSKILL)
        {
            return;
        }

        #region 카메라 입력
        if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.LOCKONTARGET]))
        {
            CameraController.instance.LockOnTarget();
        }
        #endregion

        #region 방향 입력
        //입력 값 조절
        playerMovement.InputMovement();
        #endregion

        if (playerState.doNotAct)
        {
            return;
        }

        #region 탐지
        ghostMaskSkill.DetectTargetToFinish();
        #endregion

        if (!PlatformSwitcher.instance.IsPCPlatform)
        {
            return;
        }

        #region 플레이 입력

        if (player.IsPerformingHitAction)
        {
            return;
        }


        if (maskChange.CurrentMask == maskChange.HumanMask)
        {
            if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.ATTACK_NORMAL]))
            {
                playerSkillInput.StoreInput(PlayerStateType.HUMAN_NORMALATTACK);

                if (!playerState.isPerfomingSklill) humanMaskSkill.NormalAttack();
            }
            else if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.ATTACK_NORMAL]))
            {
                //playerSkillInput.StoreInput(PlayerStateType.HUMAN_INKSHAPE);
                //if (!playerState.isPerfomingSklill) humanMaskSkill.InkShape();
            }
            else if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.ATTACK_SPECIAL]))
            {
                playerSkillInput.StoreInput(PlayerStateType.HUMAN_INKSHAPE);
                if (!playerState.isPerfomingSklill) humanMaskSkill.InkShape();
            }
            //else if (Input.GetKeyDown(inputData.secondSkill))
            //{
            //    playerSkillInput.StoreInput(PlayerStateType.HUMAN_INKFLOOR);
            //    if (!playerState.isPerfomingSklill)  humanMaskSkill.InkFloor();
            //}
            else if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.DASH]))
            {
                playerSkillInput.StoreInput(PlayerStateType.DASH);
                if (!playerState.isPerfomingSklill) humanMaskSkill.Dash();
            }
            else if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.ATTACK_FINISH]))
            {
                playerSkillInput.StoreInput(PlayerStateType.GHOST_FINISHSKILL);
                if (!playerState.isPerfomingSklill) ghostMaskSkill.Finish();
            }
        }
        else
        {
            if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.ATTACK_NORMAL]))
            {
                //if (playerState.playerCurrentSubState == PlayerSubStateType.ANIMAL_THIRDNORMALATTACK)
                //{
                //    Debug.Log("3타");
                //    playerSkillInput.StoreInput(PlayerStateType.ANIMAL_LEAPSTRIKE);
                //}
                //else
                playerSkillInput.StoreInput(PlayerStateType.ANIMAL_NORMALATTACK);

                if (!playerState.isPerfomingSklill) animalMaskSkill.NormalAttack();
            }
            else if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.ATTACK_NORMAL]))
            {
                //playerSkillInput.StoreInput(PlayerStateType.ANIMAL_LEAPSTRIKE);
                //if (!playerState.isPerfomingSklill)  animalMaskSkill.LeapStrike();
            }
            else if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.ATTACK_SPECIAL]))
            {
                playerSkillInput.StoreInput(PlayerStateType.ANIMAL_LEAPSTRIKE);
                if (!playerState.isPerfomingSklill) animalMaskSkill.LeapStrike();
            }
            //else if (Input.GetKeyDown(inputData.secondSkill))
            //{
            //    playerSkillInput.StoreInput(PlayerStateType.ANIMAL_ROAR);
            //    if (!playerState.isPerfomingSklill) animalMaskSkill.Roar();
            //}
            else if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.DASH]))
            {
                playerSkillInput.StoreInput(PlayerStateType.DASH);
                if (!playerState.isPerfomingSklill) animalMaskSkill.Dash();
            }
        }

        if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.ATTACK_FINISH]))
        {
            playerSkillInput.StoreInput(PlayerStateType.GHOST_FINISHSKILL);
            if (!playerState.isPerfomingSklill) ghostMaskSkill.Finish();
        }

        #endregion
    }
    private void FixedUpdate()
    {
        #region Player 오브젝트가 캐릭터 오브젝트 따라가기
        player.FollowCharacterObject();
        #endregion

        #region 추가적인 중력값
        if (!CheatMode.instance.isFlyMode)
        {
            playerMovement.AddGravity();
        }
        #endregion


        if (menuUI.isPlayerControlDisabled)
        {
            return;
        }

        #region 스킬로 인한 이동
        playerSkillMove.UpdateSkillMovement();
        #endregion

        if (playerState.playerCurrentState == PlayerStateType.DEAD) return;

        if (playerState.playerCurrentState == PlayerStateType.GHOST_FINISHSKILL)
        {
            return;
        }

        if (playerState.doNotAct)
        {
            return;
        }

        #region 회전, 이동
        if (!playerState.doNotRotate && !player.IsPerformingHitAction)
        {
            playerMovement.CharacterRotate();
        }

        if (!playerState.doNotMove && !player.IsPerformingHitAction)
        {
            playerMovement.CharacterMove();
        }
        #endregion
    }
}