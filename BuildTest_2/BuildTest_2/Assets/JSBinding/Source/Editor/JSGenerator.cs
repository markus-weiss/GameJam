using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

public static class JSGenerator
{
    static Type[] Classes, Enums;
	public static Action<string> Log, LogError;
	public static string Application_dataPath;
    // input
    static StringBuilder sb = null;
    public static Type type = null;

    static StreamWriter W;
    //static string enumFile = JSBindingSettings.jsGeneratedDir + "/enum" + JSBindingSettings.jsExtension;
    //static string tempFile = JSBindingSettings.jsDir + "/temp"+JSBindingSettings.jsExtension;

    public static void OnBegin()
    {
        GeneratorHelp.ClearTypeInfo();

//        if (Directory.Exists(JSBindingSettings.jsGeneratedDir))
//        {
//            // delete all last generated files
//            string[] files = Directory.GetFiles(JSBindingSettings.jsGeneratedDir);
//            for (int i = 0; i < files.Length; i++)
//            {
//                File.Delete(files[i]);
//            }
//        }
//        else
//        {
//            // create directory
//            Directory.CreateDirectory(JSBindingSettings.jsGeneratedDir);
//        }

		// clear generated enum files
		W = OpenFile(JSBindingSettings.jsGenFiles, false);
    }
    public static void OnEnd()
    {
        W.Close();
    }

    public static string SharpKitTypeName(Type type)
    {
        if (type == null)
            return "";
        string name = string.Empty;
        if (type.IsByRef)
        {
            name = SharpKitTypeName(type.GetElementType());
        }
        else if (type.IsArray)
        {
            while (type.IsArray)
            {
                Type subt = type.GetElementType();
                name += SharpKitTypeName(subt) + '$';
                type = subt;
            }
            name += "Array";
        }
        else if (type.IsGenericTypeDefinition)
        {
            // never come here
            name = type.Name;
        }
        else if (type.IsGenericType)
        {
            name = type.Name;
            Type[] ts = type.GetGenericArguments();

            bool hasGenericParameter = false;
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i].IsGenericParameter)
                {
                    hasGenericParameter = true;
                    break;
                }
            }

            if (!hasGenericParameter)
            {
                for (int i = 0; i < ts.Length; i++)
                {
                    name += "$" + SharpKitTypeName(ts[i]);
                }
            }
        }
        else
        {
            name = type.Name;
        }
        return name;

    }
    public static string SharpKitPropertyName(PropertyInfo property)
    {
        string name = property.Name;
        ParameterInfo[] ps = property.GetIndexParameters();
        if (ps.Length > 0)
        {
            for (int i = 0; i < ps.Length; i++)
            {
                Type type = ps[i].ParameterType;
                name += "$$" + SharpKitTypeName(type);
            }
            name = name.Replace("`", "$");
        }
        return name;
    }
    public static string SharpKitMethodName(string methodName, ParameterInfo[] paramS, bool overloaded, int TCounts = 0)
    {
//         if (!overloaded && TCounts > 0)
//         {
//             Debug.Log("");
//         }

        string name = methodName;
        if (TCounts > 0)
            name += "$" + TCounts.ToString();

        if (overloaded)
        {
            for (int i = 0; i < paramS.Length; i++)
            {
                Type type = paramS[i].ParameterType;
                name += "$$" + SharpKitTypeName(type);
            }
            name = name.Replace("`", "$");
        }
        return name;
    }
    public static string SharpKitClassName(Type type)
    {
        return JSNameMgr.GetJSTypeFullName(type);
    }

    public static StringBuilder BuildFields(Type type, FieldInfo[] fields, int slot, List<string> lstNames)
    {
//        string fmt2 = @"
// _jstype.{7}.get_{0} = function() [[ return CS.Call({1}, {3}, {4}, {5}{6}); ]]
// _jstype.{7}.set_{0} = function(v) [[ return CS.Call({2}, {3}, {4}, {5}{6}, v); ]]
// "; 
        string fmt3 = @"
_jstype.{7}.{0} =  [[ 
            get: function() [[ return CS.Call({1}, {3}, {4}, {5}{6}); ]], 
            set: function(v) [[ return CS.Call({2}, {3}, {4}, {5}{6}, v); ]] 
        ]];
";

        var sb = new StringBuilder();
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];

			lstNames.Add((field.IsStatic ? "Static_" : "") + field.Name);

            sb.AppendFormat(fmt3, 
                field.Name, // [0]
                (int)JSVCall.Oper.GET_FIELD, // [1]
                (int)JSVCall.Oper.SET_FIELD, // [2]
                slot, //[3]
                i,//[4]
                (field.IsStatic ? "true" : "false"),//[5]
                (field.IsStatic ? "" : ", this"), //[6]
                (field.IsStatic ? "staticFields" : "fields"));//[7]
        }
        return sb;
    }
	public static StringBuilder BuildProperties(Type type, PropertyInfo[] properties, int slot, List<string> lstNames)
    {
        string fmt2 = @"
_jstype.{7}.get_{0} = function({9}) [[ return CS.Call({1}, {3}, {4}, {5}{6}{8}); ]]
_jstype.{7}.set_{0} = function({10}v) [[ return CS.Call({2}, {3}, {4}, {5}{6}{8}, v); ]]
";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < properties.Length; i++)
        {
            PropertyInfo property = properties[i];
//             if (property.Name == "Item") //[] not support
//                 continue;

            ParameterInfo[] ps = property.GetIndexParameters();
            string indexerParamA = string.Empty;
            string indexerParamB = string.Empty;
            string indexerParamC = string.Empty;
            for (int j = 0; j < ps.Length; j++)
            {
                indexerParamA += "ind" + j.ToString();
                indexerParamB += "ind" + j.ToString() + ", ";
                if (j < ps.Length - 1) indexerParamA += ", ";
                indexerParamC += ", ind" + j.ToString();
            }


            MethodInfo[] accessors = property.GetAccessors();
            bool isStatic = accessors[0].IsStatic;

			string mName = SharpKitPropertyName(property);
			lstNames.Add((isStatic ? "Static_" : "") + "get_" + mName);
			lstNames.Add((isStatic ? "Static_" : "") + "set_" + mName);

            sb.AppendFormat(fmt2,
			                mName, // [0]
                (int)JSVCall.Oper.GET_PROPERTY, // [1] op
                (int)JSVCall.Oper.SET_PROPERTY, // [2] op
                slot,                           // [3]
                i,                              // [4]
                (isStatic ? "true" : "false"),  // [5] isStatic
                (isStatic ? "" : ", this"),     // [6] this
                (isStatic ? "staticDefinition" : "definition"),                // [7]
                indexerParamC, // [8]
                indexerParamA, 
                indexerParamB);
        }
        return sb;
    }
    public static StringBuilder BuildHeader(Type type)
    {
        string fmt = @"// {0}
_jstype = jst_pushOrReplace([[
    definition: [[]],
    staticDefinition: [[]],
    fields: [[]],
    staticFields: [[]],
    assemblyName: '{1}',
    Kind: '{2}',
    fullname: '{3}', {4}
    {5}
]]);
";
        string jsTypeName = JSNameMgr.GetTypeFullName(type);
        jsTypeName = jsTypeName.Replace('.', '$');

        string assemblyName = "";
        string Kind = "unknown";
        if (type.IsClass) {
			Kind = "Class";
		} else if (type.IsEnum) {
			Kind = "Enum";
		} else if (type.IsValueType) {
			Kind = "Struct";
		} else if (type.IsInterface) {
			Kind = "Interface";
		}

        string fullname = SharpKitClassName(type);
        string baseTypeName = SharpKitClassName(type.BaseType);
		Type[] interfaces = type.GetInterfaces();
		StringBuilder sbI = new StringBuilder();
		if (interfaces != null && interfaces.Length > 0)
		{
			sbI.Append("\n    interfaceNames: [");
			for (int i = 0; i < interfaces.Length; i++)
			{
				sbI.AppendFormat("\'{0}\'", SharpKitClassName(interfaces[i]));
				if (i < interfaces.Length - 1)
					sbI.Append(", ");
			}
			sbI.Append("],");
		}

        StringBuilder sb = new StringBuilder();
        sb.AppendFormat(fmt, 
            jsTypeName,   // [0]
            assemblyName, // [1]
            Kind,         // [2] 
            fullname,     // [3] full name
			sbI.ToString(), // [4] interfaceNames
            baseTypeName.Length > 0 ? "baseTypeName: '" + baseTypeName + "'" : ""); // [5] baseTypeName

        return sb;
    }
    public static StringBuilder BuildConstructors(Type type, ConstructorInfo[] constructors, int slot, int howmanyConstructors, List<string> lstNames)
    {
        string fmt = @"
_jstype.definition.{0} = function({1}) [[ CS.Call({2}); ]]";

        StringBuilder sb = new StringBuilder();
        var argActual = new cg.args();
        var argFormal = new cg.args();

        for (int i = 0; i < constructors.Length; i++)
        {
            ConstructorInfo con = constructors[i];
            ParameterInfo[] ps = con == null? new ParameterInfo[0] : con.GetParameters();

            argActual.Clear().Add(
                (int)JSVCall.Oper.CONSTRUCTOR, // OP
                slot,
                i,  // NOTICE
                "true", // IsStatics                
                "this"
                );

            argFormal.Clear();

            // add T to formal param
            if (type.IsGenericTypeDefinition)
            {
                // TODO check
                int TCount = type.GetGenericArguments().Length;
                for (int j = 0; j < TCount; j++)
                {
                    argFormal.Add("t" + j + "");
                    argActual.Add("t" + j + ".getNativeType()");
                }
            }

            //StringBuilder sbFormalParam = new StringBuilder();
            //StringBuilder sbActualParam = new StringBuilder();
            for (int j = 0; j < ps.Length; j++)
            {
                argFormal.Add("a" + j.ToString());
                argActual.Add("a" + j.ToString());
            }

			string mName = SharpKitMethodName("ctor", ps, howmanyConstructors > 1);
			lstNames.Add(mName);

            sb.AppendFormat(fmt,
                mName, // [0]
                argFormal,    // [1]
                argActual);    // [2]
        }
        return sb;
    }

    // can handle all methods
	public static StringBuilder BuildMethods(Type type, MethodInfo[] methods, int slot, List<string> lstNames)
    {
        string fmt = @"
/* {6} */
_jstype.definition.{1} = function({2}) [[ 
    {9}
    return CS.Call({7}, {3}, {4}, false, {8}{5}); 
]]";
        string fmtStatic = @"
/* static {6} {8} */
_jstype.staticDefinition.{1} = function({2}) [[ 
    {9}
    return CS.Call({7}, {3}, {4}, true{5}); 
]]";

        //bool bIsSystemObject = (type == typeof(System.Object));

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo method = methods[i];

            bool bOverloaded = ((i > 0 && method.Name == methods[i - 1].Name) ||
                (i < methods.Length - 1 && method.Name == methods[i + 1].Name));

            if (!bOverloaded)
            {
                if (GeneratorHelp.MethodIsOverloaded(type, method.Name))
                {
                    bOverloaded = true;
                    //Debug.Log("$$$ " + type.Name + "." + method.Name + (method.IsStatic ? " true" : " false"));
                }
            }

            StringBuilder sbFormalParam = new StringBuilder();
            StringBuilder sbActualParam = new StringBuilder();
            ParameterInfo[] paramS = method.GetParameters();
            StringBuilder sbInitT = new StringBuilder();
            int TCount = 0;

            // add T to formal param
            if (method.IsGenericMethodDefinition)
            {
                TCount = method.GetGenericArguments().Length;
                for (int j = 0; j < TCount; j++)
                {
                    sbFormalParam.AppendFormat("t{0}", j);
                    if (j < TCount - 1 || paramS.Length > 0)
                        sbFormalParam.Append(", ");


                    sbInitT.AppendFormat("    var native_t{0} = t{0}.getNativeType();\n", j);
                    sbActualParam.AppendFormat(", native_t{0}", j);
                }
            }

            int L = paramS.Length;
            for (int j = 0; j < L; j++)
            {
                sbFormalParam.AppendFormat("a{0}/*{1}*/{2}", j, paramS[j].ParameterType.Name, (j == L - 1 ? "" : ", "));

                ParameterInfo par = paramS[j];
                if (par.ParameterType.IsArray && par.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                {
                    sbActualParam.AppendFormat(", jsb_formatParamsArray({0}, a{0}, arguments)", j);
                }
                else
                {
                    sbActualParam.AppendFormat(", a{0}", j);
                }
            }

            //int TCount = method.GetGenericArguments().Length;

            string methodName = method.Name;
//            if (methodName == "ToString") { methodName = "toString"; }

			string mName = SharpKitMethodName(methodName, paramS, bOverloaded, TCount);
			lstNames.Add((method.IsStatic ? "Static_" : "") + mName);

            if (!method.IsStatic)
                sb.AppendFormat(fmt,
                    className,
				    mName, // [1] method name
                    sbFormalParam.ToString(),  // [2] formal param
                    slot,                      // [3] slot
                    i,                         // [4] index
                    sbActualParam,             // [5] actual param
                    method.ReturnType.Name,    // [6] return type name
                    (int)JSVCall.Oper.METHOD,  // [7] OP
                    "this",                // [8] this
                    sbInitT                    //[9] generic types init
                    );
            else
                sb.AppendFormat(fmtStatic, 
                    className,
				    mName, 
                    sbFormalParam.ToString(), 
                    slot, 
                    i, 
                    sbActualParam, 
                    method.ReturnType.Name, 
                    (int)JSVCall.Oper.METHOD, 
                    "",
                    sbInitT);
        }
        return sb;
    }
    public static StringBuilder BuildClass(Type type, StringBuilder sbFields, StringBuilder sbProperties, StringBuilder sbMethods, StringBuilder sbConstructors)
    {
        /*
        * class
        * 0 class name
        * 1 fields
        * 2 properties
        * 3 methods
        * 4 constructors
        */
        string fmt = @"
{4}

// fields
{1}
// properties
{2}
// methods
{3}
";
        var sb = new StringBuilder();
        sb.AppendFormat(fmt, className, sbFields.ToString(), sbProperties.ToString(), sbMethods.ToString(), sbConstructors.ToString());
        return sb;
    }

    public static List<string> GenerateClass()
    {
        /*if (type.IsInterface)
        {
            Debug.Log("Interface: " + type.ToString() + " ignored.");
            return;
        }*/

		List<string> memberNames = new List<string>();

        GeneratorHelp.ATypeInfo ti;
        int slot = GeneratorHelp.AddTypeInfo(type, out ti);
        var sbHeader = BuildHeader(type);
		var sbCons = sbHeader.Append(BuildConstructors(type, ti.constructors, slot, ti.howmanyConstructors, memberNames));
		var sbFields = BuildFields(type, ti.fields, slot, memberNames);
		var sbProperties = BuildProperties(type, ti.properties, slot, memberNames);
		var sbMethods = BuildMethods(type, ti.methods, slot, memberNames);
        //sbMethods.Append(BuildTail());
        var sbClass = BuildClass(type, sbFields, sbProperties, sbMethods, sbCons);
		HandleStringFormat(sbClass);
		
		
		//        string fileName = JSBindingSettings.jsGeneratedDir + "/" +
		//            JSNameMgr.GetTypeFileName(JSGenerator.type)
		//            + JSBindingSettings.jsExtension;
		//        var writer2 = OpenFile(fileName, false);
		//        writer2.Write(sbClass.ToString());
		//        writer2.Close();
		W.Write(sbClass.ToString());

		return memberNames;
    }

    static void GenerateEnum()
    {
		string fullName = JSNameMgr.GetTypeFullName(type);
		string jsTypeName = fullName.Replace('.', '$');

		var sbValues = new StringBuilder();
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static);
			string fmtField = "        {0}: {1}{2}\n";
			for (int i = 0; i < fields.Length; i++)
			{
				sbValues.AppendFormat(fmtField, fields[i].Name, (int)fields[i].GetValue(null), 
				                      i == fields.Length - 1 ? "" : ",");
            }
        }
		
		var sbDef = new StringBuilder();
		sbDef.AppendFormat(@"
jst_pushOrReplace([[
    fullname: '{1}',
    staticDefinition: [[
{2}    ]],
    Kind: 'Enum'
]]);
",
		jsTypeName, fullName, sbValues);
        
        HandleStringFormat(sbDef);
		W.Write(sbDef.ToString());
    }

    public static void Clear()
    {
        type = null;
        sb = new StringBuilder();
    }
    static void GenEnd()
    {
        string fmt = @"
]]
";
        sb.Append(fmt);
    }

    static void WriteUsingSection(StreamWriter writer)
    {
        string fmt = @"using System;
using UnityEngine;
";
        writer.Write(fmt);
    }
    static StreamWriter OpenFile(string fileName, bool bAppend = false)
    {
        // IMPORTANT
        // Bom (byte order mark) is not needed
        Encoding utf8NoBom = new UTF8Encoding(false);
        return new StreamWriter(fileName, bAppend, utf8NoBom);
    }

    static void HandleStringFormat(StringBuilder sb)
    {
        sb.Replace("[[", "{");
        sb.Replace("]]", "}");
        sb.Replace("'", "\"");
    }

    public static Dictionary<Type, string> typeClassName = new Dictionary<Type, string>();
    static string className = string.Empty;

    public static void GenBindings(Type[] types, Type[] enums)
    {
        JSGenerator.Classes = types;
        JSGenerator.Enums = enums;
        JSGenerator.OnBegin();

        // enums
        for (int i = 0; i < Enums.Length; i++)
        {
            JSGenerator.Clear();
            JSGenerator.type = Enums[i];
            JSGenerator.GenerateEnum();
        }

		// typeName -> member list
		Dictionary<string, List<string>> allDefs = new Dictionary<string, List<string>>();

        // classes
        for (int i = 0; i < Classes.Length; i++)
        {
            JSGenerator.Clear();
            JSGenerator.type = Classes[i];
            if (!typeClassName.TryGetValue(type, out className))
                className = type.Name;
            

			List<string> memberNames = JSGenerator.GenerateClass();
			allDefs.Add(SharpKitClassName(type), memberNames);
        }

        JSGenerator.OnEnd();

		StringBuilder sb = new StringBuilder();
		foreach (var KV in allDefs)
		{
			sb.AppendFormat("[{0}]\r\n", KV.Key);

			var lst = KV.Value;
			foreach (var l in lst)
			{
				sb.AppendFormat("    {0}\r\n", l);
			}
			sb.Append("\r\n");
		}

		string dir = Application_dataPath + "/Temp";
		Directory.CreateDirectory(dir);
		File.WriteAllText(dir + "/AllExportedMembers.txt", sb.ToString());

        Log("Generate JS Bindings OK. enum " + Enums.Length.ToString() + ", class " + Classes.Length.ToString());
    }
}