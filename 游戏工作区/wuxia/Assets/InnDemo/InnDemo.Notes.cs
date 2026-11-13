using UnityEngine;
using System.Collections.Generic;

public partial class InnDemo
{
    Vector2 observationScroll,testimonyScroll;
    string DepartureTitle()=>story.pact==1?Copy("journal_departuretitle_01"):story.pact==2?Copy("journal_departuretitle_02"):story.pact==3?Copy("journal_departuretitle_03"):Copy("journal_departuretitle_04");
    string SwordTitle()=>story.swordOutcome==1?Copy("journal_swordtitle_01"):story.swordOutcome==2?Copy("journal_swordtitle_02"):story.swordOutcome==3?Copy("journal_swordtitle_03"):Copy("journal_swordtitle_04");
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
        GUI.Label(new Rect(x+40,157,280,35),Copy("journal_draworganizednotes_01"),subhead);
        GUI.Label(new Rect(x+40,207,280,65),story.awake?Copy("journal_draworganizednotes_02"):Copy("journal_draworganizednotes_03")+(story.Has("wound")?Copy("journal_draworganizednotes_04"):Copy("journal_draworganizednotes_05")));
        GUI.Label(new Rect(x+40,280,280,65),story.antidote?(story.awake?Copy("journal_draworganizednotes_06"):Copy("journal_draworganizednotes_07")):Copy("journal_draworganizednotes_08"));
        float y=358;
        if(story.Has("orderFound")){GUI.Label(new Rect(x+40,y,280,64),Copy("journal_draworganizednotes_09")+(story.orderReturned?Copy("journal_draworganizednotes_10"):story.orderOwned?Copy("journal_draworganizednotes_11"):Copy("journal_draworganizednotes_12")));y+=73;}
        if(story.sword){GUI.Label(new Rect(x+40,y,280,58),Copy("journal_draworganizednotes_13")+SwordTitle()+"）");y+=67;}
        if(story.pact>0)GUI.Label(new Rect(x+40,y,280,72),Copy("journal_draworganizednotes_14")+DepartureTitle());
        else if(story.awake)GUI.Label(new Rect(x+40,y,280,72),Copy("journal_draworganizednotes_15"));
        DrawNoteSection(new Rect(x+360,145,770,245),Copy("journal_draworganizednotes_16"),NoteRows(false),ref observationScroll);
        DrawNoteSection(new Rect(x+360,410,770,245),Copy("journal_draworganizednotes_17"),NoteRows(true),ref testimonyScroll);
    }
    void DrawNoteSection(Rect rect,string title,List<InnFactDefinition> rows,ref Vector2 scroll){
        Panel(rect);GUI.Label(new Rect(rect.x+16,rect.y+10,rect.width-30,32),title+" · "+rows.Count);
        if(rows.Count==0){GUI.contentColor=new Color(.65f,.68f,.68f);GUI.Label(new Rect(rect.x+22,rect.y+65,rect.width-44,95),Copy("journal_drawnotesection_01"));GUI.contentColor=Color.white;return;}
        scroll=GUI.BeginScrollView(new Rect(rect.x+14,rect.y+52,rect.width-28,rect.height-64),scroll,new Rect(0,0,rect.width-55,rows.Count*64));
        for(int i=0;i<rows.Count;i++){
            var fact=rows[i];string type=fact.kind=="testimony"?Copy("journal_drawnotesection_02"):fact.kind=="document"?Copy("journal_drawnotesection_03"):fact.kind=="evidence"?Copy("journal_drawnotesection_04"):Copy("journal_drawnotesection_05");
            GUI.contentColor=new Color(.78f,.7f,.51f);GUI.Label(new Rect(8,i*64+8,78,35),type);GUI.contentColor=Color.white;
            GUI.Label(new Rect(96,i*64+8,rect.width-175,51),fact.label);
        }GUI.EndScrollView();
    }
}
