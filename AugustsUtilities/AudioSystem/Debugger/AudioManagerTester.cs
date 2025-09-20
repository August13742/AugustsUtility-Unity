using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AugustsUtility.AudioSystem;
public sealed class AudioManagerTester : MonoBehaviour
{
    [Header("Assign test assets (Resources)")]
    public List<SFXResource> SFXResources = new();
    public MusicResource musicResource;
    public MusicPlaylist playlist;

    [Header("Options")]
    public string loopEventName = "debug_loop";
    public bool spatialSFXAtMouse = false;

    [Header("UI (Optional)")]
    public Canvas targetCanvas;

    private bool loopPlaying;
    private Text logText;

    #region Event Handlers
    private void OnEnable()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnSFXFinished += HandleSFXFinished;
            //AudioManager.OnMusicStartConfirmed += HandleMusicStartConfirmed;
            AudioManager.Instance.OnPlaylistTrackChanged += HandlePlaylistTrackChanged;
        }
    }

    private void OnDisable()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnSFXFinished -= HandleSFXFinished;
            //AudioManager.OnMusicStartConfirmed -= HandleMusicStartConfirmed;
            AudioManager.Instance.OnPlaylistTrackChanged -= HandlePlaylistTrackChanged;
        }
    }

    private void HandleSFXFinished(string evt, int id) => Log($"OnSFXFinished: {evt}, id={id}");
    private void HandleMusicStartConfirmed(double dsp) => Log($"OnMusicStartConfirmed @ {dsp:F6}");
    private void HandlePlaylistTrackChanged(MusicPlaylist pl, int idx, MusicResource res) =>
        Log($"Playlist: '{pl.name}' idx={idx} -> {res?.clip?.name ?? res?.name}");
    #endregion

    private void Start()
    {
        if (targetCanvas == null)
            targetCanvas = FindFirstObjectByType<Canvas>() ?? CreateCanvas();

        var scaler = targetCanvas.GetComponent<CanvasScaler>() ?? targetCanvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        BuildUI();
    }


    private void Update()
    {
        if (AudioManager.Instance == null)
            return;

        for (int i = 0; i < SFXResources.Count; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                PlaySFXIndex(i);

        if (Input.GetKeyDown(KeyCode.M))
            ToggleMusic();
        if (Input.GetKeyDown(KeyCode.S))
            StopMusic();
        if (Input.GetKeyDown(KeyCode.L))
            ToggleLoop();
        if (Input.GetKeyDown(KeyCode.P))
            ToggleGlobalPause();
        if (Input.GetKeyDown(KeyCode.O))
        {
            AudioManager.Instance.PauseSFX();
            Log("Paused SFX");
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            AudioManager.Instance.ResumeSFX();
            Log("Resumed SFX");
        }

        if (spatialSFXAtMouse && Input.GetMouseButtonDown(0))
            PlaySFXAtMouse();
    }

    #region Actions
    private void PlaySFXIndex(int i)
    {
        if (i < 0 || i >= SFXResources.Count || SFXResources[i] == null)
        {
            Log("No SFX at index");
            return;
        }
        int id = AudioManager.Instance.PlaySFX(SFXResources[i]);
        Log($"Play SFX[{i}] id={(id >= 0 ? id.ToString() : "loop/n/a")}");
    }

    private void PlaySFXAtMouse()
    {
        if (SFXResources.Count == 0 || SFXResources[0] == null)
            return;
        var ray = Camera.main ? Camera.main.ScreenPointToRay(Input.mousePosition) : default;
        if (Camera.main && Physics.Raycast(ray, out var hit, 500f))
        {
            AudioManager.Instance.PlaySFXAtPosition(SFXResources[0], hit.point);
            Log($"SFX at {hit.point}");
        }
        else
        {
            AudioManager.Instance.PlaySFX(SFXResources[0]);
            Log("SFX 2D (no hit)");
        }
    }

    private void ToggleMusic()
    {
        if (AudioManager.Instance.IsMusicPlaying)
        {
            StopMusic();
        }
        else
        {
            if (musicResource == null)
            {
                Log("No MusicResource");
                return;
            }
            AudioManager.Instance.PlayMusicImmediate(musicResource);
            Log($"Music Play '{musicResource.clip.name}'");
        }
    }

    private void StopMusic()
    {
        AudioManager.Instance?.StopMusic();
        Log("Music Stop");
    }

    private void ToggleLoop()
    {
        var srcRes = SFXResources.Count > 0 ? SFXResources[0] : null;
        if (srcRes == null)
        {
            Log("No SFX resource for loop");
            return;
        }

        if (!loopPlaying)
        {
            // FIX: Replaced non-functional 'with' expression.
            // Create a temporary resource instance to override properties for this one call.
            var loopRes = ScriptableObject.CreateInstance<SFXResource>();
            loopRes.clip = srcRes.clip;
            loopRes.volume = srcRes.volume;
            loopRes.pitch = srcRes.pitch;
            loopRes.loop = true; // Override
            loopRes.eventName = loopEventName; // Override

            AudioManager.Instance.PlaySFX(loopRes);
            loopPlaying = true;
            Log($"Loop START '{loopEventName}'");
        }
        else
        {
            AudioManager.Instance.StopLoopedSFX(loopEventName, 0.4f);
            loopPlaying = false;
            Log($"Loop STOP '{loopEventName}'");
        }
    }

    private void ToggleGlobalPause()
    {
        bool newPausedState = !AudioListener.pause;
        AudioListener.pause = newPausedState;
        // Note: For a more robust system, manage pause state internally
        // instead of relying on AudioListener.pause, which can be affected by other scripts.
        Log($"Global Pause Toggled to: {newPausedState}");
    }
    #endregion

    #region UI Generation
    private void BuildUI()
    {
        var panelGO = new GameObject("AudioTester_Panel");
        panelGO.transform.SetParent(targetCanvas.transform, false);
        var panel = panelGO.AddComponent<RectTransform>();

        // Center the panel on the canvas
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(520f, 700f); // width x height in pixels
        panel.anchoredPosition = Vector2.zero;

        panelGO.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        var layout = panelGO.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 8f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        AddLabel(panel, "AudioManager Tester", 16, FontStyle.Bold);
        AddSliderRow(panel, "Master", 1f, v => AudioManager.Instance.SetMasterVolume(v));
        AddSliderRow(panel, "Music Bus", 1f, v => AudioManager.Instance.SetMusicBusVolume(v));
        AddSliderRow(panel, "SFX Bus", 1f, v => AudioManager.Instance.SetSFXVolume(v));

        AddLabel(panel, "Music Control");
        AddButton(panel, "Toggle Music (M)", ToggleMusic);
        AddButton(panel, "Music: Pause/Resume", () => {
            if (AudioManager.Instance.IsMusicPaused)
            {
                AudioManager.Instance.ResumeMusic();
                Log("Music Resume");
            }
            else
            {
                AudioManager.Instance.PauseMusic();
                Log("Music Pause");
            }
        });

        AddLabel(panel, "Playlist Control");
        AddButton(panel, "Playlist: Play", () => AudioManager.Instance.PlayPlaylist(playlist));
        AddButton(panel, "Playlist: Next", () => AudioManager.Instance.NextPlaylistTrack());

        AddLabel(panel, "SFX Control");
        AddButton(panel, "Toggle Loop (L)", ToggleLoop);
        for (int i = 0; i < SFXResources.Count; i++)
        {
            int idx = i;
            string nm = SFXResources[i]?.clip?.name ?? "null";
            AddButton(panel, $"Play SFX {i + 1} ({nm})", () => PlaySFXIndex(idx));
        }

        AddLabel(panel, "Global Control");
        AddButton(panel, "Toggle Global Pause (P)", ToggleGlobalPause);

        logText = AddLabel(panel, "Event Log", 12, FontStyle.Italic);
        logText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 150);
        logText.verticalOverflow = VerticalWrapMode.Truncate;
        Log("Ready.");

        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
    }

    private void Log(string msg)
    {
        if (!logText)
            return;
        string time = Time.unscaledTime.ToString("F2");
        logText.text = $"[{time}] {msg}\n" + logText.text;
        if (logText.text.Length > 1000)
            logText.text = logText.text.Substring(0, 1000);
    }

    // UI Helpers (simplified for brevity)
    private Canvas CreateCanvas()
    {
        var go = new GameObject("AudioTester_Canvas");
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay; // change to Camera if you insist
        go.AddComponent<GraphicRaycaster>();

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return c;
    }


    private Text AddLabel(RectTransform parent, string text, int size = 14, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);

        var t = go.AddComponent<Text>();
        t.text = text;
        // Use LegacyRuntime.ttf (Arial.ttf is no longer a valid built-in font)
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size;
        t.fontStyle = style;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleLeft;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = size + 10;  // ~line height
        le.flexibleWidth = 1;

        return t;
    }
    private Button AddButton(RectTransform parent, string label, Action onClick)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 32;
        le.flexibleWidth = 1;

        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(() => onClick?.Invoke());

        var txt = AddLabel(go.GetComponent<RectTransform>(), label, 14);
        txt.alignment = TextAnchor.MiddleCenter;
        var rt = txt.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        return btn;
    }

    private void AddSliderRow(RectTransform parent, string label, float startValue, Action<float> onChanged)
    {
        AddLabel(parent, label);

        var go = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 22;
        le.flexibleWidth = 1;

        var slider = go.GetComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = startValue;
        slider.onValueChanged.AddListener(v => onChanged?.Invoke(v));
    }
    #endregion
}
