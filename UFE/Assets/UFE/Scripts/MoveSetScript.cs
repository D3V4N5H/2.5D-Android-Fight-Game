using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class BasicMoveInfo:ICloneable {
	public AnimationClip clip1;
	public AnimationClip clip2;
	public AnimationClip clip3;
	public AnimationClip clip4;
	public AnimationClip clip5;
	public AnimationClip clip6;
	public WrapMode wrapMode;
	public bool autoSpeed = true;
	public float animationSpeed = 1;
	public float restingClipInterval = 6;
	public bool overrideBlendingIn = false;
	public bool overrideBlendingOut = false;
	public float blendingIn = 0;
	public float blendingOut = 0;
	public bool invincible;
	public bool disableHeadLook;
	public AudioClip[] soundEffects = new AudioClip[0];
	public bool continuousSound;
	public ParticleInfo particleEffect = new ParticleInfo();
	public BasicMoveReference reference;
	
	[HideInInspector] public string name;
	[HideInInspector] public bool editorToggle;
	[HideInInspector] public bool soundEffectsToggle;
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}


[System.Serializable]
public class ParticleInfo:ICloneable {
	public bool editorToggle;
	public GameObject prefab;
	public float duration = 1;
	public bool stick = false;
	public Vector3 offSet;
	public BodyPart bodyPart;
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class BasicMoves:ICloneable {
	public BasicMoveInfo idle = new BasicMoveInfo();
	public BasicMoveInfo moveForward = new BasicMoveInfo();
	public BasicMoveInfo moveBack = new BasicMoveInfo();
	public BasicMoveInfo takeOff = new BasicMoveInfo();
	public BasicMoveInfo jumpStraight = new BasicMoveInfo();
	public BasicMoveInfo jumpBack = new BasicMoveInfo();
	public BasicMoveInfo jumpForward = new BasicMoveInfo();
	public BasicMoveInfo fallStraight = new BasicMoveInfo();
	public BasicMoveInfo fallBack = new BasicMoveInfo();
	public BasicMoveInfo fallForward = new BasicMoveInfo();
	public BasicMoveInfo landing = new BasicMoveInfo();
	public BasicMoveInfo crouching = new BasicMoveInfo();
	public BasicMoveInfo blockingCrouchingPose = new BasicMoveInfo();
	public BasicMoveInfo blockingCrouchingHit = new BasicMoveInfo();
	public BasicMoveInfo blockingHighPose = new BasicMoveInfo();
	public BasicMoveInfo blockingHighHit = new BasicMoveInfo();
	public BasicMoveInfo blockingLowHit = new BasicMoveInfo();
	public BasicMoveInfo blockingAirPose = new BasicMoveInfo();
	public BasicMoveInfo blockingAirHit = new BasicMoveInfo();
	public BasicMoveInfo parryCrouching = new BasicMoveInfo();
	public BasicMoveInfo parryHigh = new BasicMoveInfo();
	public BasicMoveInfo parryLow = new BasicMoveInfo();
	public BasicMoveInfo parryAir = new BasicMoveInfo();
	public BasicMoveInfo bounce = new BasicMoveInfo();
	public BasicMoveInfo fallingFromBounce = new BasicMoveInfo();
	public BasicMoveInfo fallDown = new BasicMoveInfo();
	public BasicMoveInfo getHitCrouching = new BasicMoveInfo();
	public BasicMoveInfo getHitHigh = new BasicMoveInfo();
	public BasicMoveInfo getHitLow = new BasicMoveInfo();
	public BasicMoveInfo getHitHighKnockdown = new BasicMoveInfo();
	public BasicMoveInfo getHitHighLowKnockdown = new BasicMoveInfo();
	public BasicMoveInfo getHitAir = new BasicMoveInfo();
	public BasicMoveInfo getHitCrumple = new BasicMoveInfo();
	public BasicMoveInfo getHitKnockBack = new BasicMoveInfo();
	public BasicMoveInfo getHitSweep = new BasicMoveInfo();
	public BasicMoveInfo standUp = new BasicMoveInfo();

    public bool moveEnabled = true;
    public bool jumpEnabled = true;
    public bool crouchEnabled = true;
    public bool blockEnabled = true;
    public bool parryEnabled = true;

	public object Clone() {
		return CloneObject.Clone(this);
	}
}

public class MoveSetScript : MonoBehaviour {
	[HideInInspector]	public BasicMoves basicMoves;
	[HideInInspector]	public MoveInfo[] attackMoves;
	[HideInInspector]	public MoveInfo[] moves;
	[HideInInspector]	public int totalAirMoves;
	[HideInInspector]	public bool animationPaused;
	[HideInInspector]	public float overrideNextBlendingValue = -1;
	[HideInInspector]	public Dictionary<ButtonPress, float> chargeValues = new Dictionary<ButtonPress, float>();
	[HideInInspector]	public MoveInfo intro;
	[HideInInspector]	public MoveInfo outro;

#if !UFE_BASIC
    public MecanimControl MecanimControl { get { return this.mecanimControl; } }
#endif

    public LegacyControl LegacyControl { get { return this.legacyControl; } }
    
	private ControlsScript controlsScript;
	private HitBoxesScript hitBoxesScript;
	private List<ButtonPress> lastButtonPresses = new List<ButtonPress>();
	private float lastTimePress;
    //private float[] animSpeedStorage;

#if !UFE_BASIC
	private MecanimControl mecanimControl;
#endif

    private LegacyControl legacyControl;
	private bool precisionControl;

	
	void Awake(){
		controlsScript = transform.parent.gameObject.GetComponent<ControlsScript>();
		hitBoxesScript = GetComponent<HitBoxesScript>();

		foreach(ButtonPress bp in Enum.GetValues(typeof(ButtonPress))){
			chargeValues.Add (bp, 0);
		}

		controlsScript.myInfo.currentCombatStance = CombatStances.Stance10;
		ChangeMoveStances(CombatStances.Stance1);
	}
	
    
#if !UFE_BASIC
	void Start(){
		if (controlsScript.myInfo.animationType == AnimationType.Mecanim){
			mecanimControl.SetMirror(controlsScript.mirror > 0);
		}
	}
#endif
	
	public void ChangeMoveStances(CombatStances newStance){
		if (controlsScript.myInfo.currentCombatStance == newStance) return;
		foreach(MoveSetData moveSetData in controlsScript.myInfo.moves){
			if (moveSetData.combatStance == newStance){
				basicMoves = moveSetData.basicMoves;
				attackMoves = moveSetData.attackMoves;
				//moves = moveSetData.attackMoves;
				
				/*GameObject opponent = GameObject.Find(gameObject.name);
				if (!opponent.Equals(gameObject) && opponent.name == gameObject.name){ // Mirror Match Move cloning
					List<MoveInfo> moveList = new List<MoveInfo>();
					bool alreadyCloned = false;
					foreach(MoveInfo move in attackMoves){
						if (move.name.IndexOf("(Clone)") != -1) {
							alreadyCloned = true;
							break;
						}
						moveList.Add(Instantiate(move) as MoveInfo);
					}
					if (alreadyCloned){
						moves = attackMoves;
					}else{
						moves = moveList.ToArray();
					}
				}else{
					moves = attackMoves;
				}*/
				
				moves = attackMoves;

				foreach(MoveInfo move1 in moves){
					if (move1.chargeMove && move1.chargeTiming <= controlsScript.myInfo.executionTiming){
						Debug.LogWarning("Warning: " + move1.name + " ("+ move1.moveName +") charge timing must be higher then the character's execution timing.");
					}

					foreach(MoveInfo move2 in moves){
						if (move1.name != move2.name && move1.moveName == move2.moveName){
							Debug.LogWarning("Warning: " + move1.name + " ("+ move1.moveName +") has the same name as "+ move2.name + " ("+ move2.moveName +")");
						}
					}
				}

				fillMoves();

				if (moveSetData.cinematicIntro != null) {
					intro = Instantiate(moveSetData.cinematicIntro) as MoveInfo;
					intro.name = "Intro";
					attachAnimation(intro.animationClip, intro.name, intro.animationSpeed, intro.wrapMode);
				}
				if (moveSetData.cinematicOutro != null) {
					outro = Instantiate(moveSetData.cinematicOutro) as MoveInfo;
					outro.name = "Outro";
					attachAnimation(outro.animationClip, outro.name, outro.animationSpeed, outro.wrapMode);
				}

				controlsScript.myInfo.currentCombatStance = newStance;
				
				System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
					return move1.buttonExecution.Length.CompareTo(move2.buttonExecution.Length);
				});
				
				System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
					if (move1.buttonExecution.Length > 1 && move1.buttonExecution.Contains(ButtonPress.Back)) return 0;
					if (move1.buttonExecution.Length > 1 && move1.buttonExecution.Contains(ButtonPress.Foward)) return 0;
					if (move1.buttonExecution.Length > 1) return 1;
					return 0;
				});
				
				System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
					return move1.selfConditions.basicMoveLimitation.Length.CompareTo(move2.selfConditions.basicMoveLimitation.Length);
				});
				
				System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
					return move1.opponentConditions.basicMoveLimitation.Length.CompareTo(move2.opponentConditions.basicMoveLimitation.Length);
				});

				System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
					return move1.opponentConditions.possibleMoveStates.Length.CompareTo(move2.opponentConditions.possibleMoveStates.Length);
				});

				System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
					return move1.buttonSequence.Length.CompareTo(move2.buttonSequence.Length);
				});

				System.Array.Reverse(moves);

				return;
			}
		}
	}

	private void fillMoves(){
		DestroyImmediate(gameObject.GetComponent(typeof(Animation)));
		DestroyImmediate(gameObject.GetComponent(typeof(Animator)));
		DestroyImmediate(gameObject.GetComponent("MecanimControl"));

        if (UFE.isNetworkAddonInstalled && UFE.config.networkOptions.forceAnimationControl) {
            controlsScript.myInfo.animationFlow = AnimationFlow.UFEEngine;
        }

		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
			gameObject.AddComponent(typeof(Animation));
            gameObject.GetComponent<Animation>().clip = basicMoves.idle.clip1;
            gameObject.GetComponent<Animation>().wrapMode = WrapMode.Once;

            legacyControl = gameObject.AddComponent<LegacyControl>();
            if (controlsScript.myInfo.animationFlow == AnimationFlow.UFEEngine) legacyControl.overrideAnimatorUpdate = true;

		}else{
#if !UFE_BASIC
            Animator animator = (Animator) gameObject.AddComponent(typeof(Animator));
			animator.avatar = controlsScript.myInfo.avatar;
			//animator.applyRootMotion = true;

			mecanimControl = gameObject.AddComponent<MecanimControl>();
			mecanimControl.defaultTransitionDuration = controlsScript.myInfo.blendingTime;
			mecanimControl.SetDefaultClip(basicMoves.idle.clip1, "default", basicMoves.idle.animationSpeed, WrapMode.Loop, 
			                              (controlsScript.mirror > 0 && UFE.config.characterRotationOptions.autoMirror));

            mecanimControl.defaultWrapMode = WrapMode.Once;
            if (controlsScript.myInfo.animationFlow == AnimationFlow.UFEEngine) mecanimControl.overrideAnimatorUpdate = true;
#endif
		}


		foreach(MoveInfo move in moves) {
			if (move == null) Debug.LogError("Error: You have an empty move file in your character editor.");
			if (move.animationClip != null) {
				attachAnimation(move.animationClip, move.name, move.animationSpeed, move.wrapMode);
			}
		}
		setBasicMoveAnimation(basicMoves.idle, "idle", BasicMoveReference.Idle);
		setBasicMoveAnimation(basicMoves.moveForward, "moveForward", BasicMoveReference.MoveForward);
		setBasicMoveAnimation(basicMoves.moveBack, "moveBack", BasicMoveReference.MoveBack);
		setBasicMoveAnimation(basicMoves.takeOff, "takeOff", BasicMoveReference.TakeOff);
		setBasicMoveAnimation(basicMoves.jumpStraight, "jumpStraight", BasicMoveReference.JumpStraight);
		setBasicMoveAnimation(basicMoves.jumpBack, "jumpBack", BasicMoveReference.JumpBack);
		setBasicMoveAnimation(basicMoves.jumpForward, "jumpForward", BasicMoveReference.JumpForward);
		setBasicMoveAnimation(basicMoves.fallStraight, "fallStraight", BasicMoveReference.FallStraight);
		setBasicMoveAnimation(basicMoves.fallBack, "fallBack", BasicMoveReference.FallBack);
		setBasicMoveAnimation(basicMoves.fallForward, "fallForward", BasicMoveReference.FallForward);
		setBasicMoveAnimation(basicMoves.landing, "landing", BasicMoveReference.Landing);
		setBasicMoveAnimation(basicMoves.crouching, "crouching", BasicMoveReference.Crouching);

        setBasicMoveAnimation(basicMoves.blockingCrouchingPose, "blockingCrouchingPose", BasicMoveReference.BlockingCrouchingPose);
        setBasicMoveAnimation(basicMoves.blockingCrouchingHit, "blockingCrouchingHit", BasicMoveReference.BlockingCrouchingHit);
        setBasicMoveAnimation(basicMoves.blockingHighPose, "blockingHighPose", BasicMoveReference.BlockingHighPose);
        setBasicMoveAnimation(basicMoves.blockingHighHit, "blockingHighHit", BasicMoveReference.BlockingHighHit);
        setBasicMoveAnimation(basicMoves.blockingLowHit, "blockingLowHit", BasicMoveReference.BlockingLowHit);
        setBasicMoveAnimation(basicMoves.blockingAirPose, "blockingAirPose", BasicMoveReference.BlockingAirPose);
        setBasicMoveAnimation(basicMoves.blockingAirHit, "blockingAirHit", BasicMoveReference.BlockingAirHit);
        setBasicMoveAnimation(basicMoves.parryCrouching, "parryCrouching", BasicMoveReference.ParryCrouching);
		setBasicMoveAnimation(basicMoves.parryHigh, "parryHigh", BasicMoveReference.ParryHigh);
		setBasicMoveAnimation(basicMoves.parryLow, "parryLow", BasicMoveReference.ParryLow);
		setBasicMoveAnimation(basicMoves.parryAir, "parryAir", BasicMoveReference.ParryAir);

        setBasicMoveAnimation(basicMoves.getHitHigh, "getHitHigh", BasicMoveReference.GetHitHigh);
        setBasicMoveAnimation(basicMoves.getHitLow, "getHitLow", BasicMoveReference.GetHitLow);
		setBasicMoveAnimation(basicMoves.getHitCrouching, "getHitCrouching", BasicMoveReference.GetHitCrouching);
		setBasicMoveAnimation(basicMoves.getHitHighKnockdown, "getHitHighKnockdown", BasicMoveReference.GetHitHighKnockdown);
		setBasicMoveAnimation(basicMoves.getHitHighLowKnockdown, "getHitHighLowKnockdown", BasicMoveReference.GetHitHighLowKnockdown);
		setBasicMoveAnimation(basicMoves.getHitSweep, "getHitSweep", BasicMoveReference.GetHitSweep);
		setBasicMoveAnimation(basicMoves.getHitCrumple, "getHitCrumple", BasicMoveReference.GetHitCrumple);
        setBasicMoveAnimation(basicMoves.getHitAir, "getHitAir", BasicMoveReference.GetHitAir);
        
        setBasicMoveAnimation(basicMoves.fallDown, "fallDown", BasicMoveReference.FallDown);
        setBasicMoveAnimation(basicMoves.bounce, "bounce", BasicMoveReference.Bounce);
        setBasicMoveAnimation(basicMoves.fallingFromBounce, "fallingFromBounce", BasicMoveReference.FallingFromBounce);
		setBasicMoveAnimation(basicMoves.getHitKnockBack, "getHitKnockBack", BasicMoveReference.GetHitKnockBack);
		setBasicMoveAnimation(basicMoves.standUp, "standUp", BasicMoveReference.StandUp);
		
		/*if (controlsScript.myInfo.animationType == AnimationType.Legacy){
			animSpeedStorage = new float[GetComponent<Animation>().GetClipCount() + 2];
		}*/
	}
	
	private void setBasicMoveAnimation(BasicMoveInfo basicMove, string animName, BasicMoveReference basicMoveReference){
		if (basicMove.clip1 == null) {
			return;
		}
		basicMove.name = animName;
		basicMove.reference = basicMoveReference;

        attachAnimation(basicMove.clip1, animName, basicMove.animationSpeed, basicMove.wrapMode);
        WrapMode newWrapMode = animName == "idle" ? WrapMode.Once : basicMove.wrapMode;
        if (basicMove.clip2 != null) attachAnimation(basicMove.clip2, animName + "_2", basicMove.animationSpeed, newWrapMode);
        if (basicMove.clip3 != null) attachAnimation(basicMove.clip3, animName + "_3", basicMove.animationSpeed, newWrapMode);
        if (basicMove.clip4 != null) attachAnimation(basicMove.clip4, animName + "_4", basicMove.animationSpeed, newWrapMode);
        if (basicMove.clip5 != null) attachAnimation(basicMove.clip5, animName + "_5", basicMove.animationSpeed, newWrapMode);
        if (basicMove.clip6 != null) attachAnimation(basicMove.clip6, animName + "_6", basicMove.animationSpeed, newWrapMode);
	}

    private void attachAnimation(AnimationClip clip, string animName, float speed, WrapMode wrapMode) {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            legacyControl.AddClip(clip, animName, speed, wrapMode);
        } else {
#if !UFE_BASIC
            mecanimControl.AddClip(clip, animName, speed, wrapMode);
#endif
        }
    }

	public string GetAnimationString(BasicMoveInfo basicMove, int clipNum){
		if (clipNum == 1) return basicMove.name;
		if (clipNum == 2 && basicMove.clip2 != null) return basicMove.name + "_2";
		if (clipNum == 3 && basicMove.clip3 != null) return basicMove.name + "_3";
		if (clipNum == 4 && basicMove.clip4 != null) return basicMove.name + "_4";
		if (clipNum == 5 && basicMove.clip5 != null) return basicMove.name + "_5";
		if (clipNum == 6 && basicMove.clip6 != null) return basicMove.name + "_6";
		return basicMove.name;
	}


    public bool IsBasicMovePlaying(BasicMoveInfo basicMove) {
        if (basicMove.clip1 != null && IsAnimationPlaying(basicMove.name)) return true;
        if (basicMove.clip2 != null && IsAnimationPlaying(basicMove.name + "_2")) return true;
        if (basicMove.clip3 != null && IsAnimationPlaying(basicMove.name + "_3")) return true;
        if (basicMove.clip4 != null && IsAnimationPlaying(basicMove.name + "_4")) return true;
        if (basicMove.clip5 != null && IsAnimationPlaying(basicMove.name + "_5")) return true;
        if (basicMove.clip6 != null && IsAnimationPlaying(basicMove.name + "_6")) return true;
        return false;
    }

	public bool IsAnimationPlaying(string animationName){
		return IsAnimationPlaying(animationName, 0);
	}
	
	public bool IsAnimationPlaying(string animationName, float weight){
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            return legacyControl.IsPlaying(animationName);
		}else{
#if !UFE_BASIC
			return mecanimControl.IsPlaying(animationName, weight);
#else
            return 0;
#endif
		}
	}

	public float GetAnimationLengh(string animationName){
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            return legacyControl.GetAnimationData(animationName).clip.length;
		}else{
#if !UFE_BASIC
			return mecanimControl.GetAnimationData(animationName).clip.length;
#else
            return 0;
#endif
		}
	}

	public bool AnimationExists(string animationName){
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            return (legacyControl.GetAnimationData(animationName) != null);
		}else{
#if !UFE_BASIC
			return (mecanimControl.GetAnimationData(animationName) != null);
#else
            return false;
#endif
		}
	}
	
	public void PlayAnimation(string animationName, float blendingTime){
		PlayAnimation(animationName, blendingTime, 0);
	}
	
	public void PlayAnimation(string animationName, float blendingTime, float normalizedTime){
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            legacyControl.Play(animationName, blendingTime, normalizedTime);
		}else{
#if !UFE_BASIC
			mecanimControl.Play(animationName, blendingTime, normalizedTime, (controlsScript.mirror > 0 && UFE.config.characterRotationOptions.autoMirror));
#endif
		}
	}

	public void StopAnimation(string animationName){
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            legacyControl.Stop(animationName);
		}else{
#if !UFE_BASIC
			mecanimControl.Stop();
#endif
		}
	}
	
	public void SetAnimationSpeed(float speed){
        if (speed < 1) animationPaused = true;
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            legacyControl.SetSpeed(speed);
		}else{
#if !UFE_BASIC
			mecanimControl.SetSpeed(speed);
#endif
		}
	}

    public void SetAnimationSpeed(string animationName, float speed) {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            legacyControl.SetSpeed(animationName, speed);
		}else{
#if !UFE_BASIC
			mecanimControl.SetSpeed(animationName, speed);
#endif
		}
	}

    public void SetAnimationNormalizedSpeed(string animationName, float normalizedSpeed) {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            legacyControl.SetNormalizedSpeed(animationName, normalizedSpeed);
        } else {
#if !UFE_BASIC
            mecanimControl.SetNormalizedSpeed(animationName, normalizedSpeed);
#endif
        }
    }

	private void updateCurrentMoveFrames(float speed){
		if (speed > 0 && controlsScript.currentMove != null && controlsScript.currentMove.animationSpeedTemp != speed){
			controlsScript.currentMove.totalFrames = (int)Mathf.Abs(Mathf.Floor(
				(controlsScript.currentMove.fps * controlsScript.currentMove.animationClip.length) / speed));
			controlsScript.currentMove.animationSpeedTemp = speed;
		}
	}

	public void RestoreAnimationSpeed(){
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            legacyControl.RestoreSpeed();
            if (controlsScript.currentMove != null && legacyControl.IsPlaying(controlsScript.currentMove.name)) {
                controlsScript.currentMove.currentFrame = (int)Mathf.Round(legacyControl.GetCurrentClipPosition() * (float)controlsScript.currentMove.totalFrames);
                controlsScript.currentMove.currentTick = controlsScript.currentMove.currentFrame;
            }
		}else{
#if !UFE_BASIC
			mecanimControl.RestoreSpeed();
			if (controlsScript.currentMove != null && mecanimControl.IsPlaying(controlsScript.currentMove.name)){
				controlsScript.currentMove.currentFrame = (int)Mathf.Round(mecanimControl.GetCurrentClipPosition() * (float)controlsScript.currentMove.totalFrames);
				controlsScript.currentMove.currentTick = controlsScript.currentMove.currentFrame;
			}
#endif
		}
		animationPaused = false;
	}
	
	public void PlayBasicMove(BasicMoveInfo basicMove){
		PlayBasicMove(basicMove, basicMove.name);
	}
	
	public void PlayBasicMove(BasicMoveInfo basicMove, bool replay){
		PlayBasicMove(basicMove, basicMove.name, replay);
	}

	public void PlayBasicMove(BasicMoveInfo basicMove, string clipName){
		PlayBasicMove(basicMove, clipName, true);
	}

	public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, bool replay){
		if (overrideNextBlendingValue > -1){
			PlayBasicMove(basicMove, clipName, overrideNextBlendingValue);
			overrideNextBlendingValue = -1;
		}else if (basicMove.overrideBlendingIn){
			PlayBasicMove(basicMove, clipName, basicMove.blendingIn, replay);
		}else{
			PlayBasicMove(basicMove, clipName, controlsScript.myInfo.blendingTime, replay);
		}
		
		if (basicMove.overrideBlendingOut) overrideNextBlendingValue = basicMove.blendingOut;
	}
	
	public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, float blendingTime){
		PlayBasicMove(basicMove, clipName, blendingTime, true);
	}

	public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, float blendingTime, bool replay){
		//if (controlsScript.isBlocking) return;
		if (IsAnimationPlaying(clipName) && !replay) return;
		//SetAnimationSpeed(clipName, basicMove.animationSpeed);
		PlayAnimation(clipName, blendingTime);

		if (basicMove.disableHeadLook) {
			controlsScript.ToggleHeadLook(false);
		}else{
			controlsScript.ToggleHeadLook(true);
		}
		
		_playBasicMove(basicMove);
	}
	
	private void _playBasicMove(BasicMoveInfo basicMove){
		UFE.PlaySound(basicMove.soundEffects);
		controlsScript.currentBasicMove = basicMove.reference;

		if (basicMove.particleEffect.prefab != null) {
			GameObject pTemp = (GameObject) Instantiate(basicMove.particleEffect.prefab);
			Vector3 newPosition = hitBoxesScript.GetPosition(basicMove.particleEffect.bodyPart);
			newPosition.x += basicMove.particleEffect.offSet.x * -controlsScript.mirror;
			newPosition.y += basicMove.particleEffect.offSet.y;
			newPosition.z += basicMove.particleEffect.offSet.z;
			pTemp.transform.position = newPosition;
			if (basicMove.particleEffect.stick) pTemp.transform.parent = transform;

			Destroy(pTemp, basicMove.particleEffect.duration);
		}
	}

	
	public float GetAnimationSpeed(){
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            return legacyControl.GetSpeed();
		}else{
#if !UFE_BASIC
			return mecanimControl.GetSpeed();
#else
            return 0;
#endif
		}
	}

	public float GetAnimationSpeed(string animationName){
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            return legacyControl.GetSpeed(animationName);
		}else{
#if !UFE_BASIC
			return mecanimControl.GetSpeed(animationName);
#else
            return 0;
#endif
		}
	}

	public void SetAnimationPosition(string animationName, float normalizedTime){
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            legacyControl.SetCurrentClipPosition(normalizedTime);
		}else{
#if !UFE_BASIC
			mecanimControl.SetCurrentClipPosition(normalizedTime);
#endif
		}
	}
	
	public float GetCurrentClipPosition(){
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            return legacyControl.GetCurrentClipPosition();
		}else{
#if !UFE_BASIC
			return mecanimControl.GetCurrentClipPosition();
#else
            return 0;
#endif
		}
	}

	public float GetAnimationTime(int animFrame, MoveInfo move){
		if (move == null) return 0;
		if (move.animationSpeed < 0){
			return (((float)animFrame/(float)UFE.config.fps) * move.animationSpeed) + move.animationClip.length;
		}else{
			return ((float)animFrame/(float)UFE.config.fps) * move.animationSpeed;
		}
	}

	public float GetAnimationNormalizedTime(int animFrame, MoveInfo move){
		if (move == null) return 0;
		if (move.animationSpeed < 0){
			return ((float)animFrame/ (float)move.totalFrames) + 1;
		}else{
			return (float)animFrame/ (float)move.totalFrames;
		}
	}
	
	public void SetMecanimMirror(bool toggle){
#if !UFE_BASIC
		mecanimControl.SetMirror(toggle, UFE.config.characterRotationOptions.mirrorBlending, true);
#endif
	}

	public bool CompareBlockButtons(ButtonPress button){
		if (button == ButtonPress.Button1 && UFE.config.blockOptions.blockType == BlockType.HoldButton1) return true;
		if (button == ButtonPress.Button2 && UFE.config.blockOptions.blockType == BlockType.HoldButton2) return true;
		if (button == ButtonPress.Button3 && UFE.config.blockOptions.blockType == BlockType.HoldButton3) return true;
		if (button == ButtonPress.Button4 && UFE.config.blockOptions.blockType == BlockType.HoldButton4) return true;
		if (button == ButtonPress.Button5 && UFE.config.blockOptions.blockType == BlockType.HoldButton5) return true;
		if (button == ButtonPress.Button6 && UFE.config.blockOptions.blockType == BlockType.HoldButton6) return true;
		if (button == ButtonPress.Button7 && UFE.config.blockOptions.blockType == BlockType.HoldButton7) return true;
		if (button == ButtonPress.Button8 && UFE.config.blockOptions.blockType == BlockType.HoldButton8) return true;
		if (button == ButtonPress.Button9 && UFE.config.blockOptions.blockType == BlockType.HoldButton9) return true;
		if (button == ButtonPress.Button10 && UFE.config.blockOptions.blockType == BlockType.HoldButton10) return true;
		if (button == ButtonPress.Button11 && UFE.config.blockOptions.blockType == BlockType.HoldButton11) return true;
		if (button == ButtonPress.Button12 && UFE.config.blockOptions.blockType == BlockType.HoldButton12) return true;
		return false;
	}
	
	public bool CompareParryButtons(ButtonPress button){
		if (button == ButtonPress.Button1 && UFE.config.blockOptions.parryType == ParryType.TapButton1) return true;
		if (button == ButtonPress.Button2 && UFE.config.blockOptions.parryType == ParryType.TapButton2) return true;
		if (button == ButtonPress.Button3 && UFE.config.blockOptions.parryType == ParryType.TapButton3) return true;
		if (button == ButtonPress.Button4 && UFE.config.blockOptions.parryType == ParryType.TapButton4) return true;
		if (button == ButtonPress.Button5 && UFE.config.blockOptions.parryType == ParryType.TapButton5) return true;
		if (button == ButtonPress.Button6 && UFE.config.blockOptions.parryType == ParryType.TapButton6) return true;
		if (button == ButtonPress.Button7 && UFE.config.blockOptions.parryType == ParryType.TapButton7) return true;
		if (button == ButtonPress.Button8 && UFE.config.blockOptions.parryType == ParryType.TapButton8) return true;
		if (button == ButtonPress.Button9 && UFE.config.blockOptions.parryType == ParryType.TapButton9) return true;
		if (button == ButtonPress.Button10 && UFE.config.blockOptions.parryType == ParryType.TapButton10) return true;
		if (button == ButtonPress.Button11 && UFE.config.blockOptions.parryType == ParryType.TapButton11) return true;
		if (button == ButtonPress.Button12 && UFE.config.blockOptions.parryType == ParryType.TapButton12) return true;
		return false;
	}

	private bool hasEnoughGauge(float gaugeNeeded){
		if (!UFE.config.gameGUI.hasGauge) return true;
		if (controlsScript.myInfo.currentGaugePoints < gaugeNeeded) return false;
		return true;
	}

	public MoveInfo GetIntro(){
		return InstantiateMove(intro);
	}
	
	public MoveInfo GetOutro(){
		return InstantiateMove(outro);
	}
	
	public MoveInfo InstantiateMove(MoveInfo move){
		if (move == null) return null;
		MoveInfo newMove = Instantiate(move) as MoveInfo;
		newMove.name = move.name;
		return newMove;
	}

	public MoveInfo GetNextMove(MoveInfo currentMove){
		if (currentMove.frameLinks.Length == 0) return null;
		foreach(FrameLink frameLink in currentMove.frameLinks){
			if (frameLink.linkableMoves.Length == 0) continue;
			if (frameLink.cancelable){
				foreach(MoveInfo move in frameLink.linkableMoves){
					if (move.buttonExecution.Length == 0 || frameLink.ignoreInputs){
						return InstantiateMove(move);
					}
				}
			}
		}
		return null;
	}

	public void ClearLastButtonSequence(){
		lastButtonPresses.Clear();
		
		foreach(ButtonPress bp in Enum.GetValues(typeof(ButtonPress))){
			chargeValues[bp] = 0;
		}
	}

	private bool checkExecutionState(ButtonPress[] buttonPress, bool inputUp){
		if (inputUp 
		    && lastButtonPresses.Count > 0 
			&& buttonPress[0].Equals(lastButtonPresses.ToArray()[lastButtonPresses.Count - 1])) return false;

		return true;
	}

	public MoveInfo GetMove(ButtonPress[] buttonPress, float charge, MoveInfo currentMove, bool inputUp){
		return GetMove(buttonPress, charge, currentMove, inputUp, false);
	}

	public MoveInfo GetMove(ButtonPress[] buttonPress, float charge, MoveInfo currentMove, bool inputUp, bool forceExecution){
		if (buttonPress.Length > 0 
		    && Time.time - lastTimePress <= controlsScript.myInfo.executionTiming) {

			if (controlsScript.debugInfo.buttonSequence){
				string allbp = "";
				foreach(ButtonPress bp in lastButtonPresses) allbp += bp.ToString() + " ";
				string allbp2 = "";
				foreach(ButtonPress bp in buttonPress) allbp2 += bp.ToString() + " ";
				Debug.Log("Sequence: ( "+ allbp + ") | Execution: " + "( " + allbp2 + ")");
			}

			// Attempt execution
			foreach(MoveInfo move in moves) {
				MoveInfo newMove = TestMoveExecution(move, currentMove, buttonPress, inputUp, true);
				if (newMove != null) return newMove;
			}
		}

		if (buttonPress.Length > 0) {
			if (Time.time - lastTimePress > controlsScript.myInfo.executionTiming){
				ClearLastButtonSequence();
			}

			if (!forceExecution){
				lastTimePress = Time.time;
				// Store sequence
				if (!inputUp || (inputUp && lastButtonPresses.Count == 0)){
					lastButtonPresses.Add(buttonPress[0]);
					if (charge > 0) chargeValues[buttonPress[0]] = charge;
				}
			}
			
			
			// Attempt execution one more time
			foreach(MoveInfo move in moves) {
				MoveInfo newMove = TestMoveExecution(move, currentMove, buttonPress, inputUp, false, forceExecution);
				if (newMove != null) return newMove;
			}
		}

		return null;
	}


	private bool searchMove(string moveName, FrameLink[] frameLinks){
		return searchMove(moveName, frameLinks, false);
	}

    private bool searchMove(string moveName, FrameLink[] frameLinks, int currentFrame) {

        foreach (FrameLink frameLink in frameLinks) {
            if ((currentFrame >= frameLink.activeFramesBegins && currentFrame <= frameLink.activeFramesEnds)
                || (currentFrame >= (frameLink.activeFramesBegins - UFE.config.executionBufferTime)
                && currentFrame <= frameLink.activeFramesEnds)) {
                foreach (MoveInfo move in frameLink.linkableMoves) {
                    if (moveName == move.moveName) return true;
                }
            }
        }

        return false;
    }

	private bool searchMove(string moveName, FrameLink[] frameLinks, bool ignoreConditions){
		//if (moveName.IndexOf("(Clone)") != -1)
		//	moveName = moveName.Substring(0, moveName.Length - 7);
		
		foreach(FrameLink frameLink in frameLinks){
			if (frameLink.cancelable){
				if (ignoreConditions && !frameLink.ignorePlayerConditions) continue;

				foreach(MoveInfo move in frameLink.linkableMoves){
					if (moveName == move.moveName) return true;
				}
			}
		}
		
		return false;
	}

	private bool searchMove(string moveName, MoveInfo[] moves){
		foreach(MoveInfo move in moves)
			if (moveName == move.moveName) return true;
		
		return false;
	}

	public bool HasMove(string moveName){
		foreach(MoveInfo move in this.moves)
			if (moveName == move.moveName) return true;
		
		return false;
	}


	public bool ValidateMoveExecution(MoveInfo move){
		if (!ValidateMoveStances(move.selfConditions, controlsScript, true)) return false;
		if (!ValidateMoveStances(move.opponentConditions, controlsScript.opControlsScript)) return false;
		if (!ValidadeBasicMove(move.selfConditions, controlsScript)) return false;
		if (!ValidadeBasicMove(move.opponentConditions, controlsScript.opControlsScript)) return false;
		if (!hasEnoughGauge(move.gaugeUsage)) return false;
		if (move.previousMoves.Length > 0 && controlsScript.currentMove == null) return false;
		if (move.previousMoves.Length > 0 && !searchMove(controlsScript.currentMove.moveName, move.previousMoves)) return false;

		if (controlsScript.currentMove != null && controlsScript.currentMove.frameLinks.Length == 0) return false;
		if (controlsScript.currentMove != null && !searchMove(move.moveName, controlsScript.currentMove.frameLinks)) return false;
		return true;
	}

	
	public bool ValidateMoveStances(PlayerConditions conditions, ControlsScript cScript){
		return ValidateMoveStances(conditions, cScript, false);
	}

	public bool ValidateMoveStances(PlayerConditions conditions, ControlsScript cScript, bool bypassCrouchStance){
		bool stateCheck = conditions.possibleMoveStates.Length > 0? false : true;
		foreach(PossibleMoveStates possibleMoveState in conditions.possibleMoveStates){

			if (possibleMoveState.possibleState != cScript.currentState
			    && (!bypassCrouchStance || (bypassCrouchStance && cScript.currentState != PossibleStates.Stand))) continue;
			
			if (cScript.normalizedDistance < (float)possibleMoveState.proximityRangeBegins/100) continue;
			if (cScript.normalizedDistance > (float)possibleMoveState.proximityRangeEnds/100) continue;
			
			if (cScript.currentState == PossibleStates.Stand){ 
				if (!possibleMoveState.standBy && cScript.currentSubState == SubStates.Resting) continue;
				if (!possibleMoveState.movingBack && cScript.currentSubState == SubStates.MovingBack) continue;
				if (!possibleMoveState.movingForward && cScript.currentSubState == SubStates.MovingForward) continue;

			}else if (cScript.currentState == PossibleStates.StraightJump
			          || cScript.currentState == PossibleStates.ForwardJump
			          || cScript.currentState == PossibleStates.BackJump){ 
				
				if (cScript.normalizedJumpArc < (float)possibleMoveState.jumpArcBegins/100) continue;
				if (cScript.normalizedJumpArc > (float)possibleMoveState.jumpArcEnds/100) continue;
			}
			
			if (!possibleMoveState.blocking && (cScript.currentSubState == SubStates.Blocking || cScript.isBlocking)) continue;

			if ((!possibleMoveState.stunned && possibleMoveState.possibleState != PossibleStates.Down) 
			    && cScript.currentSubState == SubStates.Stunned) continue;

			stateCheck = true;
		}
		return stateCheck;
	}

	public bool ValidadeBasicMove(PlayerConditions conditions, ControlsScript cScript){
		if (conditions.basicMoveLimitation.Length == 0) return true;
		if (System.Array.IndexOf(conditions.basicMoveLimitation, cScript.currentBasicMove) != -1) return true;
		return false;
	}
	
	private MoveInfo TestMoveExecution(MoveInfo move, MoveInfo currentMove, ButtonPress[] buttonPress, bool inputUp, bool fromSequence) {
		return TestMoveExecution(move, currentMove, buttonPress, inputUp, fromSequence, false);
	}

	private MoveInfo TestMoveExecution(MoveInfo move, MoveInfo currentMove, ButtonPress[] buttonPress, bool inputUp, bool fromSequence, bool forceExecution) {
		if (!hasEnoughGauge(move.gaugeUsage)) return null;
		if (move.previousMoves.Length > 0 && currentMove == null) return null;
		if (move.previousMoves.Length > 0 && !searchMove(currentMove.moveName, move.previousMoves)) return null;

		if (currentMove == null || (currentMove != null && !searchMove(move.moveName, currentMove.frameLinks, true))){
			if (!ValidateMoveStances(move.selfConditions, controlsScript)) return null;
			if (!ValidateMoveStances(move.opponentConditions, controlsScript.opControlsScript)) return null;
			if (!ValidadeBasicMove(move.selfConditions, controlsScript)) return null;
			if (!ValidadeBasicMove(move.opponentConditions, controlsScript.opControlsScript)) return null;
		}

		Array.Sort(buttonPress);
		Array.Sort(move.buttonExecution);


		if (fromSequence){
			if (move.buttonSequence.Length == 0) return null;
			if (move.chargeMove){
				if (chargeValues[move.buttonSequence[0]] < move.chargeTiming) return null;
			}

			// Uncomment these to view how the engine filters special executions
			/*
			string allbp = "";
			foreach(ButtonPress bp in lastButtonPresses) allbp += " "+ bp.ToString();
			Debug.Log(allbp);
			string allbp2 = "";
			foreach(ButtonPress bp in move.buttonSequence) allbp2 += " "+ bp.ToString();
			Debug.Log(allbp +"="+ allbp2 +"? "+ ArraysEqual<ButtonPress>(buttonPress, move.buttonExecution));
			 */

			if (lastButtonPresses.Count >= move.buttonSequence.Length){
				ButtonPress[] compareSequence;
				int compareRange = lastButtonPresses.Count - move.buttonSequence.Length;
				if (move.allowInputLeniency){
					compareRange -= move.leniencyBuffer;
					if (compareRange < 0) compareRange = 0;
					compareSequence = lastButtonPresses.GetRange(compareRange, lastButtonPresses.Count - compareRange).ToArray();
					compareSequence = ArrayIntersect<ButtonPress>(move.buttonSequence, compareSequence);

					/*string allbp = "";
					foreach(ButtonPress bp in lastButtonPresses) allbp += " "+ bp.ToString();
					Debug.Log(move.moveName + ": lastButtonPresses (leniency): "+ allbp);

					string allbp2 = "";
					foreach(ButtonPress bp in move.buttonSequence) allbp2 += " "+ bp.ToString();
					Debug.Log(move.moveName + ": move.buttonSequence (leniency): "+ allbp2);

					if (compareSequence != null){
						allbp3 = "";
						foreach(ButtonPress bp in compareSequence) allbp3 += " "+ bp.ToString();
						Debug.Log(move.moveName + ": compareSequence (leniency): "+ allbp3);
					}*/

				}else{
					compareSequence = lastButtonPresses.GetRange(compareRange, move.buttonSequence.Length).ToArray();

					/*string allbp3 = "";
					foreach(ButtonPress bp in compareSequence) allbp3 += " "+ bp.ToString();
					Debug.Log(move.moveName + ": compareSequence: "+ allbp3);*/
				}
				
				if (!ArraysEqual<ButtonPress>(compareSequence, move.buttonSequence)) return null;
			}else{
				return null;
			}
			
			//Debug.Log("Sequence for "+ move.moveName +" pass! Testing Execution:"+ ArraysEqual<ButtonPress>(buttonPress, move.buttonExecution));
		}else{
			if (move.buttonSequence.Length > 0) return null;
		}

		if (!inputUp && !move.onPressExecution) return null;
		if (inputUp && !move.onReleaseExecution) return null;
		if (!ArraysEqual<ButtonPress>(buttonPress, move.buttonExecution)) return null;

		if (controlsScript.storedMove != null && move.moveName == controlsScript.storedMove.moveName)
			return controlsScript.storedMove;

        /*string allbp4 = "";
        foreach (ButtonPress bp in buttonPress) allbp4 += " " + bp.ToString();
        Debug.Log(move.moveName + ": Button Execution: " + allbp4);*/

        if (currentMove == null || forceExecution || (searchMove(move.moveName, currentMove.frameLinks, currentMove.currentFrame)) ||
		    UFE.config.executionBufferType == ExecutionBufferType.AnyMove){
			MoveInfo newMove = InstantiateMove(move);
			UFE.FireMove(newMove, controlsScript.myInfo);
			
			if ((controlsScript.currentState == PossibleStates.StraightJump ||
			    controlsScript.currentState == PossibleStates.ForwardJump ||
			    controlsScript.currentState == PossibleStates.BackJump) &&
			    totalAirMoves >= controlsScript.myInfo.possibleAirMoves) return null;

			return newMove;
		}

		return null;
	}
	
	private T[] ArrayIntersect<T>(T[] a1, T[] a2) {
		if (a1 == null || a2 == null) return null;
		
		EqualityComparer<T> comparer = EqualityComparer<T>.Default;
		List<T> intersection = new List<T>();
		int nextStartingPoint = 0;
		for (int i = 0; i < a1.Length; i++){ // button sequence
			bool added = false;
			for (int k = nextStartingPoint; k < a2.Length; k++){ // button presses
				if (comparer.Equals(a1[i], a2[k])) {
					intersection.Add(a2[k]);
					nextStartingPoint = k;
					added = true;
					break;
				}
			}
			if (!added) return null;
		}

		return intersection.ToArray();
	}

	private bool ArraysEqual<T>(T[] a1, T[] a2) {
    	if (ReferenceEquals(a1,a2)) return true;
  		if (a1 == null || a2 == null) return false;
		if (a1.Length != a2.Length) return false;
	    EqualityComparer<T> comparer = EqualityComparer<T>.Default;
		for (int i = 0; i < a1.Length; i++){
        	if (!comparer.Equals(a1[i], a2[i])) return false;
    	}
    	return true;
	}
}
