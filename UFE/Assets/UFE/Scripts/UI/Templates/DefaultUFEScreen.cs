using UnityEngine;
using UnityEngine.UI;

public class DefaultUFEScreen : UFEScreen{
	#region public instance properties
	public AudioClip onLoadSound;
	public AudioClip music;
	public AudioClip selectSound;
	public AudioClip cancelSound;
	public AudioClip moveCursorSound;
	public Button cancelButton;
	public bool stopPreviousSoundEffectsOnLoad = false;
	public float delayBeforePlayingMusic = 0.1f;
	#endregion

	#region public override methods
	public override void DoFixedUpdate(){
		base.DoFixedUpdate();
		this.DefaultNavigationSystem(this.selectSound, this.moveCursorSound, this.CancelAction, this.cancelSound);
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

	#region protected methods
	protected virtual void CancelAction(){
		if (this.cancelButton != null && this.cancelButton.onClick != null){
			this.cancelButton.onClick.Invoke();
		}
	}
	#endregion
}
