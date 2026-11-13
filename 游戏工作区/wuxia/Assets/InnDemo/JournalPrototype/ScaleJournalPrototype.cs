using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class ScaleJournalPrototype : MonoBehaviour
{
    [SerializeField, Min(.1f)] float secondsPerLeaf = .65f;
    [SerializeField] string[] pageTitles = {
        "昨夜行动", "外貌改变", "昏睡原因", "桌边血迹",
        "老板手伤", "手令来历", "转交顾虑", "旧账往来"
    };

    static readonly Color Ink = new(.13f,.13f,.14f);
    static readonly Color Paper = new(.79f,.78f,.74f);
    Text pageText;
    Text[] tabs;
    RectTransform[] sheets;
    const float LeafStep = 49;
    const float FirstLeafX = -28;
    const float LeafWidth = 364;
    RectTransform[] returnEdges;
    ScaleLeaf[] turningLeaves, turningBacks;
    RawImage clueIllustration;
    Texture2D[] clueTextures;
    Font font;
    int current, target;
    Coroutine turn;

    GameObject screen;
    Func<int,string> readPage;
    Func<int,bool> showIllustration;
    Action openNotes;

    public void Configure(Func<int,string> readPage, Func<int,bool> showIllustration, Action openNotes)
    {
        this.readPage = readPage;
        this.showIllustration = showIllustration;
        this.openNotes = openNotes;
    }

    public void SetVisible(bool visible)
    {
        if (screen.activeSelf == visible) return;
        screen.SetActive(visible);
        if (visible) ShowPage();
    }

    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        font = Font.CreateDynamicFontFromOSFont(new[] { "PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC" }, 22);
        if (!font) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        screen = new GameObject("Journal Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        screen.transform.SetParent(transform, false);
        screen.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = screen.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        if (!EventSystem.current)
            new GameObject("Journal Input", typeof(EventSystem), typeof(InputSystemUIInputModule)).transform.SetParent(transform, false);

        var canvas = screen.transform;
        var backing = Rect(canvas,"背景",0,0,0,0);
        backing.anchorMin = Vector2.zero;
        backing.anchorMax = Vector2.one;
        Fill(backing,Ink);
        var book = Rect(canvas,"手记",0,0,1160,520);
        Art(book,"JournalFrame",new Color(.34f,.34f,.35f));
        Art(Rect(book,"底卷",74,0,1000,495),"ScrollBase",new Color(.48f,.47f,.44f));
        tabs = new Text[pageTitles.Length];
        sheets = new RectTransform[tabs.Length];
        clueTextures = new Texture2D[tabs.Length];
        // Later leaves sit under the earlier ones. The exposed right edge belongs
        // to a full sheet whose glued edge stays fixed on the backing roll.
        for (int i = tabs.Length - 1; i >= 0; i--)
        {
            int index = i;
            var sheet = Rect(book,"错口纸叶 "+(i+1),FirstLeafX+i*LeafStep,0,LeafWidth,480);
            sheets[i] = sheet;
            Art(sheet,$"LeafFront{i+1:00}",Paper);
            clueTextures[i] = Texture($"Clue{i+1:00}");
            var lip = Rect(sheet,"露出的纸边",LeafWidth/2-17,0,34,480);
            var image = Fill(lip,Color.clear);
            image.raycastTarget = true;
            var button = lip.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => Select(index));
            tabs[i] = Label(Rect(lip,"页码",0,195,34,50),(i+1).ToString(),23,Ink);
            tabs[i].alignment = TextAnchor.MiddleCenter;
        }
        pageText = Label(Rect(book,"阅读正文",FirstLeafX,82,300,206),"",18,Ink);
        pageText.resizeTextForBestFit = true;
        pageText.resizeTextMinSize = 14;
        pageText.resizeTextMaxSize = 18;
        clueIllustration = Art(Rect(book,"线索插画",FirstLeafX,-91,256,144),null,Color.clear);
        turningLeaves = new ScaleLeaf[sheets.Length];
        turningBacks = new ScaleLeaf[sheets.Length];
        var backTexture = Texture("LeafBack");
        for (int i = turningLeaves.Length - 1; i >= 0; i--)
        {
            var leaf = Rect(book,"翻动的纸 "+(i+1),FirstLeafX+i*LeafStep,0,LeafWidth,480)
                .gameObject.AddComponent<ScaleLeaf>();
            leaf.raycastTarget = false;
            leaf.gameObject.SetActive(false);
            leaf.PaperTexture = Texture($"LeafFront{i+1:00}");
            turningLeaves[i] = leaf;
            var back = Rect(book,"翻动的纸背 "+(i+1),FirstLeafX+i*LeafStep,0,LeafWidth,480)
                .gameObject.AddComponent<ScaleLeaf>();
            back.raycastTarget = false;
            back.BackFace = true;
            back.PaperTexture = backTexture;
            back.gameObject.SetActive(false);
            turningBacks[i] = back;
        }

        returnEdges = new RectTransform[sheets.Length];
        for (int i = 0; i < returnEdges.Length; i++)
        {
            int index = i;
            float hinge = FirstLeafX+i*LeafStep-LeafWidth/2;
            var edge = Rect(book,"翻回第 "+(i+1)+" 叶",hinge-18,0,36,480);
            var image = Art(edge,"LeafReturnEdge",new Color(.67f,.63f,.55f));
            image.raycastTarget = true;
            var button = edge.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => Select(index));
            Label(Rect(edge,"页码",0,195,36,50),(i+1).ToString(),21,Ink).alignment = TextAnchor.MiddleCenter;
            returnEdges[i] = edge;
        }

        var fixedPage = Rect(book,"固定印记页",-426,0,298,495);
        Art(fixedPage,"FixedPage",new Color(.76f,.72f,.64f));
        Label(Rect(fixedPage,"目录",0,155,170,80),"行旅手记\n龙鳞装",24,Ink);
        var seal = Rect(fixedPage,"印记",0,20,76,76);
        Art(seal,"JournalSeal",new Color(.48f,.18f,.15f));
        Label(Rect(fixedPage,"操作",0,-142,250,102),"点击右侧页边向前翻\n点击左侧页边翻回来\n← / → 或数字 1–8",18,Ink);
        if (openNotes != null)
        {
            var notes = Rect(fixedPage,"随手记入口",0,-70,166,34);
            var image = Fill(notes,new Color(.48f,.18f,.15f));
            image.raycastTarget = true;
            var button = notes.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => openNotes());
            Label(Rect(notes,"文字",0,0,166,34),"随手记 / 物件",17,Color.white).alignment = TextAnchor.MiddleCenter;
            Label(Rect(fixedPage,"合上提示",0,-206,250,26),"J / Esc 合上",16,Ink);
        }
        ShowPage();
    }

    void Update()
    {
        if (!screen.activeSelf) return;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.leftArrowKey.wasPressedThisFrame) Select(target-1);
        if (keyboard.rightArrowKey.wasPressedThisFrame) Select(target+1);
        for (int i = 0; i < Mathf.Min(8,pageTitles.Length); i++)
            if (keyboard[(Key)((int)Key.Digit1+i)].wasPressedThisFrame) Select(i);
    }

    void Select(int page)
    {
        target = Mathf.Clamp(page,0,pageTitles.Length-1);
        if (turn == null && target != current) turn = StartCoroutine(TurnLeaves());
    }

    IEnumerator TurnLeaves()
    {
        while (current != target)
        {
            int destination = target;
            bool forward = destination > current;
            int first = Mathf.Min(current,destination);
            int end = Mathf.Max(current,destination);
            for (int i = first; i < end; i++)
            {
                returnEdges[i].gameObject.SetActive(false);
                turningLeaves[i].SetTurn(forward ? 0 : 1,1);
                turningLeaves[i].gameObject.SetActive(true);
                turningBacks[i].SetTurn(forward ? 0 : 1,1);
                turningBacks[i].gameObject.SetActive(true);
                sheets[i].gameObject.SetActive(false);
            }

            for (float elapsed = 0; elapsed < secondsPerLeaf; elapsed += Time.unscaledDeltaTime)
            {
                float progress = Mathf.SmoothStep(0,1,elapsed/secondsPerLeaf);
                var tint = Color.white;
                float travel = forward ? progress : 1-progress;
                tint.a = 1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.72f,1,travel));
                for (int i = first; i < end; i++)
                {
                    turningLeaves[i].SetTurn(travel,1);
                    turningLeaves[i].color = tint;
                    turningBacks[i].SetTurn(travel,1);
                    turningBacks[i].color = tint;
                }
                pageText.color = new Color(Ink.r,Ink.g,Ink.b,1-progress);
                clueIllustration.color = new Color(1,1,1,clueIllustration.texture ? 1-progress : 0);
                yield return null;
            }

            current = destination;
            for (int i = first; i < end; i++)
            {
                turningLeaves[i].gameObject.SetActive(false);
                turningBacks[i].gameObject.SetActive(false);
            }
            ShowPage();
            pageText.color = Ink;
        }
        turn = null;
    }

    void ShowPage()
    {
        pageText.text = readPage?.Invoke(current+1) ?? $"第 {current+1:00} 叶  ·  {pageTitles[current]}\n\n线索插画预览";
        float x = FirstLeafX+current*LeafStep;
        pageText.rectTransform.anchoredPosition = new Vector2(x,82);
        clueIllustration.rectTransform.anchoredPosition = new Vector2(x,-91);
        clueIllustration.texture = showIllustration == null || showIllustration(current+1) ? clueTextures[current] : null;
        clueIllustration.color = clueIllustration.texture ? Color.white : Color.clear;
        for (int i = 0; i < tabs.Length; i++)
        {
            sheets[i].gameObject.SetActive(i >= current);
            returnEdges[i].gameObject.SetActive(i < current);
        }
    }

    static RectTransform Rect(Transform parent,string name,float x,float y,float width,float height)
    {
        var rect = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent,false);
        rect.anchorMin = rect.anchorMax = new Vector2(.5f,.5f);
        rect.anchoredPosition = new Vector2(x,y);
        rect.sizeDelta = new Vector2(width,height);
        return rect;
    }

    static Texture2D Texture(string name) => name == null ? null :
        Resources.Load<Texture2D>("InnUI/Journal/"+name);

    static RawImage Art(RectTransform rect,string name,Color fallback)
    {
        var image = rect.gameObject.AddComponent<RawImage>();
        image.texture = Texture(name);
        image.color = image.texture ? Color.white : fallback;
        image.raycastTarget = false;
        return image;
    }

    static Image Fill(RectTransform rect,Color color)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    Text Label(RectTransform rect,string text,int size,Color color)
    {
        var label = rect.gameObject.AddComponent<Text>();
        label.font = font;
        label.fontSize = size;
        label.text = text;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }
}
