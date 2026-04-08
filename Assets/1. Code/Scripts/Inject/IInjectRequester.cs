using System;
using System.Collections.Generic;

namespace Refactoring
{
    // 씬 내 MonoBehaviour를 자동 수집하고 IInjectable 구현체에 주입
    public interface IInjectRequester
    {
        List<Type> TargetTypes { get; }
        void Inject(Dictionary<Type, List<object>> targets);
    }
}
