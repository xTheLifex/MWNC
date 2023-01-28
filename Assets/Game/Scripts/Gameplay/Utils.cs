using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;
using System.IO;

public static class Utils
{
    public static bool Contains(this Table t, DynValue value)
    {
        foreach(DynValue item in t.Keys)
        {
            if (item == value)
                return true;
        }
        return false;
    }

    public static bool Contains(this Table t, string value)
    {
        foreach(DynValue item in t.Keys)
        {
            if (item.String == value)
                return true;
        }
        return false;
    }
}