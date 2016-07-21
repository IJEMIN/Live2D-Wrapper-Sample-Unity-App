using UnityEngine;
using System;
using live2d;
using live2d.framework;

public class MotionDataManager : MonoBehaviour
{
	public TextAsset[] liveMotionMtnBytesFiles;
	public TextAsset[] liveExpressionExpJsonFiles;

	public TextAsset idleMotion;


	public Live2DMotion GetLiveMotion (string motionName)
	{
		foreach (var i in liveMotionMtnBytesFiles) {
			if (i.name == motionName) {
				
				return Live2DMotion.loadMotion(i.bytes);
			}
		}
		return null;
	}


	public L2DExpressionMotion GetLiveExpression (string motionName)
	{
		foreach (var i in liveExpressionExpJsonFiles) {
			if (i.name == motionName) {
				return L2DExpressionMotion.loadJson(i.bytes);
			}
		}
		return null;
	}

	public Live2DMotion GetIdleLiveMotion ()
	{
		Debug.Log(idleMotion.name);
		return Live2DMotion.loadMotion (idleMotion.bytes);

	}

	public Live2DMotion GetRandomLiveMotion ()
	{
		return Live2DMotion.loadMotion(liveMotionMtnBytesFiles[UnityEngine.Random.Range(0, liveMotionMtnBytesFiles.Length)].bytes);
	}

	public L2DExpressionMotion GetRandomLiveExpression ()
	{
		return L2DExpressionMotion.loadJson(liveExpressionExpJsonFiles[UnityEngine.Random.Range(0, liveExpressionExpJsonFiles.Length)].bytes);
	}
    

}

/*



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

*/