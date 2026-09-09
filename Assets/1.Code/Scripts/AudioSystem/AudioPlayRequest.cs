using UnityEngine;

namespace Refactoring
{
    //대원TODO: 이유가 이해안돼
    // public 필드인 이유: 요청을 만들 때 한 번에 채우는 값 묶음이라 감출 상태가 없다.
    public struct AudioPlayRequest
    {
        //대원TODO: public 설정하면 참조가 눈에 안들어옴.
        public SoundType Id;

        public bool HasPosition;
        public Vector3 Position;

        //대원TODO: 아무도 안쓰니까 삭제 필요
        public Transform Follow; 

        public static AudioPlayRequest Create(SoundType id)
        {
            return new AudioPlayRequest { Id = id };
        }

        public static AudioPlayRequest CreateAt(SoundType id, Vector3 position)
        {
            return new AudioPlayRequest { Id = id, Position = position, HasPosition = true };
        }

        //대원TODO: 아무도 안쓰니까 삭제 필요
        public static AudioPlayRequest CreateFollowing(SoundType id, Transform follow)
        {
            return new AudioPlayRequest { Id = id, Follow = follow };
        }
    }
}
