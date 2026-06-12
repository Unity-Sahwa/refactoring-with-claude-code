using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Refactoring
{
    [Serializable]
    public class HitboxDataEntry : IStartData, IPlayerHitbox, IHitboxCombat
    {
        [SerializeField] private string name;
        [SerializeField] private bool untilFinish;
        [SerializeField] private Vector3 position;
        [SerializeField] private Vector3 rotation;
        [SerializeField] private Vector3 scale;
        [SerializeField] [Range(0,1)] private float startProgress;
        [SerializeField] private float duration;
        [SerializeField] private AssetReferenceGameObject hitboxObject;
        [SerializeField] private HitboxAttachPointType attachKey;
        [SerializeField] private bool stopInPlace;   // 도중에 부모에서 떨어져 그 자리 정지할지
        [SerializeField] private float stopTime;       // 멈추는 시점(초)
        [SerializeField] private bool showMesh;       // true면 히트박스가 눈에 보이게 함(조정용)

        [SerializeField] private CombatInfo combat;   // 전투값 묶음(켜기 정보와 관심사 분리)

        //IStartData
        public float StartProgress => startProgress;

        //IPlayerHitbox
        public bool UntilFinish => untilFinish;
        public float Duration => duration;
        public Vector3 Position => position;
        public Vector3 Rotation => rotation;
        public Vector3 Scale => scale;
        public AssetReferenceGameObject HitboxObject => hitboxObject;
        public HitboxAttachPointType AttachKey => attachKey;
        public bool StopInPlace => stopInPlace;
        public float StopTime => stopTime;
        public bool ShowMesh => showMesh;

        
        //IHitboxCombat (combat 묶음에 위임)
        public float Damage => combat.Damage;
        public InkColor Color => combat.Color;
        public float InkStack => combat.InkStack;
        public IReadOnlyList<IHitReaction> Reactions => combat.Reactions;
    }

    // 히트박스가 부딪힌 상대에게 줄 전투값에 관한 정보.
    [Serializable]
    public class CombatInfo : IHitboxCombat
    {
        [SerializeField] private float damage;
        [SerializeField] private InkColor color;
        [SerializeField] private float inkStack;
        [SerializeReference, SubclassSelector] private List<IHitReaction> reactions = new(); // 다형성 직렬화: 인스펙터 드롭다운으로 반응 종류별 추가

        public float Damage => damage;
        public InkColor Color => color;
        public float InkStack => inkStack;
        public IReadOnlyList<IHitReaction> Reactions => reactions;
    }
}
