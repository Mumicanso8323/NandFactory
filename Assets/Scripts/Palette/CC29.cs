using UnityEngine;

public static class CC29 {
    // Resources/Palettes/CC29Palette.asset を読む
    private const string ResourcesPath = "Palettes/CC29Palette";

    private static PaletteAsset _asset;

    private static PaletteAsset Asset {
        get {
            if (_asset != null) return _asset;
            _asset = Resources.Load<PaletteAsset>(ResourcesPath);
            if (_asset == null) {
                Debug.LogError($"CC29 palette asset not found at Resources/{ResourcesPath}.asset");
            }
            return _asset;
        }
    }

    public static class C {
        public static Color32 Bg => Get(6);   // #212123
        public static Color32 PanelA => Get(7);   // #352b42
        public static Color32 PanelB => Get(8);   // #43436a
        public static Color32 Text => Get(0);   // #f2f0e5
        public static Color32 Highlight => Get(12);  // #ede19e
        public static Color32 Low => Get(14);  // #b45252
        public static Color32 High => Get(10);  // #68c2d3
        public static Color32 Error => Get(27);  // #cf8acb
    }

    public static Color32 Get(int index) {
        var a = Asset;
        if (a == null || a.colors == null || a.colors.Length == 0)
            return new Color32(255, 0, 255, 255);
        index = Mathf.Clamp(index, 0, a.colors.Length - 1);
        return a.colors[index];
    }

    // シェーダ用（29色ストリップ）
    public static Texture2D StripTexture => Asset != null ? Asset.stripTexture : null;
}