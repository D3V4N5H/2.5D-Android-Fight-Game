using UnityEngine;
using System;
using System.Collections;

public class BattleGUI : UFEScreen {
	#region public class definitions
	[Serializable]
	public class PlayerInfo{
		public CharacterInfo character;
		public float targetLife;
		public float totalLife;
		public int wonRounds;
	}
	#endregion

	#region protected instance properties
	protected PlayerInfo player1 = new PlayerInfo();
	protected PlayerInfo player2 = new PlayerInfo();
	protected bool isRunning;
	#endregion

	#region public override methods
	public override void DoFixedUpdate(){
		base.DoFixedUpdate ();
	}

	public override void OnShow (){
		base.OnShow ();

		/* Subscribe to UFE events:
		/* Possible Events:
		 * OnLifePointsChange(float newLifePoints, CharacterInfo player)
		 * OnNewAlert(string alertMessage, CharacterInfo player)
		 * OnHit(MoveInfo move, CharacterInfo hitter)
		 * OnMove(MoveInfo move, CharacterInfo player)
		 * OnRoundEnds(CharacterInfo winner, CharacterInfo loser)
		 * OnRoundBegins(int roundNumber)
		 * OnGameEnds(CharacterInfo winner, CharacterInfo loser)
		 * OnGameBegins(CharacterInfo player1, CharacterInfo player2, StageOptions stage)
		 * 
		 * usage:
		 * UFE.OnMove += YourFunctionHere;
		 * .
		 * .
		 * void YourFunctionHere(T param1, T param2){...}
		 * 
		 * The following code bellow show more usage examples
		 */

		UFE.OnGameBegin += this.OnGameBegin;
		UFE.OnGameEnds += this.OnGameEnd;
		UFE.OnGamePaused += this.OnGamePaused;
		UFE.OnRoundBegins += this.OnRoundBegin;
		UFE.OnRoundEnds += this.OnRoundEnd;
		UFE.OnLifePointsChange += this.OnLifePointsChange;
		UFE.OnNewAlert += this.OnNewAlert;
		UFE.OnHit += this.OnHit;
		UFE.OnMove += this.OnMove;
		UFE.OnTimer += this.OnTimer;
		UFE.OnTimeOver += this.OnTimeOver;
		UFE.OnInput += this.OnInput;
	}

	public override void OnHide (){
		UFE.OnGameBegin -= this.OnGameBegin;
		UFE.OnGameEnds -= this.OnGameEnd;
		UFE.OnGamePaused -= this.OnGamePaused;
		UFE.OnRoundBegins -= this.OnRoundBegin;
		UFE.OnRoundEnds -= this.OnRoundEnd;
		UFE.OnLifePointsChange -= this.OnLifePointsChange;
		UFE.OnNewAlert -= this.OnNewAlert;
		UFE.OnHit -= this.OnHit;
		UFE.OnMove -= this.OnMove;
		UFE.OnTimer -= this.OnTimer;
		UFE.OnTimeOver -= this.OnTimeOver;
		UFE.OnInput -= this.OnInput;

		base.OnHide ();
	}
	#endregion

	#region protected instance methods
	protected virtual void OnGameBegin(CharacterInfo player1, CharacterInfo player2, StageOptions stage){
		this.player1.character = player1;
		this.player1.targetLife = player1.lifePoints;
		this.player1.totalLife = player1.lifePoints;
		this.player1.wonRounds = 0;

		this.player2.character = player2;
		this.player2.targetLife = player2.lifePoints;
		this.player2.totalLife = player2.lifePoints;
		this.player2.wonRounds = 0;

		UFE.PlayMusic(stage.music);
		this.isRunning = true;
	}

	protected virtual void OnGameEnd(CharacterInfo winner, CharacterInfo loser){
		this.isRunning = false;

		if (UFE.gameMode == GameMode.VersusMode){
			UFE.StartVersusModeAfterBattleScreen();
		}else if (UFE.gameMode == GameMode.StoryMode){
			if (winner == this.player1.character){
				UFE.WonStoryModeBattle();
			}else{
				UFE.StartStoryModeContinueScreen();
			}
		}else{
			UFE.StartMainMenuScreen();
		}
	}

	protected virtual void OnGamePaused(bool isPaused){

	}

	protected virtual void OnRoundBegin(int roundNumber){

	}

	protected virtual void OnRoundEnd(CharacterInfo winner, CharacterInfo loser){
		// TODO: we should use the player number instead of the character info because both players could use the same character
		//++this.player1WonRounds;
		//++this.playe21WonRounds;
	}

	protected virtual void OnLifePointsChange(float newFloat, CharacterInfo player){
		// You can use this to have your own custom events when a player's life points changes
		// TODO: we should use the player number instead of the character info because both players could use the same character
	}

	protected virtual void OnNewAlert(string msg, CharacterInfo player){
		// You can use this to have your own custom events when a new text alert is fired from the engine
		// TODO: we should use the player number instead of the character info because both players could use the same character
	}

	protected virtual void OnHit(HitBox strokeHitBox, MoveInfo move, CharacterInfo player){
		// You can use this to have your own custom events when a character gets hit
		// TODO: we should use the player number instead of the character info because both players could use the same character
	}

	protected virtual void OnMove(MoveInfo move, CharacterInfo player){
		// Fires when a player successfully executes a move
	}

	protected virtual void OnTimer(float time){

	}

	protected virtual void OnTimeOver(){
		
	}

	protected virtual void OnInput(InputReferences[] inputReferences, int player){

	}
	#endregion
}
