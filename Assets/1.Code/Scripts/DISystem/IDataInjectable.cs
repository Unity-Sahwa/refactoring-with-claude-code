using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public interface IDataInjectable
    {
        Dictionary<Type,List<ScriptableObject>> RequiredData { get;}
    }
}
