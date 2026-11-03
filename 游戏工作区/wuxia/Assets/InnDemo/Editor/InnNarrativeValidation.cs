using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class InnNarrativeValidation
{
    static void Check(bool condition,string message,List<string> errors){if(!condition)errors.Add(message);}
    [MenuItem("Tools/Inn Demo/Validate Narrative Rules")]
    public static void MenuRun(){Debug.Log(Run());}
    public static string Run(){
        InnNarrativeRules.Reload();var errors=new List<string>();var rules=InnNarrativeRules.Current;
        var known=new HashSet<string>{"awake","sword","consent","pactAgreed","swordS3"};
        foreach(var fact in rules.factCatalog)Check(known.Add(fact.id),"Duplicate fact "+fact.id,errors);
        var ids=new HashSet<string>();
        foreach(var group in new[]{rules.truths,rules.swordRoutes,rules.pacts,rules.dialogueVariants})foreach(var rule in group){Check(ids.Add(rule.id),"Duplicate rule "+rule.id,errors);foreach(var list in new[]{rule.all,rule.any})if(list!=null)foreach(var key in list)Check(known.Contains(key.TrimStart('!')),"Unknown condition "+key+" in "+rule.id,errors);}
        int combinations=0;
        for(int s=1;s<=3;s++)for(int p=1;p<=3;p++){
            var a=Prepared();Check(a.SetPact(p),"Cannot agree before sword P"+p,errors);
            Check(a.TakeSword(s),"Cannot take sword S"+s,errors);
            Check(a.pact==p,"Sword resets pact",errors);
            Check(rules.Dialogue("afterSword",a).Contains("说定")||rules.Dialogue("afterSword",a).Contains("刚才说好"),"Existing pact not acknowledged S"+s+"P"+p,errors);
            Check(a.CanLeave,"Cannot depart S"+s+"P"+p,errors);
            Check(!a.TakeSword(s),"Duplicate sword",errors);
            var b=Prepared();Check(b.TakeSword(s)&&b.SetPact(p)&&b.CanLeave,"Sword-first order fails",errors);combinations++;
        }
        var minimal=new InnStoryState{awake=true,opened=true};Check(minimal.TakeSword(3)&&minimal.SetPact(1)&&minimal.CanLeave,"Minimal unknown-order route blocked",errors);
        var locked=new InnStoryState();Check(!locked.TakeSword(3)&&!locked.SetPact(1)&&!locked.CanLeave,"Premature exit",errors);
        var complete=new InnStoryState();var reversed=new InnStoryState();
        foreach(var fact in rules.factCatalog){complete.Know(fact.id);complete.Know(fact.id);}
        for(int i=rules.factCatalog.Length-1;i>=0;i--)reversed.Know(rules.factCatalog[i].id);
        Check(complete.TruthCount==8&&reversed.TruthCount==8,"Truth count/order",errors);
        complete.MarkChoice("boss.incident");complete.MarkChoice("boss.incident");Check(complete.readChoices.Count==1,"Read choice dedup",errors);
        var copy=JsonUtility.FromJson<InnStoryState>(JsonUtility.ToJson(complete));Check(copy.TruthCount==8&&copy.facts.Count==complete.facts.Count&&copy.history.Count==complete.history.Count&&copy.ChoiceWasRead("boss.incident"),"Save roundtrip",errors);
        var medicine=new InnStoryState();medicine.Know("poison");medicine.Know("meal");medicine.Know("treatment");Check(!medicine.Confirmed(3),"Healing alone identifies culprit",errors);
        if(errors.Count>0)throw new InvalidOperationException(string.Join("\n",errors));
        return "PASS: config references; 9 combinations × both pact/sword orders; minimal route; duplicate guards; 8 truths × both evidence orders; save/history roundtrip.";
    }
    static InnStoryState Prepared(){var s=new InnStoryState{awake=true,opened=true,consent=true};foreach(var id in new[]{"past","ledger","incident","concern","orderRead"})s.Know(id);return s;}
}

public static class InnNarrativeDiagnostics
{
    [MenuItem("Tools/Inn Demo/Print Narrative Snapshot")]
    public static void Print(){var demo=UnityEngine.Object.FindFirstObjectByType<InnDemo>();if(demo==null){Debug.Log("Enter Play mode first.");return;}Debug.Log(JsonUtility.ToJson(demo.story,true));}
}
