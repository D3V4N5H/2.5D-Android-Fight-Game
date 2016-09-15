using UnityEngine;
using System.Collections;

public class PauseScreen : UFEScreen {
	public virtual void GoToMainMenu(){
		UFE.StartMainMenuScreen(0f);
	}

	public virtual void ResumeGame(){
		UFE.PauseGame(false);
	}
}
