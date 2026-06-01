using System;
using System.Collections.Generic;

namespace Refactoring
{
    public interface IInterfaceInjectable
    {
        //구현체는 RequiredInterfaceType의 Key를 구현해야 함
        public Dictionary<Type,List<object>> injectedImplements {get;}
    }
}
