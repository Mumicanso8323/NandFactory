#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class PaletteTextAssetImporter {
    [MenuItem("Assets/Create/Palette/Import Palette TextAsset", true)]
    private static bool ValidateImport() {
        return Selection.activeObject is TextAsset;
    }

    [MenuItem("Assets/Create/Palette/Import Palette TextAsset")]
    private static void Import() {
        var ta = Selection.activeObject as TextAsset;
        if (ta == null) {
            EditorUtility.DisplayDialog("Import Palette", "Select a TextAsset (.pal/.hex/.txt) first.", "OK");
            return;
        }

        var colors = ParseColors(ta.text);
        if (colors == null || colors.Length == 0) {
            EditorUtility.DisplayDialog("Import Palette", "No colors parsed. Check the file format.", "OK");
            return;
        }

        // Save location next to the selected file
        var srcPath = AssetDatabase.GetAssetPath(ta);
        var dir = Path.GetDirectoryName(srcPath)?.Replace("\\", "/") ?? "Assets";
        var baseName = Path.GetFileNameWithoutExtension(srcPath);

        // Create ScriptableObject asset
        var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{dir}/{baseName}Palette.asset");
        var asset = ScriptableObject.CreateInstance<PaletteAsset>();
        asset.colors = colors;

        // Generate strip texture (width=N, height=1)
        var tex = GenerateStripTexture(colors, $"{baseName}PaletteStrip");
        var texPath = AssetDatabase.GenerateUniqueAssetPath($"{dir}/{baseName}Palette.png");
        SaveTextureAsPng(tex, texPath);

        // Reimport texture with correct settings (Point, no compression)
        AssetDatabase.ImportAsset(texPath);
        ConfigureTextureImporter(texPath);

        // Load imported texture and assign
        asset.stripTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Import Palette",
            $"Imported {colors.Length} colors.\nCreated:\n- {assetPath}\n- {texPath}",
            "OK"
        );
    }

    private static Color32[] ParseColors(string text) {
        // Accept:
        // - paint.net .pal lines like: FFf2f0e5 (AARRGGBB hex)
        // - #RRGGBB
        // - RRGGBB
        // - AARRGGBB
        // - With optional commas/spaces
        var lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

        var list = lines
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Where(l => !l.StartsWith(";")) // paint.net comment
            .Select(TryParseHexLine)
            .Where(c => c.HasValue)
            .Select(c => c.Value)
            .ToArray();

        return list;
    }

    private static Color32? TryParseHexLine(string line) {
        // Remove separators
        line = line.Trim();
        line = line.Replace(",", "").Replace(" ", "").Replace("\t", "");

        // Allow leading '#'
        if (line.StartsWith("#")) line = line.Substring(1);

        // Sometimes lines might be like "0xRRGGBB"
        if (line.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            line = line.Substring(2);

        // Accept 6 or 8 hex digits
        if (line.Length != 6 && line.Length != 8) return null;

        if (!uint.TryParse(line, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
            return null;

        byte a, r, g, b;

        if (line.Length == 6) {
            a = 255;
            r = (byte)((value >> 16) & 0xFF);
            g = (byte)((value >> 8) & 0xFF);
            b = (byte)(value & 0xFF);
        } else {
            // AARRGGBB
            a = (byte)((value >> 24) & 0xFF);
            r = (byte)((value >> 16) & 0xFF);
            g = (byte)((value >> 8) & 0xFF);
            b = (byte)(value & 0xFF);
        }

        return new Color32(r, g, b, a);
    }

    private static Texture2D GenerateStripTexture(Color32[] colors, string name) {
        var tex = new Texture2D(colors.Length, 1, TextureFormat.RGBA32, false, false);
        tex.name = name;
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels32(colors);
        tex.Apply(false, false);
        return tex;
    }

    private static void SaveTextureAsPng(Texture2D tex, string assetPath) {
        var bytes = tex.EncodeToPNG();
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        var folder = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        File.WriteAllBytes(fullPath, bytes);
    }

    private static void ConfigureTextureImporter(string assetPath) {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = false;

        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;

        importer.SaveAndReimport();
    }
}
#endif