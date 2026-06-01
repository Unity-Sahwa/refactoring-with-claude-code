using System;
using System.Collections.Generic;

namespace Refactoring
{
    public interface IInjectTarget
    {
        Type[] InterfaceTypes { get; }
    }
}
