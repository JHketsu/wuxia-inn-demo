using UnityEngine;
using Yarn;
using Yarn.Unity;

public partial class InnDemo
{
    [SerializeField] string bossDialogueLocale = "";

    void BossTalk()
    {
        var project = Resources.Load<YarnProject>("InnDialogue/InnDialogue");
        if (project == null || project.Program == null)
        {
            Debug.LogError("The InnDialogue Yarn project could not be loaded.", this);
            return;
        }

        string locale = string.IsNullOrEmpty(bossDialogueLocale)
            ? InnText.SystemLocale
            : bossDialogueLocale;
        var localization = project.GetLocalization(locale);
        var original = project.GetLocalization(null);
        var parser = new Yarn.Markup.LineParser();
        var storage = new MemoryVariableStore();
        var dialogue = new Dialogue(storage);
        dialogue.LogErrorMessage = message => Debug.LogError(message, this);
        dialogue.SetProgram(project.Program);
        storage.SetValue("$sword", story.sword);
        storage.SetValue("$awake", story.awake);
        storage.SetValue("$wound", story.Has("wound"));
        storage.SetValue("$threatened", story.threatened);
        storage.SetValue("$poison", story.Has("poison"));
        storage.SetValue("$ledger", story.Has("ledger"));
        storage.SetValue("$accused", story.accused);
        storage.SetValue("$corrected", story.corrected);
        storage.SetValue("$appearanceConsent", story.Has("appearanceConsent"));

        LocalizedLine Localize(Line line)
        {
            string text = localization.GetLocalizedString(line.ID)
                ?? original.GetLocalizedString(line.ID) ?? line.ID;
            return new LocalizedLine
            {
                Text = parser.ParseString(Yarn.Markup.LineParser.ExpandSubstitutions(text, line.Substitutions), locale),
            };
        }

        bool waiting = false;
        bool complete = false;
        bool sword = false;
        bool firstLine = true;
        dialogue.LineHandler = line =>
        {
            var localized = Localize(line);
            string text = localized.TextWithoutCharacterName.Text;
            if (firstLine)
            {
                Speak(localized.CharacterName ?? (locale == "en" ? "Innkeeper" : "老板"), text);
                firstLine = false;
            }
            else
            {
                dialogText += "\n" + text;
            }
        };
        dialogue.CommandHandler = command =>
        {
            switch (command.Text)
            {
                case "inn_boss_choice help": Fact("help"); break;
                case "inn_boss_choice incident": Fact("incident"); break;
                case "inn_boss_choice accuse": story.accused = true; break;
                case "inn_boss_choice threaten": story.threatened = true; break;
                case "inn_boss_choice timing": Fact("bossTiming"); break;
                case "inn_boss_choice past": Fact("past"); break;
                case "inn_boss_choice correct": story.corrected = true; break;
                case "inn_boss_choice sword": sword = true; break;
                default: Debug.LogError($"Unknown inn dialogue command: {command.Text}", this); break;
            }
        };
        dialogue.DialogueCompleteHandler = () => complete = true;

        void ContinueYarn()
        {
            waiting = false;
            firstLine = true;
            do
            {
                dialogue.Continue();
            } while (!waiting && !complete && dialogue.IsActive);

            if (sword)
                SwordTalk();
            else if (complete && options.Count == 0)
                Choice(InnText.Get("story_continueconversation_01", locale), BossTalk, "navigation.continue", false);
        }

        dialogue.OptionsHandler = choices =>
        {
            waiting = true;
            options.Clear();
            foreach (var option in choices.Options)
            {
                if (!option.IsAvailable)
                    continue;
                string lineId = option.Line.ID;
                const string prefix = "line:boss_";
                string choiceId = lineId.StartsWith(prefix, System.StringComparison.Ordinal)
                    ? "boss." + lineId.Substring(prefix.Length)
                    : lineId;
                Choice(Localize(option.Line).TextWithoutCharacterName.Text, () =>
                {
                    options.Clear();
                    dialogue.SetSelectedOption(option.ID);
                    ContinueYarn();
                }, choiceId);
            }
        };

        conversation = BossTalk;
        dialogue.SetNode("BossTalk");
        ContinueYarn();
    }
}
