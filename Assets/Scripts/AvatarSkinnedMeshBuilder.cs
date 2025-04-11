using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class AvatarSkinnedMeshBuilder : MonoBehaviour
{
    [Tooltip("SkinnedMeshRenderer that will hold the combined mesh")]
    public SkinnedMeshRenderer outputRenderer;

    [SerializeField] private Transform armatureTransform;
    [SerializeField] private RestPoseData restPoseData;

    private List<SkinnedMeshRenderer> _parts = new();
    private Dictionary<string, Transform> boneMap;

    /// <summary>
    /// Builds a name-to-bone map from the provided rig root and its children.
    /// This allows fast lookup when remapping bones by name.
    /// </summary>
    private void Awake()
    {
        boneMap = new Dictionary<string, Transform>();

        // Include the rig root itself
        boneMap[armatureTransform.name] = armatureTransform;

        // Add all child bones to the map
        foreach (var bone in armatureTransform.GetComponentsInChildren<Transform>())
        {
            boneMap[bone.name] = bone;
        }
    }

    /// <summary>
    /// Destroys any previously bound parts and clears the tracking list.
    /// Call this before rebinding or baking a new configuration.
    /// </summary>
    public void Clear()
    {
        foreach (var part in _parts)
        {
            if (part != null && part.gameObject != null)
                Destroy(part.gameObject);
        }

        _parts.Clear();
    }

    /// <summary>
    /// Instantiates a modular avatar part, assigns its bone hierarchy from the live rig,
    /// and registers it for later mesh baking.
    /// </summary>
    public void Bind(AvatarPart partData, Transform partParent)
    {
        // Instantiate the part prefab under the avatar's hierarchy
        var partInstance = Instantiate(partData.prefab, partParent);

        // Retrieve the SkinnedMeshRenderer from the new part
        var skinnedRenderer = partInstance.GetComponent<SkinnedMeshRenderer>();
        if (skinnedRenderer == null)
        {
            return;
        }

        // Get the stored bone names from the part’s bone map
        var boneNames = partData.boneMap?.boneNames;
        if (boneNames == null || boneNames.Length == 0)
        {
            return;
        }

        // Rebuild the bone array by resolving names against the live rig
        Transform[] resolvedBones = new Transform[boneNames.Length];
        for (int i = 0; i < boneNames.Length; i++)
        {
            string boneName = boneNames[i];
            if (!string.IsNullOrEmpty(boneName) && boneMap.TryGetValue(boneName, out var remappedBone))
            {
                resolvedBones[i] = remappedBone;
            }
        }

        // Apply resolved bones and rig root to the part’s SkinnedMeshRenderer
        skinnedRenderer.bones = resolvedBones;
        skinnedRenderer.rootBone = armatureTransform;

        // Register this part for later mesh combination
        _parts.Add(skinnedRenderer);
    }

    /// <summary>
    /// Combines all currently bound skinned parts into a single skinned mesh,
    /// preserving bone weights and bindposes, and assigns it to the output SkinnedMeshRenderer.
    /// </summary>
    public void Bake()
    {
        if (_parts.Count == 0)
        {
            Debug.LogWarning("No parts to bake.");
            return;
        }

        // Create a new mesh that will hold the combined geometry
        var combinedMesh = new Mesh
        {
            name = "CombinedSkinnedMesh"
        };

        // Working buffers for mesh data
        List<Vector3> vertices = new();
        List<Vector3> normals = new();
        List<Vector2> uvs = new();
        List<BoneWeight> boneWeights = new();
        List<Matrix4x4> bindposes = new();
        List<int> indices = new();

        int vertexOffset = 0;

        // Build a list of all bone names used by the bound parts
        var requiredBoneNames = new HashSet<string>();
        foreach (var skinnedPart in _parts)
        {
            foreach (var bone in skinnedPart.bones)
            {
                if (bone != null)
                    requiredBoneNames.Add(bone.name);
            }
        }

        // Convert bone names to actual Transforms using the rig’s bone map
        List<Transform> outputBonesList = new();
        foreach (var name in requiredBoneNames)
        {
            if (boneMap.TryGetValue(name, out var bone))
            {
                outputBonesList.Add(bone);
            }
            else
            {
                Debug.LogWarning($"Bone '{name}' not found in rig.");
            }
        }

        Transform[] outputBones = outputBonesList.ToArray();

        // Map bone names to their index in the final combined bone array
        Dictionary<string, int> boneNameToIndex = new();
        for (int i = 0; i < outputBones.Length; i++)
        {
            boneNameToIndex[outputBones[i].name] = i;
        }

        // Combine all geometry data from each part
        foreach (var skinnedPart in _parts)
        {
            var mesh = skinnedPart.sharedMesh;
            if (mesh == null) continue;

            // Transform from local space to the rig's root space
            Matrix4x4 meshToRoot = armatureTransform.worldToLocalMatrix * skinnedPart.transform.localToWorldMatrix;

            var meshVertices = mesh.vertices;
            var meshNormals = mesh.normals;
            var meshUVs = mesh.uv;
            var meshIndices = mesh.GetIndices(0);

            // Transform and append vertex data
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                vertices.Add(meshToRoot.MultiplyPoint3x4(meshVertices[i]));
                normals.Add(meshToRoot.MultiplyVector(meshNormals[i]));
                uvs.Add(meshUVs[i]);
            }

            // Remap bone weights to use indices from the output bone array
            foreach (var bw in mesh.boneWeights)
            {
                BoneWeight remapped = new()
                {
                    weight0 = bw.weight0,
                    weight1 = bw.weight1,
                    weight2 = bw.weight2,
                    weight3 = bw.weight3,
                    boneIndex0 = SafeRemapBoneIndex(skinnedPart.bones, bw.boneIndex0, boneNameToIndex),
                    boneIndex1 = SafeRemapBoneIndex(skinnedPart.bones, bw.boneIndex1, boneNameToIndex),
                    boneIndex2 = SafeRemapBoneIndex(skinnedPart.bones, bw.boneIndex2, boneNameToIndex),
                    boneIndex3 = SafeRemapBoneIndex(skinnedPart.bones, bw.boneIndex3, boneNameToIndex),
                };

                boneWeights.Add(remapped);
            }

            // Offset and append triangle indices
            foreach (var i in meshIndices)
            {
                indices.Add(i + vertexOffset);
            }

            vertexOffset += mesh.vertexCount;
        }

        // Calculate bindposes using current bone transforms relative to the rig root
        Matrix4x4 rootMatrix = armatureTransform.localToWorldMatrix;
        foreach (var bone in outputBones)
        {
            Matrix4x4 bindpose = bone.worldToLocalMatrix * rootMatrix;
            bindposes.Add(bindpose);
        }

        // Final mesh assignment
        combinedMesh.SetVertices(vertices);
        combinedMesh.SetNormals(normals);
        combinedMesh.SetUVs(0, uvs);
        combinedMesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
        combinedMesh.boneWeights = boneWeights.ToArray();
        combinedMesh.bindposes = bindposes.ToArray();
        combinedMesh.RecalculateBounds();
        combinedMesh.UploadMeshData(false);

        // Assign the combined mesh and bone data to the output SkinnedMeshRenderer
        outputRenderer.sharedMesh = combinedMesh;
        outputRenderer.bones = outputBones;
        outputRenderer.rootBone = armatureTransform;
        outputRenderer.updateWhenOffscreen = true;
        outputRenderer.sharedMaterial = outputRenderer.sharedMaterial;

        // Hide the original part renderers
        foreach (var part in _parts)
        {
            if (part != null)
                part.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Restores the local position and rotation of all bones to their original rest pose,
    /// captured in the RestPoseData SO
    /// </summary>
    public void RestoreRestPose()
    {
        if (restPoseData == null)
        {
            return;
        }

        foreach (var pose in restPoseData.bones)
        {
            if (boneMap.TryGetValue(pose.boneName, out var bone))
            {
                bone.localPosition = pose.localPosition;
                bone.localRotation = pose.localRotation;
            }
        }
    }

    
    // Safely remaps a bone index from a source bone array to the index used in the combined mesh
    private int SafeRemapBoneIndex(Transform[] boneArray, int index, Dictionary<string, int> nameToIndexMap)
    {
        if (index < 0 || index >= boneArray.Length)
        {
            return 0;
        }

        var bone = boneArray[index];
        if (bone == null)
        {
            return 0;
        }

        if (!nameToIndexMap.TryGetValue(bone.name, out int remappedIndex))
        {
            return 0;
        }

        return remappedIndex;
    }
}
