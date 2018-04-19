// UE is short for UnityEngine
var UE = UnityEngine;

// define javascript monobehaviour under this object
var jss = {};

// use this function to definen a javascript monobehaviour
jss.define_mb = function (name, fun) {
    jss[name] = fun;
    jss[name].getNativeType = function () { return "jss." + name; }
    jss[name].prototype = UE.MonoBehaviour.ctor.prototype;
}

// update coroutines and invoke repeatings
jss.UpdateCoroutineAndInvokes = function (mb, elapsed) {
    if (elapsed == undefined) {
        elapsed = UE.Time.get_deltaTime();
    }
    mb.$UpdateAllCoroutines(elapsed);
    mb.$UpdateAllInvokes(elapsed);
}

