using UnityEngine;

[CreateAssetMenu(menuName = "Palette/Palette Asset", fileName = "PaletteAsset")]
public class PaletteAsset : ScriptableObject {
    [Tooltip("Palette colors in order. Usually 29 for CC-29.")]
    public Color32[] colors;

    [Tooltip("Optional 1D strip texture generated from colors (width=colors.Length, height=1).")]
    public Texture2D stripTexture;

    public int Count => colors?.Length ?? 0;

    public Color32 Get(int index) {
        if (colors == null || colors.Length == 0) return new Color32(255, 0, 255, 255);
        index = Mathf.Clamp(index, 0, colors.Length - 1);
        return colors[index];
    }
}