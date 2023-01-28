using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;
using System.Text;
using System;

/* -------------------------------------------------------------------------- */
/*                                  GameData                                  */
/* -------------------------------------------------------------------------- */
// This class is responsible for everything that gets loaded in from the game //
// assets as well as from any mods.                                           //
/* -------------------------------------------------------------------------- */

public static class GameData
{
    public static Dictionary<string, Table> factions = new Dictionary<string, Table>();
    public static Dictionary<string, Table> locales = new Dictionary<string, Table>();
    public static string language = "ENG";

    public static void RegisterLanguage(string lang, Table table)
    {
        if (locales.ContainsKey(lang))
        {
            int i = 0;
            // We already have this language loaded. Just append the values.
            foreach(DynValue key in table.Keys)
            {
                DynValue value = table.Get(key);
                locales[lang][key] = value;
                i++;
            }

            Debug.Log($"Appended {i} definitions to '{lang}' from extra file...");
            return;
        }

        locales.Add(lang, table);
        string name = table.Get("LANG_NAME").String;
        Debug.Log($"Registered language ID '{lang}' named {name}");
    }

    public static void RegisterFaction(string id, Table table)
    {
        if (id == "" || id == null)
            return;

        if (factions.ContainsKey(id))
        {
            Debug.Log("WARNING! Attempt to register two factions to the same ID: " + id + "!");
            return;
        }
        table["id"] = id;

        if (!table.Contains("shortname"))
            table["shortname"] = id;

        factions.Add(id, table);
        string name = L(table.Get("name").String);
        Debug.Log($"Registered faction ID '{id}' named '{name}'");
    }

    public static Table GetFactionTable(string id) => factions[id];
    public static Table GetFactionTable(DynValue id) => factions[id.String];
    public static string L(string s)
    {
        if (!s.StartsWith("#"))
            return s;
        

        Table t = locales[language];
        string str = s.Replace("#", "");

        if (t.Contains(str))
        {
            DynValue v = t.Get(str);
            if (v.IsNotNil() && v.Type == DataType.String)
                str = v.String;
        }

        StringBuilder sb = new StringBuilder(str);
        // TODO: Fix values here!
        sb.Replace("$companyname", "(Replace: Ply comp name)");
        sb.Replace("$companyregion", "(Replace: Ply comp region)");
        sb.Replace("$version", GameManager.instance.version.ToString());

        return sb.ToString();
    }

    // Extensions
    public static void FillDefaults(this Script s)
    {
        // Todo: Fill with game stuff.
        s.Globals["print"] = (Action<string>) GameManager.Log;
    }

    // 0 - No errors
    // 1 - Missing localization files.
    // 2 - Game has no factions defined.
    public static int PostLoadVerify()
    {
        if (!locales.ContainsKey("ENG"))
        {
            return 1;
        }

        if (factions.Count < 1)
        {
            return 2;
        }

        // Todo: more verifications here.

        return 0;
    }
}