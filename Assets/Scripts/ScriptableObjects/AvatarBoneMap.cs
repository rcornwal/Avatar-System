using UnityEngine;

/// <summary>
/// Stores the list of bone names used by a skinned mesh part,
/// in the exact order required by its SkinnedMeshRenderer.
/// 
/// This allows the runtime system to reconstruct the bones array
/// when the original armature is stripped from the prefab
/// </summary>
[CreateAssetMenu(fileName = "BoneMap", menuName = "Avatar/Bone Name Map")]
public class AvatarBoneMap : ScriptableObject
{
    public string[] boneNames;
}