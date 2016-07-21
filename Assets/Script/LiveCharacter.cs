//created by I_Jemin (i_jemin@hotmail.com)
//게임 개발직 구직중..

//참고 자료 http://www.slideshare.net/dongrimshin/live2d-48240587

//L2D로 시작되는 클래스들은 UpdateParam이라는 메소드를 가지고 있습니다.
//해당 메소드에 Live2DModelUnity 인스턴스를 집어넣으면, 해당 인스턴스가 가리키는 캐릭터의 (표정 등)의 각각 수치가 업데이트 됩니다.

//라이브2D의 표정 모션 등의 정보는 바이트 코드로 받아야 합니다. 라이브2D 확장자를 유니티가 그대로 받아들일 수 없습니다.

using UnityEngine;
using System.Collections.Generic;

using live2d.framework;
using live2d;



/*
MotionDataManager은 임의로 만든 클래스입니다.
해당 캐릭터의 모션과 표정 파일들을 가지고 있다가 반환해주는 역할만 하면 됩니다.
알아서 만드시면 됩니다.
모션과 표정 파일을 리스트 등으로 가지고 있으며 이들을 Live2DMotion 인스턴스로 적절하게 반환하는 GetMotion()과 GetExpression() 메소드만 구현해주세요.
*/


[RequireComponent (typeof(MotionDataManager), typeof(AudioSource))]
//[ExecuteInEditMode]
public class LiveCharacter : MonoBehaviour
{
	//가슴 흔들림 등 물리 데이터
	public TextAsset physicsData;

	//캐릭터에 입력이 없는 상태일 경우 Idle 애니메이션을 반복할 것인가
	public bool enableAnimationForIdle = true;

	//모션과 모션이 페이드 되는 시간
	public int motionFadeSpeed = 1000;

	//오디오 소스의 볼륨에 따라 입모양을 조정해주는데 사용할 값
	private float lipSyncValue;

	//모델링 데이터를 가진 moc 파일들이 이쪽으로 온다.
	public TextAsset mocData;

	//모션과 표정을 관리해준다.
	public MotionDataManager motionDataManager
	{
		get;
		set;
	}

	//텍스쳐는 하나 이상을 사용할 수 있으니 배열(리스트)로.
	public Texture2D[] textureList;

	//포즈 데이터를 가져온다. (파츠를 끄고 켤수 있다)
	public TextAsset poseData;

	//랜더링 되는 모델
	private Live2DModelUnity model;

	//눈 깜박임 주기

	public int msecBetEyeBlink = 2400;
	//그려지는 매트릭스 공간.
	private Matrix4x4 canvasMatrix;

	//숨쉬는 모션을 넣어준다. 해당 모션은 재생중인 주모션을 대체하지 않고, 겹치는 속성들에 대해 자신의 속성값을 덧셈 혹은 곱한다.
	public TextAsset breathMotion;

	private L2DEyeBlink l2dEyeBlink = new L2DEyeBlink ();

	//모션은 Live2DMotion 이라는 형태로 저장 가능함을 보여줄려고 두개 나누어 놓음.
	//Live2DMotion 및 Expression들은 load 메소드를 호출해서 TextAsset에서 .bytes 를 입력으로 줘서 인스턴스를 생성해야 합니다.
	//Awake 메소드 부분 참고.
	private Live2DMotion breathMotion_;

	//모션 매니저와 표정 매나저
	private L2DMotionManager l2dMotionManager;
	private L2DMotionManager l2dExpressioNMotionManager;


	private Transform transformCache;

	//  private MeshRenderer myRender;


	/*
    L2D 형 클래스들, 기반 클래스 보다 상위쪽
    */

	//포즈 매니저, json 파일을 가져옴
	private L2DPose l2dPose = new L2DPose ();

	//물리 처리
	private L2DPhysics l2dPhysics = new L2DPhysics ();

	//목소리
	private AudioSource chVoice;


	void Awake ()
	{
		chVoice = GetComponent<AudioSource> ();
		//  myRender = gameObject.GetComponentInChildren<MeshRenderer> ();

		lipSyncValue = 0f;

		//초기화 페이즈//


		Live2D.init ();

		//json 파일 불러와서 속성 값들 리스트로 뽑아 옴
		model = Live2DModelUnity.loadModel (mocData.bytes);

		//모션 데이터 할당
		motionDataManager = GetComponent<MotionDataManager> ();


		//만약 숨쉬는 모션이 있으면
		if (breathMotion != null) {
			breathMotion_ = Live2DMotion.loadMotion (breathMotion.bytes);
		}


		//텍스쳐 세팅
		for (int i = 0; i < textureList.Length; i++) {
			model.setTexture (i, textureList [i]);
		}


		// 포즈 파일은 필수적인 것은 아님.
		if (poseData) {
			l2dPose = L2DPose.load (poseData.bytes);
		}

		if (physicsData) {
			l2dPhysics = L2DPhysics.load (physicsData.bytes);

		}


		//모션을 식별할때 이 스크립트는 string을 사용했지만 enum을 쓰든 뭘 쓰든 모션 식별자로 뭘 써도 무방.



		//아까 초기화한 모델의 가로 사이즈 받아옴.
		var width = model.getCanvasWidth ();


		//캔버스 지정
		canvasMatrix = Matrix4x4.Ortho (0.0f, width, width, 0.0f, -50.0f, 50.0f);


		//모션 매니저들 할당. 사실 얘들은 모션들의 우선순위를 정해주는 등의 역할이 있으나 복잡해서 거의 안쓴다.
		//주 역활은 자기가 가지고 있는 주모션 하나를 모델 인스턴스에 적용해주는 것.
		//얘들에게 모션을 주고 여러 설정자들로 모션의 속성들을 설정해줄 수 있다. 그외에 우선순위를 정해주고 등등 기능 있으나 자주 안쓰임.
		//모션 매니저에 현재 모션을 지정해준다고 모델의 모션이 업데이트되지 않는다.
		//반드시 L2D매니저.UpdateParam(모델 인스턴스) 를 호출해줘야 모델 인스턴스의 움직임이 갱신됨.


		l2dMotionManager = new L2DMotionManager ();
		l2dExpressioNMotionManager = new L2DMotionManager ();

		//모션 재생 시작


		//모델의 모션 업데이트
		l2dPose.updateParam (model);

		transformCache = transform;
	}

	void Start ()
	{
		//l2dEyeBlink.setEyeMotion(1000,1000,1000);

		l2dEyeBlink.setInterval (msecBetEyeBlink);
        

		//StartMotion("IDLE");

		StartIdleMotion ();
	}

	public void SetIdleActive (bool active)
	{
		enableAnimationForIdle = active;

	}




	//립싱크 부분. 오디오 소스의 출력 구간 평균을 부드럽게 Ease해서 입을 움직여준다.
	public void SetLipSync ()
	{
		if (!chVoice) {
			return;
		}

		float[] data = new float[256];
		chVoice.GetOutputData (data, 0);

		//take the median of the recorded samples
		List<float> s = new List<float> ();
		foreach (float f in data) {
			s.Add (Mathf.Abs (f));
		}
		s.Sort ();

		lipSyncValue = (float)(s [256 / 2]) * 30;

	}


	//외부에서 Speack(보이스파일) 을 호출하면 캐릭터가 말하면서 립싱크도 함.
	public void Speack (AudioClip chatacterVoiceFile)
	{
		if (chVoice) {
			chVoice.PlayOneShot (chatacterVoiceFile);
		}
	}

	private Live2DMotion idleMotion;

	public void StartIdleMotion ()
	{
		if (idleMotion == null) {
			idleMotion = motionDataManager.GetIdleLiveMotion ();
            
		}

		StartMotion (idleMotion);
	}


	//외부에서 StartMotion(모션 파일 식별자)를 호출하면 캐릭터가 현재 모션을 중단하고 해당 모션을 1회 시행함.
	public void StartMotion (string motionName)
	{


		var calledMotion = motionDataManager.GetLiveMotion (motionName);

		if (calledMotion != null) {
			StartMotion (calledMotion);
			Debug.Log("Start Motion: " + motionName);
		}
	}


	public void StartMotion (Live2DMotion rawMotionData)
	{
		rawMotionData.setFadeIn (motionFadeSpeed);
		rawMotionData.setFadeOut (motionFadeSpeed);
		l2dMotionManager.startMotionPrio (rawMotionData, 0);
		isBreath = false;
	}


	//외부에서 SetExpression(모션 파일 식별자)를 호출하면 캐릭터가 현재 모션위에 해당 표정을 겹침.
	public void SetExpression (string expressionName)
	{
		
		var calledExpression = motionDataManager.GetLiveExpression (expressionName);

		if (calledExpression != null) {
			SetExpression (calledExpression);
			Debug.Log("Start Expression: " + expressionName);
		}
	}

	public void SetExpression(L2DExpressionMotion rawExpressionData)
	{
		rawExpressionData.setFadeIn (motionFadeSpeed);
		rawExpressionData.setFadeOut (motionFadeSpeed);
		l2dExpressioNMotionManager.startMotion (rawExpressionData, false);
	}


	//실제로 모델이 랜더되는 동안 호출
	void OnRenderObject ()
	{
		model.setMatrix (transformCache.localToWorldMatrix * canvasMatrix);
		model.draw ();
	}

	private bool isBreath = false;



	void LateUpdate ()
	{

		//모션이 끝남
		//모션을 1회 재생했을 때 해당 모션이 완전이 재생되고 종료되었는지는 isFinished 메소드로 체크 가능.
		if (l2dMotionManager.isFinished ()) {

			if (enableAnimationForIdle) {
				//idle 애니메이션을 재생하길 원하는 경우
				//기본 모션으로 바꿔준다.
				//l2dMotionManager.startMotion(motionDataManager.GetLiveMotion("IDLE"));
				StartIdleMotion ();
				//isBreath = false;
			} else {
				//모션이 끝나긴 했는데, IDLE 애니메이션을 루프재생 하길 원치 않은 경우
				if (breathMotion != null) {
					//숨쉬기 모션이 있으면 모션이 끝났을때, 모션의 마지막 프레임 자세를 간직한체 그 위에 숨쉬는 모습을 덧칠해줌.
					l2dMotionManager.startMotion (breathMotion_, false);
					isBreath = true;
				}
			}

		}


		if (isBreath) {
			l2dEyeBlink.updateParam (model);
		}


		//모션 매니저들이 자신들이 가진 모션의 속성들로 모델을 갱신
		l2dMotionManager.updateParam (model);
		l2dExpressioNMotionManager.updateParam (model);

		//만약 캐릭터가 말하는 중이면 입모양 움직여 주세요. 먼저 립싱크값 갱신
		SetLipSync ();
		//그 다음에 갱신된 립싱크 값으로 입에 해당하는 속성의 값을 갱신합니다.
		model.setParamFloat (L2DStandardID.PARAM_MOUTH_OPEN_Y, lipSyncValue, 0.7f);


		l2dPhysics.updateParam (model);
		//포즈 업데이트
		l2dPose.updateParam (model);

		//그리고 최종 갱신.
		model.update ();
	}

	public void SetRandomLiveMotionAndExpression()
	{
		StartMotion (motionDataManager.GetRandomLiveMotion());
		SetExpression (motionDataManager.GetRandomLiveExpression ());
	}

	/*
    void OnMouseDown()
    {
        StartMotion(motionDataManager.GetRandomLiveMotion());
    }
    */
    
}
