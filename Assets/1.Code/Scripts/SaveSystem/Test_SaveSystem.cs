using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    public class Test_SaveSystem : MonoBehaviour
    {
        [SerializeField]
        private SaveManager _saveManager;

        [Header("저장 버튼")]
        [SerializeField]
        private Button _saveGameButton;

        [SerializeField]
        private Button _saveInputButton;

        [SerializeField]
        private Button _saveSoundButton;

        [Header("불러오기 버튼")]
        [SerializeField]
        private Button _loadGameButton;

        [SerializeField]
        private Button _loadInputButton;

        [SerializeField]
        private Button _loadSoundButton;

        private void Start()
        {
            _saveGameButton.onClick.AddListener(HandleSaveGameClicked);
            _saveInputButton.onClick.AddListener(HandleSaveInputClicked);
            _saveSoundButton.onClick.AddListener(HandleSaveSoundClicked);
            _loadGameButton.onClick.AddListener(HandleLoadGameClicked);
            _loadInputButton.onClick.AddListener(HandleLoadInputClicked);
            _loadSoundButton.onClick.AddListener(HandleLoadSoundClicked);
        }

        private void OnDestroy()
        {
            _saveGameButton.onClick.RemoveListener(HandleSaveGameClicked);
            _saveInputButton.onClick.RemoveListener(HandleSaveInputClicked);
            _saveSoundButton.onClick.RemoveListener(HandleSaveSoundClicked);
            _loadGameButton.onClick.RemoveListener(HandleLoadGameClicked);
            _loadInputButton.onClick.RemoveListener(HandleLoadInputClicked);
            _loadSoundButton.onClick.RemoveListener(HandleLoadSoundClicked);
        }

        private void HandleSaveGameClicked()
        {
            var data = new GameSaveData
            {
                SceneIndex = 1,
                ZoneIndex = 2,
                MaskType = 0,
                PlayerPosition = new Vector3(1.5f, 0f, 3.2f),
                Hp = 15f,
                LightingState = true,
                PostProcessState = false
            };

            bool result = _saveManager.Save(data);
            Debug.Log($"[Test] GameSaveData 저장 {(result ? "성공" : "실패")}");
        }

        private void HandleSaveInputClicked()
        {
            var data = new InputData
            {
                KeyBindings = new List<KeyBindingEntry>
                {
                    new KeyBindingEntry { actionName = "Attack", keyCode = (int)KeyCode.Mouse0 },
                    new KeyBindingEntry { actionName = "Skill", keyCode = (int)KeyCode.Q },
                    new KeyBindingEntry { actionName = "Dash", keyCode = (int)KeyCode.Mouse1 }
                },
                MouseSensitivity = 1.5f
            };

            bool result = _saveManager.Save(data);
            Debug.Log($"[Test] InputData 저장 {(result ? "성공" : "실패")}");
        }

        private void HandleSaveSoundClicked()
        {
            var data = new SoundData
            {
                MasterVolume = 0.8f,
                BgmVolume = 0.6f,
                EnemySfxVolume = 0.7f,
                PlayerSfxVolume = 0.9f
            };

            bool result = _saveManager.Save(data);
            Debug.Log($"[Test] SoundData 저장 {(result ? "성공" : "실패")}");
        }

        private void HandleLoadGameClicked()
        {
            GameSaveData data = _saveManager.Load<GameSaveData>();

            if (data == null)
            {
                Debug.Log("[Test] GameSaveData 불러오기 실패 — 저장된 파일 없음");
                return;
            }

            Debug.Log($"[Test] GameSaveData 불러오기 성공\n" +
                      $"씬: {data.SceneIndex}, 구역: {data.ZoneIndex}, 마스크: {data.MaskType}\n" +
                      $"위치: {data.PlayerPosition.ToVector3()}, HP: {data.Hp}\n" +
                      $"조명: {data.LightingState}, PostProcess: {data.PostProcessState}");
        }

        private void HandleLoadInputClicked()
        {
            InputData data = _saveManager.Load<InputData>();

            if (data == null)
            {
                Debug.Log("[Test] InputData 불러오기 실패 — 저장된 파일 없음");
                return;
            }

            Debug.Log($"[Test] InputData 불러오기 성공\n마우스 감도: {data.MouseSensitivity}");

            foreach (KeyBindingEntry entry in data.KeyBindings)
            {
                Debug.Log($"  {entry.actionName}: {(KeyCode)entry.keyCode}");
            }
        }

        private void HandleLoadSoundClicked()
        {
            SoundData data = _saveManager.Load<SoundData>();

            if (data == null)
            {
                Debug.Log("[Test] SoundData 불러오기 실패 — 저장된 파일 없음");
                return;
            }

            Debug.Log($"[Test] SoundData 불러오기 성공\n" +
                      $"마스터: {data.MasterVolume}, BGM: {data.BgmVolume}\n" +
                      $"적SFX: {data.EnemySfxVolume}, 플레이어SFX: {data.PlayerSfxVolume}");
        }
    }
}
