using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // 표를 CSV로 빼고 넣는다. 이게 백업이자 엑셀로 고치는 통로다.
    // SO 연결이 끊겨 표가 통째로 날아가도 CSV만 있으면 되살릴 수 있다.
    public static class TextTableCsv
    {
        [MenuItem("Tools/언어/CSV로 내보내기")]
        private static void Export()
        {
            TextTableData table = FindTable();

            if (table == null)
            {
                return;
            }

            string path = EditorUtility.SaveFilePanel("표 내보내기", Application.dataPath, "TextTable", "csv");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Key,Korean,English");

            foreach (TextTableData.Entry entry in table.Entries)
            {
                builder.AppendLine($"{Quote(entry.Key)},{Quote(entry.Korean)},{Quote(entry.English)}");
            }

            // 엑셀에서 한글이 안 깨지게 BOM을 붙여 쓴다.
            File.WriteAllText(path, builder.ToString(), new UTF8Encoding(true));

            Debug.Log($"[TextTableCsv] 내보냄: {path} ({table.Entries.Count}줄)");
        }

        [MenuItem("Tools/언어/CSV에서 불러오기")]
        private static void Import()
        {
            TextTableData table = FindTable();

            if (table == null)
            {
                return;
            }

            string path = EditorUtility.OpenFilePanel("표 불러오기", Application.dataPath, "csv");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            List<TextTableData.Entry> loaded = new List<TextTableData.Entry>();
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);

            // 첫 줄은 제목이라 건너뛴다.
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                List<string> cells = SplitCsvLine(lines[i]);

                if (cells.Count < 3)
                {
                    Debug.LogWarning($"[TextTableCsv] {i + 1}번째 줄을 못 읽음: {lines[i]}");
                    continue;
                }

                loaded.Add(new TextTableData.Entry
                {
                    Key = cells[0],
                    Korean = cells[1],
                    English = cells[2],
                });
            }

            Undo.RecordObject(table, "표 불러오기");
            table.Entries.Clear();
            table.Entries.AddRange(loaded);
            table.ClearIndex();
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();

            Debug.Log($"[TextTableCsv] 불러옴: {loaded.Count}줄");
        }

        // 쉼표·큰따옴표·줄바꿈이 들어간 칸은 큰따옴표로 감싸고, 안쪽 따옴표는 두 번 쓴다(CSV 규칙).
        private static string Quote(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (!value.Contains(",") && !value.Contains("\"") && !value.Contains("\n"))
            {
                return value;
            }

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        // 따옴표로 감싼 칸 안의 쉼표는 칸 구분이 아니다.
        private static List<string> SplitCsvLine(string line)
        {
            List<string> cells = new List<string>();
            StringBuilder cell = new StringBuilder();
            bool inQuote = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuote)
                {
                    // 따옴표 두 개는 따옴표 한 글자를 뜻한다.
                    if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                    }
                    else if (c == '"')
                    {
                        inQuote = false;
                    }
                    else
                    {
                        cell.Append(c);
                    }

                    continue;
                }

                if (c == '"')
                {
                    inQuote = true;
                }
                else if (c == ',')
                {
                    cells.Add(cell.ToString());
                    cell.Clear();
                }
                else
                {
                    cell.Append(c);
                }
            }

            cells.Add(cell.ToString());

            return cells;
        }

        private static TextTableData FindTable()
        {
            string[] guids = AssetDatabase.FindAssets("t:TextTableData");

            if (guids.Length == 0)
            {
                Debug.LogError("[TextTableCsv] TextTableData 에셋을 못 찾음. 먼저 만들어야 함.");
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<TextTableData>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
