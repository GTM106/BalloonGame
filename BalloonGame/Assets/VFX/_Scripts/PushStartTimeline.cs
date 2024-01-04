using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PushStartTimeline : AirVentInteractable
{
    [SerializeField] PlayableDirector timelineAsset;
    public override void Interact()
    {
        timelineAsset.Play();
    }
}
