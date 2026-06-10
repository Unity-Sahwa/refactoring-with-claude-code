using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.WSA;

public class InteractionTrigger : MonoBehaviour
{
    [Header("아이콘 넣는 곳")]
    public GameObject icon1;
    public GameObject icon2;
    private MobileInput mobileInput;

    [Header("상호작용 시 실행할 이벤트들")]
    public List<EventData> interactionEvents;

    [Header("각 이벤트의 지연 시간 (초)")]
    public List<float> delayTimes;

    [Header("반복 가능한지 체크")]
    public bool isLoop = false;

    public Animator animator;    

    private bool isActive = false;
    private bool playerCheck = false;

    private void Start()
    {
        mobileInput = MobileInput.instance;
    }

    private void Update()
    {
        if (playerCheck && Input.GetKeyDown(SaveManager.instance.InputKeys[KeyAction.INTERACT]))
        {
            TriggerInteraction();
        }

        if (playerCheck)
        {
            if (mobileInput != null)
            {
                if (mobileInput.gameObject.activeSelf)
                {
                    if (mobileInput.interact)
                    {
                        TriggerInteraction();
                        return;
                    }
                }
            }
        }
    }

    private void TriggerInteraction()
    {
        if (!isActive)
        {
            StartCoroutine(ExecuteEventsWithDelay());
            if (animator != null)
            {
                animator.SetTrigger("Open");
            }
            isActive = true;
            mobileInput.DisableInteraction(); //바로하면 작동을 안함

        }
        else if (isLoop)
        {
            if (animator != null)
            {
                animator.SetTrigger("Close");
            }
            isActive = false;
        }
    }

    private IEnumerator ExecuteEventsWithDelay()
    {
        while (delayTimes.Count < interactionEvents.Count)
        {
            delayTimes.Add(0f);
        }
        for (int i = 0; i < interactionEvents.Count; i++)
        {
            if (interactionEvents[i] != null)
            {
                yield return new WaitForSeconds(delayTimes[i]);
                interactionEvents[i].Execute();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCheck = true;
            if (icon1 != null)
            {
                icon1.SetActive(true);
            }
            if (icon2 != null)
            {
                icon2.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCheck = false;
            if (icon1 != null)
            {
                icon1.SetActive(false);
            }
            if (icon2 != null)
            {
                icon2.SetActive(false);
            }
        }
    }
}
