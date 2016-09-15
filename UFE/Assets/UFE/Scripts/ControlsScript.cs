using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlsScript : MonoBehaviour {
	[HideInInspector]	public GameObject character;
	[HideInInspector]	public GameObject opponent;
	[HideInInspector]	public CharacterInfo myInfo;
	[HideInInspector]	public CharacterInfo opInfo;
	
	[HideInInspector]	public int mirror;
	[HideInInspector]	public PossibleStates currentState;
	[HideInInspector]	public SubStates currentSubState;
	[HideInInspector]	public bool isBlocking;
	[HideInInspector]	public bool blockStunned;
	[HideInInspector]	public float stunTime;
	[HideInInspector]	public float normalizedDistance;
	[HideInInspector]	public float normalizedJumpArc;
	[HideInInspector]	public GUIText debugger;
	[HideInInspector]	public float potentialParry;
	[HideInInspector]	public bool potentialBlock;
	[HideInInspector]	public bool firstHit;
	[HideInInspector]	public int roundsWon;
	[HideInInspector]	public bool isDead;
	[HideInInspector]	public MoveInfo currentMove;
	[HideInInspector]	public MoveInfo storedMove;
	[HideInInspector]	public float shakeDensity;
	[HideInInspector]	public PullIn activePullIn;
	[HideInInspector]	public ControlsScript opControlsScript;
	[HideInInspector]	public BasicMoveReference currentBasicMove;
	[HideInInspector]	public AbstractInputController inputController;
	[HideInInspector]	public float horizontalForce;

	public List<ProjectileMoveScript> projectiles = new List<ProjectileMoveScript>();

	public int comboHits{get; protected set;}
	public float comboDamage{get; protected set;}
	public float comboHitDamage{get; protected set;}
	public MoveSetScript MoveSet{get{return this.myMoveSetScript;}}
	public PhysicsScript Physics{get{return this.myPhysicsScript;}}
    public string aiDebugger {get; set;}
    public bool introPlayed {get; protected set;}
    public CharacterDebugInfo debugInfo;
    public int playerNum;

	
	private AnimatorStateInfo currentBaseState;
	private PhysicsScript myPhysicsScript;
	private MoveSetScript myMoveSetScript;
	private HitBoxesScript myHitBoxesScript;
	
	private PhysicsScript opPhysicsScript;
	private HitBoxesScript opHitBoxesScript;
	
	private HeadLookScript headLookScript;
	
	private GameObject emulatedCam;
	private CameraScript cameraScript;

	private bool ignoreAnimationTransform = true; // Toggle to disable any applyRoot motion from moves
	private float standardYRotation;
	
	private Quaternion targetRotation;
	private bool hitDetected;
	private List<ButtonPress> totalButtonPressed;
	private string currentHitAnimation;
	private float hitStunDeceleration = 0;
	private int airJuggleHits;
	private bool shakeCharacter;
	private bool shakeCamera;
	private bool ignoreCollisionMass;
	private StandUpOptions standUpOverride;
	
	private Shader[] normalShaders;
	private Color[] normalColors;
	private bool lit;

	private bool roundMsgCasted;
	private bool outroPlayed;

    private float afkTimer;

	private float storedMoveTime;
	
	void Start () {
		if (gameObject.name == "Player1") {
			transform.position = new Vector3(UFE.config.roundOptions.p1XPosition, .009f, 0);
			opponent = GameObject.Find("Player2");
			if (UFE.config.player1Character == null) 
				Debug.LogError("Player 1 character not found! Make sure you have set the characters correctly in the Global Editor");

			myInfo = (CharacterInfo) Instantiate(UFE.config.player1Character);
			UFE.config.player1Character = myInfo;

			debugger = UFE.debugger1;
			mirror = -1;
			playerNum = 1;
			
			inputController = UFE.GetPlayer1Controller();
            debugInfo = UFE.config.debugOptions.p1DebugInfo;
		}else{
			transform.position = new Vector3(UFE.config.roundOptions.p2XPosition, .009f, 0);
			opponent = GameObject.Find("Player1");
			if (UFE.config.player2Character == null) 
				Debug.LogError("Player 2 character not found! Make sure you have set the characters correctly in the Global Editor");

			myInfo = (CharacterInfo) Instantiate(UFE.config.player2Character);
			UFE.config.player2Character = myInfo;
			
			debugger = UFE.debugger2;
			mirror = 1;
			playerNum = 2;
			
			inputController = UFE.GetPlayer2Controller();
            debugInfo = UFE.config.debugOptions.p2DebugInfo;
		}
		
		myInfo.currentLifePoints = (float)myInfo.lifePoints;

		if (myInfo.characterPrefab == null) 
			Debug.LogError("Character prefab for "+ gameObject.name +" not found. Make sure you have selected a prefab character in the Character Editor");

		character = (GameObject) Instantiate(myInfo.characterPrefab);
		character.transform.parent = transform;
		character.AddComponent<MoveSetScript>();

		standardYRotation = character.transform.rotation.eulerAngles.y;


		myPhysicsScript = GetComponent<PhysicsScript>();
		myMoveSetScript = character.GetComponent<MoveSetScript>();
		myHitBoxesScript = character.GetComponent<HitBoxesScript>();
		cameraScript = transform.parent.GetComponent<CameraScript>();

		myHitBoxesScript.controlsScript = this;

		if (myInfo.headLook.enabled){
			character.AddComponent<HeadLookScript>();
			headLookScript = character.GetComponent<HeadLookScript>();
			headLookScript.segments = myInfo.headLook.segments;
			headLookScript.nonAffectedJoints = myInfo.headLook.nonAffectedJoints;
			headLookScript.effect = myInfo.headLook.effect;
			headLookScript.overrideAnimation = !myInfo.headLook.overrideAnimation;
			
			foreach(BendingSegment segment in headLookScript.segments) {
				segment.firstTransform = myHitBoxesScript.GetTransform(segment.bodyPart).parent.transform;
				segment.lastTransform = myHitBoxesScript.GetTransform(segment.bodyPart);
			}
			
			foreach(NonAffectedJoints nonAffectedJoint in headLookScript.nonAffectedJoints) 
				nonAffectedJoint.joint = myHitBoxesScript.GetTransform(nonAffectedJoint.bodyPart);
		}


		if (gameObject.name == "Player2") {
			testCharacterRotation(100, true);
			UFE.FireGameBegins();
		}

		if (UFE.config.roundOptions.allowMovement) {
			UFE.config.lockMovements = false;
		}else{
			UFE.config.lockMovements = true;
		}
	}
	
	private bool isAxisRested(AbstractInputController inputController){
		if (currentState == PossibleStates.Down) return true;
		if (UFE.config.lockMovements) return true;
		foreach (InputReferences inputRef in inputController.inputReferences) {
			if (inputRef.inputType == InputType.Button) continue;
			if (inputController.GetAxisRaw(inputRef) != 0) return false;
		}
		return true;
	}

	public void ForceMirror(bool toggle){
		if (myInfo.animationType == AnimationType.Legacy){
			character.transform.localScale = new Vector3(-character.transform.localScale.x, character.transform.localScale.y, character.transform.localScale.z);
		}else{
			myHitBoxesScript.InvertHitBoxes(toggle);
			myMoveSetScript.SetMecanimMirror(toggle);
		}
	}
	
	public void InvertRotation(){
		standardYRotation = -standardYRotation;
	}

	private void testCharacterRotation(float rotationSpeed){
		testCharacterRotation(rotationSpeed, false);
	}

	private void testCharacterRotation(float rotationSpeed, bool forceMirror){
		if ((mirror == -1 || forceMirror) && transform.position.x > opponent.transform.position.x) {
			mirror = 1;
			InvertRotation();
			if (UFE.config.characterRotationOptions.autoMirror) ForceMirror(true);

		}else if ((mirror == 1 || forceMirror) && transform.position.x < opponent.transform.position.x) {
			mirror = -1;
			potentialBlock = false;
			InvertRotation();
			if (UFE.config.characterRotationOptions.autoMirror) ForceMirror(false);
		}
		character.transform.rotation = Quaternion.Slerp(character.transform.rotation, Quaternion.AngleAxis(standardYRotation, Vector3.up), Time.fixedDeltaTime * rotationSpeed);
	}
	
	private void fixCharacterRotation(){
		if (currentState == PossibleStates.Down) return;
		if (character.transform.rotation != Quaternion.AngleAxis(standardYRotation, Vector3.up)){
			character.transform.rotation = Quaternion.AngleAxis(standardYRotation, Vector3.up);
		}
	}

	private void validateRotation(){
		if (!myPhysicsScript.IsGrounded() || myPhysicsScript.freeze || currentMove != null) fixCharacterRotation();

		if (myPhysicsScript.freeze) return;
		if (currentState == PossibleStates.Down) return;
		if (currentMove != null && (!currentMove.autoCorrectRotation || currentMove.frameWindowRotation > currentMove.currentFrame)) return;
		if (myPhysicsScript.IsJumping() && !UFE.config.characterRotationOptions.rotateWhileJumping) return;
		if (currentSubState == SubStates.Stunned && !UFE.config.characterRotationOptions.fixRotationWhenStunned) return;
		if (isBlocking && !UFE.config.characterRotationOptions.fixRotationWhenBlocking) return;
		if (UFE.config.characterRotationOptions.rotateOnMoveOnly && myMoveSetScript.IsAnimationPlaying("idle")) return;

		testCharacterRotation(UFE.config.characterRotationOptions.rotationSpeed);
	}
	
	public void startFight(){
		UFE.FireAlert(UFE.config.selectedLanguage.fight, null);
		UFE.config.lockInputs = false;
		UFE.config.lockMovements = false;
		UFE.PlayTimer();
	}

	public void DoFixedUpdate(){
		// If both controllers aren't ready, ignore the player input
		if (!UFE.GetPlayer1Controller().isReady || !UFE.GetPlayer2Controller().isReady) return;


		// Training Mode
        if ((playerNum == 1 && UFE.gameMode == GameMode.TrainingRoom && UFE.config.trainingModeOptions.p1Life == LifeBarTrainingMode.Refill) ||
            (playerNum == 2 && UFE.gameMode == GameMode.TrainingRoom && UFE.config.trainingModeOptions.p2Life == LifeBarTrainingMode.Refill)) {
			if (!UFE.FindDelaySynchronizedAction(this.RefillLife))
                UFE.DelaySynchronizedAction(this.RefillLife, UFE.config.trainingModeOptions.refillTime);
		}

        if ((playerNum == 1 && UFE.gameMode == GameMode.TrainingRoom && UFE.config.trainingModeOptions.p1Gauge == LifeBarTrainingMode.Refill) ||
            (playerNum == 2 && UFE.gameMode == GameMode.TrainingRoom && UFE.config.trainingModeOptions.p2Gauge == LifeBarTrainingMode.Refill)) {
			if (!UFE.FindDelaySynchronizedAction(this.RefillGauge))
                UFE.DelaySynchronizedAction(this.RefillGauge, UFE.config.trainingModeOptions.refillTime);
		}

        if (UFE.gameMode == GameMode.TrainingRoom && myInfo.currentGaugePoints < myInfo.maxGaugePoints &&
            ((playerNum == 1 && UFE.config.trainingModeOptions.p1Gauge == LifeBarTrainingMode.Infinite) ||
            (playerNum == 2 && UFE.config.trainingModeOptions.p2Gauge == LifeBarTrainingMode.Infinite))) RefillGauge();

        if (UFE.gameMode == GameMode.TrainingRoom && myInfo.currentLifePoints < myInfo.lifePoints &&
            ((playerNum == 1 && UFE.config.trainingModeOptions.p1Life == LifeBarTrainingMode.Infinite) ||
            (playerNum == 2 && UFE.config.trainingModeOptions.p2Life == LifeBarTrainingMode.Infinite))) RefillLife();


		// Debugger
		if (debugger != null && UFE.config.debugOptions.debugMode){
			debugger.text = "";
            if (UFE.config.debugOptions.debugMode) {
				debugger.text += "-----Character Info-----\n";
                if (debugInfo.lifePoints) debugger.text += "Life Points: " + myInfo.currentLifePoints + "\n";
                if (debugInfo.position) debugger.text += "Position: " + transform.position + "\n";
                if (debugInfo.currentState) debugger.text += "State: " + currentState + "\n";
                if (debugInfo.currentSubState) debugger.text += "Sub State: " + currentSubState + "\n";
                if (debugInfo.stunTime && stunTime > 0) debugger.text += "Stun Time: " + stunTime + "\n";
                if (opControlsScript != null && opControlsScript.comboHits > 0) {
                    debugger.text += "Current Combo\n";
					if (debugInfo.comboHits) debugger.text += "- Total Hits: "+ opControlsScript.comboHits + "\n";
                    if (debugInfo.comboDamage) {
                        debugger.text += "- Total Damage: " + opControlsScript.comboDamage + "\n";
                        debugger.text += "- Hit Damage: " + opControlsScript.comboHitDamage + "\n";
                    }
				}

				// Other uses
				//if (potentialParry > 0) debugger.text += "Parry Window: "+ potentialParry + "\n";
				//debugger.text += "Air Jumps: "+ myPhysicsScript.currentAirJumps + "\n";

                if (UFE.config.debugOptions.p1DebugInfo.currentMove && currentMove != null) {
					debugger.text += "Move: "+ currentMove.name + " ("+ currentMove.currentFrame +"/"+ currentMove.totalFrames +") \n";
					/*if (currentMove.chargeMove) {
						debugger.text += "First Input Charge: "+ myMoveSetScript.chargeValues[currentMove.buttonSequence[0]] + "\n";
					}*/
					//debugger.text += "StartupFrames: "+ currentMove.moveClassification.startupSpeed +" \n";
				}
			}
            if (aiDebugger != null && debugInfo.aiWeightList) debugger.text += aiDebugger;
		}



		// Once per game
		if (opHitBoxesScript == null) {
			opControlsScript = opponent.GetComponent<ControlsScript>();
			opPhysicsScript = opponent.GetComponent<PhysicsScript>();
			opHitBoxesScript = opponent.GetComponentInChildren<HitBoxesScript>();
			opInfo = opControlsScript.myInfo;
			
			if (myInfo.enableAlternativeColor){
				if (gameObject.name == "Player2" && character.name == opControlsScript.character.name){  // Alternative Costume
					Renderer[] charRenders = character.GetComponentsInChildren<Renderer>();
					foreach(Renderer charRender in charRenders){
						charRender.material.color = myInfo.alternativeColor;
						//charRender.material.shader = Shader.Find("VertexLit");
						//charRender.material.SetColor("_Emission", myInfo.alternativeColor);
					}
				}
			}
			
			Renderer[] charRenderers = character.GetComponentsInChildren<Renderer>();
			List<Shader> shaderList = new List<Shader>();
			List<Color> colorList = new List<Color>();
			foreach(Renderer char_rend in charRenderers){
				//if (char_rend.material.HasProperty("color") && char_rend.material.HasProperty("shader")){ 
					shaderList.Add(char_rend.material.shader);
					colorList.Add(char_rend.material.color);
				//}
			}
			normalShaders = shaderList.ToArray();
			normalColors = colorList.ToArray();

			myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
		}


		// Once per round
		if ((gameObject.name == "Player1" && !introPlayed && currentMove == null) ||
		    (gameObject.name == "Player2" && !introPlayed && opControlsScript.introPlayed && currentMove == null))
        {
            KillCurrentMove();
            CastMove(myMoveSetScript.intro, true, true, false);
			if (currentMove == null) {
				introPlayed = true;
				UFE.CastNewRound();
			}
		}


		// Resolve move
		resolveMove();


		// Check inputs
		translateInputs(inputController);


		// Validate rotation
		validateRotation();

		
		// Input Viewer
		List<InputReferences> inputList = new List<InputReferences>();
		foreach (InputReferences inputRef in inputController.inputReferences){
			if (debugger != null && UFE.config.debugOptions.debugMode && debugInfo.inputs){
				debugger.text += inputRef.inputButtonName + " - "+ inputRef.heldDown + "\n";
			}
			if (inputRef.heldDown > 0 && inputRef.heldDown <= (2f/(float)UFE.config.fps)){
				inputList.Add(inputRef);
			}
		}
		UFE.CastInput(inputList.ToArray(), playerNum);


		// Force character local position
		if (ignoreAnimationTransform && (currentMove == null || !currentMove.applyRootMotion))
			character.transform.localPosition = new Vector3(0, 0, 0);


		// Force stand state
		if (!myPhysicsScript.freeze
			&& !isDead
            && currentSubState != SubStates.Stunned
			&& introPlayed
			&& myPhysicsScript.IsGrounded()
			&& !myPhysicsScript.IsMoving()
		    && currentMove == null
            && !myMoveSetScript.IsBasicMovePlaying(myMoveSetScript.basicMoves.idle)
			&& !myMoveSetScript.IsAnimationPlaying("fallStraight")
			&& isAxisRested(inputController)
			&& !myPhysicsScript.isTakingOff
			&& !myPhysicsScript.isLanding
			&& !blockStunned
			&& currentState != PossibleStates.Crouch
			&& !isBlocking
		    ){

                myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
			    currentState = PossibleStates.Stand;
			    currentSubState = SubStates.Resting;
			    if (UFE.config.blockOptions.blockType == BlockType.AutoBlock 
                    && myMoveSetScript.basicMoves.blockEnabled) potentialBlock = true;
		}

        if (myMoveSetScript.IsAnimationPlaying("idle")
            && !UFE.config.lockInputs 
		    && !UFE.config.lockMovements) {
            afkTimer += Time.fixedDeltaTime;
            if (afkTimer >= myMoveSetScript.basicMoves.idle.restingClipInterval) {
                afkTimer = 0;
                int clipNum = Mathf.RoundToInt(Random.Range(2, 6));
                if (myMoveSetScript.AnimationExists("idle_" + clipNum)) {
                    myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle, "idle_" + clipNum, false);
                }
            }
        } else {
            afkTimer = 0;
        }


		// Character colliders based on collision mass and body colliders
		normalizedDistance = Mathf.Clamp01(Vector3.Distance(opponent.transform.position, transform.position) / UFE.config.cameraOptions.maxDistance);
		if (!ignoreCollisionMass && !opControlsScript.ignoreCollisionMass) {
			float pushForce = myHitBoxesScript.TestCollision(opHitBoxesScript.hitBoxes);
			if (pushForce > 0) {
				if (transform.position.x < opponent.transform.position.x) {
					transform.Translate(new Vector3(-.1f * pushForce, 0, 0));
				}else{
					transform.Translate(new Vector3(.1f * pushForce, 0, 0));
				}
				if (opponent.transform.position.x == UFE.config.selectedStage.rightBoundary){
					opponent.transform.Translate(new Vector3(-.2f * pushForce, 0, 0));
				}
			}

			pushForce = myInfo.physics.groundCollisionMass - Vector3.Distance(transform.position, opponent.transform.position);
			if (pushForce > 0) {
				if (transform.position.x < opponent.transform.position.x) {
					transform.Translate(new Vector3(-.5f * pushForce, 0, 0));
				}else{
					transform.Translate(new Vector3(.5f * pushForce, 0, 0));
				}
				if (opponent.transform.position.x == UFE.config.selectedStage.rightBoundary){
					opponent.transform.Translate(new Vector3(-.2f * pushForce, 0, 0));
				}
			}
		}



		// Shake character
		if (shakeDensity > 0) {
			shakeDensity -= Time.fixedDeltaTime;
			if (myHitBoxesScript.isHit && myPhysicsScript.freeze){
				if (shakeCharacter) shake();
				if (shakeCamera) shakeCam();
			}else{
				if (UFE.config.bounceOptions.shakeCamOnBounce && myPhysicsScript.isBouncing) shakeCam();
			}
		}else if (shakeDensity < 0) {
			shakeDensity = 0;
			shakeCamera = false;
			shakeCharacter = false;
		}


		// Validate Parry
		if (potentialParry > 0){
			potentialParry -= Time.fixedDeltaTime;
			if (potentialParry <= 0) potentialParry = 0;
		}


		// Update head movement
		if (headLookScript != null && opHitBoxesScript != null) 
			headLookScript.target = opHitBoxesScript.GetPosition(myInfo.headLook.target);


		// Execute Move
		if (currentMove != null) ReadMove(currentMove);


		// Apply Stun
		if ((currentSubState == SubStates.Stunned || blockStunned) && stunTime > 0 && !myPhysicsScript.freeze && !isDead)
			ApplyStun();


		// Apply Forces
		myPhysicsScript.ApplyForces(currentMove);
	}

	private bool testMoveExecution(ButtonPress buttonPress, bool inputUp){
		return testMoveExecution(new ButtonPress[]{buttonPress}, inputUp);
	}
	
	private bool testMoveExecution(ButtonPress[] buttonPresses, bool inputUp){
		storedMove = myMoveSetScript.GetMove(buttonPresses, 0, currentMove, false);
		if (storedMove != null) {
			storedMoveTime = ((float)UFE.config.executionBufferTime / UFE.config.fps);
			return true;
		}
		return false;
	}
	
	private void resolveMove(){
		if (myPhysicsScript.freeze) return;
		if (storedMoveTime > 0) storedMoveTime -= Time.fixedDeltaTime;
		if (storedMoveTime <= 0 && storedMove != null){
			storedMoveTime = 0;
			if (UFE.config.executionBufferType != ExecutionBufferType.NoBuffer) storedMove = null;
		}
		
		if (currentMove != null && storedMove == null && !opControlsScript.isDead)
			storedMove = myMoveSetScript.GetNextMove(currentMove);

        //if (storedMove != null) Debug.Log("->" + storedMove.name);
        //if (currentMove) Debug.Log(currentMove.name + "(" + currentMove.currentFrame +" - "+ currentMove.cancelable + ")");

        if ((currentMove == null || currentMove.cancelable) && storedMove != null) {
            //if (currentMove) Debug.Log(currentMove.name + "(" + currentMove.currentFrame + ")");
            //Debug.Log("->" + storedMove.name);
			bool confirmQueue = false;
			bool ignoreConditions = false;
            if (currentMove != null && UFE.config.executionBufferType == ExecutionBufferType.OnlyMoveLinks) {
				foreach(FrameLink frameLink in currentMove.frameLinks){
					if (frameLink.cancelable){
						foreach(MoveInfo move in frameLink.linkableMoves) {
							if (storedMove.name == move.name) {
								storedMove.overrideStartupFrame = frameLink.nextMoveStartupFrame - 1;
								confirmQueue = true;
							}
						}
					}
					if (frameLink.ignorePlayerConditions){
						ignoreConditions = true;
					}
				}
			}else if (currentMove == null || UFE.config.executionBufferType == ExecutionBufferType.AnyMove){
				confirmQueue = true;
			}
			
			if (confirmQueue && (ignoreConditions || myMoveSetScript.ValidateMoveStances(storedMove.selfConditions, this))) {
				KillCurrentMove();
				currentMove = storedMove;
			}
			storedMove = null;
			storedMoveTime = 0;
		}
	}
	
	private void translateInputs(AbstractInputController inputController){
		if (!introPlayed || !opControlsScript.introPlayed) return;
		if (UFE.config.lockInputs && !UFE.config.roundOptions.allowMovement) return;
		if (UFE.config.lockMovements) return;
		
		foreach (InputReferences inputRef in inputController.inputReferences) {
			if (((inputRef.engineRelatedButton == ButtonPress.Down && inputController.GetAxisRaw(inputRef) >= 0)
			     || (inputRef.engineRelatedButton == ButtonPress.Up && inputController.GetAxisRaw(inputRef) <= 0))
			    && myPhysicsScript.IsGrounded() 
			    && !myHitBoxesScript.isHit 
			    && currentSubState != SubStates.Stunned){
				currentState = PossibleStates.Stand;
			}
			
			if (inputRef.inputType != InputType.Button && inputRef.heldDown > 0 && inputController.GetAxisRaw(inputRef) == 0) {
				if ((inputRef.engineRelatedButton == ButtonPress.Back && UFE.config.blockOptions.blockType == BlockType.HoldBack)){
					potentialBlock = false;
				}

				storedMove = myMoveSetScript.GetMove(new ButtonPress[]{inputRef.engineRelatedButton}, inputRef.heldDown, currentMove, true);
				inputRef.heldDown = 0;
				if (storedMove != null){
					storedMoveTime = ((float)UFE.config.executionBufferTime / UFE.config.fps);
					return;
				}
			}

			if (inputRef.heldDown == 0 && inputRef.inputType != InputType.Button) {
				inputRef.activeIcon = inputController.GetAxisRaw(inputRef) > 0? inputRef.inputViewerIcon1 : inputRef.inputViewerIcon2;
			}

			/*if (inputController.GetButtonUp(inputRef)) {
				storedMove = myMoveSetScript.GetMove(new ButtonPress[]{inputRef.engineRelatedButton}, inputRef.heldDown, currentMove, true);
				inputRef.heldDown = 0;
				if (storedMove != null){
					storedMoveTime = ((float)UFE.config.executionBufferTime / UFE.config.fps);
					return;
				}
			}*/
			
			// Axis Press
			if (inputRef.inputType != InputType.Button && inputController.GetAxisRaw(inputRef) != 0) {
				if (inputRef.inputType == InputType.HorizontalAxis) {
					// Horizontal Movements
					if (inputController.GetAxisRaw(inputRef) > 0) {
						inputRef.engineRelatedButton = mirror == 1? ButtonPress.Back : ButtonPress.Foward;
						inputRef.heldDown += Time.fixedDeltaTime;
						if (inputRef.heldDown == Time.fixedDeltaTime && testMoveExecution(inputRef.engineRelatedButton, false)) return;
						
						if (currentState == PossibleStates.Stand 
						    && !isBlocking 
						    && !myPhysicsScript.isTakingOff
						    && !myPhysicsScript.isLanding
						    && currentSubState != SubStates.Stunned
                            && !blockStunned
                            && currentMove == null
                            && myMoveSetScript.basicMoves.moveEnabled) {
							myPhysicsScript.Move(-mirror, inputController.GetAxisRaw(inputRef));
						}
					}
					
					if (inputController.GetAxisRaw(inputRef) < 0) {
						inputRef.engineRelatedButton = mirror == 1? ButtonPress.Foward : ButtonPress.Back;
						inputRef.heldDown += Time.fixedDeltaTime;
						if (inputRef.heldDown == Time.fixedDeltaTime && testMoveExecution(inputRef.engineRelatedButton, false)) return;
						
						if (currentState == PossibleStates.Stand 
						    && !isBlocking 
						    && !myPhysicsScript.isTakingOff
						    && !myPhysicsScript.isLanding
						    && currentSubState != SubStates.Stunned
                            && !blockStunned
                            && currentMove == null
                            && myMoveSetScript.basicMoves.moveEnabled) {
							myPhysicsScript.Move(mirror, inputController.GetAxisRaw(inputRef));
						}
					}

					// Check for potential blocking
					if (inputRef.engineRelatedButton == ButtonPress.Back 
					    && UFE.config.blockOptions.blockType == BlockType.HoldBack
					    && !myPhysicsScript.isTakingOff
                        && myMoveSetScript.basicMoves.blockEnabled) {
						potentialBlock = true;
					}
					
					// Check for potential parry
					if (((inputRef.engineRelatedButton == ButtonPress.Back && UFE.config.blockOptions.parryType == ParryType.TapBack) ||
					     (inputRef.engineRelatedButton == ButtonPress.Foward && UFE.config.blockOptions.parryType == ParryType.TapForward))
					    && potentialParry == 0
					    && inputRef.heldDown == Time.fixedDeltaTime
					    && currentMove == null
					    && !isBlocking 
					    && !myPhysicsScript.isTakingOff
					    && currentSubState != SubStates.Stunned
                        && !blockStunned
                        && myMoveSetScript.basicMoves.parryEnabled) {
						potentialParry = UFE.config.blockOptions.parryTiming;
					}

					
				}else{
					// Vertical Movements
					if (inputController.GetAxisRaw(inputRef) > 0) {
						inputRef.engineRelatedButton = ButtonPress.Up;
						if (!myPhysicsScript.isTakingOff && !myPhysicsScript.isLanding){
							if (inputRef.heldDown == 0) {
								if (!myPhysicsScript.IsGrounded() && myInfo.physics.canJump && myInfo.physics.multiJumps > 1){
									myPhysicsScript.Jump();
								}
								if (testMoveExecution(inputRef.engineRelatedButton, false)) return;
							}
							if (!myPhysicsScript.freeze 
							    && !myPhysicsScript.IsJumping()
							    && storedMove == null 
							    && currentMove == null 
							    && currentState == PossibleStates.Stand
							    && currentSubState != SubStates.Stunned
							    && !isBlocking
							    && myInfo.physics.canJump
                                && !blockStunned
                                && myMoveSetScript.basicMoves.jumpEnabled) {
								
								float delayTime = (float)myInfo.physics.jumpDelay/UFE.config.fps;
								myPhysicsScript.isTakingOff = true;
								potentialBlock = false;
								potentialParry = 0;

								UFE.DelaySynchronizedAction(myPhysicsScript.Jump, delayTime);
								
								if (myMoveSetScript.AnimationExists(myMoveSetScript.basicMoves.takeOff.name)){
									myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.takeOff);

                                    if (myMoveSetScript.basicMoves.takeOff.autoSpeed) {
                                        myMoveSetScript.SetAnimationSpeed(
                                            myMoveSetScript.basicMoves.takeOff.name,
                                            myMoveSetScript.GetAnimationLengh(myMoveSetScript.basicMoves.takeOff.name) / delayTime);
                                    }
									
								}
							}
						}
						inputRef.heldDown += Time.fixedDeltaTime;
						
					}else if (inputController.GetAxisRaw(inputRef) < 0) {
						inputRef.engineRelatedButton = ButtonPress.Down;
						if (inputRef.heldDown == 0 && testMoveExecution(inputRef.engineRelatedButton, false)) return;
						inputRef.heldDown += Time.fixedDeltaTime;
						
						if (!myPhysicsScript.freeze 
						    && myPhysicsScript.IsGrounded() 
						    && currentMove == null 
						    && currentSubState != SubStates.Stunned 
						    && !myPhysicsScript.isTakingOff
                            && !blockStunned
                            && myMoveSetScript.basicMoves.crouchEnabled) {
							currentState = PossibleStates.Crouch;
							if (!myMoveSetScript.IsAnimationPlaying(myMoveSetScript.basicMoves.crouching.name)) {
								if (!isBlocking && !myMoveSetScript.IsAnimationPlaying(myMoveSetScript.basicMoves.crouching.name))
									myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.crouching);
								
								if (isBlocking && !myMoveSetScript.IsAnimationPlaying(myMoveSetScript.basicMoves.blockingCrouchingPose.name))
									myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingCrouchingPose);
							}
						}
					}
				}
				
				foreach (InputReferences inputRef2 in inputController.inputReferences) {
					if (inputRef2.inputType == InputType.Button && inputController.GetButtonDown(inputRef2)) {
						storedMove = myMoveSetScript.GetMove(
							new ButtonPress[]{inputRef.engineRelatedButton, inputRef2.engineRelatedButton}, 0, currentMove, false, true);
						
						if (storedMove != null){
							storedMoveTime = ((float)UFE.config.executionBufferTime / UFE.config.fps);
							return;
						}
					}
				}
			}
			
			// Button Press
			if (inputRef.inputType == InputType.Button && !UFE.config.lockInputs){
				if (inputController.GetButton(inputRef)) {
					if (myMoveSetScript.CompareBlockButtons(inputRef.engineRelatedButton) 
					    && currentSubState != SubStates.Stunned 
					    && !myPhysicsScript.isTakingOff
					    && !blockStunned
                        && myMoveSetScript.basicMoves.blockEnabled) {
						potentialBlock = true;
						CheckBlocking(true);
					}

					if (myMoveSetScript.CompareParryButtons(inputRef.engineRelatedButton) 
					    && inputRef.heldDown == 0 
					    && potentialParry == 0 
					    && currentMove == null 
					    && !isBlocking 
					    && currentSubState != SubStates.Stunned 
					    && !myPhysicsScript.isTakingOff
					    && !blockStunned
                        && myMoveSetScript.basicMoves.parryEnabled) {
						potentialParry = UFE.config.blockOptions.parryTiming;
					}
					
					inputRef.heldDown += Time.fixedDeltaTime;
					if (inputRef.heldDown <= ((float)UFE.config.plinkingDelay/(float)UFE.config.fps)) {
						foreach (InputReferences inputRef2 in inputController.inputReferences) {
							if (inputRef2 != inputRef 
							    && inputRef2.inputType == InputType.Button 
							    && inputController.GetButtonDown(inputRef2)) {
								inputRef2.heldDown += Time.fixedDeltaTime;
								storedMove = myMoveSetScript.GetMove(
									new ButtonPress[]{inputRef.engineRelatedButton, inputRef2.engineRelatedButton}, 0, currentMove, false, true);
								
								if (storedMove != null && (currentMove == null || currentMove.currentFrame <= UFE.config.plinkingDelay)){
									KillCurrentMove();
									storedMoveTime = ((float)UFE.config.executionBufferTime / UFE.config.fps);
									return;
								}
							}
						}
					}
				}
				
				
				if (inputController.GetButtonDown(inputRef)) {
					storedMove = myMoveSetScript.GetMove(new ButtonPress[]{inputRef.engineRelatedButton}, 0, currentMove, false);
					if (storedMove != null){
						storedMoveTime = ((float)UFE.config.executionBufferTime / UFE.config.fps);
						return;
					}
				}
				
				if (inputController.GetButtonUp(inputRef)) {
					storedMove = myMoveSetScript.GetMove(new ButtonPress[]{inputRef.engineRelatedButton}, inputRef.heldDown, currentMove, true);
					inputRef.heldDown = 0;
                    if (storedMove != null) {
						storedMoveTime = ((float)UFE.config.executionBufferTime / UFE.config.fps);
						return;
					}
					if (myMoveSetScript.CompareBlockButtons(inputRef.engineRelatedButton) 
					    && !myPhysicsScript.isTakingOff) {
						potentialBlock = false;
						CheckBlocking(false);
					}
				}
			}
		}
	}
	
	public void ApplyStun(){
		// Hit Stun deceleration and knock down algorithms
		if (hitStunDeceleration > 0 && currentMove == null){
			hitStunDeceleration -= Time.fixedDeltaTime;
            myMoveSetScript.SetAnimationNormalizedSpeed(currentHitAnimation, hitStunDeceleration);
		}
		
		if (UFE.config.comboOptions.neverAirRecover && !myPhysicsScript.IsGrounded() && currentState != PossibleStates.Down){
			stunTime = 1;
		}else{
			stunTime -= Time.fixedDeltaTime;
		}

		string standUpAnimation = null;
		float standUpTime = UFE.config.knockDownOptions.air.standUpTime;

		if (!isDead && currentMove == null && myPhysicsScript.IsGrounded()){
			if (currentState == PossibleStates.Down){
				if (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitHighKnockdown, 1)
				    || standUpOverride == StandUpOptions.HighKnockdownClip){
					if (stunTime <= UFE.config.knockDownOptions.high.standUpTime){
						standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUp, 2);
						standUpTime = UFE.config.knockDownOptions.high.standUpTime;
						if (!UFE.config.knockDownOptions.high.standUpHitBoxes) myHitBoxesScript.HideHitBoxes(true);
					}
				}else if (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitHighLowKnockdown, 1)
				          || standUpOverride == StandUpOptions.LowKnockdownClip){
					if (stunTime <= UFE.config.knockDownOptions.highLow.standUpTime){
						standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUp, 3);
						standUpTime = UFE.config.knockDownOptions.highLow.standUpTime;
						if (!UFE.config.knockDownOptions.highLow.standUpHitBoxes) myHitBoxesScript.HideHitBoxes(true);
					}
				}else if (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitSweep, 1)
				          || standUpOverride == StandUpOptions.SweepClip){
					if (stunTime <= UFE.config.knockDownOptions.sweep.standUpTime){
						standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUp, 4);
						standUpTime = UFE.config.knockDownOptions.sweep.standUpTime;
						if (!UFE.config.knockDownOptions.sweep.standUpHitBoxes) myHitBoxesScript.HideHitBoxes(true);
					}
				}else{
					if (myMoveSetScript.basicMoves.standUp.clip1 == null)
						Debug.LogError("Stand Up animation not found! Make sure you have it set on Character -> Basic Moves -> Stand Up");
					
					if (stunTime <= UFE.config.knockDownOptions.air.standUpTime){
						standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUp, 1);
						standUpTime = UFE.config.knockDownOptions.air.standUpTime;
						if (!UFE.config.knockDownOptions.air.standUpHitBoxes) myHitBoxesScript.HideHitBoxes(true);
					}
				}
			}else if (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitCrumple, 1)){
				if (stunTime <= UFE.config.knockDownOptions.crumple.standUpTime){
					standUpAnimation = myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standUp, 5);
					standUpTime = UFE.config.knockDownOptions.crumple.standUpTime;
					if (!UFE.config.knockDownOptions.crumple.standUpHitBoxes) myHitBoxesScript.HideHitBoxes(true);
				}
			}
		}
		
		if (standUpAnimation != null && !myMoveSetScript.IsAnimationPlaying(standUpAnimation)){
			myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.standUp, standUpAnimation);
            if (myMoveSetScript.basicMoves.standUp.autoSpeed) {
                myMoveSetScript.SetAnimationSpeed(standUpAnimation, myMoveSetScript.GetAnimationLengh(standUpAnimation) / standUpTime);
            }
		}
		
		if (stunTime <= 0) {
			//if (currentState == PossibleStates.Stand) myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);
			ReleaseStun();
		}
	}

	public void CastMove(MoveInfo move){
		CastMove(move, false, false, false);
	}

    public void CastMove(MoveInfo move, bool overrideCurrentMove) {
        CastMove(move, overrideCurrentMove, false, false);
    }

    public void CastMove(MoveInfo move, bool overrideCurrentMove, bool forceGrounded) {
        CastMove(move, overrideCurrentMove, forceGrounded, false);
    }

    public void CastMove(MoveInfo move, bool overrideCurrentMove, bool forceGrounded, bool castWarning) {
		if (move == null) return;
		if (castWarning && !myMoveSetScript.HasMove(move.moveName)) 
            Debug.LogError("Move '"+ move.name +"' could not be found under this character's move set.");

		if (overrideCurrentMove) {
			KillCurrentMove();
			MoveInfo newMove = Instantiate(move) as MoveInfo;
			newMove.name = move.name;
			currentMove = newMove;
			currentMove.currentFrame = 0;
			currentMove.currentTick = 0;
		}else{
			storedMove = Instantiate(move) as MoveInfo;
		}
        if (forceGrounded) myPhysicsScript.ForceGrounded();
	}
	
	public void ReadMove(MoveInfo move){
		if (move == null) return;

		potentialParry = 0;
		potentialBlock = false;

		if (move.currentTick == 0) {
			if (!myMoveSetScript.AnimationExists(move.name)) 
				Debug.LogError("Animation for move '"+ move.name +"' not found!");
			
			
			if (move.disableHeadLook) ToggleHeadLook(false);

            if (myPhysicsScript.IsGrounded()) {
                myPhysicsScript.isTakingOff = false;
                myPhysicsScript.isLanding = false;
            }
			
			if (currentState == PossibleStates.StraightJump ||
			    currentState == PossibleStates.ForwardJump ||
			    currentState == PossibleStates.BackJump){
				myMoveSetScript.totalAirMoves ++;
			}

			float normalizedTimeConv = myMoveSetScript.GetAnimationNormalizedTime(move.overrideStartupFrame, move);
			
			if (move.overrideBlendingIn) {
				myMoveSetScript.PlayAnimation(move.name, move.blendingIn, normalizedTimeConv);
			}else{
				myMoveSetScript.PlayAnimation(move.name, myInfo.blendingTime, normalizedTimeConv);
			}
			
			if (currentMove.invertRotationLeft && mirror == -1) InvertRotation();
			if (currentMove.forceMirrorLeft && mirror == -1) ForceMirror(true);
			
			if (currentMove.invertRotationRight && mirror == 1) InvertRotation();
			if (currentMove.forceMirrorRight && mirror == 1) ForceMirror(!UFE.config.characterRotationOptions.autoMirror);


			move.currentTick = move.overrideStartupFrame;
			move.currentFrame = move.overrideStartupFrame;
			move.animationSpeedTemp = move.animationSpeed;
			
			myMoveSetScript.SetAnimationSpeed(move.name, move.animationSpeed);
			if (move.overrideBlendingOut) myMoveSetScript.overrideNextBlendingValue = move.blendingOut;
			
			AddGauge(move.gaugeGainOnMiss);
			RemoveGauge(move.gaugeUsage);
		}
		
		// ANIMATION FRAME DATA
		if (myMoveSetScript.animationPaused) {
			move.currentTick += Time.fixedDeltaTime * UFE.config.fps * myMoveSetScript.GetAnimationSpeed();
		}else{
			move.currentTick += Time.fixedDeltaTime * UFE.config.fps;
		}
		if (move.currentTick > move.currentFrame) move.currentFrame ++;
		
		// deprecated
        /*if (myInfo.animationFlow == AnimationFlow.MorePrecision && !myMoveSetScript.animationPaused){
			
			myMoveSetScript.SetAnimationSpeed(move.name, 0);
			if (myInfo.animationType == AnimationType.Legacy){
				character.GetComponent<Animation>()[move.name].normalizedTime = myMoveSetScript.GetAnimationNormalizedTime(move.currentFrame, move);
			}else{
				myMoveSetScript.SetAnimationPosition(move.name, myMoveSetScript.GetAnimationNormalizedTime(move.currentFrame, move));
			}
		}*/
		
		// Assign Current Frame Data Description
		if (move.currentFrame <= move.startUpFrames) {
			move.currentFrameData = CurrentFrameData.StartupFrames;
		}else if (move.currentFrame > move.startUpFrames && move.currentFrame <= move.startUpFrames + move.activeFrames) {
			move.currentFrameData = CurrentFrameData.ActiveFrames;
		}else{
			move.currentFrameData = CurrentFrameData.RecoveryFrames;
		}
		
		
		// Check Projectiles
		foreach (Projectile projectile in move.projectiles){
			if (
				!projectile.casted && 
				projectile.projectilePrefab != null &&
				move.currentFrame >= projectile.castingFrame
				){
				projectile.casted = true;
				projectile.gaugeGainOnHit = move.gaugeGainOnHit;
				projectile.gaugeGainOnBlock = move.gaugeGainOnBlock;
				projectile.opGaugeGainOnHit = move.opGaugeGainOnHit;
				projectile.opGaugeGainOnBlock = move.opGaugeGainOnBlock;
				projectile.opGaugeGainOnParry = move.opGaugeGainOnParry;
				
				Vector3 newPos = myHitBoxesScript.GetPosition(projectile.bodyPart);
				if (projectile.fixedZAxis) newPos.z = 0;
				GameObject pTemp = (GameObject) Instantiate(
					projectile.projectilePrefab, 
					newPos, 
					Quaternion.Euler(0,0,projectile.directionAngle)
					);
				
				ProjectileMoveScript projectileMoveScript = pTemp.AddComponent<ProjectileMoveScript>();
				projectileMoveScript.data = projectile;
				projectileMoveScript.opHitBoxesScript = opHitBoxesScript;
				projectileMoveScript.opControlsScript = opControlsScript;
				projectileMoveScript.myControlsScript = this;
				projectileMoveScript.mirror = mirror;
				
				projectiles.Add(projectileMoveScript);
			}
		}
		
		// Check Particle Effects
		foreach (MoveParticleEffect particleEffect in move.particleEffects){
			if (
				!particleEffect.casted && 
				particleEffect.particleEffect.prefab != null &&
				move.currentFrame >=  particleEffect.castingFrame
				){
				particleEffect.casted = true;
				GameObject pTemp = (GameObject) Instantiate(particleEffect.particleEffect.prefab);
				
				Vector3 newPosition = myHitBoxesScript.GetPosition(particleEffect.particleEffect.bodyPart);
				newPosition.x += particleEffect.particleEffect.offSet.x * -mirror;
				newPosition.y += particleEffect.particleEffect.offSet.y;
				newPosition.z += particleEffect.particleEffect.offSet.z;
				pTemp.transform.position = newPosition;
				if (particleEffect.particleEffect.stick) pTemp.transform.parent = transform;
				
				Destroy(pTemp, particleEffect.particleEffect.duration);
			}
		}
		
		// Check Applied Forces
		foreach (AppliedForce addedForce in move.appliedForces){
			if (!addedForce.casted && move.currentFrame >= addedForce.castingFrame){
				myPhysicsScript.ResetForces(addedForce.resetPreviousHorizontal, addedForce.resetPreviousVertical);
				myPhysicsScript.AddForce(addedForce.force, -mirror);
				addedForce.casted = true;
			}
		}
		
		// Check SlowMo Effects
		foreach (SlowMoEffect slowMoEffect in move.slowMoEffects){
			if (!slowMoEffect.casted && move.currentFrame >= slowMoEffect.castingFrame){
				Time.timeScale = (slowMoEffect.percentage / 100f) * UFE.config.gameSpeed;
				UFE.DelaySynchronizedAction(this.ResetTimeScale, slowMoEffect.duration);
				slowMoEffect.casted = true;
			}
		}
		
		// Check Sound Effects
		foreach (SoundEffect soundEffect in move.soundEffects){
			if (!soundEffect.casted && move.currentFrame >= soundEffect.castingFrame){
				UFE.PlaySound(soundEffect.sounds);
				soundEffect.casted = true;
			}
		}
		
		// Check In Game Alert
		foreach (InGameAlert inGameAlert in move.inGameAlert){
			if (!inGameAlert.casted && move.currentFrame >= inGameAlert.castingFrame){
				UFE.FireAlert(inGameAlert.alert, myInfo);
				inGameAlert.casted = true;
			}
		}
		
		// Change Stances
		foreach (StanceChange stanceChange in move.stanceChanges){
			if (!stanceChange.casted && move.currentFrame >= stanceChange.castingFrame){
				myMoveSetScript.ChangeMoveStances(stanceChange.newStance);
				stanceChange.casted = true;
			}
        }

#if !UFE_BASIC
        // Check Opponent Override
		foreach (OpponentOverride opponentOverride in move.opponentOverride){
			if (!opponentOverride.casted && move.currentFrame >= opponentOverride.castingFrame){
				if (opponentOverride.stun){
					opControlsScript.stunTime = (float)opponentOverride.stunTime/(float)UFE.config.fps;
					if (opponentOverride.stunTime > 0) opControlsScript.currentSubState = SubStates.Stunned;
				}
				
				opControlsScript.KillCurrentMove();
				foreach(CharacterSpecificMoves csMove in opponentOverride.characterSpecificMoves){
					if (opInfo.characterName == csMove.characterName) {
						opControlsScript.CastMove(csMove.move, true);
						if (opponentOverride.stun) opControlsScript.currentMove.standUpOptions = opponentOverride.standUpOptions;
						opControlsScript.currentMove.hitAnimationOverride = opponentOverride.overrideHitAnimations;
					}
				}
				if (opControlsScript.currentMove == null && opponentOverride.move != null){
					opControlsScript.CastMove(opponentOverride.move, true);
					if (opponentOverride.stun) opControlsScript.currentMove.standUpOptions = opponentOverride.standUpOptions;
					opControlsScript.currentMove.hitAnimationOverride = opponentOverride.overrideHitAnimations;
				}
				
				opControlsScript.activePullIn = new PullIn();
				Vector3 newPos = opponentOverride.position;
				newPos.x *= -mirror;
				opControlsScript.activePullIn.position = transform.position + newPos;
				opControlsScript.activePullIn.speed = opponentOverride.blendSpeed;
				
				if (opponentOverride.resetAppliedForces){
					opPhysicsScript.ResetForces(true, true);
					myPhysicsScript.ResetForces(true, true);
				}
				
				opponentOverride.casted = true;
			}
		}
		
		// Check Camera Movements (cinematics)
		foreach (CameraMovement cameraMovement in move.cameraMovements){
			if (cameraMovement.over) continue;
			if (cameraMovement.casted && !cameraMovement.over && cameraMovement.time >= cameraMovement.duration && UFE.freeCamera){
				cameraMovement.over = true;
				ReleaseCam();
			}
			if (move.currentFrame >= cameraMovement.castingFrame){
				cameraMovement.time += Time.fixedDeltaTime;
				if (cameraMovement.casted) continue;
				cameraMovement.casted = true;
				
				PausePlayAnimation(true, cameraMovement.myAnimationSpeed * .01f);
				opControlsScript.PausePlayAnimation(true, cameraMovement.opAnimationSpeed * .01f);
				myPhysicsScript.freeze = cameraMovement.freezePhysics;
				opPhysicsScript.freeze = cameraMovement.freezePhysics;
				cameraScript.cinematicFreeze = cameraMovement.freezePhysics;
				
				if (cameraMovement.cinematicType == CinematicType.CameraEditor){
					cameraMovement.position.x *= -mirror;
					Vector3 targetPosition = transform.TransformPoint(cameraMovement.position);
					Vector3 targetRotation = cameraMovement.rotation;
					targetRotation.y *= -mirror;
					targetRotation.z *= -mirror;
					cameraScript.MoveCameraToLocation(targetPosition,
					                                  targetRotation,
					                                  cameraMovement.fieldOfView,
					                                  cameraMovement.camSpeed, gameObject.name);
					
				}else if (cameraMovement.cinematicType == CinematicType.Prefab){
					cameraScript.SetCameraOwner(gameObject.name);
					emulatedCam = (GameObject) Instantiate(cameraMovement.prefab);
					emulatedCam.transform.position = transform.position;
					
				}else if (cameraMovement.cinematicType == CinematicType.AnimationFile){
					emulatedCam = new GameObject();
					emulatedCam.name = "Camera Parent";
					emulatedCam.transform.parent = transform;
					emulatedCam.transform.localPosition = cameraMovement.gameObjectPosition;
					emulatedCam.AddComponent(typeof(Animation));
					emulatedCam.GetComponent<Animation>().AddClip(cameraMovement.animationClip, "cam");
					emulatedCam.GetComponent<Animation>()["cam"].speed = cameraMovement.camAnimationSpeed;
					emulatedCam.GetComponent<Animation>().Play("cam");
					cameraMovement.duration = cameraMovement.animationClip.length/cameraMovement.camAnimationSpeed;
					
					Camera.main.transform.parent = emulatedCam.transform;
					cameraScript.MoveCameraToLocation(cameraMovement.position,
					                                  cameraMovement.rotation,
					                                  cameraMovement.fieldOfView,
					                                  cameraMovement.blendSpeed, gameObject.name);
					
				}
			}
		}
#endif
		
		// Check Invincible Body Parts
		if (move.invincibleBodyParts.Length > 0) {
			foreach (InvincibleBodyParts invBodyPart in move.invincibleBodyParts){
				if (move.currentFrame >= invBodyPart.activeFramesBegin &&
				    move.currentFrame < invBodyPart.activeFramesEnds) {
					if (invBodyPart.completelyInvincible){
						myHitBoxesScript.HideHitBoxes(true);
					}else{
						myHitBoxesScript.HideHitBoxes(invBodyPart.hitBoxes, true);
					}
					ignoreCollisionMass = invBodyPart.ignoreBodyColliders;
				}
				if (move.currentFrame >= invBodyPart.activeFramesEnds) {
					if (invBodyPart.completelyInvincible){
						myHitBoxesScript.HideHitBoxes(false);
					}else{
						myHitBoxesScript.HideHitBoxes(invBodyPart.hitBoxes, false);
					}
					ignoreCollisionMass = false;
				}
			}
		}
		
		// Check Blockable Area
		if (move.blockableArea.bodyPart != BodyPart.none){
			if (move.currentFrame >= move.blockableArea.activeFramesBegin &&
			    move.currentFrame < move.blockableArea.activeFramesEnds) {
				myHitBoxesScript.blockableArea = move.blockableArea;
				myHitBoxesScript.blockableArea.position = myHitBoxesScript.GetPosition(myHitBoxesScript.blockableArea.bodyPart);

				if (!opControlsScript.isBlocking
				    && !opControlsScript.blockStunned
				    && opControlsScript.currentSubState != SubStates.Stunned
				    && opHitBoxesScript.TestCollision(myHitBoxesScript.blockableArea) != Vector3.zero) {
					opControlsScript.CheckBlocking(true);
				}
			}else if (move.currentFrame >= move.blockableArea.activeFramesEnds){
				if (UFE.config.blockOptions.blockType == BlockType.HoldBack ||
				    UFE.config.blockOptions.blockType == BlockType.AutoBlock) opControlsScript.CheckBlocking(false);
			}
		}
		
#if !UFE_BASIC
		// Check Frame Links
		foreach (FrameLink frameLink in move.frameLinks){
			if (move.currentFrame >= frameLink.activeFramesBegins &&
                 move.currentFrame <= frameLink.activeFramesEnds) {
                if ((frameLink.linkType == LinkType.HitConfirm &&
                     (move.hitConfirmOnStrike && frameLink.onStrike) ||
                     (move.hitConfirmOnBlock && frameLink.onBlock) ||
                     (move.hitConfirmOnParry && frameLink.onParry))) {
                    frameLink.cancelable = true;
                    move.cancelable = true;
                } else if (frameLink.linkType == LinkType.NoConditions) {
                    frameLink.cancelable = true;
                    move.cancelable = true;
                }
            } else if (move.currentFrame >= (frameLink.activeFramesBegins - UFE.config.executionBufferTime) &&
                move.currentFrame <= frameLink.activeFramesEnds && frameLink.allowBuffer) {
                if ((frameLink.linkType == LinkType.HitConfirm && 
                    (move.hitConfirmOnStrike && frameLink.onStrike) ||
                    (move.hitConfirmOnBlock && frameLink.onBlock) ||
                    (move.hitConfirmOnParry && frameLink.onParry))) {
                    frameLink.cancelable = true;
                    move.cancelable = true;
                } else if (frameLink.linkType == LinkType.NoConditions) {
                    frameLink.cancelable = true;
                    move.cancelable = true;
                }
			}else{
				frameLink.cancelable = false;
				//move.cancelable = false;
			}
		}
#endif
		
		// Check Hits
		foreach (Hit hit in move.hits){
			HurtBox[] activeHurtBoxes = null;
			if (move.currentFrame >= hit.activeFramesBegin &&
			    move.currentFrame < hit.activeFramesEnds) {
				if (hit.hurtBoxes.Length > 0){
					activeHurtBoxes = hit.hurtBoxes;
					if (hit.disabled) continue;
					if (!opControlsScript.ValidateHit(hit)) continue;
					
					foreach(HurtBox hurtBox in activeHurtBoxes) {
						hurtBox.position = myHitBoxesScript.GetPosition(hurtBox.bodyPart);
						hurtBox.rendererBounds = myHitBoxesScript.GetBounds();
					}
					
					Vector3 collisionVector_hit = opHitBoxesScript.TestCollision(activeHurtBoxes, hit.hitConfirmType);
					if (collisionVector_hit != Vector3.zero) { // HURTBOX TEST
						// Tech Throw
						if (hit.hitConfirmType == HitConfirmType.Throw 
						    && hit.techable
						    && opControlsScript.currentMove != null 
						    && opControlsScript.currentMove.IsThrow(true)
						    ){
							CastMove(hit.techMove, true);
							opControlsScript.CastMove(opControlsScript.currentMove.GetTechMove(), true);
							return;

						// Throw
						}else if (hit.hitConfirmType == HitConfirmType.Throw){
							CastMove(hit.throwMove, true);
							return;
						
						// Block
						}else if (opControlsScript.currentSubState != SubStates.Stunned 
						          && opControlsScript.currentMove == null 
						          && opControlsScript.isBlocking 
						          && opControlsScript.TestBlockStances(hit.hitType)
						          ){
							opControlsScript.GetHitBlocking(hit, move.totalFrames - move.currentFrame, collisionVector_hit);
							AddGauge(move.gaugeGainOnBlock);
							opControlsScript.AddGauge(move.opGaugeGainOnBlock);
							move.hitConfirmOnBlock = true;
							
						// Parry
						}else if (opControlsScript.potentialParry > 0 
						          && opControlsScript.currentMove == null 
						          && hit.hitConfirmType != HitConfirmType.Throw 
						          && opControlsScript.TestParryStances(hit.hitType)
						          ){
							opControlsScript.GetHitParry(hit, move.totalFrames - move.currentFrame, collisionVector_hit);
							opControlsScript.AddGauge(move.opGaugeGainOnParry);
							move.hitConfirmOnParry = true;
							
						// Hit
						}else {
							opControlsScript.GetHit(hit, move.totalFrames - move.currentFrame, collisionVector_hit);
							AddGauge(move.gaugeGainOnHit);
							opControlsScript.AddGauge(move.opGaugeGainOnHit);
							
							if (hit.pullSelfIn.enemyBodyPart != BodyPart.none && hit.pullSelfIn.characterBodyPart != BodyPart.none){
								Vector3 newPos = opHitBoxesScript.GetPosition(hit.pullSelfIn.enemyBodyPart);
								if (newPos != Vector3.zero){
									activePullIn = new PullIn();
									activePullIn.position = transform.position + (newPos - myHitBoxesScript.GetPosition(hit.pullSelfIn.characterBodyPart));
									activePullIn.speed = hit.pullSelfIn.speed;
									activePullIn.forceStand = hit.pullEnemyIn.forceStand;
									activePullIn.position.z = 0;
									if (hit.pullEnemyIn.forceStand){
										activePullIn.position.y = 0;
										myPhysicsScript.ForceGrounded();
									}
								}
							}
							move.hitConfirmOnStrike = true;
						}
						myPhysicsScript.ResetForces(hit.resetPreviousHorizontal, hit.resetPreviousVertical);
						myPhysicsScript.AddForce(hit.appliedForce, -mirror);
						
						if ((opponent.transform.position.x >= UFE.config.selectedStage.rightBoundary - 2 ||
						     opponent.transform.position.x <= UFE.config.selectedStage.leftBoundary + 2) 
						    && myPhysicsScript.IsGrounded() 
						    && !UFE.config.comboOptions.neverCornerPush && hit.cornerPush
						    ){
							
							myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, false);
							myPhysicsScript.AddForce(
								new Vector2(hit.pushForce.x + (opPhysicsScript.airTime * opInfo.physics.friction), 0), mirror);
						}
						
						if (opPhysicsScript.freeze){
							HitPause(GetHitAnimationSpeed(hit.hitStrength) * .01f);
							UFE.DelaySynchronizedAction(this.HitUnpause, GetHitFreezingTime(hit.hitStrength));
						}
						if (!hit.continuousHit) hit.disabled = true;
					};
				}
			}
			myHitBoxesScript.activeHurtBoxes = activeHurtBoxes;
		}
		
		if(move.currentFrame >= move.totalFrames) {
			if (move.name == "Intro") {
				introPlayed = true;
				UFE.CastNewRound();
			}
			KillCurrentMove();
		}
	}

	// Imediately cancels any move being executed
	public void KillCurrentMove(){
		if (currentMove == null) return;
		currentMove.currentFrame = 0;
		currentMove.currentTick = 0;
        //myMoveSetScript.SetAnimationSpeed(currentMove.name, currentMove.animationSpeed);

		myHitBoxesScript.activeHurtBoxes = null;
		myHitBoxesScript.blockableArea = null;
		myHitBoxesScript.HideHitBoxes(false);
		ignoreCollisionMass = false;
		if (UFE.config.blockOptions.blockType == BlockType.HoldBack ||
		    UFE.config.blockOptions.blockType == BlockType.AutoBlock) opControlsScript.CheckBlocking(false);


		if (currentMove.applyRootMotion) {
			character.transform.localPosition = new Vector3(0, 0, 0);

			Vector3 newPosition = character.transform.position;

			if (currentMove.rootMotionNode != BodyPart.none){
				newPosition = myHitBoxesScript.GetPosition(currentMove.rootMotionNode);
			}
			newPosition.z = 0;

			transform.position = newPosition;

            if (currentMove.forceGrounded) myPhysicsScript.ForceGrounded();
		}

		if (currentMove.disableHeadLook) ToggleHeadLook(true);

		if (currentMove.invertRotationLeft && mirror == -1) InvertRotation();
		if (currentMove.forceMirrorLeft && mirror == -1) ForceMirror(false);

		if (currentMove.invertRotationRight && mirror == 1) InvertRotation();
		if (currentMove.forceMirrorRight && mirror == 1) ForceMirror(UFE.config.characterRotationOptions.autoMirror);
		
		testCharacterRotation(100);

		if (stunTime > 0){
			standUpOverride = currentMove.standUpOptions;
			if (standUpOverride != StandUpOptions.None) currentState = PossibleStates.Down;
		}

		currentMove = null;
		ReleaseCam();
	}

	// Release character to be playable again
	private void ReleaseStun(){
		if (currentSubState != SubStates.Stunned && !blockStunned) return;
		if (!isBlocking && comboHits > 1) {
			UFE.FireAlert(UFE.config.selectedLanguage.combo, opInfo);
		}
		currentSubState = SubStates.Resting;
		blockStunned = false;
		stunTime = 0;
		comboHits = 0;
		comboDamage = 0;
		comboHitDamage = 0;
		airJuggleHits = 0;
		CheckBlocking(false);
		
		standUpOverride = StandUpOptions.None;
		
		myPhysicsScript.ResetWeight();
		if (!isDead) ToggleHeadLook(true);

		if (myPhysicsScript.IsGrounded()) currentState = PossibleStates.Stand;
		translateInputs(inputController);

		myHitBoxesScript.HideHitBoxes(false);
	}

	private void ReleaseCam(){
		if (cameraScript.GetCameraOwner() != gameObject.name) return;
		if (outroPlayed && UFE.config.roundOptions.freezeCamAfterOutro) return;
		Camera.main.transform.parent = null;

		if (emulatedCam != null) Destroy (emulatedCam);

		opControlsScript.PausePlayAnimation(false);
		PausePlayAnimation(false);
		cameraScript.ReleaseCam();
		myPhysicsScript.freeze = false;
		opPhysicsScript.freeze = false;
	}

	public bool TestBlockStances(HitType hitType){
		if (UFE.config.blockOptions.blockType == BlockType.None) return false;
		if ((hitType == HitType.Mid || hitType == HitType.MidKnockdown || hitType == HitType.Launcher) && myPhysicsScript.IsGrounded()) return true;
		if ((hitType == HitType.Overhead || hitType == HitType.HighKnockdown) && currentState == PossibleStates.Crouch) return false;
		if ((hitType == HitType.Sweep || hitType == HitType.Low) && currentState != PossibleStates.Crouch) return false;
		if (!UFE.config.blockOptions.allowAirBlock && !myPhysicsScript.IsGrounded()) return false;
		return true;
	}
	
	public bool TestParryStances(HitType hitType){
		if (UFE.config.blockOptions.parryType == ParryType.None) return false;
		if ((hitType == HitType.Mid || hitType == HitType.MidKnockdown || hitType == HitType.Launcher) && myPhysicsScript.IsGrounded()) return true;
		if ((hitType == HitType.Overhead || hitType == HitType.HighKnockdown) && currentState == PossibleStates.Crouch) return false;
		if ((hitType == HitType.Sweep || hitType == HitType.Low) && currentState != PossibleStates.Crouch) return false;
		if (!UFE.config.blockOptions.allowAirParry && !myPhysicsScript.IsGrounded()) return false;
		return true;
	}
	
	public void CheckBlocking(bool flag){
		if (myPhysicsScript.freeze) return;
		if (myPhysicsScript.isTakingOff) return;
		if (flag){
			if (potentialBlock && UFE.config.hitOptions.resetAnimationOnHit){
				if (currentMove != null) {
					potentialBlock = false;
					return;
				}
				if (currentState == PossibleStates.Crouch){
					if (myMoveSetScript.basicMoves.blockingCrouchingPose.clip1 == null)
						Debug.LogError("Blocking Crouching Pose animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Crouching Pose");
					myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingCrouchingPose, false);
					isBlocking = true;
				}else if (currentState == PossibleStates.Stand){
					if (myMoveSetScript.basicMoves.blockingHighPose.clip1 == null)
						Debug.LogError("Blocking High Pose animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking High Pose");
					myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingHighPose, false);
					isBlocking = true;
				}else if (!myPhysicsScript.IsGrounded() && UFE.config.blockOptions.allowAirBlock){
					if (myMoveSetScript.basicMoves.blockingAirPose.clip1 == null)
						Debug.LogError("Blocking Air Pose animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Air Pose");
					myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingAirPose, false);
					isBlocking = true;
				}
			}
		}else if (!blockStunned){
			isBlocking = false;
		}
	}
	
	private void HighlightOn(GameObject target, bool flag){
		Renderer[] charRenders = target.GetComponentsInChildren<Renderer>();
		if (flag && !lit){
			lit = true;
			foreach(Renderer charRender in charRenders){
				charRender.material.shader = Shader.Find("VertexLit");
				charRender.material.color = UFE.config.blockOptions.parryColor;
			}
		}else if (lit){
			lit = false;
			for(int i = 0; i < charRenders.Length; i ++){
				charRenders[i].material.shader = normalShaders[i];
				charRenders[i].material.color = normalColors[i];
			}
		}
	}
	
	private void HighlightOff(){
		HighlightOn(character, false);
	}

	public bool ValidateHit(Hit hit){
		if (comboHits >= UFE.config.comboOptions.maxCombo) return false;
		if (!hit.groundHit && myPhysicsScript.IsGrounded()) return false;
		if (!hit.airHit && !myPhysicsScript.IsGrounded()) return false;
		if (!hit.stunHit && currentSubState == SubStates.Stunned) return false;
		if (!hit.downHit && currentState == PossibleStates.Down) return false;

		return true;
	}

	public void GetHitParry(Hit hit, int remainingFrames, Vector3 location){
		UFE.FireAlert(UFE.config.selectedLanguage.parry, myInfo);

		BasicMoveInfo currentHitInfo = myMoveSetScript.basicMoves.parryHigh;
		blockStunned = true;
		currentSubState = SubStates.Blocking;

		myHitBoxesScript.isHit = true;

		potentialParry = 0;

		if (UFE.config.blockOptions.resetButtonSequence){
			myMoveSetScript.ClearLastButtonSequence();
		}

		if (UFE.config.blockOptions.parryStunType == ParryStunType.Fixed){
			stunTime = (float)UFE.config.blockOptions.parryStunFrames/(float)UFE.config.fps;
		}else{
			int stunFrames = 0;
			if (hit.hitStunType == HitStunType.FrameAdvantage) {
				stunFrames = hit.frameAdvantageOnBlock + remainingFrames;
				stunFrames *= (UFE.config.blockOptions.parryStunFrames/100);
				if (stunFrames < 1) stunFrames = 1;
				stunTime = (float)stunFrames/(float)UFE.config.fps;
			}else if (hit.hitStunType == HitStunType.Frames) {
				stunFrames = (int) hit.hitStunOnBlock;
				stunFrames *= (UFE.config.blockOptions.parryStunFrames/100);
				if (stunFrames < 1) stunFrames = 1;
				stunTime = (float)stunFrames/(float)UFE.config.fps;
			}else{
				stunTime = hit.hitStunOnBlock * (UFE.config.blockOptions.parryStunFrames/100);
			}
		}
		
		// Create hit parry effect
		GameObject particle = UFE.config.blockOptions.parryHitEffects.hitParticle;
		float killTime = UFE.config.blockOptions.parryHitEffects.killTime;
		AudioClip soundEffect = UFE.config.blockOptions.parryHitEffects.hitSound;
		if (location != Vector3.zero && particle != null){
			GameObject pTemp = (GameObject) Instantiate(particle);
			pTemp.transform.position = location;
			pTemp.transform.localScale = new Vector3(-mirror, 1, 1);
			Destroy(pTemp, killTime);
		}
		UFE.PlaySound(soundEffect);
		
		// Shake Options
		shakeCamera = UFE.config.blockOptions.parryHitEffects.shakeCameraOnHit;
		shakeCharacter = UFE.config.blockOptions.parryHitEffects.shakeCharacterOnHit;
		shakeDensity = UFE.config.blockOptions.parryHitEffects.shakeDensity;


		
		// Get correct animation according to stance
        if (currentState == PossibleStates.Crouch) {
            currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.parryCrouching, hit);
			currentHitInfo = myMoveSetScript.basicMoves.parryCrouching;
			if (!myMoveSetScript.AnimationExists(currentHitAnimation))
                Debug.LogError("Parry Crouching animation not found! Make sure you have it set on Character -> Basic Moves -> Parry Animations -> Crouching");
		}else if (currentState == PossibleStates.Stand){
            HitBox strokeHit = myHitBoxesScript.GetStrokeHitBox();
            if (strokeHit.type == HitBoxType.low && myMoveSetScript.basicMoves.parryLow.clip1 != null) {
                currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.parryLow, hit);
                currentHitInfo = myMoveSetScript.basicMoves.parryLow;

            } else {
                currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.parryHigh, hit);
                currentHitInfo = myMoveSetScript.basicMoves.parryHigh;
                if (!myMoveSetScript.AnimationExists(currentHitAnimation))
                    Debug.LogError("Parry High animation not found! Make sure you have it set on Character -> Basic Moves -> Parry Animations -> Standing");

            }
        } else if (!myPhysicsScript.IsGrounded()) {
            currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.parryAir, hit);
			currentHitInfo = myMoveSetScript.basicMoves.parryAir;
			if (!myMoveSetScript.AnimationExists(currentHitAnimation))
                Debug.LogError("Parry Air animation not found! Make sure you have it set on Character -> Basic Moves -> Parry Animations -> Air");
		}

		myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation);
        if (currentHitInfo.autoSpeed) {
            myMoveSetScript.SetAnimationSpeed(currentHitAnimation, (myMoveSetScript.GetAnimationLengh(currentHitAnimation) / stunTime));
        }
		
		// Highlight effect when parry
		if (UFE.config.blockOptions.highlightWhenParry){
			HighlightOn(gameObject, true);
			UFE.DelaySynchronizedAction(this.HighlightOff, 0.2f);
		}
		
		// Freeze screen depending on how strong the hit was
		HitPause(GetHitAnimationSpeed(hit.hitStrength) * .01f);
		UFE.DelaySynchronizedAction(this.HitUnpause, GetHitFreezingTime(hit.hitStrength));
		
		// Reset hit to allow for another hit while the character is still stunned
		float spaceBetweenHits = 1;
		if (hit.spaceBetweenHits == Sizes.Small){
			spaceBetweenHits = 1.1f;
		}else if (hit.spaceBetweenHits == Sizes.Medium){
			spaceBetweenHits = 1.3f;
		}else if (hit.spaceBetweenHits == Sizes.High){
			spaceBetweenHits = 1.7f;
		}

		UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit,GetHitFreezingTime(hit.hitStrength) * spaceBetweenHits * 1.2f);
		
		// Add force to the move
		myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush);

		if (!UFE.config.blockOptions.ignoreAppliedForceParry)
			myPhysicsScript.AddForce(new Vector3(hit.pushForce.x, 0, 0), -opControlsScript.mirror);

	}

	public void GetHitBlocking(Hit hit, int remainingFrames, Vector3 location){
		// Lose life
		if (hit.damageOnBlock >= myInfo.currentLifePoints){
			GetHit(hit, remainingFrames, location);
			return;
		}else{
			DamageMe(hit.damageOnBlock);
		}

		blockStunned = true;
		currentSubState = SubStates.Blocking;
		
		myHitBoxesScript.isHit = true;

		int stunFrames = 0;
		BasicMoveInfo currentHitInfo = myMoveSetScript.basicMoves.blockingHighHit;

		if (hit.hitStunType == HitStunType.FrameAdvantage) {
			stunFrames = hit.frameAdvantageOnBlock + remainingFrames;
			if (stunFrames < 1) stunFrames = 1;
			stunTime = (float)stunFrames/(float)UFE.config.fps;
		}else if (hit.hitStunType == HitStunType.Frames) {
			stunFrames = (int) hit.hitStunOnBlock;
			if (stunFrames < 1) stunFrames = 1;
			stunTime = (float)stunFrames/(float)UFE.config.fps;
		}else{
			stunTime = hit.hitStunOnBlock;
		}
		
		// Create hit blocking effect
		GameObject particle = UFE.config.blockOptions.blockHitEffects.hitParticle;
		float killTime = UFE.config.blockOptions.blockHitEffects.killTime;
		AudioClip soundEffect = UFE.config.blockOptions.blockHitEffects.hitSound;
		if (location != Vector3.zero && particle != null){
			GameObject pTemp = (GameObject) Instantiate(particle);
			pTemp.transform.position = location;
			pTemp.transform.localScale = new Vector3(-mirror, 1, 1);
			Destroy(pTemp, killTime);
		}
		UFE.PlaySound(soundEffect);

		// Shake Options
		shakeCamera = UFE.config.blockOptions.blockHitEffects.shakeCameraOnHit;
		shakeCharacter = UFE.config.blockOptions.blockHitEffects.shakeCharacterOnHit;
		shakeDensity = UFE.config.blockOptions.blockHitEffects.shakeDensity;


		if (currentState == PossibleStates.Crouch){
			currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingCrouchingHit, hit);
			currentHitInfo = myMoveSetScript.basicMoves.blockingCrouchingHit;

			if (!myMoveSetScript.AnimationExists(currentHitAnimation))
				Debug.LogError("Blocking Crouching Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Crouching Hit");
		}else if (currentState == PossibleStates.Stand){
			HitBox strokeHit = myHitBoxesScript.GetStrokeHitBox();
			if (strokeHit.type == HitBoxType.low && myMoveSetScript.basicMoves.blockingLowHit.clip1 != null){
				currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingLowHit, hit);
				currentHitInfo = myMoveSetScript.basicMoves.blockingLowHit;

			}else{
				currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingHighHit, hit);
				currentHitInfo = myMoveSetScript.basicMoves.blockingHighHit;
				if (!myMoveSetScript.AnimationExists(currentHitAnimation))
					Debug.LogError("Blocking High Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking High Hit");

			}

		}else if (!myPhysicsScript.IsGrounded()){
			currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingAirHit, hit);
			currentHitInfo = myMoveSetScript.basicMoves.blockingAirHit;
			if (!myMoveSetScript.AnimationExists(currentHitAnimation))
				Debug.LogError("Blocking Air Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Air Hit");
		}


        myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation);
        float hitAnimationSpeed = myMoveSetScript.GetAnimationLengh(currentHitAnimation) / stunTime;

        if (currentHitInfo.autoSpeed) {
            myMoveSetScript.SetAnimationSpeed(currentHitAnimation, hitAnimationSpeed);
        }
        if (UFE.config.hitOptions.useHitStunDeceleration) {
            hitStunDeceleration = hitAnimationSpeed + (stunTime / 2);
        }
		
		// Freeze screen depending on how strong the hit was
		HitPause(GetHitAnimationSpeed(hit.hitStrength) * .01f);
		UFE.DelaySynchronizedAction(this.HitUnpause, GetHitFreezingTime(hit.hitStrength));
		
		// Reset hit to allow for another hit while the character is still stunned
		float spaceBetweenHits = 1;
		if (hit.spaceBetweenHits == Sizes.Small){
			spaceBetweenHits = 1.1f;
		}else if (hit.spaceBetweenHits == Sizes.Medium){
			spaceBetweenHits = 1.3f;
		}else if (hit.spaceBetweenHits == Sizes.High){
			spaceBetweenHits = 1.7f;
		}
		UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, GetHitFreezingTime(hit.hitStrength) * spaceBetweenHits);
		
		// Add force to the move
		myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush);

        if (!UFE.config.blockOptions.ignoreAppliedForceBlock)
            myPhysicsScript.AddForce(new Vector3(hit.pushForce.x, 0, 0), -opControlsScript.mirror);
	}
	
	public void GetHit(Hit hit, int remainingFrames, Vector3 location){
		// Get what animation should be played depending on the character's state
		bool airHit = false;
		bool armored = false;
		bool isKnockDown = false;
		float verticalPush = 0;
		float damageModifier = 1;
		float hitStunModifier = 1;
		BasicMoveInfo currentHitInfo;
		hitStunDeceleration = 0;
		
		myHitBoxesScript.isHit = true;

		if (myInfo.headLook.disableOnHit) ToggleHeadLook(false);

		if (currentMove != null && currentMove.frameLinks.Length > 0){
			foreach (FrameLink frameLink in currentMove.frameLinks){
				if (currentMove.currentFrame >= frameLink.activeFramesBegins &&
				    currentMove.currentFrame <= frameLink.activeFramesEnds) {
					if (frameLink.linkType == LinkType.CounterMove){
						bool cancelable = false;
						if (frameLink.counterMoveType == CounterMoveType.SpecificMove){
							if (frameLink.counterMoveFilter == currentMove) cancelable = true;
						}else{
							HitBox strokeHitBox = myHitBoxesScript.GetStrokeHitBox();
							if ((frameLink.anyHitStrength || frameLink.hitStrength == hit.hitStrength) &&
							    (frameLink.anyStrokeHitBox || frameLink.hitBoxType == strokeHitBox.type) &&
							    (frameLink.anyHitType || frameLink.hitType == hit.hitType)){
								cancelable = true;
							}
						}

						if (cancelable){
							frameLink.cancelable = true;
							currentMove.cancelable = true;
							
							if (frameLink.disableHitImpact) {
								float timeLeft = (float)(currentMove.totalFrames - currentMove.currentFrame)/(float)UFE.config.fps;

								myHitBoxesScript.ResetHit();
								UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, timeLeft);
								return;
							}
						}
					}
				}
			}
		}
		
		// Set position in case of pull enemy in
		activePullIn = null;
		if (hit.pullEnemyIn.enemyBodyPart != BodyPart.none && hit.pullEnemyIn.characterBodyPart != BodyPart.none){
			Vector3 newPos = myHitBoxesScript.GetPosition(hit.pullEnemyIn.enemyBodyPart);
			if (newPos != Vector3.zero){
				activePullIn = new PullIn();
				activePullIn.position = transform.position + (opHitBoxesScript.GetPosition(hit.pullEnemyIn.characterBodyPart) - newPos);
				activePullIn.speed = hit.pullEnemyIn.speed;
				activePullIn.forceStand = hit.pullEnemyIn.forceStand;
				activePullIn.position.z = 0;
				if (hit.pullEnemyIn.forceStand) {
					activePullIn.position.y = 0;
					myPhysicsScript.ForceGrounded();
				}
			}
		}

		if (myPhysicsScript.IsGrounded()) {
			if (hit.hitStrength == HitStrengh.Crumple){
				if (myMoveSetScript.basicMoves.getHitCrumple.clip1 == null)
					Debug.LogError("Get Hit Crumple animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Crumple");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitCrumple.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitCrumple;
				if (myMoveSetScript.basicMoves.getHitCrumple.invincible) myHitBoxesScript.HideHitBoxes(true);
			}else if (hit.hitType == HitType.Launcher){
				if (myMoveSetScript.basicMoves.getHitAir.clip1 == null)
					Debug.LogError("Get Hit Air animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Air");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitAir.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitAir;
				if (myMoveSetScript.basicMoves.getHitAir.invincible) myHitBoxesScript.HideHitBoxes(true);
				airHit = true;
			}else if (hit.hitType == HitType.KnockBack){
				if (myMoveSetScript.basicMoves.getHitKnockBack.clip1 == null)
					Debug.LogError("Get Hit Knock Back animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Knock Back");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitKnockBack.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitKnockBack;
				if (myMoveSetScript.basicMoves.getHitKnockBack.invincible) myHitBoxesScript.HideHitBoxes(true);
				airHit = true;
			}else if (hit.hitType == HitType.HighKnockdown){
				if (myMoveSetScript.basicMoves.getHitHighKnockdown.clip1 == null)
					Debug.LogError("Get Hit High Knockdown animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit High Knockdown");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitHighKnockdown.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitHighKnockdown;
				if (myMoveSetScript.basicMoves.getHitHighKnockdown.invincible) myHitBoxesScript.HideHitBoxes(true);
				isKnockDown = true;
			}else if (hit.hitType == HitType.MidKnockdown){
				if (myMoveSetScript.basicMoves.getHitHighLowKnockdown.clip1 == null)
					Debug.LogError("Get Hit High-Low Knockdown animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit High-Low Knockdown");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitHighLowKnockdown.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitHighLowKnockdown;
				if (myMoveSetScript.basicMoves.getHitHighLowKnockdown.invincible) myHitBoxesScript.HideHitBoxes(true);
				isKnockDown = true;
			}else if (hit.hitType == HitType.Sweep){
				if (myMoveSetScript.basicMoves.getHitSweep.clip1 == null)
					Debug.LogError("Get Hit Sweep animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Sweep");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitSweep.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitSweep;
				if (myMoveSetScript.basicMoves.getHitSweep.invincible) myHitBoxesScript.HideHitBoxes(true);
				isKnockDown = true;
			}else if (currentState == PossibleStates.Crouch){
				if (myMoveSetScript.basicMoves.getHitCrouching.clip1 == null)
					Debug.LogError("Get Hit Crouching animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Crouching");
				currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitCrouching, hit);
				currentHitInfo = myMoveSetScript.basicMoves.getHitCrouching;
				if (myMoveSetScript.basicMoves.getHitCrouching.invincible) myHitBoxesScript.HideHitBoxes(true);
			}else{
				HitBox strokeHit = myHitBoxesScript.GetStrokeHitBox();
				if (strokeHit.type == HitBoxType.low && myMoveSetScript.basicMoves.getHitLow.clip1 != null){
					currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitLow, hit);
					currentHitInfo = myMoveSetScript.basicMoves.getHitLow;
					if (myMoveSetScript.basicMoves.getHitLow.invincible) myHitBoxesScript.HideHitBoxes(true);
				}else{
					if (myMoveSetScript.basicMoves.getHitHigh.clip1 == null)
						Debug.LogError("Get Hit High animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit High");
					currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitHigh, hit);
					currentHitInfo = myMoveSetScript.basicMoves.getHitHigh;
					if (myMoveSetScript.basicMoves.getHitHigh.invincible) myHitBoxesScript.HideHitBoxes(true);
				}
			}
		}else{
			if (hit.hitStrength == HitStrengh.Crumple && myMoveSetScript.basicMoves.getHitKnockBack.clip1 != null){
				currentHitAnimation = myMoveSetScript.basicMoves.getHitKnockBack.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitKnockBack;
			}else{
				if (myMoveSetScript.basicMoves.getHitAir.clip1 == null)
					Debug.LogError("Get Hit Air animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Air");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitAir.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitAir;
			}
			airHit = true;
		}
		
		// Differenciate hit types
		HitTypeOptions hitEffects = hit.hitEffects;
		if (!hit.overrideHitEffects) {
			if (hit.hitStrength == HitStrengh.Weak) hitEffects = UFE.config.hitOptions.weakHit;
			if (hit.hitStrength == HitStrengh.Medium) hitEffects = UFE.config.hitOptions.mediumHit;
			if (hit.hitStrength == HitStrengh.Heavy) hitEffects = UFE.config.hitOptions.heavyHit;
			if (hit.hitStrength == HitStrengh.Crumple) hitEffects = UFE.config.hitOptions.crumpleHit;
			if (hit.hitStrength == HitStrengh.Custom1) hitEffects = UFE.config.hitOptions.customHit1;
			if (hit.hitStrength == HitStrengh.Custom2) hitEffects = UFE.config.hitOptions.customHit2;
			if (hit.hitStrength == HitStrengh.Custom3) hitEffects = UFE.config.hitOptions.customHit3;
		}

		// Cancel current move if any
		if (!hit.armorBreaker && currentMove != null && currentMove.armorOptions.hitAbsorption > 0 &&
		    currentMove.currentFrame >= currentMove.armorOptions.activeFramesBegin && 
		    currentMove.currentFrame <= currentMove.armorOptions.activeFramesEnds){
			armored = true;
			currentMove.armorOptions.hitAbsorption --;
			damageModifier -= currentMove.armorOptions.damageAbsorption * .01f;
			if (currentMove.armorOptions.overrideHitEffects) 
				hitEffects = currentMove.armorOptions.hitEffects;

		}else if (currentMove != null && !currentMove.hitAnimationOverride){
			if ((UFE.config.counterHitOptions.startUpFrames && currentMove.currentFrameData == CurrentFrameData.StartupFrames) ||
			    (UFE.config.counterHitOptions.activeFrames && currentMove.currentFrameData == CurrentFrameData.ActiveFrames) ||
			    (UFE.config.counterHitOptions.recoveryFrames && currentMove.currentFrameData == CurrentFrameData.RecoveryFrames)){
				UFE.FireAlert(UFE.config.selectedLanguage.counterHit, opInfo);
				damageModifier += UFE.config.counterHitOptions.damageIncrease * .01f;
				hitStunModifier += UFE.config.counterHitOptions.hitStunIncrease * .01f;
			}
			storedMove = null;

			KillCurrentMove();
		}
		
		// Create hit effect
		if (location != Vector3.zero && hitEffects.hitParticle != null){
			GameObject pTemp = (GameObject) Instantiate(hitEffects.hitParticle, location, Quaternion.identity);
			Destroy(pTemp, hitEffects.killTime);
		}

		// Play sound
		UFE.PlaySound(hitEffects.hitSound);

		// Shake Options
		shakeCamera = hitEffects.shakeCameraOnHit;
		shakeCharacter = hitEffects.shakeCharacterOnHit;
		shakeDensity = hitEffects.shakeDensity;

		// Cast First Hit if true
		if (!firstHit && !opControlsScript.firstHit){
			opControlsScript.firstHit = true;
			UFE.FireAlert(UFE.config.selectedLanguage.firstHit, opInfo);
		}
		UFE.FireHit(myHitBoxesScript.GetStrokeHitBox(), opControlsScript.currentMove, opInfo);


		// Convert Percentage
		if (hit.damageType == DamageType.Percentage) hit.damageOnHit = myInfo.lifePoints * (hit.damageOnHit/100);


		// Damage deterioration
		float damage = 0;
		if (!hit.damageScaling || UFE.config.comboOptions.damageDeterioration == Sizes.None){
			damage = hit.damageOnHit;
		}else if (UFE.config.comboOptions.damageDeterioration == Sizes.Small){
			damage = hit.damageOnHit - (hit.damageOnHit * (float)comboHits * .1f);
		}else if (UFE.config.comboOptions.damageDeterioration == Sizes.Medium){
			damage = hit.damageOnHit - (hit.damageOnHit * (float)comboHits * .2f);
		}else if (UFE.config.comboOptions.damageDeterioration == Sizes.High){
			damage = hit.damageOnHit - (hit.damageOnHit * (float)comboHits * .4f);
		}
		if (damage < UFE.config.comboOptions.minDamage) damage = UFE.config.comboOptions.minDamage;
		damage *= damageModifier;
        comboHitDamage = damage;
        comboDamage += damage;
		comboHits ++;

		// Lose life
		isDead = DamageMe(damage, hit.doesntKill);


		// Reset hit to allow for another hit while the character is still stunned
		float spaceBetweenHits = 1;
		if (hit.spaceBetweenHits == Sizes.Small){
			spaceBetweenHits = 1.1f;
		}else if (hit.spaceBetweenHits == Sizes.Medium){
			spaceBetweenHits = 1.3f;
		}else if (hit.spaceBetweenHits == Sizes.High){
			spaceBetweenHits = 1.7f;
		}
		UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, hitEffects.freezingTime * spaceBetweenHits);

		if ((currentMove == null || !currentMove.hitAnimationOverride) && !armored) {
			// Stun
			// Hit stun deterioration (the longer the combo gets, the harder it is to combo)
			currentSubState = SubStates.Stunned;
			int stunFrames = 0;
			if (hit.hitStunType == HitStunType.FrameAdvantage) {
				stunFrames = hit.frameAdvantageOnHit + remainingFrames;
				if (stunFrames < 1) stunFrames = 1;
				if (stunFrames < UFE.config.comboOptions.minHitStun) stunTime = UFE.config.comboOptions.minHitStun;
				stunTime = (float)stunFrames/(float)UFE.config.fps;
			}else if (hit.hitStunType == HitStunType.Frames) {
				stunFrames = (int) hit.hitStunOnHit;
				if (stunFrames < 1) stunFrames = 1;
				if (stunFrames < UFE.config.comboOptions.minHitStun) stunTime = UFE.config.comboOptions.minHitStun;
				stunTime = (float)stunFrames/(float)UFE.config.fps;
			}else{
				stunTime = hit.hitStunOnHit;
			}

			if (!hit.resetPreviousHitStun){
				if (UFE.config.comboOptions.hitStunDeterioration == Sizes.Small){
					stunTime -= (float)comboHits * .01f;
				}else if (UFE.config.comboOptions.hitStunDeterioration == Sizes.Medium){
					stunTime -= (float)comboHits * .02f;
				}else if (UFE.config.comboOptions.hitStunDeterioration == Sizes.High){
					stunTime -= (float)comboHits * .04f;
				}
			}
			stunTime *= hitStunModifier;

			verticalPush = hit.pushForce.y;
			// Add force to the move		
			// Air juggle deterioration (the longer the combo, the harder it is to push the opponent higher)
			if (verticalPush > 0 || (isDead && !isKnockDown)){
				if (UFE.config.comboOptions.airJuggleDeteriorationType == AirJuggleDeteriorationType.ComboHits){
					airJuggleHits = comboHits - 1;
				}
				if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.None){
					verticalPush = hit.pushForce.y;
				}else if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.Small){
					verticalPush = hit.pushForce.y - (hit.pushForce.y * (float)airJuggleHits * .04f);
				}else if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.Medium){
					verticalPush = hit.pushForce.y - (hit.pushForce.y * (float)airJuggleHits * .1f);
				}else if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.High){
					verticalPush = hit.pushForce.y - (hit.pushForce.y * (float)airJuggleHits * .3f);
				}
				if (verticalPush < UFE.config.comboOptions.minPushForce) verticalPush = UFE.config.comboOptions.minPushForce;
				airJuggleHits ++;
			}

			if (UFE.config.comboOptions.fixJuggleWeight){
				myPhysicsScript.ApplyNewWeight(UFE.config.comboOptions.juggleWeight);
			}
			
			if (isDead) stunTime = 9999;

			if ((airHit || UFE.config.comboOptions.neverAirRecover) && verticalPush > 0){
				
				if (myMoveSetScript.basicMoves.getHitAir.clip1 == null)
					Debug.LogError("Get Hit Air animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Air");
				if (myMoveSetScript.basicMoves.getHitAir.invincible) myHitBoxesScript.HideHitBoxes(true);

				myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush);
				myPhysicsScript.AddForce(new Vector2(hit.pushForce.x, verticalPush), -opControlsScript.mirror);
                if (myMoveSetScript.basicMoves.getHitKnockBack.clip1 != null && 
                    hit.pushForce.x > UFE.config.comboOptions.knockBackMinForce) {
                    currentHitAnimation = myMoveSetScript.basicMoves.getHitKnockBack.name;
                    currentHitInfo = myMoveSetScript.basicMoves.getHitKnockBack;
				}else{
					currentHitAnimation = myMoveSetScript.basicMoves.getHitAir.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitAir;
				}

                myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, UFE.config.hitOptions.resetAnimationOnHit);

                if (currentHitInfo.autoSpeed) {
                    // if the hit was in the air, calculate the time it will take for the character to hit the ground
                    float airTime = myPhysicsScript.GetPossibleAirTime(verticalPush) * 2;
                    if (stunTime > airTime || UFE.config.comboOptions.neverAirRecover) stunTime = airTime;
                    myMoveSetScript.SetAnimationNormalizedSpeed(currentHitAnimation, (myMoveSetScript.GetAnimationLengh(currentHitAnimation) / stunTime));
                }

			}else{

                float hitAnimationSpeed = 0;
				float knockoutAnimationLenght = 0;

				if (hit.hitType == HitType.HighKnockdown){
					// if its a knockdown, animation will run at normal speed and character will stay knockdown for -stunTime- seconds.
					knockoutAnimationLenght = myMoveSetScript.GetAnimationLengh(currentHitAnimation)/myMoveSetScript.basicMoves.getHitHighKnockdown.animationSpeed;
					stunTime = knockoutAnimationLenght + AdjustKnockdownVariables(UFE.config.knockDownOptions.high);
				}else if (hit.hitType == HitType.MidKnockdown){
					// if its a knockdown, animation will run at normal speed and character will stay knockdown for -stunTime- seconds.
					knockoutAnimationLenght = myMoveSetScript.GetAnimationLengh(currentHitAnimation)/myMoveSetScript.basicMoves.getHitHighLowKnockdown.animationSpeed;
					stunTime = knockoutAnimationLenght + AdjustKnockdownVariables(UFE.config.knockDownOptions.highLow);
				}else if (hit.hitType == HitType.Sweep){
					// if its a knockdown, animation will run at normal speed and character will stay knockdown for -stunTime- seconds.
					knockoutAnimationLenght = myMoveSetScript.GetAnimationLengh(currentHitAnimation)/myMoveSetScript.basicMoves.getHitSweep.animationSpeed;
					stunTime = knockoutAnimationLenght + AdjustKnockdownVariables(UFE.config.knockDownOptions.sweep);
				}else if (hit.hitStrength == HitStrengh.Crumple){
					stunTime += UFE.config.knockDownOptions.crumple.standUpTime;
				}else {
					hitAnimationSpeed = myMoveSetScript.GetAnimationLengh(currentHitAnimation)/stunTime;

					myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush);
					myPhysicsScript.AddForce(new Vector2(hit.pushForce.x, verticalPush), -opControlsScript.mirror);

                    // Set deceleration of hit stun animation so it can look more natural
                    if (UFE.config.hitOptions.useHitStunDeceleration) {
                        hitStunDeceleration = hitAnimationSpeed + (stunTime / 2);
                    }
                }

                myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, UFE.config.hitOptions.resetAnimationOnHit);
                if (currentHitInfo.autoSpeed && hitAnimationSpeed > 0) {
                    myMoveSetScript.SetAnimationSpeed(currentHitAnimation, hitAnimationSpeed);
                }

			}
		}
		
		// Freeze screen depending on how strong the hit was
		HitPause(GetHitAnimationSpeed(hit.hitStrength) * .01f);
		UFE.DelaySynchronizedAction(this.HitUnpause, hitEffects.freezingTime);
	}

	private float AdjustKnockdownVariables(SubKnockdownOptions knockdownOptions){
		myPhysicsScript.ResetForces(true, true);
		myPhysicsScript.AddForce(knockdownOptions.predefinedPushForce, -opControlsScript.mirror);
		currentState = PossibleStates.Down;
		return knockdownOptions.standUpTime + knockdownOptions.knockedOutTime;
	}

	private string GetHitAnimation(BasicMoveInfo hitMove, Hit hit){
		if (hit.hitStrength == HitStrengh.Weak) return hitMove.name;
		if (hitMove.clip2 != null && hit.hitStrength == HitStrengh.Medium) return myMoveSetScript.GetAnimationString(hitMove, 2);
		if (hitMove.clip3 != null && hit.hitStrength == HitStrengh.Heavy) return myMoveSetScript.GetAnimationString(hitMove, 3);
		if (hitMove.clip4 != null && hit.hitStrength == HitStrengh.Custom1) return myMoveSetScript.GetAnimationString(hitMove, 4);
		if (hitMove.clip5 != null && hit.hitStrength == HitStrengh.Custom2) return myMoveSetScript.GetAnimationString(hitMove, 5);
		if (hitMove.clip6 != null && hit.hitStrength == HitStrengh.Custom3) return myMoveSetScript.GetAnimationString(hitMove, 6);
		return hitMove.name;
	}

	public void ToggleHeadLook(bool flag){
		if (headLookScript != null && myInfo.headLook.enabled) headLookScript.enabled = flag;
	}

	// Pause animations and physics to create a sense of impact
	void HitPause(){
		HitPause(0);
	}

	void HitPause(float animSpeed){
		if (shakeCamera) Camera.main.transform.position += Vector3.forward/2;
		myPhysicsScript.freeze = true;
		
		PausePlayAnimation(true, animSpeed);
	}
	
	// Unpauses the pause
	void HitUnpause(){
		if (cameraScript.cinematicFreeze) return;
		PausePlayAnimation(false);
		myPhysicsScript.freeze = false;
	}

	
	void ResetTimeScale(){
		Time.timeScale = UFE.config.gameSpeed;
	}

	// Method to pause animations and return them to their prior speed accordly
	
	private void PausePlayAnimation(bool pause){
		PausePlayAnimation(pause, 0);
	}

	private void PausePlayAnimation(bool pause, float animSpeed){
		if (animSpeed < 0) animSpeed = 0;
		if (pause){
			myMoveSetScript.SetAnimationSpeed(animSpeed);
		}else {
			myMoveSetScript.RestoreAnimationSpeed();
		}
	}

	public void AddGauge(float gaugeGain){
		if (!UFE.config.gameGUI.hasGauge) return;
		myInfo.currentGaugePoints += gaugeGain;
		if (myInfo.currentGaugePoints > myInfo.maxGaugePoints) myInfo.currentGaugePoints = myInfo.maxGaugePoints;
	}
	
	private void RemoveGauge(float gaugeLoss){
		if (!UFE.config.gameGUI.hasGauge) return;
		if (UFE.gameMode == GameMode.TrainingRoom && playerNum == 1 && UFE.config.trainingModeOptions.p1Gauge == LifeBarTrainingMode.Infinite) return;
        if (UFE.gameMode == GameMode.TrainingRoom && playerNum == 2 && UFE.config.trainingModeOptions.p2Gauge == LifeBarTrainingMode.Infinite) return;
		myInfo.currentGaugePoints -= gaugeLoss;
		if (myInfo.currentGaugePoints < 0) myInfo.currentGaugePoints = 0;

        if ((playerNum == 1 && UFE.gameMode == GameMode.TrainingRoom && UFE.config.trainingModeOptions.p1Gauge == LifeBarTrainingMode.Refill) ||
            (playerNum == 2 && UFE.gameMode == GameMode.TrainingRoom && UFE.config.trainingModeOptions.p2Gauge == LifeBarTrainingMode.Refill)) {
                if (!UFE.FindAndUpdateDelaySynchronizedAction(this.RefillGauge, UFE.config.trainingModeOptions.refillTime)) 
				UFE.DelaySynchronizedAction(this.RefillGauge, UFE.config.trainingModeOptions.refillTime);
		}
	}
	
	private bool DamageMe(float damage, bool doesntKill){
		if (doesntKill && damage >= myInfo.currentLifePoints) damage = myInfo.currentLifePoints - 1;
		return DamageMe(damage);
	}
	
	private void RefillLife(){
		myInfo.currentLifePoints = myInfo.lifePoints;
		UFE.SetLifePoints(myInfo.lifePoints, myInfo);
	}
	
	private void RefillGauge(){
		AddGauge(myInfo.maxGaugePoints);
	}

	private bool DamageMe(float damage){
        if (UFE.gameMode == GameMode.TrainingRoom && playerNum == 1 && UFE.config.trainingModeOptions.p1Life == LifeBarTrainingMode.Infinite) return false;
        if (UFE.gameMode == GameMode.TrainingRoom && playerNum == 2 && UFE.config.trainingModeOptions.p2Life == LifeBarTrainingMode.Infinite) return false;
		if (myInfo.currentLifePoints <= 0 || opInfo.currentLifePoints <= 0) return true;
		if (UFE.GetTimer() <= 0 && UFE.config.roundOptions.hasTimer) return true;

		myInfo.currentLifePoints -= damage;
		if (myInfo.currentLifePoints < 0) myInfo.currentLifePoints = 0;
		UFE.SetLifePoints(myInfo.currentLifePoints, myInfo);

        if ((playerNum == 1 && UFE.gameMode == GameMode.TrainingRoom && UFE.config.trainingModeOptions.p1Life == LifeBarTrainingMode.Refill) ||
            (playerNum == 2 && UFE.gameMode == GameMode.TrainingRoom && UFE.config.trainingModeOptions.p2Life == LifeBarTrainingMode.Refill)) {
                if (myInfo.currentLifePoints == 0) myInfo.currentLifePoints = myInfo.lifePoints;
                if (!UFE.FindAndUpdateDelaySynchronizedAction(this.RefillLife, UFE.config.trainingModeOptions.refillTime)) {
                    UFE.DelaySynchronizedAction(this.RefillLife, UFE.config.trainingModeOptions.refillTime);
                }
		}

        if (UFE.gameMode == GameMode.TrainingRoom && playerNum == 1 && UFE.config.trainingModeOptions.p1Life != LifeBarTrainingMode.Normal) return false;
        if (UFE.gameMode == GameMode.TrainingRoom && playerNum == 2 && UFE.config.trainingModeOptions.p2Life != LifeBarTrainingMode.Normal) return false;

		if (myInfo.currentLifePoints == 0){
			UFE.PlaySound(myInfo.deathSound);
			UFE.PauseTimer();

			if (UFE.config.roundOptions.slowMotionKO){
				Time.timeScale = Time.timeScale * .2f;
				UFE.DelaySynchronizedAction(this.ReturnTimeScale, 0.4f); // Low timer to account for the slowmotion
			}else{
				UFE.DelaySynchronizedAction(this.EndRound, 3f);
			}

			return true;
		}
		return false;
	}
	
	private void ReturnTimeScale(){
		Time.timeScale = UFE.config.gameSpeed;
		UFE.DelaySynchronizedAction(this.EndRound, 1f);
	}

	public void EndRound(){
		UFE.config.lockMovements = true;
		UFE.config.lockInputs = true;

		// Make sure both characters are grounded
		if (!opPhysicsScript.IsGrounded() 
		    || !myPhysicsScript.IsGrounded()){
			UFE.DelaySynchronizedAction(this.EndRound, 1f);
			return;
		}
		
		// Reset Stats
		KillCurrentMove();
		opControlsScript.KillCurrentMove();


		// Clear All Projectiles
		foreach(ProjectileMoveScript projectileMoveScript in projectiles){
			if (projectileMoveScript != null) projectileMoveScript.destroyMe = true; 
		}
		foreach(ProjectileMoveScript projectileMoveScript in opControlsScript.projectiles){
			if (projectileMoveScript != null) projectileMoveScript.destroyMe = true; 
		}


		// Fire Round Event
		++opControlsScript.roundsWon;
		UFE.FireRoundEnds(opInfo, myInfo);

		// Start New Round or End Game
		if (opControlsScript.roundsWon > Mathf.Ceil(UFE.config.roundOptions.totalRounds/2)){
			opControlsScript.SetMoveToOutro();
			UFE.DelaySynchronizedAction(this.EndGame, UFE.config.roundOptions.endGameDelay);
		}else{
			UFE.DelaySynchronizedAction(this.NewRound, UFE.config.roundOptions.newRoundDelay);
		}
	}

	public void NewRound(){
		potentialBlock = false;
		opControlsScript.potentialBlock = false;
		if (UFE.config.roundOptions.resetPositions) {
			CameraFade.StartAlphaFade(Color.black, false, 1, 2, () => { StartNewRound(); });
		}else{
			UFE.DelaySynchronizedAction(this.StartNewRound, 2f);
		}
	}

	public void SetMoveToOutro(){
		currentMove = myMoveSetScript.GetOutro();
		if (currentMove != null) {
			currentMove.currentFrame = 0;
			currentMove.currentTick = 0;
		}
		outroPlayed = true;
	}

	private void EndGame(){
		UFE.FireGameEnds(opInfo, myInfo);
		cameraScript.killCamMove = true;
	}

	public void ResetData(bool resetLife){
		if (UFE.config.roundOptions.resetPositions){
			if (gameObject.name == "Player1"){
				transform.position = new Vector2(UFE.config.roundOptions.p1XPosition, .009f);
			}else{
				transform.position = new Vector2(UFE.config.roundOptions.p2XPosition, .009f);
			}
			myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle, myMoveSetScript.basicMoves.idle.name, 0);
			myPhysicsScript.ForceGrounded();

		}else if (currentState == PossibleStates.Down && myPhysicsScript.IsGrounded()){
			myMoveSetScript.PlayAnimation("standUp", 0);
		}

		if (resetLife || UFE.config.roundOptions.resetLifePoints){
			myInfo.currentLifePoints = (float)myInfo.lifePoints;
		}
		blockStunned = false;
		stunTime = 0;
		comboHits = 0;
		comboDamage = 0;
		comboHitDamage = 0;
		airJuggleHits = 0;
		CheckBlocking(false);
		isDead = false;
		myPhysicsScript.isTakingOff = false;
		myPhysicsScript.isLanding = false;
		
		myHitBoxesScript.HideHitBoxes(false);
		myPhysicsScript.ResetWeight();
		ToggleHeadLook(true);

		currentState = PossibleStates.Stand;
		currentSubState = SubStates.Resting;
	}

	private void StartNewRound(){
		UFE.config.currentRound ++;
		UFE.ResetTimer();

		ResetData(true);
		opControlsScript.ResetData(false);
		if (UFE.config.roundOptions.resetPositions) {
			CameraFade.StartAlphaFade(Color.black, true, 2);
			cameraScript.ResetCam();
		}

		UFE.config.lockInputs = true;
		UFE.ResetRoundCast();
		UFE.CastNewRound();

		if (UFE.config.roundOptions.allowMovement) {
			UFE.config.lockMovements = false;
		}else{
			UFE.config.lockMovements = true;
		}
	}

	// Get amount of freezing time depending on the Strengtht of the move
	public float GetHitAnimationSpeed(HitStrengh hitStrength){
		if (hitStrength == HitStrengh.Weak){
			return UFE.config.hitOptions.weakHit.animationSpeed;
		} else if (hitStrength == HitStrengh.Medium){
			return UFE.config.hitOptions.mediumHit.animationSpeed;
		}else if (hitStrength == HitStrengh.Heavy){
			return UFE.config.hitOptions.heavyHit.animationSpeed;
		}else if (hitStrength == HitStrengh.Crumple){
			return UFE.config.hitOptions.crumpleHit.animationSpeed;
		}
		return 0;
	}

	// Get amount of freezing time depending on the Strengtht of the move
	public float GetHitFreezingTime(HitStrengh hitStrength){
		if (hitStrength == HitStrengh.Weak){
			return UFE.config.hitOptions.weakHit.freezingTime;
		} else if (hitStrength == HitStrengh.Medium){
			return UFE.config.hitOptions.mediumHit.freezingTime;
		}else if (hitStrength == HitStrengh.Heavy){
			return UFE.config.hitOptions.heavyHit.freezingTime;
		}else if (hitStrength == HitStrengh.Crumple){
			return UFE.config.hitOptions.crumpleHit.freezingTime;
		}
		return 0;
	}
	
	// Shake character while being hit and in freezing mode
	
	void shakeCam(){
		float rnd = Random.Range(-.2f * shakeDensity,.2f * shakeDensity);
		Camera.main.transform.position += new Vector3(rnd, rnd, 0);
	}

	void shake(){
		float rnd = Random.Range(-.1f * shakeDensity,.2f * shakeDensity);
		character.transform.localPosition = new Vector3(rnd, 0, 0);
	}
}
