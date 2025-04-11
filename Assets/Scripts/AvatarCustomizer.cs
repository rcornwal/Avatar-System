using UnityEngine;

[RequireComponent(typeof(AvatarAssembler))]
public class AvatarCustomizer : MonoBehaviour, IAvatarCustomizer
{
    [SerializeField] private AvatarData avatarData;
    [SerializeField] private AvatarConfig activeConfig;

    private AvatarAssembler _assembler;

    private int _hairIndex;
    private int _topIndex;
    private int _bottomIndex;

    private void Awake()
    {
        _assembler = GetComponent<AvatarAssembler>();
    }

    private void Start()
    {
        // Find the initial indices for each part in the avatar config
        _hairIndex = FindIndex(avatarData.hairs, activeConfig.hair);
        _topIndex = FindIndex(avatarData.tops, activeConfig.top);
        _bottomIndex = FindIndex(avatarData.bottoms, activeConfig.bottom);

        ApplyCurrent();
    }

    public void SwapHair()
    {
        _hairIndex = (_hairIndex + 1) % avatarData.hairs.Length;
        activeConfig.hair = avatarData.hairs[_hairIndex];
        ApplyCurrent();
    }

    public void SwapTop()
    {
        _topIndex = (_topIndex + 1) % avatarData.tops.Length;
        activeConfig.top = avatarData.tops[_topIndex];
        ApplyCurrent();
    }

    public void SwapBottom()
    {
        _bottomIndex = (_bottomIndex + 1) % avatarData.bottoms.Length;
        activeConfig.bottom = avatarData.bottoms[_bottomIndex];
        ApplyCurrent();
    }

    private void ApplyCurrent()
    {
        _assembler.ApplyConfig(activeConfig);
    }

    /// <summary>
    /// Finds the index of the current part in the source list, comparing by part name.
    /// Returns 0 if not found.
    /// </summary>
    private int FindIndex(AvatarPart[] options, AvatarPart target)
    {
        if (target == null || options == null)
            return 0;

        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null && options[i].partName == target.partName)
                return i;
        }

        return 0;
    }
}