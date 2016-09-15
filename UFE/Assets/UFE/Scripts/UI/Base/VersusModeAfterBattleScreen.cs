using UnityEngine;
using System.Collections;

public class VersusModeAfterBattleScreen : UFEScreen {
	public virtual void GoToCharacterSelectionScreen(){
		UFE.StartCharacterSelectionScreen();
	}

	public virtual void GoToMainMenu(){
		UFE.StartMainMenuScreen();
	}

	public virtual void GoToStageSelectionScreen(){
		UFE.StartStageSelectionScreen();
	}

	public virtual void RepeatBattle(){
		UFE.StartLoadingBattleScreen();
	}
}
