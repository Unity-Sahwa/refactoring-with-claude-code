using System;

namespace Refactoring
{
    // 필드에 붙이면 DI가 해당 타입의 구현체를 찾아 직접 꽂아준다.
    // - Optional=false(기본): 못 찾으면 빨간 경고(LogError)
    // - Optional=true        : 못 찾으면 노란 경고(LogWarning)
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute
    {
        public bool Optional { get; }

        public InjectAttribute(bool optional = false)
        {
            Optional = optional;
        }
    }
}
