using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security;

using jsval = JSApi.jsval;

public class JSComponentCustom1 : JSComponent
{
	int idOnGUI = 0;

	protected override void initMemberFunction()
	{
		base.initMemberFunction();
		idOnGUI = JSApi.getObjFunction(jsObjID, "OnGUI");
	}

	void OnGUI()
	{
		callIfExist(idOnGUI);
	}
}
