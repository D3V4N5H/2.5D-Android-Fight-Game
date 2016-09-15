using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class JoinGameScreen : UFEScreen {
	public virtual void GoToNetworkGameScreen(){
		UFE.StartNetworkGameScreen();
	}

	public virtual void GoToConnectionLostScreen(){
		UFE.StartConnectionLostScreen();
	}

	public virtual void RefreshGameList() {

	}

	public virtual void JoinGame(Text textUI) {
		UFE.JoinGame(textUI.text);
	}
}
