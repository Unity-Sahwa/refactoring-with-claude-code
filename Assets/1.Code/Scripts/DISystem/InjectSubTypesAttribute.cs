using System;

namespace Refactoring
{
    // List<Type> 필드에 붙이면, BaseType을 상속/구현하는 "자식 타입 목록"을 DI가 꽂아준다.
    // (객체가 아니라 타입을 받는다. 예: 상태 매니저가 타입으로 상태 객체를 직접 생성할 때)
    // - Optional=false(기본): 못 찾으면 빨간 경고(LogError)
    // - Optional=true        : 못 찾으면 노란 경고(LogWarning)
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectSubTypesAttribute : Attribute
    {
        public Type BaseType { get; }
        public bool Optional { get; }

        public InjectSubTypesAttribute(Type baseType, bool optional = false)
        {
            BaseType = baseType;
            Optional = optional;
        }
    }
}
