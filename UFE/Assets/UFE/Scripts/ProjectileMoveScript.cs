using UnityEngine;
using System.Collections;

public class ProjectileMoveScript : MonoBehaviour {
	public Projectile data;
	public int mirror = -1;
	public HitBoxesScript opHitBoxesScript;
	public ControlsScript opControlsScript;
	public ControlsScript myControlsScript;
	public HurtBox hurtBox;
	public HitBox hitBox;
	public BlockArea blockableArea;
	public bool destroyMe;
	
	private Hit hit;
	private Vector3 directionVector = new Vector3(1, 0, 0);
	private int totalHits;
	private float isHit;
	private float spaceBetweenHits = .1f;
	
	private Vector3 movement;
	private Renderer projectileRenderer;


	
	//private int opProjectileLayer;
	//private int opProjectileMask;
	
	void Start () {
		gameObject.AddComponent<SphereCollider>();
		/*if (opControlsScript.gameObject.name == "Player1"){
			gameObject.layer = LayerMask.NameToLayer("Projectile1");
			opProjectileLayer = LayerMask.NameToLayer("Projectile2");
		}else{
			gameObject.layer = LayerMask.NameToLayer("Projectile2");
			opProjectileLayer = LayerMask.NameToLayer("Projectile1");
		}
		opProjectileMask = 1 << opProjectileLayer;*/

		if (mirror == 1) directionVector = new Vector3(-1, 0, 0);

		totalHits = data.totalHits;

		float angleRad = ((float)data.directionAngle/180) * Mathf.PI;
		movement = ((Mathf.Sin(angleRad) * Vector3.up) + (Mathf.Cos(angleRad) * directionVector)) * data.speed;
		UFE.DelaySynchronizedAction(delegate(){try{Destroy(this.gameObject);}catch{}}, data.duration);
		transform.Translate(new Vector3(data.castingOffSet.x * -mirror, data.castingOffSet.y, data.castingOffSet.z));

		// Create Blockable Area
		blockableArea = new BlockArea();
		blockableArea = data.blockableArea;

		// Create Hurtbox
		hurtBox = new HurtBox();
		hurtBox = data.hurtBox;

		// Create Hitbox
		hitBox = new HitBox();
		hitBox.shape = hurtBox.shape;
		hitBox.rect = hurtBox.rect;
		hitBox.followXBounds = hurtBox.followXBounds;
		hitBox.followYBounds = hurtBox.followYBounds;
		hitBox.radius = hurtBox.radius;
		hitBox.offSet = hurtBox.offSet;
		hitBox.position = gameObject.transform;

		UpdateRenderer();

		if (data.spaceBetweenHits == Sizes.Small){
			spaceBetweenHits = .15f;
		}else if (data.spaceBetweenHits == Sizes.Medium){
			spaceBetweenHits = .2f;
		}else if (data.spaceBetweenHits == Sizes.High){
			spaceBetweenHits = .3f;
		}

		
		// Create Hit data
		hit = new Hit();
		hit.hitType = data.hitType;
		hit.spaceBetweenHits = data.spaceBetweenHits;
		hit.hitStrength = data.hitStrength;
		hit.hitStunType = HitStunType.Frames;
		hit.hitStunOnHit = data.hitStunOnHit;
		hit.hitStunOnBlock = data.hitStunOnBlock;
		hit.damageOnHit = data.damageOnHit;
		hit.damageOnBlock = data.damageOnBlock;
		hit.damageScaling = data.damageScaling;
		hit.damageType = data.damageType;
		hit.groundHit = data.groundHit;
		hit.airHit = data.airHit;
		hit.downHit = data.downHit;
		hit.overrideHitEffects = data.overrideHitEffects;
		hit.hitEffects = data.hitEffects;
		hit.resetPreviousHorizontalPush = true;
		hit.pushForce = data.pushForce;
		hit.pullEnemyIn = new PullIn();
		hit.pullEnemyIn.enemyBodyPart = BodyPart.none;
	}

	public void UpdateRenderer(){
		if (hurtBox.followXBounds || hurtBox.followYBounds){
			Renderer[] rendererList = GetComponentsInChildren<Renderer>();
			foreach(Renderer childRenderer in rendererList){
				projectileRenderer = childRenderer;
			}
			if (projectileRenderer == null) 
				Debug.LogWarning("Warning: You are trying to access the projectile's bounds, but it does not have a renderer.");

		}
	}
	
	public bool IsDestroyed () {
		if (this == null) return true; 
		if (destroyMe) Destroy(gameObject);
		return destroyMe;
	}

	public void DoFixedUpdate () {
		if (isHit > 0) {
			isHit -= Time.fixedDeltaTime;
			return;
		}

		// Check if both controllers are ready
		AbstractInputController p1InputController = UFE.GetPlayer1Controller();
		AbstractInputController p2InputController = UFE.GetPlayer2Controller();
		if (p1InputController == null || !p1InputController.isReady || p2InputController == null || !p2InputController.isReady){return;}

		if (UFE.freeCamera) return;

		transform.position += (movement * Time.fixedDeltaTime);

		hurtBox.position = gameObject.transform.position;
		if (projectileRenderer != null && (hurtBox.followXBounds || hurtBox.followYBounds)) {
			hurtBox.rendererBounds = GetBounds();
			hitBox.rendererBounds = GetBounds();
		}

		blockableArea.position = transform.position;
		if (!opControlsScript.isBlocking
		    && !opControlsScript.blockStunned
		    && opControlsScript.currentSubState != SubStates.Stunned
		    && opHitBoxesScript.TestCollision(blockableArea) != Vector3.zero) {
			opControlsScript.CheckBlocking(true);
		}

		if (data.projectileCollision){
			if (opControlsScript.projectiles.Count > 0){
				foreach(ProjectileMoveScript projectile in opControlsScript.projectiles){
					if (projectile == null) continue;
					if (projectile.hitBox == null) continue;
					if (projectile.hurtBox == null) continue;

					if (HitBoxesScript.TestCollision(new HitBox[]{projectile.hitBox}, new HurtBox[]{hurtBox}, HitConfirmType.Hit, mirror) != Vector3.zero){

						if (data.impactPrefab != null){
							GameObject hitEffect = (GameObject) Instantiate(data.impactPrefab, transform.position, Quaternion.Euler(0,0,data.directionAngle));
							UFE.DelaySynchronizedAction(delegate(){try{Destroy(hitEffect);}catch{}}, data.impactDuration);
						}
						totalHits --;
						if (totalHits <= 0){
							destroyMe = true;
						}
						isHit = spaceBetweenHits;
						transform.Translate(movement * -1 * Time.fixedDeltaTime);
						break;
					}
				}
			}
		}

		if (opHitBoxesScript.TestCollision(new HurtBox[]{hurtBox}, HitConfirmType.Hit) != Vector3.zero
		    && opControlsScript.ValidateHit(hit)) {

			if (data.impactPrefab != null){
				GameObject hitEffect = (GameObject) Instantiate(data.impactPrefab, transform.position, Quaternion.Euler(0,0,data.directionAngle));
				UFE.DelaySynchronizedAction(delegate(){try{Destroy(hitEffect);}catch{}}, data.impactDuration);
			}
			totalHits --;
			if (totalHits <= 0){
				UFE.DelaySynchronizedAction(delegate(){try{Destroy(gameObject);}catch{}}, (float)(2/UFE.config.fps));
			}

			
			if (opControlsScript.currentSubState != SubStates.Stunned && opControlsScript.isBlocking && opControlsScript.TestBlockStances(hit.hitType)){
				myControlsScript.AddGauge(data.gaugeGainOnBlock);
				opControlsScript.AddGauge(data.opGaugeGainOnBlock);
				opControlsScript.GetHitBlocking(hit, 20, transform.position);

                if (data.moveLinkOnBlock != null)
                    myControlsScript.CastMove(data.moveLinkOnBlock, true, data.forceGrounded);

			}else if (opControlsScript.potentialParry > 0 && opControlsScript.TestParryStances(hit.hitType)){
				opControlsScript.AddGauge(data.opGaugeGainOnParry);
				opControlsScript.GetHitParry(hit, 20, transform.position);

                if (data.moveLinkOnParry != null)
                    myControlsScript.CastMove(data.moveLinkOnParry, true, data.forceGrounded);

			}else{
				myControlsScript.AddGauge(data.gaugeGainOnHit);
				opControlsScript.AddGauge(data.opGaugeGainOnHit);
				opControlsScript.GetHit(hit, 30, Vector3.zero);

                if (data.moveLinkOnStrike != null)
                    myControlsScript.CastMove(data.moveLinkOnStrike, true, data.forceGrounded);

			}

			isHit = opControlsScript.GetHitFreezingTime(data.hitStrength) * 1.2f;
			opControlsScript.CheckBlocking(false);
		}
	}

	public Rect GetBounds(){
		if (projectileRenderer != null){
			return new Rect(projectileRenderer.bounds.min.x, 
			                projectileRenderer.bounds.min.y, 
			                projectileRenderer.bounds.max.x,
			                projectileRenderer.bounds.max.y);
		}else{
			// alternative bounds
		}
		
		return new Rect();
	}

	private void GizmosDrawRectangle(Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight){
		Gizmos.DrawLine(topLeft, bottomLeft);
		Gizmos.DrawLine(bottomLeft, bottomRight);
		Gizmos.DrawLine(bottomRight, topRight);
		Gizmos.DrawLine(topRight, topLeft);
	}

	void OnDrawGizmos() {
		// COLLISION BOX SIZE
		// HURTBOXES
		if (hurtBox != null) {
			Gizmos.color = Color.cyan;

			Vector3 hurtBoxPosition = transform.position;
			if (UFE.config == null || !UFE.config.detect3D_Hits) hurtBoxPosition.z = -1;

			if (hurtBox.shape == HitBoxShape.circle){
				hurtBoxPosition += new Vector3(hurtBox.offSet.x * -mirror, hurtBox.offSet.y, 0);
				Gizmos.DrawWireSphere(hurtBoxPosition, hurtBox.radius);
			}else{
				Vector3 topLeft = new Vector3(hurtBox.rect.x * -mirror, hurtBox.rect.y) + hurtBoxPosition;
				Vector3 topRight = new Vector3((hurtBox.rect.x + hurtBox.rect.width) * -mirror, hurtBox.rect.y) + hurtBoxPosition;
				Vector3 bottomLeft = new Vector3(hurtBox.rect.x * -mirror, hurtBox.rect.y + hurtBox.rect.height) + hurtBoxPosition;
				Vector3 bottomRight = new Vector3((hurtBox.rect.x + hurtBox.rect.width) * -mirror, hurtBox.rect.y + hurtBox.rect.height) + hurtBoxPosition;

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

		// BLOCKBOXES
		if (blockableArea != null){
			Gizmos.color = Color.blue;
			
			if (!data.unblockable){
				Vector3 blockableAreaPosition;
				blockableAreaPosition = transform.position;
				if (UFE.config == null || !UFE.config.detect3D_Hits) blockableAreaPosition.z = -1;
				if (blockableArea.shape == HitBoxShape.circle){
					blockableAreaPosition += new Vector3(blockableArea.offSet.x * -mirror, blockableArea.offSet.y, 0);
					Gizmos.DrawWireSphere(blockableAreaPosition, blockableArea.radius);
				}else{
					Vector3 topLeft = new Vector3(blockableArea.rect.x * -mirror, blockableArea.rect.y) + blockableAreaPosition;
					Vector3 topRight = new Vector3((blockableArea.rect.x + blockableArea.rect.width) * -mirror, blockableArea.rect.y) + blockableAreaPosition;
					Vector3 bottomLeft = new Vector3(blockableArea.rect.x * -mirror, blockableArea.rect.y + blockableArea.rect.height) + blockableAreaPosition;
					Vector3 bottomRight = new Vector3((blockableArea.rect.x + blockableArea.rect.width) * -mirror, blockableArea.rect.y + blockableArea.rect.height) + blockableAreaPosition;
					GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
				}
			}
		}
    }
}
