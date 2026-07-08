using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 요청 시점마다 범위 내 처형 대상을 스캔해 알려준다.
    public class FinishTargetScanner : MonoBehaviour, IFinishTargetProvider, IFinishChecker
    {
        [SerializeField] private float _radius = 20f;
        [SerializeField] private LayerMask _enemyMask;

        [Inject] private ICurrentCharacterProvider _currentCharacterProvider;

        private readonly Collider[] _overlapHits = new Collider[32];
        private Camera _cam;

        private readonly List<Enemy> _scanEnemies = new(32);
        private readonly List<CaliSystem> _scanCalis = new(32);
        private readonly List<Enemy> _executeTargets = new(32);
        private int _scanHighestFullCap;
        private bool _scanHasFull;

        // 덧칠스택이 가득찬 대상가 화면 안에 존재한다면 처형가능 상태
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

            if (_currentCharacterProvider?.CurrentCharacter == null) return;

            Vector3 center = _currentCharacterProvider.CurrentCharacter.transform.position;
            int hitCount = Physics.OverlapSphereNonAlloc(center, _radius, _overlapHits, _enemyMask);

            for (int i = 0; i < hitCount; i++)
            {
                if (!_overlapHits[i].TryGetComponent(out Enemy enemy) || enemy.isDead) continue;

                _overlapHits[i].TryGetComponent(out CaliSystem cali);
                _scanEnemies.Add(enemy);
                _scanCalis.Add(cali);

                if (cali != null && cali.IsPaintOverMax())
                {
                    _scanHasFull = true;

                    // 덧칠 풀스택 대상 중 최고 한계치를 기준으로 타겟들을 처형하기 위해 _scanHighestFullCap에 최고 한계치를 저장
                    if (_scanHighestFullCap < cali.MaxPaintOver) _scanHighestFullCap = cali.MaxPaintOver;
                }
            }

            for (int i = 0; i < _scanEnemies.Count; i++)
            {
                CaliSystem cali = _scanCalis[i];

                //처형 대상은 _scanHighestFullCap보다 낮은 한계치를 가진 대상
                if (cali != null && cali.MaxPaintOver <= _scanHighestFullCap) _executeTargets.Add(_scanEnemies[i]);
            }
        }

        // 처형 대상 중 하나라도 화면 안에 있나.
        private bool HasExecuteTargetOnScreen()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return false;

            for (int i = 0; i < _executeTargets.Count; i++)
            {
                Vector3 vp = _cam.WorldToViewportPoint(_executeTargets[i].transform.position);
                if (vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f) return true;
            }
            return false;
        }
    }
}
