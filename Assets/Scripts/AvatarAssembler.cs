using UnityEngine;

public class AvatarAssembler : MonoBehaviour
{
    public Transform partParent;
    private GameObject currentHair, currentTop, currentBottom;

    [SerializeField] private AvatarSkinnedMeshBuilder avatarSkinnedMeshBuilder;
    [SerializeField] private Animator animator;

    public void ApplyConfig(AvatarConfig config)
    {
        // pause animation as we do a mesh combine
        animator.enabled = false;
        
        // clean
        avatarSkinnedMeshBuilder.RestoreRestPose();
        avatarSkinnedMeshBuilder.Clear();

        // create & bind all avatar parts
        if (config.hair) avatarSkinnedMeshBuilder.Bind(config.hair, partParent);
        if (config.top) avatarSkinnedMeshBuilder.Bind(config.top, partParent);
        if (config.bottom) avatarSkinnedMeshBuilder.Bind(config.bottom, partParent);

        // combine parts
        avatarSkinnedMeshBuilder.Bake();
        animator.enabled = true;    
    }
}
