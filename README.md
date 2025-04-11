# Modular Avatar System
## Overview
This system was designed to support modular 3D avatars, where each character can be assembled from interchangeable parts like hair, top, and bottom. All parts are rigged to the same skeleton and are swappable at runtime via UI buttons.

<img width="707" alt="Screenshot 2025-04-11 at 10 24 54 AM" src="https://github.com/user-attachments/assets/4043238f-bc7f-4505-9220-569a5980dfba" />

**Key assumptions:**
* All avatar parts are rigged to the same armature hierarchy.
* The armature exists once in the scene and drives all meshes.
* There is one animator that drives motion for all parts.
* Each part prefab is made of 1 `SkinnedMeshRenderer`. It only contains the geometry and uses a pre-captured BoneMap asset to rebuild the bones array.
* A `RestPoseData` asset is used to restore the skeleton to a known rest pose before rebaking, keeping consistent mesh deformation during animation.

## Rendering Optimization
To reduce draw calls, I implemented a custom runtime skinned mesh combiner:
* Each modular part (hair, top, bottom) is instantiated and rigged using a pre-generated AvatarBoneMap that stores its original bone names.
* The system reads each part's sharedMesh, transforms vertices into the combined rig’s local space, remaps bone weights using a shared bone index map, and merges all geometry into a single combined Mesh.
* The final mesh is assigned to a new `SkinnedMeshRenderer` that uses the shared skeleton, with correct bindposes and bone weights.

This approach reduces draw calls by rendering one mesh instead of 1 per part.
* Preserves skinned mesh deformation via animator
* Supports runtime modularity and part swapping, even while animating

This solution was chosen over GPU instancing or material atlasing because:
* Instancing doesn't apply to animated skinned meshes.
* Atlasing would increase UV/material complexity with performance gain

## Results
| Condition              | Draw Calls | Shadow Casters | Skinned Meshes | Description                         |
|------------------------|------------|----------------|----------------|----------------
| Before Baking          | 3          | 4              | 4              | Each part rendered separately       |
| After Baking           | 1          | 1              | 1              | Combined into a single skinned mesh |

**Before Baking**
<img width="776" alt="Screenshot 2025-04-11 at 10 19 52 AM" src="https://github.com/user-attachments/assets/7617b41a-27ff-408d-8bf7-dbbb7e979ae7" />

**After Backing**
<img width="744" alt="Screenshot 2025-04-11 at 10 26 58 AM" src="https://github.com/user-attachments/assets/bd459b00-aeab-482d-b1f6-3fd9fbec7fbf" />
