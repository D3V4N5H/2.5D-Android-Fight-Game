using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class PhysicsData {
	public float moveForwardSpeed = 4f; // How fast this character can move forward
	public float moveBackSpeed = 3.5f; // How fast this character can move backwards
	public bool highMovingFriction = true; // When releasing the horizontal controls character will stop imediatelly
	public float friction = 30f; // Friction used in case of highMovingFriction false. Also used when player is pushed

	public bool canJump = true;
	public float jumpForce = 20f; // How high this character will jumps
	public float jumpDistance = 8f; // How far this character will move horizontally while jumping
	public bool cumulativeForce = true; // If this character is being juggled, should new forces add to or replace existing force?
	public int multiJumps = 1; // Can this character double or triple jump? Set how many times the character can jump here
	public float weight = 175;
	public int jumpDelay = 4;
	public int landingDelay = 7;
	public float groundCollisionMass = 2;
}

[System.Serializable]
public class HeadLook {
	public bool enabled = false;
	public BendingSegment[] segments = new BendingSegment[0];
	public NonAffectedJoints[] nonAffectedJoints = new NonAffectedJoints[0];
	public BodyPart target = BodyPart.head;
	public float effect = 1;
	public bool overrideAnimation = true;
	public bool disableOnHit = true;
}

[System.Serializable]
public class MoveSetData: ICloneable {
	public CombatStances combatStance = CombatStances.Stance1; // This move set combat stance
	public MoveInfo cinematicIntro;
	public MoveInfo cinematicOutro;

	public BasicMoves basicMoves = new BasicMoves(); // List of basic moves
	public MoveInfo[] attackMoves = new MoveInfo[0]; // List of attack moves
	
	[HideInInspector] public bool enabledBasicMovesToggle;
	[HideInInspector] public bool basicMovesToggle;
	[HideInInspector] public bool attackMovesToggle;
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class CharacterInfo: ScriptableObject {
	public Texture2D profilePictureSmall;
	public Texture2D profilePictureBig;
	public string characterName;
	public Gender gender;
	public string characterDescription;
	public bool enableAlternativeColor;
	public Color alternativeColor;
	public AudioClip selectionSound;
	public AudioClip deathSound;
	public float height;
	public int age;
	public string bloodType;
	public int lifePoints = 1000;
	public int maxGaugePoints;
	public GameObject characterPrefab; // The prefab representing the character (must have hitBoxScript attached to it)

	public PhysicsData physics;
	public HeadLook headLook;
	
	public float executionTiming = .3f; // How fast the player needs to press each key during the execution of a special move
	public int possibleAirMoves = 1; // How many moves this character can perform while in the air
    public float blendingTime = .1f; // The speed of transiction between basic moves

	public AnimationType animationType;
	public Avatar avatar; // Mecanim variable
	public bool applyRootMotion; // Mecanim variable
	public AnimationFlow animationFlow;

	public MoveSetData[] moves = new MoveSetData[0];
	public AIInstructionsSet[] aiInstructionsSet = new AIInstructionsSet[0];
	

	public CombatStances currentCombatStance{get; set;}
	public float currentLifePoints{get; set;}
	public float currentGaugePoints{get; set;}
}