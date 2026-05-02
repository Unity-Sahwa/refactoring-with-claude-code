using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneTrigger : EventData
{
    private SceneSwitcher sceneSwitcher;

    [SerializeField] SceneName sceneToLoad;
    private void Start()
    {
        sceneSwitcher = SceneSwitcher.instance;
    }

    public override void Execute()
    {
        sceneSwitcher.SwitchScene((int)sceneToLoad);
    }
}
