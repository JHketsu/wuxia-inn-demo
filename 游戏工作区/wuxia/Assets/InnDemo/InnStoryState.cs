using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InnStoryState
{
    public List<string> facts = new();
    public List<string> readChoices = new();
    public bool ChoiceWasRead(string id)=>readChoices!=null&&readChoices.Contains(id);
    public void MarkChoice(string id){readChoices??=new List<string>();if(!readChoices.Contains(id)){readChoices.Add(id);Record("已选择："+id);}}
    public List<string> history = new();
    public void Record(string entry){history??=new List<string>();history.Add(entry);if(history.Count>120)history.RemoveAt(0);}
    public bool awake, antidote, bandaged, sword, opened, broken, won, assisted;
    public bool accused, corrected, threatened, consent, orderOwned, orderReturned;
    public int swordOutcome, pact;
    public int layoutVersion;
    public Vector3 playerPosition = new(-5,4.5f,9.3f), companionPosition = new(-5,.29f,1);
    public bool Has(string id) => facts!=null && facts.Contains(id);
    public void Know(string id) { if(!Has(id)){facts??=new List<string>();facts.Add(id);Record("获得信息："+id);} }
    public bool Condition(string key){if(key.StartsWith("!"))return !Condition(key.Substring(1));return key switch {"awake"=>awake,"sword"=>sword,"consent"=>consent,"pactAgreed"=>pact>0,"swordS3"=>swordOutcome==3,_=>Has(key)};}
    public bool Confirmed(int n){var r=InnNarrativeRules.Current;var rule=r.Find(r.truths,"T"+n.ToString("00"));return rule!=null&&rule.Matches(this);}
    public bool Suspected(int n){var r=InnNarrativeRules.Current;var rule=r.Find(r.truths,"T"+n.ToString("00"));return rule!=null&&rule.MatchesAny(this);}
    public int TruthCount { get { int c=0;for(int i=1;i<=8;i++)if(Confirmed(i))c++;return c; } }
    public bool CanSword(int route){var r=InnNarrativeRules.Current;var rule=r.Find(r.swordRoutes,"S"+route);return rule!=null&&rule.Matches(this);}
    public bool TakeSword(int route){if(!CanSword(route))return false;sword=true;swordOutcome=route;return true;}
    public bool CanPact(int route){var r=InnNarrativeRules.Current;var rule=r.Find(r.pacts,"P"+route);return rule!=null&&rule.Matches(this);}
    public bool SetPact(int route){if(!CanPact(route))return false;pact=route;return true;}
    public bool CanLeave => awake && sword && pact>0 && opened;
}
