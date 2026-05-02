using System;
using System.Reflection;
using UnityEngine.Scripting;

namespace Refactoring
{
    //TODO: reflection 적용하는 부분 [Preserve] 작성하기
    [Preserve]
    public static class AssemblyCache
    {
        public static readonly Type[] AllTypes = Assembly.GetExecutingAssembly().GetTypes();
    }
}