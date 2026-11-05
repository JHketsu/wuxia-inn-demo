using UnityEngine;

public partial class InnDemo
{
    string CurrentObjective(){
        if(!story.awake){
            if(!story.Has("wound")){if(!story.Has("emptyBed"))return "叫同行者起床\n今日要继续护送她上路。先去旁边床位叫她。";return "寻找同行者\n旁边床位空着，行李却还在。下楼找找她。";}
            if(!story.Has("poison"))return story.Has("help")?"查明她为何昏迷\n老板提到二楼靠楼梯、门开着的单床房。去看看。":story.Has("medicineNote")?"查明她为何昏迷\n药笺说须先辨明药种。问问老板，或找找昨夜留下的痕迹。":"先设法救她\n她呼吸尚稳，却叫不醒。问问老板昨夜发生了什么。";
            if(!story.antidote)return "寻找对应的解药\n已辨出昏睡散。查看柜台右侧药箱的标签。";
            if(!story.Has("medicineNote")&&(!story.Has("help")||story.threatened))return "核对救治方法\n解药与绷带已备好。二楼行李中的药笺记有用法。";
            return "回到她身边救治\n解药与绷带已备好，检查她并选择救治方式。";
        }
        if(!story.sword)return "准备上路\n她已经醒来。向老板谈谈借剑防身的事。";
        if(story.pact==0)return "与她说定接下来的路\n离开之前，听听她的想法，确认护送约定。";
        return !story.opened?"一同离开客栈\n前往正门，抬起门闩。":"一同离开客栈\n等她跟上，再一起越过门槛。";
    }
    string ObjectLabel(string label){
        switch(label){
            case "己方行李与杯子":return "行李与杯子";
            case "昨夜客房：药粉与残食":return "留下的碗碟";
            case "检查同行者 / 交谈":return story.awake?"与她交谈":"查看同行者";
            case "柜台血滴与手伤":return "柜台旁的血滴";
            case "剑鞘镖号与旧账":return "旧账与剑鞘";
            case "正门与离店约定":return "正门";
            default:return label;
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
        if(scanning)GUI.Label(new Rect(width/2-150,210,300,35),"凝神观察 · "+Mathf.CeilToInt(scanUntil-Time.time)+" 秒");
    }
    void ReadDocument(string title,string text,params string[] ids){foreach(var id in ids)Fact(id);Show(title,text,NarrativeMode.Document);}
}
