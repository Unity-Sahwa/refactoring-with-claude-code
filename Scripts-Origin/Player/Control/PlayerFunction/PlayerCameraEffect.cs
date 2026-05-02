using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class PlayerCameraEffect : MonoBehaviour
{
    [SerializeField] private PlayerState playerState;

    [SerializeField] private GameObject[] cameraObject;

    [Header("<CinemachineImpulseSource> 보유한 플레이어 넣기")]
    public CinemachineImpulseSource impulseSource;

    [Header("Cinemachine > Presets > Noise 에셋 그대로 넣기")]
    public NoiseSettings[] secondaryNoise;
    public void Start()
    {
        for (int i = 0; i < cameraObject.Length; i++)
        {
            CinemachineRecomposer camera = cameraObject[i].GetComponent<CinemachineRecomposer>();
            camera.m_ZoomScale = 1;
            camera.m_FollowAttachment = 1;
            camera.m_LookAtAttachment = 1;
        }
    }

    public void Initialize()
    {
        if (playerState.playerCurrentState == PlayerStateType.DEAD)
        {
            return;
        }
    }

    public void ShakeCamera(CameraShakeStruct cameraShake)
    {
        if (!cameraShake.useFunction)
        {
            return;
        }

        #region 카메라 선택
        GameObject currentCamera = null;
        for (int i = 0; i < cameraObject.Length; i++)
        {
            if (!cameraObject[i].activeSelf)
            {
                continue;
            }

            currentCamera = cameraObject[i];
            break;
        }
        #endregion

        #region secondaryShake 값 적용
        CinemachineImpulseListener listener = currentCamera.GetComponent<CinemachineImpulseListener>();
        //m_SecondaryNoise -> 에셋형태로 되어있음. 배열을 똑같이 해주어서 편하게 짝지어주기
        listener.m_ReactionSettings.m_SecondaryNoise = secondaryNoise[(int)cameraShake.reactionType];
        listener.m_ReactionSettings.m_AmplitudeGain = cameraShake.amplitudeGain;
        listener.m_ReactionSettings.m_FrequencyGain = cameraShake.frequencyGain;
        listener.m_ReactionSettings.m_Duration = cameraShake.reactionDuration;
        #endregion

        #region impulseSource 값 적용
        switch (cameraShake.shakeType)
        {
            case CameraShakeType.IMPULSE_RECOIL:
                impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Recoil;
                break;
            case CameraShakeType.IMPULSE_BUMP:
                impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
                break;
            case CameraShakeType.IMPULSE_EXPOLOSION:
                impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
                break;
            case CameraShakeType.IMPULSE_RUMBLE:
                impulseSource.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Rumble;
                break;
        }
        impulseSource.m_ImpulseDefinition.m_ImpulseDuration = cameraShake.impulseDuration;
        impulseSource.GenerateImpulseWithVelocity(cameraShake.impulseVelocty);
        #endregion
    }

    public IEnumerator ToggleCameraRecomposer(CameraRecomposerStruct cameraRecomposer)
    {
        if (!cameraRecomposer.useFunction)
        {
            yield break;
        }
        #region 카메라 선택
        GameObject currentCamera = null;
        for (int i = 0; i < cameraObject.Length; i++)
        {
            if (!cameraObject[i].activeSelf)
            {
                continue;
            }

            currentCamera = cameraObject[i];
            break;
        }

        CinemachineRecomposer camera = currentCamera.GetComponent<CinemachineRecomposer>();
        #endregion

        if (cameraRecomposer.waitTime != 0)
        {
            yield return new WaitForSeconds(cameraRecomposer.waitTime);
        }

        camera.m_ZoomScale = cameraRecomposer.zoomScale;
        camera.m_FollowAttachment = cameraRecomposer.followAttachment;
        camera.m_LookAtAttachment = cameraRecomposer.lookAtAttachment;
         
        yield return new WaitForSeconds(cameraRecomposer.duration);

        camera.m_ZoomScale = 1;
        camera.m_FollowAttachment = 1;
        camera.m_LookAtAttachment = 1;
    }
}
