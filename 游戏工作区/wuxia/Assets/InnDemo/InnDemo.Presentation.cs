using UnityEngine;
using System.Collections.Generic;

public partial class InnDemo
{
    int selectedTruth=1;
    bool journalOverview;
    ScaleJournalPrototype gameJournal;

    void UpdateGameJournal()
    {
        bool visible = journal && !won && !journalOverview;
        if (visible && !gameJournal)
        {
            var root = new GameObject("龙鳞装手记");
            root.SetActive(false);
            root.transform.SetParent(transform,false);
            gameJournal = root.AddComponent<ScaleJournalPrototype>();
            gameJournal.Configure(
                id => $"{id:00} · {TruthTitle(id)}\n{TruthStatus(id)}\n\n{ClueDetails(id)}",
                id => story.Confirmed(id),
                () => journalOverview = true);
            root.SetActive(true);
        }
        if (gameJournal) gameJournal.SetVisible(visible);
    }
    readonly Texture2D[] clueImages=new Texture2D[8];
    readonly bool[] clueImageLoaded=new bool[8];
    readonly Texture2D[] clueIcons=new Texture2D[8];
    static string ClueTopic(int id) => Copy($"journal_updategamejournal_{id:00}");
    Color PanelColor=>new(.075f,.07f,.065f,.97f);
    void Panel(Rect rect){var old=GUI.color;GUI.color=PanelColor;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;GUI.Box(rect,"");}
    string TruthTitle(int id)=>story.Confirmed(id)?InnNarrativeRules.Current.truths[id-1].label:story.Suspected(id)?ClueTopic(id):Copy("journal_truthtitle_01");
    string TruthStatus(int id)=>story.Confirmed(id)?Copy("journal_truthstatus_01"):story.Suspected(id)?Copy("journal_truthstatus_02"):Copy("journal_truthstatus_03");
    Color TruthColor(int id)=>story.Confirmed(id)?new Color(.94f,.77f,.42f):story.Suspected(id)?new Color(.73f,.72f,.62f):new Color(.44f,.48f,.5f);
    void Thumbnail(Rect rect,int id){
        if(!clueImageLoaded[id-1]){clueImages[id-1]=Resources.Load<Texture2D>("InnUI/Clue"+id.ToString("00"));clueIcons[id-1]=Resources.Load<Texture2D>("InnUI/ClueIcon"+id.ToString("00"));clueImageLoaded[id-1]=true;}
        var color=GUI.color;GUI.color=TruthColor(id);GUI.Box(rect,"");GUI.color=color;
        if(story.Confirmed(id)&&clueImages[id-1]!=null)GUI.DrawTexture(rect,clueImages[id-1],ScaleMode.ScaleToFit);
        else GUI.Label(new Rect(rect.x+5,rect.y+5,rect.width-10,rect.height-10),story.Confirmed(id)?Copy("journal_thumbnail_01"):story.Suspected(id)?Copy("journal_thumbnail_02"):Copy("journal_thumbnail_03"));
    }
    void Badge(Rect rect,int id){
        var old=GUI.color;GUI.color=TruthColor(id);GUI.Box(rect,"");GUI.color=old;
        if(story.Confirmed(id)&&clueIcons[id-1]!=null)GUI.DrawTexture(rect,clueIcons[id-1],ScaleMode.ScaleToFit);
        else GUI.Label(rect,story.Confirmed(id)?"✓":story.Suspected(id)?"·":Copy("journal_badge_01"));
    }
    void DrawPresentation(){
        if(player==null)return;
        float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);
        float w=Screen.width/scale,h=Screen.height/scale;
        GUI.skin.label.fontSize=20;GUI.skin.box.fontSize=20;GUI.skin.button.fontSize=20;GUI.skin.label.wordWrap=true;GUI.skin.box.wordWrap=true;
        if(won){DrawClueEnding(w,h);return;}
        if(journal){if(journalOverview)DrawJournal(w,h);return;}
        if(dialogTitle==null) DrawDiscovery(scale,w);
    }
    void DrawJournal(float w,float h){
        float x=w/2-580;Panel(new Rect(x,35,1160,650));
        GUI.Label(new Rect(x+24,48,830,35),Copy("journal_drawjournal_01")+story.TruthCount+" / 8");
        if(GUI.Button(new Rect(x+24,94,180,36),Copy("journal_drawjournal_02")))journalOverview=false;
        if(GUI.Button(new Rect(x+215,94,180,36),Copy("journal_drawjournal_03")))journalOverview=true;
        if(GUI.Button(new Rect(x+940,49,190,35),Copy("journal_drawjournal_04"))){journal=false;SetCursor(false);}
        if(journalOverview){DrawOrganizedNotes(x);return;}
        journalScroll=GUI.BeginScrollView(new Rect(x+20,145,690,510),journalScroll,new Rect(0,0,660,568));
        for(int id=1;id<=8;id++){
            int i=id-1;Rect card=new(i%2*330,i/2*142,320,132);
            var old=GUI.color;GUI.color=selectedTruth==id?new Color(1,.86f,.55f):Color.white;bool clicked=GUI.Button(card,"");GUI.color=old;if(clicked)selectedTruth=id;
            Thumbnail(new Rect(card.x+10,card.y+12,76,76),id);Badge(new Rect(card.x+62,card.y+8,30,30),id);
            GUI.Label(new Rect(card.x+98,card.y+10,210,70),TruthTitle(id));
            GUI.contentColor=TruthColor(id);GUI.Label(new Rect(card.x+98,card.y+88,205,30),TruthStatus(id));GUI.contentColor=Color.white;
        }GUI.EndScrollView();
        Panel(new Rect(x+730,145,400,510));Thumbnail(new Rect(x+850,163,150,112),selectedTruth);
        GUI.Label(new Rect(x+746,286,365,66),TruthTitle(selectedTruth));
        GUI.contentColor=TruthColor(selectedTruth);GUI.Label(new Rect(x+746,353,365,30),TruthStatus(selectedTruth));GUI.contentColor=Color.white;
        string detail=ClueDetails(selectedTruth);GUI.Label(new Rect(x+746,393,365,245),detail);
    }
    string ClueDetails(int id){
        if(!story.Confirmed(id)&&!story.Suspected(id))return Copy("journal_cluedetails_01");
        var rule=InnNarrativeRules.Current.truths[id-1];var lines=new List<string>();var seen=new HashSet<string>();
        foreach(var keys in new[]{rule.all,rule.any})if(keys!=null)foreach(var key in keys)if(story.Has(key)&&seen.Add(key)){
            foreach(var fact in InnNarrativeRules.Current.factCatalog)if(fact.id==key){string kind=fact.kind=="testimony"?Copy("journal_cluedetails_02"):fact.kind=="document"?Copy("journal_cluedetails_03"):Copy("journal_cluedetails_04");lines.Add(kind+" · "+fact.label);break;}
        }
        return string.Join("\n",lines)+(story.Confirmed(id)?Copy("journal_cluedetails_05"):Copy("journal_cluedetails_06"));
    }
    void DrawClueEnding(float w,float h){
        float x=w/2-570;Panel(new Rect(x,30,1140,660));
        var heading=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=48,fontStyle=FontStyle.Bold};
        var caption=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=22};
        GUI.Label(new Rect(x+24,43,1092,38),Copy("journal_drawclueending_01"),caption);
        GUI.Label(new Rect(x+24,86,1092,84),DepartureTitle(),heading);
        GUI.Label(new Rect(x+24,170,1092,35),Copy("journal_drawclueending_02")+SwordTitle(),caption);
        string ending=Ending();int firstLine=ending.IndexOf('\n');if(firstLine>=0)ending=ending.Substring(firstLine+1);
        ending=ending.Replace("\n\n","\n");var bodyStyle=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true};GUI.Label(new Rect(x+28,215,1080,128),ending,bodyStyle);
        GUI.Label(new Rect(x+28,350,1080,32),Copy("journal_drawclueending_03")+story.TruthCount+" / 8");
        for(int id=1;id<=8;id++){
            float bx=x+25+(id-1)*137;Rect slot=new(bx,390,128,124);
            var old=GUI.color;GUI.color=TruthColor(id);if(GUI.Button(slot,""))selectedTruth=id;GUI.color=old;
            Thumbnail(new Rect(bx+26,401,76,63),id);Badge(new Rect(bx+91,393,28,28),id);GUI.Label(new Rect(bx+8,470,112,38),TruthStatus(id));
        }
        GUI.Label(new Rect(x+28,530,1080,38),TruthTitle(selectedTruth));
        GUI.Label(new Rect(x+28,575,1080,58),story.Confirmed(selectedTruth)?ClueTopic(selectedTruth)+Copy("journal_drawclueending_04"):story.Suspected(selectedTruth)?Copy("journal_drawclueending_05"):Copy("journal_drawclueending_06"));
        GUI.Label(new Rect(x+28,644,1080,32),Copy("journal_drawclueending_07"));
    }
}
