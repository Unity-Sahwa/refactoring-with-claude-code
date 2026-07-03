using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    public class CameraController : MonoBehaviour
    {
        [Inject] ICharacterSwapNotifier characterSwapNotifier;
        [Inject] ICurrentCharacterProvider currentCharacterProvider;

        //대원_TODO: 아직 카메라가 어떤게 있을지 잘 모르지만 Swap 이벤트 연동은 필요함.
        [Inject] CinemachineCamera defaultCamera;

        private void Awake()
        {
            if(characterSwapNotifier != null)
            {
                characterSwapNotifier.OnCharacterSwapped += CameraTargetChanged;
            }
        }
        
        private void Start()
        {
            CameraTargetChanged();
        }


        private void OnDestroy()
        {
            if(characterSwapNotifier != null)
            {
                characterSwapNotifier.OnCharacterSwapped -= CameraTargetChanged;
            }
        }

        
        private void CameraTargetChanged()
        {
            defaultCamera.Target.TrackingTarget = currentCharacterProvider.CurrentCharacter.transform;
        }
    }
}
