using UnityEngine;

[CreateAssetMenu(fileName = "AvatarConfig", menuName = "Avatar/Avatar Config")]
public class AvatarConfig : ScriptableObject
{
    public AvatarPart hair;
    public AvatarPart top;
    public AvatarPart bottom;
}
