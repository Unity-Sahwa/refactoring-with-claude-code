using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class EventTimelineActive : EventData
{
    public PlayableDirector playableDirector;

    public override void Execute()
    {
        if (playableDirector != null)
        {
            // PlayableDirector에 할당된 TimelineAsset 가져오기
            TimelineAsset timelineAsset = playableDirector.playableAsset as TimelineAsset;

            if (timelineAsset != null)
            {
                playableDirector.Play();
            }
        }
    }
}