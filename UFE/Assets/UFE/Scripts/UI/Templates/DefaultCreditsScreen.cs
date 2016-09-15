using UnityEngine;
using System.Collections;

public class DefaultCreditsScreen : CreditsScreen{
	#region public instance properties
	public AudioClip music;
	public AudioClip onLoadSound;
	public bool stopPreviousSoundEffectsOnLoad = false;
	public float delayBeforePlayingMusic = 0.1f;
	#endregion
	
	#region public override methods
	public override void DoFixedUpdate(){
		base.DoFixedUpdate();
		this.DefaultNavigationSystem(null, null, this.GoToMainMenuScreen);
	}

	public override void OnShow (){
		base.OnShow ();
		this.HighlightOption(this.FindFirstSelectable());
		
		if (this.music != null){
			UFE.DelayLocalAction(delegate(){UFE.PlayMusic(this.music);}, this.delayBeforePlayingMusic);
		}
		
		if (this.stopPreviousSoundEffectsOnLoad){
			UFE.StopSounds();
		}
		
		if (this.onLoadSound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.onLoadSound);}, this.delayBeforePlayingMusic);
		}
	}
	#endregion
}
