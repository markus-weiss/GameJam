_jstype = jst_find("UnityEngine.GameObject");

(function () {
    if (_jstype) {
        var old = _jstype.definition.AddComponent$1;
        _jstype.definition.AddComponent$1 = function(t0, iOfJsComp) { 
            if (iOfJsComp == undefined) {
                iOfJsComp = 0;
            }
            ComponentsHelper.s_iOfJSComponent = iOfJsComp;
            old.call(this, t0);
        }
    }
}());