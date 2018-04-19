using UnityEngine;
//using UnityEditor;
using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using SharpKit.JavaScript;
using System.Linq;

public class JSBindingSettings
{
	public static string jsExtension = ".javascript";
	public static string jsDir = Application.streamingAssetsPath + "/JavaScript";
	public static string jsRelDir = "Assets/StreamingAssets/JavaScript";
	
	public static string jsGenFiles { get { return jsDir + "/Lib/Gen" + jsExtension; } }
	public static string csGenDir = Application.dataPath + "/JSBinding/Generated";

    public static Type[] enums = new Type[]
    {
		typeof(KeyCode),
    };

	public static Type[] classes = new Type[]
	{
		// special for JSBinding, do not delete it
		// it's for custom JSComponent
		typeof(ComponentsHelper),

        typeof(UnityEngine.Animation),
        typeof(UnityEngine.GameObject),

// #if UNITY_5_3_5
//         typeof(UnityEngine.Experimental.Director.DirectorPlayer),
// #endif
		typeof(UnityEngine.Animator),
        typeof(UnityEngine.RuntimeAnimatorController),
        typeof(UnityEngine.AnimatorOverrideController),
        typeof(UnityEngine.YieldInstruction),
		typeof(UnityEngine.WaitForSeconds),
		
		typeof(UnityEngine.Resources),
		
		typeof(UnityEngine.Application),
		typeof(UnityEngine.Behaviour),
		typeof(UnityEngine.MonoBehaviour),
		typeof(UnityEngine.Debug),
		
		typeof(UnityEngine.Input),
		typeof(UnityEngine.Object),
		typeof(UnityEngine.Component),
		typeof(UnityEngine.Transform),
		
		typeof(UnityEngine.Time),
		typeof(UnityEngine.PlayerPrefs),
		
		typeof(UnityEngine.Vector2),
		typeof(UnityEngine.Vector3),
		typeof(UnityEngine.Color),
		typeof(UnityEngine.Color32),
        typeof(UnityEngine.Events.UnityEventBase),
        typeof(UnityEngine.Events.UnityEvent),

		typeof(UnityEngine.UI.Graphic),
		typeof(UnityEngine.EventSystems.UIBehaviour),
		typeof(UnityEngine.UI.Selectable),
        typeof(UnityEngine.UI.MaskableGraphic),

        typeof(UnityEngine.UI.Button),
        typeof(UnityEngine.UI.Button.ButtonClickedEvent),
		typeof(UnityEngine.UI.Image),
		typeof(UnityEngine.UI.Text),
		typeof(UnityEngine.RectTransform),
		typeof(UnityEngine.Sprite),
	};

	public static bool IsDiscardType(Type type)
	{
		if (type.Name == "IHasXmlChildNode" ||
		    type.Name == "IGraphicEnabledDisabled")
		{
			return true;
		}

		return false;
	}
    
    // some public class members can be used
    // some of them are only in editor mode
    // some are because of unknown reason
    //
    // they can't be distinguished by code, but can be known by checking unity docs
    public static bool IsDiscard(Type type, MemberInfo memberInfo)
    {
        string memberName = memberInfo.Name;

        if (typeof(Delegate).IsAssignableFrom(type)/* && (memberInfo is MethodInfo || memberInfo is PropertyInfo || memberInfo is FieldInfo)*/)
        {
            return true;
        }

        if (memberName == "networkView" && (type == typeof(GameObject) || typeof(Component).IsAssignableFrom(type)))
        {
            return true;
        }

        if ((type == typeof(Application) && memberName == "ExternalEval") ||
                        (type == typeof(Input) && memberName == "IsJoystickPreconfigured"))
        {
            return true;
        }
            
        //
        // Temporarily commented out
        // Uncomment them yourself!!
        //
        if ((type == typeof(Motion)) ||
            (type == typeof(AnimationClip) && memberInfo.DeclaringType == typeof(Motion)) ||
            (type == typeof(Application) && memberName == "ExternalEval") ||
            (type == typeof(Input) && memberName == "IsJoystickPreconfigured") ||
            (type == typeof(AnimatorOverrideController) && memberName == "PerformOverrideClipListCleanup") ||
            (type == typeof(Caching) && (memberName == "ResetNoBackupFlag" || memberName == "SetNoBackupFlag")) ||
            (type == typeof(Light) && (memberName == "areaSize")) ||
            (type == typeof(Security) && memberName == "GetChainOfTrustValue") ||
            (type == typeof(Texture2D) && memberName == "alphaIsTransparency") ||
            (type == typeof(WebCamTexture) && (memberName == "isReadable" || memberName == "MarkNonReadable")) ||
            (type == typeof(StreamReader) && (memberName == "CreateObjRef" || memberName == "GetLifetimeService" || memberName == "InitializeLifetimeService")) ||
            (type == typeof(StreamWriter) && (memberName == "CreateObjRef" || memberName == "GetLifetimeService" || memberName == "InitializeLifetimeService")) ||
            (type == typeof(UnityEngine.Font) && memberName == "textureRebuildCallback")

             || (type == typeof(UnityEngine.EventSystems.PointerEventData) && memberName == "lastPress")
             || (type == typeof(UnityEngine.UI.InputField) && memberName == "onValidateInput") // property delegate
		    || (type == typeof(UnityEngine.UI.Graphic) && memberName == "OnRebuildRequested")
		    || (type == typeof(UnityEngine.UI.Text) && memberName == "OnRebuildRequested")

)
        {
            return true;
        }

#if UNITY_ANDROID || UNITY_IPHONE
        if (type == typeof(WWW) && (memberName == "movie"))
            return true;
#endif
        return false;
	}
	
	public static bool IsSupportByDotNet2SubSet(string functionName)
	{
		if (functionName == "Directory_CreateDirectory__String__DirectorySecurity" ||
		    functionName == "Directory_GetAccessControl__String__AccessControlSections" ||
		    functionName == "Directory_GetAccessControl__String" ||
		    functionName == "Directory_SetAccessControl__String__DirectorySecurity" ||
		    functionName == "DirectoryInfo_Create__DirectorySecurity" ||
		    functionName == "DirectoryInfo_CreateSubdirectory__String__DirectorySecurity" ||
		    functionName == "DirectoryInfo_GetAccessControl__AccessControlSections" ||
		    functionName == "DirectoryInfo_GetAccessControl" ||
		    functionName == "DirectoryInfo_SetAccessControl__DirectorySecurity" ||
		    functionName == "File_Create__String__Int32__FileOptions__FileSecurity" ||
		    functionName == "File_Create__String__Int32__FileOptions" ||
		    functionName == "File_GetAccessControl__String__AccessControlSections" ||
		    functionName == "File_GetAccessControl__String" ||
		    functionName == "File_SetAccessControl__String__FileSecurity")
		{
			return false;
		}
		return true;
	}

    public static bool NeedGenDefaultConstructor(Type type)
    {
        if (typeof(Delegate).IsAssignableFrom(type))
            return false;

        if (type.IsInterface)
            return false;

        // don't add default constructor
        // if it has non-public constructors
        // (also check parameter count is 0?)
        if (type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Length != 0)
            return false;

        //foreach (var c in type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance))
        //{
        //    if (c.GetParameters().Length == 0)
        //        return false;
        //}

        if (type.IsClass && (type.IsAbstract || type.IsInterface))
            return false;

        if (type.IsClass)
        {
            return type.GetConstructors().Length == 0;
        }
        else
        {
            foreach (var c in type.GetConstructors())
            {
                if (c.GetParameters().Length == 0)
                    return false;
            }
            return true;
        }
    }

	public static Type[] CheckClassBindings()
	{
        HashSet<Type> skips = new HashSet<Type>();
		{
			//
			// these types are defined in clrlibrary.javascript
			//
			skips.Add(typeof(System.Object));
			skips.Add(typeof(System.Exception));
			skips.Add(typeof(System.SystemException));
			skips.Add(typeof(System.ValueType));
		}

        HashSet<Type> wanted = new HashSet<Type>();
		var sb = new StringBuilder();
		bool ret = true;
		
		foreach (var type in classes)
		{
			if (typeof(System.Delegate).IsAssignableFrom(type))
			{
                sb.AppendFormat("Delegate \"{0}\" can not be exported.\n",
				                JSNameMgr.GetTypeFullName(type));
				ret = false;
			}
			
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				sb.AppendFormat("\"{0}\" is not allowed. Try \"{1}\".\n",
					JSNameMgr.GetTypeFullName(type), JSNameMgr.GetTypeFullName(type.GetGenericTypeDefinition()));
				ret = false;
			}

            if (type.IsInterface)
            {
                sb.AppendFormat("Interface \"{0}\" should not be in JSBindingSettings.classes.\n",
                    JSNameMgr.GetTypeFullName(type));
                ret = false;
            }
			
			if (wanted.Contains(type))
			{
				sb.AppendFormat("There are more than 1 \"{0}\" in JSBindingSettings.classes.\n", 
                    JSNameMgr.GetTypeFullName(type));
				ret = false;
			}
			else if (!skips.Contains(type))
			{
				wanted.Add(type);
			}
		}

		foreach (var typeb in wanted.ToArray())
		{
			Type type = typeb;

            // add base types
			Type baseType = type.BaseType;
            while (baseType != null)
            {
                if (!skips.Contains(baseType) && !wanted.Contains(baseType) &&
				    !(baseType.IsGenericType && !baseType.IsGenericTypeDefinition) &&
				    !IsDiscardType(baseType))
                {
                    wanted.Add(baseType);
                }
                baseType = baseType.BaseType;
            }
        }

        foreach (var typeb in wanted.ToArray())
        {
            Type type = typeb;
            Type[] interfaces = type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                Type ti = interfaces[i];
                string tiFullName = JSNameMgr.GetTypeFullName(ti);

                // some intefaces's name has <>, skip them
                if (!tiFullName.Contains("<") && !tiFullName.Contains(">") &&
                    !skips.Contains(ti) && !wanted.Contains(ti) &&
				    !IsDiscardType(ti))
                {
                    wanted.Add(ti);
                }
            }
        }

        Type[] arr = null;
        if (!ret)
        {
            Debug.LogError(sb);
        }
        else
        {
            arr = new Type[wanted.Count];
            wanted.CopyTo(arr);

            sb.Remove(0, sb.Length);
            sb.AppendLine("Classes to export:");
            foreach (var t in arr)
            {
                sb.AppendLine(JSNameMgr.GetTypeFullName(t));
            }
            Debug.Log(sb.ToString());
        }
        return arr;
    }
}
