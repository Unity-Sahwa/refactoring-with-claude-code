using System;
using UnityEngine;

namespace Refactoring
{
    [Serializable]
    public struct SerializableVector3
    {
        // JsonUtility가 필드명을 그대로 키로 써서, 이름을 고치면 이미 저장된 위치를 못 읽는다.
        // Vector3의 x·y·z와도 짝이 맞아 컨벤션(PascalCase)에서 벗어나 있어도 그대로 둔다.
        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);

        public static implicit operator Vector3(SerializableVector3 value) => value.ToVector3();
        public static implicit operator SerializableVector3(Vector3 vector) => new SerializableVector3(vector);
    }
}
