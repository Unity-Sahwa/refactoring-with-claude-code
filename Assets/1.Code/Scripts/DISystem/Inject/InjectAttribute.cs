using System;

namespace Refactoring
{
    // 책임: 이 필드에 DI가 구현체를 찾아 꽂도록 표시한다.
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute
    {
        // false(기본)면 못 찾을 때 LogError, true면 LogWarning
        public bool Optional { get; }

        public InjectAttribute(bool optional = false)
        {
            Optional = optional;
        }
    }
}
