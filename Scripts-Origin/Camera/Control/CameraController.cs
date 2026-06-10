using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum CameraType
{
    DEFAULT,
    LOCKON,
    FINISHSKILL
}

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    #region 외부
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SaveManager saveManager;

    private MaskChange maskChange;
    private Player player;

    [SerializeField] private PlayerState playerState;
    [SerializeField] private MenuUI menuUI;

    //데이터
    private PlayerCommonData commonData;
    private CameraData cameraData;
    #endregion
        
    #region 공통
    [SerializeField] private Camera mainCamera;
    public Camera MainCamera
    {
        get
        { return mainCamera; }

        private set 
        {
            mainCamera = value; 
        }
    }

    [SerializeField] private CinemachineFreeLook defaultCamera;
    public CinemachineFreeLook DefaultCamera
    {
        get
        {
            return defaultCamera;
        }
    }

    [SerializeField] private CinemachineVirtualCamera lockOnCamera;
    private CinemachineTransposer lockOnCameraTransposer;
    private CinemachineGroupComposer lockOnCameraGroupComposer;
    [SerializeField] private CinemachineVirtualCamera finishSkillCamera;
    
    [SerializeField] private Camera terrainLoadCamera;
    public Camera TerrainLoadCamera
    {
        get
        {
            return terrainLoadCamera;
        }
    }
    #endregion

    [SerializeField] private bool isLockOnTarget;
    [SerializeField] private bool isCurrentEnemyDead;

    #region Camera Move
    [SerializeField] private VariableJoystick joystick;
    #endregion

    #region 주목
    public Collider visibleTarget { get; private set; } //보이는 타겟
    public bool isTargetDetected { get; private set; } //주변에 감지되는 타겟
    public bool isTargetWithMaxStack { get; private set; } //주변에 잉크 풀스택의 타겟이 있는지

    //현재 타겟
    private Collider currentTarget;
    public Collider CurrentTarget { get { return currentTarget; } }
    
    //카메라가 바라보는 타겟의 위치
    private Transform headTransform;
    private Transform targetTransform;

    [SerializeField] private Image targetMarker;

    [SerializeField] CinemachineTargetGroup targetGroup;

    [SerializeField] private Material outlineMaterial;
    private Material[] targetMaterials;
    private GameObject outlineTarget;

    #endregion

    private void Awake()
    {
        #region 싱글톤
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);
        #endregion

        AdjustResolution();
        Application.targetFrameRate = 60;
        
        lockOnCamera.gameObject.SetActive(true);
    }

    void Start()
    {
        CameraInitialSet();
    }
    private void Update()
    {
        if (playerState.playerCurrentState == PlayerStateType.GHOST_FINISHSKILL)
        {
            return;
        }

        //타겟 항시 탐색
        DetectTargetAlways();
        CheckCurrentTargetState();
        ControlTargetMarker();
        CheckOutlineTarget();
        CameraHorizontalSwipe();
    }

    private void CameraInitialSet()
    {
        #region 외부
        maskChange = playerController.maskChange;
        player = playerController.player;
        
        commonData = PlayerCommonData.Instance;
        cameraData = CameraData.Instance;
        #endregion

        #region 카메라
        lockOnCameraTransposer = lockOnCamera.GetCinemachineComponent<CinemachineTransposer>();
        lockOnCameraGroupComposer = lockOnCamera.GetCinemachineComponent<CinemachineGroupComposer>();

        defaultCamera.gameObject.SetActive(true);
        lockOnCamera.gameObject.SetActive(false);
        #endregion

        #region 락온 마커
        targetMarker.gameObject.SetActive(true);
        targetMarker.color = cameraData.detectedTargetMarkerColor;
        #endregion

        //마우스
        if (menuUI.MainMenu.activeSelf)
        {
            defaultCamera.m_XAxis.m_MaxSpeed = 0;
            defaultCamera.m_YAxis.m_MaxSpeed = 0;
        }
        else
        {
            defaultCamera.m_XAxis.m_MaxSpeed = saveManager.mouseSpeedWithXAxis;
            defaultCamera.m_YAxis.m_MaxSpeed = saveManager.mouseSpeedWithYAxis;
        }

        //카메라 초기에 수치값 조정
        StartCoroutine(HoldDefaultCameraValue());
    }
    private void AdjustResolution()
    {
        int targetWidth; // Target horizontal resolution
        int targetHeight; // Target vertical resolution
#if UNITY_ANDROID
        targetWidth = 1280;
        targetHeight = 720;
#else
        targetWidth = 1920;
        targetHeight = 1080;
#endif

        // Calculate the device's aspect ratio
        float deviceAspectRatio = (float)Screen.width / Screen.height;

        // Calculate the target pixel count
        int targetPixelCount = targetWidth * targetHeight;

        // Compute adjusted resolution to match target pixel count
        float adjustedHeight = Mathf.Sqrt(targetPixelCount / deviceAspectRatio);
        float adjustedWidth = adjustedHeight * deviceAspectRatio;

        // Ensure the resolution is a multiple of 2 for better GPU performance
        //adjustedWidth = Mathf.RoundToInt(adjustedWidth / 2f) * 2;
        //adjustedHeight = Mathf.RoundToInt(adjustedHeight / 2f) * 2;

        // Log the adjusted resolution for debugging
        Debug.Log($"Adjusted Resolution: {adjustedWidth}x{adjustedHeight}");

        // Set the screen resolution (fullscreen mode)
        Screen.SetResolution((int)adjustedWidth, (int)adjustedHeight, true);

        // Adjust render scale for Universal Render Pipeline (URP)
        //if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urpAsset)
        //{
        //    float renderScale = Mathf.Clamp((float)adjustedWidth / targetWidth, 0.5f, 1.0f);
        //    urpAsset.renderScale = renderScale;
        //    Debug.Log($"Render Scale set to: {renderScale}");
        //}
        //else
        //{
        //    Debug.LogWarning("Render scale adjustment skipped: Not using Universal Render Pipeline.");
        //}

        // Update all CanvasScaler components in the scene
        //CanvasScaler[] canvasScalers = FindObjectsOfType<CanvasScaler>();
        //if (canvasScalers.Length == 0)
        //{
        //    Debug.LogWarning("No CanvasScaler found. Ensure your UI uses CanvasScaler for proper scaling.");
        //}
        //foreach (CanvasScaler canvasScaler in canvasScalers)
        //{
        //    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        //    canvasScaler.referenceResolution = new Vector2(targetWidth, targetHeight);
        //}
    }
    public void SetPCPlatform(bool isSet)
    {
        if (isSet)
        {
            //디폴트 카메라 회전들어가도록
            defaultCamera.m_YAxis.m_InputAxisName = "Mouse Y";
            defaultCamera.m_XAxis.m_InputAxisName = "Mouse X";
        }
        else
        {
            defaultCamera.m_YAxis.m_InputAxisName = "";
            defaultCamera.m_XAxis.m_InputAxisName = "";
        }
    }
    public void ChangeCamera(CameraType cameraType)
    {
        if (cameraType == CameraType.LOCKON)
        {
            defaultCamera.gameObject.SetActive(false);
            lockOnCamera.gameObject.SetActive(true);
            finishSkillCamera.gameObject.SetActive(false);

            StartCoroutine(HoldLockOnCameraValue());

        }
        else if(cameraType == CameraType.FINISHSKILL)
        {
            defaultCamera.gameObject.SetActive(false);
            lockOnCamera.gameObject.SetActive(false);
            finishSkillCamera.gameObject.SetActive(true);
        }
        else
        {
            defaultCamera.gameObject.SetActive(true);
            lockOnCamera.gameObject.SetActive(false);
            finishSkillCamera.gameObject.SetActive(false);

            if (PlatformSwitcher.instance.IsPCPlatform == false)
            {
                StartCoroutine(HoldDefaultCameraValue());
            }
        }
    }
    public IEnumerator HoldDefaultCameraValue()
    {
        bool isBossScene = (SceneManager.GetActiveScene().buildIndex == 4);
        var defaultCameraTopRig = defaultCamera.GetRig(0).GetCinemachineComponent<CinemachineComposer>(); //Top Rig
        var defaultCameraMiddleRig = defaultCamera.GetRig(1).GetCinemachineComponent<CinemachineComposer>(); //Middle Rig
        var defaultCameraBottomRig = defaultCamera.GetRig(2).GetCinemachineComponent<CinemachineComposer>(); //Bottom Rig
        
        float time = 0;

        while(true)
        {
            time += Time.deltaTime;

            if (!defaultCamera.gameObject.activeSelf)
            {
                break;
            }

            if (isBossScene)
            {
                defaultCamera.m_Orbits[0].m_Height = 10;
                defaultCamera.m_Orbits[0].m_Radius = 15;
                defaultCameraTopRig.m_TrackedObjectOffset.y = 4.7f;

                defaultCamera.m_Orbits[1].m_Height = 5;
                defaultCamera.m_Orbits[1].m_Radius = 15;
                defaultCameraMiddleRig.m_TrackedObjectOffset.y = 2.5f;

                defaultCamera.m_Orbits[2].m_Height = 1;
                defaultCamera.m_Orbits[2].m_Radius = 11;
                defaultCameraBottomRig.m_TrackedObjectOffset.y = 1.4f;  
            }
            else
            {
                defaultCamera.m_YAxis.Value = 0.5f;
            }


            if (time >= 2)
            {
                break;
            }

            yield return null; 
        }
    }
    private IEnumerator HoldLockOnCameraValue()
    {
        bool isBossScene = (SceneManager.GetActiveScene().buildIndex == 4);

        float time = 0;

        Vector3 lockOnCameraFollowOffset = new Vector3(0, 2.04f, -8.57f);
        Vector3 lockOnCameraTrackedObjectOffset = new Vector3(0, 1.08f, 0f);

        Vector3 lockOnCameraFollowOffset_4 = new Vector3(0, 4.73f, -10.34f);
        Vector3 lockOnCameraTrackedObjectOffset_4 = new Vector3(0, 2.2f, 0f);

        while (true)
        {
            time += Time.deltaTime;

            if (!lockOnCamera.gameObject.activeSelf)
            {
                break;
            }

            if (isBossScene)
            {
                lockOnCameraGroupComposer.m_TrackedObjectOffset = lockOnCameraTrackedObjectOffset_4;
                lockOnCameraTransposer.m_FollowOffset = lockOnCameraFollowOffset_4;
            }
            else
            {
                lockOnCameraGroupComposer.m_TrackedObjectOffset = lockOnCameraTrackedObjectOffset;
                lockOnCameraTransposer.m_FollowOffset = lockOnCameraFollowOffset;
            }


            if (time >= 2)
            {
                break;
            }

            yield return null;
        }
    }

    public void SetMouseSpeed(bool isXAxis, float value)
    {
        if (isXAxis)
        {
            defaultCamera.m_XAxis.m_MaxSpeed = value;
        }
        else
        {
            defaultCamera.m_YAxis.m_MaxSpeed = value;
        }
    }
    public float GetMouseSpeed(bool isXAxis)
    {
        if (isXAxis)
        {
            return defaultCamera.m_XAxis.m_MaxSpeed;
        }
        else
        {
            return defaultCamera.m_YAxis.m_MaxSpeed;
        }
    }

    public void SetBossRoomCamera()
    {
        defaultCamera.m_Orbits[0].m_Height = 10;
        defaultCamera.m_Orbits[0].m_Radius = 15;
        defaultCamera.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.y = 4.7f;

        defaultCamera.m_Orbits[1].m_Height = 5;
        defaultCamera.m_Orbits[1].m_Radius = 15;
        defaultCamera.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.y = 2.5f;

        defaultCamera.m_Orbits[2].m_Height = 1;
        defaultCamera.m_Orbits[2].m_Radius = 11;
        defaultCamera.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.y = 1.4f;
    }

    #region 주목
    private void DetectTargetAlways()
    {
        visibleTarget = null;

        if (playerState.playerCurrentState == PlayerStateType.DEAD)
        {
            return;
        }

        Collider[] colliders
            = Physics.OverlapSphere(maskChange.CurrentMask.transform.position, CameraData.Instance.detectRange, CameraData.Instance.enemyLayer);

        //주변에 적이없거나 플레이어 사망시 반환
        if (colliders.Length == 0 || colliders == null)
        {
            headTransform = null;
            isTargetDetected = false;
            return;
        }
        else
        {
            UIEffect.instance.ShowPlayerHUDFadeEffect();
            isTargetDetected = true;
        }

        #region 조건에 맞는 타겟 선별
        //조건1: 시야각 / idDead / 거리
        float smallestAngle = Mathf.Infinity;
       
        float smallestDistance = cameraData.distanceWithCloseTarget;
        bool isEnemyInRange = false;
        
        isTargetWithMaxStack = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject.GetComponent<Enemy>() == null) continue; //적이 없으면 패스
            if (colliders[i].gameObject.GetComponent<Enemy>().isDead) continue; //적이 죽으면 패스

            Vector3 directionTowardTarget = colliders[i].transform.position - MainCamera.transform.position;
            directionTowardTarget.y = 0;
            Vector3 cameraForward = player.transform.position - MainCamera.transform.position;
            cameraForward.y = 0;
            float angleWithTarget = Vector3.Angle(directionTowardTarget, cameraForward);

            Vector3 targetPosition = colliders[i].gameObject.transform.position;
            targetPosition.y = 0;
            Vector3 characterPosition = maskChange.CurrentMask.transform.position;
            characterPosition.y = 0;
            float distanceWithTarget = Vector3.Distance(characterPosition, targetPosition);

            //일정 각도를 벗어나면 패스
            if (angleWithTarget > cameraData.maximumAngleWithTarget) continue;
            //일정 거리를 벗어나면 패스
            if (distanceWithTarget > cameraData.maximumDistanceWithTarget ) continue;

            #region Activate HUD
            if (colliders[i].GetComponent<CalliSystem>())
            {
                if (colliders[i].GetComponent<CalliSystem>().IsPaintOverMax())
                {
                    isTargetWithMaxStack = true;

                    if(SceneManager.GetActiveScene().buildIndex == 0)
                    {
                        if (colliders[i].GetComponent<EnemyMino>())
                        {
                            Invoke("InvokeFinishGuide", 1.2f);
                        }
                    }
                }
            }
            #endregion

            //각도가 가장 작은 오브젝트 선별
            if (distanceWithTarget < smallestDistance)
            {
                smallestDistance = distanceWithTarget;
                visibleTarget = colliders[i];

                isEnemyInRange = true;
            }

            //가까운 적이 있다면 우선시함
            if (isEnemyInRange) continue;

            if (angleWithTarget < smallestAngle)
            {
                //각도가 가장 작은 오브젝트 선별
                smallestAngle = angleWithTarget;
                visibleTarget = colliders[i];
            }

            //현재타겟이 없다면 탐지된 타겟 머리위치 저장(마커표시를 위함) 
            if (currentTarget == null)
            {
                headTransform = visibleTarget.transform.Find("HeadPosition");
            }
        }
        #endregion
    }
   
    private void CheckOutlineTarget()
    {
        //visibleTarget이 없으면 outline 제거
        if (visibleTarget == null)
        {
            ClearOutline(); 
            return;
        }

        //visibleTarget이 있고 outlineTarget이 없으면 outline 그리기
        else if((visibleTarget != null) && (outlineTarget == null))
        {
            DrawOutlineOnTarget();
            return;
        }

        //visibleTarget이 있고 outlineTarget이랑 같으면 반환
        else if ((visibleTarget != null) && (visibleTarget.gameObject == outlineTarget.transform.parent.gameObject))
        {
            return;
        }

        //visibleTarget이 있고 outlineTarget이랑 다르면 outline 제거
        else if ((visibleTarget != null) && (visibleTarget.gameObject != outlineTarget.transform.parent.gameObject))
        {
            ClearOutline();
            return;
        }
    }
    private void DrawOutlineOnTarget()
    {
        //탐지된 타겟이 있고 탐지범위 내에 있다면 
        if (visibleTarget.GetComponent<BrokenObject>() || visibleTarget.GetComponent<WisuSuppressionController>())
        {
            for (int i = 0; i < visibleTarget.transform.childCount; i++)
            {
                var childObject = visibleTarget.transform.GetChild(i);

                if (childObject.gameObject.CompareTag("OutlineTarget"))
                {
                    //outlineTarget에 target저장
                    outlineTarget = childObject.gameObject;

                    //target이 가진 materials 일단 저장
                    var targetMeshRenderer = outlineTarget.GetComponent<MeshRenderer>();
                    targetMaterials = targetMeshRenderer.materials;

                    //outlineMaterial 하나 추가해주기
                    Material[] newMaterials = new Material[targetMaterials.Length + 1];
                    for (int j = 0; j < targetMaterials.Length; j++)
                    {
                        newMaterials[j] = targetMaterials[j];
                    }
                    newMaterials[newMaterials.Length - 1] = outlineMaterial;
                    targetMeshRenderer.materials = newMaterials;
                }
            }
        }
    }
    private void ClearOutline()
    {
        if (outlineTarget == null)
        {
            return;
        }

        var targetMeshRenderer = outlineTarget.GetComponent<MeshRenderer>();
        targetMeshRenderer.materials = targetMaterials;

        targetMaterials = null;
        outlineTarget = null;
    }
    private void InvokeFinishGuide()
    {
        PlayGuide.instance.ShowTutorialUI(TutorialState.FINISHATTACK);
    }
    private void CheckCurrentTargetState()
    {
        //락온 기능 활성 상태
        if (isLockOnTarget)
        {
            //적이 죽었을 때 주목 자동 이동
            if (currentTarget == null || 
                currentTarget.IsDestroyed() || 
                (currentTarget.gameObject.TryGetComponent<Enemy>(out Enemy enemy) && enemy.isDead) ||
                (enemy.hpBarCount <= 0))
            {
                //주변에 적이 있으면 자동 타겟
                if (visibleTarget)
                {
                    targetGroup.RemoveMember(targetTransform);

                    isLockOnTarget = true;
                    currentTarget = visibleTarget;
                    visibleTarget = null;

                    headTransform = currentTarget.transform.Find("HeadPosition");

                    if (headTransform == null)
                    {
                        targetTransform = currentTarget.transform;

                    }
                    else
                    {
                        targetTransform = headTransform;
                    }

                    targetGroup.AddMember(targetTransform, 1f, 1);

                }
                else
                {
                    DeactivateLockOn();
                }
            }
            else if ( Vector3.Distance(maskChange.CurrentMask.transform.position, currentTarget.gameObject.transform.position) > cameraData.detectRange)
            {
                DeactivateLockOn();
            }
        }
    }
    public void DeactivateLockOn()
    {
        targetGroup.RemoveMember(targetTransform);

        maskChange.CurrentAnimator.SetBool("isFocused", false);

        isLockOnTarget = false;
        currentTarget = null;
        targetTransform = null;
        headTransform = null;

        ChangeCamera(CameraType.DEFAULT);

        return;
    }
    private void ControlTargetMarker()
    {
        if (currentTarget)
        {
            if (!targetTransform.GetComponent<CapsuleCollider>())
            {
                return;
            }
            //currentTarget이 있을 때 마커의 움직임, 투명도
            targetMarker.gameObject.SetActive(true);

            Vector3 targetPosition;
            if (headTransform)
            {
                targetPosition = targetTransform.position ;
            }
            else
            {
                CapsuleCollider targetCollider = targetTransform.GetComponent<CapsuleCollider>();
                float targetHeight = targetCollider.transform.localScale.y * (targetCollider.center.y + targetCollider.height * 0.5f);

                targetPosition = new Vector3(targetTransform.position.x, targetTransform.position.y + targetHeight, targetTransform.position.z) + cameraData.targetMarkerOffset;
            }
            Vector3 targetScreenPoint = MainCamera.WorldToScreenPoint(targetPosition);
            targetMarker.transform.position = targetScreenPoint;

            targetMarker.color = CameraData.Instance.LockOnTargetMarkerColor;
        }
        else if (visibleTarget)
        {
            if (!visibleTarget.GetComponent<CapsuleCollider>())
            {
                return;
            }

            targetMarker.gameObject.SetActive(true);
            Vector3 targetPosition;
            
            if (headTransform)
            {
                targetPosition = headTransform.position;
            }
            else
            {
                CapsuleCollider targetCollider = visibleTarget.GetComponent<CapsuleCollider>();
                    
                float targetHeight = targetCollider.transform.localScale.y * (targetCollider.center.y + targetCollider.height * 0.5f);

                targetPosition = 
                    new Vector3(visibleTarget.transform.position.x, visibleTarget.transform.position.y + targetHeight, visibleTarget.transform.position.z) + cameraData.targetMarkerOffset;
            }


            Vector3 targetScreenPoint = MainCamera.WorldToScreenPoint(targetPosition);
            targetMarker.transform.position = targetScreenPoint;

            targetMarker.color = CameraData.Instance.detectedTargetMarkerColor;
        }
        else
        {
            targetMarker.gameObject.SetActive(false);
        }
    }
    public void LockOnTarget()
    {
        //락온 상태에서 한번 더 기능실행시 기능 비활성화
        if (isLockOnTarget)
        {
            DeactivateLockOn();

            return;
        }

        //detectedTarget가 있으면 currentTarget으로
        if (visibleTarget)
        {
            isLockOnTarget = true;
            currentTarget = visibleTarget;
            visibleTarget = null;

            maskChange.CurrentAnimator.SetBool("isFocused", true);

            targetTransform = currentTarget.transform.Find("HeadPosition");

            //(수정사항)락온시 타겟마커활성화 >> 지속 업데이트
            targetMarker.gameObject.transform.localScale = cameraData.targetMarkerScale;


            if (targetTransform == null)
            {
                targetTransform = currentTarget.transform;
            }
            
            targetGroup.AddMember(targetTransform, 1f, 1);

            ChangeCamera(CameraType.LOCKON);
        }
    }
    #endregion
    public void CameraHorizontalSwipe()
    {
        if (joystick.Horizontal == 0) return;

        defaultCamera.m_XAxis.Value += joystick.Horizontal * cameraData.cameraHorizontalSwipeRate;
    }
}