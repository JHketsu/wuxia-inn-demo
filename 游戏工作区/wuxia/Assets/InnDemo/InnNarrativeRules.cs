using System;
using UnityEngine;

[Serializable] public class InnRule
{
    public string id, label, group, text;
    public string[] all, any;
    public bool Matches(InnStoryState state) { if(all!=null)foreach(var key in all)if(!state.Condition(key))return false;return true; }
    public bool MatchesAny(InnStoryState state) {if(any!=null)foreach(var key in any)if(state.Condition(key))return true;return false;}
}
[Serializable] public class InnFactDefinition {public string id,label,source,kind;}
[Serializable] public class InnNarrativeRules
{
    public int version;
    public InnFactDefinition[] factCatalog;
    public InnRule[] truths, swordRoutes, pacts, dialogueVariants;
    static InnNarrativeRules cached;
    public static void Reload(){cached=null;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void ResetOnPlay(){Reload();}
    public static InnNarrativeRules Current {get {if(cached==null){var asset=Resources.Load<TextAsset>("InnNarrativeRules");if(asset==null)throw new InvalidOperationException("Missing InnNarrativeRules.json");cached=JsonUtility.FromJson<InnNarrativeRules>(asset.text);}return cached;}}
    public InnRule Find(InnRule[] rules,string id){foreach(var rule in rules)if(rule.id==id)return rule;return null;}
    public string Dialogue(string group,InnStoryState state){foreach(var rule in dialogueVariants)if(rule.group==group&&rule.Matches(state))return rule.text;return "";}
}
