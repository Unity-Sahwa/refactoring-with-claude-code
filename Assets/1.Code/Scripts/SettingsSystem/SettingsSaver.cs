using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    //역할 : 설정창 오브젝트에 붙어, 꺼질 때 설정값이 저장되도록 함
    public class SettingsSaver : MonoBehaviour
    {
        [Inject(true)] private List<ISettingsHolder> _holders;

        private void OnDisable()
        {
            if (_holders == null)
            {
                return;
            }

            foreach (ISettingsHolder holder in _holders)
            {
                holder.Save();
            }
        }
    }
}
