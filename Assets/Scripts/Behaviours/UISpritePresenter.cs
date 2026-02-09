using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public sealed class UISpritePresenter : MonoBehaviour {
    [Serializable]
    public struct SpriteSet {
        public string key;          // "green", "red", "error", "input" など
        public Sprite[] frames;     // 任意枚数
    }

    [Header("Sprite Sets")]
    [SerializeField] private SpriteSet[] sets;

    [Header("Options")]
    [SerializeField] private bool setNativeSizeOnChange = false;

    private Image img;
    private readonly Dictionary<string, Sprite[]> map = new();

    private Sprite[] currentFrames;
    private int currentIndex;

    // 任意順序列（例：0,1,2,1,0,3...）
    private int[] sequence;
    private int sequencePos;

    private void Awake() {
        img = GetComponent<Image>();

        map.Clear();
        foreach (var s in sets) {
            if (string.IsNullOrWhiteSpace(s.key) || s.frames == null) continue;
            map[s.key] = s.frames;
        }

        // 初期セット（あれば最初のやつ）
        if (sets != null && sets.Length > 0)
            SetSet(sets[0].key, resetIndex: true);
        else
            ApplySprite(null);
    }

    // --- 基本：セット切替 ---
    public bool SetSet(string key, bool resetIndex = true) {
        if (!map.TryGetValue(key, out var frames) || frames == null || frames.Length == 0)
            return false;

        currentFrames = frames;
        if (resetIndex) currentIndex = 0;
        ClampIndex();
        ApplyCurrent();
        return true;
    }

    // --- 基本：任意フレームを指定 ---
    public void SetIndex(int index) {
        currentIndex = index;
        ClampIndex();
        ApplyCurrent();
    }

    public void SetSprite(Sprite sprite) {
        // セットに依存しない単発表示（例：特別な警告アイコンなど）
        currentFrames = null;
        sequence = null;
        ApplySprite(sprite);
    }

    // --- 任意順序列を外から与える（ここが“事故らない”ポイント） ---
    public void SetSequence(int[] seq, bool reset = true, bool applyNow = true) {
        sequence = seq;
        if (reset) sequencePos = 0;

        if (applyNow) ApplySequenceFrame();
    }

    // 外から呼んだときだけ進む
    public void StepSequence(int step = 1, bool loop = true) {
        if (sequence == null || sequence.Length == 0) return;

        sequencePos += step;

        if (loop) {
            sequencePos %= sequence.Length;
            if (sequencePos < 0) sequencePos += sequence.Length;
        } else {
            sequencePos = Mathf.Clamp(sequencePos, 0, sequence.Length - 1);
        }

        ApplySequenceFrame();
    }

    // 現在のsequence位置のフレームを適用
    private void ApplySequenceFrame() {
        if (sequence == null || sequence.Length == 0) return;
        if (currentFrames == null || currentFrames.Length == 0) return;

        int idx = sequence[sequencePos];
        currentIndex = idx;
        ClampIndex();
        ApplyCurrent();
    }

    private void ApplyCurrent() {
        if (currentFrames == null || currentFrames.Length == 0) {
            ApplySprite(null);
            return;
        }

        ApplySprite(currentFrames[currentIndex]);
    }

    private void ClampIndex() {
        if (currentFrames == null || currentFrames.Length == 0) {
            currentIndex = 0;
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, currentFrames.Length - 1);
    }

    private void ApplySprite(Sprite s) {
        img.sprite = s;
        if (setNativeSizeOnChange && s != null) img.SetNativeSize();
    }
}
