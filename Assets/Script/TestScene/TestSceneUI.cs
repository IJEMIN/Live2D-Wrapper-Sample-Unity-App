using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TestSceneUI : MonoBehaviour {


	public InputField expressionField;
	public InputField motionField;

	public LiveCharacter character;
	public void ApplyLiveMotionAndExpression()
	{
		character.SetExpression(expressionField.text);
		character.StartMotion(motionField.text);
	}

}
