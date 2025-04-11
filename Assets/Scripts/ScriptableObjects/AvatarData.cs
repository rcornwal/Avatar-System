using UnityEngine;

[CreateAssetMenu(fileName = "AvatarData", menuName = "Avatar/Avatar Data")]
public class AvatarData : ScriptableObject
{
    public AvatarPart[] hairs;
    public AvatarPart[] tops;
    public AvatarPart[] bottoms;
}