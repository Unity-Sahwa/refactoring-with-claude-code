
using System.Collections.Generic;
using TMPro;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class MVP_View1 : MonoBehaviour, IInjectable, IViewUpdatable, IModelUpdatable
{
    //view의 자식 오브젝트는 드래그앤드롭으로 할당, view마다 프리팹으로 관리
    [SerializeField] private Slider envSoundSlider;
    [SerializeField] private TMP_Text sliderValueText;
    [SerializeField] private TMP_Text sliderLabelText;

    public Dictionary<DataPropertyNameEnum, object> ViewUpdatableProp
    {
        get => viewUpdatableProp;
    }
    private Dictionary<DataPropertyNameEnum, object> viewUpdatableProp = new Dictionary<DataPropertyNameEnum, object>();

    
    public event IModelUpdatable.SendDataDelegate SendDataEvent;

    public void Initialize()
    {
        if (viewUpdatableProp.Count <= 0)
        {
            viewUpdatableProp.Add(DataPropertyNameEnum.EnvSoundSliderValue, envSoundSlider.value);
            viewUpdatableProp.Add(DataPropertyNameEnum.EnvSoundLabelText, sliderLabelText.text);
        }
    }

    //Model 데이터 변경시 해당 함수 실행됨.
    public void GetData(DataPropertyNameEnum property, object data)
    {
        if (property == DataPropertyNameEnum.EnvSoundSliderValue)
        {
            sliderValueText.text = envSoundSlider.value.ToString();

            if (envSoundSlider.value != (float)data)
            {
                envSoundSlider.value = (float)data;
            }
        }
        else if (property == DataPropertyNameEnum.EnvSoundLabelText)
        {
            sliderLabelText.text = (string)data;
        }
    }

    public void AddEvent()
    {
        envSoundSlider.onValueChanged.AddListener(EnvSoundSliderEvent);
    }

    private void EnvSoundSliderEvent(float value)
    {
        //if (value == envSoundSlider.value) return;

        SendDataEvent.Invoke(DataPropertyNameEnum.EnvSoundSliderValue, value);
    }
}
