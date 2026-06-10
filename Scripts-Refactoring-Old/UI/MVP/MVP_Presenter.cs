using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class MVP_Presenter : MonoBehaviour, IInjectable
{
    //View, Model 모두 Presenter에 의존
    //DIContainer로 변수, 함수 연결
    //Model, View는 각자의 역할에 맞는 로직을 가지고 Presenter에서는 신호를 받고 신호를 보내는 것에 관련된 로직만 작성

    //데이터 집합
    [SerializeField] private BaseDataSO[] models;
    [SerializeField] private MVP_Model mvpModel;
    [SerializeField] private TMP_Text mvpText;  

    private Dictionary<DataPropertyNameEnum, List<IViewUpdatable>> dictViewUpdater = new Dictionary<DataPropertyNameEnum, List<IViewUpdatable>>();

    private void Awake()
    {
        InitializeModel();
    }

    private void Update()
    {
        //테스트
        mvpText.text = mvpModel.EnvSoundSliderValue.ToString();
    }


    private void InitializeModel()
    {
        foreach (var model in models)
        {
            model.UpdateDataEvent += ModelToView;
            model.Initialize();
        }
    }

    public void InitializeViewUpdater(List<IViewUpdatable> viewList)
    {
        if (viewList == null)
        {
            Debug.Log("IViewUpdatable 없음");
            return;
        }

        //Presenter에 View 저장
        foreach (var view in viewList)
        {
            view.Initialize();

            //view가 가진 PropertyNameEnum 값을 dictViewUpdatable에게 전달
            foreach (var item in view.ViewUpdatableProp.Keys)
            {
                if (dictViewUpdater.ContainsKey(item))
                {
                    dictViewUpdater[item].AddRange(new List<IViewUpdatable> { view });
                }
                else
                {
                    dictViewUpdater.Add(item, new List<IViewUpdatable> { view });
                }
            }
        }
    }
    public void InitializeModelUpdater(List<IModelUpdatable> viewList)
    {
        if (viewList == null)
        {
            return;
        }

        foreach (var view in viewList)
        {
            view.AddEvent();
            view.SendDataEvent += ViewToModel;
        }
    }

    private void ViewToModel(DataPropertyNameEnum property, object data) //View(사용자 또는 외부의 입력반응)에서 Model(데이터 컨테이너)로 전달
    {
        //PropertyNameEnum로 Model의 프로퍼티를 찾아서 Set
        //Enum이나 데이터 자료형이 일치하지 않으면 패스

        foreach (var model in models)
        {
            if (model.DataGroup.ContainsKey(property))
            {
                model.UpdateDataFromView(property, data);
            }
        }
    }

    private void ModelToView(DataPropertyNameEnum property, object data) //Model(데이터 컨테이너)에서 View(사용자 또는 외부의 입력반응)로 전달
    {
        //Model delegate에 의해 실행
        //MtoVData의 Key 대상에게 전달.
        if (!dictViewUpdater.ContainsKey(property))
        {
            Debug.Log(1);
            return;
        }

        foreach (var item in dictViewUpdater[property])
        {
            Debug.Log(2);

            item.GetData(property, data);
        }
    }

    //dictionary로 주소가져오기
}
