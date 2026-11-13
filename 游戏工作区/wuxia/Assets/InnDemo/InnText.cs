using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

internal static class InnText
{
    static YarnProject project;
    static string locale;
    static readonly Dictionary<string, string> cache = new();

    internal static string SystemLocale =>
        Application.systemLanguage == SystemLanguage.English ? "en" : "zh-Hans";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset()
    {
        project = null;
        locale = null;
        cache.Clear();
    }

    internal static string Get(string id, string requestedLocale = null)
    {
        string currentLocale = requestedLocale ?? SystemLocale;
        if (locale != currentLocale)
        {
            locale = currentLocale;
            cache.Clear();
        }

        if (cache.TryGetValue(id, out string text))
            return text;

        if (project == null)
            project = Resources.Load<YarnProject>("InnDialogue/InnDialogue");

        var original = project != null ? project.GetLocalization(null) : null;
        var translated = project != null ? project.GetLocalization(locale) : null;
        string tag = "line:" + id;
        string source = original?.GetLocalizedString(tag);
        if (source == null)
        {
            Debug.LogError($"Missing inn text: {tag}");
            return id;
        }

        var lines = new List<string> { translated?.GetLocalizedString(tag) ?? source };
        for (int index = 2; ; index++)
        {
            string partTag = $"{tag}__{index}";
            string originalPart = original.GetLocalizedString(partTag);
            if (originalPart == null)
                break;
            lines.Add(translated?.GetLocalizedString(partTag) ?? originalPart);
        }

        for (int index = 0; index < lines.Count; index++)
            lines[index] = lines[index] == "␀" ? "" : lines[index].Replace('␠', ' ').Replace("\\[", "[").Replace("\\]", "]");

        text = string.Join("\n", lines);
        cache.Add(id, text);
        return text;
    }
}

public partial class InnDemo
{
    static string Copy(string id) => InnText.Get(id);
}
