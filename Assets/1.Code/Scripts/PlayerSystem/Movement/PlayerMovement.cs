using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class PlayerMovement : MonoBehaviour
    {
        [Inject] private IInputEventProvider _inputEventProvider;
        [Inject] private ICharacterSwapNotifier _playerCharacterSwitcher;
        [Inject] private List<BaseCharacter<PlayerCharacterType>> _characters;
        private Dictionary<PlayerCharacterType, Rigidbody> _characterRB = new Dictionary<PlayerCharacterType, Rigidbody>();
        private PlayerCharacterType _currentCharacterType;
        
        private Transform _camera;
        private Vector3 _cameraForward;
        private Vector3 _cameraRight;
        private Vector2 _playerMoveVector = new Vector2();
        private Vector3 _movePosition;
        private Vector3 _playerPosition;
        Quaternion _targetRotation;
        private float _moveRate = 10;
        private float _rotateRate = 10;

        void Awake()
        {
            _inputEventProvider.OnMoveInput += OnMove;
            _playerCharacterSwitcher.OnCharacterSwapped += OnCharacterSwapped;

            foreach (var characterObj in _characters)
            {
                _characterRB[characterObj.Type] = characterObj.GetCharacterComponent<Rigidbody>();
            }
        }

        void Start()
        {
            _currentCharacterType = _playerCharacterSwitcher.CurrentCharacter;

            _camera = Camera.main.transform;
        }

        void FixedUpdate()
        {
            if(_playerMoveVector != Vector2.zero)
            {
                UpdateMoveDirection();
                Move();
                Rotate();
            }
        }

        private void OnCharacterSwapped(PlayerCharacterType type)
        {
            _currentCharacterType = type;
        }
        
        private void OnMove(Vector2 vector2)
        {
            _playerMoveVector = vector2;
        }

        private void UpdateMoveDirection()
        {
            //카메라가 바라보는 방향에서 위아래 방향을 제거한 노멀벡터
            _cameraForward.Set(_camera.forward.x,0,_camera.forward.z);
            _cameraForward.Normalize();
            _cameraRight.Set(_camera.right.x,0,_camera.right.z);
            _cameraRight.Normalize();

            //카메라기준 플레이어 좌우이동 + 플레이어 앞뒤이동
            _movePosition = _cameraRight * _playerMoveVector.x + _cameraForward * _playerMoveVector.y;
            _movePosition.Normalize();
        }
        private void Move()
        {
            _playerPosition = _characterRB[_currentCharacterType].transform.position;
            _characterRB[_currentCharacterType].MovePosition(_playerPosition + _movePosition * Time.deltaTime * _moveRate);
        }

        private void Rotate()
        {
            _targetRotation = Quaternion.LookRotation(_movePosition);
            _characterRB[_currentCharacterType].MoveRotation(Quaternion.Slerp(_characterRB[_currentCharacterType].rotation, _targetRotation, Time.deltaTime * _rotateRate));
        }
    }
} 