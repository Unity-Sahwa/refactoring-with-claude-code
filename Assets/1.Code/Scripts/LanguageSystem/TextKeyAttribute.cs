using UnityEngine;

namespace Refactoring
{
    // 문자열 칸에 붙이면 인스펙터에서 표의 키 목록을 드롭다운으로 고르게 된다.
    // 직접 타이핑하면 오타가 나고, 오타는 실행해야 발견되기 때문에 둔다.
    public class TextKeyAttribute : PropertyAttribute
    {
        // 이 글자가 들어간 키만 드롭다운에 보인다. 안 넘기면 전부 보인다.
        // 키가 늘수록 목록이 길어져서, 쓰는 자리마다 볼 것만 걸러 쓰라고 둔다.
        //
        // public인 이유: 어트리뷰트 값은 드로어(TextKeyDrawer)가 밖에서 읽어야 쓸모가 있다.
        public readonly string Filter;

        public TextKeyAttribute(string filter = null)
        {
            Filter = filter;
        }
    }
}
