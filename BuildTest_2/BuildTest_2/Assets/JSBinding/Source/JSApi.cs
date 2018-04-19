using UnityEngine;
using System.Collections;

public class JSApi : JSApiBase
{
	public static Vector2 getVector2S(int e)
	{
		getVector2(e);
		return new Vector2(getObjX(), getObjY());
	}
	public static Vector3 getVector3S(int e)
	{
		getVector3(e);
		return new Vector3(getObjX(), getObjY(), getObjZ());
	}
	public static void setVector2S(int e, Vector2 v)
	{
		setVector2(e, v.x, v.y);
	}
	public static void setVector3S(int e, Vector3 v)
	{
		setVector3(e, v.x, v.y, v.z);
	}
}
