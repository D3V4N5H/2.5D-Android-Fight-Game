using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class DefaultBattleGUI : BattleGUI{
	#region public class definitions
	[Serializable]
	public class PlayerGUI{
		public Text name;
		public Image lifeBar;
		public Image gaugeMeter;
		public Image[] wonRoundsImages;
		public AlertGUI alert = new AlertGUI();
	}

	[Serializable]
	public class AlertGUI{
		public Text text;
		public Vector3 initialPosition;
		public Vector3 finalPosition;
		public float movementSpeed = 15f;
	}

	[Serializable]
	public class WonRoundsGUI{
		public Sprite NotFinishedRounds;
		public Sprite WonRounds;
		public Sprite LostRounds;
		public DefaultBattleGUI.VisibleImages VisibleImages = DefaultBattleGUI.VisibleImages.WonRounds;

		public int GetNumberOfRoundsImages(){
			// To calculate the target number of images, check if the "Lost Rounds" Sprite is defined or not
			if (this.VisibleImages == VisibleImages.AllRounds){
				return UFE.config.roundOptions.totalRounds;
			}
			return (UFE.config.roundOptions.totalRounds + 1) / 2;
		}
	}

	public enum VisibleImages{
		WonRounds,
		AllRounds,
	}
	#endregion

	#region public instance properties
	public bool muteAnnouncer = false;
	public AnnouncerOptions announcer;
	public WonRoundsGUI wonRounds = new WonRoundsGUI();
	public PlayerGUI player1GUI = new PlayerGUI();
	public PlayerGUI player2GUI = new PlayerGUI();
	public AlertGUI mainAlert = new AlertGUI();
	public Text info;
	public Text timer;
	public float lifeDownSpeed = 500f;
	public float lifeUpSpeed = 900f;
	public UFEScreen pauseScreen;
	#endregion

	#region protected instance properties
	protected List<List<Image>> player1ButtonPresses = new List<List<Image>>(12);
	protected List<List<Image>> player2ButtonPresses = new List<List<Image>>(12);

	protected bool showInputs = true;
	protected bool hiding = false;

	protected float player1AlertTimer = 0f;
	protected float player2AlertTimer = 0f;
	protected float mainAlertTimer = 0f;
	protected UFEScreen pause = null;
	#endregion

	#region public override methods
	public override void DoFixedUpdate (){
		base.DoFixedUpdate ();

		if (this.isRunning){
			AbstractInputController p1InputController = UFE.GetPlayer1Controller();
			AbstractInputController p2InputController = UFE.GetPlayer2Controller();
			float deltaTime = Time.fixedDeltaTime;

			// Animate the alert messages if they exist
			if (this.player1GUI != null && this.player1GUI.alert != null && this.player1GUI.alert.text != null){
				this.player1GUI.alert.text.rectTransform.anchoredPosition = Vector3.Lerp(
					this.player1GUI.alert.text.rectTransform.anchoredPosition, 
					this.player1GUI.alert.finalPosition, 
					this.player1GUI.alert.movementSpeed * deltaTime
				);

				if (this.player1AlertTimer > 0f){
					this.player1AlertTimer -= deltaTime;
				}else if (!string.IsNullOrEmpty(this.player1GUI.alert.text.text)){
					this.player1GUI.alert.text.text = string.Empty;
				}
			}

			if (this.player2GUI != null && this.player2GUI.alert != null && this.player2GUI.alert.text != null){
				this.player2GUI.alert.text.rectTransform.anchoredPosition = Vector3.Lerp(
					this.player2GUI.alert.text.rectTransform.anchoredPosition, 
					this.player2GUI.alert.finalPosition, 
					this.player2GUI.alert.movementSpeed * deltaTime
					);

				if (this.player2AlertTimer > 0f){
					this.player2AlertTimer -= deltaTime;
				}else if (!string.IsNullOrEmpty(this.player2GUI.alert.text.text)){
					this.player2GUI.alert.text.text = string.Empty;
				}
			}

			if (this.mainAlert != null && this.mainAlert.text != null){
				if (this.mainAlertTimer > 0f){
					this.mainAlertTimer -= deltaTime;
				}else if (!string.IsNullOrEmpty(this.mainAlert.text.text)){
					this.mainAlert.text.text = string.Empty;
				}
			}

			
			// Animate life points when it goes down (P1)
			if (this.player1.targetLife > UFE.config.player1Character.currentLifePoints){
				this.player1.targetLife -= this.lifeDownSpeed * deltaTime;
                if (this.player1.targetLife < UFE.config.player1Character.currentLifePoints)
                    this.player1.targetLife = UFE.config.player1Character.currentLifePoints;
			}
			if (this.player1.targetLife < UFE.config.player1Character.currentLifePoints){
                this.player1.targetLife += this.lifeUpSpeed * deltaTime;
                if (this.player1.targetLife > UFE.config.player1Character.currentLifePoints)
                    this.player1.targetLife = UFE.config.player1Character.currentLifePoints;
			}
			
			// Animate life points when it goes down (P2)
			if (this.player2.targetLife > UFE.config.player2Character.currentLifePoints){
                this.player2.targetLife -= this.lifeDownSpeed * deltaTime;
                if (this.player2.targetLife < UFE.config.player2Character.currentLifePoints)
                    this.player2.targetLife = UFE.config.player2Character.currentLifePoints;
			}
			if (this.player2.targetLife < UFE.config.player2Character.currentLifePoints){
                this.player2.targetLife += this.lifeUpSpeed * deltaTime;
                if (this.player2.targetLife > UFE.config.player2Character.currentLifePoints)
                    this.player2.targetLife = UFE.config.player2Character.currentLifePoints;
			}
			

			if(
				// Check if both players have their life points above zero...
				UFE.config.player1Character.currentLifePoints > 0 &&
				UFE.config.player2Character.currentLifePoints > 0 &&
				UFE.gameMode != GameMode.NetworkGame &&
				(
					// and at least one of the players have pressed the Start button...
					p1InputController != null && p1InputController.GetButtonDown(ButtonPress.Start) ||
					p2InputController != null && p2InputController.GetButtonDown(ButtonPress.Start)
				)
			){
				// In that case, we can process pause menu events
				UFE.PauseGame(!UFE.isPaused());
			}


			// Draw the Life Bars and Gauge Meters using the data stored in UFE.config.guiOptions
			if (this.player1GUI != null && this.player1GUI.lifeBar != null){
				this.player1GUI.lifeBar.fillAmount = this.player1.targetLife / this.player1.totalLife;
			}
			
			if (this.player2GUI != null && this.player2GUI.lifeBar != null){
				this.player2GUI.lifeBar.fillAmount = this.player2.targetLife / this.player2.totalLife;
			}

			if (UFE.config.gameGUI.hasGauge){
				if (this.player1GUI != null && this.player1GUI.gaugeMeter != null){
					this.player1GUI.gaugeMeter.fillAmount = UFE.config.player1Character.currentGaugePoints / UFE.config.player1Character.maxGaugePoints;
				}

				if (this.player2GUI != null && this.player2GUI.gaugeMeter != null){
					this.player2GUI.gaugeMeter.fillAmount = UFE.config.player2Character.currentGaugePoints / UFE.config.player2Character.maxGaugePoints;
				}
			}

			if (this.pause != null){
				this.pause.DoFixedUpdate();
			}


			/*
			if (Debug.isDebugBuild){
				player1NameGO.guiText.text = string.Format(
					"{0}\t\t({1},\t{2},\t{3})",
					this.player1.characterName,
					UFE.GetPlayer1ControlsScript().transform.position.x,
					UFE.GetPlayer1ControlsScript().transform.position.y,
					UFE.GetPlayer1ControlsScript().transform.position.z
				);

				player2NameGO.guiText.text = string.Format(
					"{0}\t\t({1},\t{2},\t{3})",
					this.player2.characterName,
					UFE.GetPlayer2ControlsScript().transform.position.x,
					UFE.GetPlayer2ControlsScript().transform.position.y,
					UFE.GetPlayer2ControlsScript().transform.position.z
				);
			}
			*/
		}
	}

	public override void OnHide (){
		if (this.player1ButtonPresses != null){
			foreach (List<Image> images in this.player1ButtonPresses){
				if (images != null){
					foreach (Image image in images){
						if (image != null){
							GameObject.Destroy(image.gameObject);
						}
					}
				}
			}
			this.player1ButtonPresses.Clear();
		}

		if (this.player2ButtonPresses != null){
			foreach (List<Image> images in this.player2ButtonPresses){
				if (images != null){
					foreach (Image image in images){
						if (image != null){
							GameObject.Destroy(image.gameObject);
						}
					}
				}
			}
			this.player2ButtonPresses.Clear();
		}

		UFE.debugger1.enabled = false;
		UFE.debugger2.enabled = false;

		this.hiding = true;
		this.OnGamePaused(false);
		base.OnHide ();
	}

	public override void OnShow (){
		base.OnShow();
		this.hiding = false;

		if (UFE.config.debugOptions.debugMode){
			UFE.debugger1.enabled = true;
			UFE.debugger2.enabled = true;
		}else{
			UFE.debugger1.enabled = false;
			UFE.debugger2.enabled = false;
		}

		if (this.announcer != null){
			Array.Sort(this.announcer.combos, delegate(ComboAnnouncer c1, ComboAnnouncer c2) {
				return c2.hits.CompareTo(c1.hits);
			});
		}
	}

	public override void SelectOption(int option, int player){
		if (this.pause != null){
			this.pause.SelectOption(option, player);
		}
	}
	#endregion

	#region protected instance methods
	protected virtual string ProcessMessage(string msg, ControlsScript controlsScript){
		if (msg == UFE.config.selectedLanguage.combo){
			if (this.announcer != null && !this.muteAnnouncer){
				foreach(ComboAnnouncer comboAnnouncer in this.announcer.combos){
					if (controlsScript.opControlsScript.comboHits >= comboAnnouncer.hits){
						UFE.PlaySound(comboAnnouncer.audio);
						break;
					}
				}
			}
		}else if (msg == UFE.config.selectedLanguage.parry){
			if (this.announcer != null && !this.muteAnnouncer){
				UFE.PlaySound(this.announcer.parry);
			}
			UFE.PlaySound(UFE.config.blockOptions.parrySound);
		}else if (msg == UFE.config.selectedLanguage.counterHit){
			if (this.announcer != null && !this.muteAnnouncer){
				UFE.PlaySound(this.announcer.counterHit);
			}
			UFE.PlaySound(UFE.config.counterHitOptions.sound);
		}else if (msg == UFE.config.selectedLanguage.firstHit){
			if (this.announcer != null && !this.muteAnnouncer){
				UFE.PlaySound(this.announcer.firstHit);
			}
		}else{
			return this.SetStringValues(msg, null);
		}

		return this.SetStringValues(msg, controlsScript);
	}

	protected virtual string SetStringValues(string msg, ControlsScript controlsScript){
		CharacterInfo character = controlsScript != null ? controlsScript.myInfo : null;
		if (controlsScript != null) msg = msg.Replace("%combo%", controlsScript.opControlsScript.comboHits.ToString());
		if (character != null)		msg = msg.Replace("%character%", character.characterName);
		msg = msg.Replace("%round%", UFE.config.currentRound.ToString());

		return msg;
	}
	#endregion

	#region protected override methods
	protected override void OnGameBegin (CharacterInfo player1, CharacterInfo player2, StageOptions stage){
		base.OnGameBegin (player1, player2, stage);

		if (this.wonRounds.NotFinishedRounds == null){
			Debug.LogError("\"Not Finished Rounds\" Sprite not found! Make sure you have set the sprite correctly in the Editor");
		}else if (this.wonRounds.WonRounds == null){
			Debug.LogError("\"Won Rounds\" Sprite not found! Make sure you have set the sprite correctly in the Editor");
		}else if (this.wonRounds.LostRounds == null && this.wonRounds.VisibleImages == DefaultBattleGUI.VisibleImages.AllRounds){
			Debug.LogError("\"Lost Rounds\" Sprite not found! If you want to display Lost Rounds, make sure you have set the sprite correctly in the Editor");
		}else{
			// To calculate the target number of images, check if the "Lost Rounds" Sprite is defined or not
			int targetNumberOfImages = this.wonRounds.GetNumberOfRoundsImages();

			if(
				this.player1GUI != null && 
				this.player1GUI.wonRoundsImages != null && 
				this.player1GUI.wonRoundsImages.Length >= targetNumberOfImages
			){
				for (int i = 0; i < targetNumberOfImages; ++i){
					this.player1GUI.wonRoundsImages[i].enabled = true;
					this.player1GUI.wonRoundsImages[i].sprite = this.wonRounds.NotFinishedRounds;
				}
					
				for (int i = targetNumberOfImages; i < this.player1GUI.wonRoundsImages.Length; ++i){
					this.player1GUI.wonRoundsImages[i].enabled = false;
				}
			}else{
				Debug.LogError(
					"Player 1: not enough \"Won Rounds\" Images not found! " +
					"Expected:" + targetNumberOfImages + " / Found: " + this.player1GUI.wonRoundsImages.Length +
					"\nMake sure you have set the images correctly in the Editor"
				);
			}

			if(
				this.player2GUI != null && 
				this.player2GUI.wonRoundsImages != null && 
				this.player2GUI.wonRoundsImages.Length >= targetNumberOfImages
			){
				for (int i = 0; i < targetNumberOfImages; ++i){
					this.player2GUI.wonRoundsImages[i].enabled = true;
					this.player2GUI.wonRoundsImages[i].sprite = this.wonRounds.NotFinishedRounds;
				}
					
				for (int i = targetNumberOfImages; i < this.player2GUI.wonRoundsImages.Length; ++i){
					this.player2GUI.wonRoundsImages[i].enabled = false;
				}
			}else{
				Debug.LogError(
					"Player 2: not enough \"Won Rounds\" Images not found! " +
					"Expected:" + targetNumberOfImages + " / Found: " + this.player2GUI.wonRoundsImages.Length +
					"\nMake sure you have set the images correctly in the Editor"
				);
			}
		}
		
		// Set the character names
		if (this.player1GUI != null && this.player1GUI.name != null){
			this.player1GUI.name.text = player1.characterName;
		}

		if (this.player2GUI != null && this.player2GUI.name != null){
			this.player2GUI.name.text = player2.characterName;
		}

		// If we want to use a Timer, set the default value for the timer
		if (this.timer != null){
			if (UFE.config.roundOptions.hasTimer){
				this.timer.gameObject.SetActive(true);
				this.timer.text = UFE.config.roundOptions.timer.ToString();
			}else{
				this.timer.gameObject.SetActive(false);
			}
		}

		// Set the max and min values for the Life Bars and the Gauge Meters
		if (this.player1GUI != null && this.player1GUI.lifeBar != null){
			this.player1GUI.lifeBar.fillAmount = this.player1.targetLife / this.player1.totalLife;
		}
		
		if (this.player2GUI != null && this.player2GUI.lifeBar != null){
			this.player2GUI.lifeBar.fillAmount = this.player2.targetLife / this.player2.totalLife;
		}
		
		if (UFE.config.gameGUI.hasGauge){
			if (this.player1GUI != null && this.player1GUI.gaugeMeter != null){
				this.player1GUI.gaugeMeter.gameObject.SetActive(true);
				this.player1GUI.gaugeMeter.fillAmount = UFE.config.player1Character.currentGaugePoints / UFE.config.player1Character.maxGaugePoints;
			}
			
			if (this.player2 != null && this.player2GUI.gaugeMeter != null){
				this.player2GUI.gaugeMeter.gameObject.SetActive(true);
				this.player2GUI.gaugeMeter.fillAmount = UFE.config.player2Character.currentGaugePoints / UFE.config.player2Character.maxGaugePoints;
			}
		}else{
			if (this.player1GUI != null && this.player1GUI.gaugeMeter != null){
				this.player1GUI.gaugeMeter.gameObject.SetActive(false);
			}
			
			if (this.player2GUI != null && this.player2GUI.gaugeMeter != null){
				this.player2GUI.gaugeMeter.gameObject.SetActive(false);
			}
		}
	}

	protected override void OnGameEnd (CharacterInfo winner, CharacterInfo loser){
		base.OnGameEnd (winner, loser);

		if (this.player1GUI.name != null)	this.player1GUI.name.text = string.Empty;
		if (this.player2GUI.name != null)	this.player2GUI.name.text = string.Empty;
		if (this.info != null)				this.info.text = string.Empty;
		if (this.timer != null)				this.timer.text = string.Empty;
	}


	protected override void OnGamePaused (bool isPaused){
		base.OnGamePaused(isPaused);

		if (this.pauseScreen != null){
			if (isPaused){
				this.pause = (UFEScreen) GameObject.Instantiate(this.pauseScreen);
				this.pause.transform.SetParent(UFE.canvas != null ? UFE.canvas.transform : null, false);
				this.pause.OnShow();
			}else if (this.pause != null){
				if (!this.hiding){
					UFE.PlayMusic(UFE.config.selectedStage.music);
				}

				this.pause.OnHide();
				GameObject.Destroy(this.pause.gameObject);
			}
		}
	}

	protected override void OnNewAlert (string msg, CharacterInfo player){
		base.OnNewAlert (msg, player);

		// You can use this to have your own custom events when a new text alert is fired from the engine
		if (player == this.player1.character){
			if (this.player1GUI != null && this.player1GUI.alert != null && this.player1GUI.alert.text != null){
				this.player1GUI.alert.text.rectTransform.anchoredPosition = this.player1GUI.alert.initialPosition;
				this.player1GUI.alert.text.text = this.ProcessMessage(msg, UFE.GetPlayer1ControlsScript());
				this.player1AlertTimer = 2f;
			}
		}else if (player == this.player2.character){
			if (this.player2GUI != null && this.player2GUI.alert != null && this.player2GUI.alert.text != null){
				this.player2GUI.alert.text.rectTransform.anchoredPosition = this.player2GUI.alert.initialPosition;
				this.player2GUI.alert.text.text = this.ProcessMessage(msg, UFE.GetPlayer2ControlsScript());
				this.player2AlertTimer = 2f;
			}
		}else if (msg.IndexOf("Round") != -1){
			if (this.mainAlert != null && this.mainAlert.text != null){
				this.mainAlert.text.text = this.ProcessMessage(msg, null);
				this.mainAlertTimer = 2f;
			}
		}else if (msg == UFE.config.selectedLanguage.fight){
			if (this.announcer != null && !this.muteAnnouncer){
				UFE.PlaySound(this.announcer.fight);
			}

			if (this.mainAlert != null && this.mainAlert.text != null){
				this.mainAlert.text.text = this.ProcessMessage(msg, null);
				this.mainAlertTimer = 1f;
			}
		}else{
			if (this.mainAlert != null && this.mainAlert.text != null){
				this.mainAlert.text.text = this.ProcessMessage(msg, null);
				this.mainAlertTimer = 60f;
			}
		}
	}

	protected override void OnRoundBegin(int roundNumber){
		base.OnRoundBegin(roundNumber);

		if (this.player1GUI != null && this.player1GUI.alert != null && this.player1GUI.alert.text != null){
			this.player1GUI.alert.text.text = string.Empty;
		}
		
		if (this.player2GUI != null && this.player2GUI.alert != null && this.player2GUI.alert.text != null){
			this.player2GUI.alert.text.text = string.Empty;
		}

		if (roundNumber < UFE.config.roundOptions.totalRounds){
			this.OnNewAlert(UFE.config.selectedLanguage.round, null);

			if (this.announcer != null && !this.muteAnnouncer){
				if (roundNumber == 1) UFE.PlaySound(this.announcer.round1);
				if (roundNumber == 2) UFE.PlaySound(this.announcer.round2);
				if (roundNumber == 3) UFE.PlaySound(this.announcer.round3);
				if (roundNumber > 3) UFE.PlaySound(this.announcer.otherRounds);
			}
			
		}else{
			this.OnNewAlert(UFE.config.selectedLanguage.finalRound, null);

			if (this.announcer != null && !this.muteAnnouncer){
				UFE.PlaySound(this.announcer.finalRound);
			}
		}
	}
	
	protected override void OnRoundEnd (CharacterInfo winner, CharacterInfo loser){
		base.OnRoundEnd (winner, loser);

		// Find out who is the winner and who is the loser...
		int winnerPlayer = winner == this.player1.character ? 1 : 2;
		int loserPlayer = loser == this.player1.character ? 1 : 2;
		PlayerGUI winnerGUI = winnerPlayer == 1 ? this.player1GUI : this.player2GUI;
		PlayerGUI loserGUI = loserPlayer == 1 ? this.player1GUI : this.player2GUI;
		ControlsScript winnerControlsScript = UFE.GetControlsScript(winnerPlayer);

		// Then update the "Won Rounds" sprites...
		if (this.wonRounds.NotFinishedRounds == null){
			Debug.LogError("\"Not Finished Rounds\" Sprite not found! Make sure you have set the sprite correctly in the Editor");
		}else if (this.wonRounds.WonRounds == null){
			Debug.LogError("\"Won Rounds\" Sprite not found! Make sure you have set the sprite correctly in the Editor");
		}else if (this.wonRounds.LostRounds == null && this.wonRounds.VisibleImages == DefaultBattleGUI.VisibleImages.AllRounds){
			Debug.LogError("\"Lost Rounds\" Sprite not found! If you want to display Lost Rounds, make sure you have set the sprite correctly in the Editor");
		}else{
			// To calculate the target number of images, check if the "Lost Rounds" Sprite is defined or not
			int targetNumberOfImages = this.wonRounds.GetNumberOfRoundsImages();

			if (this.wonRounds.VisibleImages == DefaultBattleGUI.VisibleImages.AllRounds){
				// If the "Lost Rounds" sprite is defined, that means that we must display all won and lost rounds...
				if(
					winnerGUI != null && 
					winnerGUI.wonRoundsImages != null && 
					winnerGUI.wonRoundsImages.Length >= targetNumberOfImages
				){
					winnerGUI.wonRoundsImages[UFE.config.currentRound - 1].sprite = this.wonRounds.WonRounds;
				}else{
					Debug.LogError(
						"Player " + winnerPlayer + ": not enough \"Won Rounds\" Images not found! " +
						"Expected:" + targetNumberOfImages + " / Found: " + winnerGUI.wonRoundsImages.Length +
						"\nMake sure you have set the images correctly in the Editor"
					);
				}

				if(
					loserGUI != null && 
					loserGUI.wonRoundsImages != null && 
					loserGUI.wonRoundsImages.Length >= targetNumberOfImages
				){
					loserGUI.wonRoundsImages[UFE.config.currentRound - 1].sprite = this.wonRounds.LostRounds;
				}else{
					Debug.LogError(
						"Player " + winnerPlayer + ": not enough \"Won Rounds\" Images not found! " +
						"Expected:" + targetNumberOfImages + " / Found: " + winnerGUI.wonRoundsImages.Length +
						"\nMake sure you have set the images correctly in the Editor"
					);
				}
			}else{
				// If the "Lost Rounds" sprite is not defined, that means that we must only display won rounds...
				if(
					winnerGUI != null && 
					winnerGUI.wonRoundsImages != null && 
					winnerGUI.wonRoundsImages.Length >= winnerControlsScript.roundsWon
				){
					winnerGUI.wonRoundsImages[winnerControlsScript.roundsWon - 1].sprite = this.wonRounds.WonRounds;
				}else{
					Debug.LogError(
						"Player " + winnerPlayer + ": not enough \"Won Rounds\" Images not found! " +
						"Expected:" + targetNumberOfImages + " / Found: " + winnerGUI.wonRoundsImages.Length +
						"\nMake sure you have set the images correctly in the Editor"
					);
				}
			}
		}

		if (this.announcer != null && !this.muteAnnouncer){
			// Check if it was the last round
			if (winnerControlsScript.roundsWon > Mathf.Ceil(UFE.config.roundOptions.totalRounds/2)){
				if (winnerPlayer == 1) {
					UFE.PlaySound(this.announcer.player1Wins);
				}else{
					UFE.PlaySound(this.announcer.player2Wins);
				}
			}

			// Finally, check if we should play any AudioClip
			if (winnerControlsScript.myInfo.currentLifePoints == winnerControlsScript.myInfo.lifePoints){
				UFE.PlaySound(this.announcer.perfect);
			}
		}

		if (winnerControlsScript.myInfo.currentLifePoints == winnerControlsScript.myInfo.lifePoints){
			this.OnNewAlert(this.SetStringValues(UFE.config.selectedLanguage.perfect, winnerControlsScript), null);
		}

		if (winnerControlsScript.roundsWon > Mathf.Ceil(UFE.config.roundOptions.totalRounds / 2)){
			this.OnNewAlert(this.SetStringValues(UFE.config.selectedLanguage.victory, winnerControlsScript), null);
			UFE.PlayMusic(UFE.config.roundOptions.victoryMusic);
		}
	}

	protected override void OnTimer (float time){
		base.OnTimer (time);
		if (this.timer != null) this.timer.text = Mathf.Round(time).ToString();
	}

	protected override void OnTimeOver(){
		base.OnTimeOver();
		this.OnNewAlert(this.SetStringValues(UFE.config.selectedLanguage.timeOver, null), null);

		if (this.announcer != null && !this.muteAnnouncer){
			UFE.PlaySound(this.announcer.timeOver);
		}
	}

	protected override void OnInput (InputReferences[] inputReferences, int player){
		base.OnInput (inputReferences, player);

		// Fires whenever a player presses a button
		if(
			this.isRunning
			&& inputReferences != null
			&& inputReferences.Length > 0
            && UFE.gameMode == GameMode.TrainingRoom
            && UFE.config.trainingModeOptions.inputInfo
		){
			List<Sprite> activeIconList = new List<Sprite>();
			foreach(InputReferences inputRef in inputReferences){
				if (inputRef != null && inputRef.activeIcon != null){
					Sprite sprite = Sprite.Create(
						inputRef.activeIcon,
						new Rect(0f, 0f, inputRef.activeIcon.width, inputRef.activeIcon.height),
						new Vector2(0.5f * inputRef.activeIcon.width, 0.5f * inputRef.activeIcon.height)
					);
					
					activeIconList.Add(sprite);
				}
			}


			List<List<Image>> playerButtonPresses = null;
			if (player == 1){
				playerButtonPresses = this.player1ButtonPresses;
			}else if (player == 2){
				playerButtonPresses = this.player2ButtonPresses;
			}

			// If we have at least one icon, show those icons
			if (activeIconList.Count > 0){
				List<Image> images = new List<Image>();

				foreach (Sprite sprite in activeIconList){
					GameObject go = new GameObject("Player " + player + " - Button Press");

                    go.transform.parent = UFE.canvas != null ? UFE.canvas.transform : null;
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;

					Image image = go.AddComponent<Image>();
					image.sprite = sprite;
					images.Add(image);
				}

				playerButtonPresses.Add(images);
			}

			// If we have too many lines, remove the exceeding lines
			while (playerButtonPresses.Count >= 11){
				foreach(Image image in playerButtonPresses[0]){
					if (image != null){
						GameObject.Destroy(image.gameObject);
					}
				}

				playerButtonPresses.RemoveAt(0);
			}

			for(int i = 0; i < playerButtonPresses.Count; ++i){
				int distance = 0;

				foreach(Image image in playerButtonPresses[i]){
					if (image != null && image.rectTransform){
						float x = player == 1 ? 0f : 1f;
						float y = Mathf.Lerp(0.8f, 0.05f, (float)(i) / 11f);

						image.rectTransform.anchorMin = new Vector2(x, y);
						image.rectTransform.anchorMax = image.rectTransform.anchorMin;
						image.rectTransform.anchoredPosition = Vector2.zero;
						image.rectTransform.offsetMax = Vector2.zero;
						image.rectTransform.offsetMin = Vector2.zero;
						image.rectTransform.sizeDelta = new Vector2(image.preferredWidth * 200, image.preferredHeight * 200);

						if (player == 1){
							image.rectTransform.pivot = new Vector2(0f, 0.5f);
							image.rectTransform.anchoredPosition = new Vector2(image.rectTransform.sizeDelta.x * distance, 0f);
						}else{
							image.rectTransform.pivot = new Vector2(1f, 0.5f);
							image.rectTransform.anchoredPosition = new Vector2(-image.rectTransform.sizeDelta.x * distance, 0f);
						}

						++distance;
					}
				}
			}
		}
	}
	#endregion
	/*
	// DEBUG INFORMATION
	public virtual void LateUpdate(){
		if (this.mainAlert != null && this.mainAlert.text != null){
			this.mainAlert.text.text = "TimeScale: " + Time.timeScale;
		}
	}
	*/
}
