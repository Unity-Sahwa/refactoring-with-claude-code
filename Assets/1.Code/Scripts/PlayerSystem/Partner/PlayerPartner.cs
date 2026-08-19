using System;
using System.Collections;
using UnityEngine;

namespace Refactoring
{
    // 역할: 현재 캐릭터의 머리 왼쪽 뒤를 부드럽게 따라다니며 떠 있고,
    //       캐릭터 스왑 시 캐릭터에게 부딪혀 이펙트와 소리를 낸다.
    public class PlayerPartner : MonoBehaviour
    {
        [Inject]
        private ICurrentCharacterProvider _characterProvider;

        [Inject]
        private ICharacterSwapNotifier _swapNotifier;

        [Tooltip("부딪힐 때 켤 이펙트. 파트너 자식으로 미리 배치하고 꺼둔다")]
        [SerializeField]
        private GameObject _swapToHumanEffect;

        [SerializeField]
        private GameObject _swapToAnimalEffect;

        [Tooltip("부딪힐 때 낼 소리. 세 경우 모두 같다")]
        [SerializeField]
        private AudioSource _collideAudio;

        [Tooltip("따라다닐 위치. 캐릭터 기준 로컬 좌표(머리 왼쪽 뒤)")]
        [SerializeField]
        private Vector3 _followOffset = new Vector3(-0.5f, 1.8f, -0.5f);

        [Tooltip("부딪힐 위치. 캐릭터 기준 로컬 좌표(가슴 앞)")]
        [SerializeField]
        private Vector3 _collideOffset = new Vector3(0f, 1.3f, 0.3f);

        [SerializeField]
        private float _followSmoothTime = 0.35f;

        [Tooltip("이 거리보다 멀어지면 순간이동으로 따라붙는다")]
        [SerializeField]
        private float _warpDistance = 15f;

        [Tooltip("구체 주위를 도는 마스크들의 부모")]
        [SerializeField]
        private Transform _orbitRoot;

        [SerializeField]
        private Transform[] _masks;

        [SerializeField]
        private float _orbitSpeed = 40f;

        [SerializeField]
        private float _floatHeight = 0.15f;

        [SerializeField]
        private float _floatSpeed = 1.5f;

        [SerializeField]
        private float _collideSpeed = 12f;

        [Tooltip("부딪힌 뒤 이펙트를 유지하는 시간")]
        [SerializeField]
        private float _effectTime = 1f;

        // 불러오기·낙하 복귀로 캐릭터를 되돌릴 때는 스왑 소리를 내지 않는다. GameStateSaver가 스왑 직전에 켠다.
        public bool SkipNextCollideSound;

        private float[] _maskBaseHeights;
        private Vector3 _velocity;
        private Coroutine _collideRoutine;

        private void Awake()
        {
            _maskBaseHeights = new float[_masks.Length];
            for (int i = 0; i < _masks.Length; i++)
            {
                _maskBaseHeights[i] = _masks[i].localPosition.y;
            }

            if (_swapNotifier != null)
            {
                _swapNotifier.OnCharacterSwapped += HandleCharacterSwapped;
            }
        }

        private void Start()
        {
            Transform characterTransform = GetCharacterTransform();
            if (characterTransform == null)
            {
                return;
            }

            transform.position = characterTransform.TransformPoint(_followOffset);
        }

        private void Update()
        {
            _orbitRoot.Rotate(Vector3.up, _orbitSpeed * Time.deltaTime, Space.Self);
            FloatMasks();
        }

        // 캐릭터가 렌더 프레임 기준으로 움직이므로 추종도 LateUpdate에서 한다.
        // FixedUpdate에서 하면 물리 스텝 수와 프레임 수가 어긋나 끊겨 보인다.
        private void LateUpdate()
        {
            // 부딪히는 중에는 코루틴이 위치를 직접 잡으므로 추종하지 않는다.
            if (_collideRoutine != null)
            {
                return;
            }

            Follow();
        }

        private void OnDestroy()
        {
            if (_swapNotifier != null)
            {
                _swapNotifier.OnCharacterSwapped -= HandleCharacterSwapped;
            }
        }

        // 마스크마다 위상을 어긋내서 서로 따로따로 위아래로 떠오르게 한다.
        private void FloatMasks()
        {
            for (int i = 0; i < _masks.Length; i++)
            {
                float phase = Mathf.PI * 2f * i / _masks.Length;
                float height = _maskBaseHeights[i] + _floatHeight * Mathf.Sin(Time.time * _floatSpeed + phase);

                Vector3 localPosition = _masks[i].localPosition;
                localPosition.y = height;
                _masks[i].localPosition = localPosition;
            }
        }

        private void Follow()
        {
            Transform characterTransform = GetCharacterTransform();
            if (characterTransform == null)
            {
                return;
            }

            Vector3 targetPosition = characterTransform.TransformPoint(_followOffset);

            if (Vector3.Distance(transform.position, targetPosition) > _warpDistance)
            {
                transform.position = targetPosition;
                _velocity = Vector3.zero;
                return;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _followSmoothTime);
        }

        private Transform GetCharacterTransform()
        {
            PlayerCharacter character = _characterProvider?.CurrentCharacter;
            if (character == null)
            {
                return null;
            }

            return character.transform;
        }

        private void HandleCharacterSwapped()
        {
            PlayerCharacter character = _characterProvider?.CurrentCharacter;
            if (character == null)
            {
                return;
            }

            GameObject effect = character.Type == PlayerCharacterType.AnimalCharacter
                ? _swapToAnimalEffect
                : _swapToHumanEffect;

            StartCollide(effect);
        }

        private void StartCollide(GameObject effect)
        {
            // 이미 부딪히는 중이면 무시한다.
            if (_collideRoutine != null)
            {
                return;
            }

            _collideRoutine = StartCoroutine(CoCollide(effect));
        }

        private IEnumerator CoCollide(GameObject effect)
        {
            Transform characterTransform = GetCharacterTransform();
            if (characterTransform == null)
            {
                _collideRoutine = null;
                yield break;
            }

            Vector3 hitPosition = characterTransform.TransformPoint(_collideOffset);

            // 캐릭터가 움직이거나 돌아도 따라붙도록 매 프레임 목표를 다시 잡는다.
            while (Vector3.Distance(transform.position, hitPosition) > 0.1f)
            {
                hitPosition = characterTransform.TransformPoint(_collideOffset);
                transform.position = Vector3.MoveTowards(transform.position, hitPosition, _collideSpeed * Time.deltaTime);
                yield return null;
            }

            if (SkipNextCollideSound)
            {
                SkipNextCollideSound = false;
            }
            else
            {
                _collideAudio?.Play();
            }

            if (effect != null)
            {
                effect.SetActive(true);
            }

            yield return new WaitForSeconds(_effectTime);

            if (effect != null)
            {
                effect.SetActive(false);
            }

            _velocity = Vector3.zero;
            _collideRoutine = null;
        }
    }
}
