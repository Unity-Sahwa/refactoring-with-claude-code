using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "PlayerCharacterData", menuName = "Data/PlayerCharacterData")]
    public class PlayerCharacterData : ScriptableObject, IMovementData
    {
        [SerializeField] private float initialHP;

        [SerializeField] private float moveSpeed;
        [SerializeField] private float rotateSpeed;

        public float MoveSpeed => moveSpeed;
        public float RotateSpeed => rotateSpeed;
    }
}