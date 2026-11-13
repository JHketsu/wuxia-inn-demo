using UnityEngine;

public partial class InnDemo
{
    string CurrentObjective(){
        if(!story.awake){
            if(!story.Has("wound")){if(!story.Has("emptyBed"))return Copy("exploration_currentobjective_01");return Copy("exploration_currentobjective_02");}
            if(!story.Has("poison"))return story.Has("help")?Copy("exploration_currentobjective_03"):story.Has("medicineNote")?Copy("exploration_currentobjective_04"):Copy("exploration_currentobjective_05");
            if(!story.antidote)return Copy("exploration_currentobjective_06");
            if(!story.Has("medicineNote")&&(!story.Has("help")||story.threatened))return Copy("exploration_currentobjective_07");
            return Copy("exploration_currentobjective_08");
        }
        if(!story.sword)return Copy("exploration_currentobjective_09");
        if(story.pact==0)return Copy("exploration_currentobjective_10");
        return !story.opened?Copy("exploration_currentobjective_11"):Copy("exploration_currentobjective_12");
    }
    string ObjectLabel(string label){
        switch(label){
            case "hotspot_luggage_and_cup":return Copy("exploration_objectlabel_01");
            case "hotspot_guest_meal":return Copy("exploration_objectlabel_02");
            case "hotspot_companion":return story.awake?Copy("exploration_objectlabel_03"):Copy("exploration_objectlabel_04");
            case "hotspot_counter_blood":return Copy("exploration_objectlabel_05");
            case "hotspot_ledger":return Copy("exploration_objectlabel_06");
            case "hotspot_front_door":return Copy("exploration_objectlabel_07");
            default:return Copy(label);
        }
    }
    bool MarkerVisible(Transform anchor,float range){
        if(Vector3.Distance(anchor.position,player.transform.position+Vector3.up)>range)return false;
        if(Mathf.Abs(anchor.position.y-player.transform.position.y-1)>1.9f)return false;
        return !Physics.Linecast(player.transform.position+Vector3.up*HeroEyeY,anchor.position);
    }
    void DrawDiscovery(float scale,float width){
        var nearest=Nearest();bool scanning=Time.time<scanUntil;
        foreach(var marker in markers){
            if(!scanning&&(nearest==null||nearest.target!=marker.anchor))continue;
            if(!MarkerVisible(marker.anchor,scanning?6f:2.5f))continue;
            var screen=cam.WorldToScreenPoint(marker.anchor.position);if(screen.z<=0)continue;
            GUI.color=scanning?new Color(1f,.85f,.4f):Color.white;
            GUI.Label(new Rect(screen.x/scale-120,(Screen.height-screen.y)/scale-22,240,44),"◇ "+ObjectLabel(marker.label));
        }
        GUI.color=Color.white;
        if(scanning)GUI.Label(new Rect(width/2-150,210,300,35),Copy("exploration_drawdiscovery_01")+Mathf.CeilToInt(scanUntil-Time.time)+Copy("exploration_drawdiscovery_02"));
    }
    void ReadDocument(string title,string text,params string[] ids){foreach(var id in ids)Fact(id);Show(title,text,NarrativeMode.Document);}
}
