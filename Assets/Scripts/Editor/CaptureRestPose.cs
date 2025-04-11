using UnityEditor;
using UnityEngine;

/// <summary>
/// An editor window that captures the current local transform pose of a rig hierarchy
/// and stores it in a RestPoseData asset for use at runtime.
/// 
/// Used for restoring a rig to its rest pose during skinned mesh rebaking.
/// </summary>
public class CaptureRestPose : EditorWindow
{
    private Transform root;
    private RestPoseData poseAsset;

    [MenuItem("Tools/Avatar/Capture Rest Pose")]
    public static void ShowWindow()
    {
        GetWindow<CaptureRestPose>("Capture Rest Pose");
    }

    void OnGUI()
    {
        // Select the root
        root = (Transform)EditorGUILayout.ObjectField("Armature Transform", root, typeof(Transform), true);

        // Select the asset to save the pose data to
        poseAsset = (RestPoseData)EditorGUILayout.ObjectField("Save To", poseAsset, typeof(RestPoseData), false);

        // Button to trigger the capture
        if (GUILayout.Button("Capture"))
        {
            if (root == null || poseAsset == null)
            {
                Debug.LogWarning("Assign both a rig root and a RestPoseData asset.");
                return;
            }

            CapturePose();
        }
    }

    /// <summary>
    /// Captures the current localPosition and localRotation of all bones under the root
    /// and writes them to the RestPoseData asset.
    /// </summary>
    private void CapturePose()
    {
        poseAsset.bones.Clear();

        foreach (var bone in root.GetComponentsInChildren<Transform>())
        {
            poseAsset.bones.Add(new RestPoseData.BonePose
            {
                boneName = bone.name,
                localPosition = bone.localPosition,
                localRotation = bone.localRotation
            });
        }

        // Mark asset dirty so Unity knows to save it
        EditorUtility.SetDirty(poseAsset);
        AssetDatabase.SaveAssets();
    }
}
