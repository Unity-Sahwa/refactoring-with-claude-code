using UnityEngine;

namespace Refactoring
{
    // public 필드인 이유: 요청을 만들 때 한 번에 채우는 값 묶음이라 감출 상태가 없다.
    public struct AudioPlayRequest
    {
        public SoundType Id;

        public bool HasPosition;
        public Vector3 Position;

        //해당 이동체에 자식으로 붙음
        public Transform Follow; 

        public static AudioPlayRequest Create(SoundType id)
        {
            return new AudioPlayRequest { Id = id };
        }

        public static AudioPlayRequest CreateAt(SoundType id, Vector3 position)
        {
            return new AudioPlayRequest { Id = id, Position = position, HasPosition = true };
        }

        public static AudioPlayRequest CreateFollowing(SoundType id, Transform follow)
        {
            return new AudioPlayRequest { Id = id, Follow = follow };
        }
    }
}
