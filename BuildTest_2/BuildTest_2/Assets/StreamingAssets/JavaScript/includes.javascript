// DO NOT change order

// ****** 1 js compiler ******

CS.require("Lib/Sk/compiler");

// ****** 2 Libraries ******

// Sk js classes library
CS.require("Lib/Sk/clrlibrary");

// C# classes library, may overwrite some Sk classes
CS.require("Lib/Gen");

// Manually written js, will overwrite some classes methods above
CS.require("Lib/Manual/UnityEngine_Vector3");
CS.require("Lib/Manual/UnityEngine_Vector2");
CS.require("Lib/Manual/UnityEngine_MonoBehaviour");
CS.require("Lib/Manual/UnityEngine_WaitForSeconds");
CS.require("Lib/Manual/UnityEngine_GameObject");
CS.require("Lib/Manual/MissingClasses");

// A class representing C# object of unknown type
CS.require("Lib/JSRepresentedObject");

// Some functions used by js/c++/c#
CS.require("Lib/ForNative");

// todo
// UnityEngine.MonoBehaviour.ctor = function () {}

// find Unity Debug.Log and Debug.LogError
var printError = function () {};
var print = function () {};
(function () {
	var dbg = jst_find("UnityEngine.Debug");
	if (dbg) {
		printError = dbg.staticDefinition.LogError$$Object;
		print = dbg.staticDefinition.Log$$Object;
	}
}());


// ****** 3 Compile now! ******

// sort JsTypes before Compile()
// if we have 2 types: A.B.C and A.B
// A.B will be in front of A.B.C after sort
JsTypes.sort(function (a, b) {
    return (a.fullname < b.fullname ? -1 : 1);
});

try {
    Compile();
}
catch (ex)  {
    //if (ex.message)
    //    printError("JS Error! Error: " + ex.message + "\n\nStack: \n" + ex.stack);
    //else
        printError("JS Error! Error: " + ex + "\n\nStack: \n" + ex.stack);
}

// 5 Error handler (disable error catching by commenting this line)
//----------------------------------------------------------
CS.require("ErrorHandler");


// 6 user scripts
//----------------------------------------------------------

// A convenient way to define javascript MonoBehaviour
CS.require("JsMonoBehaviurDef");


// 2048 sample
CS.require("Samples/2048/Official/grid");
CS.require("Samples/2048/Official/tile");
CS.require("Samples/2048/Official/game_manager");
CS.require("Samples/2048/Modified/actuator");
CS.require("Samples/2048/Modified/input_manager");
CS.require("Samples/2048/Modified/storage");
CS.require("Samples/2048/Modified/tilemovement");
CS.require("Samples/2048/Modified/ui_controller");

// misc sample
CS.require("Samples/Misc/Misc");
