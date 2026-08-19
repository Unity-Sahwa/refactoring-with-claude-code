using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    [Serializable]
    public struct ShakeEntry
    {
        public PlayerStateType State;
        public ShakeData Shake;
    }

    // 책임: 플레이어 상태별 카메라 셰이크 수치를 담아 둔다. 타격/피격 둘 다 이 표 하나를 State로 조회해서 쓴다.
    [CreateAssetMenu(fileName = "PlayerCameraShakeData", menuName = "Data/PlayerCameraShakeData")]
    public class PlayerCameraShakeData : ScriptableObject
    {
        [SerializeField] private List<ShakeEntry> _shakeList = new List<ShakeEntry>();

        // 캐싱 없이 리스트를 직접 훑는다. 항목이 10개 안팎이라 비용은 무시할 수준이고,
        // 대신 에디터에서 값을 고치면(플레이 중 포함) 캐시 갱신 없이 바로 반영된다.
        public bool TryGetShake(PlayerStateType state, out ShakeData shake)
        {
            foreach (ShakeEntry entry in _shakeList)
            {
                if (entry.State == state)
                {
                    shake = entry.Shake;
                    return true;
                }
            }
            shake = default;
            return false;
        }
    }
}
