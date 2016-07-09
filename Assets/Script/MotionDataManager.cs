using UnityEngine;
using System;
using live2d;
using live2d.framework;

public class MotionDataManager : MonoBehaviour
{
	public Live2DMotionData[] liveMotions;
	public Live2DExpressionData[] liveExpressions;

	public TextAsset idleMotion;


	//입력값으로 줄 모션 아이디를 리플렉션으로 쉽게 참조할 수 있게 할 필요가 있음.
	public Live2DMotion GetLiveMotion (string motionName)
	{
		foreach (var iteredMotion in liveMotions) {
			if (iteredMotion.motionName == motionName) {
				return iteredMotion.GetLiveMotion ();
			}
		}
		return null;
	}


	public L2DExpressionMotion GetLiveExpression (string motionName)
	{
		foreach (var iteredMotion in liveExpressions) {
			if (iteredMotion.expressionName == motionName) {
				return iteredMotion.GetLiveExpression ();
			}
		}
		return null;
	}

	public Live2DMotion GetIdleLiveMotion ()
	{
		return Live2DMotion.loadMotion (idleMotion.bytes);

	}

	public Live2DMotion GetRandomLiveMotion ()
	{
		return liveMotions [UnityEngine.Random.Range (0, liveMotions.Length)].GetLiveMotion ();
	}

	public L2DExpressionMotion GetRandomLiveExpression ()
	{
		return liveExpressions [UnityEngine.Random.Range (0, liveExpressions.Length)].GetLiveExpression ();
	}
    

}

[Serializable]
public class Live2DMotionData
{

	public string motionName {
		get {
			return motionData.name;
		}
	}

	public TextAsset motionData;

	public Live2DMotion GetLiveMotion ()
	{
		return Live2DMotion.loadMotion (motionData.bytes);
	}
}


[Serializable]
public class Live2DExpressionData
{

	public string expressionName {
		get {
			return expressionData.name;
		}
	}

	//바이트 파일 말고 json으로 넣을것. raw하게 넣으려면 L2DExpression 말고
	//Live2DMotion 의 Load 메소드를 쓰면 됨. L2D가 붙는 클래스들은 프레임워크에서 제공.
	public TextAsset expressionData;

	public L2DExpressionMotion GetLiveExpression ()
	{
		return L2DExpressionMotion.loadJson (expressionData.bytes);
	}
}