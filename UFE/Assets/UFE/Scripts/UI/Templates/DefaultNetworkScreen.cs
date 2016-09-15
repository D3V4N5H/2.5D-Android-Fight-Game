using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DefaultNetworkScreen : NetworkGameScreen {

	public Text myInfoText;

	// Use this for initialization
	void Start () {
		if (myInfoText != null) {
			myInfoText.text = "IP: " + GetIp() + "\nPort: " + UFE.config.networkOptions.port;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}


}
