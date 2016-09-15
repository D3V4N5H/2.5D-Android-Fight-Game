using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using System.Net;
[RequireComponent(typeof(EventSystem))]
public class UFE : MonoBehaviour {
	#region public instance properties
	public GlobalInfo UFE_Config;
	#endregion

	#region public event definitions
	public delegate void MeterHandler(float newFloat, CharacterInfo player);
	public static event MeterHandler OnLifePointsChange;

	public delegate void IntHandler(int newInt);
	public static event IntHandler OnRoundBegins;

	public delegate void StringHandler(string newString, CharacterInfo player);
	public static event StringHandler OnNewAlert;
	
	public delegate void HitHandler(HitBox strokeHitBox, MoveInfo move, CharacterInfo player);
	public static event HitHandler OnHit;

	public delegate void MoveHandler(MoveInfo move, CharacterInfo player);
	public static event MoveHandler OnMove;
	
	public delegate void GameBeginHandler(CharacterInfo player1, CharacterInfo player2, StageOptions stage);
	public static event GameBeginHandler OnGameBegin;

	public delegate void GameEndsHandler(CharacterInfo winner, CharacterInfo loser);
	public static event GameEndsHandler OnGameEnds;
	public static event GameEndsHandler OnRoundEnds;

	public delegate void GamePausedHandler(bool isPaused);
	public static event GamePausedHandler OnGamePaused;

	public delegate void ScreenChangedHandler(UFEScreen previousScreen, UFEScreen newScreen);
	public static event ScreenChangedHandler OnScreenChanged;

	public delegate void StoryModeHandler(CharacterInfo character);
	public static event StoryModeHandler OnStoryModeStarted;
	public static event StoryModeHandler OnStoryModeCompleted;

	public delegate void TimerHandler(float time);
	public static event TimerHandler OnTimer;

	public delegate void TimeOverHandler();
	public static event TimeOverHandler OnTimeOver;

	public delegate void InputHandler(InputReferences[] inputReferences, int player);
	public static event InputHandler OnInput;
	#endregion

	#region public class properties
	//-----------------------------------------------------------------------------------------------------------------
	// GUI
	//-----------------------------------------------------------------------------------------------------------------
	public static Canvas canvas{get; protected set;}
	public static CanvasGroup canvasGroup{get; protected set;}
	public static EventSystem eventSystem{get; protected set;}
	public static GraphicRaycaster graphicRaycaster{get; protected set;}
	public static TouchInputModule touchInputModule{get; protected set;}
	//public static StandaloneInputModule standaloneInputModule{get; protected set;}
	//-----------------------------------------------------------------------------------------------------------------
	protected static readonly string MusicEnabledKey = "MusicEnabled";
	protected static readonly string MusicVolumeKey = "MusicVolume";
	protected static readonly string SoundsEnabledKey = "SoundsEnabled";
	protected static readonly string SoundsVolumeKey = "SoundsVolume";
	protected static readonly string DifficultyLevelKey = "DifficultyLevel";
	protected static readonly string DebugModeKey = "DebugMode";
	//-----------------------------------------------------------------------------------------------------------------

	public static GameMode gameMode = GameMode.None;
	public static GlobalInfo config;
	public static bool freeCamera;
	public static bool normalizedCam = true;
	public static bool debug = true;
	public static GUIText debugger1;
	public static GUIText debugger2;

	public static bool isAiAddonInstalled {get; set;}
	public static bool isCInputInstalled {get; set;}
	public static bool isControlFreakInstalled {get; set;}
	public static bool isNetworkAddonInstalled {get; set;}
	public static long currentNetworkFrame {get; protected set;}
	public static UFEScreen currentScreen{get; protected set;}
	public static UFEScreen battleGUI{get; protected set;}
	public static GameObject gameEngine{get; protected set;}
	public static bool gameRunning{get; protected set;}
	public static bool synchronizedRandomSeed{get; protected set;}
	#endregion

	#region private class properties
	private static float timer;
	private static int intTimer;
	private static bool pauseTimer;
	private static bool newRoundCasted;
	private static CameraScript cameraScript;
	private static AudioSource musicAudioSource;
	private static AudioSource soundsAudioSource;

	private static UFEController p1Controller;
	private static UFEController p2Controller;
	private static UFEController localPlayerController;
	private static UFEController remotePlayerController;
	private static ControlsScript p1ControlsScript;
	private static ControlsScript p2ControlsScript;

    private static List<object> memoryDump = new List<object>();

	private static GameObject controlFreakPrefab;
	private static List<DelayedAction> delayedLocalActions = new List<DelayedAction>();
	private static List<DelayedAction> delayedSynchronizedActions = new List<DelayedAction>();
	private static StoryModeInfo storyMode = new StoryModeInfo();

	private static List<string> unlockedCharactersInStoryMode = new List<string>();
	private static List<string> unlockedCharactersInVersusMode = new List<string>();

	//private int uiLayer;
	//private int uiMask;

	private static bool closing = false;
	private static bool disconnecting = false;

	//-----------------------------------------------------------------------------------------------------------------
	// Required for the Story Mode: if the player lost its previous battle, 
	// he needs to fight the same opponent again, not the next opponent.
	//-----------------------------------------------------------------------------------------------------------------
	private static bool player1WonLastBattle;
	private static int lastStageIndex;
	#endregion


	#region public class methods: Delay the execution of a method maintaining synchronization between clients
	public static void DelayLocalAction(Action action, float seconds = 0f){
		if (Time.fixedDeltaTime > 0f){
			UFE.DelayLocalAction(action, Mathf.FloorToInt(seconds * config.fps));
		}else{
			UFE.DelayLocalAction(action, 1);
		}
	}

	public static void DelayLocalAction(Action action, int steps){
		UFE.DelayLocalAction(new DelayedAction(action, steps));
	}

	public static void DelayLocalAction(DelayedAction delayedAction){
		UFE.delayedLocalActions.Add(delayedAction);
	}

	public static void DelaySynchronizedAction(Action action, float seconds = 0f){
		if (Time.fixedDeltaTime > 0f){
			UFE.DelaySynchronizedAction(action, Mathf.FloorToInt(seconds * config.fps));
		}else{
			UFE.DelaySynchronizedAction(action, 1);
		}
	}

	public static void DelaySynchronizedAction(Action action, int steps){
		UFE.DelaySynchronizedAction(new DelayedAction(action, steps));
	}

	public static void DelaySynchronizedAction(DelayedAction delayedAction){
		UFE.delayedSynchronizedActions.Add(delayedAction);
	}
	
	
	public static bool FindDelaySynchronizedAction(Action action){
		foreach (DelayedAction delayedAction in UFE.delayedSynchronizedActions){
			if (action == delayedAction.action) return true;
		}
		return false;
	}

	public static bool FindAndUpdateDelaySynchronizedAction(Action action, float seconds){
		foreach (DelayedAction delayedAction in UFE.delayedSynchronizedActions){
			if (action == delayedAction.action) {
				delayedAction.steps = Mathf.FloorToInt(seconds * config.fps);
				return true;
			}
		}
		return false;
	}

	#endregion

	#region public class methods: Audio related methods
	public static bool GetMusic(){
		return config.music;
	}

	public static AudioClip GetMusicClip(){
		return UFE.musicAudioSource.clip;
	}
	
	public static bool GetSoundFX(){
		return config.soundfx;
	}

	public static float GetMusicVolume(){
		if (UFE.config != null) return config.musicVolume;
		return 1f;
	}

	public static float GetSoundFXVolume(){
		if (UFE.config != null) return UFE.config.soundfxVolume;
		return 1f;
	}

	public static void InitializeAudioSystem(){
		Camera cam = Camera.main;

		// Create the AudioSources required for the music and sound effects
		UFE.musicAudioSource = cam.GetComponent<AudioSource>();
		if (UFE.musicAudioSource == null){
			UFE.musicAudioSource = cam.gameObject.AddComponent<AudioSource>();
		}

		UFE.musicAudioSource.loop = true;
		UFE.musicAudioSource.playOnAwake = false;
		UFE.musicAudioSource.volume = config.musicVolume;


		UFE.soundsAudioSource = cam.gameObject.AddComponent<AudioSource>();
		UFE.soundsAudioSource.loop = false;
		UFE.soundsAudioSource.playOnAwake = false;
		UFE.soundsAudioSource.volume = 1f;
	}

	public static bool IsPlayingMusic(){
		if (UFE.musicAudioSource.clip != null) return UFE.musicAudioSource.isPlaying;
		return false;
	}

	public static bool IsMusicLooped(){
		return UFE.musicAudioSource.loop;
	}

	public static bool IsPlayingSoundFX(){
		return false;
	}

	public static void LoopMusic(bool loop){
		UFE.musicAudioSource.loop = loop;
	}

	public static void PlayMusic(){
		if (config.music && !UFE.IsPlayingMusic() && UFE.musicAudioSource.clip != null){
			UFE.musicAudioSource.Play();
		}
	}

	public static void PlayMusic(AudioClip music){
		if (music != null){
			AudioClip oldMusic = UFE.GetMusicClip();

			if (music != oldMusic){
				UFE.musicAudioSource.clip = music;
			}

			if (config.music && (music != oldMusic || !UFE.IsPlayingMusic())){
				UFE.musicAudioSource.Play();
			}
		}
	}

	public static void PlaySound(IList<AudioClip> sounds){
		if (sounds.Count > 0){
			UFE.PlaySound(sounds[UnityEngine.Random.Range(0, sounds.Count)]);
		}
	}
	
	public static void PlaySound(AudioClip soundFX){
		UFE.PlaySound(soundFX, UFE.GetSoundFXVolume());
	}

	public static void PlaySound(AudioClip soundFX, float volume){
		if (config.soundfx && soundFX != null && UFE.soundsAudioSource != null){
			UFE.soundsAudioSource.PlayOneShot(soundFX, volume);
		}
	}
	
	public static void SetMusic(bool on){
		bool isPlayingMusic = UFE.IsPlayingMusic();
		UFE.config.music = on;

		if (on && !isPlayingMusic)		UFE.PlayMusic();
		else if (!on && isPlayingMusic)	UFE.StopMusic();

		PlayerPrefs.SetInt(UFE.MusicEnabledKey, on ? 1 : 0);
		PlayerPrefs.Save();
	}
	
	public static void SetSoundFX(bool on){
		UFE.config.soundfx = on;
		PlayerPrefs.SetInt(UFE.SoundsEnabledKey, on ? 1 : 0);
		PlayerPrefs.Save();
	}
	
	public static void SetMusicVolume(float volume){
		if (UFE.config != null) UFE.config.musicVolume = volume;
		if (UFE.musicAudioSource != null) UFE.musicAudioSource.volume = volume;

		PlayerPrefs.SetFloat(UFE.MusicVolumeKey, volume);
		PlayerPrefs.Save();
	}

	public static void SetSoundFXVolume(float volume){
		if (UFE.config != null) UFE.config.soundfxVolume = volume;
		PlayerPrefs.SetFloat(UFE.SoundsVolumeKey, volume);
		PlayerPrefs.Save();
	}
	
	public static void SetAIEngine(AIEngine engine){
		UFE.config.aiOptions.engine = engine;
	}
	
	public static AIEngine GetAIEngine(){
		return UFE.config.aiOptions.engine;
	}
	
	public static void SetDebugMode(bool flag){
		UFE.config.debugOptions.debugMode = flag;
		debugger1.enabled = flag;
		debugger2.enabled = flag;
	}

	public static void SetAIDifficulty(AIDifficultyLevel difficulty){
		foreach(AIDifficultySettings difficultySettings in UFE.config.aiOptions.difficultySettings){
			if (difficultySettings.difficultyLevel == difficulty) {
				UFE.SetAIDifficulty(difficultySettings);
				break;
			}
		}
	}

	public static void SetAIDifficulty(AIDifficultySettings difficulty){
		UFE.config.aiOptions.selectedDifficulty = difficulty;
		UFE.config.aiOptions.selectedDifficultyLevel = difficulty.difficultyLevel;

		for (int i = 0; i < UFE.config.aiOptions.difficultySettings.Length; ++i){
			if (difficulty == UFE.config.aiOptions.difficultySettings[i]){
				PlayerPrefs.SetInt(UFE.DifficultyLevelKey, i);
				PlayerPrefs.Save();
				break;
			}
		}
	}

	public static void StopMusic(){
		if (UFE.musicAudioSource.clip != null) UFE.musicAudioSource.Stop();
	}

	public static void StopSounds(){
		UFE.soundsAudioSource.Stop();
	}
	#endregion

	#region public class methods: Story Mode related methods
	public static CharacterStory GetCharacterStory(CharacterInfo character){
		if (!UFE.config.storyMode.useSameStoryForAllCharacters){
			for (int i = 0; i < UFE.config.characters.Length; ++i){
				int index = UFE.config.storyMode.selectableCharactersInStoryMode.IndexOf(i);

				if (UFE.config.characters[i] != null && index >= 0){
					return UFE.config.storyMode.characterStories[index];
				}
			}
		}
		
		return UFE.config.storyMode.defaultStory;
	}
	

	public static AIDifficultySettings GetAIDifficulty(){
		return UFE.config.aiOptions.selectedDifficulty;
	}

	public static void SetFuzzyAI(int player, CharacterInfo character){
		if (UFE.isAiAddonInstalled){
			UFEController controller = UFE.GetController(player);
			
			if (controller != null && controller.isCPU){
				AbstractInputController cpu = controller.cpuController;
				
				if (cpu != null){
					MethodInfo method = cpu.GetType().GetMethod(
						"SetAIInformation", 
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy,
						null,
						new Type[]{typeof(ScriptableObject)},
					null
					);
					
					if (method != null){
						if (character != null && character.aiInstructionsSet != null && character.aiInstructionsSet.Length > 0){
							if (UFE.config.aiOptions.selectedDifficulty.startupBehavior == AIBehavior.Any){
								method.Invoke(cpu, new object[]{character.aiInstructionsSet[0].aiInfo});
							}else{
								ScriptableObject selectedAIInfo = character.aiInstructionsSet[0].aiInfo;
								foreach(AIInstructionsSet instructionSet in character.aiInstructionsSet){
									if (instructionSet.behavior == UFE.config.aiOptions.selectedDifficulty.startupBehavior){
										selectedAIInfo = instructionSet.aiInfo;
										break;
									}
								}
								method.Invoke(cpu, new object[]{selectedAIInfo});
							}
						}else{
							method.Invoke(cpu, new object[]{null});
						}
					}
				}
			}
		}
	}

	#endregion

	#region public class methods: GUI Related methods
	public static BattleGUI GetBattleGUI(){
		return UFE.config.gameGUI.battleGUI;
	}

	public static CharacterSelectionScreen GetCharacterSelectionScreen(){
		return UFE.config.gameGUI.characterSelectionScreen;
	}

	public static ConnectionLostScreen GetConnectionLostScreen(){
		return UFE.config.gameGUI.connectionLostScreen;
	}

	public static CreditsScreen GetCreditsScreen(){
		return UFE.config.gameGUI.creditsScreen;
	}

	public static HostGameScreen GetHostGameScreen(){
		return UFE.config.gameGUI.hostGameScreen;
	}

	public static IntroScreen GetIntroScreen(){
		return UFE.config.gameGUI.introScreen;
	}

	public static JoinGameScreen GetJoinGameScreen(){
		return UFE.config.gameGUI.joinGameScreen;
	}

	public static LoadingBattleScreen GetLoadingBattleScreen(){
		return UFE.config.gameGUI.loadingBattleScreen;
	}

	public static MainMenuScreen GetMainMenuScreen(){
		return UFE.config.gameGUI.mainMenuScreen;
	}

	public static NetworkGameScreen GetNetworkGameScreen(){
		return UFE.config.gameGUI.networkGameScreen;
	}

	public static OptionsScreen GetOptionsScreen(){
		return UFE.config.gameGUI.optionsScreen;
	}

	public static StageSelectionScreen GetStageSelectionScreen(){
		return UFE.config.gameGUI.stageSelectionScreen;
	}

	public static StoryModeScreen GetStoryModeCongratulationsScreen(){
		return UFE.config.gameGUI.storyModeCongratulationsScreen;
	}

	public static StoryModeContinueScreen GetStoryModeContinueScreen(){
		return UFE.config.gameGUI.storyModeContinueScreen;
	}

	public static StoryModeScreen GetStoryModeGameOverScreen(){
		return UFE.config.gameGUI.storyModeGameOverScreen;
	}

	public static VersusModeAfterBattleScreen GetVersusModeAfterBattleScreen(){
		return UFE.config.gameGUI.versusModeAfterBattleScreen;
	}

	public static VersusModeScreen GetVersusModeScreen(){
		return UFE.config.gameGUI.versusModeScreen;
	}

	public static void HideScreen(UFEScreen screen){
		if (screen != null){
			screen.OnHide();
			GameObject.Destroy(screen.gameObject);
		}
	}
	
	public static void ShowScreen(UFEScreen screen, Action nextScreenAction = null){
		if (screen != null){
			if (UFE.OnScreenChanged != null){
				UFE.OnScreenChanged(UFE.currentScreen, screen);
			}

			UFE.currentScreen = (UFEScreen) GameObject.Instantiate(screen);
			UFE.currentScreen.transform.SetParent(UFE.canvas != null ? UFE.canvas.transform : null, false);

			StoryModeScreen storyModeScreen = UFE.currentScreen as StoryModeScreen;
			if (storyModeScreen != null){
				storyModeScreen.nextScreenAction = nextScreenAction;
			}

			UFE.currentScreen.OnShow ();
		}
	}

	public static void Quit(){
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

	public static void StartCharacterSelectionScreen(){
		UFE.StartCharacterSelectionScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartCharacterSelectionScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartCharacterSelectionScreen(fadeTime/2); });
	}

	public static void StartCpuVersusCpu(){
		UFE.StartCpuVersusCpu(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartCpuVersusCpu(float fadeTime){
		UFE.gameMode = GameMode.VersusMode;
		UFE.SetCPU(1, true);
		UFE.SetCPU(2, true);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	public static void StartConnectionLostScreen(){
		UFE.StartConnectionLostScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartConnectionLostScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartConnectionLostScreen(fadeTime/2); });
	}

	public static void StartCreditsScreen(){
		UFE.StartCreditsScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartCreditsScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartCreditsScreen(fadeTime/2); });
	}

	public static void StartGame(){
		UFE.StartGame(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartGame(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartGame(fadeTime/2); });
	}

	public static void StartHostGameScreen(){
		UFE.StartHostGameScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartHostGameScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2f, 0, () => { UFE._StartHostGameScreen(fadeTime/2f); });
	}

	public static void StartIntroScreen(){
		UFE.StartIntroScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartIntroScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2f, 0, () => { UFE._StartIntroScreen(fadeTime/2f); });
	}

	public static void StartJoinGameScreen(){
		UFE.StartJoinGameScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartJoinGameScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2f, 0, () => { UFE._StartJoinGameScreen(fadeTime/2f); });
	}

	public static void StartLoadingBattleScreen(){
		UFE.StartLoadingBattleScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartLoadingBattleScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2f, 0, () => { UFE._StartLoadingBattleScreen(fadeTime/2f); });
	}

	public static void StartMainMenuScreen(){
		UFE.StartMainMenuScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartMainMenuScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2f, 0, () => { UFE._StartMainMenuScreen(fadeTime/2f); });
	}

	public static void StartNetworkGameScreen(){
		//UFE.StartNetworkGameScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartNetworkGameScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2f, 0, () => { UFE._StartNetworkGameScreen(fadeTime/2f); });
	}

	public static void StartOptionsScreen(){
		UFE.StartOptionsScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartOptionsScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2f, 0, () => { UFE._StartOptionsScreen(fadeTime/2f); });
	}

	public static void StartPlayerVersusPlayer(){
		UFE.StartPlayerVersusPlayer(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartPlayerVersusPlayer(float fadeTime){
		UFE.gameMode = GameMode.VersusMode;
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, false);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	public static void StartPlayerVersusCpu(){
		UFE.StartPlayerVersusCpu(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartPlayerVersusCpu(float fadeTime){
		UFE.gameMode = GameMode.VersusMode;
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, true);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	public static void StartNetworkGame(float fadeTime, int localPlayer){
		UFE.disconnecting = false;
		UFE.synchronizedRandomSeed = false;
		Application.runInBackground = true;

		UFE.localPlayerController.Initialize(UFE.p1Controller.inputReferences, UFE.config.networkOptions.minFrameDelay);
		UFE.localPlayerController.humanController = p1Controller.humanController;
		UFE.localPlayerController.cpuController = p1Controller.cpuController;
		UFE.remotePlayerController.Initialize(UFE.p2Controller.inputReferences);

		if (localPlayer == 1){
			UFE.localPlayerController.player = 1;
			UFE.remotePlayerController.player = 2;
		}else{
			UFE.localPlayerController.player = 2;
			UFE.remotePlayerController.player = 1;
		}

		UFE.gameMode = GameMode.NetworkGame;
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, false);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	public static void StartStageSelectionScreen(){
		UFE.StartStageSelectionScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartStageSelectionScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartStageSelectionScreen(fadeTime/2); });
	}

	public static void StartStoryMode(){
		UFE.StartStoryMode(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartStoryMode(float fadeTime){
		//-------------------------------------------------------------------------------------------------------------
		// Required for loading the first combat correctly.
		UFE.player1WonLastBattle = true; 
		//-------------------------------------------------------------------------------------------------------------
		UFE.gameMode = GameMode.StoryMode;
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, true);
		UFE.storyMode.characterStory = null;
		UFE.storyMode.canFightAgainstHimself = UFE.config.storyMode.canCharactersFightAgainstThemselves;
		UFE.storyMode.currentGroup = -1;
		UFE.storyMode.currentBattle = -1;
		UFE.storyMode.currentBattleInformation = null;
		UFE.storyMode.defeatedOpponents.Clear();
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	public static void StartStoryModeBattle(){
		UFE.StartStoryModeBattle(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartStoryModeBattle(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartStoryModeBattle(fadeTime/2); });
	}

	public static void StartStoryModeCongratulationsScreen(){
		UFE.StartStoryModeCongratulationsScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartStoryModeCongratulationsScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartStoryModeCongratulationsScreen(fadeTime/2); });
	}

	public static void StartStoryModeContinueScreen(){
		UFE.StartStoryModeContinueScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartStoryModeContinueScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartStoryModeContinueScreen(fadeTime/2); });
	}

	public static void StartStoryModeConversationAfterBattleScreen(UFEScreen conversationScreen){
		UFE.StartStoryModeConversationAfterBattleScreen(conversationScreen, UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartStoryModeConversationAfterBattleScreen(UFEScreen conversationScreen, float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartStoryModeConversationAfterBattleScreen(conversationScreen, fadeTime/2); });
	}

	public static void StartStoryModeConversationBeforeBattleScreen(UFEScreen conversationScreen){
		UFE.StartStoryModeConversationBeforeBattleScreen(conversationScreen, UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartStoryModeConversationBeforeBattleScreen(UFEScreen conversationScreen, float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartStoryModeConversationBeforeBattleScreen(conversationScreen, fadeTime/2); });
	}

	public static void StartStoryModeEndingScreen(){
		UFE.StartStoryModeEndingScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartStoryModeEndingScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartStoryModeEndingScreen(fadeTime/2); });
	}

	public static void StartStoryModeGameOverScreen(){
		UFE.StartStoryModeGameOverScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartStoryModeGameOverScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartStoryModeGameOverScreen(fadeTime/2); });
	}

	public static void StartStoryModeOpeningScreen(){
		UFE.StartStoryModeOpeningScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartStoryModeOpeningScreen(float fadeTime){
		// First, retrieve the character story, so we can find the opening associated to this player
		UFE.storyMode.characterStory = UFE.GetCharacterStory(UFE.GetPlayer1());
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartStoryModeOpeningScreen(fadeTime/2); });
	}

	public static void StartTrainingMode(){
		UFE.StartTrainingMode(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartTrainingMode(float fadeTime){
		UFE.gameMode = GameMode.TrainingRoom;
		UFE.SetCPU(1, false);
		UFE.SetCPU(2, false);
		UFE.StartCharacterSelectionScreen(fadeTime);
	}

	public static void StartVersusModeAfterBattleScreen(){
		UFE.StartVersusModeAfterBattleScreen(0f);
	}

	public static void StartVersusModeAfterBattleScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartVersusModeAfterBattleScreen(fadeTime/2); });
	}

	public static void StartVersusModeScreen(){
		UFE.StartVersusModeScreen(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void StartVersusModeScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, false, fadeTime/2, 0, () => { UFE._StartVersusModeScreen(fadeTime/2); });
	}

	public static void WonStoryModeBattle(){
		UFE.WonStoryModeBattle(UFE.config.gameGUI.defaultFadeDuration);
	}

	public static void WonStoryModeBattle(float fadeTime){
		UFE.storyMode.defeatedOpponents.Add(UFE.storyMode.currentBattleInformation.opponentCharacterIndex);
		UFE.StartStoryModeConversationAfterBattleScreen(UFE.storyMode.currentBattleInformation.conversationAfterBattle, fadeTime);
	}
	#endregion

	#region public class methods: Language
	public static void SetLanguage(){
		foreach(LanguageOptions languageOption in config.languages){
			if (languageOption.defaultSelection){
				config.selectedLanguage = languageOption;
				return;
			}
		}
	}

	public static void SetLanguage(string language){
		foreach(LanguageOptions languageOption in config.languages){
			if (language == languageOption.languageName){
				config.selectedLanguage = languageOption;
				return;
			}
		}
	}
	#endregion

	#region public class methods: Input Related methods
	public static bool GetCPU(int player){
		UFEController controller = UFE.GetController(player);
		if (controller != null){
			return controller.isCPU;
		}
		return false;
	}

	public static string GetInputReference(ButtonPress button, InputReferences[] inputReferences){
		foreach(InputReferences inputReference in inputReferences){
			if (inputReference.engineRelatedButton == button) return inputReference.inputButtonName;
		}
		return null;
	}
	
	public static string GetInputReference(InputType inputType, InputReferences[] inputReferences){
		foreach(InputReferences inputReference in inputReferences){
			if (inputReference.inputType == inputType) return inputReference.inputButtonName;
		}
		return null;
	}

	public static UFEController GetPlayer1Controller(){
		if (UFE.isNetworkAddonInstalled && UFE.isConnected){
			if (Network.peerType == NetworkPeerType.Server){
				return UFE.localPlayerController;
			}else{
				return UFE.remotePlayerController;
			}
		}
		return UFE.p1Controller;
	}
	
	public static UFEController GetPlayer2Controller(){
		if (UFE.isNetworkAddonInstalled && UFE.isConnected){
			if (Network.peerType == NetworkPeerType.Server){
				return UFE.remotePlayerController;
			}else{
				return UFE.localPlayerController;
			}
		}
		return UFE.p2Controller;
	}
	
	public static UFEController GetController(int player){
		if		(player == 1)	return UFE.GetPlayer1Controller();
		else if (player == 2)	return UFE.GetPlayer2Controller();
		else					return null;
	}
	
	public static int GetLocalPlayer(){
		if		(UFE.localPlayerController == UFE.GetPlayer1Controller())	return 1;
		else if	(UFE.localPlayerController == UFE.GetPlayer2Controller())	return 2;
		else																return -1;
	}
	
	public static int GetRemotePlayer(){
		if		(UFE.remotePlayerController == UFE.GetPlayer1Controller())	return 1;
		else if	(UFE.remotePlayerController == UFE.GetPlayer2Controller())	return 2;
		else																return -1;
	}

	public static void SetAI(int player, CharacterInfo character){
		if (UFE.isAiAddonInstalled){
			UFEController controller = UFE.GetController(player);
			
			if (controller != null && controller.isCPU){
				AbstractInputController cpu = controller.cpuController;
				
				if (cpu != null){
					MethodInfo method = cpu.GetType().GetMethod(
						"SetAIInformation", 
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy,
						null,
						new Type[]{typeof(ScriptableObject)},
					null
					);
					
					if (method != null){
						if (character != null && character.aiInstructionsSet != null && character.aiInstructionsSet.Length > 0){
							method.Invoke(cpu, new object[]{character.aiInstructionsSet[0].aiInfo});
						}else{
							method.Invoke(cpu, new object[]{null});
						}
					}
				}
			}
		}
	}

	public static void SetCPU(int player, bool cpuToggle){
		UFEController controller = UFE.GetController(player);
		if (controller != null){
			controller.isCPU = cpuToggle;
		}
	}
	#endregion

	#region public class methods: methods related to the character selection
	public static CharacterInfo GetPlayer(int player){
		if (player == 1){
			return UFE.GetPlayer1();
		}else if (player == 2){
			return UFE.GetPlayer2();
		}
		return null;
	}
	
	public static CharacterInfo GetPlayer1(){
		return config.player1Character;
	}
	
	public static CharacterInfo GetPlayer2(){
		return config.player2Character;
	}

	public static CharacterInfo[] GetStoryModeSelectableCharacters(){
		List<CharacterInfo> characters = new List<CharacterInfo>();

		for (int i = 0; i < UFE.config.characters.Length; ++i){
			if(
				UFE.config.characters[i] != null 
				&& 
				(
					UFE.config.storyMode.selectableCharactersInStoryMode.Contains(i) || 
					UFE.unlockedCharactersInStoryMode.Contains(UFE.config.characters[i].characterName)
				)
			){
				characters.Add(UFE.config.characters[i]);
			}
		}
		
		return characters.ToArray();
	}

	public static CharacterInfo[] GetTrainingRoomSelectableCharacters(){
		List<CharacterInfo> characters = new List<CharacterInfo>();
		
		for (int i = 0; i < UFE.config.characters.Length; ++i){
			// If the character is selectable on Story Mode or Versus Mode,
			// then the character should be selectable on Training Room...
			if(
				UFE.config.characters[i] != null 
				&& 
				(
					UFE.config.storyMode.selectableCharactersInStoryMode.Contains(i) || 
					UFE.config.storyMode.selectableCharactersInVersusMode.Contains(i) || 
					UFE.unlockedCharactersInStoryMode.Contains(UFE.config.characters[i].characterName) ||
					UFE.unlockedCharactersInVersusMode.Contains(UFE.config.characters[i].characterName)
				)
			){
				characters.Add(UFE.config.characters[i]);
			}
		}
		
		return characters.ToArray();
	}
	
	public static CharacterInfo[] GetVersusModeSelectableCharacters(){
		List<CharacterInfo> characters = new List<CharacterInfo>();
		
		for (int i = 0; i < UFE.config.characters.Length; ++i){
			if(
				UFE.config.characters[i] != null && 
				(
					UFE.config.storyMode.selectableCharactersInVersusMode.Contains(i) || 
					UFE.unlockedCharactersInVersusMode.Contains(UFE.config.characters[i].characterName)
				)
			){
				characters.Add(UFE.config.characters[i]);
			}
		}
		
		return characters.ToArray();
	}

	public static void SetPlayer(int player, CharacterInfo info){
		if (player == 1){
			config.player1Character = info;
		}else if (player == 2){
			config.player2Character = info;
		}
	}

	public static void SetPlayer1(CharacterInfo player1){
		config.player1Character = player1;
	}

	public static void SetPlayer2(CharacterInfo player2){
		config.player2Character = player2;
	}

	public static void LoadUnlockedCharacters(){
		UFE.unlockedCharactersInStoryMode.Clear();
		string value = PlayerPrefs.GetString("UCSM", null);

		if (!string.IsNullOrEmpty(value)){
			string[] characters = value.Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);
			foreach (string character in characters){
				unlockedCharactersInStoryMode.Add(character);
			}
		}


		UFE.unlockedCharactersInVersusMode.Clear();
		value = PlayerPrefs.GetString("UCVM", null);
		
		if (!string.IsNullOrEmpty(value)){
			string[] characters = value.Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);
			foreach (string character in characters){
				unlockedCharactersInVersusMode.Add(character);
			}
		}
	}

	public static void SaveUnlockedCharacters(){
		StringBuilder sb = new StringBuilder();
		foreach (string characterName in UFE.unlockedCharactersInStoryMode){
			if (!string.IsNullOrEmpty(characterName)){
				if (sb.Length > 0){
					sb.AppendLine();
				}
				sb.Append(characterName);
			}
		}
		PlayerPrefs.SetString("UCSM", sb.ToString());

		sb = new StringBuilder();
		foreach (string characterName in UFE.unlockedCharactersInVersusMode){
			if (!string.IsNullOrEmpty(characterName)){
				if (sb.Length > 0){
					sb.AppendLine();
				}
				sb.Append(characterName);
			}
		}
		PlayerPrefs.SetString("UCVM", sb.ToString());
		PlayerPrefs.Save();
	}

	public static void RemoveUnlockedCharacterInStoryMode(CharacterInfo character){
		if (character != null && !string.IsNullOrEmpty(character.characterName)){
			UFE.unlockedCharactersInStoryMode.Remove(character.characterName);
		}
		
		UFE.SaveUnlockedCharacters();
	}

	public static void RemoveUnlockedCharacterInVersusMode(CharacterInfo character){
		if (character != null && !string.IsNullOrEmpty(character.characterName)){
			UFE.unlockedCharactersInVersusMode.Remove(character.characterName);
		}
		
		UFE.SaveUnlockedCharacters();
	}

	public static void RemoveUnlockedCharactersInStoryMode(){
		UFE.unlockedCharactersInStoryMode.Clear();
		UFE.SaveUnlockedCharacters();
	}
	
	public static void RemoveUnlockedCharactersInVersusMode(){
		UFE.unlockedCharactersInVersusMode.Clear();
		UFE.SaveUnlockedCharacters();
	}

	public static void UnlockCharacterInStoryMode(CharacterInfo character){
		if(
			character != null && 
			!string.IsNullOrEmpty(character.characterName) &&
			!UFE.unlockedCharactersInStoryMode.Contains(character.characterName)
		){
			UFE.unlockedCharactersInStoryMode.Add(character.characterName);
		}

		UFE.SaveUnlockedCharacters();
	}

	public static void UnlockCharacterInVersusMode(CharacterInfo character){
		if(
			character != null && 
			!string.IsNullOrEmpty(character.characterName) &&
			!UFE.unlockedCharactersInVersusMode.Contains(character.characterName)
		){
			UFE.unlockedCharactersInVersusMode.Add(character.characterName);
		}

		UFE.SaveUnlockedCharacters();
	}
	#endregion

	#region public class methods: methods related to the stage selection
	public static void SetStage(StageOptions stage){
		config.selectedStage = stage;
	}

	public static void SetStage(string stageName){
		foreach(StageOptions stage in config.stages){
			if (stageName == stage.stageName){
				UFE.SetStage(stage);
				return;
			}
		}
	}
	
	public static StageOptions GetStage(){
		return config.selectedStage;
	}
	#endregion


	#region public class methods: methods related to the behaviour of the characters during the battle
	public static MoveSetData GetCurrentMoveSet(CharacterInfo character){
		foreach(MoveSetData moveSetData in character.moves)
			if (moveSetData.combatStance == character.currentCombatStance)
				return moveSetData;
		return null;
	}

	public static ControlsScript GetControlsScript(int player){
		if (player == 1){
			return UFE.GetPlayer1ControlsScript();
		}else if (player == 2){
			return UFE.GetPlayer2ControlsScript();
		}
		return null;
	}

	public static ControlsScript GetPlayer1ControlsScript(){
		return p1ControlsScript;
	}
	
	public static ControlsScript GetPlayer2ControlsScript(){
		return p2ControlsScript;
	}
	#endregion

	#region public class methods: methods that are used for raising events
	public static void SetLifePoints(float newValue, CharacterInfo player){
		if (UFE.OnLifePointsChange != null) UFE.OnLifePointsChange(newValue, player);
	}

	public static void FireAlert(string alertMessage, CharacterInfo player){
		if (UFE.OnNewAlert != null) UFE.OnNewAlert(alertMessage, player);
	}

	public static void FireHit(HitBox strokeHitBox, MoveInfo move, CharacterInfo player){
		if (UFE.OnHit != null) UFE.OnHit(strokeHitBox, move, player);
	}
	
	public static void FireMove(MoveInfo move, CharacterInfo player){
		if (UFE.OnMove != null) UFE.OnMove(move, player);
	}
	
	public static void FireGameBegins(){
		if (UFE.OnGameBegin != null) {
			gameRunning = true;
			UFE.OnGameBegin(config.player1Character, config.player2Character, config.selectedStage);
		}
	}
	
	public static void FireGameEnds(CharacterInfo winner, CharacterInfo loser){
		// I've commented the next line because it worked with the old GUI, but not with the new one.
		//UFE.EndGame();

		Time.timeScale = UFE.config.gameSpeed;
		UFE.gameRunning = false;
		UFE.newRoundCasted = false;
		UFE.player1WonLastBattle = (winner == UFE.GetPlayer1());

		if (UFE.OnGameEnds != null) {
			UFE.OnGameEnds(winner, loser);
		}
	}
	
	public static void FireRoundBegins(int currentRound){
		if (UFE.OnRoundBegins != null) UFE.OnRoundBegins(currentRound);
	}

	public static void FireRoundEnds(CharacterInfo winner, CharacterInfo loser){
		if (UFE.OnRoundEnds != null) UFE.OnRoundEnds(winner, loser);
	}

	public static void FireTimeOver(){
		if (UFE.OnTimeOver != null) UFE.OnTimeOver();
	}
	#endregion



	#region public class methods: UFE CORE methods
	public static void PauseGame(bool pause){
		if (pause && Time.timeScale == 0) return;

		if (pause){
			Time.timeScale = 0;
		}else{
			Time.timeScale = config.gameSpeed;
		}

		if (UFE.OnGamePaused != null){
			UFE.OnGamePaused(pause);
		}
	}

	public static bool IsInstalled(string theClass){
		return UFE.SearchClass(theClass) != null;
	}
	
	public static bool isPaused(){
		return Time.timeScale == 0;
	}
	
	public static float GetTimer(){
		return timer;
	}
	
	public static void ResetTimer(){
		timer = config.roundOptions.timer;
		intTimer = (int)config.roundOptions.timer;
		if (UFE.OnTimer != null) OnTimer(timer);
	}
	
	public static Type SearchClass(string theClass){
		// If we haven't made a decision yet, check if CInput is installed
		Type type = null;
		
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()){
			type = assembly.GetType(theClass);
			if (type != null){break;}
		}
		
		return type;
	}
	
	public static void SetTimer(float time){
		timer = time;
		intTimer = (int) time;
		if (UFE.OnTimer != null) OnTimer(timer);
	}
	
	public static void PlayTimer(){
		pauseTimer = false;
	}
	
	public static void PauseTimer(){
		pauseTimer = true;
	}
	
	public static void EndGame(){
		Time.timeScale = UFE.config.gameSpeed;
		UFE.gameRunning = false;
		UFE.newRoundCasted = false;
		
		if (UFE.battleGUI != null){
			UFE.battleGUI.OnHide();
			GameObject.Destroy(UFE.battleGUI.gameObject);
		}
		
		if (gameEngine != null) {
			GameObject.Destroy(gameEngine);
			gameEngine = null;
		}
	}

	public static void ResetRoundCast(){
		newRoundCasted = false;
	}
	
	public static void CastNewRound(){
		if (newRoundCasted) return;
		if (p1ControlsScript.introPlayed && p2ControlsScript.introPlayed){
			FireRoundBegins(config.currentRound);
			UFE.DelaySynchronizedAction(p1ControlsScript.startFight, 2f);
			newRoundCasted = true;
		}
	}

	public static void CastInput(InputReferences[] inputReferences, int player){
		if (UFE.OnInput != null) OnInput(inputReferences, player);
	}
	#endregion

	#region public class methods: Network Related methods
	public static void HostGame(){
		if (!UFE.isNetworkAddonInstalled){
			//Networking.Host(15937, Networking.TransportationProtocolType.UDP, 31, false, useNat: false);
			//Network.InitializeServer(2,Networking.Host(15937, Networking.TransportationProtocolType.UDP, 31, true, useNat: false), true);
		}
	}

	public static void JoinGame(string ip){
		if (UFE.isNetworkAddonInstalled){
            Network.Connect(ip, UFE.config.networkOptions.port);
		}
	}

	public static void DisconnectFromGame(){
		if (UFE.isNetworkAddonInstalled){
			Network.Disconnect();
		}
	}
	#endregion


	#region protected instance methods: MonoBehaviour methods
	protected void Awake(){
		// TODO: it would be cool to load/save the user settings from/to disk (PlayerPrefs)
		UFE.config = UFE_Config;

		// Check which characters have been unlocked
		UFE.LoadUnlockedCharacters();

        // Check the installed Addons and supported 3rd party products
        UFE.isCInputInstalled = UFE.IsInstalled("cInput");
        UFE.isAiAddonInstalled = UFE.IsInstalled("RuleBasedAI");
        UFE.isNetworkAddonInstalled = UFE.IsInstalled("NetworkController");

#if !UFE_BASIC
		UFE.isControlFreakInstalled = UFE.IsInstalled("TouchController");
#else
        UFE.isControlFreakInstalled = false;
#endif

		// Check if we should run the application in background
		Application.runInBackground = UFE.config.runInBackground;

		// Check if cInput is installed and initialize the cInput GUI
		if (UFE.isCInputInstalled){
			Type t = UFE.SearchClass("cGUI");
			if (t != null) t.GetField("cSkin").SetValue(null, UFE.config.inputOptions.cInputSkin);
		}

#if !UFE_BASIC
		// Check if "Control Freak Virtual Controller" is installed and instantiate the prefab
		if (
			UFE.isControlFreakInstalled &&
			UFE.config.inputOptions.inputManagerType == InputManagerType.ControlFreak && 
		    UFE.config.inputOptions.controlFreakPrefab != null
		){
			UFE.controlFreakPrefab = (GameObject) Instantiate(UFE.config.inputOptions.controlFreakPrefab);
		}
#endif

		// Check if the "network addon" is installed
		if (UFE.isNetworkAddonInstalled){
			NetworkView network = this.gameObject.AddComponent<NetworkView>();
			network.stateSynchronization = NetworkStateSynchronization.Off;
			Network.sendRate = 1f / (float)UFE.config.fps;

			UFE.localPlayerController = this.gameObject.AddComponent(UFE.SearchClass("LocalPlayerController")) as UFEController;
			UFE.remotePlayerController = this.gameObject.AddComponent(UFE.SearchClass("RemotePlayerController")) as UFEController;
			network.observed = UFE.remotePlayerController;
		}

		UFE.InitializeAudioSystem();

		// Initialize the input systems
		p1Controller = gameObject.AddComponent<UFEController> ();
		if (Input.multiTouchEnabled){
			p1Controller.humanController = gameObject.AddComponent<InputTouchController>();
		}else{
			p1Controller.humanController = gameObject.AddComponent<InputController>();
		}
		if (UFE.isAiAddonInstalled){
			p1Controller.cpuController = (AbstractInputController)gameObject.AddComponent<RuleBasedAI>();
		}else{
			p1Controller.cpuController = gameObject.AddComponent<RandomAI>();
		}
		p1Controller.isCPU = config.p1CPUControl;
		p1Controller.player = 1;

		p2Controller = gameObject.AddComponent<UFEController> ();
		p2Controller.humanController = gameObject.AddComponent<InputController>();
		if (UFE.isAiAddonInstalled){
			p2Controller.cpuController = (AbstractInputController )gameObject.AddComponent<RuleBasedAI>();
		}else{
			p2Controller.cpuController = gameObject.AddComponent<RandomAI>();
		}
		p2Controller.isCPU = config.p2CPUControl;
		p2Controller.player = 2;


		p1Controller.Initialize(config.player1_Inputs);
		p2Controller.Initialize(config.player2_Inputs);

		if (config.fps > 0) {
			Time.timeScale = config.gameSpeed;
			Application.targetFrameRate = config.fps;
		}
		
		SetLanguage();

		//-------------------------------------------------------------------------------------------------------------
		// Initialize the GUI
		//-------------------------------------------------------------------------------------------------------------
        GameObject goGroup = new GameObject("CanvasGroup");
        UFE.canvasGroup = goGroup.AddComponent<CanvasGroup>();


		GameObject go = new GameObject("Canvas");
        go.transform.SetParent(goGroup.transform);

		UFE.canvas = go.AddComponent<Canvas>();
		UFE.canvas.renderMode = RenderMode.ScreenSpaceOverlay;

		UFE.eventSystem = go.AddComponent<EventSystem>();
		UFE.graphicRaycaster = go.AddComponent<GraphicRaycaster>();

		UFE.touchInputModule = go.AddComponent<TouchInputModule>();
		UFE.touchInputModule.forceModuleActive = true;
		UFE.touchInputModule.ActivateModule();

		//UFE.standaloneInputModule = go.AddComponent<StandaloneInputModule>();
		//UFE.standaloneInputModule.verticalAxis = "P1KeyboardVertical";
		//UFE.standaloneInputModule.horizontalAxis = "P1KeyboardHorizontal";
		//UFE.standaloneInputModule.allowActivationOnMobileDevice = true;

		if (UFE.config.gameGUI.useCanvasScaler){
			CanvasScaler cs = go.AddComponent<CanvasScaler>();
			cs.defaultSpriteDPI = UFE.config.gameGUI.canvasScaler.defaultSpriteDPI;
			cs.fallbackScreenDPI = UFE.config.gameGUI.canvasScaler.fallbackScreenDPI;
			cs.matchWidthOrHeight = UFE.config.gameGUI.canvasScaler.matchWidthOrHeight;
			cs.physicalUnit = UFE.config.gameGUI.canvasScaler.physicalUnit;
			cs.referencePixelsPerUnit = UFE.config.gameGUI.canvasScaler.referencePixelsPerUnit;
			cs.referenceResolution = UFE.config.gameGUI.canvasScaler.referenceResolution;
			cs.scaleFactor = UFE.config.gameGUI.canvasScaler.scaleFactor;
			cs.screenMatchMode = UFE.config.gameGUI.canvasScaler.screenMatchMode;
			cs.uiScaleMode = UFE.config.gameGUI.canvasScaler.scaleMode;

			//---------------------------------------------------------------------------------------------------------
			// We use comment the next line because we use a "Screen Space - Overlay" canvas
			// and the "dynaicPixelsPerUnit" property is only used in "World Space" Canvas.
			//---------------------------------------------------------------------------------------------------------
			//cs.dynamicPixelsPerUnit = UFE.config.gameGUI.canvasScaler.dynamicPixelsPerUnit; 
		}

		// DEBUGGER
		GameObject debuggerGO = new GameObject("Debugger1");
		UFE.debugger1 = debuggerGO.AddComponent<GUIText>();
		UFE.debugger1.pixelOffset = new Vector2(55 * ((float)Screen.width/1280), 570f * ((float)Screen.height/720));
		UFE.debugger1.text = "Debug mode";
		UFE.debugger1.anchor = TextAnchor.UpperLeft;
		UFE.debugger1.color = Color.black;
		UFE.debugger1.richText = true;
		debugger1.enabled = false;
		
		GameObject debuggerGO2 = new GameObject("Debugger2");
		UFE.debugger2 = debuggerGO2.AddComponent<GUIText>();
		UFE.debugger2.pixelOffset = new Vector2(1225 * ((float)Screen.width/1280), 570f * ((float)Screen.height/720));
		UFE.debugger2.text = "Debug mode";
		UFE.debugger2.alignment = TextAlignment.Right;
		UFE.debugger2.anchor = TextAnchor.UpperRight;
		UFE.debugger2.color = Color.black;
		UFE.debugger2.richText = true;
		debugger2.enabled = false;
		//-------------------------------------------------------------------------------------------------------------

		// Load the player settings from disk
		UFE.SetMusic(PlayerPrefs.GetInt(UFE.MusicEnabledKey, 1) > 0);
		UFE.SetMusicVolume(PlayerPrefs.GetFloat(UFE.MusicVolumeKey, 1f));
		UFE.SetSoundFX(PlayerPrefs.GetInt(UFE.SoundsEnabledKey, 1) > 0);
		UFE.SetSoundFXVolume(PlayerPrefs.GetFloat(UFE.SoundsVolumeKey, 1f));
		UFE.SetDebugMode(config.debugOptions.debugMode);

		// Set default difficulty level
		/*int difficultyIndex = PlayerPrefs.GetInt(UFE.DifficultyLevelKey, -1);
		if (difficultyIndex >= 0 && difficultyIndex < UFE.config.aiOptions.difficultySettings.Length){
			UFE.SetAIDifficulty(UFE.config.aiOptions.difficultySettings[difficultyIndex]);
		}else{*/

		UFE.SetAIDifficulty(UFE.config.aiOptions.selectedDifficultyLevel);
		//}

		// Load the intro screen or the combat, depending on the UFE Config settings
		if (UFE.config.debugOptions.startGameImmediately){
            if (UFE.config.debugOptions.trainingMode) {
                UFE.gameMode = GameMode.TrainingRoom;
            } else {
                UFE.gameMode = GameMode.VersusMode;
            }
			UFE.config.player1Character = config.p1CharStorage;
			UFE.config.player2Character = config.p2CharStorage;
			UFE.SetCPU(1, config.p1CPUControl);
			UFE.SetCPU(2, config.p2CPUControl);
			//UFE.StartGame(0);
			UFE._StartLoadingBattleScreen(0);
		}else{
			UFE.StartIntroScreen(0f);
		}
	}

	protected void Update(){
		UFEController controller1 = UFE.GetPlayer1Controller();
		UFEController controller2 = UFE.GetPlayer2Controller();
		bool bothReady = controller1.isReady && controller2.isReady;
		
		controller1.DoUpdate();
		controller2.DoUpdate();
		
		
		//-------------------------------------------------------------------------------------------------------------
		// If the Time Scale is zero, we must invoke the FixedUpdate() method manually or we would be unable to detect
		// the player input in that situation. For example, if the game were paused, we wouldn't be able to detect the 
		// button presses of the player when he tries to exit from the "pause mode".
		//-------------------------------------------------------------------------------------------------------------
		if (Time.timeScale <= 0f && bothReady){
			// Update the battle GUI.
			if (UFE.battleGUI != null){
				UFE.battleGUI.DoFixedUpdate();
			}
			
			// Update the GUI of the current screen
			if (UFE.currentScreen != null){
				UFE.currentScreen.DoFixedUpdate();
			}
			
			// Check if we need to execute any local "delayed action" (such as playing a sound)
			for (int i = UFE.delayedLocalActions.Count - 1; i >= 0; --i){
				DelayedAction action = UFE.delayedLocalActions[i];
				--action.steps;
				
				if (action.steps <= 0){
					action.action();
					UFE.delayedLocalActions.RemoveAt(i);
				}
			}
			
			controller1.DoFixedUpdate();
			controller2.DoFixedUpdate();
			
			if(UFE.isConnected){
				++UFE.currentNetworkFrame;
			}else{
				UFE.currentNetworkFrame = 0;
			}
		}
	}

	protected void FixedUpdate(){
		UFEController controller1 = UFE.GetPlayer1Controller();
		UFEController controller2 = UFE.GetPlayer2Controller();
		bool bothReady = controller1.isReady && controller2.isReady;

		if (bothReady){
			if (UFE.cameraScript != null){
				UFE.cameraScript.DoFixedUpdate();
			}

			if (CameraFade.instance.enabled){
				CameraFade.instance.DoFixedUpdate();
			}

			if (UFE.battleGUI != null){
				UFE.battleGUI.DoFixedUpdate();
			}

			if (UFE.currentScreen != null){
				UFE.currentScreen.DoFixedUpdate();
			}

            if (UFE.canvasGroup.alpha == 0) UFE.canvasGroup.alpha = 1;

			if (gameRunning){
				if (config.roundOptions.hasTimer && timer > 0 && !pauseTimer){
                    if (UFE.gameMode != GameMode.TrainingRoom || !config.trainingModeOptions.freezeTime) timer -= Time.fixedDeltaTime * (config.roundOptions.timerSpeed * .01f);
					if (timer < intTimer) {
						intTimer --;
						if (UFE.OnTimer != null) OnTimer(timer);
					}
				}
				if (timer < 0) timer = 0;
				if (intTimer < 0) intTimer = 0;
				
				
				if (timer == 0 && p1ControlsScript != null && !config.lockMovements){
					float p1LifePercentage = p1ControlsScript.myInfo.currentLifePoints/(float)p1ControlsScript.myInfo.lifePoints;
					float p2LifePercentage = p2ControlsScript.myInfo.currentLifePoints/(float)p2ControlsScript.myInfo.lifePoints;
					PauseTimer();
					config.lockMovements = true;
					config.lockInputs = true;

					UFE.FireTimeOver();
					
					if (p1LifePercentage > p2LifePercentage) {
						UFE.DelaySynchronizedAction(p2ControlsScript.EndRound, 3f);
					}else if (p1LifePercentage == p2LifePercentage){
						UFE.DelaySynchronizedAction(p1ControlsScript.NewRound, 3f);
					}else{
						UFE.DelaySynchronizedAction(p1ControlsScript.EndRound, 3f);
					}
				}
			}

			if (p1ControlsScript != null){
				if (p1ControlsScript.MoveSet != null && p1ControlsScript.MoveSet.MecanimControl != null){
					p1ControlsScript.MoveSet.MecanimControl.DoFixedUpdate();
                }
                if (p1ControlsScript.MoveSet != null && p1ControlsScript.MoveSet.LegacyControl != null) {
                    p1ControlsScript.MoveSet.LegacyControl.DoFixedUpdate();
                }

				if (p1ControlsScript.projectiles.Count > 0){
					p1ControlsScript.projectiles.RemoveAll(item => item.IsDestroyed() || item == null);

					foreach(ProjectileMoveScript projectileMoveScript in p1ControlsScript.projectiles){
						if (projectileMoveScript != null) projectileMoveScript.DoFixedUpdate();
					}
				}

				p1ControlsScript.DoFixedUpdate();
			}

			if (p2ControlsScript != null){
				if (p2ControlsScript.MoveSet != null && p2ControlsScript.MoveSet.MecanimControl != null){
					p2ControlsScript.MoveSet.MecanimControl.DoFixedUpdate();
                }
                if (p2ControlsScript.MoveSet != null && p2ControlsScript.MoveSet.LegacyControl != null) {
                    p2ControlsScript.MoveSet.LegacyControl.DoFixedUpdate();
                }
				
				if (p2ControlsScript.projectiles.Count > 0){
					p2ControlsScript.projectiles.RemoveAll(item => item.IsDestroyed() || item == null);

					foreach(ProjectileMoveScript projectileMoveScript in p2ControlsScript.projectiles){
						if (projectileMoveScript != null) projectileMoveScript.DoFixedUpdate();
					}
				}

				p2ControlsScript.DoFixedUpdate();
			}

			if (UFE.battleGUI != null){
				if (controlFreakPrefab != null && !controlFreakPrefab.activeSelf) controlFreakPrefab.SetActive(true);
			}else{
				if (controlFreakPrefab != null && controlFreakPrefab.activeSelf) controlFreakPrefab.SetActive(false);
			}


			// Check if we need to execute any delayed "local action" (such as playing a sound)
			for (int i = UFE.delayedLocalActions.Count - 1; i >= 0; --i){
				DelayedAction action = UFE.delayedLocalActions[i];
				--action.steps;
				
				if (action.steps <= 0){
					action.action();
					UFE.delayedLocalActions.RemoveAt(i);
				}
			}

			// Check if we need to execute any delayed "synchronized action"
			for (int i = UFE.delayedSynchronizedActions.Count - 1; i >= 0; --i){
				DelayedAction action = UFE.delayedSynchronizedActions[i];
				--action.steps;

				if (action.steps <= 0){
					action.action();
					UFE.delayedSynchronizedActions.RemoveAt(i);
				}
			}
			
			controller1.DoFixedUpdate();
			controller2.DoFixedUpdate();
			
			if(UFE.isConnected){
				++UFE.currentNetworkFrame;
			}else{
				UFE.currentNetworkFrame = 0;
			}
		}
	}

	protected void OnApplicationQuit(){
		UFE.closing = true;
		if (Network.peerType != NetworkPeerType.Disconnected && !UFE.disconnecting) Network.Disconnect();
	}
	#endregion

	#region protected instance methods: Network Events
	public static bool isConnected{
		get{
			return	Network.peerType != NetworkPeerType.Disconnected &&
					Network.peerType != NetworkPeerType.Connecting &&
					//UFE.synchronizedRandomSeed &&
					(Network.connections.Length > 0 || UFE.config.networkOptions.fakeNetwork);
		}
	}

	public static void SetSynchronizedRandomSeed(int randomSeed){
		UnityEngine.Random.seed = randomSeed;
		UFE.synchronizedRandomSeed = true;
	}

	protected void OnConnectedToServer(){
		Debug.Log("Connected to server");
		UFE.StartNetworkGame(0.5f, 2);
	}

	protected void OnDisconnectedFromServer(NetworkDisconnection info) {
		Debug.Log("Disconnected from server: " + info);

		if (!UFE.closing){
			UFE.disconnecting = true;
			Application.runInBackground = UFE.config.runInBackground;
			UFE.StartConnectionLostScreen();
		}
	}

	protected void OnFailedToConnect(NetworkConnectionError error) {
		Debug.Log("Could not connect to server: " + error);
		Application.runInBackground = UFE.config.runInBackground;
		UFE.StartConnectionLostScreen();
	}

	protected void OnPlayerConnected(NetworkPlayer player) {
		Debug.Log("Player connected from " + player.ipAddress + ":" + player.port);
		UFE.StartNetworkGame(0.5f, 1);
	}

	protected void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Clean up after player " + player);
		//Network.RemoveRPCs(player);
		//Network.DestroyPlayerObjects(player);

		if (!UFE.closing){
			UFE.disconnecting = true;
			Application.runInBackground = UFE.config.runInBackground;
			UFE.StartConnectionLostScreen();
		}
	}

	protected void OnServerInitialized() {
		Debug.Log("Server initialized and ready");
		Application.runInBackground = true;
		UFE.disconnecting = false;
		
		if (UFE.config.networkOptions.fakeNetwork){
			this.OnPlayerConnected(Network.player);
		}
	}
	#endregion

	#region private class methods: GUI Related methods
	private static void _StartCharacterSelectionScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.characterSelectionScreen == null){
			Debug.LogError("Character Selection Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}else{
			UFE.ShowScreen(UFE.config.gameGUI.characterSelectionScreen);
		}
	}

	private static void _StartIntroScreen(float fadeTime){
		if (Network.peerType != NetworkPeerType.Disconnected) Network.Disconnect();
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.introScreen == null){
			//Debug.Log("Intro Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}else{
			UFE.ShowScreen(UFE.config.gameGUI.introScreen);
		}
	}

	private static void _StartMainMenuScreen(float fadeTime){
		if (UFE.isConnected && !UFE.disconnecting) Network.Disconnect();
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.mainMenuScreen == null){
			Debug.LogError("Main Menu Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}else{
			UFE.ShowScreen(UFE.config.gameGUI.mainMenuScreen);
		}
	}

	private static void _StartStageSelectionScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.stageSelectionScreen == null){
			Debug.LogError("Stage Selection Screen not found! Make sure you have set the prefab correctly in the Global Editor");
		}else{
			UFE.ShowScreen(UFE.config.gameGUI.stageSelectionScreen);
		}
	}
	
	private static void _StartCreditsScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.creditsScreen == null){
			Debug.Log("Credits screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}else{
			UFE.ShowScreen(UFE.config.gameGUI.creditsScreen);
		}
	}

	private static void _StartConnectionLostScreen(float fadeTime){
		if (Network.peerType != NetworkPeerType.Disconnected) Network.Disconnect();
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.connectionLostScreen == null){
			Debug.LogError("Connection Lost Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}else if (UFE.isNetworkAddonInstalled){
			UFE.ShowScreen(UFE.config.gameGUI.connectionLostScreen);
		}else{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

    public static void SearchAndCastGameObject(object target) {
        if (target != null) {
            Type typeSource = target.GetType();
            FieldInfo[] fields = typeSource.GetFields();

            foreach (FieldInfo field in fields) {
                object fieldValue = field.GetValue(target);
                if (fieldValue == null || fieldValue.Equals(null)) continue;
                if (memoryDump.Contains(fieldValue)) continue;
                memoryDump.Add(fieldValue);

                if (field.FieldType.Equals(typeof(GameObject))) {
                    if (UFE.config.debugOptions.preloadedObjects) Debug.Log(fieldValue + " preloaded");
                    GameObject tempGO = (GameObject)Instantiate((GameObject)fieldValue);
                    Destroy(tempGO, 0);

                } else if (field.FieldType.IsArray && !field.FieldType.GetElementType().IsEnum){
                    object[] fieldValueArray = (object[])fieldValue;
                    foreach (object obj in fieldValueArray) {
                        SearchAndCastGameObject(obj);
                    }
                }
            }
        }
    }

	private static void _StartGame(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.battleGUI == null){
			Debug.LogError("Battle GUI not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE.battleGUI = new GameObject("BattleGUI").AddComponent<UFEScreen>();
		}else{
			UFE.battleGUI = (UFEScreen) GameObject.Instantiate(UFE.config.gameGUI.battleGUI);
		}
		UFE.battleGUI.transform.SetParent(UFE.canvas != null ? UFE.canvas.transform : null, false);
        UFE.battleGUI.OnShow();
        UFE.canvasGroup.alpha = 0;
		
		gameEngine = new GameObject("Game");
		UFE.cameraScript = gameEngine.AddComponent<CameraScript>();
		
		if (UFE.config.player1Character == null){
			Debug.LogError("No character selected for player 1.");
			return;
		}
		if (UFE.config.player2Character == null){
			Debug.LogError("No character selected for player 2.");
			return;
		}
		if (UFE.config.selectedStage == null){
			Debug.LogError("No stage selected.");
			return;
		}
		
		if (UFE.config.aiOptions.engine == AIEngine.FuzzyAI){
			UFE.SetFuzzyAI(1, UFE.config.player1Character);
			UFE.SetFuzzyAI(2, UFE.config.player2Character);
		}
		
		UFE.config.player1Character.currentLifePoints = (float)UFE.config.player1Character.lifePoints;
		UFE.config.player2Character.currentLifePoints = (float)UFE.config.player2Character.lifePoints;
		UFE.config.player1Character.currentGaugePoints = 0;
		UFE.config.player2Character.currentGaugePoints = 0;
		
		if (UFE.config.selectedStage.prefab == null){
			Debug.LogError("Stage prefab not found! Make sure you have set the prefab correctly in the Global Editor");
		}
		GameObject stageInstance = (GameObject) Instantiate(config.selectedStage.prefab);
		
		
		stageInstance.transform.parent = gameEngine.transform;
		UFE.config.currentRound = 1;
		UFE.config.lockInputs = true;
		UFE.SetTimer(config.roundOptions.timer);
		UFE.PauseTimer();
		
		GameObject p1 = new GameObject("Player1");
		p1.transform.parent = gameEngine.transform;
		p1ControlsScript = p1.AddComponent<ControlsScript>();
		p1.AddComponent<PhysicsScript>();
		
		GameObject p2 = new GameObject("Player2");
		p2.transform.parent = gameEngine.transform;
		p2ControlsScript = p2.AddComponent<ControlsScript>();
		p2.AddComponent<PhysicsScript>();


        //Preload
        if (UFE.config.preloadHitEffects) {
            SearchAndCastGameObject(UFE.config.hitOptions);
            if (UFE.config.debugOptions.preloadedObjects) Debug.Log("Hit Effects Loaded");
        }
        if (UFE.config.preloadStage) {
            SearchAndCastGameObject(UFE.config.selectedStage);
            if (UFE.config.debugOptions.preloadedObjects) Debug.Log("Stage Loaded");
        }
        if (UFE.config.preloadCharacter1) {
            SearchAndCastGameObject(UFE.config.player1Character);
            if (UFE.config.debugOptions.preloadedObjects) Debug.Log("Character 1 Loaded");
        }
        if (UFE.config.preloadCharacter2) {
            SearchAndCastGameObject(UFE.config.player2Character);
            if (UFE.config.debugOptions.preloadedObjects) Debug.Log("Character 2 Loaded");
        }
        if (UFE.config.warmAllShaders) Shader.WarmupAllShaders();

        memoryDump.Clear();
	}

	private static void _StartHostGameScreen(float fadeTime){
		if (Network.peerType != NetworkPeerType.Disconnected) Network.Disconnect();
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.hostGameScreen == null){
			Debug.LogError("Host Game Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}else if (UFE.isNetworkAddonInstalled){
			UFE.ShowScreen(UFE.config.gameGUI.hostGameScreen);
		}else{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	private static void _StartJoinGameScreen(float fadeTime){
		if (Network.peerType != NetworkPeerType.Disconnected) Network.Disconnect();
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.joinGameScreen == null){
			Debug.LogError("Join To Game Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}else if (UFE.isNetworkAddonInstalled){
			UFE.ShowScreen(UFE.config.gameGUI.joinGameScreen);
		}else{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}
	
	private static void _StartLoadingBattleScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.loadingBattleScreen == null){
			Debug.Log("Loading Battle Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartGame(fadeTime);
		}else{
			UFE.ShowScreen(UFE.config.gameGUI.loadingBattleScreen);
		}
	}
	
	private static void _StartNetworkGameScreen(float fadeTime){
		if (Network.peerType != NetworkPeerType.Disconnected) Network.Disconnect();
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.networkGameScreen == null){
			Debug.LogError("Network Game Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}else if (UFE.isNetworkAddonInstalled){
			UFE.ShowScreen(UFE.config.gameGUI.networkGameScreen);
		}else{
			Debug.LogWarning("Network Addon not found!");
			UFE._StartMainMenuScreen(fadeTime);
		}
	}

	private static void _StartOptionsScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.optionsScreen == null){
			Debug.LogError("Options Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}else{
			UFE.ShowScreen(UFE.config.gameGUI.optionsScreen);
		}
	}

	public static void _StartStoryModeBattle(float fadeTime){
		// If the player 1 won the last battle, load the information of the next battle. 
		// Otherwise, repeat the last battle...
		CharacterInfo character = UFE.GetPlayer(1);

		if (UFE.player1WonLastBattle){
			// If the player 1 won the last battle...
			if (UFE.storyMode.currentGroup < 0){
				// If we haven't fought any battle, raise the "Story Mode Started" event...
				if (UFE.OnStoryModeStarted != null){
					UFE.OnStoryModeStarted(character);
				}

				// And start with the first battle of the first group
				UFE.storyMode.currentGroup = 0;
				UFE.storyMode.currentBattle = 0;
			}else if (UFE.storyMode.currentGroup >= 0 && UFE.storyMode.currentGroup < UFE.storyMode.characterStory.fightsGroups.Length){
				// Otherwise, check if there are more remaining battles in the current group
				FightsGroup currentGroup = UFE.storyMode.characterStory.fightsGroups[UFE.storyMode.currentGroup];
				int numberOfFights = currentGroup.maxFights;
				
				if (currentGroup.mode != FightsGroupMode.FightAgainstSeveralOpponentsInTheGroupInRandomOrder){
					numberOfFights = currentGroup.opponents.Length;
				}
				
				if (UFE.storyMode.currentBattle < numberOfFights - 1){
					// If there are more battles in the current group, go to the next battle...
					++UFE.storyMode.currentBattle;
				}else{
					// Otherwise, go to the next group of battles...
					++UFE.storyMode.currentGroup;
					UFE.storyMode.currentBattle = 0;
					UFE.storyMode.defeatedOpponents.Clear();
				}
			}

			// If the player hasn't finished the game...
			UFE.storyMode.currentBattleInformation = null;
			while (
				UFE.storyMode.currentBattleInformation == null &&
				UFE.storyMode.currentGroup >= 0 && 
				UFE.storyMode.currentGroup < UFE.storyMode.characterStory.fightsGroups.Length
			){
				// Try to retrieve the information of the next battle
				FightsGroup currentGroup = UFE.storyMode.characterStory.fightsGroups[UFE.storyMode.currentGroup];
				UFE.storyMode.currentBattleInformation = null;
				
				if (currentGroup.mode == FightsGroupMode.FightAgainstAllOpponentsInTheGroupInTheDefinedOrder){
					StoryModeBattle b = currentGroup.opponents[UFE.storyMode.currentBattle];
					CharacterInfo opponent = UFE.config.characters[b.opponentCharacterIndex];

					if (UFE.storyMode.canFightAgainstHimself || !character.characterName.Equals(opponent.characterName)){
						UFE.storyMode.currentBattleInformation = b;
					}else{
						// Otherwise, check if there are more remaining battles in the current group
						int numberOfFights = currentGroup.maxFights;
						
						if (currentGroup.mode != FightsGroupMode.FightAgainstSeveralOpponentsInTheGroupInRandomOrder){
							numberOfFights = currentGroup.opponents.Length;
						}
						
						if (UFE.storyMode.currentBattle < numberOfFights - 1){
							// If there are more battles in the current group, go to the next battle...
							++UFE.storyMode.currentBattle;
						}else{
							// Otherwise, go to the next group of battles...
							++UFE.storyMode.currentGroup;
							UFE.storyMode.currentBattle = 0;
							UFE.storyMode.defeatedOpponents.Clear();
						}
					}
				}else{
					List<StoryModeBattle> possibleBattles = new List<StoryModeBattle>();
					
					foreach (StoryModeBattle b in currentGroup.opponents){
						if (!UFE.storyMode.defeatedOpponents.Contains(b.opponentCharacterIndex)){
							CharacterInfo opponent = UFE.config.characters[b.opponentCharacterIndex];
							
							if (UFE.storyMode.canFightAgainstHimself || !character.characterName.Equals(opponent.characterName)){
								possibleBattles.Add(b);
							}
						}
					}
					
					if (possibleBattles.Count > 0){
						int index = UnityEngine.Random.Range(0, possibleBattles.Count);
						UFE.storyMode.currentBattleInformation = possibleBattles[index];
					}else{
						// If we can't find a valid battle in this group, try moving to the next group
						++UFE.storyMode.currentGroup;
					}
				}
			}
		}

		if (UFE.storyMode.currentBattleInformation != null){
			// If we could retrieve the battle information, load the opponent and the stage
			int characterIndex = UFE.storyMode.currentBattleInformation.opponentCharacterIndex;
			UFE.SetPlayer2(UFE.config.characters[characterIndex]);

			if (UFE.player1WonLastBattle){
				UFE.lastStageIndex = UnityEngine.Random.Range(0, UFE.storyMode.currentBattleInformation.possibleStagesIndexes.Count);
			}

			UFE.SetStage(UFE.config.stages[UFE.storyMode.currentBattleInformation.possibleStagesIndexes[UFE.lastStageIndex]]);
			
			// Finally, check if we should display any "Conversation Screen" before the battle
			UFE._StartStoryModeConversationBeforeBattleScreen(UFE.storyMode.currentBattleInformation.conversationBeforeBattle, fadeTime);
		}else{
			// Otherwise, show the "Congratulations" Screen
			if (UFE.OnStoryModeCompleted != null){
				UFE.OnStoryModeCompleted(character);
			}

			UFE._StartStoryModeCongratulationsScreen(fadeTime);
		}
	}

	private static void _StartStoryModeCongratulationsScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.storyModeCongratulationsScreen == null){
			Debug.Log("Congratulations Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartStoryModeEndingScreen(fadeTime);
		}else{
			UFE.ShowScreen(UFE.config.gameGUI.storyModeCongratulationsScreen, delegate(){UFE.StartStoryModeEndingScreen(fadeTime);});
		}
	}

	private static void _StartStoryModeContinueScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.storyModeContinueScreen == null){
			Debug.Log("Continue Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}else{
			UFE.ShowScreen(UFE.config.gameGUI.storyModeContinueScreen);
		}
	}

	private static void _StartStoryModeConversationAfterBattleScreen(UFEScreen conversationScreen, float fadeTime){
		if (conversationScreen != null){
			CameraFade.StartAlphaFade(Color.black, true, fadeTime);
			UFE.EndGame();
			UFE.HideScreen(UFE.currentScreen);
			UFE.ShowScreen(conversationScreen, delegate(){UFE.StartStoryModeBattle(fadeTime);});
		}else{
			UFE._StartStoryModeBattle(fadeTime);
		}
	}

	private static void _StartStoryModeConversationBeforeBattleScreen(UFEScreen conversationScreen, float fadeTime){
		if (conversationScreen != null){
			CameraFade.StartAlphaFade(Color.black, true, fadeTime);
			UFE.EndGame();
			UFE.HideScreen(UFE.currentScreen);
			UFE.ShowScreen(conversationScreen, delegate(){UFE.StartLoadingBattleScreen(fadeTime);});
		}else{
			UFE._StartLoadingBattleScreen(fadeTime);
		}
	}

	private static void _StartStoryModeEndingScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);
		
		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.storyMode.characterStory.ending == null){
			Debug.Log("Ending Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartCreditsScreen(fadeTime);
		}else{
			UFE.ShowScreen(UFE.storyMode.characterStory.ending, delegate(){UFE.StartCreditsScreen(fadeTime);});
		}
	}

	private static void _StartStoryModeGameOverScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.storyModeGameOverScreen == null){
			Debug.Log("Game Over Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartMainMenuScreen(fadeTime);
		}else{
			UFE.ShowScreen(UFE.config.gameGUI.storyModeGameOverScreen, delegate(){UFE.StartMainMenuScreen(fadeTime);});
		}
	}

	private static void _StartStoryModeOpeningScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);
		
		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.storyMode.characterStory.opening == null){
			Debug.Log("Opening Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE._StartStoryModeBattle(fadeTime);
		}else{
			UFE.ShowScreen(UFE.storyMode.characterStory.opening, delegate(){UFE.StartStoryModeBattle(fadeTime);});
		}
	}

	private static void _StartVersusModeScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.versusModeScreen == null){
			Debug.Log("Versus Mode Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			UFE.StartPlayerVersusPlayer(fadeTime);
		}else{
			UFE.ShowScreen(UFE.config.gameGUI.versusModeScreen);
		}
	}

	private static void _StartVersusModeAfterBattleScreen(float fadeTime){
		CameraFade.StartAlphaFade(Color.black, true, fadeTime);

		//UFE.EndGame();
		UFE.HideScreen(UFE.currentScreen);
		if (UFE.config.gameGUI.versusModeAfterBattleScreen == null){
			Debug.Log("Versus Mode \"After Battle\" Screen not found! Make sure you have set the prefab correctly in the Global Editor");
			
			UFE._StartMainMenuScreen(fadeTime);
		}else{
			UFE.ShowScreen(UFE.config.gameGUI.versusModeAfterBattleScreen);
		}
	}
	#endregion
}