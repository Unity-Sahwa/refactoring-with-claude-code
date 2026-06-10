using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _CharacterSwitcher : MonoBehaviour
{
    //0904
    public GameObject HumanCharacter
    {
        get
        {
            return humanCharacter;
        }
    }
    [SerializeField] private GameObject humanCharacter;

    public GameObject AnimalCharacter
    {
        get
        {
            return animalCharacter;
        }
    }
    [SerializeField] private GameObject animalCharacter;

    public GameObject CurrentCharacter
    {
        get
        {
            if (humanCharacter.activeSelf)
            {

                return humanCharacter;
            }
            else
            {
                return animalCharacter;
            }
        }
    }
    public Animator CurrentAnimator
    {
        get
        {
            if (humanCharacter.activeSelf)
            {
                return humanCharacter.GetComponent<Animator>();
            }
            else
            {
                return animalCharacter.GetComponent<Animator>();
            }
        }
    }


    private void Awake()
    {
        humanCharacter.SetActive(true);
        animalCharacter.SetActive(false);
    }

    public void SwitchCharacter(bool isHumanCharacter)
    {
        if (isHumanCharacter)
        {
            humanCharacter.SetActive(true);
            animalCharacter.SetActive(false);
        }
        else
        {
            humanCharacter.SetActive(false);
            animalCharacter.SetActive(true);
        }
    }
}
