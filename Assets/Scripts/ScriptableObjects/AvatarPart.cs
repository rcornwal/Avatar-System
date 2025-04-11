using UnityEngine;

[CreateAssetMenu(fileName = "AvatarPart", menuName = "Avatar/Avatar Part")]
public class AvatarPart : ScriptableObject
{
    public string partName;
    public GameObject prefab;
    public AvatarBoneMap boneMap;
}
