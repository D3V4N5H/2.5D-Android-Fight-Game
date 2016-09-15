using UnityEngine;
using System.Collections;

public class StoryModeContinueScreen : UFEScreen {
	public virtual void RepeatBattle(){
		UFE.StartStoryModeBattle();
	}

	public virtual void GoToGameOverScreen(){
		UFE.StartStoryModeGameOverScreen();
	}
}
