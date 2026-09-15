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
            if (_eventSubscriber == null)
            {
                Debug.LogWarning($"{name}: {nameof(IPlayerStateEventSubscriber)}가 없어 스킬 줌 구독을 건너뜀.");
                return;
            }

            _eventDisposable = _eventSubscriber.Register(StateEventCategory.CameraZoom, HandleZoom, HandleReset);
        }

        private void OnDestroy()
        {
            _eventDisposable?.Dispose();

            if (_zoomRoutine != null)
            {
                StopCoroutine(_zoomRoutine);
            }
        }

        private void HandleZoom(IStartData data)
        {
            if (!TryGetZoom(data, out IPlayerCameraZoom zoom))
            {
                return;
            }

            if (!TryGetSetter(out Action<float> setter))
            {
                return;
            }

            StartZoom(setter, zoom);
        }

        private static bool TryGetZoom(IStartData data, out IPlayerCameraZoom zoom)
        {
            zoom = default;
            if (data is not IPlayerCameraZoom result)
            {
                Debug.LogError($"[PlayerCameraZoomHandler] IPlayerCameraZoom이 필요한데 {data?.GetType().Name ?? "null"}을 받음");
                return false;
            }

            // 배율이 1이면 변화가 없는 항목이라 건너뛴다.
            if (Mathf.Approximately(result.DistanceScale, 1f))
            {
                return false;
            }

            zoom = result;
            return true;
        }

        private bool TryGetSetter(out Action<float> setter)
        {
            setter = null;
            if (_currentCameraProvider == null)
            {
                Debug.LogWarning($"{name}: {nameof(ICurrentCameraProvider)}가 없어 스킬 줌을 건너뜀.");
                return false;
            }

            CinemachineCamera camera = _currentCameraProvider.ActiveCamera;
            if (camera == null)
            {
                return false;
            }

            setter = MakeDistanceSetter(camera);
            return setter != null;
        }

        private void StartZoom(Action<float> setter, IPlayerCameraZoom zoom)
        {
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
        private static Action<float> MakeDistanceSetter(CinemachineCamera camera)
        {
            return MakeOrbitalSetter(camera) ?? MakeFollowSetter(camera);
        }

        // OrbitalFollow는 OrbitStyle이 ThreeRing이면 단일 Radius가 아니라 Orbits 세 개가 실제로 쓰인다.
        private static Action<float> MakeOrbitalSetter(CinemachineCamera camera)
        {
            if (!camera.TryGetComponent(out CinemachineOrbitalFollow orbital))
            {
                return null;
            }

            OrbitalBase baseValues = new OrbitalBase(orbital);
            return scale => ApplyOrbitalScale(orbital, baseValues, scale);
        }

        private static void ApplyOrbitalScale(CinemachineOrbitalFollow orbital, OrbitalBase baseValues, float scale)
        {
            orbital.Radius = baseValues.Radius * scale;

            Cinemachine3OrbitRig.Settings orbits = orbital.Orbits;
            orbits.Top.Radius = baseValues.Top * scale;
            orbits.Center.Radius = baseValues.Center * scale;
            orbits.Bottom.Radius = baseValues.Bottom * scale;
            orbital.Orbits = orbits;
        }

        // OrbitalFollow의 줌 전 기준 반지름 4개(Radius·Top·Center·Bottom)를 담아 둔다.
        private readonly struct OrbitalBase
        {
            public readonly float Radius;
            public readonly float Top;
            public readonly float Center;
            public readonly float Bottom;

            public OrbitalBase(CinemachineOrbitalFollow orbital)
            {
                Radius = orbital.Radius;
                Top = orbital.Orbits.Top.Radius;
                Center = orbital.Orbits.Center.Radius;
                Bottom = orbital.Orbits.Bottom.Radius;
            }
        }

        private static Action<float> MakeFollowSetter(CinemachineCamera camera)
        {
            if (!camera.TryGetComponent(out CinemachineFollow follow))
            {
                return null;
            }

            Vector3 baseOffset = follow.FollowOffset;
            return scale => follow.FollowOffset = baseOffset * scale;
        }

        private IEnumerator CoZoom(Action<float> setter, float targetScale, float outTime, float holdTime, float inTime)
        {
            outTime = Mathf.Max(outTime, 0f);
            holdTime = Mathf.Max(holdTime, 0f);
            // 0이면 나눗셈이 무한대가 되므로 최소 시간을 준다.
            float returnTime = Mathf.Max(inTime, 0.01f);

            yield return LerpOver(setter, 1f, targetScale, outTime);
            setter(targetScale);

            yield return new WaitForSeconds(holdTime);

            yield return LerpOver(setter, targetScale, 1f, returnTime);
            setter(1f);

            _zoomRoutine = null;
            _zoomSetter = null;
        }

        private static IEnumerator LerpOver(Action<float> setter, float from, float to, float duration)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                setter(Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }
        }
    }
}
