﻿//***************************************************************
// add by qiucw

if (typeof(window) == 'undefined')
    var window = this;

var JsTypes = [];
var JsTypesMap = {};
function jst_pushOrReplace(jst) {
    if (JsTypesMap.hasOwnProperty(jst.fullname)) {
        var i = JsTypesMap[jst.fullname];
        JsTypes[i] = jst;
    }
    else {
        JsTypes.push(jst);
        JsTypesMap[jst.fullname] = JsTypes.length - 1;
    }
    return jst;
}

function jst_find(fullname) {
    if (JsTypesMap.hasOwnProperty(fullname)) {
        var i = JsTypesMap[fullname];
        return JsTypes[i];
    }
    return undefined;
}

// add by qiucw end.
//***************************************************************

if (typeof($CreateException)=='undefined') 
{
    var $CreateException = function(ex, error) 
    {
        if(error==null)
            error = new Error();
        if(ex==null)
            ex = new System.Exception.ctor();       
        error.message = ex.message;
        for (var p in ex)
           error[p] = ex[p];
        return error;
    }
}

if (typeof ($CreateAnonymousDelegate) == 'undefined') {
    var $CreateAnonymousDelegate = function (target, func) {
        //if (target == null || func == null)
        //    return func;
        //var delegate = function () {
        //    return func.apply(target, arguments);
        //};
        //delegate.func = func;
        //delegate.target = target;
        //delegate.isDelegate = true;
        //return delegate;

        return $CreateDelegate(target, func);
    }
}

if (typeof($CreateDelegate)=='undefined'){
    if(typeof($iKey)=='undefined') var $iKey = 0;
    if(typeof($pKey)=='undefined') var $pKey = String.fromCharCode(1);
    var $CreateDelegate = function(target, func){
        if (target == null || func == null) 
            return func;
        if(func.target==target && func.func==func)
            return func;
        if (target.$delegateCache == null)
            target.$delegateCache = {};
        if (func.$key == null)
            func.$key = $pKey + String(++$iKey);
        var delegate;
        if(target.$delegateCache!=null)
            delegate = target.$delegateCache[func.$key];
        if (delegate == null){
            delegate = function(){
                return func.apply(target, arguments);
            };
            delegate.func = func;
            delegate.target = target;
            delegate.isDelegate = true;
            if(target.$delegateCache!=null)
                target.$delegateCache[func.$key] = delegate;
        }
        return delegate;
    }
}

if (typeof(Uint8Array) == "undefined")
    var Uint8Array = Array;
function $CombineDelegates(del1,del2)
{
    if(del1 == null)
        return del2;
    if(del2 == null)
        return del1;
    var del=$CreateMulticastDelegateFunction();
    del.delegates = [];
    if(del1.isMulticastDelegate)
    {
        for(var i=0;i < del1.delegates.length;i++)
            del.delegates.push(del1.delegates[i]);
    }
    else
    {
        del.delegates.push(del1);
    }
    if(del2.isMulticastDelegate)
    {
        for(var i=0;i < del2.delegates.length;i++)
            del.delegates.push(del2.delegates[i]);
    }
    else
    {
        del.delegates.push(del2);
    }
    return del;
};

function $CreateMulticastDelegateFunction()
{
    var del2 = null;
    
    var del=function()
    {
        var x=undefined;
        for(var i=0;i < del2.delegates.length;i++)
        {
            var del3=del2.delegates[i];
            x = del3.apply(null,arguments);
        }
        return x;
    };
    del.isMulticastDelegate = true;
    del2 = del;   
    
    return del;
};

function $RemoveDelegate(delOriginal,delToRemove)
{
    if(delToRemove == null || delOriginal == null)
        return delOriginal;
    if(delOriginal.isMulticastDelegate)
    {
        if(delToRemove.isMulticastDelegate)
            throw new Error("Multicast to multicast delegate removal is not implemented yet");
        var del=$CreateMulticastDelegateFunction();
        for(var i=0;i < delOriginal.delegates.length;i++)
        {
            var del2=delOriginal.delegates[i];
            if(del2 != delToRemove)
            {
                if(del.delegates == null)
                    del.delegates = [];
                del.delegates.push(del2);
            }
        }
        if(del.delegates == null)
            return null;
        if(del.delegates.length == 1)
            return del.delegates[0];
        return del;
    }
    else
    {
        if(delToRemove.isMulticastDelegate)
            throw new Error("single to multicast delegate removal is not supported");
        if(delOriginal == delToRemove)
            return null;
        return delOriginal;
    }
};


var AfterCompilationFunctions =  [];
var BeforeCompilationFunctions =  [];
var IsCompiled = false;
function RemoveDelegate(delOriginal, delToRemove){
    if (delToRemove == null || delOriginal == null)
        return delOriginal;
    if (delOriginal.isMulticastDelegate){
        if (delToRemove.isMulticastDelegate)
            throw $CreateException(new System.NotImplementedException.ctor$$String("Multicast to multicast delegate removal is not implemented yet"), new Error());
        var del = CreateMulticastDelegateFunction();
        for (var i = 0; i < delOriginal.delegates.length; i++){
            var del2 = delOriginal.delegates[i];
            if (del2 != delToRemove){
                if (del.delegates == null)
                    del.delegates =  [];
                del.delegates.push(del2);
            }
        }
        if (del.delegates == null)
            return null;
        if (del.delegates.length == 1)
            return del.delegates[0];
        return del;
    }
    else {
        if (delToRemove.isMulticastDelegate)
            throw $CreateException(new System.NotImplementedException.ctor$$String("single to multicast delegate removal is not supported"), new Error());
        if (delOriginal == delToRemove)
            return null;
        return delOriginal;
    }
};
function CombineDelegates(del1, del2){
    if (del1 == null)
        return del2;
    if (del2 == null)
        return del1;
    var del = CreateMulticastDelegateFunction();
    del.delegates =  [];
    if (del1.isMulticastDelegate){
        for (var i = 0; i < del1.delegates.length; i++)
            del.delegates.push(del1.delegates[i]);
    }
    else {
        del.delegates.push(del1);
    }
    if (del2.isMulticastDelegate){
        for (var i = 0; i < del2.delegates.length; i++)
            del.delegates.push(del2.delegates[i]);
    }
    else {
        del.delegates.push(del2);
    }
    return del;
};
function CreateMulticastDelegateFunction(){
    var del2 = null;
    var del = function (){
        var x = undefined;
        for (var i = 0; i < del2.delegates.length; i++){
            var del3 = del2.delegates[i];
            x = del3.apply(null, arguments);
        }
        return x;
    };
    del.isMulticastDelegate = true;
    del2 = del;
    return del;
};
function CreateClrDelegate(type, genericArgs, target, func){
    return JsTypeHelper.GetDelegate(target, func);
};
function Typeof(jsTypeOrName){
    if (jsTypeOrName == null)
        throw $CreateException(new Error("Unknown type."), new Error());
    if (typeof(jsTypeOrName) == "function"){
        jsTypeOrName = JsTypeHelper.GetType(jsTypeOrName);
    }
    if (typeof(jsTypeOrName) == "string")
        return System.Type.GetType$$String$$Boolean(jsTypeOrName, true);
    return System.Type._TypeOf(jsTypeOrName);
};
function JsTypeof(typeName){
    return JsTypeHelper.GetType(typeName, false);
};
function New(typeName, args){
    var type = JsTypeHelper.GetType(typeName, true);
    if (args == null || args.length == 0){
        var obj = JsCompiler.NewByFunc(type.ctor);
        return obj;
    }
    else {
        var obj = JsCompiler.NewByFuncArgs(type.ctor, args);
        return obj;
    }
};
function NewWithInitializer(type, json){
    var obj = JsCompiler.NewByFunc(type.ctor);
    if (typeof(json) == "array"){
        throw $CreateException(new System.Exception.ctor$$String("not implemented"), new Error());
    }
    else {
        for (var p in json){
            var setter = obj["set_" + p];
            if (typeof(setter) == "function")
                setter.call(obj, json[p]);
            else
                obj[p] = json[p];
        }
    }
    return obj;
};
function As(obj, typeOrName){
    if (obj == null)
        return obj;
    var type = JsTypeHelper.GetType(typeOrName, true);
    if (Is(obj, type))
        return obj;
    return null;
};
function Cast(obj, typeOrName){
    // don't need cast
    return obj;

    if (obj == null)
        return obj;
    var type = JsTypeHelper.GetType(typeOrName, true);
    if (Is(obj, type))
        return obj;
    var converted = TryImplicitConvert(obj, type);
    if (converted != null)
        return converted;
    var objTypeName = typeof(obj);
    if (typeof(obj.getTypeName) == "function"){
        objTypeName = obj.getTypeName();
    }
    var msg = new Array("InvalidCastException: Cannot cast ", objTypeName, " to ", type.fullname, "Exception generated by JsRuntime").join("");
    throw $CreateException(new Error(msg), new Error());
};
function _TestTypeInterfacesIs(testType, iface, testedInterfaces){
    if (testedInterfaces[iface.name])
        return false;
    for (var i = 0; i < testType.interfaces.length; i++){
        var testIface = testType.interfaces[i];
        if (testIface == iface)
            return true;
        testedInterfaces[testIface.name] = true;
        if (_TestTypeInterfacesIs(testIface, iface, testedInterfaces))
            return true;
    }
    return false;
};
function TypeIs(objType, type){
    if (objType == type)
        return true;
    if (type.Kind == "Interface"){
        var testedInterfaces = new Object();
        while (objType != null){
            if (objType == type)
                return true;
            if (_TestTypeInterfacesIs(objType, type, testedInterfaces))
                return true;
            objType = objType.baseType;
        }
        return false;
    }
    if (type.Kind == "Delegate" && objType.fullname == "System.Delegate"){
        return true;
    }
    if (objType.fullname == "System.Int32"){
        if (type.fullname == "System.Decimal")
            return true;
        if (type.fullname == "System.Double")
            return true;
        if (type.fullname == "System.Single")
            return true;
    }
    var t = objType.baseType;
    while (t != null){
        if (t == type)
            return true;
        t = t.baseType;
    }
    return false;
};
function Is(obj, typeOrName){
    if (obj == null){
        return false;
    }
    var type = JsTypeHelper.GetType(typeOrName, true);
    if (type == null){
        if (type == null && typeof(typeOrName) == "function"){
            var ctor = typeOrName;
            var i = 0;
            while (ctor != null && i < 20){
                if (obj instanceof ctor)
                    return true;
                ctor = ctor["$baseCtor"];
                i++;
            }
            return false;
        }
        throw $CreateException(new Error("type expected"), new Error());
    }
    var objType = GetObjectType(obj);
    if (objType == null)
        return false;
    var isIt = TypeIs(objType, type);
    return isIt;
};
function Default(T){
    return null;
};
function GetObjectType(obj){
    	var objType;	
	if(
			obj.constructor==null ||  //IE
			obj instanceof Node || //FireFox
			obj.constructor==HTMLImageElement || obj.constructor==HTMLInputElement ||								//IE & Firefox
			obj.constructor.name=='HTMLImageElement' || obj.constructor.name=='HTMLInputElement' 		//IE & Safari
		 )
	{
		var objTypeName = SharpKit.Html4.HtmlDom.GetTypeNameFromHtmlNode(obj);
		if(objTypeName==null)
			throw new Error();
		objType = JsTypeHelper.GetType(objTypeName, true);
	}
	else
	{
		objType = obj.constructor._type;
	}
	return objType === undefined ? null : objType;

};
function TryImplicitConvert(obj, type){
    	if (obj instanceof Error)
	{
		if (obj._Exception != null)
		{
			if(Is(obj._Exception, type))
				return obj._Exception;
			else
				return null;
		}
		else if (type.get_FullName() == 'System.Exception')
		{
			obj._Exception = new Exception(obj.message);
			return obj._Exception;
		}
	}
	return null;
};
function Compile(){
    JsCompiler.Compile_Direct();
};
function AfterCompilation(func){
    if (IsCompiled)
        func();
    else
        AfterCompilationFunctions.push(func);
};
function AfterNextCompilation(func){
    AfterCompilationFunctions.push(func);
};
function BeforeCompilation(func){
    BeforeCompilationFunctions.push(func);
};
var JsCompiler = function (){
};
JsCompiler.__LastException = null;
JsCompiler.Types = new Object();
JsCompiler._hashKeyIndex = 0;
JsCompiler._hashKeyPrefix = String.fromCharCode(1);
JsCompiler.Compile_Direct = function (){
    JsCompiler.Compile_Phase1();
    JsCompiler.Compile_Phase2();
    JsCompiler.Compile_Phase3();
};
JsCompiler.Compile_Phase1 = function (){
    for (var $i2 = 0,$l2 = BeforeCompilationFunctions.length,action = BeforeCompilationFunctions[$i2]; $i2 < $l2; $i2++, action = BeforeCompilationFunctions[$i2])
        action();
    BeforeCompilationFunctions =  [];
    for (var $i3 = 0,$l3 = JsTypes.length,jsType = JsTypes[$i3]; $i3 < $l3; $i3++, jsType = JsTypes[$i3]){
        var fullName = jsType.fullname;
        var type = JsCompiler.Types[fullName];
        if (type == null){
            JsCompiler.Types[fullName] = jsType;
        }
        else {
            jsType.isPartial = true;
            jsType.realType = type;
        }
        if (jsType.derivedTypes == null)
            jsType.derivedTypes =  [];
        if (jsType.interfaces == null)
            jsType.interfaces =  [];
        if (jsType.definition == null)
            jsType.definition = new Object();
        var index = fullName.lastIndexOf(".");
        if (index == -1){
            jsType.name = fullName;
        }
        else {
            jsType.name = fullName.substring(index + 1);
            jsType.ns = fullName.substring(0, index);
        }
        if (jsType.Kind == "Enum"){
            if (jsType.baseTypeName == null)
                jsType.baseTypeName = "System.Object";
            if (jsType.definition["toString"] ==  Object.prototype.toString)
                jsType.definition["toString"] = new Function("return this._Name;");
        }
        else if (jsType.Kind == "Struct"){
            if (jsType.baseTypeName == null)
                jsType.baseTypeName = "System.ValueType";
        }


		// if (jsType.Kind == "Struct" || jsType.Kind == "Class") 
		{
            // add default constructor!!
            if (jsType.definition.ctor == undefined) {
                jsType.definition.ctor = function () {};
			}

			//jsType.ctors = jsType.ctors || {};
			//jsType.ctors.ctor = jsType.ctor;

            // add a function to obtain c# Type
            jsType.definition.ctor.getNativeType = function () 
			{
                if (this.__nativeType != undefined) 
				{
                    return this.__nativeType;
                }
                if (this._type && this._type.fullname) 
				{
                    this.__nativeType = this._type.fullname;
					this.__nativeType = this.__nativeType.replace("$1", "<>")
						.replace("$2", "<,>")
						.replace("$3", "<,,>")
						.replace("$4", "<,,,>");
                } 
				else
				{
					this.__nativeType = "ERROR_unknowntype";
				}
				return this.__nativeType;
            }

            if (jsType.fields != undefined) {
				jsType.commonPrototype = jsType.definition.ctor.prototype;
				//for (var v in jsType.fields)
				//{
				//	var o = jsType.fields[v];
                 //   if (typeof o === typeof {}) {
                 //       Object.defineProperty(jsType.commonPrototype, v, o);
                 //   }
				//}
				//delete jsType.fields;
			}
		}
    }
};
JsCompiler.Compile_Phase2 = function (){
    for (var i = 0; i < JsTypes.length; i++){
        var jsType = JsTypes[i];
        JsCompiler.Compile_Phase2_TmpType(jsType);
    }
    // by qiucw
    // make fields as type's property
    for (var i = 0; i < JsTypes.length; i++){
        var jt = JsTypes[i];

        var f = jt.fields;
        if (f && jt.commonPrototype) {
            for (var v in f) {
                var o = f[v];
                //if (typeof o === typeof {}) {
                    Object.defineProperty(jt.commonPrototype, v, o);
                //}
            }
            delete jt.fields;
        }

        var sf = jt.staticFields;
        if (sf) {
            for (var p in sf) {
                var member = sf[p];
                if (typeof(member.get) == "function")
                    Object.defineProperty(jt, p, member);
                else
                    jt[p] = member;
            }
            delete jt.staticFields;
        }
    }

    for (var $i4 = 0,$l4 = JsTypes.length,ce = JsTypes[$i4]; $i4 < $l4; $i4++, ce = JsTypes[$i4]){
        if (ce.cctor != null)
            ce.cctor();
    }
    // qiucw comment
    // LinkInterfaceMethods doesn't do anything we want
    // JsCompiler.LinkInterfaceMethods();
    JsTypes =  [];
};
JsCompiler.Compile_Phase2_TmpType = function (tmpType){
    var p = tmpType.fullname;
    var type = JsCompiler.CompileType(tmpType);


    if (type != null)
        JsCompiler.CopyMemberIfNotDefined(type, type.fullname, window);


    if (type.ns != null){
        var ns = JsCompiler.ResolveNamespace(type.ns);
        if (type != null) {
			// !
			// Assume JsType = [..., A.B.C, A.B, ...]
			// After this line, A.B is re-assigned, so A.B.C is gone
            ns[type.name] = type;
		}
    }
};
JsCompiler.LinkInterfaceMethods = function (){
    for (var it = 0; it < JsTypes.length; it++){
        var jsType = JsTypes[it];
        for (var ii = 0; ii < jsType.interfaces.length; ii++){
            var intType = jsType.interfaces[ii];
            for (var shortName in intType.definition){
                var longName = intType.name + "$$" + shortName;
                var longMem = jsType.commonPrototype[longName];
                if (longMem == undefined){
                    var shortMem = jsType.commonPrototype[shortName];
                    if (shortMem != undefined){
                        jsType.commonPrototype[longName] = jsType.commonPrototype[shortName];
                    }
                }
            }
        }
    }
};
//
// add by qiucw
// copy interface methods to JsType
// this is done before copying baseType's methods into it
//
JsCompiler._CopyInterfaceMethods = function (jsType) {
    for (var i = 0; i < jsType.interfaces.length; i++) {
        var iface = jsType.interfaces[i];
        for (var methodName in iface.definition) {
            jsType.commonPrototype[methodName] = iface.commonPrototype[methodName];
        }
    }
};
JsCompiler.Compile_Phase3 = function (){
    var funcs = AfterCompilationFunctions;
    AfterCompilationFunctions =  [];
    for (var $i5 = 0,$l5 = funcs.length,action = funcs[$i5]; $i5 < $l5; $i5++, action = funcs[$i5])
        action();
    IsCompiled = true;
};
JsCompiler.CopyMemberIfNotDefined = function (source, name, target){
    if(target[name]===undefined) target[name] = source;
};
JsCompiler._CopyObject = function (source, target){
    for(var p in source)
		target[p] = source[p];
	if(source.toString!=Object.prototype.toString && target.toString==Object.prototype.toString)
		target.toString = source.toString;
};
// qiucw source: base JsType, target: child JsType
JsCompiler._CopyFields = function (source, target){
    for (var p in source.fields) {
        if (target.fields == undefined) target.fields = {};
        target.fields[p] = source.fields[p];
    }
    for (var p in source.staticFields) {
        if (target.staticFields == undefined) target.staticFields = {};
        target.staticFields[p] = source.staticFields[p];
    }
};
JsCompiler._SafeCopyObject = function (source, target){
    	for(var p in source)
	{
		if(typeof(target[p])!='undefined')
		{
			//TODO: Alon - unmark this. throw new Error(p+' is already defined on target object');
		}
		else
			target[p] = source[p];
	}
	if(source.toString!=Object.prototype.toString)
	{//TODO: commented out by dan-el
		//if(target.toString!=Object.prototype.toString)
			//throw new Error('toString is already defined on target object');
	}
};
JsCompiler._EnumTryParse = function (name){
    return this.staticDefintion[name];
};
JsCompiler.NewByFunc = function (ctor){
    return new ctor();
};
JsCompiler.NewByFuncArgs = function (ctor, args){
    return new ctor.apply(null, args);
};
JsCompiler.GetNativeToStringFunction = function (){
    return Object.prototype.toString;
};
JsCompiler.Throw = function (exception){
    __LastException = exception || __LastException;
			var error = new Error(exception.ToString());
			error['_Exception'] = exception;
			throw error;
};
JsCompiler.CreateEmptyCtor = function (){
    return function(){};
};
JsCompiler.CreateBaseCtor = function (type){
    return function(){this.construct(type);};
};
if(typeof(Node)=='undefined')
	var Node = function(){};

JsCompiler.ResolveNamespace = function (nsText){
    var ns = window;
    var tokens = nsText.split(".");
    for (var i = 0; i < tokens.length; i++){
        var token = tokens[i];
        if (typeof(ns[token]) == "undefined")
            ns[token] = {};
        ns[token].name = tokens.slice(0, i).join(".");
        ns = ns[token];
    }
    return ns;
};
JsCompiler.ResolveBaseType = function (type, currentType){
    var baseType = JsTypeHelper.GetType(type.baseTypeName);
    if (baseType == null)
        baseType = JsTypeHelper.GetTypeIgnoreNamespace(type.baseTypeName, true);
    if (!baseType.isCompiled)
        JsCompiler.CompileType(baseType);
    currentType.baseType = baseType;
    baseType.derivedTypes.push(currentType);
};
JsCompiler.ResolveInterfaces = function (type, currentType){
    if (type.interfaceNames == null)
        return;
    for (var i = 0; i < type.interfaceNames.length; i++){
        var iName = type.interfaceNames[i];
        var iface = JsTypeHelper.GetType(iName);
        if (iface == null)
            iface = JsTypeHelper.GetTypeIgnoreNamespace(iName, true);
        if (!iface.isCompiled)
            JsCompiler.CompileType(iface);
        currentType.interfaces.push(iface);
    }
};
JsCompiler.CompileType = function (type){
    var currentType = (JsCompiler.Types[type.fullname] != null ? JsCompiler.Types[type.fullname] : type);
    if (currentType.ctors == null)
        currentType.ctors = new Object();
    if (!type.isCompiled){
        var baseTypeResolved = false;
        if (currentType.baseType == null && currentType.baseTypeName != null){
            JsCompiler.ResolveBaseType(type, currentType);
            if (currentType.baseType != null)
                baseTypeResolved = true;
        }
        JsCompiler.ResolveInterfaces(type, currentType);
        for (var p in type.definition){
            if (p.search("ctor") == 0){
                currentType[p] = type.definition[p];
                delete type.definition[p];
                if (typeof(currentType.commonPrototype) == "undefined")
                    currentType.commonPrototype = currentType[p].prototype;
                else
                    currentType[p].prototype = currentType.commonPrototype;
                currentType.ctors[p] = currentType[p];
            }
            if (p == "cctor")
                currentType.cctor = p;
        }
        if (currentType.ctor == null){
            if (currentType.ns == null || currentType.ns == ""){
                var jsCtor = window[currentType.name];
                if (typeof(jsCtor) == "function")
                    currentType.ctor = jsCtor;
            }
            if (currentType.ctor == null && currentType.ctors != null){
				// commented by qiucw
                //if (currentType.baseType != null)
                //    currentType.ctor = JsCompiler.CreateBaseCtor(currentType);
                //else
                    currentType.ctor = JsCompiler.CreateEmptyCtor();
            }
            if (currentType.ctor != null){
                currentType.ctors["ctor"] = currentType.ctor;
                if (typeof(currentType.commonPrototype) == "undefined")
                    currentType.commonPrototype = currentType.ctor.prototype;
                else
                    currentType.ctor.prototype = currentType.commonPrototype;
            }
        }
        for (var p in currentType.ctors){
            var ctor = currentType.ctors[p];
            if (ctor._type == null)
                ctor._type = currentType;
        }
        if (baseTypeResolved){
            JsCompiler._CopyInterfaceMethods(currentType);
            // 1)
            JsCompiler._CopyObject(currentType.baseType.commonPrototype, currentType.commonPrototype);
            // copy fields and staticFields(then are {get:xx, set:xx})
            // 2)
            JsCompiler._CopyFields(currentType.baseType, currentType);
        }

        var isArray = type.fullname == "Array";

        for (var p in type.definition){
            var member = type.definition[p];
            currentType.commonPrototype[p] = member;
            if (typeof(member) == "function"){
                member._name = p;
                member._type = currentType;

                // 往标准 Array 上添加我们自己的函数
                // 可以在 clrlibrary 中查找 fullname: "Array"
                if (isArray) {
                    // print("copy to [] " + p);
                    Array.prototype[p] = member;
                }
            }
        }
        if (type.definition.toString != Object.prototype.toString){
            currentType.commonPrototype.toString = type.definition.toString;
            currentType.commonPrototype.toString._type = currentType;
        }
        for (var p in type.staticDefinition){
            var member = type.staticDefinition[p];
            currentType[p] = member;
            if (typeof(member) == "function"){
                member._name = p;
                member._type = currentType;
            }
        }
        type.isCompiled = true;
    }
    JsCompiler.CompileEnum(currentType);
    if (currentType != type && type.customAttributes != null){
        if (currentType.customAttributes != null){
            for (var i = 0; i < type.customAttributes.length; i++){
                currentType.customAttributes.push(type.customAttributes[i]);
            }
        }
        else {
            currentType.customAttributes = type.customAttributes;
        }
    }
    return currentType;
};
JsCompiler.CompileEnum = function (currentType){
    if (currentType.Kind == "Enum"){
        currentType.tryParse = JsCompiler._EnumTryParse;
        for (var p in currentType.staticDefinition){
            if (typeof(currentType.staticDefinition[p]) == "string"){
                var x = JsCompiler.NewByFunc(currentType.ctor);
                x["_Name"] = p;
                currentType.staticDefinition[p] = x;
                currentType[p] = x;
            }
        }
    }
};
JsCompiler.GetHashKey = function (obj){
    if (obj == undefined)
        return "undefined";
    if (obj == null)
        return "null";
    if (obj.valueOf)
        obj = obj.valueOf();
    var type = typeof(obj);
    if (type == "string")
        return obj;
    if (type == "object" || type == "function"){
        if (obj._hashKey == null){
            obj._hashKey = JsCompiler._hashKeyPrefix + JsCompiler._hashKeyIndex;
            JsCompiler._hashKeyIndex++;
        }
        return obj._hashKey;
    }
    return obj.toString();
};
var JsTypeHelper = function (){
};
JsTypeHelper.GetTypeIgnoreNamespaceCache = null;
JsTypeHelper.GetTypeIgnoreNamespace = function (name, throwIfNotFound){
    var type;
    var cache = JsTypeHelper.GetTypeIgnoreNamespaceCache;
    if (cache != null){
        type = cache[name];
        if (typeof(type) != "undefined"){
            if (throwIfNotFound && type == null)
                throw $CreateException(new Error("type " + name + " was not found with (with IgnoreNamespace)."), new Error());
            return type;
        }
    }
    if (name.search(".") > -1){
        var tokens = name.split(".");
        name = tokens[tokens.length - 1];
    }
    type = JsCompiler.Types[name];
    var nameAfterNs = "." + name;
    if (type == null){
        for (var p in JsCompiler.Types){
            if (p == name || p.endsWith(nameAfterNs)){
                type = JsCompiler.Types[p];
                break;
            }
        }
    }
    if (throwIfNotFound && type == null)
        throw $CreateException(new Error("type " + name + " was not found with (with IgnoreNamespace)."), new Error());
    if (cache != null)
        cache[name] = (type != null ? type : null);
    return type;
};
JsTypeHelper._HasTypeArguments = function (typeName){
    return typeName.indexOf("[") > -1;
};
JsTypeHelper._GetTypeWithArguments = function (typeName, throwIfNotFound){
    var name = typeName;
    var gti = name.indexOf("`");
    if (gti != -1 && name.indexOf("[") > -1){
        var args = JsTypeHelper._ParseTypeNameArgs(name);
        if (args == null)
            return null;
        var type = JsTypeHelper.GetType(args[0], throwIfNotFound);
        if (type == null)
            return null;
        var res = new Array(0);
        res.push(type);
        var typeArgs = new Array(0);
        for (var i = 0; i < args[1].length; i++){
            var typeArg = JsTypeHelper.GetType(args[1][i][0], throwIfNotFound);
            if (typeArg == null)
                return null;
            typeArgs.push(typeArg);
        }
        res.push(typeArgs);
        return res;
    }
    return null;
};
JsTypeHelper._ParseTypeNameArgs = function (name){
    	var code = name.replace(/, [a-zA-Z0-9, =.]+\]/g, ']'); //remove all the ', mscorlib, Version=1.0.0.0, publicKeyToken=xxxxxxxxx
	code = code.replace(/`([0-9])/g, '$$$1,'); //remove the `2 and replace to $2, (the comma is for array to compile)
	code = '[' + code + ']';
try
{
	var args = eval(code);
return args;
}
catch(e)
{
  //ERROR
  return null;
}
	
};
JsTypeHelper.GetType = function (typeOrNameOrCtor, throwIfNotFound){
    if (typeof(typeOrNameOrCtor) != "string"){
        if (typeof(typeOrNameOrCtor) == "function")
            return typeOrNameOrCtor._type;
        return typeOrNameOrCtor;
    }
    var name = typeOrNameOrCtor;
    var gti = name.indexOf("`");
    if (gti != -1){
        name = name.substr(0, gti + 2).replace("`", "$");
    }
    var type = JsCompiler.Types[name];
    if (type == null){
        if (throwIfNotFound)
            throw $CreateException(new Error("JsType " + name + " was not found"), new Error());
        return null;
    }
    return type;
};
JsTypeHelper.FindType = function (name, throwIfNotFound){
    var type = JsTypeHelper.GetType(name, false);
    if (type == null)
        type = JsTypeHelper.GetTypeIgnoreNamespace(name, throwIfNotFound);
    return type;
};
JsTypeHelper.GetAssemblyQualifiedName = function (type){
    if (type._AssemblyQualifiedName == null){
        var name = type.fullname;
        if (type.assemblyName != null)
            name += ", " + type.assemblyName;
        type._AssemblyQualifiedName = name;
    }
    return type._AssemblyQualifiedName;
};
JsTypeHelper.GetName = function (type){
    return type.name;
};
JsTypeHelper.getMemberTypeName = function (instance, memberName){
    var signature = instance[memberName + "$$"];
    if (signature == null)
        return null;
    var tokens = signature.split(" ");
    var memberTypeName = tokens[tokens.length - 1];
    return memberTypeName;
};
JsTypeHelper.GetDelegate = function (obj, func){
    var target = obj;
    if (target == null)
        return func;
    if (typeof(func) == "string")
        func = target[func];
    var cache = target.__delegateCache;
    if (cache == null){
        cache = new Object();
        target.__delegateCache = cache;
    }
    var key = JsCompiler.GetHashKey(func);
    var del = cache[key];
    if (del == null){
        del = function (){
            var del2 = arguments.callee;
            return del2.func.apply(del.target, arguments);
        };
        del.func = func;
        del.target = target;
        del.isDelegate = true;
        cache[key] = del;
    }
    return del;
};

