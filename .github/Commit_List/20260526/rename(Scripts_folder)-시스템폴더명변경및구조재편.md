<!-- 피드백
- 폴더 rename에서 폴더 이름 바뀐 것 바로 밑에 있는 것들은 뭐야? 왜 넣은거야?
- 함께 삭제된 파일 중에서 삭제가 아니라 파일 이름이 수정되거나 파일이 이동된 것도 있는 것 같은데?
-->

rename(Scripts_folder): 시스템 폴더명 변경 및 구조 재편

시스템 역할이 명확히 드러나도록 폴더명을 수정하고, 이 과정에서 불필요한 파일 삭제.

폴더 rename:
- Inject/ → DISystem/
  - DIContainer, IInjectRequester, IInjectTarget
- GameState/ → GameStateSystem/
  - GameManager, IGameStateEvent, Test_GameStateChanger·Pause·InputBlock
- InputSystem/Enum/ → InputSystem/Types/
  - InputActionType, InputPlatformType
- PlayerStateSystem/ → PlayerSystem/
  - 내부 구조도 재편 (State/, Manager/ → States/, States/Manager/, Character/, Swap/, Movement/)

함께 삭제된 파일:
- AssemblyCache.cs
- DataSystem/DataClass/ (AudioDataClass, InputDataClass, SkillEffectDataClass)
- DataSystem/Editor/StateDataEntryDrawer.cs
- DataSystem/StateDataManager/ (StateDataManager, StateDataEntry, IStateDataProvider)
- DataSystem/ 루트 (HIdleData, HNormalAttack1~3 .cs/.asset, IHasTimingData, ITimingData, StateDataCategoryType)
- 4. Plug-in/DoubleL/Demo/Anim/OneHand_Up_Attack_B_1~3_InPlace.anim
