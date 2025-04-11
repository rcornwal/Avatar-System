/// <summary>
/// Customizing modular avatar parts at runtime.
/// </summary>
public interface IAvatarCustomizer
{
    /// <summary>
    /// Swaps the current hair with the next available option from AvatarData
    /// and sets AvatarConfig
    /// </summary>
    void SwapHair();

    /// <summary>
    /// Swaps the current top with the next available option from AvatarData
    /// and sets AvatarConfig
    /// </summary>
    void SwapTop();

    /// <summary>
    /// Swaps the current bottom with the next available option from AvatarData
    /// and sets AvatarConfig
    /// </summary>
    void SwapBottom();
}