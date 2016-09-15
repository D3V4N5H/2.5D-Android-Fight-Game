using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DefaultLoadingBattleScreen : LoadingBattleScreen{
	#region public instance properties
	public AudioClip onLoadSound;
	public AudioClip music;
	public Text namePlayer1;
	public Text namePlayer2;
	public Text nameStage;
	public Image portraitPlayer1;
	public Image portraitPlayer2;
	public Image screenshotStage;
	public bool stopPreviousSoundEffectsOnLoad = false;
	public float delayBeforePlayingMusic = 0.1f;
	public float delayBeforeLoadingNextScreen = 3f;
	#endregion
	
	#region public override methods
	public override void OnShow (){
		base.OnShow ();

		if (this.music != null){
			UFE.DelayLocalAction(delegate(){UFE.PlayMusic(this.music);}, this.delayBeforePlayingMusic);
		}
		
		if (this.stopPreviousSoundEffectsOnLoad){
			UFE.StopSounds();
		}
		
		if (this.onLoadSound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.onLoadSound);}, this.delayBeforePlayingMusic);
		}

		if (UFE.config.player1Character != null){
			if (this.portraitPlayer1 != null){
				this.portraitPlayer1.sprite = Sprite.Create(
					UFE.config.player1Character.profilePictureBig,
					new Rect(0f, 0f, UFE.config.player1Character.profilePictureBig.width, UFE.config.player1Character.profilePictureBig.height),
					new Vector2(0.5f * UFE.config.player1Character.profilePictureBig.width, 0.5f * UFE.config.player1Character.profilePictureBig.height)
				);
			}

			if (this.namePlayer1 != null){
				this.namePlayer1.text = UFE.config.player1Character.characterName;
			}
		}

		if (UFE.config.player2Character != null){
			if (this.portraitPlayer2 != null){
				this.portraitPlayer2.sprite = Sprite.Create(
					UFE.config.player2Character.profilePictureBig,
					new Rect(0f, 0f, UFE.config.player2Character.profilePictureBig.width, UFE.config.player2Character.profilePictureBig.height),
					new Vector2(0.5f * UFE.config.player2Character.profilePictureBig.width, 0.5f * UFE.config.player2Character.profilePictureBig.height)
				);
			}

			if (this.namePlayer2 != null){
				this.namePlayer2.text = UFE.config.player2Character.characterName;
			}
		}

		if (UFE.config.selectedStage != null){
			if (this.screenshotStage != null){
				this.screenshotStage.sprite = Sprite.Create(
					UFE.config.selectedStage.screenshot,
					new Rect(0f, 0f, UFE.config.selectedStage.screenshot.width, UFE.config.selectedStage.screenshot.height),
					new Vector2(0.5f * UFE.config.selectedStage.screenshot.width, 0.5f * UFE.config.selectedStage.screenshot.height)
				);

				Animator anim = this.screenshotStage.GetComponent<Animator>();
				if (anim != null){
					anim.enabled = UFE.gameMode != GameMode.StoryMode;
				}
			}

			/*if (this.nameStage != null){
				this.nameStage.text = UFE.config.selectedStage.stageName;
			}*/
		}

		UFE.DelaySynchronizedAction(this.StartBattle, this.delayBeforeLoadingNextScreen);
	}
	#endregion
}
