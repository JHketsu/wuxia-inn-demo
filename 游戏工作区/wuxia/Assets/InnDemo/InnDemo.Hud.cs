using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public partial class InnDemo
{
    Canvas hud;
    Font hudFont;
    Sprite hudFrame, hudPlate, heroPortrait, companionPortrait;
    RectTransform hudTask, hudMenu, hudDialogue, hudContent, hudViewport;
    Text hudGoal, hudHint, hudName;
    Image hudArrow, continueArrow;
    Image hudPortrait;
    ScrollRect hudScroll, choiceScroll;
    RectTransform choicePanel, choiceContent, choiceViewport;
    bool hudMenuOpen, hudCursorWasLocked;
    string hudDialogueKey;

    static RectTransform HudRect(Transform parent, string name, Vector2 anchor, Vector2 pivot,
        Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    static Sprite HudSprite(string name, Vector4 border = default)
    {
        var texture = Resources.Load<Texture2D>("InnUI/Hud/" + name);
        return texture ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(.5f,.5f), 100, 0, SpriteMeshType.FullRect, border) : null;
    }

    static Image HudImage(RectTransform rect, Sprite sprite, bool sliced = false)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = sliced && sprite ? Image.Type.Sliced : Image.Type.Simple;
        image.raycastTarget = false;
        if (!sprite) image.color = new Color(.06f,.09f,.13f,.98f);
        return image;
    }

    Text HudText(RectTransform rect, string value, int size, TextAnchor alignment = TextAnchor.UpperLeft)
    {
        var label = rect.gameObject.AddComponent<Text>();
        label.font = hudFont;
        label.fontSize = size;
        label.color = new Color(.95f,.92f,.84f);
        label.alignment = alignment;
        label.text = value;
        label.raycastTarget = false;
        return label;
    }

    void HudButton(RectTransform rect, Action action)
    {
        var image = HudImage(rect, hudPlate, true);
        image.raycastTarget = true;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => action());
    }

    void ToggleHudMenu()
    {
        if (!hudMenuOpen) hudCursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
        hudMenuOpen = !hudMenuOpen;
        SetCursor(hudMenuOpen || !hudCursorWasLocked || dialogTitle != null);
    }

    void BuildHud()
    {
        hudFont = Font.CreateDynamicFontFromOSFont(new[] { "PingFang SC", "Microsoft YaHei", "Noto Sans CJK SC", "Arial Unicode MS", "Heiti SC" }, 20);
        if (!hudFont) hudFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hudFrame = HudSprite("Frame", new Vector4(70,65,70,65));
        hudPlate = HudSprite("Plate", new Vector4(130,135,130,135));
        heroPortrait = HudSprite("Heroine");
        companionPortrait = HudSprite("Companion");
        var root = new GameObject("荒夜客栈 HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        hud = root.GetComponent<Canvas>();
        hud.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        if (!EventSystem.current)
        {
            var events = new GameObject("HUD Events", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform, false);
        }

        Vector2 topLeft = new(0,1), topRight = Vector2.one, bottom = new(.5f,0);
        RectTransform Top(Transform parent, string name, float x, float y, float w, float h)
            => HudRect(parent,name,topLeft,topLeft,new Vector2(x,-y),new Vector2(w,h));
        hudTask = Top(root.transform,"当前所寻",24,36,350,125);
        HudImage(hudTask,hudFrame,true).pixelsPerUnitMultiplier = 4;
        var taskPlate = Top(hudTask,"任务牌",14,-15,170,32);
        HudImage(taskPlate,hudPlate,true).pixelsPerUnitMultiplier = 9;
        HudText(Top(taskPlate,"任务标题",0,0,170,32),Copy("interface_buildhud_01"),16,TextAnchor.MiddleCenter);
        hudGoal = HudText(Top(hudTask,"目标",18,27,314,27),"",20);
        hudHint = HudText(Top(hudTask,"下一步",18,58,314,58),"",17);

        var menuButton = HudRect(root.transform,"菜单入口",topRight,topRight,new Vector2(-24,-22),new Vector2(145,45));
        HudButton(menuButton,ToggleHudMenu);
        menuButton.GetComponent<Image>().pixelsPerUnitMultiplier = 8;
        HudImage(Top(menuButton,"双卷轴",8,4,36,36),HudSprite("Scroll")).preserveAspect = true;
        HudText(Top(menuButton,"菜单文字",48,0,62,45),Copy("interface_buildhud_02"),20,TextAnchor.MiddleCenter);
        var triangle = HudSprite("Triangle");
        hudArrow = HudImage(Top(menuButton,"展开箭头",115,14,17,17),triangle);
        hudArrow.preserveAspect = true;
        hudMenu = HudRect(root.transform,"快捷键",topRight,topRight,new Vector2(-24,-73),new Vector2(304,278));
        HudImage(hudMenu,hudFrame,true).pixelsPerUnitMultiplier = 4;
        HudText(Top(hudMenu,"快捷键说明",18,15,270,250),
            Copy("interface_buildhud_03"),17).lineSpacing = 1.4f;

        hudDialogue = HudRect(root.transform,"对话框",bottom,bottom,new Vector2(0,22),new Vector2(1144,210));
        HudImage(hudDialogue,hudFrame,true).pixelsPerUnitMultiplier = 3;
        HudImage(HudRect(hudDialogue,"角纹",Vector2.zero,Vector2.zero,new Vector2(12,12),new Vector2(230,77)),HudSprite("Ink")).color = new Color(1,1,1,.32f);
        var plate = Top(hudDialogue,"姓名牌",34,-16,220,38);
        HudImage(plate,hudPlate,true).pixelsPerUnitMultiplier = 8;
        hudName = HudText(Top(plate,"姓名",8,0,204,38),"",19,TextAnchor.MiddleCenter);
        hudPortrait = HudImage(HudRect(hudDialogue,"胸像",Vector2.right,Vector2.right,new Vector2(-20,12),new Vector2(280,184)),null);
        hudPortrait.preserveAspect = true;

        hudViewport = Top(hudDialogue,"正文视口",34,33,780,135);
        var hit = HudImage(hudViewport,null);
        hit.color = Color.clear;
        hit.raycastTarget = true;
        hudViewport.gameObject.AddComponent<RectMask2D>();
        hudScroll = hudDialogue.gameObject.AddComponent<ScrollRect>();
        hudScroll.viewport = hudViewport;
        hudContent = Top(hudViewport,"正文和选项",0,0,780,135);
        hudScroll.content = hudContent;
        hudScroll.horizontal = false;
        hudScroll.movementType = ScrollRect.MovementType.Clamped;
        var close = HudRect(hudDialogue,"关闭",Vector2.zero,Vector2.zero,new Vector2(648,12),new Vector2(166,31));
        HudButton(close,CloseDialog);
        close.GetComponent<Image>().pixelsPerUnitMultiplier = 9;
        HudText(Top(close,"关闭提示",0,0,166,31),Copy("interface_buildhud_04"),16,TextAnchor.MiddleCenter);
        continueArrow = HudImage(HudRect(hudDialogue,"继续交谈",Vector2.zero,new Vector2(.5f,.5f),
            new Vector2(826,27),new Vector2(22,22)),triangle);
        continueArrow.preserveAspect = true;
        continueArrow.raycastTarget = true;
        var next = continueArrow.gameObject.AddComponent<Button>();
        next.targetGraphic = continueArrow;
        next.onClick.AddListener(() =>
        {
            if (CanContinueHudDialogue) options[0].action();
        });
        choicePanel = HudRect(root.transform,"对话选项",new Vector2(.5f,.5f),new Vector2(.5f,.5f),
            new Vector2(0,25),new Vector2(660,260));
        choiceViewport = Top(choicePanel,"选项视口",20,0,620,205);
        var choiceHit = HudImage(choiceViewport,null);
        choiceHit.color = Color.clear;
        choiceHit.raycastTarget = true;
        choiceViewport.gameObject.AddComponent<RectMask2D>();
        choiceContent = Top(choiceViewport,"选项列表",0,0,620,205);
        choiceScroll = choicePanel.gameObject.AddComponent<ScrollRect>();
        choiceScroll.viewport = choiceViewport;
        choiceScroll.content = choiceContent;
        choiceScroll.horizontal = false;
        choiceScroll.movementType = ScrollRect.MovementType.Clamped;

    }

    bool CanContinueHudDialogue => dialogTitle != null && options.Count == 1 && options[0].id == "navigation.continue";

    void UpdateHud()
    {
        if (!player) return;
        if (!hud) BuildHud();
        hud.enabled = !won && !journal;
        if (!hud.enabled)
        {
            hudMenuOpen = false;
            return;
        }
        hudMenu.gameObject.SetActive(hudMenuOpen);
        hudArrow.rectTransform.localRotation = Quaternion.Euler(0,0,hudMenuOpen ? 180 : 0);
        hudTask.sizeDelta = new Vector2(350,showObjectives ? 125 : 28);
        hudGoal.gameObject.SetActive(showObjectives);
        hudHint.gameObject.SetActive(showObjectives);
        var objective = CurrentObjective().Split('\n',2);
        hudGoal.text = objective[0];
        hudHint.text = objective.Length > 1 ? objective[1] : "";
        bool dialogue = dialogTitle != null;
        bool thought = !dialogue && Time.time < messageUntil && !string.IsNullOrEmpty(message);
        hudDialogue.gameObject.SetActive(dialogue || thought);
        bool canContinue = CanContinueHudDialogue;
        continueArrow.gameObject.SetActive(canContinue);
        choicePanel.gameObject.SetActive(dialogue && !canContinue && options.Count > 0);
        if (!dialogue && !thought)
        {
            hudDialogueKey = null;
            return;
        }

        string title = dialogue ? dialogTitle : Copy("speaker_hero");
        string bodyText = dialogue ? dialogText : message;
        if (thought && bodyText.StartsWith(Copy("interface_updatehud_01"))) bodyText = bodyText.Substring(Copy("interface_updatehud_02").Length);
        if (dialogue && !string.IsNullOrEmpty(innerThought)) bodyText += Copy("interface_updatehud_03") + innerThought;
        var choices = dialogue && !canContinue ? options.ToArray() : Array.Empty<(string text, Action action, string id, bool trackRead)>();
        string key = title + "\n" + bodyText + "|" + dialogue;
        foreach (var choice in choices) key += "|" + choice.text + story.ChoiceWasRead(choice.id);
        if (key == hudDialogueKey) return;
        hudDialogueKey = key;
        hudName.text = title;
        hudPortrait.sprite = title == Copy("speaker_companion") ? companionPortrait :
            thought || title.StartsWith(Copy("speaker_hero")) ? heroPortrait : null;
        hudPortrait.enabled = hudPortrait.sprite;
        hudPortrait.color = Color.white;
        hudDialogue.Find("关闭").gameObject.SetActive(dialogue);
        foreach (var container in new[] { hudContent, choiceContent })
        foreach (Transform child in container)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        float width = hudPortrait.enabled ? 780 : 1076;
        var bodyRect = HudRect(hudContent,"正文",new Vector2(0,1),new Vector2(0,1),Vector2.zero,new Vector2(width,1));
        var label = HudText(bodyRect,bodyText,22);
        label.lineSpacing = 1.15f;
        float y = Mathf.Ceil(label.preferredHeight) + 10;
        bodyRect.sizeDelta = new Vector2(width,y);
        float choiceY = 0;
        const float choiceWidth = 620;
        foreach (var choice in choices)
        {
            var row = HudRect(choiceContent,"选项",new Vector2(0,1),new Vector2(0,1),new Vector2(0,-choiceY),new Vector2(choiceWidth,34));
            HudButton(row,choice.action);
            row.GetComponent<Image>().pixelsPerUnitMultiplier = 10;
            bool read = choice.trackRead && story.ChoiceWasRead(choice.id);
            var choiceRect = HudRect(row,"选项文字",new Vector2(0,1),new Vector2(0,1),new Vector2(12,-6),new Vector2(choiceWidth-24,24));
            var choiceLabel = HudText(choiceRect,(read ? Copy("interface_updatehud_04") : "") + choice.text,17);
            if (read) choiceLabel.color = new Color(.7f,.81f,.84f);
            float height = Mathf.Max(34,choiceLabel.preferredHeight+12);
            choiceRect.sizeDelta = new Vector2(choiceWidth-24,height-12);
            row.sizeDelta = new Vector2(choiceWidth,height);
            choiceY += height + 5;
        }
        float panelHeight = Mathf.Clamp(y + 75,210,choices.Length > 0 ? 235 : 390);
        float choiceHeight = Mathf.Min(choiceY,250);
        choicePanel.sizeDelta = new Vector2(660,choiceHeight);
        choicePanel.anchoredPosition = new Vector2(0,Mathf.Max(35,22+panelHeight+18+(choiceHeight)/2-360));
        choiceViewport.sizeDelta = new Vector2(choiceWidth,choiceHeight);
        choiceContent.sizeDelta = new Vector2(choiceWidth,choiceY);
        choiceScroll.StopMovement();
        choiceScroll.verticalNormalizedPosition = 1;
        hudDialogue.sizeDelta = new Vector2(1144,panelHeight);
        hudViewport.sizeDelta = new Vector2(width,panelHeight-76);
        hudContent.sizeDelta = new Vector2(width,Mathf.Max(y,hudViewport.rect.height));
        hudScroll.StopMovement();
        hudScroll.verticalNormalizedPosition = 1;
    }
}
