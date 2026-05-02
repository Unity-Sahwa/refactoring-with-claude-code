using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MVP_Model))]
public class MVP_ModelEditor : Editor
{
    MVP_Model targetSO;

    //SerializedProperty는 해당 프로퍼티를 가리키는 참조자 역할을 함.
    SerializedProperty envSoundValueProp;
    SerializedProperty envSoundLabelTextProp;

    private void OnEnable()
    {
        targetSO = (MVP_Model)target;

        //FindProperty 매개변수로는 직렬화 가능한 객체가 들어옴
        //직렬화: 데이터 구조나 객체 상태를 컴퓨터가 저장하거나 전송하기 편한 방식으로 변환하는 과정
        //private, protected 등은 클래스 외부에서 접근 제한이라 직렬화 불가. [serializeFiedl]는 이를 예외로 만들어줌.
        envSoundValueProp = serializedObject.FindProperty("envSoundSliderValue");
        envSoundLabelTextProp = serializedObject.FindProperty("envSoundLabelText");
    }


    public override void OnInspectorGUI() //입력이 들어올때마다(ex. 마우스를 움직이던지) 업데이트
    {
        //serializedObject는 현재 편집하고 있는데 객체(MVP_Model) 인스턴스
        
        serializedObject.Update(); //MVP_Model 데이터를 가져와 MVP_ModelEditor의 SerializeObject를 최신화함.
        
        EditorGUI.BeginChangeCheck();
        //BeginChangeCheck 이후에 실행된 모든 EditorGUI / EditorGUILayout 호출의 입력 이벤트를 감시. 감시범위는 End 까지
        
        //SerializedProperty를 에디터 화면(인스펙터)에 표시, 편집 가능하게 만듬.
        EditorGUILayout.PropertyField(envSoundValueProp);
        EditorGUILayout.PropertyField(envSoundLabelTextProp);
        
        if (EditorGUI.EndChangeCheck()) //실제로 변화했는지 확인
        {
            serializedObject.ApplyModifiedProperties(); //수정된 SerializedProperty 값을 실제 가리키는 대상에게 적용

            //타겟 클래스의 변수 프로퍼티를 실행
            targetSO.EnvSoundSliderValue = envSoundValueProp.floatValue;
            targetSO.EnvSoundLabelText = envSoundLabelTextProp.stringValue;
        }
    }
}
