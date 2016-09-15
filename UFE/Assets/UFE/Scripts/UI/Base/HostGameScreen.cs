using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
public class HostGameScreen : UFEScreen{
	public Text roomName;
	public GameObject HostGameMenu;
	public virtual void GoToNetworkGameScreen(){
		Network.InitializeServer(32,25002,!Network.HavePublicAddress());
		MasterServer.RegisterHost(roomName.ToString(),"Fighting");
		//UFE.StartNetworkGameScreen();
	}

	public virtual void GoToConnectionLostScreen(){
		UFE.StartConnectionLostScreen();
	}

	public virtual void StartHostGame() {
		//UFE.HostGame();
		Network.InitializeServer(32,25002,!Network.HavePublicAddress());
		MasterServer.RegisterHost(roomName.ToString(),"Fighting");
	}
}
