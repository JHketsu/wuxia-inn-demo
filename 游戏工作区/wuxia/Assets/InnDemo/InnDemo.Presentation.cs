using UnityEngine;
using System.Collections.Generic;

public partial class InnDemo
{
    int selectedTruth=1;
    bool journalOverview;
    readonly Texture2D[] clueImages=new Texture2D[8];
    readonly bool[] clueImageLoaded=new bool[8];
    readonly Texture2D[] clueIcons=new Texture2D[8];
    readonly string[] clueTopics={"她昨夜的行动","外貌改变的缘故","昏睡的原因","桌边血迹的来处","老板手上的伤","纸页的来历","她不愿说的事","旧账上的往来"};
    Color PanelColor=>new(.075f,.07f,.065f,.97f);
    void Panel(Rect rect){var old=GUI.color;GUI.color=PanelColor;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;GUI.Box(rect,"");}
    string TruthTitle(int id)=>story.Confirmed(id)?InnNarrativeRules.Current.truths[id-1].label:story.Suspected(id)?clueTopics[id-1]:"尚未发现的线索";
    string TruthStatus(int id)=>story.Confirmed(id)?"已确认":story.Suspected(id)?"待核对":"未发现";
    Color TruthColor(int id)=>story.Confirmed(id)?new Color(.94f,.77f,.42f):story.Suspected(id)?new Color(.73f,.72f,.62f):new Color(.44f,.48f,.5f);
    void Thumbnail(Rect rect,int id){
        if(!clueImageLoaded[id-1]){clueImages[id-1]=Resources.Load<Texture2D>("InnUI/Clue"+id.ToString("00"));clueIcons[id-1]=Resources.Load<Texture2D>("InnUI/ClueIcon"+id.ToString("00"));clueImageLoaded[id-1]=true;}
        var color=GUI.color;GUI.color=TruthColor(id);GUI.Box(rect,"");GUI.color=color;
        if(story.Confirmed(id)&&clueImages[id-1]!=null)GUI.DrawTexture(rect,clueImages[id-1],ScaleMode.ScaleToFit);
        else GUI.Label(new Rect(rect.x+5,rect.y+5,rect.width-10,rect.height-10),story.Confirmed(id)?"图像\n占位":story.Suspected(id)?"线索\n待核":"？");
    }
    void Badge(Rect rect,int id){
        var old=GUI.color;GUI.color=TruthColor(id);GUI.Box(rect,"");GUI.color=old;
        if(story.Confirmed(id)&&clueIcons[id-1]!=null)GUI.DrawTexture(rect,clueIcons[id-1],ScaleMode.ScaleToFit);
        else GUI.Label(rect,story.Confirmed(id)?"✓":story.Suspected(id)?"·":"？");
    }
    void DrawPresentation(){
        if(player==null)return;
        float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);
        float w=Screen.width/scale,h=Screen.height/scale;
        GUI.skin.label.fontSize=20;GUI.skin.box.fontSize=20;GUI.skin.button.fontSize=20;GUI.skin.label.wordWrap=true;GUI.skin.box.wordWrap=true;
        if(won){DrawClueEnding(w,h);return;}
        if(journal){DrawJournal(w,h);return;}
        if(dialogTitle!=null){DrawDialogue(w,h);return;}
        if(showObjectives){Panel(new Rect(20,20,450,150));GUI.Label(new Rect(38,28,415,130),"当前目的 · Tab 收起\n\n"+CurrentObjective());}
        else GUI.Label(new Rect(20,20,430,32),"荒夜客栈 · Tab 显示任务");
        GUI.Label(new Rect(20,175,w-40,35),"WASD 移动 · 鼠标/方向键镜头 · E 交互/关闭 · Q 凝神观察 · J 手记 · Tab 任务 · Esc 关闭");
        DrawDiscovery(scale,w,h);
        if(Time.time<messageUntil){Panel(new Rect(20,h-115,w-40,95));GUI.Label(new Rect(40,h-105,w-80,80),message);}
    }
    void DrawDialogue(float w,float h){
        float x=w/2-520;Panel(new Rect(x,220,1040,480));
        string channel=narrativeMode==NarrativeMode.Dialogue?"交谈":narrativeMode==NarrativeMode.Observation?"所见":narrativeMode==NarrativeMode.Document?"读到的文字":"行动";
        GUI.Label(new Rect(x+22,230,995,35),channel+" · "+dialogTitle);
        GUI.Label(new Rect(x+22,273,995,innerThought==null?145:95),dialogText);
        if(innerThought!=null){GUI.contentColor=new Color(.85f,.8f,.65f);GUI.Label(new Rect(x+22,370,995,48),"女主 · 心想："+innerThought);GUI.contentColor=Color.white;}
        var snapshot=options.ToArray();for(int i=0;i<snapshot.Length;i++){
            float bw=snapshot.Length>5?485:995;float bx=x+22+(snapshot.Length>5?i%2*505:0);float by=420+(snapshot.Length>5?i/2:i)*43;
            bool read=snapshot[i].trackRead&&story.ChoiceWasRead(snapshot[i].id);var style=new GUIStyle(GUI.skin.button);
            style.normal.background=null;style.hover.background=null;style.active.background=null;style.focused.background=null;
            var oldColor=GUI.color;GUI.color=new Color(.14f,.15f,.15f);GUI.DrawTexture(new Rect(bx,by,bw,39),Texture2D.whiteTexture);GUI.color=oldColor;
            style.normal.textColor=read?new Color(.7f,.81f,.84f):new Color(.96f,.89f,.71f);
            style.hover.textColor=read?new Color(.8f,.85f,.86f):Color.white;
            if(GUI.Button(new Rect(bx,by,bw,39),(read?"[已读] ":"")+snapshot[i].text,style))snapshot[i].action();
        }
        GUI.Label(new Rect(x+22,654,650,32),"灰蓝文字 / [已读]：此前选过，仍可再次交谈");
        if(GUI.Button(new Rect(x+795,653,220,35),"[E / Esc] 关闭"))CloseDialog();
    }
    void DrawJournal(float w,float h){
        float x=w/2-580;Panel(new Rect(x,35,1160,650));
        GUI.Label(new Rect(x+24,48,830,35),"行旅手记 · 客栈之夜    已确认 "+story.TruthCount+" / 8");
        if(GUI.Button(new Rect(x+24,94,180,36),"线索图卡"))journalOverview=false;
        if(GUI.Button(new Rect(x+215,94,180,36),"随手记 / 物件"))journalOverview=true;
        if(GUI.Button(new Rect(x+940,49,190,35),"[J / Esc] 合上")){journal=false;SetCursor(false);}
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
        if(!story.Confirmed(id)&&!story.Suspected(id))return "这页还空着。留心眼前的物件与谈话，新的见闻会记在这里。";
        var rule=InnNarrativeRules.Current.truths[id-1];var lines=new List<string>();var seen=new HashSet<string>();
        foreach(var keys in new[]{rule.all,rule.any})if(keys!=null)foreach(var key in keys)if(story.Has(key)&&seen.Add(key)){
            foreach(var fact in InnNarrativeRules.Current.factCatalog)if(fact.id==key){string kind=fact.kind=="testimony"?"证词":fact.kind=="document"?"文书":"观察 / 物证";lines.Add(kind+" · "+fact.label);break;}
        }
        return string.Join("\n",lines)+(story.Confirmed(id)?"\n\n这些见闻已能相互印证。":"\n\n还留着疑问，先听听别的说法。 ");
    }
    void DrawClueEnding(float w,float h){
        float x=w/2-570;Panel(new Rect(x,30,1140,660));
        var heading=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=48,fontStyle=FontStyle.Bold};
        var caption=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=22};
        GUI.Label(new Rect(x+24,43,1092,38),"客栈之夜 · 离店结局",caption);
        GUI.Label(new Rect(x+24,86,1092,84),DepartureTitle(),heading);
        GUI.Label(new Rect(x+24,170,1092,35),"剑的来由 · "+SwordTitle(),caption);
        string ending=Ending();int tail=ending.IndexOf("\n已确认真相");if(tail>=0)ending=ending.Substring(0,tail);int firstLine=ending.IndexOf('\n');if(firstLine>=0)ending=ending.Substring(firstLine+1);
        ending=ending.Replace("\n\n","\n");var bodyStyle=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true};GUI.Label(new Rect(x+28,215,1080,128),ending,bodyStyle);
        GUI.Label(new Rect(x+28,350,1080,32),"此夜留下的往事 · 已确认 "+story.TruthCount+" / 8");
        for(int id=1;id<=8;id++){
            float bx=x+25+(id-1)*137;Rect slot=new(bx,390,128,124);
            var old=GUI.color;GUI.color=TruthColor(id);if(GUI.Button(slot,""))selectedTruth=id;GUI.color=old;
            Thumbnail(new Rect(bx+26,401,76,63),id);Badge(new Rect(bx+91,393,28,28),id);GUI.Label(new Rect(bx+8,470,112,38),TruthStatus(id));
        }
        GUI.Label(new Rect(x+28,530,1080,38),TruthTitle(selectedTruth));
        GUI.Label(new Rect(x+28,575,1080,58),story.Confirmed(selectedTruth)?clueTopics[selectedTruth-1]+" · 点击徽记查看；插画暂为占位。":story.Suspected(selectedTruth)?"这段往事仍待核对。":"这一页仍然留白。");
        GUI.Label(new Rect(x+28,644,1080,32),"R 重新开始 · F9 读取存档");
    }
}
