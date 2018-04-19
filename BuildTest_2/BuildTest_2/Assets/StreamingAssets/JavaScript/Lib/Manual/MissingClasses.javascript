[
    {
        fullname: "System.Collections.ArrayList.SimpleEnumerator",
        baseTypeName: "System.Object",
        interfaceNames: ["System.Collections.IEnumerator"],
        Kind: "Class"
    },
    {
        fullname: "UnityEngine.UI.IGraphicEnabledDisabled",
        baseTypeName: "System.Object",
        Kind: "Interface",
    },
    {
        fullname: "UnityEngine.Transform.Enumerator",
        baseTypeName: "System.Object",
        interfaceNames: ["System.Collections.IEnumerator"],
        Kind: "Class"
    },
    {
        fullname: "UnityEngine.ISerializationCallbackReceiver",
        baseTypeName: "System.Object",
        Kind: "Interface",
    },
].forEach(function (jsType) {
        jst_pushOrReplace(jsType);
    });