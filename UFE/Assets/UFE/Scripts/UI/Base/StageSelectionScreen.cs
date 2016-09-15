using UnityEngine;
using System;
using System.Reflection;

public class StageSelectionScreen : UFEScreen{
	#region public instance properties
	public AudioClip selectSound;
	public AudioClip cancelSound;
	public bool fadeBeforeGoingToLoadingBattleScreen = false;
	#endregion

	#region protected instance fields
	protected bool closing = false;
	protected int stageHoverIndex = 0;
	#endregion

	#region public instance methods
	public virtual void GoToCharacterSelectionScreen(){
		this.closing = true;
		UFE.StartCharacterSelectionScreen();
	}

	public virtual void GoToLoadingBattleScreen(){
		this.closing = true;
		if (this.fadeBeforeGoingToLoadingBattleScreen){
			UFE.StartLoadingBattleScreen();
		}else{
			UFE.StartLoadingBattleScreen(0f);
		}
	}

	public virtual void SetHoverIndex(int stageIndex){
		if (!this.closing && stageIndex >= 0 && stageIndex < UFE.config.stages.Length){
			this.stageHoverIndex = stageIndex;
		}
	}

	public void OnStageSelectionAllowed(int stageIndex){
		if (!this.closing){
			if (stageIndex >= 0 && stageIndex < UFE.config.stages.Length){
				if (this.selectSound != null)UFE.PlaySound(this.selectSound);
				this.SetHoverIndex(stageIndex);

				UFE.config.selectedStage = UFE.config.stages[stageIndex];
				this.GoToLoadingBattleScreen();
			}else if (stageIndex < 0){
				if (UFE.config.selectedStage != null){
					if (this.cancelSound != null) UFE.PlaySound(this.cancelSound);
					UFE.config.selectedStage = null;
				}else{
					if (this.cancelSound != null) UFE.PlaySound(this.cancelSound);
					this.GoToCharacterSelectionScreen();
				}
			}
		}
	}

	public void TryDeselectStage(){
		this.TrySelectStage(-1);
	}

	public void TrySelectStage(){
		this.TrySelectStage(this.stageHoverIndex);
	}

	public void TrySelectStage(int stageIndex){
		// Check if he was playing online or not...
		if (Network.peerType == NetworkPeerType.Disconnected){
			// If it's a local game, update the corresponding stage immediately...
			this.OnStageSelectionAllowed(stageIndex);
		}else{
			// If it's an online game, we only select the stage if it has been requested by Player 1...
			int localPlayer = UFE.GetLocalPlayer();
			if (localPlayer == 1){
				UFEController controller = UFE.GetController(localPlayer);
				
				// We don't invoke the OnstageSelected() method immediately because we are using the frame-delay 
				// algorithm to keep players synchronized, so we can't invoke the OnstageSelected() method
				// until the other player has received the message with our choice.
				controller.GetType().GetMethod(
					"RequestOptionSelection",
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy,
					null,
					new Type[]{typeof(int)},
					null
				).Invoke(controller, new object[]{stageIndex});
			}
		}
	}
	#endregion

	#region public override methods
	public override void OnShow (){
		this.closing = false;
	}

	public override void SelectOption (int option, int player){
		this.OnStageSelectionAllowed(option	);
	}
	#endregion
}
