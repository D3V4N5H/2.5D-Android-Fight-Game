using UnityEngine;
using System.Collections;

public class PhysicsScript : MonoBehaviour {
	[HideInInspector]	public bool freeze;
	[HideInInspector]	public float airTime = 0;
	[HideInInspector]	public bool isBouncing;
	[HideInInspector]	public bool isTakingOff;
	[HideInInspector]	public bool isLanding;
	[HideInInspector]	public int currentAirJumps;

	private float moveDirection = 0;
	private float verticalForce = 0;
	private float horizontalForce = 0;
	private float verticalTotalForce = 0;
	private int groundLayer;
	private int groundMask;
	private int bounceTimes;
	private float appliedGravity;
	
	private ControlsScript myControlsScript;
	private HitBoxesScript myHitBoxesScript;
	private MoveSetScript myMoveSetScript;
	private GameObject character;

	public void Start(){
		/*Plane groundPlane = (Plane) GameObject.FindObjectOfType(typeof(Plane));
		if (groundPlane == null) Debug.LogError("Plane not found. Please add a plane mesh to your stage prefab!");*/

		groundLayer = LayerMask.NameToLayer("Ground");
  		groundMask = 1 << groundLayer;
		myControlsScript = GetComponent<ControlsScript>();
		character = myControlsScript.character;
		myHitBoxesScript = character.GetComponent<HitBoxesScript>();
		myMoveSetScript = character.GetComponent<MoveSetScript>();
		appliedGravity = myControlsScript.myInfo.physics.weight * UFE.config.gravity;
	}
	
	public void Move(int mirror, float direction){
		if (!IsGrounded()) return;
		if (freeze) return;
		if (isTakingOff) return;
		if (isLanding) return;
		moveDirection = direction;
		if (mirror == 1){
			myControlsScript.currentSubState = SubStates.MovingForward;
			myControlsScript.horizontalForce = horizontalForce = myControlsScript.myInfo.physics.moveForwardSpeed * direction;
		}else{
			myControlsScript.currentSubState = SubStates.MovingBack;
			myControlsScript.horizontalForce = horizontalForce = myControlsScript.myInfo.physics.moveBackSpeed * direction;
		}
	}
	
	public void Jump(){
		if (isTakingOff && currentAirJumps > 0) return;
		if (myControlsScript.currentMove != null) return;

		isTakingOff = false;
		isLanding = false;
		myControlsScript.storedMove = null;
		myControlsScript.potentialBlock = false;

		if (myControlsScript.currentState == PossibleStates.Down) return;
		if (myControlsScript.currentSubState == SubStates.Stunned || myControlsScript.currentSubState == SubStates.Blocking) return;
		if (currentAirJumps >= myControlsScript.myInfo.physics.multiJumps) return;
		currentAirJumps ++;
		horizontalForce = myControlsScript.myInfo.physics.jumpDistance * moveDirection;
		verticalForce = myControlsScript.myInfo.physics.jumpForce;
		setVerticalData(myControlsScript.myInfo.physics.jumpForce);
		ApplyForces(myControlsScript.currentMove);
	}

	public bool IsJumping(){
		return (currentAirJumps > 0);
	}
	
	public bool IsMoving(){
		return (moveDirection != 0);
	}

    public void ResetLanding() {
        isLanding = false;
    }

	public void ResetForces(bool resetX, bool resetY){
		if (resetX) horizontalForce = 0;
		if (resetY) verticalForce = 0;
	}
	
	public void AddForce(Vector2 push, int mirror){
		push.x *= mirror;
		isBouncing = false;
		if (!myControlsScript.myInfo.physics.cumulativeForce){
			horizontalForce = 0;
			verticalForce = 0;
		}
		if (verticalForce < 0 && push.y > 0 && UFE.config.comboOptions.resetFallingForceOnHit) verticalForce = 0;
		horizontalForce += push.x;
		verticalForce += push.y;
		setVerticalData(verticalForce);
	}
	
	void setVerticalData(float appliedForce){
		float maxHeight = Mathf.Pow(appliedForce,2) / (appliedGravity * 2);
		maxHeight += transform.position.y;
		airTime = Mathf.Sqrt(maxHeight * 2 / appliedGravity);
		verticalTotalForce = appliedGravity * airTime;
	}

	public void ApplyNewWeight(float newWeight){
		appliedGravity = newWeight * UFE.config.gravity;
	}

	public void ResetWeight(){
		appliedGravity = myControlsScript.myInfo.physics.weight * UFE.config.gravity;
	}
	
	public float GetPossibleAirTime(float appliedForce){
		float maxHeight = Mathf.Pow(appliedForce,2) / (appliedGravity * 2);
		maxHeight += transform.position.y;
		return Mathf.Sqrt(maxHeight * 2 / appliedGravity);
	}

	public void ForceGrounded() {
		verticalForce = 0;
		horizontalForce = 0;
		setVerticalData(0);
		currentAirJumps = 0;
		isTakingOff = false;
		isLanding = false;
		if (transform.position.y != 0) transform.Translate(new Vector3(0, -transform.position.y, 0));
		myControlsScript.currentState = PossibleStates.Stand;
	}
	
	public void ApplyForces() {
		ApplyForces(null);
	}

	public void ApplyForces(MoveInfo move) {
		//if (myControlsScript.debugger != null) myControlsScript.debugger.text = "";
		//if (myControlsScript.debugger != null) myControlsScript.debugger.text = "IsGrounded = " + IsGrounded() + "\n";
		//if (myControlsScript.debugger != null) myControlsScript.debugger.text += "verticalForce = " + verticalForce + "\n";
		//if (myControlsScript.debugger != null) myControlsScript.debugger.text += "verticalTotalForce = " + verticalTotalForce + "\n";
		//if (myControlsScript.debugger != null) myControlsScript.debugger.text += "normalizedJumpArc = " + myControlsScript.normalizedJumpArc + "\n";
		//if (myControlsScript.debugger != null) myControlsScript.debugger.text += "isTakingOff = " + isTakingOff + "\n";
		//if (myControlsScript.debugger != null) myControlsScript.debugger.text += "myControlsScript.currentState = " + myControlsScript.currentState + "\n";

		if (freeze) return;

		myControlsScript.normalizedJumpArc = 1 - ((verticalForce + verticalTotalForce)/(verticalTotalForce * 2));

		float appliedFriction = (moveDirection != 0 || myControlsScript.myInfo.physics.highMovingFriction) ? 
			UFE.config.selectedStage.groundFriction : myControlsScript.myInfo.physics.friction;


		if (move != null && move.ignoreFriction) appliedFriction = 0;

		if (myControlsScript.activePullIn != null){
			transform.position = Vector3.Lerp(transform.position, 
			                                  myControlsScript.activePullIn.position, 
			                                  Time.fixedDeltaTime * myControlsScript.activePullIn.speed);

			if (myControlsScript.activePullIn.forceStand && !IsGrounded()) ForceGrounded();

			if (Vector3.Distance(myControlsScript.activePullIn.position, transform.position) <= myControlsScript.activePullIn.targetDistance || 
			    myControlsScript.currentSubState != SubStates.Stunned) {
				myControlsScript.activePullIn = null;
			}

			//if (transform.position.z != 0) transform.Translate(new Vector3(0, 0, -transform.position.z));

		}else{
			if (!IsGrounded()) {
				appliedFriction = 0;
				if (verticalForce == 0) verticalForce = -.1f;
			}

			if (horizontalForce != 0 && !isTakingOff) {
				if (horizontalForce > 0) {
					horizontalForce -= appliedFriction * Time.fixedDeltaTime;
					horizontalForce = Mathf.Max(0, horizontalForce);
				}else if (horizontalForce < 0) {
					horizontalForce += appliedFriction * Time.fixedDeltaTime;
					horizontalForce = Mathf.Min(0, horizontalForce);
				}


				transform.Translate(horizontalForce * Time.fixedDeltaTime, 0, 0);
			}
			
			if (move == null || (move != null && !move.ignoreGravity)){
				if ((verticalForce < 0 && !IsGrounded()) || verticalForce > 0) {
					verticalForce -= appliedGravity* Time.fixedDeltaTime;
					transform.Translate(moveDirection * myControlsScript.myInfo.physics.jumpDistance * Time.fixedDeltaTime, verticalForce * Time.fixedDeltaTime, 0);
				} else if (verticalForce < 0 && IsGrounded()) {
					currentAirJumps = 0;
					verticalForce = 0;
				}
			}
		}

		/*if (myControlsScript.debugger != null) {
			myControlsScript.debugger.text = "isBouncing = " + isBouncing + "\n";
			myControlsScript.debugger.text += "controlsScript.stunTime = " + controlsScript.stunTime + "\n";
			myControlsScript.debugger.text += "Animations:\n";
			foreach(AnimationState animState in character.animation){
				if (myMoveSetScript.IsAnimationPlaying(animState.name)){
					myControlsScript.debugger.text += "<color=#003300>"+ animState.name +"</color>\n";
					myControlsScript.debugger.text += "<color=#003300>"+ animState.speed +"</color>\n";
				}
			}
		}*/
		
		/*if (UFE.normalizedCam) {
			Vector3 cameraLeftBounds = Camera.main.ViewportToWorldPoint(new Vector3(0,0,-Camera.main.transform.position.z - 10));
			Vector3 cameraRightBounds = Camera.main.ViewportToWorldPoint(new Vector3(1,0,-Camera.main.transform.position.z - 10));
			
			transform.position = new Vector3(
				Mathf.Clamp(transform.position.x,cameraLeftBounds.x,cameraRightBounds.x),
				transform.position.y,
				transform.position.z);
		}*/
		
		float minDist = myControlsScript.opponent.transform.position.x - UFE.config.cameraOptions.maxDistance;
		float maxDist = myControlsScript.opponent.transform.position.x + UFE.config.cameraOptions.maxDistance;
		transform.position = new Vector3(Mathf.Clamp(transform.position.x, minDist, maxDist), transform.position.y, transform.position.z);

		transform.position = new Vector3(
			Mathf.Clamp(transform.position.x,
		            UFE.config.selectedStage.leftBoundary,
		            UFE.config.selectedStage.rightBoundary),
			transform.position.y,
			transform.position.z);


		if (myControlsScript.currentState == PossibleStates.Down) return;

		if (IsGrounded() && myControlsScript.currentState != PossibleStates.Down){
			if (verticalTotalForce != 0) {
				if (bounceTimes < UFE.config.bounceOptions.maximumBounces && myControlsScript.currentSubState == SubStates.Stunned &&
					UFE.config.bounceOptions.bounceForce != Sizes.None &&
					verticalForce <= -UFE.config.bounceOptions.minimumBounceForce) {
					if (!UFE.config.bounceOptions.bounceHitBoxes) myHitBoxesScript.HideHitBoxes(true);
					if (UFE.config.bounceOptions.bounceForce == Sizes.Small){ 
						AddForce(new Vector2(0,-verticalForce/2.4f), 1);
					}else if (UFE.config.bounceOptions.bounceForce == Sizes.Medium){ 
						AddForce(new Vector2(0,-verticalForce/1.8f), 1);
					}else if (UFE.config.bounceOptions.bounceForce == Sizes.High){ 
						AddForce(new Vector2(0,-verticalForce/1.2f), 1);
					}
					bounceTimes ++;
					if (!isBouncing){
						if (myMoveSetScript.basicMoves.bounce.clip1 == null)
							Debug.LogError("Bounce animation not found! Make sure you have it set on Character -> Basic Moves -> Bounce");

						myControlsScript.stunTime += airTime + UFE.config.knockDownOptions.air.knockedOutTime;
						myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.bounce);
						if (UFE.config.bounceOptions.bouncePrefab != null) {
							GameObject pTemp = (GameObject) Instantiate(UFE.config.bounceOptions.bouncePrefab);
							pTemp.transform.parent = transform;
							pTemp.transform.localPosition = Vector3.zero;
							Destroy(pTemp, UFE.config.bounceOptions.bounceKillTime);
						}
						if (UFE.config.bounceOptions.shakeCamOnBounce){
							myControlsScript.shakeDensity = UFE.config.bounceOptions.shakeDensity;
						}
						UFE.PlaySound(UFE.config.bounceOptions.bounceSound);
						isBouncing = true;
					}
					return;
				}
				verticalTotalForce = 0;
				airTime = 0;
				myMoveSetScript.totalAirMoves = 0;
				BasicMoveInfo airAnimation = null;
				isBouncing = false;
				bounceTimes = 0;
                float animationSpeed = 0;
                float delayTime = 0;
				if (myControlsScript.currentMove != null && myControlsScript.currentMove.hitAnimationOverride) return;
				if (myControlsScript.currentSubState == SubStates.Stunned){
					myControlsScript.stunTime = UFE.config.knockDownOptions.air.knockedOutTime + UFE.config.knockDownOptions.air.standUpTime;

					if (myMoveSetScript.basicMoves.fallDown.clip1 == null)
						Debug.LogError("Fall Down From Air Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Fall Down From Air Hit");

					airAnimation = myMoveSetScript.basicMoves.fallDown;
					myControlsScript.currentState = PossibleStates.Down;
					if (!UFE.config.knockDownOptions.air.standUpHitBoxes) myHitBoxesScript.HideHitBoxes(true);

				}else if (myControlsScript.currentState != PossibleStates.Stand){
                    if (myMoveSetScript.basicMoves.landing.clip1 != null
                        && ((myControlsScript.currentMove != null && myControlsScript.currentMove.cancelMoveWheLanding) ||
					    myControlsScript.currentMove == null)){

						airAnimation = myMoveSetScript.basicMoves.landing;
						moveDirection = 0;
                        isLanding = true;
						myControlsScript.KillCurrentMove();

                        delayTime = (float)myControlsScript.myInfo.physics.landingDelay / UFE.config.fps;
                        UFE.DelaySynchronizedAction(ResetLanding, delayTime);

                        if (airAnimation.autoSpeed) {
                            animationSpeed = myMoveSetScript.GetAnimationLengh(airAnimation.name) / delayTime;
                        }
					}

					if (myControlsScript.currentState != PossibleStates.Crouch) myControlsScript.currentState = PossibleStates.Stand;

				}
				if (airAnimation != null && !myMoveSetScript.IsAnimationPlaying(airAnimation.name)){
					myMoveSetScript.PlayBasicMove(airAnimation);
                    if (animationSpeed != 0) {
                        myMoveSetScript.SetAnimationSpeed(airAnimation.name, animationSpeed);
                    }
				}
			}
			
			if (myControlsScript.currentSubState != SubStates.Stunned && !myControlsScript.isBlocking && !myControlsScript.blockStunned && 
			    move == null && !isTakingOff && !isLanding && myControlsScript.currentState == PossibleStates.Stand){
				if (moveDirection > 0 && myControlsScript.mirror == -1 ||
				    moveDirection < 0 && myControlsScript.mirror == 1) {
					if (myMoveSetScript.basicMoves.moveForward.clip1 == null)
						Debug.LogError("Move Forward animation not found! Make sure you have it set on Character -> Basic Moves -> Move Forward");
					if (!myMoveSetScript.IsAnimationPlaying(myMoveSetScript.basicMoves.moveForward.name)) {
					    myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.moveForward);
					}

				}else if (moveDirection > 0 && myControlsScript.mirror == 1||
				    moveDirection < 0 && myControlsScript.mirror == -1) {
					if (myMoveSetScript.basicMoves.moveBack.clip1 == null)
						Debug.LogError("Move Back animation not found! Make sure you have it set on Character -> Basic Moves -> Move Back");
					if (!myMoveSetScript.IsAnimationPlaying(myMoveSetScript.basicMoves.moveBack.name)) {
						myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.moveBack);
					}
				}
			}
		}else if ((verticalForce > 0 || !IsGrounded())){
			if (move != null && myControlsScript.currentState == PossibleStates.Stand)
				myControlsScript.currentState = PossibleStates.StraightJump;
			if (move == null && verticalForce/verticalTotalForce > 0 && verticalForce/verticalTotalForce <= 1) {
				if (isBouncing) return;

				if (moveDirection == 0) {
					myControlsScript.currentState = PossibleStates.StraightJump;
				}else{
					if (moveDirection > 0 && myControlsScript.mirror == -1 ||
					    moveDirection < 0 && myControlsScript.mirror == 1) {
						myControlsScript.currentState = PossibleStates.ForwardJump;
					}

					if (moveDirection > 0 && myControlsScript.mirror == 1||
					    moveDirection < 0 && myControlsScript.mirror == -1) {
						myControlsScript.currentState = PossibleStates.BackJump;
					}
				}

				BasicMoveInfo airAnimation;
				if (myControlsScript.currentSubState == SubStates.Stunned){
					if (myMoveSetScript.basicMoves.getHitKnockBack.clip1 != null && 
					    Mathf.Abs(horizontalForce) > UFE.config.comboOptions.knockBackMinForce && 
					    UFE.config.comboOptions.knockBackMinForce > 0){
						airAnimation = myMoveSetScript.basicMoves.getHitKnockBack;
					}else{
						if (myMoveSetScript.basicMoves.getHitAir.clip1 == null)
							Debug.LogError("Get Hit Air animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Air");

						airAnimation = myMoveSetScript.basicMoves.getHitAir;
					}
				}else{
					if (myMoveSetScript.basicMoves.jumpForward.clip1 != null && myControlsScript.currentState == PossibleStates.ForwardJump) {
						airAnimation = myMoveSetScript.basicMoves.jumpForward;
					}else if (myMoveSetScript.basicMoves.jumpBack.clip1 != null && myControlsScript.currentState == PossibleStates.BackJump) {
						airAnimation = myMoveSetScript.basicMoves.jumpBack;
					}else{
						if (myMoveSetScript.basicMoves.jumpStraight.clip1 == null)
							Debug.LogError("Jump animation not found! Make sure you have it set on Character -> Basic Moves -> Jump Straight");

						airAnimation = myMoveSetScript.basicMoves.jumpStraight;
					}
				}

                if (!myMoveSetScript.IsAnimationPlaying(airAnimation.name)) {
                    myMoveSetScript.PlayBasicMove(airAnimation);
                    if (airAnimation.autoSpeed) {
                        myMoveSetScript.SetAnimationNormalizedSpeed(airAnimation.name, (myMoveSetScript.GetAnimationLengh(airAnimation.name) / (airTime * 2)));
                    }
                    //if (airAnimation.autoSpeed || myControlsScript.currentSubState == SubStates.Stunned) {
                    //myMoveSetScript.SetAnimationNormalizedSpeed(airAnimation.name, (myMoveSetScript.GetAnimationLengh(airAnimation.name) / airTime));
				}
				
			}else if (move == null && verticalForce/verticalTotalForce <= 0) {
				BasicMoveInfo airAnimation;
				if (isBouncing){
					if (myMoveSetScript.basicMoves.fallingFromBounce.clip1 == null)
						Debug.LogError("Falling From Bounce animation not found! Make sure you have it set on Character -> Basic Moves -> Falling From Bounce");

					airAnimation = myMoveSetScript.basicMoves.fallingFromBounce;
					if (myMoveSetScript.basicMoves.fallingFromBounce.invincible) myHitBoxesScript.HideHitBoxes(true);
				}else {
					if (myControlsScript.currentSubState == SubStates.Stunned){
						if (myMoveSetScript.basicMoves.getHitKnockBack.clip1 != null && 
						    Mathf.Abs(horizontalForce) > UFE.config.comboOptions.knockBackMinForce && 
						    UFE.config.comboOptions.knockBackMinForce > 0){
							airAnimation = myMoveSetScript.basicMoves.getHitKnockBack;
						}else{
							if (myMoveSetScript.basicMoves.getHitAir.clip1 == null)
								Debug.LogError("Get Hit Air animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Air");

							airAnimation = myMoveSetScript.basicMoves.getHitAir;
						}
					}else{
						if (myMoveSetScript.basicMoves.fallForward.clip1 != null && myControlsScript.currentState == PossibleStates.ForwardJump) {
							airAnimation = myMoveSetScript.basicMoves.fallForward;
						}else if (myMoveSetScript.basicMoves.fallBack.clip1 != null && myControlsScript.currentState == PossibleStates.BackJump) {
							airAnimation = myMoveSetScript.basicMoves.fallBack;
						}else{
							if (myMoveSetScript.basicMoves.fallStraight.clip1 == null)
								Debug.LogError("Fall animation not found! Make sure you have it set on Character -> Basic Moves -> Fall Straight");
							
							airAnimation = myMoveSetScript.basicMoves.fallStraight;
						}
					}
				}

				if (!myMoveSetScript.IsAnimationPlaying(airAnimation.name)){
                    myMoveSetScript.PlayBasicMove(airAnimation);

                    if (airAnimation.autoSpeed) {
                        myMoveSetScript.SetAnimationNormalizedSpeed(airAnimation.name, (myMoveSetScript.GetAnimationLengh(airAnimation.name) / (airTime * 2)));
                    }
				}
			}
		}
		if (horizontalForce == 0 && verticalForce == 0) moveDirection = 0;
	}

	public bool IsGrounded() {
		if (Physics.RaycastAll(transform.position + new Vector3(0, 2f, 0), Vector3.down, 2.02f, groundMask).Length > 0) {
			if (transform.position.y != 0) transform.Translate(new Vector3(0, -transform.position.y, 0));
			return true;
		}
		return false;
	}
}
