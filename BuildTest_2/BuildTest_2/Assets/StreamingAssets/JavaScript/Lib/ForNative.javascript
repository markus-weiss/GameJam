
//function jsb_NewMonoBehaviour(name, nativeObj) {
//    var jsType = this[name];
//    if (jsType && jsType.ctor) {
//        var obj = new jsType.ctor();
//        obj.__nativeObj = nativeObj;
//        return obj;
//    }
//    return undefined;
//}
//
//function jsb_NewObject(name) {
//    var arr = name.split(".");
//    var obj = this;
//    arr.forEach(function (a) {
//        if (obj)
//            obj = obj[a];
//    });
//    if (obj && obj.ctor) {
//        var o = {};
//        o.__proto__ = obj.ctor.prototype;
//        return o;
//    }
//    return undefined;
//}

// called from C
function jsb_CallObjectCtor(name) {
    var arr = name.split(".");
    var obj = this;
    arr.forEach(function (a) {
        if (obj)
            obj = obj[a];
    });
    if (typeof(obj) == "function")
        return new obj();
    else
        return undefined;
    
    if (obj && obj.ctor) {
        return new obj.ctor();        
    }
    return undefined;
}

// called from js
function jsb_formatParamsArray(preCount, argArray, funArguments) {
    if (Object.prototype.toString.apply(argArray) === "[object Array]") {
        return argArray;
    } else {
        return Array.prototype.slice.apply(funArguments).slice(preCount);
    }
}

// called from c#
function jsb_IsInheritanceRel(baseClassName, subClassName) {
    var arr = subClassName.split(".");
    var obj = window;
    arr.forEach(function (a) {
        if (obj)
            obj = obj[a];
    });
    
    if (obj == undefined || obj === this)
        return false;

    while (true) {
        if (obj.baseType != undefined) {
            if (obj.baseType.fullname == baseClassName)
                return true;

            if (obj.interfaceNames !== undefined) {
                for (var i in obj.interfaceNames) {
                    if (obj.interfaceNames[i] == baseClassName) {
                        return true;
                    }
                }
            }
            obj = obj.baseType;
        }
        else break;
    }
    return false;
}