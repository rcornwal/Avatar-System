using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Custom inspector for SkinnedMeshRenderer that adds a button to create a BoneMap asset
/// </summary>
[CustomEditor(typeof(SkinnedMeshRenderer))]
public class CaptureBoneMap : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Bone Map Utilities", EditorStyles.boldLabel);

        if (GUILayout.Button("Create BoneMap"))
        {
            var smr = (SkinnedMeshRenderer)target;
            CreateBoneMap(smr);
        }
    }

    /// <summary>
    /// Extracts the bone names from the SkinnedMeshRenderer and saves them into a bone map asset
    /// </summary>
    private void CreateBoneMap(SkinnedMeshRenderer smr)
    {
        if (smr.bones == null || smr.bones.Length == 0)
        {
            return;
        }

        List<string> boneNames = new();
        foreach (var bone in smr.bones)
        {
            boneNames.Add(bone != null ? bone.name : "");
        }

        // Create the SO
        AvatarBoneMap map = CreateInstance<AvatarBoneMap>();
        map.boneNames = boneNames.ToArray();

        // Ensure the target folder exists
        string dir = "Assets/AvatarData";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
        }

        // Create and save the asset
        string path = Path.Combine(dir, $"BoneMap_{smr.name}.asset");
        AssetDatabase.CreateAsset(map, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Select the new asset
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = map;
    }
}
