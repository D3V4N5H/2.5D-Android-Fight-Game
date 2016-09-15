using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HitBox: ICloneable {
	public BodyPart bodyPart;
	public HitBoxType type;
	public HitBoxShape shape;
	public Rect rect = new Rect(0, 0, 4, 4);
	public bool followXBounds;
	public bool followYBounds;
	public float radius = .5f;
	public Vector2 offSet;

	public CollisionType collisionType;
	public Transform position;

	public int state{get;set;}
	//public float storedRadius{get;set;}
	public Rect rendererBounds{get;set;}
	public bool hide{get;set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class HurtBox: ICloneable {
	public BodyPart bodyPart;
	public HitBoxShape shape;
	public Rect rect = new Rect(0, 0, 4, 4);
	public bool followXBounds;
	public bool followYBounds;
	public float radius = .5f;
	public Vector2 offSet;

	public Vector3 position{get;set;}
	public Rect rendererBounds{get;set;}
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

[System.Serializable]
public class BlockArea {
	public int activeFramesBegin;
	public int activeFramesEnds;

	public BodyPart bodyPart;
	public HitBoxShape shape;
	public Rect rect = new Rect(0, 0, 4, 4);
	public bool followXBounds;
	public bool followYBounds;
	public float radius = .5f;
	public Vector2 offSet;

	[HideInInspector] public Vector3 position;
}

[System.Serializable]
public class PullIn: ICloneable {
	public int speed = 50;
	public bool forceStand = true;
	public BodyPart characterBodyPart;
	public BodyPart enemyBodyPart;
	public float targetDistance = .5f;

	[HideInInspector] public Vector3 position;
	
	public object Clone() {
		return CloneObject.Clone(this);
	}
}

public class HitBoxesScript : MonoBehaviour {
	public HitBox[] hitBoxes;
	
	[HideInInspector] public bool isHit;
	[HideInInspector] public HurtBox[] activeHurtBoxes;
	[HideInInspector] public HitConfirmType hitConfirmType;
	[HideInInspector] public BlockArea blockableArea;
	[HideInInspector] public float collisionBoxSize;
	[HideInInspector] public ControlsScript controlsScript;
	[HideInInspector] public bool previewInvertRotation;
	[HideInInspector] public bool previewMirror;

	private MoveSetScript myMoveSetScript;
	private bool currentMirror;
	//private Rect characterBounds = new Rect(0,0,0,0);
	private Renderer characterRenderer;


	
	void Start(){
		controlsScript = transform.parent.gameObject.GetComponent<ControlsScript>();
		myMoveSetScript = GetComponent<MoveSetScript>();
		UpdateRenderer();
		
		foreach(MoveInfo move in myMoveSetScript.moves){
			foreach(InvincibleBodyParts invBodyPart in move.invincibleBodyParts){
				List<HitBox> invHitBoxes = new List<HitBox>();
				foreach(BodyPart bodyPart in invBodyPart.bodyParts){
					foreach(HitBox hitBox in hitBoxes){
						if (bodyPart == hitBox.bodyPart) {
							invHitBoxes.Add(hitBox);
							break;
						}
					}
				}
				invBodyPart.hitBoxes = invHitBoxes.ToArray();
			}
		}
	}
	
	public static Vector3 TestCollision(HitBox[] hitBoxes, HurtBox[] hurtBoxes, HitConfirmType hitConfirmType, int mirror) {
		foreach (HitBox hitBox in hitBoxes) {
			if (hitBox.hide) continue;
			if (hitBox.collisionType == CollisionType.noCollider) continue;
			if (hitConfirmType == HitConfirmType.Throw && hitBox.collisionType != CollisionType.throwCollider) continue;
			if (hitConfirmType == HitConfirmType.Hit && hitBox.collisionType == CollisionType.throwCollider) continue;

			hitBox.state = 0;
			foreach (HurtBox hurtBox in hurtBoxes) {
				Vector3 hurtBoxPosition = hurtBox.position;
				Vector3 hitBoxPosition = hitBox.position.position;
				float dist = 0;
				bool collisionConfirm = false;
				
				if (!UFE.config.detect3D_Hits){
					hurtBoxPosition.z = -1;
					hitBoxPosition.z = -1;
				}
				
				if (hurtBox.shape == HitBoxShape.circle){
					if (hitBox.shape == HitBoxShape.circle){
						dist = Vector3.Distance(hurtBoxPosition, hitBoxPosition);
						if (dist <= hurtBox.radius + hitBox.radius) collisionConfirm = true;
						
					}else if (hitBox.shape == HitBoxShape.rectangle){
						float mirrorDiff = mirror > 0 ? hitBox.rect.width : 0f;
						Rect hitBoxRectanglePosition = new Rect(((hitBox.rect.x + mirrorDiff) * mirror) + hitBoxPosition.x, 
						                                        hitBox.rect.y + hitBoxPosition.y, 
						                                        hitBox.rect.width, hitBox.rect.height);
						if (hitBox.followXBounds){
							hitBoxRectanglePosition.x = hitBox.rendererBounds.x - (hitBox.rect.width/2);
							hitBoxRectanglePosition.width = (hitBox.rendererBounds.width + hitBox.rect.width) - hitBox.rendererBounds.x;
						}
						if (hitBox.followYBounds){
							hitBoxRectanglePosition.y = hitBox.rendererBounds.y - (hitBox.rect.height/2);
							hitBoxRectanglePosition.height = (hitBox.rendererBounds.height + hitBox.rect.height) - hitBox.rendererBounds.y;
						}
						
						dist = distancePointToRectangle(hurtBoxPosition, hitBoxRectanglePosition);
						if (dist <= hurtBox.radius) collisionConfirm = true;
					}
				}else if (hurtBox.shape == HitBoxShape.rectangle){
					Rect hurtBoxRectanglePosition;
					if (hitBox.shape == HitBoxShape.circle){
						float mirrorDiff = mirror < 0 ? hurtBox.rect.width : 0f;
						hurtBoxRectanglePosition = new Rect(((hurtBox.rect.x + mirrorDiff) * mirror) + hurtBoxPosition.x, 
						                                    hurtBox.rect.y + hurtBoxPosition.y, 
						                                    hurtBox.rect.width, hurtBox.rect.height);
						if (hurtBox.followXBounds){
							hurtBoxRectanglePosition.x = hurtBox.rendererBounds.x - (hurtBox.rect.width/2);
							hurtBoxRectanglePosition.width = (hurtBox.rendererBounds.width + hurtBox.rect.width) - hurtBox.rendererBounds.x;
						}
						if (hurtBox.followYBounds){
							hurtBoxRectanglePosition.y = hurtBox.rendererBounds.y - (hurtBox.rect.height/2);
							hurtBoxRectanglePosition.height = (hurtBox.rendererBounds.height + hurtBox.rect.height) - hurtBox.rendererBounds.y;
						}

						dist = distancePointToRectangle(hitBoxPosition, hurtBoxRectanglePosition);
						if (dist <= hitBox.radius) collisionConfirm = true;
						
					}else if (hitBox.shape == HitBoxShape.rectangle){
						float mirrorDiff = mirror < 0 ? hurtBox.rect.width : 0f;
						hurtBoxRectanglePosition = new Rect(((hurtBox.rect.x + mirrorDiff) * mirror) + hurtBoxPosition.x, 
						                                    hurtBox.rect.y + hurtBoxPosition.y, 
						                                    hurtBox.rect.width, hurtBox.rect.height);
						
						mirrorDiff = mirror > 0 ? hitBox.rect.width : 0f;
						Rect hitBoxRectanglePosition = new Rect(((hitBox.rect.x + mirrorDiff) * - mirror) + hitBoxPosition.x, 
						                                   hitBox.rect.y + hitBoxPosition.y, 
						                                   hitBox.rect.width, hitBox.rect.height);
						if (hitBox.followXBounds){
							hitBoxRectanglePosition.x = hitBox.rendererBounds.x - (hitBox.rect.width/2);
							hitBoxRectanglePosition.width = (hitBox.rendererBounds.width + hitBox.rect.width) - hitBox.rendererBounds.x;
						}
						if (hitBox.followYBounds){
							hitBoxRectanglePosition.y = hitBox.rendererBounds.y - (hitBox.rect.height/2);
							hitBoxRectanglePosition.height = (hitBox.rendererBounds.height + hitBox.rect.height) - hitBox.rendererBounds.y;
						}

						if (hurtBox.followXBounds){
							hurtBoxRectanglePosition.x = hurtBox.rendererBounds.x - (hurtBox.rect.width/2);
							hurtBoxRectanglePosition.width = (hurtBox.rendererBounds.width + hurtBox.rect.width) - hurtBox.rendererBounds.x;
						}
						if (hurtBox.followYBounds){
							hurtBoxRectanglePosition.y = hurtBox.rendererBounds.y - (hurtBox.rect.height/2);
							hurtBoxRectanglePosition.height = (hurtBox.rendererBounds.height + hurtBox.rect.height) - hurtBox.rendererBounds.y;
						}
						
						if (hurtBoxRectanglePosition.Overlaps(hitBoxRectanglePosition)) collisionConfirm = true;
					}
				}

				if (collisionConfirm) {
					if (hitConfirmType == HitConfirmType.Hit) {
						hitBox.state = 1;
					}
					return (hurtBoxPosition + hitBoxPosition)/2;
				}
			}
		}

		foreach (HitBox hitBox in hitBoxes) {
			if (hitBox.state == 1) hitBox.state = 0;
		}
		return Vector3.zero;
	}

	public Vector3 TestCollision(HurtBox[] hurtBoxes, HitConfirmType hitConfirmType) {
		if (isHit && hitConfirmType == HitConfirmType.Hit) return Vector3.zero;
		foreach(HitBox hitbox in this.hitBoxes) if (hitbox.followXBounds || hitbox.followYBounds) hitbox.rendererBounds = GetBounds();
		/*Vector3 result = HitBoxesScript.TestCollision(this.hitBoxes, hurtBoxes, hitConfirmType, controlsScript.mirror);
		if (result == Vector3.zero) {
			foreach (HitBox hitBox in hitBoxes) {
				if (hitBox.state == 1) hitBox.state = 0;
			}
		}*/

		return HitBoxesScript.TestCollision(this.hitBoxes, hurtBoxes, hitConfirmType, controlsScript.mirror);
	}

	public Vector3 TestCollision(BlockArea blockableArea) {
		HurtBox hurtBox = new HurtBox();
		hurtBox.position = blockableArea.position;
		hurtBox.shape = blockableArea.shape;
		hurtBox.rect = blockableArea.rect;
		hurtBox.followXBounds = blockableArea.followXBounds;
		hurtBox.followYBounds = blockableArea.followYBounds;
		hurtBox.radius = blockableArea.radius;
		hurtBox.offSet = blockableArea.offSet;

		// We use throw confirmation type so the engine doesn't register the state of the stroke hitbox as hit
		return HitBoxesScript.TestCollision(this.hitBoxes, new HurtBox[]{hurtBox}, HitConfirmType.Hit, controlsScript.mirror);
	}
	
	public float TestCollision(HitBox[] opHitBoxes) {
		float totalPushForce = 0;
		foreach (HitBox hitBox in hitBoxes) {
			if (hitBox.collisionType != CollisionType.bodyCollider) continue;
			foreach (HitBox opHitBox in opHitBoxes) {
				if (opHitBox.collisionType != CollisionType.bodyCollider) continue;
				Vector3 opHitBoxPosition = opHitBox.position.position;
				Vector3 hitBoxPosition = hitBox.position.position;
				if (!UFE.config.detect3D_Hits){
					opHitBoxPosition = new Vector3(opHitBox.position.position.x, opHitBox.position.position.y, 0);
					hitBoxPosition = new Vector3(hitBox.position.position.x, hitBox.position.position.y, 0);
				}
				float dist = Vector3.Distance(opHitBoxPosition, hitBoxPosition);
				if (dist <= opHitBox.radius + hitBox.radius) totalPushForce += (opHitBox.radius + hitBox.radius) - dist;
			}
		}
		return totalPushForce;
	}
	
	public Vector3 GetPosition(BodyPart bodyPart){
		foreach(HitBox hitBox in hitBoxes){
			if (bodyPart == hitBox.bodyPart) return hitBox.position.position;
		}
		return Vector3.zero;
	}
	
	public Transform GetTransform(BodyPart bodyPart){
		foreach(HitBox hitBox in hitBoxes){
			if (bodyPart == hitBox.bodyPart) return hitBox.position;
		}
		return null;
	}

	public void SetTransform(BodyPart bodyPart, Transform position){
		foreach(HitBox hitBox in hitBoxes){
			if (bodyPart == hitBox.bodyPart) {
				hitBox.position = position;
				return;
			}
		}
	}

	public HitBox[] GetHitBoxes(BodyPart[] bodyParts){
		List<HitBox> hitBoxesList = new List<HitBox>();
		foreach(HitBox hitBox in hitBoxes){
			foreach(BodyPart bodyPart in bodyParts){
				if (bodyPart == hitBox.bodyPart) {
					hitBoxesList.Add(hitBox);
					break;
				}
			}
		}

		return hitBoxesList.ToArray();
	}
	
	public void ResetHit(){
		//if (!isHit) return;
		foreach (HitBox hitBox in hitBoxes) {
			if (hitBox.state == 1) hitBox.state = 0;
		}
		isHit = false;
	}

	public HitBox GetStrokeHitBox(){
		if (!isHit) return null;
		foreach (HitBox hitBox in hitBoxes) {
			if (hitBox.state == 1) return hitBox;
		}
		return null;
	}
	
	public void HideHitBoxes(HitBox[] invincibleHitBoxes, bool hide){
		foreach (HitBox hitBox in invincibleHitBoxes) {
			hitBox.hide = hide;
		}
	}
	
	public void HideHitBoxes(bool hide){
		foreach (HitBox hitBox in hitBoxes) {
			hitBox.hide = hide;
		}
	}

	public void InvertHitBoxes(bool mirror){
		if (currentMirror == mirror) return;
		currentMirror = mirror;

		foreach (HitBox hitBox in hitBoxes) {
			foreach (HitBox hitBox2 in hitBoxes) {
				if ((hitBox.bodyPart == BodyPart.leftCalf && hitBox2.bodyPart == BodyPart.rightCalf) ||
				    (hitBox.bodyPart == BodyPart.leftFoot && hitBox2.bodyPart == BodyPart.rightFoot) ||
				    (hitBox.bodyPart == BodyPart.leftForearm && hitBox2.bodyPart == BodyPart.rightForearm) ||
				    (hitBox.bodyPart == BodyPart.leftHand && hitBox2.bodyPart == BodyPart.rightHand) ||
				    (hitBox.bodyPart == BodyPart.leftThigh && hitBox2.bodyPart == BodyPart.rightThigh) ||
				    (hitBox.bodyPart == BodyPart.leftUpperArm && hitBox2.bodyPart == BodyPart.rightUpperArm)) 
					invertTransform(hitBox, hitBox2);
			}
		}
	}
	
	private static float distancePointToRectangle(Vector3 point, Rect rect) {
		//http://wiki.unity3d.com/index.php/Distance_from_a_point_to_a_rectangle
		if (point.x < rect.xMin) { // Region I, VIII, or VII
			if (point.y < rect.yMin) { // I
				Vector3 diff = point - new Vector3(rect.xMin, rect.yMin);
				return diff.magnitude;
			}
			else if (point.y > rect.yMax) { // VII
				Vector2 diff = point - new Vector3(rect.xMin, rect.yMax);
				return diff.magnitude;
			}
			else { // VIII
				return rect.xMin - point.x;
			}
		}
		else if (point.x > rect.xMax) { // Region III, IV, or V
			if (point.y < rect.yMin) { // III
				Vector3 diff = point - new Vector3(rect.xMax, rect.yMin);
				return diff.magnitude;
			}
			else if (point.y > rect.yMax) { // V
				Vector3 diff = point - new Vector3(rect.xMax, rect.yMax);
				return diff.magnitude;
			}
			else { // IV
				return point.x - rect.xMax;
			}
		}
		else { // Region II, IX, or VI
			if (point.y < rect.yMin) { // II
				return rect.yMin - point.y;
			}
			else if (point.y > rect.yMax) { // VI
				return point.y - rect.yMax;
			}
			else { // IX
				return 0f;
			}
		}
	}

	private void invertTransform(HitBox hb1, HitBox hb2){
		Transform transformStorage = hb2.position;
		hb2.position = hb1.position;
		hb1.position = transformStorage;
	}
	
	public Transform FindTransform(string searchString){
		Transform[] transformChildren = GetComponentsInChildren<Transform>();
		Transform found;
		foreach(Transform transformChild in transformChildren){
			found = transformChild.Find("mixamorig:"+ searchString);
			if (found == null) found = transformChild.Find(gameObject.name + ":" + searchString);
			if (found == null) found = transformChild.Find(searchString);
			if (found != null) return found;
		}
		return null;
	}

	
	public Rect GetBounds(){
		if (characterRenderer != null){
			return new Rect(characterRenderer.bounds.min.x, 
		    	            characterRenderer.bounds.min.y, 
		        	        characterRenderer.bounds.max.x,
		            	    characterRenderer.bounds.max.y);
		}else{
			// alternative bounds
		}

		return new Rect();
	}
	
	public void UpdateBounds(HurtBox[] hurtBoxes){
		foreach(HurtBox hurtBox in hurtBoxes) if (hurtBox.followXBounds || hurtBox.followYBounds) hurtBox.rendererBounds = GetBounds();
	}

	public void UpdateRenderer(){
		bool confirmUpdate = false;
		foreach(HitBox hitBox in hitBoxes){
			//hitBox.storedRadius = hitBox.radius;
			if (hitBox.followXBounds || hitBox.followYBounds) confirmUpdate = true;
		}

		if (myMoveSetScript != null){
			foreach(MoveInfo move in myMoveSetScript.moves){
				foreach(Hit hit in move.hits){
					foreach(HurtBox hurtbox in hit.hurtBoxes){
						if (hurtbox.followXBounds || hurtbox.followYBounds) confirmUpdate = true;
					}
				}

				if (move.blockableArea != null && (move.blockableArea.followXBounds || move.blockableArea.followYBounds))
					confirmUpdate = true;
			}
		}

		if (confirmUpdate){
			Renderer[] rendererList = GetComponentsInChildren<Renderer>();
			foreach(Renderer childRenderer in rendererList){
				characterRenderer = childRenderer;
				return;
			}
			Debug.LogWarning("Warning: You are trying to access the character's bounds, but it does not have a renderer.");
		}
	}

	private void GizmosDrawRectangle(Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight){
		Gizmos.DrawLine(topLeft, bottomLeft);
		Gizmos.DrawLine(bottomLeft, bottomRight);
		Gizmos.DrawLine(bottomRight, topRight);
		Gizmos.DrawLine(topRight, topLeft);
	}

	void OnDrawGizmos() {
		// HITBOXES
		if (hitBoxes == null) return;
		int mirrorAdjust = controlsScript != null? controlsScript.mirror : -1;

		foreach (HitBox hitBox in hitBoxes) {
			if (hitBox.position == null) continue;
			if (hitBox.hide) continue;
			if (hitBox.state == 1) {
				Gizmos.color = Color.red;
			} else if (isHit){
				Gizmos.color = Color.magenta;
			} else if (hitBox.collisionType == CollisionType.bodyCollider) {	
				Gizmos.color = Color.yellow;
			} else if (hitBox.collisionType == CollisionType.noCollider) {	
				Gizmos.color = Color.white;
			} else if (hitBox.collisionType == CollisionType.throwCollider) {	
				Gizmos.color = new Color(1f, 0, .5f);
			}else{
				Gizmos.color = Color.green;
			}

			Vector3 hitBoxPosition = hitBox.position.position + new Vector3(hitBox.offSet.x, hitBox.offSet.y, 0);
			if (UFE.config == null || !UFE.config.detect3D_Hits) hitBoxPosition.z = -1;
			if (hitBox.shape == HitBoxShape.circle && hitBox.radius > 0){
				Gizmos.DrawWireSphere(hitBoxPosition, hitBox.radius);
			}else if (hitBox.shape == HitBoxShape.rectangle){
				hitBoxPosition = hitBox.position.position;
				Vector3 topLeft = new Vector3(hitBox.rect.x * -mirrorAdjust, hitBox.rect.y) + hitBoxPosition;
				Vector3 topRight = new Vector3((hitBox.rect.x + hitBox.rect.width) * -mirrorAdjust, hitBox.rect.y) + hitBoxPosition;
				Vector3 bottomLeft = new Vector3(hitBox.rect.x * -mirrorAdjust, hitBox.rect.y + hitBox.rect.height) + hitBoxPosition;
				Vector3 bottomRight = new Vector3((hitBox.rect.x + hitBox.rect.width) * -mirrorAdjust, hitBox.rect.y + hitBox.rect.height) + hitBoxPosition;
				
				if (hitBox.followXBounds){
					hitBox.rect.x = 0;
					topLeft.x = GetBounds().x - (hitBox.rect.width/2);
					topRight.x = GetBounds().width + (hitBox.rect.width/2);
					bottomLeft.x = GetBounds().x - (hitBox.rect.width/2);
					bottomRight.x = GetBounds().width + (hitBox.rect.width/2);
				}
				
				if (hitBox.followYBounds){
					hitBox.rect.y = 0;
					topLeft.y = GetBounds().height + (hitBox.rect.height/2);
					topRight.y = GetBounds().height + (hitBox.rect.height/2);
					bottomLeft.y = GetBounds().y - (hitBox.rect.height/2);
					bottomRight.y = GetBounds().y - (hitBox.rect.height/2);
				}

				GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
			}
			
			if (hitBox.collisionType != CollisionType.noCollider){
				if (hitBox.type == HitBoxType.low){
					Gizmos.color = Color.red;
				}else{
					Gizmos.color = Color.yellow;
				}
				Gizmos.DrawWireSphere(hitBoxPosition, .1f);
			}
		}

		// COLLISION BOX SIZE
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, collisionBoxSize);


		// HURTBOXES
		if (activeHurtBoxes != null) {
			if (hitConfirmType == HitConfirmType.Throw){
				Gizmos.color = new Color(1f, .5f, 0);
			}else{
				Gizmos.color = Color.cyan;
			}

			foreach (HurtBox hurtBox in activeHurtBoxes) {
				if (GetTransform(hurtBox.bodyPart) != null){
					Vector3 hurtBoxPosition;
					hurtBoxPosition = GetPosition(hurtBox.bodyPart);
					if (UFE.config == null || !UFE.config.detect3D_Hits) hurtBoxPosition.z = -1;
					if (hurtBox.shape == HitBoxShape.circle){
						hurtBoxPosition += new Vector3(hurtBox.offSet.x * -mirrorAdjust, hurtBox.offSet.y, 0);
						Gizmos.DrawWireSphere(hurtBoxPosition, hurtBox.radius);
					}else{
						Vector3 topLeft = new Vector3(hurtBox.rect.x * -mirrorAdjust, hurtBox.rect.y) + hurtBoxPosition;
						Vector3 topRight = new Vector3((hurtBox.rect.x + hurtBox.rect.width) * -mirrorAdjust, hurtBox.rect.y) + hurtBoxPosition;
						Vector3 bottomLeft = new Vector3(hurtBox.rect.x * -mirrorAdjust, hurtBox.rect.y + hurtBox.rect.height) + hurtBoxPosition;
						Vector3 bottomRight = new Vector3((hurtBox.rect.x + hurtBox.rect.width) * -mirrorAdjust, hurtBox.rect.y + hurtBox.rect.height) + hurtBoxPosition;

						if (hurtBox.followXBounds){
							hurtBox.rect.x = 0;
							topLeft.x = GetBounds().x - (hurtBox.rect.width/2);
							topRight.x = GetBounds().width + (hurtBox.rect.width/2);
							bottomLeft.x = GetBounds().x - (hurtBox.rect.width/2);
							bottomRight.x = GetBounds().width + (hurtBox.rect.width/2);
						}
						
						if (hurtBox.followYBounds){
							hurtBox.rect.y = 0;
							topLeft.y = GetBounds().height + (hurtBox.rect.height/2);
							topRight.y = GetBounds().height + (hurtBox.rect.height/2);
							bottomLeft.y = GetBounds().y - (hurtBox.rect.height/2);
							bottomRight.y = GetBounds().y - (hurtBox.rect.height/2);
						}
						GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
					}
				}
			}
		}
		
		
		// BLOCKBOXES
		if (blockableArea != null){
			Gizmos.color = Color.blue;

			if (GetTransform(blockableArea.bodyPart) != null){
				Vector3 blockableAreaPosition;
				blockableAreaPosition = GetPosition(blockableArea.bodyPart);
				if (UFE.config == null || !UFE.config.detect3D_Hits) blockableAreaPosition.z = -1;
				if (blockableArea.shape == HitBoxShape.circle){
					blockableAreaPosition += new Vector3(blockableArea.offSet.x * -mirrorAdjust, blockableArea.offSet.y, 0);
					//blockableAreaPosition.x += (blockableArea.radius/2) * -mirrorAdjust;
					Gizmos.DrawWireSphere(blockableAreaPosition, blockableArea.radius);
				}else{
					Vector3 topLeft = new Vector3(blockableArea.rect.x * -mirrorAdjust, blockableArea.rect.y) + blockableAreaPosition;
					Vector3 topRight = new Vector3((blockableArea.rect.x + blockableArea.rect.width) * -mirrorAdjust, blockableArea.rect.y) + blockableAreaPosition;
					Vector3 bottomLeft = new Vector3(blockableArea.rect.x * -mirrorAdjust, blockableArea.rect.y + blockableArea.rect.height) + blockableAreaPosition;
					Vector3 bottomRight = new Vector3((blockableArea.rect.x + blockableArea.rect.width) * -mirrorAdjust, blockableArea.rect.y + blockableArea.rect.height) + blockableAreaPosition;
					GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
				}
			}
		}
    }
}
