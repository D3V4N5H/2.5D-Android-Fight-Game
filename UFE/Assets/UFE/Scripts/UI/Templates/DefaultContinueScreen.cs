using UnityEngine;
using System.Collections;

public class DefaultContinueScreen : StoryModeContinueScreen{
	#region public instance properties
	public AudioClip music;
	public AudioClip countdownSound;
	public AudioClip selectSound;
	public AudioClip cancelSound;
	public AudioClip moveCursorSound;
	public float delayBeforePlayingMusic = 0.1f;
	#endregion

	#region public override methods
	public override void DoFixedUpdate(){
		base.DoFixedUpdate();
		this.DefaultNavigationSystem(this.selectSound, this.moveCursorSound, null, this.cancelSound);
	}

	public override void OnShow (){
		base.OnShow ();
		this.HighlightOption(this.FindFirstSelectable());

		if (this.music != null){
			UFE.DelayLocalAction(delegate(){UFE.PlayMusic(this.music);}, this.delayBeforePlayingMusic);
		}
		
		if (this.countdownSound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.countdownSound);}, this.delayBeforePlayingMusic);
		}
	}
	#endregion
}
