using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

public static class CSGenerator
{
    static Type[] Classes;
	public static Action<string> Log, LogError;
    static StringBuilder sb;
    public static Type type;
    public static string thisClassName;

    public static void OnBegin()
    {
        GeneratorHelp.ClearTypeInfo();

        if (Directory.Exists(JSBindingSettings.csGenDir))
        {
            string[] files = Directory.GetFiles(JSBindingSettings.csGenDir);
            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i]);
            }
        }
        else
        {
            Directory.CreateDirectory(JSBindingSettings.csGenDir);
        }
    }
    public static void OnEnd()
    {

    }
    public static string SharpKitTypeName(Type type)
    {
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
        else if (type.IsGenericType)
        {
            name = type.Name;
            Type[] ts = type.GetGenericArguments();
            for (int i = 0; i < ts.Length; i++)
            {
                name += "$" + SharpKitTypeName(ts[i]);
            }
        }
        else
		{
			if (type == typeof(UnityEngine.Object))
				name = "UE" + type.Name;
			else
            	name = type.Name;
        }
        return name;

    }
    public static string SharpKitMethodName(string methodName, ParameterInfo[] paramS, bool overloaded, int TCounts = 0)
    {
        string name = methodName;
        if (overloaded)
        {
            if (TCounts > 0)
                name += "T" + TCounts.ToString();
            for (int i = 0; i < paramS.Length; i++)
            {
                Type type = paramS[i].ParameterType;
                name += "$$" + SharpKitTypeName(type);
            }
            name = name.Replace("`", "T");
        }
        name = name.Replace("$", "_");
        return name;
    }
    public static StringBuilder BuildFields(Type type, FieldInfo[] fields, int[] fieldsIndex, ClassCallbackNames ccbn)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < fields.Length; i++)
        {
            //var sbCall = new StringBuilder();

            FieldInfo field = fields[i];
            bool isDelegate = JSDataExchangeEditor.IsDelegateDerived(field.FieldType);// (typeof(System.Delegate).IsAssignableFrom(field.FieldType));
            if (isDelegate)
            {
                sb.Append(JSDataExchangeEditor.Build_DelegateFunction(type, field, field.FieldType, i, 0));
            }
            bool bGenericT = type.IsGenericTypeDefinition;
            if (bGenericT)
            {
                sb.AppendFormat("public static FieldID fieldID{0} = new FieldID(\"{1}\");\n", i, field.Name);
            }


            JSDataExchangeEditor.MemberFeature features = 0;
            if (field.IsStatic) features |= JSDataExchangeEditor.MemberFeature.Static;

            StringBuilder sbt = null;
            if (bGenericT)
            {
                sbt = new StringBuilder();

                sbt.AppendFormat("    FieldInfo member = GenericTypeCache.getField(vc.csObj.GetType(), fieldID{0}); \n", i);
                sbt.AppendFormat("    if (member == null) return;\n");
                sbt.Append("\n");
            }

            string functionName = JSNameMgr.HandleFunctionName(type.Name + "_" + field.Name);
            sb.AppendFormat("static void {0}(JSVCall vc)\n[[\n", functionName);

            if (bGenericT)
            {
                sb.Append(sbt);
            }

            bool bReadOnly = (field.IsInitOnly || field.IsLiteral);
            if (!bReadOnly)
            {
                sb.Append("    if (vc.bGet) [[\n");
            }

            // Debug.Log("FIELD " + type.Name + "." + field.Name);

            sb.Append(JSDataExchangeEditor.BuildCallString(type, field, "" /* argList */,
                                features | JSDataExchangeEditor.MemberFeature.Get));

            sb.AppendFormat("        {0}\n", JSDataExchangeEditor.Get_Return(field.FieldType, "result"));

            // set
            if (!bReadOnly)
            {
                sb.Append("    ]]\n    else [[\n");

                if (!isDelegate)
                {
                    var paramHandler = JSDataExchangeEditor.Get_ParamHandler(field);
                    sb.Append("        " + paramHandler.getter + "\n");

                    sb.Append(JSDataExchangeEditor.BuildCallString(type, field, "" /* argList */,
                                features | JSDataExchangeEditor.MemberFeature.Set, paramHandler.argName));
                }
                else
                {
                    var getDelegateFuncitonName = JSDataExchangeEditor.GetMethodArg_DelegateFuncionName(type, field.Name, i, 0);

//                     sb.Append(JSDataExchangeEditor.BuildCallString(type, field, "" /* argList */,
//                                 features | JSDataExchangeEditor.MemberFeature.Set, getDelegateFuncitonName + "(vc.getJSFunctionValue())"));

                    string getDelegate = JSDataExchangeEditor.Build_GetDelegate(getDelegateFuncitonName, field.FieldType);
                    sb.Append(JSDataExchangeEditor.BuildCallString(type, field, "" /* argList */,
                                features | JSDataExchangeEditor.MemberFeature.Set, getDelegate));
                }
                sb.Append("    ]]\n");
            }

            sb.AppendFormat("]]\n");
            ccbn.fields.Add(functionName);
        }

        return sb;
    }
    public static StringBuilder BuildPropertiesTypeT(Type type, PropertyInfo[] properties, int[] propertiesIndex, ClassCallbackNames ccbn)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < properties.Length; i++)
        {
            var sbCall = new StringBuilder();

            PropertyInfo property = properties[i];

            bool bT = type.IsGenericTypeDefinition;
            StringBuilder sbt = null;
            if (bT)
            {
                sbt = new StringBuilder();

                sbt.AppendFormat("    PropertyInfo property = JSDataExchangeMgr.GetPropertyInfoOfGenericClass(vc.csObj.GetType(), {0}); \n",
                        propertiesIndex[i]);        // [0] methodArrIndex

                sbt.AppendFormat("    if (property == null)\n        return true;\n");
                sbt.Append("\n");

                sb.Append(sbt);
            }

            //
            // check to see if this is a indexer
            //
            ParameterInfo[] ps = property.GetIndexParameters();
            bool bIndexer = (ps.Length > 0);
            StringBuilder sbActualParam = null;
            JSDataExchangeEditor.ParamHandler[] paramHandlers = null;
            if (bIndexer)
            {
                sbActualParam = new StringBuilder();
                paramHandlers = new JSDataExchangeEditor.ParamHandler[ps.Length];
                sbActualParam.Append("[");
                for (int j = 0; j < ps.Length; j++)
                {
                    paramHandlers[j] = JSDataExchangeEditor.Get_ParamHandler(ps[j].ParameterType, j, false, false);
                    sbActualParam.AppendFormat("{0}", paramHandlers[j].argName);
                    if (j != ps.Length - 1)
                        sbActualParam.Append(", ");
                }
                sbActualParam.Append("]");
            }

            string functionName = type.Name + "_" + property.Name;
            if (bIndexer)
            {
                foreach (var p in ps)
                {
                    functionName += "_" + p.ParameterType.Name;
                }
            }
            functionName = JSNameMgr.HandleFunctionName(functionName);

            sb.AppendFormat("static void {0}(JSVCall vc)\n[[\n", functionName);

            MethodInfo[] accessors = property.GetAccessors();
            bool isStatic = accessors[0].IsStatic;

            bool bReadOnly = !property.CanWrite;
            if (bIndexer)
            {
                for (int j = 0; j < ps.Length; j++)
                {
                    sb.Append("        " + paramHandlers[j].getter + "\n");
                }
                if (bT)
                {
                    if (isStatic)
                    {
                        sbCall.AppendFormat("{0}{1}", JSNameMgr.GetTypeFullName(type), sbActualParam);
                    }
                    else
                    {
                        sbCall.AppendFormat("(({0})vc.csObj){1}", JSNameMgr.GetTypeFullName(type), sbActualParam);
                    }
                }
                else
                {
                    if (isStatic)
                    {
                        sbCall.AppendFormat("{0}{1}", JSNameMgr.GetTypeFullName(type), sbActualParam);
                    }
                    else
                    {
                        sbCall.AppendFormat("(({0})vc.csObj){1}", JSNameMgr.GetTypeFullName(type), sbActualParam);
                    }
                }
            }

            if (!bReadOnly)
            {
                sb.Append("    if (vc.bGet) [[ \n");
            }

            if (!bIndexer)
            {
                // get
                if (isStatic)
                    sbCall.AppendFormat("{0}.{1}", JSNameMgr.GetTypeFullName(type), property.Name);
                else
                    sbCall.AppendFormat("(({0})vc.csObj).{1}", JSNameMgr.GetTypeFullName(type), property.Name);
            }

            //if (type.IsValueType && !field.IsStatic)
            //    sb.AppendFormat("{0} argThis = ({0})vc.csObj;", type.Name);

            sb.AppendFormat("        {0}", JSDataExchangeEditor.Get_Return(property.PropertyType, sbCall.ToString()));
            if (!bReadOnly)
            {
                sb.Append("\n    ]]\n");
            }

            // set
            if (!bReadOnly)
            {
                sb.Append("    else [[\n");

                int ParamIndex = ps.Length;

                var paramHandler = JSDataExchangeEditor.Get_ParamHandler(property.PropertyType, ParamIndex, false, false);
                sb.Append("        " + paramHandler.getter + "\n");

                if (bIndexer)
                {
                    if (isStatic)
                        sb.AppendFormat("{0} = {1};\n", sbCall, paramHandler.argName);
                    else
                    {
                        if (type.IsValueType)
                        {
                            sb.AppendFormat("        {0} argThis = ({0})vc.csObj;\n", JSNameMgr.GetTypeFullName(type));
                            sb.AppendFormat("argThis{0} = {1};", sbActualParam, paramHandler.argName);
                            sb.Append("        JSMgr.changeJSObj(vc.jsObjID, argThis);\n");
                        }
                        else
                        {
                            sb.AppendFormat("        {0} = {1};\n", sbCall, paramHandler.argName);
                        }
                    }
                }
                else
                {
                    if (isStatic)
                        sb.AppendFormat("{0}.{1} = {2};\n", JSNameMgr.GetTypeFullName(type), property.Name, paramHandler.argName);
                    else
                    {
                        if (type.IsValueType)
                        {
                            sb.AppendFormat("        {0} argThis = ({0})vc.csObj;\n", JSNameMgr.GetTypeFullName(type));
                            sb.AppendFormat("        argThis.{0} = {1};\n", property.Name, paramHandler.argName);
                            sb.Append("        JSMgr.changeJSObj(vc.jsObjID, argThis);\n");
                        }
                        else
                        {
                            sb.AppendFormat("        (({0})vc.csObj).{1} = {2};\n", JSNameMgr.GetTypeFullName(type), property.Name, paramHandler.argName);
                        }
                    }
                }
                sb.Append("    ]]\n");
            }

            sb.AppendFormat("]]\n");

            ccbn.properties.Add(functionName);
        }
        return sb;
    }    

    public static StringBuilder BuildProperties(Type type, PropertyInfo[] properties, int[] propertiesIndex, ClassCallbackNames ccbn)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < properties.Length; i++)
        {
            var sbCall = new StringBuilder();

            PropertyInfo property = properties[i];
            MethodInfo[] accessors = property.GetAccessors();
            bool isStatic = accessors[0].IsStatic;
            JSDataExchangeEditor.MemberFeature features = 0;
            if (isStatic) features |= JSDataExchangeEditor.MemberFeature.Static;

            bool bGenericT = type.IsGenericTypeDefinition;
            StringBuilder sbt = null;

            bool isDelegate = JSDataExchangeEditor.IsDelegateDerived(property.PropertyType); ;// (typeof(System.Delegate).IsAssignableFrom(property.PropertyType));
            if (isDelegate)
            {
                sb.Append(JSDataExchangeEditor.Build_DelegateFunction(type, property, property.PropertyType, i, 0));
            }

            // PropertyID
            if (bGenericT)
            {
                cg.args arg = new cg.args();
                arg.AddFormat("\"{0}\"", property.Name);

                arg.AddFormat("\"{0}\"", property.PropertyType.Name);
                if (property.PropertyType.IsGenericParameter)
                {
                    arg.Add("TypeFlag.IsT");
                }
                else
                {
                    arg.Add("TypeFlag.None");
                }

                cg.args arg1 = new cg.args();
                cg.args arg2 = new cg.args();

                foreach (ParameterInfo p in property.GetIndexParameters())
                {
                    cg.args argFlag = ParameterInfo2TypeFlag(p);

                    arg1.AddFormat("\"{0}\"", p.ParameterType.Name);                    
                    arg2.Add(argFlag.Format(cg.args.ArgsFormat.Flag));
                }

                if (arg1.Count > 0)
                    arg.AddFormat("new string[]{0}", arg1.Format(cg.args.ArgsFormat.Brace));
                else
                    arg.Add("null");
                if (arg2.Count > 0)
                    arg.AddFormat("new TypeFlag[]{0}", arg2.Format(cg.args.ArgsFormat.Brace));
                else
                    arg.Add("null");
                sb.AppendFormat("public static PropertyID propertyID{0} = new PropertyID({1});\n", i, arg.ToString());
            }

            if (bGenericT)
            {
                sbt = new StringBuilder();
                sbt.AppendFormat("    PropertyInfo member = GenericTypeCache.getProperty(vc.csObj.GetType(), propertyID{0}); \n", i);
                sbt.AppendFormat("    if (member == null) return;\n");
                sbt.Append("\n");
            }

            //
            // check to see if this is a indexer
            //
            ParameterInfo[] ps = property.GetIndexParameters();
            bool bIndexer = (ps.Length > 0);
            if (bIndexer) features |= JSDataExchangeEditor.MemberFeature.Indexer;
            cg.args argActual = new cg.args();
            JSDataExchangeEditor.ParamHandler[] paramHandlers = new JSDataExchangeEditor.ParamHandler[ps.Length];
            for (int j = 0; j < ps.Length; j++)
            {
                paramHandlers[j] = JSDataExchangeEditor.Get_ParamHandler(ps[j].ParameterType, j, false, false);
                argActual.Add(paramHandlers[j].argName);
            }

            string functionName = type.Name + "_" + property.Name;
            if (bIndexer)
            {
                foreach (var p in ps)
                {
                    functionName += "_" + p.ParameterType.Name;
                }
            }
            functionName = JSNameMgr.HandleFunctionName(functionName);

            sb.AppendFormat("static void {0}(JSVCall vc)\n[[\n", functionName);

            if (bGenericT)
            {
                sb.Append(sbt);
            }
            for (int j = 0; j < ps.Length; j++)
            {
                sb.Append("        " + paramHandlers[j].getter + "\n");
            }

            bool bReadOnly = (!property.CanWrite || property.GetSetMethod() == null);
            sbCall.Append(JSDataExchangeEditor.BuildCallString(type, property, argActual.Format(cg.args.ArgsFormat.OnlyList), 
                                features | JSDataExchangeEditor.MemberFeature.Get));

            if (!bReadOnly)
            {
                sb.Append("    if (vc.bGet)\n");
                sb.Append("    [[ \n");
            }

            //if (type.IsValueType && !field.IsStatic)
            //    sb.AppendFormat("{0} argThis = ({0})vc.csObj;", type.Name);

            if (property.CanRead)
            {
                if (property.GetGetMethod() != null)
                {
                    sb.Append(sbCall);
                    sb.AppendFormat("        {0}\n", JSDataExchangeEditor.Get_Return(property.PropertyType, "result"));
                }
                else
                {
					Log(type.Name + "." + property.Name + " 'get' is ignored because it's not public.");
                }
            }
            if (!bReadOnly)
            {
                sb.Append("    ]]\n");
            }

            // set
            if (!bReadOnly)
            {
                sb.Append("    else\n");
                sb.Append("    [[ \n");

                if (!isDelegate)
                {
                    int ParamIndex = ps.Length;

                    var paramHandler = JSDataExchangeEditor.Get_ParamHandler(property.PropertyType, ParamIndex, false, false);
                    sb.Append("        " + paramHandler.getter + "\n");

                    sb.Append(JSDataExchangeEditor.BuildCallString(type, property, argActual.Format(cg.args.ArgsFormat.OnlyList),
                                    features | JSDataExchangeEditor.MemberFeature.Set, paramHandler.argName));
                }
                else
                {
                    var getDelegateFuncitonName = JSDataExchangeEditor.GetMethodArg_DelegateFuncionName(type, property.Name, i, 0);

                    //                     sb.Append(JSDataExchangeEditor.BuildCallString(type, field, "" /* argList */,
                    //                                 features | JSDataExchangeEditor.MemberFeature.Set, getDelegateFuncitonName + "(vc.getJSFunctionValue())"));

                    string getDelegate = JSDataExchangeEditor.Build_GetDelegate(getDelegateFuncitonName, property.PropertyType);
                    sb.Append(JSDataExchangeEditor.BuildCallString(type, property, "" /* argList */,
                                features | JSDataExchangeEditor.MemberFeature.Set, getDelegate));
                }
                sb.Append("    ]]\n");
            }

            sb.AppendFormat("]]\n");

            ccbn.properties.Add(functionName);
        }
        return sb;
    }
    
    static StringBuilder GenListCSParam2(ParameterInfo[] ps)
    {
        StringBuilder sb = new StringBuilder();

        string fmt = "new JSVCall.CSParam({0}, {1}, {2}, {3}{4}, {5}), ";
        for (int i = 0; i < ps.Length; i++)
        {
            ParameterInfo p = ps[i];
            Type t = p.ParameterType;
            sb.AppendFormat(fmt, t.IsByRef ? "true" : "false", p.IsOptional ? "true" : "false", t.IsArray ? "true" : "false", "typeof(" + JSNameMgr.GetTypeFullName(t) + ")", t.IsByRef ? ".MakeByRefType()" : "", "null");
        }
        fmt = "new JSVCall.CSParam[][[{0}]]";
        StringBuilder sbX = new StringBuilder();
        sbX.AppendFormat(fmt, sb);
        return sbX;
    }
    public static StringBuilder BuildSpecialFunctionCall(ParameterInfo[] ps, string className, string methodName, bool bStatic, bool returnVoid, Type returnType)
    {
        StringBuilder sb = new StringBuilder();
        var paramHandlers = new JSDataExchangeEditor.ParamHandler[ps.Length];
        for (int i = 0; i < ps.Length; i++)
        {
            paramHandlers[i] = JSDataExchangeEditor.Get_ParamHandler(ps[i], i);
            sb.Append("    " + paramHandlers[i].getter + "\n");
        }

        string strCall = string.Empty;

        // must be static
        if (methodName == "op_Addition")
            strCall = paramHandlers[0].argName + " + " + paramHandlers[1].argName;
        else if (methodName == "op_Subtraction")
            strCall = paramHandlers[0].argName + " - " + paramHandlers[1].argName;
        else if (methodName == "op_Multiply")
            strCall = paramHandlers[0].argName + " * " + paramHandlers[1].argName;
        else if (methodName == "op_Division")
            strCall = paramHandlers[0].argName + " / " + paramHandlers[1].argName;
        else if (methodName == "op_Equality")
            strCall = paramHandlers[0].argName + " == " + paramHandlers[1].argName;
        else if (methodName == "op_Inequality")
            strCall = paramHandlers[0].argName + " != " + paramHandlers[1].argName;

        else if (methodName == "op_UnaryNegation")
            strCall = "-" + paramHandlers[0].argName;

        else if (methodName == "op_LessThan")
            strCall = paramHandlers[0].argName + " < " + paramHandlers[1].argName;
        else if (methodName == "op_LessThanOrEqual")
            strCall = paramHandlers[0].argName + " <= " + paramHandlers[1].argName;
        else if (methodName == "op_GreaterThan")
            strCall = paramHandlers[0].argName + " > " + paramHandlers[1].argName;
        else if (methodName == "op_GreaterThanOrEqual")
            strCall = paramHandlers[0].argName + " >= " + paramHandlers[1].argName;
        else if (methodName == "op_Implicit")
            strCall = "(" + JSNameMgr.GetTypeFullName(returnType) + ")" + paramHandlers[0].argName;
        else
            LogError("Unknown special name: " + methodName);

        string ret = JSDataExchangeEditor.Get_Return(returnType, strCall);
        sb.Append("    " + ret);
        return sb;
    }
    public static StringBuilder BuildNormalFunctionCall(
        int methodTag, 
        ParameterInfo[] ps,
        string methodName, 
        bool bStatic, 
        Type returnType, 
        bool bConstructor,
        int TCount = 0)
    {
        StringBuilder sb = new StringBuilder();

        if (bConstructor)
        {
            sb.Append("    int _this = JSApi.getObject((int)JSApi.GetType.Arg);\n");
            sb.Append("    JSApi.attachFinalizerObject(_this);\n");
            sb.Append("    --argc;\n\n");
        }

        if (bConstructor)
        {
            if (type.IsGenericTypeDefinition)
            {
                // Not generic method, but is generic type
                StringBuilder sbt = new StringBuilder();

                sbt.AppendFormat("    ConstructorInfo constructor = JSDataExchangeMgr.makeGenericConstructor(typeof({0}), constructorID{1}); \n",
                        JSNameMgr.GetTypeFullName(type), methodTag);

                //sbMethodHitTest.AppendFormat("GenericTypeCache.getConstructor(typeof({0}), {2}.constructorID{1});\n", JSNameMgr.GetTypeFullName(type), methodTag, JSNameMgr.GetTypeFileName(type));

                sbt.AppendFormat("    if (constructor == null) return true;\n");
                sbt.Append("\n");

                sb.Append(sbt);
            }
        }
        
        else if (TCount > 0)
        {
            StringBuilder sbt = new StringBuilder();
            sbt.Append("    // Get generic method by name and param count.\n");

            if (!bStatic) // instance method
            {
                sbt.AppendFormat("    MethodInfo method = JSDataExchangeMgr.makeGenericMethod(vc.csObj.GetType(), methodID{0}, {1}); \n",
                    methodTag,
                    TCount);
            }
            else // static method
            {
                sbt.AppendFormat("    MethodInfo method = JSDataExchangeMgr.makeGenericMethod(typeof({0}), methodID{1}, {2}); \n",
                    JSNameMgr.GetTypeFullName(type),
                    methodTag,
                    TCount);
            }
            sbt.AppendFormat("    if (method == null) return true;\n");
            sbt.Append("\n");

            sb.Append(sbt);
        }
        else if (type.IsGenericTypeDefinition)
        {
            // not generic method, but is generic type
            StringBuilder sbt = new StringBuilder();
            sbt.Append("    // Get generic method by name and param count.\n");

            if (!bStatic) // instance method
            {
                sbt.AppendFormat("    MethodInfo method = GenericTypeCache.getMethod(vc.csObj.GetType(), methodID{0}); \n", methodTag);
            }
            else // static method
            {
                // Debug.LogError("=================================ERROR");
                sbt.AppendFormat("    MethodInfo method = GenericTypeCache.getMethod(typeof({0}), methodID{1}); \n",
                    JSNameMgr.GetTypeFullName(type), // [0]
                    methodTag);
            }
            sbt.AppendFormat("    if (method == null) return true;\n");
            sbt.Append("\n");

            sb.Append(sbt);
        }
        else if (type.IsGenericType)
        {
            /////////////////////
            /// ERROR ///////////
            /////////////////////
        }

        var paramHandlers = new JSDataExchangeEditor.ParamHandler[ps.Length];        
        for (int i = 0; i < ps.Length; i++)
        {
            if (true /* !ps[i].ParameterType.IsGenericParameter */ )
            {
                // use original method's parameterinfo
                if (!JSDataExchangeEditor.IsDelegateDerived(ps[i].ParameterType))
                    paramHandlers[i] = JSDataExchangeEditor.Get_ParamHandler(ps[i], i);
//                if (ps[i].ParameterType.IsGenericParameter)
//                {
                //                    paramHandlers[i].getter = "    JSMgr.datax.setTemp(method.GetParameters()[" + i.ToString() + "].ParameterType);\n" + paramHandlers[i].getter;
//                }
            }
        }

        // minimal params needed
        int minNeedParams = 0;
        for (int i = 0; i < ps.Length; i++)
        {
            if (ps[i].IsOptional) { break; }
            minNeedParams++;
        }

        
        if (bConstructor && type.IsGenericTypeDefinition)
            sb.AppendFormat("    int len = argc - {0};\n", type.GetGenericArguments().Length);
        else if (TCount == 0)
            sb.AppendFormat("    int len = argc;\n");
        else
            sb.AppendFormat("    int len = argc - {0};\n", TCount);

        for (int j = minNeedParams; j <= ps.Length; j++)
        {
            StringBuilder sbGetParam = new StringBuilder();
            StringBuilder sbActualParam = new StringBuilder();
            StringBuilder sbUpdateRefParam = new StringBuilder();

            // receive arguments first
            for (int i = 0; i < j; i++)
            {
                ParameterInfo p = ps[i];
                //if (typeof(System.Delegate).IsAssignableFrom(p.ParameterType))
                if (JSDataExchangeEditor.IsDelegateDerived(p.ParameterType))
                {
                    //string delegateGetName = JSDataExchangeEditor.GetFunctionArg_DelegateFuncionName(className, methodName, methodIndex, i);
                    string delegateGetName = JSDataExchangeEditor.GetMethodArg_DelegateFuncionName(type, methodName, methodTag, i);

                    //if (p.ParameterType.IsGenericType)
                    if (p.ParameterType.ContainsGenericParameters)
                    {
                        // cg.args ta = new cg.args();
                        // sbGetParam.AppendFormat("foreach (var a in method.GetParameters()[{0}].ParameterType.GetGenericArguments()) ta.Add();");


                        sbGetParam.AppendFormat("object arg{0} = JSDataExchangeMgr.GetJSArg<object>(()=>[[\n", i);
                        sbGetParam.AppendFormat("    if (JSApi.isFunctionS((int)JSApi.GetType.Arg)) [[\n");
                        sbGetParam.AppendFormat("        var getDelegateFun{0} = typeof({1}).GetMethod(\"{2}\").MakeGenericMethod\n", i, thisClassName, delegateGetName);
                        sbGetParam.AppendFormat("            (method.GetParameters()[{0}].ParameterType.GetGenericArguments());\n", i);
                        sbGetParam.AppendFormat("        return getDelegateFun{0}.Invoke(null, new object[][[{1}]]);\n", i, "JSApi.getFunctionS((int)JSApi.GetType.Arg)");
                        sbGetParam.Append("    ]]\n");
                        sbGetParam.Append("    else\n");
                        sbGetParam.AppendFormat("        return JSMgr.datax.getObject((int)JSApi.GetType.Arg);\n");
                        sbGetParam.Append("]]);\n");


//                         sbGetParam.AppendFormat("        var getDelegateFun{0} = typeof({1}).GetMethod(\"{2}\").MakeGenericMethod\n", i, thisClassName, delegateGetName);
//                         sbGetParam.AppendFormat("            (method.GetParameters()[{0}].ParameterType.GetGenericArguments());\n", i);
//                         sbGetParam.AppendFormat("        object arg{0} = getDelegateFun{0}.Invoke(null, new object[][[{1}]]);\n", i, "vc.getJSFunctionValue()");
                    }   
                    else
                    {
                        sbGetParam.AppendFormat("        {0} arg{1} = {2};\n",
                                                JSNameMgr.GetTypeFullName(p.ParameterType), // [0]
                                                i, // [1]
                                                JSDataExchangeEditor.Build_GetDelegate(delegateGetName, p.ParameterType) // [2]
                                                );
                    }
                }
                else
                {
                    sbGetParam.Append("        " + paramHandlers[i].getter + "\n");
                }

                // value type array
                // no 'out' nor 'ref'
                if ((p.ParameterType.IsByRef || p.IsOut) && !p.ParameterType.IsArray)
                    sbActualParam.AppendFormat("{0} arg{1}{2}", (p.IsOut) ? "out" : "ref", i, (i == j - 1 ? "" : ", "));
                else
                    sbActualParam.AppendFormat("arg{0}{1}", i, (i == j - 1 ? "" : ", "));

                // updater
                sbUpdateRefParam.Append(paramHandlers[i].updater);
            }

            /*
             * 0 parameters count
             * 1 class name
             * 2 function name
             * 3 actual parameters
             */
            if (bConstructor)
            {
                StringBuilder sbCall = new StringBuilder();

                if (!type.IsGenericTypeDefinition)
                    sbCall.AppendFormat("new {0}({1})", JSNameMgr.GetTypeFullName(type), sbActualParam.ToString());
                else
                {
                    sbCall.AppendFormat("constructor.Invoke(null, new object[][[{0}]])", sbActualParam);
                }

                // string callAndReturn = JSDataExchangeEditor.Get_Return(type/*don't use returnType*/, sbCall.ToString());
                string callAndReturn = new StringBuilder().AppendFormat("        JSMgr.addJSCSRel(_this, {0});", sbCall).ToString();

                sb.AppendFormat("    {0}if (len == {1})\n", (j == minNeedParams) ? "" : "else ", j);
                sb.Append("    [[\n");
                sb.Append(sbGetParam);
                sb.Append(callAndReturn).Append("\n");
                if (sbUpdateRefParam.Length > 0) 
                    sb.Append(sbUpdateRefParam);
                sb.Append("    ]]\n");

//                 sb.AppendFormat(@"    {0}if (len == {1}) 
//     [[
// {2}
//         {3}
// {4}
//     ]]
// ",
//                  (j == minNeedParams) ? "" : "else ", // [0] else
//                  j,                  // [1] param length
//                  sbGetParam,         // [2] get param
//                  callAndReturn,      // [3] 
//                  sbUpdateRefParam);  // [4] update ref/out params
            }
            else
            {
                StringBuilder sbCall = new StringBuilder();
                StringBuilder sbActualParamT_arr = new StringBuilder();
                //StringBuilder sbUpdateRefT = new StringBuilder();

                if (TCount == 0 && !type.IsGenericTypeDefinition)
                {
                    if (bStatic)
                        sbCall.AppendFormat("{0}.{1}({2})", JSNameMgr.GetTypeFullName(type), methodName, sbActualParam.ToString());
                    else if (!type.IsValueType)
                        sbCall.AppendFormat("(({0})vc.csObj).{1}({2})", JSNameMgr.GetTypeFullName(type), methodName, sbActualParam.ToString());
                    else
                        sbCall.AppendFormat("argThis.{0}({1})", methodName, sbActualParam.ToString());
                }
                else
                {
                    if (ps.Length > 0)
                    {
                        sbActualParamT_arr.AppendFormat("object[] arr_t = new object[][[ {0} ]];", sbActualParam);
                        // reflection call doesn't need out or ref modifier
                        sbActualParamT_arr.Replace(" out ", " ").Replace(" ref ", " ");
                    }
                    else
                    {
                        sbActualParamT_arr.Append("object[] arr_t = null;");
                    }

                    if (bStatic)
                        sbCall.AppendFormat("method.Invoke(null, arr_t)");
                    else if (!type.IsValueType)
                        sbCall.AppendFormat("method.Invoke(vc.csObj, arr_t)");
                    else
                        sbCall.AppendFormat("method.Invoke(vc.csObj, arr_t)");
                }

                string callAndReturn = JSDataExchangeEditor.Get_Return(returnType, sbCall.ToString());

                StringBuilder sbStruct = null;
                if (type.IsValueType && !bStatic && TCount == 0 && !type.IsGenericTypeDefinition)
                {
                    sbStruct = new StringBuilder();
                    sbStruct.AppendFormat("{0} argThis = ({0})vc.csObj;", JSNameMgr.GetTypeFullName(type));
                }

                sb.AppendFormat("    {0}if (len == {1}) \n", (j == minNeedParams) ? "" : "else ", j);
                sb.Append("    [[\n");
                sb.Append(sbGetParam);
                if (sbActualParamT_arr.Length > 0)
                {
                    sb.Append("        ").Append(sbActualParamT_arr).Append("\n");
                }

                // if it is Struct, get argThis first
                if (type.IsValueType && !bStatic && TCount == 0 && !type.IsGenericTypeDefinition)
                {
                    sb.Append(sbStruct);
                }

                sb.Append("        ").Append(callAndReturn).Append("\n");

                // if it is Struct, update 'this' object
                if (type.IsValueType && !bStatic && TCount == 0 && !type.IsGenericTypeDefinition)
                {
                    sb.Append("        JSMgr.changeJSObj(vc.jsObjID, argThis);\n");
                }
                sb.Append(sbUpdateRefParam);
                sb.Append("    ]]\n");

//                 sb.AppendFormat(@"    {0}if (len == {1}) 
//     [[
// {2}
//         {3}
//         {4}
//         {5}
// {6}
//     ]]
// ",
//                  (j == minNeedParams) ? "" : "else ",  // [0] else
//                  j, // [1] param count
//                  sbGetParam,        // [2] get param
//                  (type.IsValueType && !bStatic && TCount == 0 && !type.IsGenericTypeDefinition) ? sbStruct.ToString() : "",  // [3] if Struct, get argThis first
//                  callAndReturn,  // [4] function call and return to js
//                  (type.IsValueType && !bStatic && TCount == 0 && !type.IsGenericTypeDefinition) ? "JSMgr.changeJSObj(vc.jsObjID, argThis);" : "",  // [5] if Struct, update 'this' object
//                  sbUpdateRefParam); // [6] update ref/out param

            }
        }

        return sb;
    }
    public static StringBuilder BuildConstructors(Type type, ConstructorInfo[] constructors, int[] constructorsIndex, ClassCallbackNames ccbn)
    {
        /*
        * methods
        * 0 function name
        * 1 list<CSParam> generation
        * 2 function call
        */
        string fmt = @"
static bool {0}(JSVCall vc, int argc)
[[
{1}
    return true;
]]
";
        StringBuilder sb = new StringBuilder();
        /*if (constructors.Length == 0 && JSBindingSettings.IsGeneratedDefaultConstructor(type) &&
            (type.IsValueType || (type.IsClass && !type.IsAbstract && !type.IsInterface)))
        {
            int olIndex = 1;
            bool returnVoid = false;
            string functionName = type.Name + "_" + type.Name +
                (olIndex > 0 ? olIndex.ToString() : "") + "";// (cons.IsStatic ? "_S" : "");
            sb.AppendFormat(fmt, functionName,
                BuildNormalFunctionCall(0, new ParameterInfo[0], type.Name, type.Name, false, returnVoid, null, true));

            ccbn.constructors.Add(functionName);
            ccbn.constructorsCSParam.Add(GenListCSParam2(new ParameterInfo[0]).ToString());        
        }*/

        // increase index if adding default constructor
//         int deltaIndex = 0;
         if (JSBindingSettings.NeedGenDefaultConstructor(type))
         {
//             deltaIndex = 1;
         }

        for (int i = 0; i < constructors.Length; i++)
        {
            ConstructorInfo cons = constructors[i];

            if (cons == null)
            {
                sb.AppendFormat("public static ConstructorID constructorID{0} = new ConstructorID({1});\n", i, "null, null");

                // this is default constructor
                //bool returnVoid = false;
                //string functionName = type.Name + "_" + type.Name + "1";
                int olIndex = i + 1; // for constuctors, they are always overloaded
                string functionName = JSNameMgr.HandleFunctionName(type.Name + "_" + type.Name + (olIndex > 0 ? olIndex.ToString() : ""));

                sb.AppendFormat(fmt, functionName,
                    BuildNormalFunctionCall(0, new ParameterInfo[0], type.Name, false, null, true));

                ccbn.constructors.Add(functionName);
                ccbn.constructorsCSParam.Add(GenListCSParam2(new ParameterInfo[0]).ToString());
            }
            else
            {
                ParameterInfo[] paramS = cons.GetParameters();
                int olIndex = i + 1; // for constuctors, they are always overloaded
                int methodTag = i/* + deltaIndex*/;

                for (int j = 0; j < paramS.Length; j++)
                {
                    if (JSDataExchangeEditor.IsDelegateDerived(paramS[j].ParameterType))
                    {
                        StringBuilder sbD = JSDataExchangeEditor.Build_DelegateFunction(type, cons, paramS[j].ParameterType, methodTag, j);
                        sb.Append(sbD);
                    }
                }

                // ConstructorID
                if (type.IsGenericTypeDefinition)
                {
                    cg.args arg = new cg.args();
                    cg.args arg1 = new cg.args();
                    cg.args arg2 = new cg.args();

                    foreach (ParameterInfo p in cons.GetParameters())
                    {
                        cg.args argFlag = ParameterInfo2TypeFlag(p);
                        arg1.AddFormat("\"{0}\"", p.ParameterType.Name);
                        arg2.Add(argFlag.Format(cg.args.ArgsFormat.Flag));
                    }

                    if (arg1.Count > 0)
                        arg.AddFormat("new string[]{0}", arg1.Format(cg.args.ArgsFormat.Brace));
                    else
                        arg.Add("null");
                    if (arg2.Count > 0)
                        arg.AddFormat("new TypeFlag[]{0}", arg2.Format(cg.args.ArgsFormat.Brace));
                    else
                        arg.Add("null");
                    sb.AppendFormat("public static ConstructorID constructorID{0} = new ConstructorID({1});\n", i, arg.ToString());
                }

                string functionName = JSNameMgr.HandleFunctionName(type.Name + "_" + type.Name + (olIndex > 0 ? olIndex.ToString() : "") + (cons.IsStatic ? "_S" : ""));

                sb.AppendFormat(fmt, functionName,
                    BuildNormalFunctionCall(methodTag, paramS, cons.Name, cons.IsStatic, null, true, 0));

                ccbn.constructors.Add(functionName);
                ccbn.constructorsCSParam.Add(GenListCSParam2(paramS).ToString());
            }
        }
        return sb;
    }
    public static StringBuilder BuildMethods(Type type, MethodInfo[] methods, int[] methodsIndex, int[] olInfo, ClassCallbackNames ccbn)
    {
        /*
        * methods
        * 0 function name
        * 1 list<CSParam> generation
        * 2 function call
        */
        string fmt = @"
static bool {0}(JSVCall vc, int argc)
[[
{1}
    return true;
]]
";
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo method = methods[i];
            ParameterInfo[] paramS = method.GetParameters();

            for (int j = 0; j < paramS.Length; j++)
            {
//                 if (paramS[j].ParameterType == typeof(DaikonForge.Tween.TweenAssignmentCallback<Vector3>))
//                 {
//                     Debug.Log("yes");
//                
                //if (typeof(System.Delegate).IsAssignableFrom(paramS[j].ParameterType))
                if (JSDataExchangeEditor.IsDelegateDerived(paramS[j].ParameterType))
                {
                    // StringBuilder sbD = JSDataExchangeEditor.BuildFunctionArg_DelegateFunction(type.Name, method.Name, paramS[j].ParameterType, i, j);
                    StringBuilder sbD = JSDataExchangeEditor.Build_DelegateFunction(type, method, paramS[j].ParameterType, i, j);

                    sb.Append(sbD);
                }
            }

            // MethodID
            if (type.IsGenericTypeDefinition || method.IsGenericMethodDefinition)
            {
                cg.args arg = new cg.args();
                arg.AddFormat("\"{0}\"", method.Name);

                arg.AddFormat("\"{0}\"", method.ReturnType.Name);
                if (method.ReturnType.IsGenericParameter)
                {
                    arg.Add("TypeFlag.IsT");
                }
                else
                {
                    arg.Add("TypeFlag.None");
                }

                cg.args arg1 = new cg.args();
                cg.args arg2 = new cg.args();

                foreach (ParameterInfo p in method.GetParameters())
                {
                    // flag of a parameter
                    cg.args argFlag = ParameterInfo2TypeFlag(p);

                    arg1.AddFormat("\"{0}\"", p.ParameterType.Name);
                    arg2.Add(argFlag.Format(cg.args.ArgsFormat.Flag));
                }

                if (arg1.Count > 0)
                    arg.AddFormat("new string[]{0}", arg1.Format(cg.args.ArgsFormat.Brace));
                else
                    arg.Add("null");
                if (arg2.Count > 0)
                    arg.AddFormat("new TypeFlag[]{0}", arg2.Format(cg.args.ArgsFormat.Brace));
                else
                    arg.Add("null");
                sb.AppendFormat("public static MethodID methodID{0} = new MethodID({1});\n", i, arg.ToString());
            }

            int olIndex = olInfo[i];
            bool returnVoid = (method.ReturnType == typeof(void));

            string functionName = type.Name + "_" + method.Name + (olIndex > 0 ? olIndex.ToString() : "") + (method.IsStatic ? "_S" : "");
            
            int TCount = 0;
            if (method.IsGenericMethodDefinition) {
                TCount = method.GetGenericArguments().Length;
            }

            // if you change functionName
            // also have to change code in 'Manual/' folder
            functionName = JSNameMgr.HandleFunctionName(type.Name + "_" + SharpKitMethodName(method.Name, paramS, true, TCount));
            if (method.IsSpecialName && method.Name == "op_Implicit" && paramS.Length > 0)
            {
                functionName += "_to_" + method.ReturnType.Name;
            }
            if (UnityEngineManual.isManual(functionName))
            {
                sb.AppendFormat(fmt, functionName, "    UnityEngineManual." + functionName + "(vc, argc);");
			}
			else if (!JSBindingSettings.IsSupportByDotNet2SubSet(functionName))
			{
				sb.AppendFormat(fmt, functionName, "    UnityEngine.Debug.LogError(\"This method is not supported by .Net 2.0 subset.\");");
			}
            else
            {
                sb.AppendFormat(fmt, functionName,

                    method.IsSpecialName ? BuildSpecialFunctionCall(paramS, type.Name, method.Name, method.IsStatic, returnVoid, method.ReturnType)
                    : BuildNormalFunctionCall(i, paramS, method.Name, method.IsStatic, method.ReturnType, 
                    false/* is constructor */, 
                    TCount));
            }

            ccbn.methods.Add(functionName);
            ccbn.methodsCSParam.Add(GenListCSParam2(paramS).ToString());
        }
        return sb;
    }
    public static StringBuilder BuildClass(Type type, StringBuilder sbFields, StringBuilder sbProperties, StringBuilder sbMethods, StringBuilder sbConstructors, StringBuilder sbRegister)
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
////////////////////// {0} ///////////////////////////////////////
// constructors
{4}
// fields
{1}
// properties
{2}
// methods
{3}

//register
{5}
";
        var sb = new StringBuilder();
        sb.AppendFormat(fmt, type.Name, sbFields.ToString(), sbProperties.ToString(), sbMethods.ToString(), sbConstructors.ToString(), sbRegister.ToString());
        return sb;
    }

    // used for record information
    public class ClassCallbackNames
    {
        // class type
        public Type type;

        public List<string> fields;
        public List<string> properties;
        public List<string> constructors;
        public List<string> methods;

        // genetated, generating CSParam code
        public List<string> constructorsCSParam;
        public List<string> methodsCSParam;
    }
    public static List<ClassCallbackNames> allClassCallbackNames;
    static StringBuilder BuildRegisterFunction(ClassCallbackNames ccbn, GeneratorHelp.ATypeInfo ti)
    {
        string fmt = @"
public static void __Register()
[[
    JSMgr.CallbackInfo ci = new JSMgr.CallbackInfo();
    ci.type = typeof({0});
    ci.fields = new JSMgr.CSCallbackField[]
    [[
{1}
    ]];
    ci.properties = new JSMgr.CSCallbackProperty[]
    [[
{2}
    ]];
    ci.constructors = new JSMgr.MethodCallBackInfo[]
    [[
{3}
    ]];
    ci.methods = new JSMgr.MethodCallBackInfo[]
    [[
{4}
    ]];
    JSMgr.allCallbackInfo.Add(ci);
]]
";
        StringBuilder sb = new StringBuilder();

        StringBuilder sbField = new StringBuilder();
        StringBuilder sbProperty = new StringBuilder();
        StringBuilder sbCons = new StringBuilder();
        StringBuilder sbMethod = new StringBuilder();

        for (int i = 0; i < ccbn.fields.Count; i++)
            sbField.AppendFormat("        {0},\n", ccbn.fields[i]);
        for (int i = 0; i < ccbn.properties.Count; i++)
            sbProperty.AppendFormat("        {0},\n", ccbn.properties[i]);
        for (int i = 0; i < ccbn.constructors.Count; i++)
        {
            if (ccbn.constructors.Count == 1 && ti.constructors.Length == 0) // no constructors   add a default  so ...
                sbCons.AppendFormat("        new JSMgr.MethodCallBackInfo({0}, '{1}'),\n", 
                    ccbn.constructors[i],
                    type.Name);
            else
                sbCons.AppendFormat("        new JSMgr.MethodCallBackInfo({0}, '{1}'),\n", 
                    ccbn.constructors[i],
                    ti.constructors[i] == null ? ".ctor" : ti.constructors[i].Name);
        }
        for (int i = 0; i < ccbn.methods.Count; i++)
        {
            // if method is not overloaded
            // don's save the cs param array
            sbMethod.AppendFormat("        new JSMgr.MethodCallBackInfo({0}, '{1}'),\n", 
                ccbn.methods[i], 
                ti.methods[i].Name);
        }

        sb.AppendFormat(fmt, JSNameMgr.GetTypeFullName(ccbn.type), sbField, sbProperty, sbCons, sbMethod);
        return sb;
    }
    public static void GenerateRegisterAll()
    {
        string fmt = @"using UnityEngine;
public class CSharpGenerated
[[
    public static void RegisterAll()
    [[
        if (JSMgr.allCallbackInfo.Count != 0)
        [[
            Debug.LogError(999777454);
        ]]
{0}
    ]]
]]
";
        StringBuilder sbA = new StringBuilder();
        for (int i = 0; i < Classes.Length; i++)
        {
            sbA.AppendFormat("        {0}Generated.__Register();\n", JSNameMgr.GetTypeFileName(Classes[i]));
        }
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat(fmt, sbA);
        HandleStringFormat(sb);

        sb.Replace("\r\n", "\n");

        string fileName = JSBindingSettings.csGenDir + "/" + "CSharpGenerated.cs";
        var writer2 = OpenFile(fileName, false);
        writer2.Write(sb.ToString());
        writer2.Close();
    }

    public static void GenerateClass()
    {
        GeneratorHelp.ATypeInfo ti;
        /*int slot = */
        GeneratorHelp.AddTypeInfo(type, out ti);
//         var sbCons = BuildConstructors(type, ti.constructors, slot);
//         var sbFields = BuildFields(type, ti.fields, slot);
//         var sbProperties = BuildProperties(type, ti.properties, slot);
//         var sbMethods = BuildMethods(type, ti.methods, slot);
//         var sbClass = BuildClass(type, sbFields, sbProperties, sbMethods, sbCons);
//         HandleStringFormat(sbClass);

        ClassCallbackNames ccbn = new ClassCallbackNames();
        {
            ccbn.type = type;
            ccbn.fields = new List<string>(ti.fields.Length);
            ccbn.properties = new List<string>(ti.properties.Length);
            ccbn.constructors = new List<string>(ti.constructors.Length);
            ccbn.methods = new List<string>(ti.methods.Length);

            ccbn.constructorsCSParam = new List<string>(ti.constructors.Length);
            ccbn.methodsCSParam = new List<string>(ti.methods.Length);
        }

        thisClassName = JSNameMgr.GetTypeFileName(type) + "Generated";

        var sbFields = BuildFields(type, ti.fields, ti.fieldsIndex, ccbn);
        var sbProperties = BuildProperties(type, ti.properties, ti.propertiesIndex, ccbn);
        var sbMethods = BuildMethods(type, ti.methods, ti.methodsIndex, ti.methodsOLInfo, ccbn);
        var sbCons = BuildConstructors(type, ti.constructors, ti.constructorsIndex, ccbn);
        var sbRegister = BuildRegisterFunction(ccbn, ti);
        var sbClass = BuildClass(type, sbFields, sbProperties, sbMethods, sbCons, sbRegister);

        /*
         * 0 typeName
         * 1 class contents
         * 2 type namespace
         */
        string fmtFile = @"
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
{2}

using jsval = JSApi.jsval;

public class {0}
[[
{1}
]]
";
        var sbFile = new StringBuilder();
        string nameSpaceString = string.Empty;
        if (type.Namespace != null)
        {
            nameSpaceString = type.Namespace;
            if (nameSpaceString == "UnityEngine"
                || nameSpaceString == "System"
                || nameSpaceString == "System.Collections"
                || nameSpaceString == "System.Collections.Generic"
                || nameSpaceString == "System.IO"
                || nameSpaceString == "System.Reflection"
                )
            {
                nameSpaceString = string.Empty;
            }
        }
        sbFile.AppendFormat(fmtFile, thisClassName, sbClass, nameSpaceString.Length > 0 ? "using " + nameSpaceString + ";" : "");
        HandleStringFormat(sbFile);

        sbFile.Replace("\r\n", "\n");

        string fileName = JSBindingSettings.csGenDir + "/" +
            JSNameMgr.GetTypeFileName(type) + 
            "Generated.cs";
        var writer2 = OpenFile(fileName, false);
        writer2.Write(sbFile.ToString());
        writer2.Close();
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
        return new StreamWriter(fileName, bAppend, Encoding.UTF8);
    }

    static void HandleStringFormat(StringBuilder sb)
    {
        sb.Replace("[[", "{");
        sb.Replace("]]", "}");
        sb.Replace("'", "\"");
    }

    public static void GenBindings(Type[] classes)
    {
        CSGenerator.Classes = classes;
        CSGenerator.OnBegin();

        allClassCallbackNames = null;
        allClassCallbackNames = new List<ClassCallbackNames>(Classes.Length);

        for (int i = 0; i < Classes.Length; i++)
        {
            CSGenerator.Clear();
            CSGenerator.type = Classes[i];
            CSGenerator.GenerateClass();
        }
        GenerateRegisterAll();
        //GenerateAllJSFileNames();

        CSGenerator.OnEnd();

        Log("Generate CS Bindings OK. total = " + Classes.Length.ToString());
    }

    public static void Type2TypeFlag(Type type, cg.args argFlag)
    {
        if (type.IsByRef)
        {
            argFlag.Add("TypeFlag.IsRef");
            type = type.GetElementType();
        }

        if (type.IsGenericParameter)
        {
            argFlag.Add("TypeFlag.IsT");
        }
        else if (type.IsGenericType)
        {
            argFlag.Add("TypeFlag.IsGenericType");
        }

        if (type.IsArray)
            argFlag.Add("TypeFlag.IsArray");
    }
    public static cg.args ParameterInfo2TypeFlag(ParameterInfo p)
    {
        cg.args argFlag = new cg.args();

        Type2TypeFlag(p.ParameterType, argFlag);

        if (p.IsOut)
            argFlag.Add("TypeFlag.IsOut");

        if (argFlag.Count == 0)
            argFlag.Add("TypeFlag.None");

        return argFlag;
    }
}
