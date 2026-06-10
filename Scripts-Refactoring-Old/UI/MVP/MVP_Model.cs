using System;
using UnityEngine;



[CreateAssetMenu(menuName = "Scriptable Objects/UI/MVP_Model")]
public class MVP_Model : BaseDataSO
{
    public float EnvSoundSliderValue 
    {
        get 
        { 
            return envSoundSliderValue; 
        } 
        set
        {
            envSoundSliderValue = value;
            UpdateData(DataPropertyNameEnum.EnvSoundSliderValue, value);
        }
    }
    [SerializeField]            
    [Range(0,1)] private float envSoundSliderValue;

    public string EnvSoundLabelText
    {
        get
        {
            return envSoundLabelText;
        }
        set
        {
            envSoundLabelText = value;
            UpdateData(DataPropertyNameEnum.EnvSoundLabelText, value);
        }
    }
    [SerializeField]
    private string envSoundLabelText;
}
