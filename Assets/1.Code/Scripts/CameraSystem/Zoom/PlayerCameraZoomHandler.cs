using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 상태 이벤트(CameraZoom)를 받아 스킬 연출용 카메라 거리 줌을 실행한다.
    // 흐름: 이벤트 수신 → 켜진 카메라에 맞는 거리 적용자 생성 → 멀어짐 → 유지 → 복귀
    public class PlayerCameraZoomHandler : MonoBehaviour
    {
        [Preserve, Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;
        [Preserve, Inject(true)] private ICurrentCameraProvider _currentCameraProvider;

        private IDisposable _eventDisposable;
        private Coroutine _zoomRoutine;

        // 진행 중인 줌을 즉시 원복(배율 1)하기 위해 들고 있다.
        private Action<float> _zoomSetter;

        private void Awake()
        {
            _eventDisposable = _eventSubscriber?.Register(StateEventCategory.CameraZoom, HandleZoom, HandleReset);
        }

        private void OnDestroy()
        {
            _eventDisposable?.Dispose();
        }

        private void HandleZoom(IStartData data)
        {
            if (data is not IPlayerCameraZoom zoom)
            {
                Debug.LogError($"[PlayerCameraZoomHandler] IPlayerCameraZoom이 필요한데 {data?.GetType().Name ?? "null"}을 받음");
                return;
            }

            // 배율이 1이면 변화가 없는 항목이라 건너뛴다.
            if (Mathf.Approximately(zoom.DistanceScale, 1f))
            {
                return;
            }

            CinemachineCamera camera = _currentCameraProvider?.ActiveCamera;
            if (camera == null)
            {
                return;
            }

            Action<float> setter = MakeDistanceSetter(camera);
            if (setter == null)
            {
                return;
            }

            if (_zoomRoutine != null)
            {
                StopCoroutine(_zoomRoutine);
            }

            _zoomSetter = setter;
            _zoomRoutine = StartCoroutine(CoZoom(setter, zoom.DistanceScale, zoom.ZoomOutTime, zoom.ZoomHoldTime, zoom.ZoomInTime));
        }

        // 상태가 끝나면 진행 중인 줌을 즉시 원래 거리로 되돌린다.
        private void HandleReset(CloseEventType reason)
        {
            if (_zoomRoutine == null)
            {
                return;
            }

            StopCoroutine(_zoomRoutine);
            _zoomRoutine = null;
            _zoomSetter?.Invoke(1f);
            _zoomSetter = null;
        }

        // 카메라 Body 종류에 맞는 거리 배율 적용자를 만든다. 둘 다 없으면 null.
        // FOV 대신 거리를 바꾸는 이유는 원근 왜곡을 피하기 위해서다.
        // OrbitalFollow는 OrbitStyle이 ThreeRing이면 단일 Radius가 아니라 Orbits 세 개가 실제로 쓰인다.
        // static인 이유: 넘겨받은 카메라만으로 끝나는 계산이라 특정 인스턴스에 속하지 않는다.
        private static Action<float> MakeDistanceSetter(CinemachineCamera camera)
        {
            if (camera.TryGetComponent(out CinemachineOrbitalFollow orbital))
            {
                float baseRadius = orbital.Radius;
                float baseTop = orbital.Orbits.Top.Radius;
                float baseCenter = orbital.Orbits.Center.Radius;
                float baseBottom = orbital.Orbits.Bottom.Radius;

                return scale =>
                {
                    orbital.Radius = baseRadius * scale;

                    Cinemachine3OrbitRig.Settings orbits = orbital.Orbits;
                    orbits.Top.Radius = baseTop * scale;
                    orbits.Center.Radius = baseCenter * scale;
                    orbits.Bottom.Radius = baseBottom * scale;
                    orbital.Orbits = orbits;
                };
            }

            if (camera.TryGetComponent(out CinemachineFollow follow))
            {
                Vector3 baseOffset = follow.FollowOffset;
                return scale => follow.FollowOffset = baseOffset * scale;
            }

            return null;
        }

        private IEnumerator CoZoom(Action<float> setter, float targetScale, float outTime, float holdTime, float inTime)
        {
            outTime = Mathf.Max(outTime, 0f);
            holdTime = Mathf.Max(holdTime, 0f);

            // 0이면 나눗셈이 무한대가 되므로 최소 시간을 준다.
            float returnTime = Mathf.Max(inTime, 0.01f);

            for (float elapsed = 0f; elapsed < outTime; elapsed += Time.deltaTime)
            {
                setter(Mathf.Lerp(1f, targetScale, elapsed / outTime));
                yield return null;
            }

            setter(targetScale);

            yield return new WaitForSeconds(holdTime);

            for (float elapsed = 0f; elapsed < returnTime; elapsed += Time.deltaTime)
            {
                setter(Mathf.Lerp(targetScale, 1f, elapsed / returnTime));
                yield return null;
            }

            setter(1f);

            _zoomRoutine = null;
            _zoomSetter = null;
        }
    }
}
