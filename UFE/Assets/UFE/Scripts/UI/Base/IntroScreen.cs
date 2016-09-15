using UnityEngine;
using System.Collections;

public class IntroScreen : UFEScreen {
	public virtual void GoToMainMenu(){
		UFE.StartMainMenuScreen(0f);
	}
}
