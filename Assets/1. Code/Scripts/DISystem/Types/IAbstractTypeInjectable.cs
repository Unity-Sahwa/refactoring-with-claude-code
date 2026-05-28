using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public interface IAbstractTypeInjected
    {
        Dictionary<Type,List<Type>> AbstractTypeMap { get;}
    }
}
