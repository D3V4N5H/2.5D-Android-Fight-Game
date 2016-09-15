using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public enum ButtonPress {
	Foward,
	Back,
	Up,
	Down,
	Button1,
	Button2,
	Button3,
	Button4,
	Button5,
	Button6,
	Button7,
	Button8,
	Button9,
	Button10,
	Button11,
	Button12,
	Start
}

public enum BasicMoveReference {
	Idle,
	MoveForward,
	MoveBack,
	TakeOff,
	JumpStraight,
	JumpBack,
	JumpForward,
	FallStraight,
	FallBack,
	FallForward,
	Landing,
	Crouching,
	BlockingCrouchingPose,
	BlockingCrouchingHit,
	BlockingHighPose,
	BlockingHighHit,
	BlockingLowHit,
	BlockingAirPose,
	BlockingAirHit,
	ParryCrouching,
	ParryLow,
	ParryHigh,
	ParryAir,
	Bounce,
	FallingFromBounce,
	FallDown,
	GetHitCrouching,
	GetHitHigh,
	GetHitLow,
	GetHitHighKnockdown,
	GetHitHighLowKnockdown,
	GetHitAir,
	GetHitCrumple,
	GetHitKnockBack,
	GetHitSweep,
	StandUp
}

public enum ClipNum {
	Clip1,
	Clip2,
	Clip3,
	Clip4,
	Clip5,
	Clip6
}

public enum StandUpOptions {
	None,
	DefaultClip,
	HighKnockdownClip,
	LowKnockdownClip,
	SweepClip
}

public enum InputType {
	HorizontalAxis,
	VerticalAxis,
	Button
}

public enum PossibleStates {
	Stand,
	Crouch,
	StraightJump,
	ForwardJump,
	BackJump,
	Down
}

public enum SubStates {
	Resting,
	MovingForward,
	MovingBack,
	Blocking,
	Stunned
}

public enum CombatStances {
	Stance1,
	Stance2,
	Stance3,
	Stance4,
	Stance5,
	Stance6,
	Stance7,
	Stance8,
	Stance9,
	Stance10
}

public enum DamageType {
	Percentage,
	Points
}

public enum AttackType {
	Neutral,
	NormalAttack,
	ForwardLauncher,
	BackLauncher,
	Dive,
	AntiAir,
	Projectile
}

public enum GaugeUsage {
	Any,
	None,
	Quarter,
	Half,
	ThreeQuarters,
	All
}

public enum ProjectileType {
	Shot,
	Beam
}

public enum HitConfirmType {
	Hit,
	Throw
}

public enum HitType {
	Mid,
	Low,
	Overhead,
	Launcher,
	HighKnockdown,
	MidKnockdown,
	KnockBack,
	Sweep
}

public enum HitStrengh {
	Weak,
	Medium,
	Heavy,
	Crumple,
	Custom1,
	Custom2,
	Custom3
}

public enum HitStunType {
	FrameAdvantage,
	Frames,
	Seconds
}

public enum LinkType {
	HitConfirm,
	CounterMove,
	NoConditions
}

public enum CounterMoveType {
	MoveFilter,
	SpecificMove
}

public enum CinematicType {
	CameraEditor,
	AnimationFile,
	Prefab
}

public enum CharacterDistance {
	Any,
	VeryClose,
	Close,
	Mid,
	Far,
	VeryFar,
	Other
}

public enum FrameSpeed {
	Any,
	VerySlow,
	Slow,
	Normal,
	Fast,
	VeryFast
}

public enum JumpArc {
	Any,
	TakeOff,
	Jumping,
	Top,
	Falling,
	Landing,
	Other
}

public enum CurrentFrameData {
	Any,
	StartupFrames,
	ActiveFrames,
	RecoveryFrames
}

[System.Serializable]
public class Projectile: ICloneable {
	public int castingFrame = 1;
	public GameObject projectilePrefab;
	public GameObject impactPrefab;

	public BodyPart bodyPart;
	public Vector3 castingOffSet;
	public int speed = 20;
	public int directionAngle;
	public float duration;
	public float impactDuration = 1;
	public bool fixedZAxis;
	public bool projectileCollision;
	public bool unblockable;

	public HitBox hitBox;
	public HurtBox hurtBox;
	public BlockArea blockableArea;

	/*public HitBoxShape shape;
	public Rect rect = new Rect(0, 0, 4, 4);
	public bool followXBounds;
	public bool followYBounds;
	public float hitRadius;
	public Vector2 hitOffSet;*/
	
	public Sizes spaceBetweenHits;
	public int totalHits = 1;
	public bool resetPreviousHitStun = true;
	public int hitStunOnHit;
	public int hitStunOnBlock;
	
	public bool overrideHitEffects;
	public HitTypeOptions hitEffects;

	public bool groundHit;
	public bool airHit;
	public bool downHit;

	public DamageType damageType;
	public float damageOnHit;
	public float damageOnBlock;
	public bool damageScaling;

	
	public Vector2 pushForce;
	public HitStrengh hitStrength;
	public HitType hitType;

    public MoveInfo moveLinkOnStrike;
    public MoveInfo moveLinkOnBlock;
    public MoveInfo moveLinkOnParry;
    public bool forceGrounded;
    
	[HideInInspector] public bool moveLinksToggle;
	[HideInInspector] public bool damageOptionsToggle;
	[HideInInspector] public bool hitStunOptionsToggle;
	[HideInInspector] public bool preview;

	public bool casted{get; set;}
	public int gaugeGainOnHit{get; set;}
	public int gaugeGainOnBlock{get; set;}
	public int opGaugeGainOnHit{get; set;}
	public int opGaugeGainOnBlock{get; set;}
	public int opGaugeGainOnParry{get; set;}
	public Transform position{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class InvincibleBodyParts: ICloneable {
	public BodyPart[] bodyParts = new BodyPart[0];
	public bool completelyInvincible = true;
	public bool ignoreBodyColliders = false;
	public int activeFramesBegin;
	public int activeFramesEnds;

	[HideInInspector]	public HitBox[] hitBoxes;
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}


[System.Serializable]
public class AppliedForce: ICloneable {
	public int castingFrame;
	public bool resetPreviousVertical;
	public bool resetPreviousHorizontal;
	public Vector2 force;

	public bool casted{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class Hit: ICloneable {
	public int activeFramesBegin;
	public int activeFramesEnds;
	public HitConfirmType hitConfirmType;
	public MoveInfo throwMove;
	public MoveInfo techMove;
	public bool techable = true;
	public bool armorBreaker;
	public bool continuousHit;
	public Sizes spaceBetweenHits;
	public bool groundHit = true;
	public bool airHit = true;
	public bool stunHit = true;
	public bool downHit;
	public bool resetPreviousHitStun;
	public bool cornerPush = true;

	public bool overrideHitEffects;
	public HitTypeOptions hitEffects;

	public HitStrengh hitStrength;
	public HitStunType hitStunType = HitStunType.Frames;
	public float hitStunOnHit;
	public float hitStunOnBlock;
	public int frameAdvantageOnHit;
	public int frameAdvantageOnBlock;
	public bool damageScaling;
	public DamageType damageType;
	public float damageOnHit;
	public float damageOnBlock;
	public bool doesntKill;
   	public HitType hitType;
	public bool resetPreviousHorizontalPush;
	public bool resetPreviousVerticalPush;
	public Vector2 pushForce;
	public bool resetPreviousHorizontal;
	public bool resetPreviousVertical;
	public Vector2 appliedForce;
	public HurtBox[] hurtBoxes = new HurtBox[0];
	public PullIn pullEnemyIn;
	public PullIn pullSelfIn;

	[HideInInspector]	public bool damageOptionsToggle;
	[HideInInspector]	public bool hitStunOptionsToggle;
	[HideInInspector]	public bool forceOptionsToggle;
	[HideInInspector]	public bool hitConditionsToggle;
	[HideInInspector]	public bool pullInToggle;
	[HideInInspector]	public bool hurtBoxesToggle;

	
	public bool disabled{get; set;}

	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class FrameLink: ICloneable {
	public LinkType linkType = LinkType.NoConditions;
	public bool allowBuffer = true;
	public bool onStrike = true;
	public bool onBlock = true;
	public bool onParry = true;
	public int activeFramesBegins;
	public int activeFramesEnds;
	public CounterMoveType counterMoveType;
	public MoveInfo counterMoveFilter;
	public bool disableHitImpact = true;
	public bool anyHitStrength = true;
	public HitStrengh hitStrength;
	public bool anyStrokeHitBox = true;
	public HitBoxType hitBoxType;
	public bool anyHitType = true;
	public HitType hitType;
	public bool ignoreInputs;
	public bool ignorePlayerConditions;
	public int nextMoveStartupFrame = 1;
	public MoveInfo[] linkableMoves = new MoveInfo[0];

	[HideInInspector]	public bool cancelable;
	[HideInInspector]	public bool linkableMovesToggle;
	[HideInInspector]	public bool hitConfirmToggle;
	[HideInInspector]	public bool counterMoveToggle;
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class MoveParticleEffect: ICloneable {
	public int castingFrame;
	public ParticleInfo particleEffect;

	public bool casted{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class SlowMoEffect: ICloneable {
	public int castingFrame;
	public float duration;
	public float percentage;

	public bool casted{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class SoundEffect: ICloneable {
	public int castingFrame;
	/*[System.Obsolete]*/ public AudioClip sound;
	public AudioClip[] sounds = new AudioClip[0];
	
	[HideInInspector]	public bool soundEffectsToggle;

	public bool casted{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class InGameAlert: ICloneable {
	public int castingFrame;
	public string alert;
	
	public bool casted{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class StanceChange: ICloneable {
	public int castingFrame;
	public CombatStances newStance;
	
	public bool casted{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class ArmorOptions {
	public int activeFramesBegin;
	public int activeFramesEnds;
	
	public bool overrideHitEffects;
	public HitTypeOptions hitEffects;

	public int hitAbsorption;
	public int damageAbsorption;
	public BodyPart[] nonAffectedBodyParts = new BodyPart[0];
}

[System.Serializable]
public class CameraMovement: ICloneable {
	public CinematicType cinematicType = CinematicType.CameraEditor;
	public AnimationClip animationClip;
	public GameObject prefab;
	public float camAnimationSpeed = 1;
	public float blendSpeed = 100;
	public Vector3 gameObjectPosition;
	public Vector3 position;
	public Vector3 rotation;
	public int castingFrame;
	public float duration;
	public float fieldOfView;
	public float camSpeed = 2;
	public bool freezePhysics;
	public float myAnimationSpeed = 100;
	public float opAnimationSpeed = 100;
	public bool previewToggle;
	public float startTime;
	
	public bool casted{get; set;}
	public bool over{get; set;}
	public float time{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class OpponentOverride: ICloneable {
	public Vector3 position;
	public int castingFrame;
	public int blendSpeed = 80;
	public bool stun;
	public float stunTime;
	public bool overrideHitAnimations;
	public bool resetAppliedForces;
	
	// End Options
	public StandUpOptions standUpOptions;

	// Options
	public bool characterSpecific;

	// Move
	public MoveInfo move;
	// Character Specific Moves
	public CharacterSpecificMoves[] characterSpecificMoves = new CharacterSpecificMoves[0];
	
	[HideInInspector]	public bool animationPreview = false;
	[HideInInspector]	public bool movesToggle = false;

	public bool casted{get; set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class CharacterSpecificMoves {
	public MoveInfo move;
	public string characterName;
}


[System.Serializable]
public class PossibleMoveStates: ICloneable {
	public PossibleStates possibleState;
	public JumpArc jumpArc;
	public int jumpArcBegins = 0;
	public int jumpArcEnds = 100;
	
	public CharacterDistance opponentDistance;
	public int proximityRangeBegins = 0;
	public int proximityRangeEnds = 100;

	public bool movingForward = true;
	public bool movingBack = true;

	public bool standBy = true;
	public bool blocking;
	public bool stunned;
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class PlayerConditions {
	public BasicMoveReference[] basicMoveLimitation = new BasicMoveReference[0];
	public PossibleMoveStates[] possibleMoveStates = new PossibleMoveStates[0];

	[HideInInspector]	public bool basicMovesToggle = false;
	[HideInInspector]	public bool statesToggle = false;
}

[System.Serializable]
public class MoveClassification {
    public AttackType attackType;
    public HitType hitType;
    public FrameSpeed startupSpeed;
    public FrameSpeed recoverySpeed;
    public HitConfirmType hitConfirmType;
    public CharacterDistance preferableDistance;
    public GaugeUsage gaugeUsage;
    public bool anyAttackType = true;
    public bool anyHitType = true;
    public bool anyHitConfirmType = true;
}

/*[System.Serializable]
public class MoveInput {
    public bool chargeMove;
    public float chargeTiming = .7f;
    public bool allowInputLeniency;
    public int leniencyBuffer = 3;
    public bool onReleaseExecution;
    public bool onPressExecution = true;
    public ButtonPress[] buttonSequence = new ButtonPress[0];
    public ButtonPress[] buttonExecution = new ButtonPress[0];

	[HideInInspector]	public bool buttonSequenceToggle = false;
	[HideInInspector]	public bool buttonExecutionToggle = false;
}*/

[System.Serializable]
public class MoveInfo: ScriptableObject {
	public GameObject characterPrefab;
	public string moveName;
	public string description;
	public int fps = 60;
	public bool ignoreGravity;
	public bool ignoreFriction;
	public bool cancelMoveWheLanding;
	public bool forceMirrorLeft;
	public bool forceMirrorRight;
	public bool invertRotationLeft;
	public bool invertRotationRight;
	public bool autoCorrectRotation;
	public int frameWindowRotation;

	
	public bool gaugeToggle;
	public int gaugeUsage;
	public int gaugeGainOnMiss;
	public int gaugeGainOnHit;
	public int gaugeGainOnBlock;
	public int opGaugeGainOnBlock;
	public int opGaugeGainOnParry;
	public int opGaugeGainOnHit;

	/*[System.Obsolete]*/ public PossibleStates[] possibleStates = new PossibleStates[0];
	/*[System.Obsolete]*/ public PossibleMoveStates[] possibleMoveStates = new PossibleMoveStates[0];

	public AnimationClip animationClip;
	public bool disableHeadLook = true;
	public WrapMode wrapMode;
	public float animationSpeed = 1;
	public int totalFrames = 15;
	public int startUpFrames = 5;
	public int activeFrames = 5;
	public int recoveryFrames = 5;
	public bool applyRootMotion = false;
	public bool forceGrounded = false;
	public BodyPart rootMotionNode = BodyPart.none;
	public bool overrideBlendingIn = true;
	public bool overrideBlendingOut = false;
	public float blendingIn = 0;
	public float blendingOut = 0;

    
	public bool chargeMove;
	public float chargeTiming = .7f;
	public bool allowInputLeniency;
	public int leniencyBuffer = 3;
	public bool onReleaseExecution;
	public bool onPressExecution = true;
	public ButtonPress[] buttonSequence = new ButtonPress[0];
	public ButtonPress[] buttonExecution = new ButtonPress[0];
    //public MoveInput[] inputOptions = new MoveInput[0];


	public MoveInfo[] previousMoves = new MoveInfo[0];
	public FrameLink[] frameLinks = new FrameLink[0];
	
	public MoveParticleEffect[] particleEffects = new MoveParticleEffect[0];
	public AppliedForce[] appliedForces = new AppliedForce[0];
	public SlowMoEffect[] slowMoEffects = new SlowMoEffect[0];
	
	public OpponentOverride[] opponentOverride = new OpponentOverride[0];
	public SoundEffect[] soundEffects = new SoundEffect[0];
	public InGameAlert[] inGameAlert = new InGameAlert[0];
	public StanceChange[] stanceChanges = new StanceChange[0];
	public CameraMovement[] cameraMovements = new CameraMovement[0];
	
	public Hit[] hits = new Hit[0];
	public BlockArea blockableArea;
	public InvincibleBodyParts[] invincibleBodyParts = new InvincibleBodyParts[0];
	public ArmorOptions armorOptions;
	
	public Projectile[] projectiles = new Projectile[0];
	public PlayerConditions opponentConditions;
	public PlayerConditions selfConditions;
	public MoveClassification moveClassification;

	
	[HideInInspector] public ButtonPress[][] simulatedInputs;

	public bool cancelable{get; set;}
	public bool kill{get; set;}
	public int currentFrame {get; set;}
	public int overrideStartupFrame{get; set;}
	public float animationSpeedTemp{get; set;}
	public float currentTick{get; set;}
	public bool hitConfirmOnBlock{get; set;}
	public bool hitConfirmOnParry{get; set;}
	public bool hitConfirmOnStrike{get; set;}
	public bool hitAnimationOverride{get; set;}
	public StandUpOptions standUpOptions{get; set;}
	public CurrentFrameData currentFrameData{get; set;}

	public bool IsThrow(bool techable){
		foreach (Hit hit in this.hits){
			//if (this.currentFrame >= hit.activeFramesBegin && this.currentFrame < hit.activeFramesEnds) {
				if (hit.hitConfirmType == HitConfirmType.Throw && hit.techable == techable) return true;
			//}
		}
		return false;
	}
	
	public MoveInfo GetTechMove(){
		foreach (Hit hit in this.hits){
			//if (this.currentFrame >= hit.activeFramesBegin && this.currentFrame < hit.activeFramesEnds) {
				if (hit.hitConfirmType == HitConfirmType.Throw && hit.techable) return hit.techMove;
			//}
		}
		return null;
	}
}