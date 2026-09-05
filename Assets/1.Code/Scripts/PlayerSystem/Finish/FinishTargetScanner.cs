using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 요청 시점마다 범위 내 처형 대상을 스캔해 알려준다.
    public class FinishTargetScanner : MonoBehaviour, IFinishTargetProvider, IFinishChecker
    {
        [SerializeField] private float _radius = 20f;
        [SerializeField] private LayerMask _enemyMask;

        [Preserve, Inject] private ICurrentCharacterProvider _currentCharacterProvider;

        private readonly Collider[] _overlapHits = new Collider[32];
        private Camera _camera;

        private readonly List<Enemy> _scanEnemies = new(32);
        private readonly List<CaliSystem> _scanCalis = new(32);
        private readonly List<Enemy> _executeTargets = new(32);
        private int _scanHighestFullCap;
        private bool _scanHasFull;

        // 덧칠 스택이 가득 찬 대상이 화면 안에 있어야 처형 가능하다.
        public bool CanFinish()
        {
            ScanTargets();
            return _scanHasFull && HasExecuteTargetOnScreen();
        }

        public IReadOnlyList<Enemy> GatherStunTargets()
        {
            ScanTargets();
            return _scanEnemies;
        }

        public IReadOnlyList<Enemy> GatherExecuteTargets()
        {
            ScanTargets();
            return _executeTargets;
        }

        private void ScanTargets()
        {
            _scanEnemies.Clear();
            _scanCalis.Clear();
            _executeTargets.Clear();
            _scanHighestFullCap = 0;
            _scanHasFull = false;

            Transform characterTransform = _currentCharacterProvider?.GetCurrentComponent<Transform>();
            if (characterTransform == null)
            {
                return;
            }

            Vector3 center = characterTransform.position;
            int hitCount = Physics.OverlapSphereNonAlloc(center, _radius, _overlapHits, _enemyMask);

            for (int i = 0; i < hitCount; i++)
            {
                if (!_overlapHits[i].TryGetComponent(out Enemy enemy) || enemy.isDead)
                {
                    continue;
                }

                _overlapHits[i].TryGetComponent(out CaliSystem cali);
                _scanEnemies.Add(enemy);
                _scanCalis.Add(cali);

                if (cali != null && cali.IsPaintOverMax())
                {
                    _scanHasFull = true;

                    // 풀스택 대상 중 최고 한계치를 기준으로 처형 대상을 고른다.
                    if (_scanHighestFullCap < cali.MaxPaintOver)
                    {
                        _scanHighestFullCap = cali.MaxPaintOver;
                    }
                }
            }

            for (int i = 0; i < _scanEnemies.Count; i++)
            {
                CaliSystem cali = _scanCalis[i];

                // 처형 대상은 최고 한계치보다 낮은 한계치를 가진 대상이다.
                if (cali != null && cali.MaxPaintOver <= _scanHighestFullCap)
                {
                    _executeTargets.Add(_scanEnemies[i]);
                }
            }
        }

        // 처형 대상 중 하나라도 화면 안에 있나.
        private bool HasExecuteTargetOnScreen()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            if (_camera == null)
            {
                return false;
            }

            for (int i = 0; i < _executeTargets.Count; i++)
            {
                Vector3 viewportPoint = _camera.WorldToViewportPoint(_executeTargets[i].transform.position);
                if (viewportPoint.z > 0f && viewportPoint.x >= 0f && viewportPoint.x <= 1f && viewportPoint.y >= 0f && viewportPoint.y <= 1f)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
