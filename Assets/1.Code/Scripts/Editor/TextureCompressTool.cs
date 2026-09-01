using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    public static class TextureCompressTool
    {
        private const int MaxTextureSize = 1024;
        private const string AndroidPlatform = "Android";

        [MenuItem("Tools/Texture/Android 압축 일괄 적용 (ASTC 6x6, 1024)")]
        private static void ApplyAndroidCompression()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
            List<string> changedPaths = new List<string>();

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    if (EditorUtility.DisplayCancelableProgressBar(
                            "텍스처 설정 적용 중", path, (float)i / guids.Length))
                    {
                        break;
                    }

                    if (AssetImporter.GetAtPath(path) is not TextureImporter importer) continue;
                    if (!ApplyAndroidSettings(importer)) continue;

                    importer.SaveAndReimport();
                    changedPaths.Add(path);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"텍스처 {changedPaths.Count}개 변경 완료 (전체 {guids.Length}개 검사)");
        }

        // Android 플랫폼 오버라이드를 설정한다. 이미 동일하면 false를 반환한다
        private static bool ApplyAndroidSettings(TextureImporter importer)
        {
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(AndroidPlatform);

            // 노멀맵 등 알파 없는 텍스처도 ASTC 6x6로 통일한다
            TextureImporterFormat targetFormat = TextureImporterFormat.ASTC_6x6;

            bool isSame = settings.overridden
                          && settings.maxTextureSize <= MaxTextureSize
                          && settings.format == targetFormat
                          && settings.textureCompression == TextureImporterCompression.Compressed;
            if (isSame) return false;

            settings.overridden = true;
            settings.maxTextureSize = Mathf.Min(settings.maxTextureSize, MaxTextureSize);
            settings.format = targetFormat;
            settings.textureCompression = TextureImporterCompression.Compressed;
            settings.compressionQuality = (int)TextureCompressionQuality.Normal;

            importer.SetPlatformTextureSettings(settings);
            return true;
        }
    }
}

// TODO: EditorWindow 형태의 텍스처 관리 툴로 확장
// - 수집 버튼: 프로젝트 전체 텍스처(또는 씬에서 실제 참조되는 텍스처만) 목록화
// - 표 형태 표시: 행=텍스처, 열=플랫폼별(Default/Android) MaxSize·Format·Compression
// - 일괄 적용 버튼: 플랫폼별 설정을 한 번에 적용
// - 예외 체크박스: 체크된 텍스처는 일괄 적용에서 제외
//   (예외 목록은 경로가 아닌 GUID로 ScriptableObject에 저장해야 파일 이동에 안 깨짐)
// - 신규 임포트 텍스처는 툴로 못 잡으므로 AssetPostprocessor 병행 필요
