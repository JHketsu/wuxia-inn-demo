using UnityEngine;
using System.Collections.Generic;

public partial class InnDemo
{
    Vector2 observationScroll,testimonyScroll;
    string DepartureTitle()=>story.pact==1?"亲自履约":story.pact==2?"共同决定":story.pact==3?"暂缓交付":"一同离店";
    string SwordTitle()=>story.swordOutcome==1?"托付":story.swordOutcome==2?"追责":story.swordOutcome==3?"夺取":"未定";
    List<InnFactDefinition> NoteRows(bool testimony){
        var rows=new List<InnFactDefinition>();
        foreach(var id in story.facts)foreach(var fact in InnNarrativeRules.Current.factCatalog)if(fact.id==id){
            if(testimony?fact.kind=="testimony":fact.kind=="evidence"||fact.kind=="document"||fact.kind=="observation")rows.Add(fact);break;
        }
        return rows;
    }
    void DrawOrganizedNotes(float x){
        Panel(new Rect(x+24,145,315,510));
        var subhead=new GUIStyle(GUI.skin.label){fontSize=23,fontStyle=FontStyle.Bold};
        GUI.Label(new Rect(x+40,157,280,35),"随身与约定",subhead);
        GUI.Label(new Rect(x+40,207,280,65),story.awake?"同行者 · 已醒，伤已包扎":"同行者 · "+(story.Has("wound")?"尚待救治":"还未找到"));
        GUI.Label(new Rect(x+40,280,280,65),story.antidote?(story.awake?"救治物品 · 药已用下，绷带已换":"救治物品 · 解药与绷带已备好"):"行囊 · 暂无新增物品");
        float y=358;
        if(story.Has("orderFound")){GUI.Label(new Rect(x+40,y,280,64),"手令 · "+(story.orderReturned?"她已收好":story.orderOwned?"由你保管":"已发现"));y+=73;}
        if(story.sword){GUI.Label(new Rect(x+40,y,280,58),"防身剑 · 已取得（"+SwordTitle()+"）");y+=67;}
        if(story.pact>0)GUI.Label(new Rect(x+40,y,280,72),"离店约定\n"+DepartureTitle());
        else if(story.awake)GUI.Label(new Rect(x+40,y,280,72),"离店约定\n还未说定接下来的路");
        DrawNoteSection(new Rect(x+360,145,770,245),"现场与文书",NoteRows(false),ref observationScroll);
        DrawNoteSection(new Rect(x+360,410,770,245),"听过的证词",NoteRows(true),ref testimonyScroll);
    }
    void DrawNoteSection(Rect rect,string title,List<InnFactDefinition> rows,ref Vector2 scroll){
        Panel(rect);GUI.Label(new Rect(rect.x+16,rect.y+10,rect.width-30,32),title+" · "+rows.Count);
        if(rows.Count==0){GUI.contentColor=new Color(.65f,.68f,.68f);GUI.Label(new Rect(rect.x+22,rect.y+65,rect.width-44,95),"这一栏还空着。新的见闻会按类别记在这里。");GUI.contentColor=Color.white;return;}
        scroll=GUI.BeginScrollView(new Rect(rect.x+14,rect.y+52,rect.width-28,rect.height-64),scroll,new Rect(0,0,rect.width-55,rows.Count*64));
        for(int i=0;i<rows.Count;i++){
            var fact=rows[i];string type=fact.kind=="testimony"?"证词":fact.kind=="document"?"文书":fact.kind=="evidence"?"物证":"观察";
            GUI.contentColor=new Color(.78f,.7f,.51f);GUI.Label(new Rect(8,i*64+8,78,35),type);GUI.contentColor=Color.white;
            GUI.Label(new Rect(96,i*64+8,rect.width-175,51),fact.label);
        }GUI.EndScrollView();
    }
}
