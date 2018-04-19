using UnityEngine;
//using UnityEditor;
using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

public static class JSNameMgr
{
    public static string GetTypeFileName(Type type)
    {
        string fullName = GetTypeFullName(type);
        return fullName.Replace('`', '_').Replace('.', '_').Replace('<', '7').Replace('>', '7').Replace(',', '_');
    }

    public static string HandleFunctionName(string functionName)
    {
        return functionName.Replace('<', '7').Replace('>', '7').Replace('`', 'A').Replace('.', '_');
    }

    public static string[] GenTSuffix = new string[] { "`1", "`2", "`3", "`4", "`5" };
    public static string[] GenTSuffixReplaceCS = new string[] { "<>", "<,>", "<,,>", "<,,,>", "<,,,,>" };
    public static string[] GenTSuffixReplaceJS = new string[] { "$1", "$2", "$3", "$4", "$5" };

    public static string GetTypeFullName(Type type, bool withT = false)
    {
        if (type == null) return "";

        if (type.IsByRef)
            type = type.GetElementType();

        if (type.IsGenericParameter)
        {  // T
            return type.Name;
        }
        else if (!type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            string rt = type.FullName;
            if (rt == null)
            {
                rt = ">>>>>>>>>>>?????????????????/";
            }
            rt = rt.Replace('+', '.');
            return rt;
        }
        else if (type.IsGenericTypeDefinition)
        {
            var t = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();
            string ret = t.FullName;
            if (!withT)
            {
                for (var i = 0; i < GenTSuffix.Length; i++)
                    ret = ret.Replace(GenTSuffix[i], GenTSuffixReplaceCS[i]);
                return ret.Replace('+', '.');
            }
            else
            {
                int length = ret.Length;
                if (length > 2 && ret[length - 2] == '`')
                {
                    ret = ret.Substring(0, length - 2);
                    Type[] ts = type.GetGenericArguments();
                    ret += "<";
                    for (int i = 0; i < ts.Length; i++)
                    {
                        ret += GetTypeFullName(ts[i]); // it's T
                        if (i != ts.Length - 1)
                        {
                            ret += ", ";
                        }
                    }
                    ret += ">";
                }
                return ret.Replace('+', '.');
            }

            // `1 or `2, `3, ...
            //            string rt = type.FullName;

            //            rt = rt.Substring(0, rt.Length - 2);
            //            rt += "<";
            //            int TCount = type.GetGenericArguments().Length;
            //            for (int i = 0; i < TCount - 1; i++)
            //            {
            //                //no space
            //                rt += ",";
            //            }
            //            rt += ">";
            //            rt = rt.Replace('+', '.');
            //            return rt;
        }
        else
        {
            string parentName = string.Empty;
            if (type.IsNested && type.DeclaringType != null)
            {
                parentName = GetTypeFullName(type.DeclaringType, withT) + ".";
            }

            string Name = type.Name;
            int length = Name.Length;
            if (length > 2 && Name[length - 2] == '`')
            {
                Name = Name.Substring(0, length - 2);
                Type[] ts = type.GetGenericArguments();
                Name += "<";
                for (int i = 0; i < ts.Length; i++)
                {
                    Name += GetTypeFullName(ts[i]); // it's T
                    if (i != ts.Length - 1)
                    {
                        Name += ", ";
                    }
                }
                Name += ">";
            }
            return (parentName + Name).Replace('+', '.');
        }
    }
    public static string GetJSTypeFullName(Type type)
    {
        if (type == null) return "";

        if (type.IsByRef)
            type = type.GetElementType();

        if (type.IsGenericParameter)
        {
            return "object";
        }
        else if (!type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            string rt = type.FullName;
            if (rt == null)
            {
                rt = ">>>>>>>>>>>?????????????????/";
            }
            rt = rt.Replace('+', '.');
            return rt;
        }
        else if (type.IsGenericTypeDefinition || type.IsGenericType)
        {
            // ATTENSION
            // typeof(List<>).FullName    == System.Collections.Generic.List`1
            // typeof(List<int>).FullName == Systcem.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]

            // `1 or `2, `3, ...
            //            string rt = string.Empty;
            //            if (type.IsGenericTypeDefinition)
            //                rt = type.FullName;
            //            else
            //                rt = type.GetGenericTypeDefinition().FullName;
            //            rt = rt.Substring(0, rt.Length - 2);
            //            int TCount = type.GetGenericArguments().Length;
            //            rt += "$" + TCount.ToString();
            //return rt;

            Type t = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();
            return t.FullName.Replace('`', '$').Replace('+', '.');
        }
        else
        {
            string fatherName = type.Name.Substring(0, type.Name.Length - 2);
            Type[] ts = type.GetGenericArguments();
            fatherName += "<";
            for (int i = 0; i < ts.Length; i++)
            {
                fatherName += ts[i].Name;
                if (i != ts.Length - 1)
                    fatherName += ", ";
            }
            fatherName += ">";
            fatherName.Replace('+', '.');
            return fatherName;
        }
    }
}