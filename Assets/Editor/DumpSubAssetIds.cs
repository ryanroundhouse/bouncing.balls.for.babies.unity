using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class DumpSubAssetIds
{
    [MenuItem("Tools/Dump FBX Sub-Asset IDs")]
    public static void Dump()
    {
        var paths = new[] {
            "Assets/Resources/BugAnimated.fbx",
            "Assets/Resources/goo.fbx",
        };

        var sb = new StringBuilder();
        foreach (var path in paths)
        {
            sb.AppendLine("=== " + path + " ===");
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a == null) continue;
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(a, out string guid, out long fileId))
                {
                    sb.AppendLine($"{fileId}\t{a.GetType().Name}\t{a.name}");
                }
            }
            sb.AppendLine();
        }

        var outPath = Path.Combine(Application.dataPath, "../subasset_dump.txt");
        File.WriteAllText(outPath, sb.ToString());
        Debug.Log("Wrote " + outPath);
        EditorUtility.DisplayDialog("Dump complete", "Wrote subasset_dump.txt at project root", "OK");
    }
}
