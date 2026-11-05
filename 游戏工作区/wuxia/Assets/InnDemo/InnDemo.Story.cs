using System;
using System.Collections.Generic;
using UnityEngine;

public partial class InnDemo
{
    public InnStoryState story = new();
    string dialogTitle, dialogText; bool journal;
    enum NarrativeMode { Observation, Dialogue, Document, Action }
    NarrativeMode narrativeMode;
    string innerThought;
    bool showObjectives=true;
    float scanUntil;
    Action conversation;

    readonly List<(string text,Action action,string id,bool trackRead)> options = new();
    readonly List<Vector3> trail = new();
    readonly List<(Transform anchor,string label)> markers = new();
    Vector2 journalScroll; float trailTimer;
    bool companionFollowing;
    const float FollowStopDistance=2.2f, FollowResumeDistance=3.2f, PersonalSpace=1.35f;
    const string SaveKey="Wuxia.Inn.StoryV1";
    static readonly string[] truths={"主动离房并参与取令","知情请求并服用改貌药","客人先下昏睡毒，改貌药并非主因","桌边血来自同行者旧伤","老板曾拒绝交人并拦截客人","手令来自家中渠道，要求接管","隐瞒是担心被依令转交","老板曾强送活人，后来放人结怨"};
    void SetCursor(bool free){Cursor.lockState=free?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=free;}
    void Show(string title,string text, NarrativeMode mode=NarrativeMode.Action){dialogTitle=title;dialogText=text;narrativeMode=mode;innerThought=null;options.Clear();SetCursor(true);}
    void Speak(string speaker,string text,Action back=null){Show(speaker,text,NarrativeMode.Dialogue);if(back!=null)ContinueConversation(back);}
    void Observe(string title,string text,string thought,params string[] ids){foreach(var id in ids)Fact(id);Show(title,text,NarrativeMode.Observation);innerThought=thought;}
    void Choice(string text,Action action,string id=null,bool trackRead=true){string key=id??(dialogTitle+"|"+text);options.Add((text,()=>{if(trackRead)story.MarkChoice(key);var before=Snapshot();action();Audit(before,text);if(dialogTitle!=null && options.Count==0 && narrativeMode==NarrativeMode.Dialogue && conversation!=null)ContinueConversation(conversation);},key,trackRead));}
    void ContinueConversation(Action action){Choice("继续交谈",action,"navigation.continue",false);}
    void CloseDialog(){dialogTitle=null;innerThought=null;conversation=null;options.Clear();SetCursor(false);}
    void Fact(string id){story.Know(id);if(story.Has("bossTiming")&&story.Has("companionTiming"))story.Know("timing");}
    void Inspect(string title,string text,params string[] ids){foreach(var id in ids)Fact(id);Show(title,text,NarrativeMode.Observation);}
    void Hotspot(string label,Vector3 point,Action action){var g=new GameObject("交互 · "+label);g.transform.position=point;Add(g.transform,label,()=>{conversation=null;var before=Snapshot();action();Audit(before,label);});markers.Add((g.transform,label));}
    void BuildStory(Transform stain){
        // Each interaction uses a reachable anchor separate from prop colliders.
        Hotspot("己方行李与杯子",new(-6,UpperWalkY+.9f,9.5f),()=>Observe("床边的行李与杯子","两人的包袱放在床边。杯沿凝着一圈浅色药渍。",story.Has("emptyBed")?"包袱还在，她应该没走远。下楼找找。":"收拾好就该上路了。先叫她起来。","luggage","cup"));
        Hotspot("随行药笺",new(-7.5f,UpperWalkY+.9f,9),()=>ReadDocument("原照顾者留下的药笺","昏睡散：使人沉睡，伤重者尤甚。辨明药种后，以对应解药服下；若旧伤出血，须另行清创包扎。\n换容药：仅易容貌，不可作解药。\n笺末，是原照顾者熟悉的笔迹：路上留着，莫急着乱用药。","medicineNote"));
        BuildUpstairsRooms();
        BuildGuestRoom();
        Hotspot("昨夜客房：药粉与残食",new(-6,UpperWalkY+.85f,3.4f),()=>Observe("残食与药粉","纸包上写着「昏睡散」。碗底剩下的粉末，与包里的一样。","昏睡散……这倒像她的症状。去看看药箱里有没有对应的解药。","poison","meal"));
        Hotspot("检查同行者 / 交谈",new(-5,1.1f,-.2f),CompanionTalk);
        Hotspot("桌边血迹",new(-3,1,-.2f),()=>Observe("桌边的血迹","桌沿上有一道沾血的擦痕。血点落到她躺过的位置，桌底一根木条上还有新划痕。","她倒下前碰过这里？那根木条像是松的。","blood"));
        Table("藏令桌",new(-4,0,2),1.4f,.9f);
        Hotspot("桌底松动木条",new(-2.5f,1,2),OrderTalk);
        Box("柜台血滴",new(4,.03f,4.8f),new(.4f,.04f,.4f),new Color(.4f,.02f,.02f),false);
        Hotspot("柜台血滴与手伤",new(3,1.2f,4.5f),()=>Observe("柜台前的血滴","几滴血落在柜台脚边。老板握布的手缠着绷带，边缘仍渗着红。","这里也有人受了伤。问问他昨夜发生了什么。","hand"));
        Hotspot("老板：交谈",new(5,1.4f,4.9f),BossTalk);
        Box("柜台药箱",new(7,CounterHeight+.15f,6),new(.5f,.3f,.4f),new Color(.2f,.5f,.4f));
        Hotspot("药箱与药物标签",new(7,1.2f,4.9f),MedicineTalk);
        Table("柜台侧边书桌",new(8.6f,0,5.8f),1.1f,.8f);
        Box("账本",new(8.65f,.8f,5.8f),new(.3f,.06f,.4f),new Color(.34f,.26f,.16f));
        Hotspot("剑鞘镖号与旧账",new(8.8f,1.3f,5),()=>Observe("旧账与剑鞘","翻开的账页上，交付的不是货物，而是人。几笔款项旁有同一个客人的记号。剑鞘刻着旧镖号。","他从前走过活人镖？这几笔往来，得听他亲口说。","ledger"));
        Hotspot("正门与离店约定",new(0,1.4f,-8.6f),ExitTalk);
        Say("女主 · 心想\n天快亮了。昨夜说好，今日由我陪她继续上路。先叫她起来，看看伤势如何。 ");
    }
    void BossTalk(){
        conversation=BossTalk;
        Speak("老板",story.sword?"「还有什么想问的？」":story.awake?"「醒了就好。她伤还没稳，路上慢些。」":story.Has("wound")?"他放下擦剑的布，望了一眼桌边。\n「你醒了。她呼吸还稳，只是一直叫不应。」":"他抬起头，手里的布停了下来。\n「你在找同来的姑娘？她在那边桌旁。」");
        if(!story.awake)Choice("「她怎么了？你能帮忙么？」",()=>{Fact("help");Speak("老板",story.threatened?"「药箱照样能用。但先认清药种，我不碰她，你自己来。」":"「她夜里忽然倒下。我不敢乱喂药。昨夜那客人行迹可疑，你去二楼靠楼梯、门开着的那间单床房看看。兴许有线索。药箱在我右手边；认清了药，我帮你包扎。」",BossTalk);},"boss.help");
        Choice("「昨夜究竟出了什么事？」",()=>{Fact("incident");Speak("老板","「她从楼上回来，撑到桌边就倒了，旧伤直渗血。那客人非要我交人，我没答应，跟他动了剑，手也伤了。他不肯再闹大，走了。\n这剑上的血，有扶她时沾的。我只见她倒前摸过桌底，没看清放了什么。」",BossTalk);},"boss.incident");
        Choice("「她这副模样，是你用药害的？」",()=>{story.accused=true;Speak("老板","「换容药是她自己来买的，我没瞒她是什么。你若不信，等她醒来问她。眼下先救人，药箱我不锁。」",BossTalk);},"boss.accuse");
        if(!story.awake)Choice("「你退开。药我自己拿，不许再碰她。」",()=>{story.threatened=true;Speak("老板","他松开手，退了半步。\n「好，我不碰。药箱在右边。你照带来的药笺办，别把药用错。」",BossTalk);},"boss.threaten");
        if(story.Has("poison"))Choice("「她吃饭和服换容药，哪个在先？」",()=>{Fact("bossTiming");Speak("老板","「先吃的饭，后来才向我买换容药。客人发现东西丢了，是再后面的事。他没认出她的脸，却盯上了衣服，追问原先那个金发住客。\n我没看见他下药，不敢说她是哪一口吃中的。」",BossTalk);},"boss.timing");
        if(story.Has("ledger"))Choice("「旧账上送的那些人，都愿意跟你走么？」",()=>{Fact("past");Speak("老板","他把布折了又折。\n「有一个不愿意。我还是照约交了人。后来听说被关着，再没消息。最后怎样，我不知道。\n再后来我放走过另一个，害那客人赔了钱，也得罪了雇主。我退了这行，却没断干净。如今说一句按约办事，也遮不住。」",BossTalk);},"boss.past");
        if(story.accused&&!story.corrected&&story.Has("appearanceConsent"))Choice("「改貌是她自己要的。方才是我判断太快。」",()=>{story.corrected=true;Speak("老板","「肯回头核对，就好。但我的旧事，也不会因此变干净。带她走吧。」",BossTalk);},"boss.correct");
        if(story.awake&&!story.sword)Choice("「路上不太平。谈谈你这把剑。」",SwordTalk,"boss.sword");
    }
    void SwordTalk(){
        Speak("老板","他下意识拢了拢受伤的手。\n「你想拿这把剑？」");
        if(story.CanSword(1))Choice("「我会听她自己的意思。借你的剑，护我们一程。」",()=>ConfirmSword(1));
        if(story.CanSword(2))Choice("「她不该再为你的旧账冒险。这把剑，交给我护路。」",()=>ConfirmSword(2));
        Choice("「把剑给我。」（逼他交剑……）",()=>ConfirmSword(3));
        Choice("「我再想想。」",BossTalk);
    }
    void ConfirmSword(int route){
        Show("老板的剑",route==1?"老板：我愿交剑防身，但我自己也少了一分自保。愿你别替她决定去留。":route==2?"老板：剑给你，算我承担的一份。别把我说成恩人；没了剑，这店也未必保得住。":"你将手按向剑。老板看了眼受伤的手，没有迎上来：我不愿再伤人，你要拿，便拿。你也看到了我拿什么守店。");
        Choice("确认"+(route==3?"夺取":"接过")+"这把剑",()=>{if(story.TakeSword(route)){SyncStory();Show("剑已在你手中",InnNarrativeRules.Current.Dialogue("afterSword",story));}});
        Choice("暂不取剑",BossTalk);
    }
    void MedicineTalk(){
        Observe("柜台药箱","药箱分着几格，标签分别写着「换容」「昏睡散解药」。旁边叠着干净的绷带。",story.Has("poison")?"药种对得上。拿解药和绷带，回她身边。":"先别乱用。得知道她为何昏迷。","medicineLabels");
        if(!story.Has("poison")){Choice("核对药种",()=>Show("女主 · 心想","几种药不能混着试。得先知道她中了什么；若老板知道昨夜的事，先问问他。"));return;}
        if(!story.antidote)Choice("取得对应解药与绷带",()=>{story.antidote=true;Show("已取得救治物品","你取出标好的解药和一卷绷带，收进随身的药囊。\n女主 · 心想：回她身边去。");});
    }
    void CompanionTalk(){
        conversation=CompanionTalk;
        if(!story.awake){Observe("她的伤势","你俯下身。她呼吸尚稳，衣下旧伤却又渗出了血。你唤了两声，没有回应。","脸和发色都变了……但这衣服、这道伤，是她。先想办法让她醒来。","wound");
            if(story.antidote){if(story.Has("help")&&!story.threatened)Choice("请老板协助解毒、重新包扎",()=>Heal(true));if(story.Has("medicineNote"))Choice("依药笺独自解毒、重新包扎",()=>Heal(false));else Choice("想想随行的药笺",()=>Show("随行药笺","那张药笺还夹在二楼的行李里。上面记过几种常见毒物的用法。"));}return;}
        string intro=story.Has("orderRead")&&!story.Has("signature")?"她的目光停在那页纸上，手指收紧。\n「你读过了？上面叫你看管我，你……怎么想？」":!story.Has("wakeConversation")?"她睁开眼，先望向空着的客房门。\n「昨夜那个客人……走了么？」":"她扶着桌沿，呼吸比方才平稳些。\n「你还有什么想问的？」";
        Fact("wakeConversation");Speak("同行者",intro);
        if(story.accused&&!story.corrected)dialogText+="\n她：改貌是我主动找老板买的，不是他逼我。你先别认定是他下的毒。";
        Choice("「昨夜我歇下后，你去了哪里？」",()=>{Fact("admission");Fact("appearanceConsent");Fact("companionTiming");if(story.Has("bossTiming"))Fact("timing");Speak("同行者","「我吃过饭，向老板买了换容药。他告诉了我是什么，是我自己要喝的。\n趁那客人离房，我进去拿了手令。回来才突然发沉，旧伤也裂了……纸塞在桌底的松木条后。至少它没落回他手里。」");},"companion.actions");
        Choice("「你是不是有什么事，一直没敢告诉我？」",()=>{Fact("concern");Speak("同行者","「我看见那客人拿着家里的手令，怕你见了，就觉得送到他手里也算履约。\n我答应跟你回去，也答应帮你查事……可不是谁来接，我都愿意跟。那时候，我还不敢先问你。」");},"companion.concern");
        if(story.Has("orderRead"))Choice("「这签押你认得么？他们要把你交给谁？」",()=>{Fact("signature");Fact("admission");Fact("concern");Speak("同行者","她把纸举近些，指着暗记。\n「这是家里用的。只凭这道手令，我还不知道是谁安排的。可我认识这句『等待接应』——他们没打算问我愿不愿意。\n所以我才先拿过来。我想留着它，回去也有得问。」");},"companion.order");
        if(story.orderOwned){Choice("把手令归还给她",()=>{story.orderOwned=false;story.orderReturned=true;Speak("同行者","她把手令仔细折好。\n「谢谢。你想知道什么，可以问我。别让这张纸替我答应。」");},"companion.returnOrder");Choice("「先放我这里，可好？你要看时就拿去。」",()=>{story.consent=true;Speak("同行者","「好，先放你那里。要用时给我看，别悄悄把它交出去。」");},"companion.keepOrder");}
        Choice("「有人要接你，我会先问你的意思。」",()=>{story.consent=true;Speak("同行者","她：那遇到家里来人，先跟我谈。别只在这时说给我听。");},"companion.consent");
        Choice(story.pact>0?"「刚才说好的路，我想再谈谈。」":"「出了客栈，我们接下来怎么走？」",PactTalk);
    }
    void Heal(bool assisted){if(story.awake||!story.antidote||!story.Has("poison"))return;story.assisted=assisted;story.bandaged=true;story.awake=true;Fact("treatment");SyncStory();CompanionTalk();dialogText=(assisted?"老板按住绷带，你喂下对应解药；她渐渐睁眼。":"你依药笺喂药，重新包扎旧伤；她渐渐睁眼。")+"\n"+dialogText;}
    void OrderTalk(){
        if(!story.Has("orderFound")){Show("桌底 · 新划痕","血迹终点旁的木条刚被推回。里面露出纸角，像是有人仓促藏下的私人物件。");Choice("挪开木条，取出纸页",()=>{Fact("orderFound");story.orderOwned=true;OrderTalk();});return;}
        Show("手令 · "+(story.orderOwned?"由你持有":story.orderReturned?"已归还她":"曾经发现"),"纸页被折得很紧，边缘蹭着血。封口已经撕开，里面似有一份手令。");
        Choice("阅读手令",()=>{Fact("orderRead");Show("手令正文","「找到后，就地看管，等待接应。」\n落款处有签押，旁边压着一个细小暗记。",NarrativeMode.Document );});
    }
    void PactTalk(){
        Show("离店约定",story.awake?InnNarrativeRules.Current.Dialogue("pactGreeting",story):"她仍没有睁眼，听不见你的问话。");if(!story.awake)return;
        Choice("「我答应亲自送你，就不会把这件事交给旁人。」",()=>ConfirmPact(1),"pact.personal");
        if(story.CanPact(2))Choice("「继续回去。但接应、改道、交接，我们都先商量。」",()=>ConfirmPact(2),"pact.together");
        if(story.CanPact(3))Choice("「先查清这道手令和接应的人，再定怎么回去。」",()=>ConfirmPact(3),"pact.verify");
    }
    void ConfirmPact(int route){Speak("同行者",route==1?"「你亲自送，我信得过。只是路上若有变故，也听听我怎么说。」":route==2?"「好。遇到接应的人，先让我知道。别替我答应，我也不瞒着你改道。」":"「好，先查清楚。我不是要反悔，只是不想这么糊里糊涂地被带回去。」");Choice("双方确认",()=>{story.SetPact(route);Show("说定接下来的路","她点了点头，整理好衣袖。\n「那就这样说定。走之前，你若还有话，我听着。」");});}
    void ExitTalk(){
        Show("客栈正门",!story.awake?"她还昏迷着。先辨毒、救醒并包扎，不能把她留下。":!story.sword?"女主 · 心想：她能走了。路上未必太平，我得向老板要件防身的兵器。":story.pact==0?"她看着你手里的剑，像是还有话要说。\n「出了这道门以后，你打算怎么办？」":"门外晨光渐亮。她正缓缓走来，等一等，就能一起上路。");
        if(!opened)Choice("抬起门闩，开门",()=>{Open(false);ExitTalk();});
        if(story.awake)Choice(story.pact>0?"再谈刚才的约定（可选）":"与她商量接下来的路",PactTalk);
    }
    void SyncStory(){
        awake=story.awake;armed=story.sword;opened=story.opened;won=story.won;medicine=story.antidote;door.SetActive(!opened);
        if(awake){var col=companion.GetComponent<CapsuleCollider>();if(col)col.enabled=false;if(!follower){follower=companion.gameObject.AddComponent<CharacterController>();follower.height=CompanionHeight/companion.lossyScale.y;follower.radius=.28f/Mathf.Max(companion.lossyScale.x,companion.lossyScale.z);follower.stepOffset=.35f/companion.lossyScale.y;follower.skinWidth=.03f;companion.position=CompanionRest(true);}companion.rotation=Quaternion.identity;}
        else {if(follower)follower.enabled=false;var col=companion.GetComponent<CapsuleCollider>();if(col)col.enabled=true;companion.rotation=Quaternion.Euler(0,0,90);}
        if(follower)follower.enabled=awake;
        if(armed){sword.SetParent(body);sword.localPosition=new(.45f,.2f,.2f);sword.localRotation=Quaternion.identity;}else{sword.SetParent(null);sword.position=SwordRest;}
        if(story.Has("bossTiming")&&story.Has("companionTiming"))Fact("timing");
    }
    void TickFollower(){
        SyncStory();if(!awake||!follower)return;
        foreach(var marker in markers)if(marker.label=="检查同行者 / 交谈")marker.anchor.position=companion.position+Vector3.forward*.7f;
        trailTimer-=Time.deltaTime;if(trailTimer<=0){trailTimer=.15f;var point=player.transform.position+Vector3.up*CompanionCenterY;if(trail.Count==0||Vector3.Distance(trail[trail.Count-1],point)>.5f)trail.Add(point);}
        Vector3 separation=companion.position-(player.transform.position+Vector3.up*CompanionCenterY);
        float heightDifference=Mathf.Abs(separation.y);separation.y=0;float distance=separation.magnitude;
        if(distance<PersonalSpace && heightDifference<1.2f){
            Vector3 away=distance>.05f?separation.normalized:-body.forward;
            follower.Move((away*1.3f+Vector3.down*5)*Time.deltaTime);
            companionFollowing=false;return;
        }
        if(companionFollowing){if(distance<=FollowStopDistance && heightDifference<1.2f)companionFollowing=false;}
        else if(distance>=FollowResumeDistance || heightDifference>=1.2f)companionFollowing=true;
        if(!companionFollowing){follower.Move(Vector3.down*5*Time.deltaTime);return;}
        if(trail.Count==0)return;
        // Follow the player's visited route rather than walking through furniture.
        var goal=trail[0];if(Vector3.Distance(companion.position,goal)<.85f){if(trail.Count>1)trail.RemoveAt(0);else return;}
        Vector3 delta=goal-companion.position;delta.y=0;Vector3 step=delta.normalized*2.3f;
        follower.Move((step+Vector3.down*5)*Time.deltaTime);if(delta.sqrMagnitude>.01f)companion.rotation=Quaternion.LookRotation(delta);
    }
    void SaveStory(){story.layoutVersion=LayoutVersion;story.playerPosition=player.transform.position;story.companionPosition=companion.position;PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(story));PlayerPrefs.Save();Say("进度已保存（F9 读取）。");}
    void LoadStory(){if(!PlayerPrefs.HasKey(SaveKey)){Say("还没有剧情 v1 存档，按 F5 保存。");return;}try{story=JsonUtility.FromJson<InnStoryState>(PlayerPrefs.GetString(SaveKey));if(story.layoutVersion!=LayoutVersion){story.playerPosition=story.awake?new Vector3(-3,0,-1):HeroSpawn;story.companionPosition=CompanionRest(story.awake);story.layoutVersion=LayoutVersion;}SyncStory();player.enabled=false;player.transform.position=story.playerPosition;player.enabled=true;if(follower)follower.enabled=false;companion.position=story.companionPosition;if(follower)follower.enabled=awake;trail.Clear();companionFollowing=false;departing=false;ResetCamera();dialogTitle=null;journal=false;SetCursor(won);Say("已读取：线索、物件归属、约定与剑的来由均已恢复。");}catch(Exception e){Say("存档无法读取："+e.Message);}}
    string Ending(){string pact=story.pact==1?"亲自履约\n她：你亲自送我，我记下了。路上的决定，我们还得继续谈。":story.pact==2?"共同决定\n她：遇到家中来人，先商量。今天我们说定的，我会记着。":"暂缓交付\n她：先核实手令和接应。一起走，答案还没到该替我写下的时候。";string swordLine=story.swordOutcome==1?"托付 · 老板：剑交给你，愿你记住她的意愿。我也得想想这店怎么守。":story.swordOutcome==2?"追责 · 老板：这是我担的代价，不是求你们谢我。":"夺取 · 老板看着空剑架：拿去吧。这一步是你选的。";return pact+"\n\n"+swordLine+"\n\n"+(story.broken?"门闩断在身后。":"门在身后缓缓合上。")+(story.orderReturned?"手令收在她怀里。":story.orderOwned?"手令仍由你保管。":"还有一些事留在了客栈。")+"\n已确认真相 "+story.TruthCount+"/8（不代表善恶评价）\nR 重开 · F9 读档";}
    void DrawStoryUI(){DrawPresentation();}
    string EvidenceText(){var map=new Dictionary<string,string>{{"emptyBed","观察：呼唤后发现旁边床位无人"},{"luggage","物证：未带走的行李"},{"cup","物证：杯中药渍（不能单独归因）"},{"medicineNote","物证：原照顾者的药笺"},{"wound","观察：旧伤与外貌变化"},{"blood","观察：桌边血迹与木条划痕"},{"hand","观察：老板手伤与柜台血滴"},{"poison","物证：昏睡散包装"},{"meal","物证：残食中相符的粉末"},{"ledger","物证：镖号与旧账"},{"incident","证词：老板讲述倒地、拦人和交手"},{"bossTiming","证词：老板确认饭后售改貌药"},{"companionTiming","证词：她确认用餐、服药、取令先后"},{"admission","证词：她承认主动取令"},{"appearanceConsent","证词：她确认知情请求改貌"},{"signature","证词：她识别手令签押和暗记"},{"concern","证词：她明确担忧被转交"},{"past","证词：老板承认旧事（旧受害者最终下落未知）"},{"orderFound","物件：已发现手令"},{"orderRead","阅读：手令要求就地看管、等待接应"}};var lines=new List<string>();foreach(var pair in map)if(story.Has(pair.Key))lines.Add(pair.Value);lines.Add("手令归属："+(story.orderReturned?"同行者":story.orderOwned?"女主":"尚未取得"));lines.Add("救治方式："+(!awake?"尚未救醒":story.assisted?"老板协助":"女主依药笺独自救治"));if(story.accused)lines.Add("行为：曾公开指控"+(story.corrected?"，已明确修正":"，尚未修正"));return string.Join("\n",lines);}
}
