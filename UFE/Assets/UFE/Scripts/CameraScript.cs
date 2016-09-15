using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	public GameObject playerLight;
	[HideInInspector] public bool killCamMove;
	[HideInInspector] public bool cinematicFreeze;
	
	private Vector3 targetPosition;
	private Quaternion targetRotation;
	private float targetFieldOfView;
	private float camSpeed;
	
	private Transform player1;
	private Transform player2;
	private string lastOwner;
	
	//private Vector3 cameraStartingPos;
	//private float standardZoom;
	private float standardDistance;
	private float standardGroundHeight;
	//private Quaternion standardRotation;
	//private float fieldOfView;


	void Start(){
		playerLight = GameObject.Find("Player Light");
		player1 = GameObject.Find("Player1").transform;
		player2 = GameObject.Find("Player2").transform;

		ResetCam();
		//standardZoom = UFE.config.cameraOptions.initialDistance.z;
		standardDistance = Vector3.Distance(player1.position, player2.position);
		UFE.freeCamera = false;
	}

	public void ResetCam(){
		Camera.main.transform.localPosition = UFE.config.cameraOptions.initialDistance;
		Camera.main.transform.position = UFE.config.cameraOptions.initialDistance;
		Camera.main.transform.localRotation = Quaternion.Euler(UFE.config.cameraOptions.initialRotation);
		Camera.main.fieldOfView = UFE.config.cameraOptions.initialFieldOfView;
		//standardGroundHeight = Camera.main.transform.position.y;

	}

	public Vector3 LerpByDistance(Vector3 A, Vector3 B, float speed){
		Vector3 P = speed * Time.fixedDeltaTime * Vector3.Normalize(B - A) + A;
		return P;
	}

	public void DoFixedUpdate() {
		if (killCamMove) return;
		if (UFE.freeCamera) {
			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFieldOfView, Time.fixedDeltaTime * camSpeed * 1.8f);
			Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, targetPosition, Time.fixedDeltaTime * camSpeed * 1.8f);
			Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, targetRotation, Time.fixedDeltaTime * camSpeed * 1.8f);

		}else{
			Vector3 newPosition = ((player1.position + player2.position)/2) + UFE.config.cameraOptions.initialDistance;
			if (UFE.config.cameraOptions.followJumpingCharacter) 
				newPosition.y += Mathf.Abs(player1.position.y - player2.position.y)/2;

			newPosition.x = Mathf.Clamp(newPosition.x, 
				UFE.config.selectedStage.leftBoundary + 8, 
				UFE.config.selectedStage.rightBoundary - 8);
			newPosition.z = UFE.config.cameraOptions.initialDistance.z - Vector3.Distance(player1.position, player2.position) + standardDistance;
			newPosition.z = Mathf.Clamp(newPosition.z, -UFE.config.cameraOptions.maxZoom, -UFE.config.cameraOptions.minZoom);
			
			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, UFE.config.cameraOptions.initialFieldOfView, Time.fixedDeltaTime * UFE.config.cameraOptions.smooth);
			Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newPosition, Time.fixedDeltaTime * UFE.config.cameraOptions.smooth);
			Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, Quaternion.Euler(UFE.config.cameraOptions.initialRotation), Time.fixedDeltaTime * UFE.config.cameraOptions.smooth);

			if (Camera.main.transform.localRotation == Quaternion.Euler(UFE.config.cameraOptions.initialRotation))
				UFE.normalizedCam = true;

			if (playerLight != null) playerLight.GetComponent<Light>().enabled = false;

			if (UFE.config.cameraOptions.enableLookAt){
				Vector3 lookAtPosition = ((player1.position + player2.position)/2);
				lookAtPosition.y += UFE.config.cameraOptions.heightOffSet;
				Camera.main.transform.LookAt(lookAtPosition, Vector3.up);
			}
		}
	}

	public void MoveCameraToLocation(Vector3 targetPos, Vector3 targetRot, float targetFOV, float speed, string owner){
		targetFieldOfView = targetFOV;
		targetPosition = targetPos;
		targetRotation = Quaternion.Euler(targetRot);
		camSpeed = speed;
		UFE.freeCamera = true;
		UFE.normalizedCam = false;
		lastOwner = owner;
		if (playerLight != null) playerLight.GetComponent<Light>().enabled = true;
	}
	
	public void DisableCam(){
		Camera.main.enabled = false;
	}

	public void ReleaseCam(){
		Camera.main.enabled = true;
		cinematicFreeze = false;
		UFE.freeCamera = false;
		lastOwner = "";
	}

	public void SetCameraOwner(string owner){
		lastOwner = owner;
	}

	public string GetCameraOwner(){
		return lastOwner;
	}

	public Vector3 GetRelativePosition(Transform origin, Vector3 position) {
		Vector3 distance = position - origin.position;
		Vector3 relativePosition = Vector3.zero;
		relativePosition.x = Vector3.Dot(distance, origin.right.normalized);
		relativePosition.y = Vector3.Dot(distance, origin.up.normalized);
		relativePosition.z = Vector3.Dot(distance, origin.forward.normalized);
		
		return relativePosition;
	}
}
