using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RestPoseData", menuName = "Avatar/Rest Pose")]
public class RestPoseData : ScriptableObject
{
    [System.Serializable]
    public class BonePose
    {
        public string boneName;
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    public List<BonePose> bones = new();
}