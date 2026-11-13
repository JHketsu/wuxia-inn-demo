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
    void SetCursor(bool free){Cursor.lockState=free?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=free;}
    void Show(string title,string text, NarrativeMode mode=NarrativeMode.Action){dialogTitle=title;dialogText=text;narrativeMode=mode;innerThought=null;options.Clear();SetCursor(true);}
    void Speak(string speaker,string text,Action back=null){Show(speaker,text,NarrativeMode.Dialogue);if(back!=null)ContinueConversation(back);}
    void Observe(string title,string text,string thought,params string[] ids){foreach(var id in ids)Fact(id);Show(title,text,NarrativeMode.Observation);innerThought=thought;}
    void Choice(string text,Action action,string id=null,bool trackRead=true){string key=id??(dialogTitle+"|"+text);options.Add((text,()=>{if(trackRead)story.MarkChoice(key);var before=Snapshot();action();Audit(before,text);if(dialogTitle!=null && options.Count==0 && narrativeMode==NarrativeMode.Dialogue && conversation!=null)ContinueConversation(conversation);},key,trackRead));}
    void ContinueConversation(Action action){Choice(Copy("story_continueconversation_01"),action,"navigation.continue",false);}
    void CloseDialog(){dialogTitle=null;innerThought=null;conversation=null;options.Clear();SetCursor(false);}
    void Fact(string id){story.Know(id);if(story.Has("bossTiming")&&story.Has("companionTiming"))story.Know("timing");}
    void Inspect(string title,string text,params string[] ids){foreach(var id in ids)Fact(id);Show(title,text,NarrativeMode.Observation);}
    void Hotspot(string label,Vector3 point,Action action){var g=new GameObject("交互 · "+Copy(label));g.transform.position=point;Add(g.transform,label,()=>{conversation=null;var before=Snapshot();action();Audit(before,Copy(label));});markers.Add((g.transform,label));}
    void BuildStory(Transform stain){
        // Each interaction uses a reachable anchor separate from prop colliders.
        Hotspot("hotspot_luggage_and_cup",new(-6,UpperWalkY+.9f,9.5f),()=>Observe(Copy("story_buildstory_01"),Copy("story_buildstory_02"),story.Has("emptyBed")?Copy("story_buildstory_03"):Copy("story_buildstory_04"),"luggage","cup"));
        Hotspot("hotspot_medicine_note",new(-7.5f,UpperWalkY+.9f,9),()=>ReadDocument(Copy("story_buildstory_05"),Copy("story_buildstory_06"),"medicineNote"));
        BuildUpstairsRooms();
        BuildGuestRoom();
        Hotspot("hotspot_guest_meal",new(-6,UpperWalkY+.85f,3.4f),()=>Observe(Copy("story_buildstory_07"),Copy("story_buildstory_08"),Copy("story_buildstory_09"),"poison","meal"));
        Hotspot("hotspot_companion",new(-5,1.1f,-.2f),CompanionTalk);
        Hotspot("hotspot_table_blood",new(-3,1,-.2f),()=>Observe(Copy("story_buildstory_10"),Copy("story_buildstory_11"),Copy("story_buildstory_12"),"blood"));
        Table("藏令桌",new(-4,0,2),1.4f,.9f);
        Hotspot("hotspot_hidden_order",new(-2.5f,1,2),OrderTalk);
        Box("柜台血滴",new(4,.03f,4.8f),new(.4f,.04f,.4f),new Color(.4f,.02f,.02f),false);
        Hotspot("hotspot_counter_blood",new(3,1.2f,4.5f),()=>Observe(Copy("story_buildstory_13"),Copy("story_buildstory_14"),Copy("story_buildstory_15"),"hand"));
        Hotspot("hotspot_innkeeper",new(5,1.4f,4.9f),BossTalk);
        Box("柜台药箱",new(7,CounterHeight+.15f,6),new(.5f,.3f,.4f),new Color(.2f,.5f,.4f));
        Hotspot("hotspot_medicine_chest",new(7,1.2f,4.9f),MedicineTalk);
        Table("柜台侧边书桌",new(8.6f,0,5.8f),1.1f,.8f);
        Box("账本",new(8.65f,.8f,5.8f),new(.3f,.06f,.4f),new Color(.34f,.26f,.16f));
        Hotspot("hotspot_ledger",new(8.8f,1.3f,5),()=>Observe(Copy("story_buildstory_16"),Copy("story_buildstory_17"),Copy("story_buildstory_18"),"ledger"));
        Hotspot("hotspot_front_door",new(0,1.4f,-8.6f),ExitTalk);
        Say(Copy("story_buildstory_19"));
    }
    void SwordTalk(){
        Speak(Copy("speaker_boss"),Copy("story_swordtalk_01"));
        if(story.CanSword(1))Choice(Copy("story_swordtalk_02"),()=>ConfirmSword(1));
        if(story.CanSword(2))Choice(Copy("story_swordtalk_03"),()=>ConfirmSword(2));
        Choice(Copy("story_swordtalk_04"),()=>ConfirmSword(3));
        Choice(Copy("story_swordtalk_05"),BossTalk);
    }
    void ConfirmSword(int route){
        Show(Copy("story_confirmsword_01"),route==1?Copy("story_confirmsword_02"):route==2?Copy("story_confirmsword_03"):Copy("story_confirmsword_04"));
        Choice(Copy("story_confirmsword_05")+(route==3?Copy("story_confirmsword_06"):Copy("story_confirmsword_07"))+Copy("story_confirmsword_08"),()=>{if(story.TakeSword(route)){SyncStory();Show(Copy("story_confirmsword_09"),InnNarrativeRules.Current.Dialogue("afterSword",story));}});
        Choice(Copy("story_confirmsword_10"),BossTalk);
    }
    void MedicineTalk(){
        Observe(Copy("story_medicinetalk_01"),Copy("story_medicinetalk_02"),story.Has("poison")?Copy("story_medicinetalk_03"):Copy("story_medicinetalk_04"),"medicineLabels");
        if(!story.Has("poison")){Choice(Copy("story_medicinetalk_05"),()=>Show(Copy("speaker_thought"),Copy("story_medicinetalk_06")));return;}
        if(!story.antidote)Choice(Copy("story_medicinetalk_07"),()=>{story.antidote=true;Show(Copy("story_medicinetalk_08"),Copy("story_medicinetalk_09"));});
    }
    void CompanionTalk(){
        conversation=CompanionTalk;
        if(!story.awake){Observe(Copy("story_companiontalk_01"),Copy("story_companiontalk_02"),Copy("story_companiontalk_03"),"wound");
            if(story.antidote){if(story.Has("help")&&!story.threatened)Choice(Copy("story_companiontalk_04"),()=>Heal(true));if(story.Has("medicineNote"))Choice(Copy("story_companiontalk_05"),()=>Heal(false));else Choice(Copy("story_companiontalk_06"),()=>Show(Copy("story_companiontalk_07"),Copy("story_companiontalk_08")));}return;}
        string intro=story.Has("orderRead")&&!story.Has("signature")?Copy("story_companiontalk_09"):!story.Has("wakeConversation")?Copy("story_companiontalk_10"):Copy("story_companiontalk_11");
        Fact("wakeConversation");Speak(Copy("speaker_companion"),intro);
        if(story.accused&&!story.corrected)dialogText+=Copy("story_companiontalk_12");
        Choice(Copy("story_companiontalk_13"),()=>{Fact("admission");Fact("appearanceConsent");Fact("companionTiming");if(story.Has("bossTiming"))Fact("timing");Speak(Copy("speaker_companion"),Copy("story_companiontalk_14"));},"companion.actions");
        Choice(Copy("story_companiontalk_15"),()=>{Fact("concern");Speak(Copy("speaker_companion"),Copy("story_companiontalk_16"));},"companion.concern");
        if(story.Has("orderRead"))Choice(Copy("story_companiontalk_17"),()=>{Fact("signature");Fact("admission");Fact("concern");Speak(Copy("speaker_companion"),Copy("story_companiontalk_18"));},"companion.order");
        if(story.orderOwned){Choice(Copy("story_companiontalk_19"),()=>{story.orderOwned=false;story.orderReturned=true;Speak(Copy("speaker_companion"),Copy("story_companiontalk_20"));},"companion.returnOrder");Choice(Copy("story_companiontalk_21"),()=>{story.consent=true;Speak(Copy("speaker_companion"),Copy("story_companiontalk_22"));},"companion.keepOrder");}
        Choice(Copy("story_companiontalk_23"),()=>{story.consent=true;Speak(Copy("speaker_companion"),Copy("story_companiontalk_24"));},"companion.consent");
        Choice(story.pact>0?Copy("story_companiontalk_25"):Copy("story_companiontalk_26"),PactTalk);
    }
    void Heal(bool assisted){if(story.awake||!story.antidote||!story.Has("poison"))return;story.assisted=assisted;story.bandaged=true;story.awake=true;Fact("treatment");SyncStory();CompanionTalk();dialogText=(assisted?Copy("story_heal_01"):Copy("story_heal_02"))+"\n"+dialogText;}
    void OrderTalk(){
        if(!story.Has("orderFound")){Show(Copy("story_ordertalk_01"),Copy("story_ordertalk_02"));Choice(Copy("story_ordertalk_03"),()=>{Fact("orderFound");story.orderOwned=true;OrderTalk();});return;}
        Show(Copy("story_ordertalk_04")+(story.orderOwned?Copy("story_ordertalk_05"):story.orderReturned?Copy("story_ordertalk_06"):Copy("story_ordertalk_07")),Copy("story_ordertalk_08"));
        Choice(Copy("story_ordertalk_09"),()=>{Fact("orderRead");Show(Copy("story_ordertalk_10"),Copy("story_ordertalk_11"),NarrativeMode.Document );});
    }
    void PactTalk(){
        Show(Copy("story_pacttalk_01"),story.awake?InnNarrativeRules.Current.Dialogue("pactGreeting",story):Copy("story_pacttalk_02"));if(!story.awake)return;
        Choice(Copy("story_pacttalk_03"),()=>ConfirmPact(1),"pact.personal");
        if(story.CanPact(2))Choice(Copy("story_pacttalk_04"),()=>ConfirmPact(2),"pact.together");
        if(story.CanPact(3))Choice(Copy("story_pacttalk_05"),()=>ConfirmPact(3),"pact.verify");
    }
    void ConfirmPact(int route){Speak(Copy("speaker_companion"),route==1?Copy("story_confirmpact_01"):route==2?Copy("story_confirmpact_02"):Copy("story_confirmpact_03"));Choice(Copy("story_confirmpact_04"),()=>{story.SetPact(route);Show(Copy("story_confirmpact_05"),Copy("story_confirmpact_06"));});}
    void ExitTalk(){
        Show(Copy("story_exittalk_01"),!story.awake?Copy("story_exittalk_02"):!story.sword?Copy("story_exittalk_03"):story.pact==0?Copy("story_exittalk_04"):Copy("story_exittalk_05"));
        if(!opened)Choice(Copy("story_exittalk_06"),()=>{Open(false);ExitTalk();});
        if(story.awake)Choice(story.pact>0?Copy("story_exittalk_07"):Copy("story_exittalk_08"),PactTalk);
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
        foreach(var marker in markers)if(marker.label=="hotspot_companion")marker.anchor.position=companion.position+Vector3.forward*.7f;
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
    void SaveStory(){story.layoutVersion=LayoutVersion;story.playerPosition=player.transform.position;story.companionPosition=companion.position;PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(story));PlayerPrefs.Save();Say(Copy("story_savestory_01"));}
    void LoadStory(){if(!PlayerPrefs.HasKey(SaveKey)){Say(Copy("story_loadstory_01"));return;}try{story=JsonUtility.FromJson<InnStoryState>(PlayerPrefs.GetString(SaveKey));if(story.layoutVersion!=LayoutVersion){story.playerPosition=story.awake?new Vector3(-3,0,-1):HeroSpawn;story.companionPosition=CompanionRest(story.awake);story.layoutVersion=LayoutVersion;}SyncStory();player.enabled=false;player.transform.position=story.playerPosition;player.enabled=true;if(follower)follower.enabled=false;companion.position=story.companionPosition;if(follower)follower.enabled=awake;trail.Clear();companionFollowing=false;departing=false;ResetCamera();dialogTitle=null;journal=false;SetCursor(won);Say(Copy("story_loadstory_02"));}catch(Exception e){Say(Copy("story_loadstory_03")+e.Message);}}
    string Ending(){string pact=story.pact==1?Copy("story_ending_01"):story.pact==2?Copy("story_ending_02"):Copy("story_ending_03");string swordLine=story.swordOutcome==1?Copy("story_ending_04"):story.swordOutcome==2?Copy("story_ending_05"):Copy("story_ending_06");return pact+"\n\n"+swordLine+"\n\n"+(story.broken?Copy("story_ending_07"):Copy("story_ending_08"))+(story.orderReturned?Copy("story_ending_09"):story.orderOwned?Copy("story_ending_10"):Copy("story_ending_11"));}
    void DrawStoryUI(){DrawPresentation();}
}
