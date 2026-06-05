using System;
using UnityEngine;

namespace Refactoring
{
    [Serializable]
    public struct SerializableVector3
    {
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

        public static implicit operator Vector3(SerializableVector3 v) => v.ToVector3();
        public static implicit operator SerializableVector3(Vector3 v) => new SerializableVector3(v);
    }
}
