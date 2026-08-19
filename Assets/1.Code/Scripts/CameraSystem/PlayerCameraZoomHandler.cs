using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // 역할: 상태 이벤트(CameraZoom)를 받아 스킬 연출용 카메라 거리 줌(확 멀어짐→유지→복귀)을 실행한다.
    //       DefaultCamera(OrbitalFollow, ThreeRing)/LockOnCamera(Follow)는 거리를 다루는 컴포넌트가 달라
    //       "기준 거리 * 배율" setter를 만들어 공용으로 다룬다(FOV 대신 거리를 써서 원근 왜곡을 피함).
    public class PlayerCameraZoomHandler : MonoBehaviour
    {
        [Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;
        [Inject(true)] private ICurrentCameraProvider _currentCameraProvider;

        private IDisposable _eventDisposable;
        private Coroutine _zoomRoutine;
        private Action<float> _zoomSetter; // 진행 중인 줌을 즉시 원복(스케일 1)하기 위해 들고 있음

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

            // DistanceScale이 1이면 변화 없는 항목이므로 생략
            if (Mathf.Approximately(zoom.DistanceScale, 1f) || zoom.ZoomDuration <= 0f)
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
            _zoomRoutine = StartCoroutine(CoZoom(setter, zoom.DistanceScale, zoom.ZoomOutTime, zoom.ZoomHoldTime, zoom.ZoomDuration));
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

        // 카메라 Body 종류(Orbital/Follow)에 맞는 "거리 배율 적용자"를 만든다. 둘 다 없으면 null.
        // OrbitalFollow는 OrbitStyle이 ThreeRing이면 단일 Radius가 아니라 Orbits(Top/Center/Bottom)가 실제로 쓰이므로 셋 다 스케일한다.
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

        // outTime 동안 목표 배율까지 "확" 도달 → holdTime 동안 유지 → 남은 시간 동안 1배(원래 거리)로 천천히 복귀.
        private IEnumerator CoZoom(Action<float> setter, float targetScale, float outTime, float holdTime, float totalDuration)
        {
            outTime = Mathf.Clamp(outTime, 0f, totalDuration);
            holdTime = Mathf.Clamp(holdTime, 0f, totalDuration - outTime);
            float returnTime = Mathf.Max(totalDuration - outTime - holdTime, 0.01f);

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
