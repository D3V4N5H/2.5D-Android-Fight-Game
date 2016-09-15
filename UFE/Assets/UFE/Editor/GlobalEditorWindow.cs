using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GlobalEditorWindow : EditorWindow {
	public static GlobalEditorWindow globalEditorWindow;
	private GlobalInfo globalInfo;
	private Vector2 scrollPos;


	private bool advancedOptions;
	private bool preloadOptions;
	private bool cameraOptions;
	private bool characterRotationOptions;
	private bool fontOptions;
	private bool languageOptions;
	private bool announcerOptions;
	private bool guiOptions;
	private bool guiScreensOptions;
	private bool screenOptions;
	private bool roundOptions;
	private bool bounceOptions;
	private bool counterHitOptions;
	private bool comboOptions;
	private bool debugOptions;
	private bool aiOptions;
	private bool blockOptions;
	private bool knockDownOptions;
	private bool sweepOptions;
	private bool hitOptions;
	private bool inputsOptions;
	private bool networkOptions;
	private bool player1InputOptions;
	private bool player2InputOptions;
	private bool cInputOptions;
	private bool touchControllerOptions;
	private bool stageOptions;
    private bool storyModeOptions;
    private bool trainingModeOptions;
	private bool storyModeSelectableCharactersInStoryModeOptions;
	private bool storyModeSelectableCharactersInVersusModeOptions;
	private bool characterOptions;
	private bool updateConfirm;
    private GameObject canvasPreview;
    private GameObject eventSystemPreview;
    private UFEScreen screenPreview;
	
	private string titleStyle;
	private string addButtonStyle;
	private string borderBarStyle;
	private string rootGroupStyle;
	private string fillBarStyle1;
	private string subGroupStyle;
	private string arrayElementStyle;
	private string subArrayElementStyle;
	private string foldStyle;
	private string enumStyle;
	private GUIStyle labelStyle;
	private GUIContent helpGUIContent = new GUIContent();
	private string pName;

	[MenuItem("Window/U.F.E./Global Editor")]
	public static void Init(){
		globalEditorWindow = EditorWindow.GetWindow<GlobalEditorWindow>(false, "Global", true);
		globalEditorWindow.Show();
		globalEditorWindow.Populate();
	}
	
	void OnSelectionChange(){
		Populate();
		Repaint();
	}
	
	void OnEnable(){
		Populate();
	}
	
	void OnFocus(){
		Populate();
	}
	
	void OnDisable(){
		Clear();
	}
	
	void OnDestroy(){
		Clear();
	}
	
	void OnLostFocus(){
		//Clear();
	}
	
	void Update(){
		if (EditorApplication.isPlayingOrWillChangePlaymode) {
			Clear();
		}
	}

	void Clear(){
		if (globalInfo != null){
            CloseGUICanvas();
		}
	}

	void helpButton(string page){
		if (GUILayout.Button("?", GUILayout.Width(18), GUILayout.Height(18))) 
			Application.OpenURL("http://www.ufe3d.com/doku.php/"+ page);
	}

	void Populate(){
		// Style Definitions
		titleStyle = "MeTransOffRight";
		borderBarStyle = "ProgressBarBack";
		addButtonStyle = "CN CountBadge";
		rootGroupStyle = "GroupBox";
		subGroupStyle = "ObjectFieldThumb";
		arrayElementStyle = "flow overlay box";
		fillBarStyle1 = "ProgressBarBar";
		subArrayElementStyle = "HelpBox";
		foldStyle = "Foldout";
		enumStyle = "MiniPopup";
		
		labelStyle = new GUIStyle();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.fontStyle = FontStyle.Bold;
		labelStyle.normal.textColor = Color.white;


		helpGUIContent.text = "";
		helpGUIContent.tooltip = "Open Live Docs";
		//helpGUIContent.image = (Texture2D) EditorGUIUtility.Load("icons/SVN_Local.png");
		
		UnityEngine.Object[] selection = Selection.GetFiltered(typeof(GlobalInfo), SelectionMode.Assets);
		if (selection.Length > 0){
			if (selection[0] == null) return;
			globalInfo = (GlobalInfo) selection[0];
		}
		
		UFE.isCInputInstalled = UFE.IsInstalled("cInput");
		UFE.isControlFreakInstalled = UFE.IsInstalled("TouchController");
		UFE.isAiAddonInstalled = UFE.IsInstalled("RuleBasedAI");
        UFE.isNetworkAddonInstalled = UFE.IsInstalled("NetworkController");

        versionUpdate();
		
	}


    private void versionUpdate() {
        if (globalInfo == null || globalInfo.characters == null || globalInfo.characters.Length == 0)
            return;

        // 1.1.0 -> 1.5.0 Update
        foreach (CharacterInfo character in globalInfo.characters) {
            if (character == null || character.moves == null || character.moves.Length == 0) continue;

            foreach (MoveSetData moveSetData in character.moves) {
                foreach (MoveInfo moveInfo in moveSetData.attackMoves) {
                    if (moveInfo != null) {
                        if (moveInfo.possibleStates != null
                            && moveInfo.possibleStates.Length > 0
                            && moveInfo.selfConditions.possibleMoveStates.Length == 0) {
                            updateConfirm = EditorUtility.DisplayDialog("UFE Update", "Update move files from 1.1.0 to 1.6.0?", "Yes", "No");
                            break;
                        }
                    }
                }
            }
        }

        if (updateConfirm) {
            foreach (CharacterInfo character in globalInfo.characters) {
                if (character == null || character.moves == null || character.moves.Length == 0) continue;

                foreach (MoveSetData moveSetData in character.moves) {
                    foreach (MoveInfo moveInfo in moveSetData.attackMoves) {
                        if (moveInfo != null) {
                            bool updateThis = false;
                            if (moveInfo.possibleStates != null
                                && moveInfo.possibleStates.Length > 0
                                && moveInfo.selfConditions.possibleMoveStates.Length == 0) {
                                foreach (PossibleStates possibleState in moveInfo.possibleStates) {
                                    PossibleMoveStates pTemp = new PossibleMoveStates();
                                    pTemp.possibleState = possibleState;
                                    moveInfo.selfConditions.possibleMoveStates = AddElement<PossibleMoveStates>(moveInfo.selfConditions.possibleMoveStates, pTemp);
                                }
                                System.Array.Clear(moveInfo.possibleStates, 0, moveInfo.possibleStates.Length);
                                moveInfo.possibleStates = null;
                                updateThis = true;
                            }

                            foreach (SoundEffect soundEffect in moveInfo.soundEffects) {
                                if (soundEffect.sound != null && soundEffect.sounds.Length == 0) {
                                    soundEffect.sounds = AddElement<AudioClip>(soundEffect.sounds, soundEffect.sound);
                                    soundEffect.sound = null;
                                    updateThis = true;
                                }
                            }

                            if (updateThis) {
                                EditorUtility.SetDirty(moveInfo);
                                Debug.Log("Move " + moveInfo.moveName + " updated.");
                            }
                        }
                    }
                }
            }
        }
        // End of Update


        // 1.5.0 -> 1.6.0 Update
        if (globalInfo.version < 1.6f) {
            updateConfirm = EditorUtility.DisplayDialog("UFE Update", "Update character files to 1.6.0?", "Yes", "No");

            if (updateConfirm) {
                globalInfo.version = 1.6f;

                // Block and Parry Hit replacements
                if (globalInfo != null && globalInfo.blockOptions.blockPrefab != null && globalInfo.blockOptions.blockHitEffects.hitParticle == null) {
                    globalInfo.blockOptions.blockHitEffects.hitParticle = globalInfo.blockOptions.blockPrefab;
                    globalInfo.blockOptions.blockHitEffects.killTime = globalInfo.blockOptions.blockKillTime;
                    globalInfo.blockOptions.blockHitEffects.hitSound = globalInfo.blockOptions.blockSound;
                    globalInfo.blockOptions.blockPrefab = null;
                }
                if (globalInfo != null && globalInfo.blockOptions.parryPrefab != null && globalInfo.blockOptions.parryHitEffects.hitParticle == null) {
                    globalInfo.blockOptions.parryHitEffects.hitParticle = globalInfo.blockOptions.parryPrefab;
                    globalInfo.blockOptions.parryHitEffects.killTime = globalInfo.blockOptions.parryKillTime;
                    globalInfo.blockOptions.parryHitEffects.hitSound = globalInfo.blockOptions.parrySound;
                    globalInfo.blockOptions.parryPrefab = null;
                }

                // Basic Moves Update
                foreach (CharacterInfo character in globalInfo.characters) {
                    if (character == null || character.moves == null || character.moves.Length == 0) continue;

                    foreach (MoveSetData moveSetData in character.moves) {
                        basicMoveUpdate(moveSetData.basicMoves.idle, WrapMode.Loop, false);
                        basicMoveUpdate(moveSetData.basicMoves.moveForward, WrapMode.Loop, false);
                        basicMoveUpdate(moveSetData.basicMoves.moveBack, WrapMode.Loop, false);

                        basicMoveUpdate(moveSetData.basicMoves.takeOff, WrapMode.ClampForever, true);
                        basicMoveUpdate(moveSetData.basicMoves.jumpStraight, WrapMode.ClampForever, true);
                        basicMoveUpdate(moveSetData.basicMoves.jumpBack, WrapMode.ClampForever, true);
                        basicMoveUpdate(moveSetData.basicMoves.jumpForward, WrapMode.ClampForever, true);
                        basicMoveUpdate(moveSetData.basicMoves.fallStraight, WrapMode.ClampForever, true);
                        basicMoveUpdate(moveSetData.basicMoves.fallBack, WrapMode.ClampForever, true);
                        basicMoveUpdate(moveSetData.basicMoves.fallForward, WrapMode.ClampForever, true);
                        basicMoveUpdate(moveSetData.basicMoves.landing, WrapMode.Once, true);

                        basicMoveUpdate(moveSetData.basicMoves.blockingHighPose, WrapMode.ClampForever, false);
                        basicMoveUpdate(moveSetData.basicMoves.blockingHighHit, WrapMode.Once, true);
                        basicMoveUpdate(moveSetData.basicMoves.blockingLowHit, WrapMode.Once, true);
                        basicMoveUpdate(moveSetData.basicMoves.blockingCrouchingPose, WrapMode.ClampForever, false);
                        basicMoveUpdate(moveSetData.basicMoves.blockingCrouchingHit, WrapMode.Once, true);
                        basicMoveUpdate(moveSetData.basicMoves.blockingAirPose, WrapMode.ClampForever, false);
                        basicMoveUpdate(moveSetData.basicMoves.blockingAirHit, WrapMode.Once, true);

                        basicMoveUpdate(moveSetData.basicMoves.parryHigh, WrapMode.Once, true);
                        basicMoveUpdate(moveSetData.basicMoves.parryLow, WrapMode.Once, true);
                        basicMoveUpdate(moveSetData.basicMoves.parryCrouching, WrapMode.Once, true);
                        basicMoveUpdate(moveSetData.basicMoves.parryAir, WrapMode.Once, true);

                        basicMoveUpdate(moveSetData.basicMoves.getHitHigh, WrapMode.Once, true);
                        basicMoveUpdate(moveSetData.basicMoves.getHitLow, WrapMode.Once, true);
                        basicMoveUpdate(moveSetData.basicMoves.getHitCrouching, WrapMode.Once, true);
                        basicMoveUpdate(moveSetData.basicMoves.getHitHighKnockdown, WrapMode.ClampForever, false);
                        basicMoveUpdate(moveSetData.basicMoves.getHitHighLowKnockdown, WrapMode.ClampForever, false);
                        basicMoveUpdate(moveSetData.basicMoves.getHitSweep, WrapMode.ClampForever, false);
                        basicMoveUpdate(moveSetData.basicMoves.getHitCrumple, WrapMode.ClampForever, true);
                        basicMoveUpdate(moveSetData.basicMoves.getHitKnockBack, WrapMode.ClampForever, true);
                        basicMoveUpdate(moveSetData.basicMoves.getHitAir, WrapMode.ClampForever, true);

                        basicMoveUpdate(moveSetData.basicMoves.fallDown, WrapMode.ClampForever, false);
                        basicMoveUpdate(moveSetData.basicMoves.bounce, WrapMode.ClampForever, false);
                        basicMoveUpdate(moveSetData.basicMoves.fallingFromBounce, WrapMode.ClampForever, true);
                        basicMoveUpdate(moveSetData.basicMoves.standUp, WrapMode.ClampForever, true);
                    }

                    EditorUtility.SetDirty(character);
                    Debug.Log("Character " + character.characterName + " updated.");
                }

                EditorUtility.SetDirty(globalInfo);
                Debug.Log("Global Options updated.");
            }
        }
        // End of Update
    }

    private void basicMoveUpdate(BasicMoveInfo basicMove, WrapMode wrapMode, bool autoSpeed) {
        basicMove.wrapMode = wrapMode;
        basicMove.autoSpeed = autoSpeed;
    }

	public void OnGUI(){
		if (globalInfo == null){
			GUILayout.BeginHorizontal("GroupBox");
			GUILayout.Label("Select a Global Configuration File or create a new one.","CN EntryInfo");
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			if (GUILayout.Button("Create new Global Configuration"))
				ScriptableObjectUtility.CreateAsset<CharacterInfo> ();
			return;
		}


		GUIStyle fontStyle = new GUIStyle();
		fontStyle.font = (Font) EditorGUIUtility.Load("EditorFont.TTF");
		fontStyle.fontSize = 30;
		fontStyle.alignment = TextAnchor.UpperCenter;
		fontStyle.normal.textColor = Color.white;
		fontStyle.hover.textColor = Color.white;
		EditorGUILayout.BeginVertical(titleStyle);{
			EditorGUILayout.BeginHorizontal();{
				EditorGUILayout.LabelField("", (globalInfo.gameName == ""? "Universal Fighting Engine":globalInfo.gameName) , fontStyle, GUILayout.Height(32));
				helpButton("global:start");
			}EditorGUILayout.EndHorizontal();
		}EditorGUILayout.EndVertical();
		
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);{
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUIUtility.labelWidth = 120;
				globalInfo.gameName = EditorGUILayout.TextField("Project Name:", globalInfo.gameName);
				EditorGUILayout.Space();

				EditorGUIUtility.labelWidth = 200;

				EditorGUILayout.Space();


				EditorGUIUtility.labelWidth = 150;
			}EditorGUILayout.EndVertical();

			
			// Debug Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					debugOptions = EditorGUILayout.Foldout(debugOptions, "Debug Options", foldStyle);
					helpButton("global:debugoptions");
				}EditorGUILayout.EndHorizontal();
				
				if (debugOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						EditorGUILayout.Space();
						EditorGUIUtility.labelWidth = 200;

                        globalInfo.debugOptions.preloadedObjects = EditorGUILayout.Toggle("Preload Info (console)", globalInfo.debugOptions.preloadedObjects);
						globalInfo.debugOptions.debugMode = EditorGUILayout.Toggle("Display Character Debug", globalInfo.debugOptions.debugMode);

                       EditorGUI.BeginDisabledGroup(!globalInfo.debugOptions.debugMode);
                        {
                            CharacterDebugOptions(globalInfo.debugOptions.p1DebugInfo, "Player 1 Debugger");
                            CharacterDebugOptions(globalInfo.debugOptions.p2DebugInfo, "Player 2 Debugger");
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.Space();


						GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

						globalInfo.debugOptions.startGameImmediately = EditorGUILayout.Toggle("Start Game Immediately", globalInfo.debugOptions.startGameImmediately);
						EditorGUI.BeginDisabledGroup(!globalInfo.debugOptions.startGameImmediately);
                        {
							if (globalInfo.stages.Length > 0) globalInfo.selectedStage = globalInfo.stages[0];
							globalInfo.p1CharStorage = (CharacterInfo) EditorGUILayout.ObjectField("Player 1 Character:", globalInfo.p1CharStorage, typeof(CharacterInfo), false);
							globalInfo.p2CharStorage = (CharacterInfo) EditorGUILayout.ObjectField("Player 2 Character:", globalInfo.p2CharStorage, typeof(CharacterInfo), false);
							globalInfo.p1CPUControl = EditorGUILayout.Toggle("Player 1 CPU Controlled", globalInfo.p1CPUControl);
							globalInfo.p2CPUControl = EditorGUILayout.Toggle("Player 2 CPU Controlled", globalInfo.p2CPUControl);
						    globalInfo.debugOptions.trainingMode = EditorGUILayout.Toggle("Play in Training Mode", globalInfo.debugOptions.trainingMode);
						}
                        EditorGUI.EndDisabledGroup();
                        
                        if (!globalInfo.debugOptions.startGameImmediately){
                            globalInfo.selectedStage = null;
							globalInfo.player1Character = null;
							globalInfo.player2Character = null;
                        }
                        EditorGUILayout.Space();

						
						EditorGUIUtility.labelWidth = 150;
						EditorGUI.indentLevel -= 1;

					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();


			// AI Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					aiOptions = EditorGUILayout.Foldout(aiOptions, "AI Options", foldStyle);
					helpButton("global:aioptions");
				}EditorGUILayout.EndHorizontal();
				
				if (aiOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 150;
						globalInfo.aiOptions.engine = (AIEngine) EditorGUILayout.EnumPopup("AI Engine:", globalInfo.aiOptions.engine, enumStyle);

						if (globalInfo.aiOptions.engine == AIEngine.RandomAI){
							EditorGUIUtility.labelWidth = 200;
							globalInfo.aiOptions.attackWhenEnemyIsDown = EditorGUILayout.Toggle("Attack When Enemy is Down", globalInfo.aiOptions.attackWhenEnemyIsDown);
							globalInfo.aiOptions.moveWhenEnemyIsDown = EditorGUILayout.Toggle("Move When Enemy is Down", globalInfo.aiOptions.moveWhenEnemyIsDown);
							
							EditorGUILayout.Space();
							globalInfo.aiOptions.inputFrequency = Mathf.Max(EditorGUILayout.FloatField("Input Frequency (seconds);", globalInfo.aiOptions.inputFrequency), 0f);
							EditorGUILayout.Space();
							
							EditorGUIUtility.labelWidth = 150;
							
							globalInfo.aiOptions.behaviourToggle = EditorGUILayout.Foldout(globalInfo.aiOptions.behaviourToggle, "Distance Behaviours ("+ globalInfo.aiOptions.distanceBehaviour.Length +")", foldStyle);
							if (globalInfo.aiOptions.behaviourToggle){
								EditorGUILayout.BeginVertical(subGroupStyle);{
									EditorGUILayout.Space();
									EditorGUI.indentLevel += 1;
									
									for (int i = 0; i < globalInfo.aiOptions.distanceBehaviour.Length; i ++){
										EditorGUILayout.Space();
										EditorGUILayout.BeginVertical(subArrayElementStyle);{
											EditorGUILayout.Space();
											
											EditorGUIUtility.labelWidth = 160;
											EditorGUILayout.BeginHorizontal();{
												globalInfo.aiOptions.distanceBehaviour[i].characterDistance = (CharacterDistance)EditorGUILayout.EnumPopup("Opponent Distance:", globalInfo.aiOptions.distanceBehaviour[i].characterDistance, enumStyle);
												Vector2 newRange = ReturnRange(globalInfo.aiOptions.distanceBehaviour[i].characterDistance, globalInfo.aiOptions.distanceBehaviour.Length);
												if (newRange != Vector2.zero){
													globalInfo.aiOptions.distanceBehaviour[i].proximityRangeBegins = (int)newRange.x;
													globalInfo.aiOptions.distanceBehaviour[i].proximityRangeEnds = (int)newRange.y;
												}
												
												//GUILayout.Label(DistanceToString(i, globalInfo.aiOptions.distanceBehaviour.Length) + " Distance");
												if (GUILayout.Button("", "PaneOptions")){
													PaneOptions<AIDistanceBehaviour>(globalInfo.aiOptions.distanceBehaviour, globalInfo.aiOptions.distanceBehaviour[i], delegate (AIDistanceBehaviour[] newElement) { globalInfo.aiOptions.distanceBehaviour = newElement; });
												}
											}EditorGUILayout.EndHorizontal();
											GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
											
											int pArcBeginsTemp = globalInfo.aiOptions.distanceBehaviour[i].proximityRangeBegins;
											int pArcEndsTemp = globalInfo.aiOptions.distanceBehaviour[i].proximityRangeEnds;
											EditorGUI.indentLevel += 2;
											StyledMinMaxSlider("Proximity", ref globalInfo.aiOptions.distanceBehaviour[i].proximityRangeBegins, ref globalInfo.aiOptions.distanceBehaviour[i].proximityRangeEnds, 0, 100, EditorGUI.indentLevel);
											EditorGUI.indentLevel -= 2;
											if (globalInfo.aiOptions.distanceBehaviour[i].proximityRangeBegins != pArcBeginsTemp ||
											    globalInfo.aiOptions.distanceBehaviour[i].proximityRangeEnds != pArcEndsTemp){
												globalInfo.aiOptions.distanceBehaviour[i].characterDistance = CharacterDistance.Other;
											}
											
											EditorGUILayout.Space();
											
											EditorGUIUtility.labelWidth = 200;

											globalInfo.aiOptions.distanceBehaviour[i].movingForwardProbability = EditorGUILayout.Slider("Move Forward Probability:", globalInfo.aiOptions.distanceBehaviour[i].movingForwardProbability, 0, 1);
											globalInfo.aiOptions.distanceBehaviour[i].movingBackProbability = EditorGUILayout.Slider("Move Back Probability:", globalInfo.aiOptions.distanceBehaviour[i].movingBackProbability, 0, 1);
											globalInfo.aiOptions.distanceBehaviour[i].jumpingProbability = EditorGUILayout.Slider("Jump Probability:", globalInfo.aiOptions.distanceBehaviour[i].jumpingProbability, 0, 1);
											globalInfo.aiOptions.distanceBehaviour[i].crouchProbability = EditorGUILayout.Slider("Crouch Probability:", globalInfo.aiOptions.distanceBehaviour[i].crouchProbability, 0, 1);
											globalInfo.aiOptions.distanceBehaviour[i].attackProbability = EditorGUILayout.Slider("Attack Probability:", globalInfo.aiOptions.distanceBehaviour[i].attackProbability, 0, 1);

											EditorGUILayout.Space();
										}EditorGUILayout.EndVertical();
									}
									EditorGUILayout.Space();
									if (StyledButton("New Distance Behaviour"))
										globalInfo.aiOptions.distanceBehaviour = AddElement<AIDistanceBehaviour>(globalInfo.aiOptions.distanceBehaviour, null);
									
									EditorGUILayout.Space();
									EditorGUI.indentLevel -= 1;
								}EditorGUILayout.EndVertical();
							}


						}else if (globalInfo.aiOptions.engine == AIEngine.FuzzyAI && UFE.isAiAddonInstalled){
							
							EditorGUIUtility.labelWidth = 180;
							globalInfo.aiOptions.multiCoreSupport = EditorGUILayout.Toggle("Multi Core Support", globalInfo.aiOptions.multiCoreSupport);
							globalInfo.aiOptions.persistentBehavior = EditorGUILayout.Toggle("Persistent Behavior", globalInfo.aiOptions.persistentBehavior);

							globalInfo.aiOptions.selectedDifficultyLevel = (AIDifficultyLevel) EditorGUILayout.EnumPopup("Default Difficulty:", globalInfo.aiOptions.selectedDifficultyLevel, enumStyle);

							globalInfo.aiOptions.difficultyToggle = EditorGUILayout.Foldout(globalInfo.aiOptions.difficultyToggle, "Difficulty Settings ("+ globalInfo.aiOptions.difficultySettings.Length +")", foldStyle);
							if (globalInfo.aiOptions.difficultyToggle){
								EditorGUILayout.BeginVertical(subGroupStyle);{
									EditorGUILayout.Space();
									//EditorGUI.indentLevel += 1;
									
									for (int i = 0; i < globalInfo.aiOptions.difficultySettings.Length; i ++){
										EditorGUILayout.Space();
										EditorGUILayout.BeginVertical(subArrayElementStyle);{
											EditorGUILayout.Space();
											
											EditorGUIUtility.labelWidth = 160;
											EditorGUILayout.BeginHorizontal();{
												globalInfo.aiOptions.difficultySettings[i].difficultyLevel = (AIDifficultyLevel)EditorGUILayout.EnumPopup("Difficulty Level:", globalInfo.aiOptions.difficultySettings[i].difficultyLevel, enumStyle);

												if (GUILayout.Button("", "PaneOptions")){
													PaneOptions<AIDifficultySettings>(globalInfo.aiOptions.difficultySettings, globalInfo.aiOptions.difficultySettings[i], delegate (AIDifficultySettings[] newElement) { globalInfo.aiOptions.difficultySettings = newElement; });
												}
											}EditorGUILayout.EndHorizontal();

											
											SubGroupTitle("Override Instructions");
											
											EditorGUILayout.Space();
											
											EditorGUIUtility.labelWidth = 176;

											globalInfo.aiOptions.difficultySettings[i].startupBehavior = (AIBehavior)EditorGUILayout.EnumPopup("Startup Behavior:", globalInfo.aiOptions.difficultySettings[i].startupBehavior, enumStyle);

											DisableableSlider("Time Between Decisions:",
											                  ref globalInfo.aiOptions.difficultySettings[i].overrideTimeBetweenDecisions,
											                  ref globalInfo.aiOptions.difficultySettings[i].timeBetweenDecisions, 
											                  0f,
											                  .5f);

											DisableableSlider("Time Between Actions:",
											                  ref globalInfo.aiOptions.difficultySettings[i].overrideTimeBetweenActions,
											                  ref globalInfo.aiOptions.difficultySettings[i].timeBetweenActions, 
											                  0f,
											                  .5f);

											DisableableSlider("Rule Compliance:",
											                  ref globalInfo.aiOptions.difficultySettings[i].overrideRuleCompliance,
											                  ref globalInfo.aiOptions.difficultySettings[i].ruleCompliance, 
											                  0f,
											                  1f);

											DisableableSlider("Aggressiveness:",
											                  ref globalInfo.aiOptions.difficultySettings[i].overrideAggressiveness,
											                  ref globalInfo.aiOptions.difficultySettings[i].aggressiveness, 
											                  .1f,
											                  .9f);

											DisableableSlider("Combo Efficiency:",
											                  ref globalInfo.aiOptions.difficultySettings[i].overrideComboEfficiency,
											                  ref globalInfo.aiOptions.difficultySettings[i].comboEfficiency, 
											                  0f,
											                  1f);

											/*globalInfo.aiOptions.difficultySetup[i].timeBetweenActions = EditorGUILayout.Slider("Time Between Actions:", globalInfo.aiOptions.difficultySetup[i].timeBetweenActions, 0f, 0.5f);
											
											EditorGUILayout.Space();
											EditorGUIUtility.labelWidth = 160;
											globalInfo.aiOptions.difficultySetup[i].ruleCompliance = EditorGUILayout.Slider("Rule Compliance:", globalInfo.aiOptions.difficultySetup[i].ruleCompliance, 0f, 1f);
											globalInfo.aiOptions.difficultySetup[i].aggressiveness = EditorGUILayout.Slider("Aggressiveness:", globalInfo.aiOptions.difficultySetup[i].aggressiveness, 0.1f, 0.9f);
											globalInfo.aiOptions.difficultySetup[i].comboEfficiency = EditorGUILayout.Slider("Combo Efficiency:", globalInfo.aiOptions.difficultySetup[i].comboEfficiency, 0f, 1f);
											*/
											EditorGUIUtility.labelWidth = 150;

											EditorGUILayout.Space();

											EditorGUILayout.Space();
										}EditorGUILayout.EndVertical();
									}
									EditorGUILayout.Space();
									if (StyledButton("New Difficulty Setup"))
										globalInfo.aiOptions.difficultySettings = AddElement<AIDifficultySettings>(globalInfo.aiOptions.difficultySettings, null);
									
									EditorGUILayout.Space();
									//EditorGUI.indentLevel -= 1;
								}EditorGUILayout.EndVertical();
							}
						}else{
							GUILayout.BeginHorizontal("GroupBox");
							GUILayout.Label("You must have Fuzzy AI installed\n in order to use this option.", "CN EntryWarn");
							GUILayout.EndHorizontal();
						}
						EditorGUIUtility.labelWidth = 150;
						
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

			// Language Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					languageOptions = EditorGUILayout.Foldout(languageOptions, "Languages ("+ globalInfo.languages.Length +")", foldStyle);
					helpButton("global:languages");
				}EditorGUILayout.EndHorizontal();

				if (languageOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 160;
						for (int i = 0; i < globalInfo.languages.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									globalInfo.languages[i].languageName = EditorGUILayout.TextField("Language:", globalInfo.languages[i].languageName);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<LanguageOptions>(globalInfo.languages, globalInfo.languages[i], delegate (LanguageOptions[] newElement) { globalInfo.languages = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								
								bool defaultTemp = globalInfo.languages[i].defaultSelection;
								globalInfo.languages[i].defaultSelection = EditorGUILayout.Toggle("Default", globalInfo.languages[i].defaultSelection);
								if (defaultTemp != globalInfo.languages[i].defaultSelection){
									for (int t = 0; t < globalInfo.languages.Length; t ++){
										if (t != i) globalInfo.languages[t].defaultSelection = false;
									}
									globalInfo.languages[i].defaultSelection = true;
								}
								EditorGUILayout.Space();

								globalInfo.languages[i].start = EditorGUILayout.TextField("Start:", globalInfo.languages[i].start);
								globalInfo.languages[i].options = EditorGUILayout.TextField("Options:", globalInfo.languages[i].options);
								globalInfo.languages[i].credits = EditorGUILayout.TextField("Credits:", globalInfo.languages[i].credits);
								globalInfo.languages[i].selectYourCharacter = EditorGUILayout.TextField("Select Your Character:", globalInfo.languages[i].selectYourCharacter);
								globalInfo.languages[i].selectYourStage = EditorGUILayout.TextField("Select Your Stage:", globalInfo.languages[i].selectYourStage);
								globalInfo.languages[i].round = EditorGUILayout.TextField("Round:", globalInfo.languages[i].round);
								globalInfo.languages[i].finalRound = EditorGUILayout.TextField("Final Round:", globalInfo.languages[i].finalRound);
								globalInfo.languages[i].fight = EditorGUILayout.TextField("Fight:", globalInfo.languages[i].fight);
								globalInfo.languages[i].firstHit = EditorGUILayout.TextField("First Hit:", globalInfo.languages[i].firstHit);
								globalInfo.languages[i].combo = EditorGUILayout.TextField("Combo:", globalInfo.languages[i].combo);
								globalInfo.languages[i].parry = EditorGUILayout.TextField("Parry:", globalInfo.languages[i].parry);
								globalInfo.languages[i].counterHit = EditorGUILayout.TextField("Counter Hit:", globalInfo.languages[i].counterHit);
								globalInfo.languages[i].victory = EditorGUILayout.TextField("Victory:", globalInfo.languages[i].victory);
								globalInfo.languages[i].timeOver = EditorGUILayout.TextField("Time Over:", globalInfo.languages[i].timeOver);
								globalInfo.languages[i].perfect = EditorGUILayout.TextField("Perfect:", globalInfo.languages[i].perfect);
								globalInfo.languages[i].rematch = EditorGUILayout.TextField("Rematch:", globalInfo.languages[i].rematch);
								globalInfo.languages[i].quit = EditorGUILayout.TextField("Quit:", globalInfo.languages[i].quit);
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
							EditorGUILayout.Space();
						}
						
						if (StyledButton("New Language"))
							globalInfo.languages = AddElement<LanguageOptions>(globalInfo.languages, new LanguageOptions());
						
						EditorGUIUtility.labelWidth = 150;
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();


			// Camera Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					cameraOptions = EditorGUILayout.Foldout(cameraOptions, "Camera Options", foldStyle);
					helpButton("global:camera");
				}EditorGUILayout.EndHorizontal();

				if (cameraOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 200;
						globalInfo.cameraOptions.initialFieldOfView = EditorGUILayout.Slider("Field of View:", globalInfo.cameraOptions.initialFieldOfView, 1, 179);
						globalInfo.cameraOptions.initialDistance = EditorGUILayout.Vector3Field("Initial Distance:", globalInfo.cameraOptions.initialDistance);
						globalInfo.cameraOptions.initialDistance.x = 0;
						globalInfo.cameraOptions.initialRotation = EditorGUILayout.Vector3Field("Initial Rotation:", globalInfo.cameraOptions.initialRotation);
						globalInfo.cameraOptions.smooth = EditorGUILayout.FloatField("Smooth Translation:", globalInfo.cameraOptions.smooth);
						globalInfo.cameraOptions.minZoom = EditorGUILayout.FloatField("Minimum Zoom:", globalInfo.cameraOptions.minZoom);
						globalInfo.cameraOptions.maxZoom = EditorGUILayout.FloatField("Maximum Zoom:", globalInfo.cameraOptions.maxZoom);
						globalInfo.cameraOptions.maxDistance = EditorGUILayout.FloatField("Maximum Players Distance:", globalInfo.cameraOptions.maxDistance);
						globalInfo.cameraOptions.followJumpingCharacter = EditorGUILayout.Toggle("Follow Jumping Characters", globalInfo.cameraOptions.followJumpingCharacter);
						globalInfo.cameraOptions.enableLookAt = EditorGUILayout.Toggle("Enable LookAt", globalInfo.cameraOptions.enableLookAt);
						if (globalInfo.cameraOptions.enableLookAt)
							globalInfo.cameraOptions.heightOffSet = EditorGUILayout.FloatField("LookAt Height Offset:", globalInfo.cameraOptions.heightOffSet);
						EditorGUIUtility.labelWidth = 150;

						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

			
			// Character Rotation Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					characterRotationOptions = EditorGUILayout.Foldout(characterRotationOptions, "Character Rotation Options", foldStyle);
					helpButton("global:rotation");
				}EditorGUILayout.EndHorizontal();
				
				if (characterRotationOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 210;
						globalInfo.characterRotationOptions.autoMirror = EditorGUILayout.Toggle("Auto Mirror", globalInfo.characterRotationOptions.autoMirror);
						globalInfo.characterRotationOptions.rotateWhileJumping = EditorGUILayout.Toggle("Rotate While Jumping", globalInfo.characterRotationOptions.rotateWhileJumping);
						globalInfo.characterRotationOptions.rotateOnMoveOnly = EditorGUILayout.Toggle("Rotate On Move Only", globalInfo.characterRotationOptions.rotateOnMoveOnly);
						globalInfo.characterRotationOptions.fixRotationWhenStunned = EditorGUILayout.Toggle("Fix Rotation When Stunned", globalInfo.characterRotationOptions.fixRotationWhenStunned);
						globalInfo.characterRotationOptions.fixRotationWhenBlocking = EditorGUILayout.Toggle("Fix Rotation When Blocking", globalInfo.characterRotationOptions.fixRotationWhenBlocking);
						globalInfo.characterRotationOptions.rotationSpeed = EditorGUILayout.FloatField("Rotation Speed:", globalInfo.characterRotationOptions.rotationSpeed);
						globalInfo.characterRotationOptions.mirrorBlending = EditorGUILayout.FloatField("Mirror Blending (Mecanim only):", globalInfo.characterRotationOptions.mirrorBlending);
						EditorGUIUtility.labelWidth = 150;
						
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();


			// Round Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					roundOptions = EditorGUILayout.Foldout(roundOptions, "Round Options", foldStyle);
					helpButton("global:round");
				}EditorGUILayout.EndHorizontal();

				if (roundOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 210;
						globalInfo.roundOptions.totalRounds = EditorGUILayout.IntField("Total Rounds (Best of):", globalInfo.roundOptions.totalRounds);
						globalInfo.roundOptions.p1XPosition = EditorGUILayout.FloatField("Initial Spawn Position (P1):", globalInfo.roundOptions.p1XPosition);
						globalInfo.roundOptions.p2XPosition = EditorGUILayout.FloatField("Initial Spawn Position (P2):", globalInfo.roundOptions.p2XPosition);
						globalInfo.roundOptions.newRoundDelay = EditorGUILayout.FloatField("New Round Delay (seconds):", globalInfo.roundOptions.newRoundDelay);
						globalInfo.roundOptions.endGameDelay = EditorGUILayout.FloatField("End Game Delay (seconds):", globalInfo.roundOptions.endGameDelay);
						globalInfo.roundOptions.victoryMusic = (AudioClip) EditorGUILayout.ObjectField("Victory Music:", globalInfo.roundOptions.victoryMusic, typeof(UnityEngine.AudioClip), false);
						globalInfo.roundOptions.hasTimer = EditorGUILayout.Toggle("Has Timer", globalInfo.roundOptions.hasTimer);
						if (globalInfo.roundOptions.hasTimer){
							globalInfo.roundOptions.timer = EditorGUILayout.FloatField("Round Timer (seconds):", globalInfo.roundOptions.timer);
							globalInfo.roundOptions.timerSpeed = EditorGUILayout.FloatField("Timer Speed (%):", globalInfo.roundOptions.timerSpeed);
						}
						globalInfo.roundOptions.resetLifePoints = EditorGUILayout.Toggle("Reset life points", globalInfo.roundOptions.resetLifePoints);
						globalInfo.roundOptions.resetPositions = EditorGUILayout.Toggle("Reset positions", globalInfo.roundOptions.resetPositions);
						globalInfo.roundOptions.allowMovement = EditorGUILayout.Toggle("Allow movement before battle", globalInfo.roundOptions.allowMovement);
						globalInfo.roundOptions.slowMotionKO = EditorGUILayout.Toggle("Slow motion K.O.", globalInfo.roundOptions.slowMotionKO);
						//globalInfo.roundOptions.cameraZoomKO = EditorGUILayout.Toggle("Camera Zoom K.O.", globalInfo.roundOptions.cameraZoomKO);
						globalInfo.roundOptions.freezeCamAfterOutro = EditorGUILayout.Toggle("Freeze camera after outro", globalInfo.roundOptions.freezeCamAfterOutro);
						EditorGUIUtility.labelWidth = 150;
						
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

			// Bounce Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					bounceOptions = EditorGUILayout.Foldout(bounceOptions, "Bounce Options", foldStyle);
					helpButton("global:bounce");
				}EditorGUILayout.EndHorizontal();

				if (bounceOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 180;
						globalInfo.bounceOptions.bouncePrefab = (GameObject) EditorGUILayout.ObjectField("Bounce Effect:", globalInfo.bounceOptions.bouncePrefab, typeof(UnityEngine.GameObject), true);
						globalInfo.bounceOptions.bounceKillTime = EditorGUILayout.FloatField("Effect Duration:", globalInfo.bounceOptions.bounceKillTime);
						globalInfo.bounceOptions.bounceSound = (AudioClip) EditorGUILayout.ObjectField("Bounce Sound:", globalInfo.bounceOptions.bounceSound, typeof(UnityEngine.AudioClip), false);
						globalInfo.bounceOptions.minimumBounceForce = EditorGUILayout.FloatField("Minimum Bounce Force:", globalInfo.bounceOptions.minimumBounceForce);
						globalInfo.bounceOptions.bounceForce = (Sizes) EditorGUILayout.EnumPopup("Bounce Back Force:", globalInfo.bounceOptions.bounceForce, enumStyle);
						globalInfo.bounceOptions.maximumBounces = EditorGUILayout.FloatField("Maximum Bounces:", globalInfo.bounceOptions.maximumBounces);
						globalInfo.bounceOptions.bounceHitBoxes = EditorGUILayout.Toggle("Bounce Hit Boxes", globalInfo.bounceOptions.bounceHitBoxes);
						globalInfo.bounceOptions.shakeCamOnBounce = EditorGUILayout.Toggle("Shake Camera On Bounce", globalInfo.bounceOptions.shakeCamOnBounce);
						if (globalInfo.bounceOptions.shakeCamOnBounce){
							globalInfo.bounceOptions.shakeDensity = EditorGUILayout.FloatField("Shake Density:", globalInfo.bounceOptions.shakeDensity);
						}
						EditorGUIUtility.labelWidth = 150;

						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();
			
			// Counter Hit Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					counterHitOptions = EditorGUILayout.Foldout(counterHitOptions, "Counter Hit Options", foldStyle);
					helpButton("global:counterhit");
				}EditorGUILayout.EndHorizontal();
				
				if (counterHitOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 180;
						globalInfo.counterHitOptions.startUpFrames = EditorGUILayout.Toggle("Start Up Frames", globalInfo.counterHitOptions.startUpFrames);
						globalInfo.counterHitOptions.activeFrames = EditorGUILayout.Toggle("Active Frames", globalInfo.counterHitOptions.activeFrames);
						globalInfo.counterHitOptions.recoveryFrames = EditorGUILayout.Toggle("Recovery Frames", globalInfo.counterHitOptions.recoveryFrames);
						globalInfo.counterHitOptions.damageIncrease = EditorGUILayout.FloatField("Damage Increase (%):", globalInfo.counterHitOptions.damageIncrease);
						globalInfo.counterHitOptions.hitStunIncrease = EditorGUILayout.FloatField("Hit Stun Increase (%):", globalInfo.counterHitOptions.hitStunIncrease);
						globalInfo.counterHitOptions.sound = (AudioClip) EditorGUILayout.ObjectField("Sound File:", globalInfo.counterHitOptions.sound, typeof(UnityEngine.AudioClip), false);
						EditorGUIUtility.labelWidth = 150;
						
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

			// Combo Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					comboOptions = EditorGUILayout.Foldout(comboOptions, "Combo Options", foldStyle);
					helpButton("global:combo");
				}EditorGUILayout.EndHorizontal();

				if (comboOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 210;
						globalInfo.comboOptions.maxCombo = EditorGUILayout.IntField("Maximum Hits:", globalInfo.comboOptions.maxCombo);

						globalInfo.comboOptions.hitStunDeterioration = (Sizes) EditorGUILayout.EnumPopup("Hit Stun Deterioration:", globalInfo.comboOptions.hitStunDeterioration, enumStyle);
						EditorGUI.BeginDisabledGroup(globalInfo.comboOptions.hitStunDeterioration == Sizes.None);{
							globalInfo.comboOptions.minHitStun = EditorGUILayout.FloatField("Minimum Hit Stun (frames):", globalInfo.comboOptions.minHitStun);
						}EditorGUI.EndDisabledGroup();

						globalInfo.comboOptions.damageDeterioration = (Sizes) EditorGUILayout.EnumPopup("Damage Deterioration:", globalInfo.comboOptions.damageDeterioration, enumStyle);
						EditorGUI.BeginDisabledGroup(globalInfo.comboOptions.damageDeterioration == Sizes.None);{
							globalInfo.comboOptions.minDamage = EditorGUILayout.FloatField("Minimum Damage:", globalInfo.comboOptions.minDamage);
						}EditorGUI.EndDisabledGroup();

						globalInfo.comboOptions.airJuggleDeterioration = (Sizes) EditorGUILayout.EnumPopup("Air-Juggle Deterioration:", globalInfo.comboOptions.airJuggleDeterioration, enumStyle);
						globalInfo.comboOptions.airJuggleDeteriorationType = (AirJuggleDeteriorationType) EditorGUILayout.EnumPopup("Air-Juggle Deterioration Type:", globalInfo.comboOptions.airJuggleDeteriorationType, enumStyle);
						EditorGUI.BeginDisabledGroup(globalInfo.comboOptions.airJuggleDeterioration == Sizes.None);{
							globalInfo.comboOptions.minPushForce = EditorGUILayout.FloatField("Minimum Juggle Force (Y):", globalInfo.comboOptions.minPushForce);
						}EditorGUI.EndDisabledGroup();
						
						globalInfo.comboOptions.knockBackMinForce = EditorGUILayout.FloatField("Mininum Knock Back Force (X):", globalInfo.comboOptions.knockBackMinForce);
						globalInfo.comboOptions.neverAirRecover = EditorGUILayout.Toggle("Never Air-Recover", globalInfo.comboOptions.neverAirRecover);
						globalInfo.comboOptions.resetFallingForceOnHit = EditorGUILayout.Toggle("Reset Falling Force On Hit", globalInfo.comboOptions.resetFallingForceOnHit);

						globalInfo.comboOptions.neverCornerPush = EditorGUILayout.Toggle("Never Corner Push", globalInfo.comboOptions.neverCornerPush);

						globalInfo.comboOptions.fixJuggleWeight = EditorGUILayout.Toggle("Fixed Juggle Weight", globalInfo.comboOptions.fixJuggleWeight);
						globalInfo.comboOptions.juggleWeight = EditorGUILayout.FloatField("Juggle Weight:", globalInfo.comboOptions.juggleWeight);

						EditorGUIUtility.labelWidth = 150;
						
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

			
			// Block Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					blockOptions = EditorGUILayout.Foldout(blockOptions, "Block Options", foldStyle);
					helpButton("global:block");
				}EditorGUILayout.EndHorizontal();

				if (blockOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						EditorGUIUtility.labelWidth = 180;

						SubGroupTitle("Block");

						globalInfo.blockOptions.blockType = (BlockType) EditorGUILayout.EnumPopup("Block Input:", globalInfo.blockOptions.blockType, enumStyle);
						EditorGUI.BeginDisabledGroup(globalInfo.blockOptions.blockType == BlockType.None);{
                            globalInfo.blockOptions.allowAirBlock = EditorGUILayout.Toggle("Allow Air Block", globalInfo.blockOptions.allowAirBlock);
                            globalInfo.blockOptions.ignoreAppliedForceBlock = EditorGUILayout.Toggle("Ignore Applied Forces", globalInfo.blockOptions.ignoreAppliedForceBlock);

						}EditorGUI.EndDisabledGroup();

						EditorGUILayout.Space();
						
						SubGroupTitle("Parry");

						globalInfo.blockOptions.parryType = (ParryType) EditorGUILayout.EnumPopup("Parry Input:", globalInfo.blockOptions.parryType, enumStyle);
                        EditorGUI.BeginDisabledGroup(globalInfo.blockOptions.parryType == ParryType.None);
                        {
                            globalInfo.blockOptions.parryTiming = EditorGUILayout.FloatField("Parry Timing:", globalInfo.blockOptions.parryTiming);
                            globalInfo.blockOptions.parryStunType = (ParryStunType)EditorGUILayout.EnumPopup("Parry Stun Type:", globalInfo.blockOptions.parryStunType, enumStyle);
                            if (globalInfo.blockOptions.parryStunType == ParryStunType.Fixed) {
                                globalInfo.blockOptions.parryStunFrames = EditorGUILayout.IntField("Parry Stun (Frames):", globalInfo.blockOptions.parryStunFrames);
                            } else {
                                globalInfo.blockOptions.parryStunFrames = EditorGUILayout.IntSlider("Parry Stun Percentage:", globalInfo.blockOptions.parryStunFrames, 1, 100);
                            }

                            globalInfo.blockOptions.highlightWhenParry = EditorGUILayout.Toggle("Highlight When Parry", globalInfo.blockOptions.highlightWhenParry);
                            EditorGUI.BeginDisabledGroup(!globalInfo.blockOptions.highlightWhenParry);{
                                globalInfo.blockOptions.parryColor = EditorGUILayout.ColorField("Parry Color Mask:", globalInfo.blockOptions.parryColor);
                            } EditorGUI.EndDisabledGroup();

                            globalInfo.blockOptions.allowAirParry = EditorGUILayout.Toggle("Allow Air Parry", globalInfo.blockOptions.allowAirParry);
                            globalInfo.blockOptions.ignoreAppliedForceParry = EditorGUILayout.Toggle("Ignore Applied Forces", globalInfo.blockOptions.ignoreAppliedForceParry);
                            globalInfo.blockOptions.resetButtonSequence = EditorGUILayout.Toggle("Reset Button Sequence", globalInfo.blockOptions.resetButtonSequence);
                        } EditorGUI.EndDisabledGroup();
						
						EditorGUIUtility.labelWidth = 150;

						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();


			// Knock Down Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					knockDownOptions = EditorGUILayout.Foldout(knockDownOptions, "Knock Down Options", foldStyle);
					helpButton("global:knockdown");
				}EditorGUILayout.EndHorizontal();

				if (knockDownOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 180;
						KnockdownOptionsBlock("Default Knockdown", globalInfo.knockDownOptions.air);
						KnockdownOptionsBlock("High Knockdown", globalInfo.knockDownOptions.high);
						KnockdownOptionsBlock("Mid Knockdown", globalInfo.knockDownOptions.highLow);
						KnockdownOptionsBlock("Sweep Knockdown", globalInfo.knockDownOptions.sweep);
						KnockdownOptionsBlock("Crumple Knockdown", globalInfo.knockDownOptions.crumple);
						EditorGUIUtility.labelWidth = 150;


						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

			
			// Hit Effects Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					hitOptions = EditorGUILayout.Foldout(hitOptions, "Hit Effect Options", foldStyle);
					helpButton("global:hitEffects");
				}EditorGUILayout.EndHorizontal();

				if (hitOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;

						EditorGUIUtility.labelWidth = 220;
						HitOptionBlock("Weak Hit Options", globalInfo.hitOptions.weakHit);
						HitOptionBlock("Medium Hit Options", globalInfo.hitOptions.mediumHit);
						HitOptionBlock("Heavy Hit Options", globalInfo.hitOptions.heavyHit);
						HitOptionBlock("Crumple Hit Options", globalInfo.hitOptions.crumpleHit);

						EditorGUILayout.Space();
						
						HitOptionBlock("Block Hit Options", globalInfo.blockOptions.blockHitEffects, true);
						HitOptionBlock("Parry Hit Options", globalInfo.blockOptions.parryHitEffects, true);

						EditorGUILayout.Space();

						HitOptionBlock("Custom Hit 1 Options", globalInfo.hitOptions.customHit1);
						HitOptionBlock("Custom Hit 2 Options", globalInfo.hitOptions.customHit2);
						HitOptionBlock("Custom Hit 3 Options", globalInfo.hitOptions.customHit3);

						EditorGUILayout.Space();

						globalInfo.hitOptions.resetAnimationOnHit = EditorGUILayout.Toggle("Restart Animation on Hit", globalInfo.hitOptions.resetAnimationOnHit);
						globalInfo.hitOptions.useHitStunDeceleration = EditorGUILayout.Toggle("Animation Deceleration Effect", globalInfo.hitOptions.useHitStunDeceleration);
						EditorGUIUtility.labelWidth = 150;
						
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();


			// Inputs
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					inputsOptions = EditorGUILayout.Foldout(inputsOptions, "Input Options", foldStyle);
					helpButton("global:input");
				}EditorGUILayout.EndHorizontal();

				if (inputsOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;

						string errorMsg = null;

						globalInfo.inputOptions.inputManagerType = (InputManagerType) EditorGUILayout.EnumPopup("Input Manager:", globalInfo.inputOptions.inputManagerType, enumStyle);
						if (globalInfo.inputOptions.inputManagerType == InputManagerType.cInput && !UFE.isCInputInstalled){
							errorMsg = "You must have cInput installed\n in order to use this option.";
#if !UFE_BASIC
						}else if (globalInfo.inputOptions.inputManagerType == InputManagerType.ControlFreak && !UFE.isControlFreakInstalled){
							errorMsg = "You must have Control Freak installed\n in order to use this option.";
#endif
						}

						if (errorMsg != null){
							GUILayout.BeginHorizontal("GroupBox");
							GUILayout.Label(errorMsg, "CN EntryWarn");
							GUILayout.EndHorizontal();
						}else{
							player1InputOptions = EditorGUILayout.Foldout(player1InputOptions, "Player 1 Inputs ("+ globalInfo.player1_Inputs.Length +")", foldStyle);
							if (player1InputOptions) globalInfo.player1_Inputs = PlayerInputsBlock(globalInfo.player1_Inputs);
							
							player2InputOptions = EditorGUILayout.Foldout(player2InputOptions, "Player 2 Inputs ("+ globalInfo.player2_Inputs.Length +")", foldStyle);
							if (player2InputOptions) globalInfo.player2_Inputs = PlayerInputsBlock(globalInfo.player2_Inputs);

                            if (globalInfo.inputOptions.inputManagerType == InputManagerType.cInput) {
                                EditorGUILayout.Space();
                                cInputOptions = EditorGUILayout.Foldout(cInputOptions, "cInput Preferences", foldStyle);
                                if (cInputOptions) CInputPreferences();
#if !UFE_BASIC
							} else if (globalInfo.inputOptions.inputManagerType == InputManagerType.ControlFreak){
								EditorGUILayout.Space();
								touchControllerOptions = EditorGUILayout.Foldout(touchControllerOptions, "Control Freak Preferences", foldStyle);
								if (touchControllerOptions) ControlFreakPreferences();
#endif
							}
						}
						
						EditorGUILayout.Space();
						EditorGUIUtility.labelWidth = 180;
						globalInfo.inputOptions.confirmButton = (ButtonPress) EditorGUILayout.EnumPopup("Confirm Button:", globalInfo.inputOptions.confirmButton, enumStyle);
						globalInfo.inputOptions.cancelButton = (ButtonPress) EditorGUILayout.EnumPopup("Cancel Button:", globalInfo.inputOptions.cancelButton, enumStyle);
						EditorGUIUtility.labelWidth = 150;



						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

			
			// Stages
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					stageOptions = EditorGUILayout.Foldout(stageOptions, "Stages ("+ globalInfo.stages.Length +")", foldStyle);
					helpButton("global:stages");
				}EditorGUILayout.EndHorizontal();

				if (stageOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						for (int i = 0; i < globalInfo.stages.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									globalInfo.stages[i].prefab = (GameObject) EditorGUILayout.ObjectField("Stage Prefab:", globalInfo.stages[i].prefab, typeof(UnityEngine.GameObject), true);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<StageOptions>(
											globalInfo.stages, 
											globalInfo.stages[i], 
											delegate (StageOptions[] newElement){
												globalInfo.stages = newElement;
												globalInfo.ValidateStoryModeInformation();
											}
										);
									}
								}EditorGUILayout.EndHorizontal();
								globalInfo.stages[i].stageName = EditorGUILayout.TextField("Name:", globalInfo.stages[i].stageName);
								globalInfo.stages[i].music = (AudioClip) EditorGUILayout.ObjectField("Music:", globalInfo.stages[i].music, typeof(UnityEngine.AudioClip), true);
								globalInfo.stages[i].leftBoundary = EditorGUILayout.FloatField("Left Boundary:", globalInfo.stages[i].leftBoundary);
								globalInfo.stages[i].rightBoundary = EditorGUILayout.FloatField("Right Boundary:", globalInfo.stages[i].rightBoundary);
								globalInfo.stages[i].groundFriction = EditorGUILayout.FloatField("Ground Friction:", globalInfo.stages[i].groundFriction);
								EditorGUILayout.LabelField("Screenshot:");
								globalInfo.stages[i].screenshot = (Texture2D) EditorGUILayout.ObjectField(globalInfo.stages[i].screenshot, typeof(Texture2D), false);

								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
							EditorGUILayout.Space();
						}
						EditorGUI.indentLevel -= 1;
						
						if (StyledButton("New Stage")){
							globalInfo.stages = AddElement<StageOptions>(globalInfo.stages, new StageOptions());
							globalInfo.ValidateStoryModeInformation();
						}
						
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();


			// Characters
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					characterOptions = EditorGUILayout.Foldout(characterOptions, "Characters ("+ globalInfo.characters.Length +")", foldStyle);
					helpButton("global:characters");
				}EditorGUILayout.EndHorizontal();

				if (characterOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						for (int i = 0; i < globalInfo.characters.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									globalInfo.characters[i] = (CharacterInfo)EditorGUILayout.ObjectField("Character File:", globalInfo.characters[i], typeof(CharacterInfo), false);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<CharacterInfo>(
											globalInfo.characters, 
											globalInfo.characters[i], 
											delegate (CharacterInfo[] newElement) { 
												globalInfo.characters = newElement; 
												globalInfo.ValidateStoryModeInformation();
											}
										);
									}
								}EditorGUILayout.EndHorizontal();
								
								if (GUILayout.Button("Open in the Character Editor")) {
									CharacterEditorWindow.sentCharacterInfo = globalInfo.characters[i];
									CharacterEditorWindow.Init();
								}
							}EditorGUILayout.EndVertical();
						}
						EditorGUI.indentLevel -= 1;
						
						EditorGUILayout.Space();
						if (StyledButton("New Character")){
							globalInfo.characters = AddElement<CharacterInfo>(globalInfo.characters, null);
							globalInfo.storyMode.selectableCharactersInStoryMode.Add(globalInfo.characters.Length - 1);
							globalInfo.storyMode.selectableCharactersInVersusMode.Add(globalInfo.characters.Length - 1);
							globalInfo.storyMode.characterStories.Add(new CharacterStory());
							globalInfo.ValidateStoryModeInformation();
						}
						
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();


			// Screen Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					screenOptions = EditorGUILayout.Foldout(screenOptions, "GUI Options", foldStyle);
					helpButton("global:gui");
				}EditorGUILayout.EndHorizontal();
				
				if (screenOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						EditorGUIUtility.labelWidth = 180;
						
						globalInfo.gameGUI.defaultFadeDuration = Mathf.Max(0f, EditorGUILayout.FloatField("Default Fade Duration:", globalInfo.gameGUI.defaultFadeDuration));
						globalInfo.gameGUI.hasGauge = EditorGUILayout.Toggle("Gauge/Meter", globalInfo.gameGUI.hasGauge);
						globalInfo.gameGUI.useCanvasScaler = EditorGUILayout.Toggle("Use Canvas Scaler", globalInfo.gameGUI.useCanvasScaler);
						
						EditorGUILayout.Space();
						
						if (globalInfo.gameGUI.useCanvasScaler){
							EditorGUILayout.BeginVertical(this.subGroupStyle);{
								EditorGUILayout.Space();
								globalInfo.gameGUI.canvasScaler.scaleMode = (CanvasScaler.ScaleMode)EditorGUILayout.EnumPopup("Scale Mode:", globalInfo.gameGUI.canvasScaler.scaleMode, enumStyle);
								if (globalInfo.gameGUI.canvasScaler.scaleMode == CanvasScaler.ScaleMode.ConstantPhysicalSize){
									globalInfo.gameGUI.canvasScaler.referencePixelsPerUnit = EditorGUILayout.FloatField("Reference Pixels Per Unit:", globalInfo.gameGUI.canvasScaler.referencePixelsPerUnit);
									globalInfo.gameGUI.canvasScaler.physicalUnit = (CanvasScaler.Unit) EditorGUILayout.EnumPopup("Physical Unit:", globalInfo.gameGUI.canvasScaler.physicalUnit, enumStyle);
									globalInfo.gameGUI.canvasScaler.fallbackScreenDPI = EditorGUILayout.FloatField("Fallback Screen DPI:", globalInfo.gameGUI.canvasScaler.fallbackScreenDPI);
									globalInfo.gameGUI.canvasScaler.defaultSpriteDPI = EditorGUILayout.FloatField("Default Sprite DPI:", globalInfo.gameGUI.canvasScaler.defaultSpriteDPI);
								}else if (globalInfo.gameGUI.canvasScaler.scaleMode == CanvasScaler.ScaleMode.ConstantPixelSize){
									globalInfo.gameGUI.canvasScaler.referencePixelsPerUnit = EditorGUILayout.FloatField("Reference Pixels Per Unit:", globalInfo.gameGUI.canvasScaler.referencePixelsPerUnit);
									globalInfo.gameGUI.canvasScaler.scaleFactor = EditorGUILayout.FloatField("Scale Factor:", globalInfo.gameGUI.canvasScaler.scaleFactor);
								}else if (globalInfo.gameGUI.canvasScaler.scaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize){
									globalInfo.gameGUI.canvasScaler.referencePixelsPerUnit = EditorGUILayout.FloatField("Reference Pixels Per Unit:", globalInfo.gameGUI.canvasScaler.referencePixelsPerUnit);
									globalInfo.gameGUI.canvasScaler.screenMatchMode = (CanvasScaler.ScreenMatchMode)EditorGUILayout.EnumPopup("Screen Match Mode:", globalInfo.gameGUI.canvasScaler.screenMatchMode, enumStyle);
									if (globalInfo.gameGUI.canvasScaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight){
										globalInfo.gameGUI.canvasScaler.matchWidthOrHeight = EditorGUILayout.Slider("Match Width or Height:", globalInfo.gameGUI.canvasScaler.matchWidthOrHeight, 0f, 1f);
									}
									globalInfo.gameGUI.canvasScaler.referenceResolution = EditorGUILayout.Vector2Field("Resolution:", globalInfo.gameGUI.canvasScaler.referenceResolution);
								}
                                EditorGUI.BeginDisabledGroup(canvasPreview == null);{
									if (GUILayout.Button("Update Canvas Preview")) UpdateGUICanvas();
                                } EditorGUI.EndDisabledGroup();
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
						
						EditorGUILayout.Space();
						
						EditorGUILayout.BeginHorizontal();{
							guiScreensOptions = EditorGUILayout.Foldout(guiScreensOptions, "Screens", foldStyle);
						}EditorGUILayout.EndHorizontal();

                        if (guiScreensOptions) {
                            EditorGUIUtility.labelWidth = 150;
							EditorGUILayout.BeginVertical(this.subGroupStyle);{
								EditorGUILayout.Space();
								
                                SubGroupTitle("Main");
                                EditorGUILayout.BeginHorizontal();{
                                    globalInfo.gameGUI.mainMenuScreen = (MainMenuScreen)EditorGUILayout.ObjectField("Main Menu:", globalInfo.gameGUI.mainMenuScreen, typeof(MainMenuScreen), true);
                                    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.mainMenuScreen));{
                                       // if (GUILayout.Button("Open", GUILayout.Width(45))) OpenGUICanvas(globalInfo.gameGUI.mainMenuScreen);
                                        ScreenButton(globalInfo.gameGUI.mainMenuScreen);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();{
                                    globalInfo.gameGUI.optionsScreen = (OptionsScreen)EditorGUILayout.ObjectField("Options:", globalInfo.gameGUI.optionsScreen, typeof(OptionsScreen), true);
                                    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.optionsScreen));{
                                        ScreenButton(globalInfo.gameGUI.optionsScreen);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();
                                
								EditorGUILayout.BeginHorizontal();{
								    globalInfo.gameGUI.characterSelectionScreen = (CharacterSelectionScreen)EditorGUILayout.ObjectField("Character Selection:", globalInfo.gameGUI.characterSelectionScreen, typeof(CharacterSelectionScreen), true);
                                    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.characterSelectionScreen));{
                                        ScreenButton(globalInfo.gameGUI.characterSelectionScreen);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();
                                
								EditorGUILayout.BeginHorizontal();{
								    globalInfo.gameGUI.stageSelectionScreen = (StageSelectionScreen)EditorGUILayout.ObjectField("Stage Selection:", globalInfo.gameGUI.stageSelectionScreen, typeof(StageSelectionScreen), true);
                                    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.stageSelectionScreen));{
                                        ScreenButton(globalInfo.gameGUI.stageSelectionScreen);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();
                                
								EditorGUILayout.BeginHorizontal();{
								    globalInfo.gameGUI.loadingBattleScreen = (LoadingBattleScreen)EditorGUILayout.ObjectField("Loading Screen:", globalInfo.gameGUI.loadingBattleScreen, typeof(LoadingBattleScreen), true);
                                    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.loadingBattleScreen));{
                                        ScreenButton(globalInfo.gameGUI.loadingBattleScreen);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();

                                
								EditorGUILayout.BeginHorizontal();{
								    globalInfo.gameGUI.battleGUI = (BattleGUI)EditorGUILayout.ObjectField("Battle GUI:", globalInfo.gameGUI.battleGUI, typeof(BattleGUI), true);
                                    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.battleGUI));{
                                        ScreenButton(globalInfo.gameGUI.battleGUI);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();

								EditorGUILayout.BeginHorizontal();{
									globalInfo.gameGUI.pauseScreen = (PauseScreen)EditorGUILayout.ObjectField("Pause Screen:", globalInfo.gameGUI.pauseScreen, typeof(PauseScreen), true);
									EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.pauseScreen));{
										ScreenButton(globalInfo.gameGUI.pauseScreen);
									} EditorGUI.EndDisabledGroup();
								} EditorGUILayout.EndHorizontal();

								EditorGUILayout.Space();


                                SubGroupTitle("Extras");
                                EditorGUILayout.BeginHorizontal();
                                {
                                    globalInfo.gameGUI.introScreen = (IntroScreen)EditorGUILayout.ObjectField("Intro Screen:", globalInfo.gameGUI.introScreen, typeof(IntroScreen), true, GUILayout.ExpandWidth(true));
                                    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.introScreen));{
                                        ScreenButton(globalInfo.gameGUI.introScreen);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();


                                EditorGUILayout.BeginHorizontal();
                                {
                                    globalInfo.gameGUI.creditsScreen = (CreditsScreen)EditorGUILayout.ObjectField("Credits:", globalInfo.gameGUI.creditsScreen, typeof(CreditsScreen), true);
                                    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.creditsScreen));{
                                        ScreenButton(globalInfo.gameGUI.creditsScreen);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();

                                EditorGUILayout.Space();


								SubGroupTitle("Story Mode");
								EditorGUILayout.BeginHorizontal();{
								    globalInfo.gameGUI.storyModeContinueScreen = (StoryModeContinueScreen)EditorGUILayout.ObjectField("Continue?:", globalInfo.gameGUI.storyModeContinueScreen, typeof(StoryModeContinueScreen), true);
                                    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.storyModeContinueScreen));{
                                        ScreenButton(globalInfo.gameGUI.storyModeContinueScreen);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();
                                
								EditorGUILayout.BeginHorizontal();{
								    globalInfo.gameGUI.storyModeGameOverScreen = (StoryModeScreen)EditorGUILayout.ObjectField("Game Over:", globalInfo.gameGUI.storyModeGameOverScreen, typeof(StoryModeScreen), true);
                                    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.storyModeGameOverScreen));{
                                        ScreenButton(globalInfo.gameGUI.storyModeGameOverScreen);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();

								EditorGUILayout.BeginHorizontal();{
								    globalInfo.gameGUI.storyModeCongratulationsScreen = (StoryModeScreen)EditorGUILayout.ObjectField("Congratulations:", globalInfo.gameGUI.storyModeCongratulationsScreen, typeof(StoryModeScreen), true);
								    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.storyModeCongratulationsScreen));{
                                        ScreenButton(globalInfo.gameGUI.storyModeCongratulationsScreen);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();

								EditorGUILayout.Space();
								

								SubGroupTitle("Versus Mode");
								EditorGUILayout.BeginHorizontal();{
								    globalInfo.gameGUI.versusModeScreen = (VersusModeScreen)EditorGUILayout.ObjectField("Options:", globalInfo.gameGUI.versusModeScreen, typeof(VersusModeScreen), true);
                                    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.versusModeScreen));{
                                        ScreenButton(globalInfo.gameGUI.versusModeScreen);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();
                                
								EditorGUILayout.BeginHorizontal();{
								    globalInfo.gameGUI.versusModeAfterBattleScreen = (VersusModeAfterBattleScreen)EditorGUILayout.ObjectField("After Battle:", globalInfo.gameGUI.versusModeAfterBattleScreen, typeof(VersusModeAfterBattleScreen), true);
                                    EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.versusModeAfterBattleScreen));{
                                        ScreenButton(globalInfo.gameGUI.versusModeAfterBattleScreen);
                                    } EditorGUI.EndDisabledGroup();
                                } EditorGUILayout.EndHorizontal();
								
								if (UFE.isNetworkAddonInstalled){
                                    EditorGUILayout.Space();

                                    SubGroupTitle("Network Mode");
									
								    EditorGUILayout.BeginHorizontal();{
									    globalInfo.gameGUI.networkGameScreen = (NetworkGameScreen)EditorGUILayout.ObjectField("Network Game:", globalInfo.gameGUI.networkGameScreen, typeof(NetworkGameScreen), true);
                                        EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.networkGameScreen));{
                                            ScreenButton(globalInfo.gameGUI.networkGameScreen);
                                        } EditorGUI.EndDisabledGroup();
                                    } EditorGUILayout.EndHorizontal();
                                    
								    EditorGUILayout.BeginHorizontal();{
									    globalInfo.gameGUI.hostGameScreen = (HostGameScreen)EditorGUILayout.ObjectField("Host Game:", globalInfo.gameGUI.hostGameScreen, typeof(HostGameScreen), true);
                                        EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.hostGameScreen));{
                                            ScreenButton(globalInfo.gameGUI.hostGameScreen);
                                        } EditorGUI.EndDisabledGroup();
                                    } EditorGUILayout.EndHorizontal();
                                    
								    EditorGUILayout.BeginHorizontal();{
									    globalInfo.gameGUI.joinGameScreen = (JoinGameScreen)EditorGUILayout.ObjectField("Join Game:", globalInfo.gameGUI.joinGameScreen, typeof(JoinGameScreen), true);
                                        EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.joinGameScreen));{
                                            ScreenButton(globalInfo.gameGUI.joinGameScreen);
                                        } EditorGUI.EndDisabledGroup();
                                    } EditorGUILayout.EndHorizontal();
                                    
								    EditorGUILayout.BeginHorizontal();{
									    globalInfo.gameGUI.connectionLostScreen = (ConnectionLostScreen)EditorGUILayout.ObjectField("Connection Lost:", globalInfo.gameGUI.connectionLostScreen, typeof(ConnectionLostScreen), true);
                                        EditorGUI.BeginDisabledGroup(DisableScreenButton(globalInfo.gameGUI.connectionLostScreen));{
                                            ScreenButton(globalInfo.gameGUI.connectionLostScreen);
                                        } EditorGUI.EndDisabledGroup();
                                    } EditorGUILayout.EndHorizontal();

								}
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
                        } else if (!storyModeOptions){
                            CloseGUICanvas();
                        }
						
						EditorGUILayout.Space();
						EditorGUIUtility.labelWidth = 150;
						EditorGUI.indentLevel -= 1;
						
					}EditorGUILayout.EndVertical();
                } else if (!storyModeOptions) {
                    CloseGUICanvas();
                }
			}EditorGUILayout.EndVertical();
			

			// Story Mode Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					storyModeOptions = EditorGUILayout.Foldout(storyModeOptions, "Story Mode Options", foldStyle);
					helpButton("global:storymode");
				}EditorGUILayout.EndHorizontal();
				
				if (storyModeOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						EditorGUILayout.Space();
						EditorGUIUtility.labelWidth = 180;

						EditorGUILayout.BeginVertical(subGroupStyle);{
							EditorGUILayout.Space();
							EditorGUILayout.BeginHorizontal();{
								storyModeSelectableCharactersInStoryModeOptions = EditorGUILayout.Foldout(storyModeSelectableCharactersInStoryModeOptions, "Selectable characters (Story Mode)", foldStyle);
							}EditorGUILayout.EndHorizontal();

							if (storyModeSelectableCharactersInStoryModeOptions){
								EditorGUI.indentLevel += 1;
								for (int i = 0; i < globalInfo.characters.Length; i ++){
									EditorGUILayout.Space();
									EditorGUILayout.BeginVertical(arrayElementStyle);{
										EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();{
											string name;
											if (globalInfo.characters[i] != null){
												name = globalInfo.characters[i].characterName;
											}else{
												name = "Character " + i;
											}

											bool oldValue = globalInfo.storyMode.selectableCharactersInStoryMode.Contains(i);
											bool newValue = EditorGUILayout.Toggle(name, oldValue);
											
											if (oldValue != newValue){
												if (newValue){
													globalInfo.storyMode.selectableCharactersInStoryMode.Add(i);
													globalInfo.storyMode.characterStories.Add(new CharacterStory());
												}else{
													int index = globalInfo.storyMode.selectableCharactersInStoryMode.IndexOf(i);
													globalInfo.storyMode.selectableCharactersInStoryMode.RemoveAt(index);
													globalInfo.storyMode.characterStories.RemoveAt(index);
												}
											}
										}EditorGUILayout.EndHorizontal();
										EditorGUILayout.Space();
									}EditorGUILayout.EndVertical();
								}
								EditorGUI.indentLevel -= 1;
							}
							EditorGUILayout.Space();

						}EditorGUILayout.EndVertical();

						EditorGUILayout.Space();

						EditorGUILayout.BeginVertical(subGroupStyle);{
							EditorGUILayout.Space();
							EditorGUILayout.BeginHorizontal();{
								storyModeSelectableCharactersInVersusModeOptions = EditorGUILayout.Foldout(storyModeSelectableCharactersInVersusModeOptions, "Selectable characters (Versus Mode)", foldStyle);
							}EditorGUILayout.EndHorizontal();
							
							if (storyModeSelectableCharactersInVersusModeOptions){
								EditorGUI.indentLevel += 1;
								for (int i = 0; i < globalInfo.characters.Length; i ++){
									EditorGUILayout.Space();
									EditorGUILayout.BeginVertical(arrayElementStyle);{
										EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();{
											string name;
											if (globalInfo.characters[i] != null){
												name = globalInfo.characters[i].characterName;
											}else{
												name = "Character " + i;
											}

											bool oldValue = globalInfo.storyMode.selectableCharactersInVersusMode.Contains(i);
											bool newValue = EditorGUILayout.Toggle(name, oldValue);
											
											if (oldValue != newValue){
												if (newValue){
													globalInfo.storyMode.selectableCharactersInVersusMode.Add(i);
												}else{
													int index = globalInfo.storyMode.selectableCharactersInVersusMode.IndexOf(i);
													globalInfo.storyMode.selectableCharactersInVersusMode.RemoveAt(index);
												}
											}
										}EditorGUILayout.EndHorizontal();
										EditorGUILayout.Space();
									}EditorGUILayout.EndVertical();
								}
								EditorGUI.indentLevel -= 1;
							}
							EditorGUILayout.Space();
							
						}EditorGUILayout.EndVertical();

						EditorGUILayout.Space();
						EditorGUILayout.Space();
						EditorGUIUtility.labelWidth = 250;

						globalInfo.storyMode.canCharactersFightAgainstThemselves = EditorGUILayout.Toggle("Allow mirror matches", globalInfo.storyMode.canCharactersFightAgainstThemselves);
						globalInfo.storyMode.useSameStoryForAllCharacters = EditorGUILayout.Toggle("Use the same story for all characters", globalInfo.storyMode.useSameStoryForAllCharacters);

						EditorGUIUtility.labelWidth = 180;
						EditorGUILayout.Space();
						EditorGUILayout.Space();

						if (globalInfo.storyMode.useSameStoryForAllCharacters){
							this.EditCharacterStory(globalInfo.storyMode.defaultStory);
						}else{
							for (int i = 0; i < globalInfo.characters.Length; i ++){
								if (globalInfo.storyMode.selectableCharactersInStoryMode.Contains(i)){
									string name;
									if (globalInfo.characters[i] != null){
										name = globalInfo.characters[i].characterName;
									}else{
										name = "Character " + i;
									}
									this.EditCharacterStory(globalInfo.storyMode.characterStories[i], name + "'s Story");
								}
							}
						}
						
						EditorGUIUtility.labelWidth = 150;
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();


            // Training Mode Options
            EditorGUILayout.BeginVertical(rootGroupStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    trainingModeOptions = EditorGUILayout.Foldout(trainingModeOptions, "Training Mode Options", foldStyle);
                    helpButton("global:trainingmode");
                } EditorGUILayout.EndHorizontal();

                if (trainingModeOptions) {
                    EditorGUILayout.BeginVertical(subGroupStyle);
                    {
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel += 1;
                        EditorGUIUtility.labelWidth = 200;
                        globalInfo.trainingModeOptions.inputInfo = EditorGUILayout.Toggle("Display Input", globalInfo.trainingModeOptions.inputInfo);
                        globalInfo.trainingModeOptions.freezeTime = EditorGUILayout.Toggle("Freeze Timer", globalInfo.trainingModeOptions.freezeTime);
                        globalInfo.trainingModeOptions.p1Life = (LifeBarTrainingMode)EditorGUILayout.EnumPopup("Player 1 Life:", globalInfo.trainingModeOptions.p1Life, enumStyle);
                        globalInfo.trainingModeOptions.p2Life = (LifeBarTrainingMode)EditorGUILayout.EnumPopup("Player 2 Life:", globalInfo.trainingModeOptions.p2Life, enumStyle);
                        globalInfo.trainingModeOptions.p1Gauge = (LifeBarTrainingMode)EditorGUILayout.EnumPopup("Player 1 Gauge:", globalInfo.trainingModeOptions.p1Gauge, enumStyle);
                        globalInfo.trainingModeOptions.p2Gauge = (LifeBarTrainingMode)EditorGUILayout.EnumPopup("Player 2 Gauge:", globalInfo.trainingModeOptions.p2Gauge, enumStyle);
                        globalInfo.trainingModeOptions.refillTime = EditorGUILayout.FloatField("Refill Time (seconds)", globalInfo.trainingModeOptions.refillTime);
                        EditorGUIUtility.labelWidth = 150;

                        EditorGUI.indentLevel -= 1;
                        EditorGUILayout.Space();
                    } EditorGUILayout.EndVertical();
                }
            } EditorGUILayout.EndVertical();


            // Preload Options
            EditorGUILayout.BeginVertical(rootGroupStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    preloadOptions = EditorGUILayout.Foldout(preloadOptions, "Preload Options", foldStyle);
                    helpButton("global:preload");
                } EditorGUILayout.EndHorizontal();

                if (preloadOptions) {
                    EditorGUILayout.BeginVertical(subGroupStyle);
                    {
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel += 1;
                        EditorGUIUtility.labelWidth = 200;
                        globalInfo.preloadHitEffects = EditorGUILayout.Toggle("Hit Effects", globalInfo.preloadHitEffects);
                        globalInfo.preloadCharacter1 = EditorGUILayout.Toggle("Player 1 Character & Moves", globalInfo.preloadCharacter1);
                        globalInfo.preloadCharacter2 = EditorGUILayout.Toggle("Player 2 Character & Moves", globalInfo.preloadCharacter2);
                        globalInfo.preloadStage = EditorGUILayout.Toggle("Stage", globalInfo.preloadStage);
                        globalInfo.warmAllShaders = EditorGUILayout.Toggle("Warm All Shaders", globalInfo.warmAllShaders);

                        EditorGUIUtility.labelWidth = 150;
                        EditorGUI.indentLevel -= 1;
                        EditorGUILayout.Space();
                    } EditorGUILayout.EndVertical();
                }
            } EditorGUILayout.EndVertical();


			// Advanced Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					advancedOptions = EditorGUILayout.Foldout(advancedOptions, "Advanced Options", foldStyle);
					helpButton("global:advanced");
				}EditorGUILayout.EndHorizontal();

				if (advancedOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						EditorGUIUtility.labelWidth = 180;

						EditorGUILayout.Space();
						globalInfo.fps = EditorGUILayout.IntField("Frames Per Second:", globalInfo.fps);
						globalInfo.executionBufferType = (ExecutionBufferType)EditorGUILayout.EnumPopup("Execution Buffer Type:", globalInfo.executionBufferType, enumStyle);
						EditorGUI.BeginDisabledGroup(globalInfo.executionBufferType == ExecutionBufferType.NoBuffer);{
							globalInfo.executionBufferTime = EditorGUILayout.IntField("Execution Buffer (frames):", Mathf.Clamp(globalInfo.executionBufferTime, 1, int.MaxValue));
						}EditorGUI.EndDisabledGroup();
						globalInfo.plinkingDelay = EditorGUILayout.IntField("Plinking Delay (frames):", Mathf.Clamp(globalInfo.plinkingDelay, 1, int.MaxValue));
						globalInfo.gameSpeed = EditorGUILayout.Slider("Game Speed:", globalInfo.gameSpeed, .01f, 10);
						globalInfo.gravity = EditorGUILayout.FloatField("Global Gravity:", globalInfo.gravity);
                        globalInfo.detect3D_Hits = EditorGUILayout.Toggle("3D Hit Detection", globalInfo.detect3D_Hits);
                        globalInfo.runInBackground = EditorGUILayout.Toggle("Run in Background", globalInfo.runInBackground);
						
						EditorGUIUtility.labelWidth = 150;
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

			if (UFE.isNetworkAddonInstalled){
				EditorGUILayout.BeginVertical(rootGroupStyle);{
					EditorGUILayout.BeginHorizontal();{
						networkOptions = EditorGUILayout.Foldout(networkOptions, "Network Options", foldStyle);
						helpButton("global:network");
					}EditorGUILayout.EndHorizontal();

					if (networkOptions){
						EditorGUILayout.BeginVertical(subGroupStyle);{
							EditorGUILayout.Space();
							EditorGUI.indentLevel += 1;
							EditorGUIUtility.labelWidth = 200;

                            globalInfo.networkOptions.networkService = (NetworkService)EditorGUILayout.EnumPopup("Network Service:", globalInfo.networkOptions.networkService, enumStyle);
                            if (globalInfo.networkOptions.networkService == NetworkService.Server) {
                                GUILayout.BeginHorizontal("GroupBox");
                                GUILayout.Label("Coming Soon.", "CN EntryWarn");
                                GUILayout.EndHorizontal();
                            }
							globalInfo.networkOptions.fakeNetwork = EditorGUILayout.Toggle("Fake Network", globalInfo.networkOptions.fakeNetwork);
							globalInfo.networkOptions.forceAnimationControl = EditorGUILayout.Toggle("Force UFE Animation Control", globalInfo.networkOptions.forceAnimationControl);
                            globalInfo.networkOptions.applyFrameDelayOffline = EditorGUILayout.Toggle("Apply Frame Delay Offline", globalInfo.networkOptions.applyFrameDelayOffline);
                            globalInfo.networkOptions.minFrameDelay = Mathf.RoundToInt(EditorGUILayout.Slider("Min Frame Delay: ", globalInfo.networkOptions.minFrameDelay, 1f, 30f));
                            globalInfo.networkOptions.maxFrameDelay = Mathf.RoundToInt(EditorGUILayout.Slider("Max Frame Delay: ", globalInfo.networkOptions.maxFrameDelay, 1f, 30f));
							globalInfo.networkOptions.defaultFrameDelay = Mathf.RoundToInt(EditorGUILayout.Slider("Default Frame Delay: ", globalInfo.networkOptions.defaultFrameDelay, globalInfo.networkOptions.minFrameDelay, globalInfo.networkOptions.maxFrameDelay));
                            globalInfo.networkOptions.port = EditorGUILayout.IntField("Port:", globalInfo.networkOptions.port);

							EditorGUIUtility.labelWidth = 150;
							EditorGUI.indentLevel -= 1;
							EditorGUILayout.Space();
						}EditorGUILayout.EndVertical();
					}
				}EditorGUILayout.EndVertical();
			}

		}EditorGUILayout.EndScrollView();

		if (Application.dataPath.Contains("C:/LocalAssets/UFE/UFE")){
			pName = EditorGUILayout.TextField("Package Name:", pName);
			if (StyledButton("Export Package")) 
				AssetDatabase.ExportPackage("Assets", pName,
				                            ExportPackageOptions.Interactive | 
				                            ExportPackageOptions.Recurse | 
				                            ExportPackageOptions.IncludeLibraryAssets | 
				                            ExportPackageOptions.IncludeDependencies);
		}

		if (GUI.changed) {
			Undo.RecordObject(globalInfo, "Global Editor Modify");
			EditorUtility.SetDirty(globalInfo);
		}
	}

    public bool DisableScreenButton(UFEScreen screen) {
        if (screen == null || (screenPreview != null && screenPreview.name != screen.name)) {
            return true;
        }
        return false;
    }

    public void ScreenButton(UFEScreen screen) {
        if (screenPreview != null && screen != null && screenPreview.name == screen.name) {
            if (GUILayout.Button("Close", GUILayout.Width(45))) CloseGUICanvas();
        } else {
            if (GUILayout.Button("Open", GUILayout.Width(45))) OpenGUICanvas(screen);
        }
    }

	public void UpdateGUICanvas() {
		if (canvasPreview != null) {
			CanvasScaler cScaler = canvasPreview.GetComponent<CanvasScaler>();
			if (canvasPreview.GetComponent<Canvas>() == null) {
				cScaler = canvasPreview.AddComponent<CanvasScaler>();
			}
			cScaler.uiScaleMode = globalInfo.gameGUI.canvasScaler.scaleMode;
			cScaler.referencePixelsPerUnit = globalInfo.gameGUI.canvasScaler.referencePixelsPerUnit;
			cScaler.screenMatchMode	= globalInfo.gameGUI.canvasScaler.screenMatchMode;
			cScaler.matchWidthOrHeight = globalInfo.gameGUI.canvasScaler.matchWidthOrHeight;
			cScaler.referenceResolution = globalInfo.gameGUI.canvasScaler.referenceResolution;
		}
	}

    public void OpenGUICanvas(UFEScreen screen) {
        CloseGUICanvas();

        if (screen.canvasPreview) {
            canvasPreview = new GameObject("Canvas");
            canvasPreview.AddComponent<Canvas>();
            CanvasScaler cScaler = canvasPreview.AddComponent<CanvasScaler>();
            canvasPreview.AddComponent<GraphicRaycaster>();
            canvasPreview.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasPreview.layer = 5;

			cScaler.uiScaleMode = globalInfo.gameGUI.canvasScaler.scaleMode;
			cScaler.referencePixelsPerUnit = globalInfo.gameGUI.canvasScaler.referencePixelsPerUnit;
			cScaler.screenMatchMode	= globalInfo.gameGUI.canvasScaler.screenMatchMode;
			cScaler.matchWidthOrHeight = globalInfo.gameGUI.canvasScaler.matchWidthOrHeight;
			cScaler.referenceResolution = globalInfo.gameGUI.canvasScaler.referenceResolution;

            eventSystemPreview = new GameObject("EventSystem");
            eventSystemPreview.AddComponent<EventSystem>();
            eventSystemPreview.AddComponent<StandaloneInputModule>();
            eventSystemPreview.AddComponent<TouchInputModule>();

            screenPreview = (UFEScreen)PrefabUtility.InstantiatePrefab(screen);
            (screenPreview.transform as RectTransform).SetParent(canvasPreview.transform);

            (screenPreview.transform as RectTransform).anchorMin = Vector2.zero;
            (screenPreview.transform as RectTransform).anchorMax = Vector2.one;
            (screenPreview.transform as RectTransform).offsetMin = Vector2.zero;
            (screenPreview.transform as RectTransform).offsetMax = Vector2.zero;

            EditorWindow.FocusWindowIfItsOpen<SceneView>();
            Selection.activeObject = screenPreview;
        } else {
            Selection.activeObject = screen;
        }

        System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
        System.Type type = assembly.GetType("UnityEditor.InspectorWindow");
        EditorWindow inspectorView = EditorWindow.GetWindow(type);
        inspectorView.Focus();

    }

    public void CloseGUICanvas() {
        if (screenPreview != null) {
            Editor.DestroyImmediate(screenPreview);
            screenPreview = null;
        }
        if (canvasPreview != null) {
            Editor.DestroyImmediate(canvasPreview);
            canvasPreview = null;
        }
        if (eventSystemPreview != null) {
            Editor.DestroyImmediate(eventSystemPreview);
            eventSystemPreview = null;
        }
    }

	public bool StyledButton (string label) {
		EditorGUILayout.Space();
		GUILayoutUtility.GetRect(1, 20);
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		bool clickResult = GUILayout.Button(label, addButtonStyle);
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		return clickResult;
	}

    public void CharacterDebugOptions(CharacterDebugInfo debugInfo, string label) {
        debugInfo.toggle = EditorGUILayout.Foldout(debugInfo.toggle, label, foldStyle);
        if (debugInfo.toggle) {
            EditorGUILayout.BeginVertical(subGroupStyle);
            {
                EditorGUI.indentLevel += 1;
                debugInfo.currentMove = EditorGUILayout.Toggle("Move Info", debugInfo.currentMove);
                debugInfo.position = EditorGUILayout.Toggle("Position", debugInfo.position);
                debugInfo.lifePoints = EditorGUILayout.Toggle("Life Points", debugInfo.lifePoints);
                debugInfo.currentState = EditorGUILayout.Toggle("State", debugInfo.currentState);
                debugInfo.currentSubState = EditorGUILayout.Toggle("SubState", debugInfo.currentSubState);
                debugInfo.stunTime = EditorGUILayout.Toggle("Stun Time", debugInfo.stunTime);
                debugInfo.comboHits = EditorGUILayout.Toggle("Combo Hits", debugInfo.comboHits);
                debugInfo.comboDamage = EditorGUILayout.Toggle("Combo Damage", debugInfo.comboDamage);
                debugInfo.inputs = EditorGUILayout.Toggle("Input Held Time", debugInfo.inputs);
                debugInfo.buttonSequence = EditorGUILayout.Toggle("Move Execution (Console)", debugInfo.buttonSequence);
                EditorGUI.BeginDisabledGroup(!UFE.isAiAddonInstalled);
                {
                    debugInfo.aiWeightList = EditorGUILayout.Toggle("[Fuzzy A.I.] Weight List", debugInfo.aiWeightList);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel -= 1;
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();
        }
    }

	public void HitOptionBlock(string label, HitTypeOptions hit){
		HitOptionBlock(label, hit, false);
	}

	public void HitOptionBlock(string label, HitTypeOptions hit, bool disableFreezingTime){
		hit.editorToggle = EditorGUILayout.Foldout(hit.editorToggle, label, foldStyle);
		if (hit.editorToggle){
			EditorGUILayout.BeginVertical(subGroupStyle);{
				EditorGUILayout.Space();
				EditorGUI.indentLevel += 1;

				hit.hitParticle = (GameObject) EditorGUILayout.ObjectField("Particle Effect:", hit.hitParticle, typeof(UnityEngine.GameObject), true);
				hit.killTime = EditorGUILayout.FloatField("Effect Duration:", hit.killTime);
				hit.hitSound = (AudioClip) EditorGUILayout.ObjectField("Sound Effect:", hit.hitSound, typeof(UnityEngine.AudioClip), true);
				
				if (disableFreezingTime){
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.TextField("Freezing Time:", "(Automatic)");
					EditorGUILayout.TextField("Animation Speed", "(Automatic)");
					EditorGUI.EndDisabledGroup();
				}else{
					hit.freezingTime = EditorGUILayout.FloatField("Freezing Time:", hit.freezingTime);
					hit.animationSpeed = EditorGUILayout.FloatField("Animation Speed (%):", hit.animationSpeed);
				}

				hit.shakeCharacterOnHit = EditorGUILayout.Toggle("Shake Character On Hit", hit.shakeCharacterOnHit);
				hit.shakeCameraOnHit = EditorGUILayout.Toggle("Shake Camera On Hit", hit.shakeCameraOnHit);
				if (hit.shakeCharacterOnHit || hit.shakeCameraOnHit)
					hit.shakeDensity = EditorGUILayout.FloatField("Shake Density:", hit.shakeDensity);

				EditorGUI.indentLevel -= 1;
				EditorGUILayout.Space();
				
			}EditorGUILayout.EndVertical();
		}
	}
	
	public void KnockdownOptionsBlock(string label, SubKnockdownOptions option){
		option.editorToggle = EditorGUILayout.Foldout(option.editorToggle, label, foldStyle);
		if (option.editorToggle){
			EditorGUILayout.BeginVertical(subGroupStyle);{
				EditorGUILayout.Space();
				EditorGUI.indentLevel += 1;


				if (label == "Crumple Knockdown"){
					option.standUpTime = EditorGUILayout.FloatField("Stand Up Time:", option.standUpTime);
					option.standUpHitBoxes = EditorGUILayout.Toggle("Stand Up Hit Boxes", option.standUpHitBoxes);
				}else{
					option.knockedOutTime = EditorGUILayout.FloatField("Knockout Time:", option.knockedOutTime);
					option.standUpTime = EditorGUILayout.FloatField("Stand Up Time:", option.standUpTime);
					option.standUpHitBoxes = EditorGUILayout.Toggle("Knockdown Hit Boxes", option.standUpHitBoxes);

					option.hasQuickStand = EditorGUILayout.Toggle("Allow Quick Stand", option.hasQuickStand);
					if (option.hasQuickStand){
						EditorGUILayout.BeginVertical(subGroupStyle);{
							EditorGUI.indentLevel += 1;
							for (int i = 0; i < option.quickStandButtons.Length; i ++){
								EditorGUILayout.Space();
								EditorGUILayout.BeginVertical(arrayElementStyle);{
									EditorGUILayout.Space();
									EditorGUILayout.BeginHorizontal();{
										option.quickStandButtons[i] = (ButtonPress)EditorGUILayout.EnumPopup("Button:", option.quickStandButtons[i], enumStyle);
										if (GUILayout.Button("", "PaneOptions")){
											PaneOptions<ButtonPress>(option.quickStandButtons, option.quickStandButtons[i], delegate (ButtonPress[] newElement) { option.quickStandButtons = newElement; });
										}
									}EditorGUILayout.EndHorizontal();
									EditorGUILayout.Space();
								}EditorGUILayout.EndVertical();
							}
							EditorGUILayout.Space();
							if (StyledButton("New Button"))
								option.quickStandButtons = AddElement<ButtonPress>(option.quickStandButtons, ButtonPress.Button1);
							
							EditorGUI.indentLevel -= 1;
						}EditorGUILayout.EndVertical();
					}
					
					option.hasDelayedStand = EditorGUILayout.Toggle("Allow Delayed Stand", option.hasDelayedStand);
					if (option.hasDelayedStand){
						EditorGUILayout.BeginVertical(subGroupStyle);{
							EditorGUI.indentLevel += 1;
							for (int i = 0; i < option.delayedStandButtons.Length; i ++){
								EditorGUILayout.Space();
								EditorGUILayout.BeginVertical(arrayElementStyle);{
									EditorGUILayout.Space();
									EditorGUILayout.BeginHorizontal();{
										option.delayedStandButtons[i] = (ButtonPress)EditorGUILayout.EnumPopup("Button:", option.delayedStandButtons[i], enumStyle);
										if (GUILayout.Button("", "PaneOptions")){
											PaneOptions<ButtonPress>(option.delayedStandButtons, option.delayedStandButtons[i], delegate (ButtonPress[] newElement) { option.delayedStandButtons = newElement; });
										}
									}EditorGUILayout.EndHorizontal();
									EditorGUILayout.Space();
								}EditorGUILayout.EndVertical();
							}
							EditorGUILayout.Space();
							if (StyledButton("New Button"))
								option.delayedStandButtons = AddElement<ButtonPress>(option.delayedStandButtons, ButtonPress.Down);
							
							EditorGUI.indentLevel -= 1;
						}EditorGUILayout.EndVertical();
					}
					
					if (label != "Default Knockdown") option.predefinedPushForce = EditorGUILayout.Vector2Field("Predefined Push Force:", option.predefinedPushForce);
				}
				EditorGUI.indentLevel -= 1;
				EditorGUILayout.Space();
				
			}EditorGUILayout.EndVertical();
		}
	}

	public void EditCharacterStory(CharacterStory characterStory, string storyName = "Story"){
		EditorGUILayout.BeginVertical(subGroupStyle);{
			EditorGUILayout.Space();
			characterStory.showStoryInEditor = EditorGUILayout.Foldout(characterStory.showStoryInEditor, storyName, foldStyle);

			if (characterStory.showStoryInEditor){
				EditorGUI.indentLevel += 1;
				EditorGUILayout.BeginHorizontal();{
					characterStory.opening = (StoryModeScreen) EditorGUILayout.ObjectField("Opening Scene:", characterStory.opening, typeof(StoryModeScreen), true);
					EditorGUI.BeginDisabledGroup(DisableScreenButton(characterStory.opening) || !guiScreensOptions);{
						ScreenButton(characterStory.opening);
					} EditorGUI.EndDisabledGroup();
				} EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();{
					characterStory.ending = (StoryModeScreen) EditorGUILayout.ObjectField("Ending Scene:", characterStory.ending, typeof(StoryModeScreen), true);
					EditorGUI.BeginDisabledGroup(DisableScreenButton(characterStory.ending) || !guiScreensOptions);{
						ScreenButton(characterStory.ending);
					} EditorGUI.EndDisabledGroup();
				} EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space();

                SubGroupTitle("Fight Groups");

				this.FightGroupsBlock(characterStory.fightsGroups, delegate(FightsGroup[] obj) {
					int previousLength = characterStory.fightsGroups.Length;
					int currentLength = obj.Length;


					characterStory.fightsGroups = obj;


					if (previousLength == 0 && currentLength == 1){
						characterStory.fightsGroups[0].name = "Random Fights";
						characterStory.fightsGroups[0].mode = FightsGroupMode.FightAgainstAllOpponentsInTheGroupInRandomOrder;
					}else if (previousLength == 1 && currentLength == 2){
						characterStory.fightsGroups[1].name = "Boss Fights";
						characterStory.fightsGroups[1].mode = FightsGroupMode.FightAgainstAllOpponentsInTheGroupInTheDefinedOrder;
					}else if (currentLength == previousLength + 1){
						characterStory.fightsGroups[previousLength].name = "Group " + currentLength;
						characterStory.fightsGroups[previousLength].mode = FightsGroupMode.FightAgainstAllOpponentsInTheGroupInTheDefinedOrder;
					}
				});

				EditorGUI.indentLevel -= 1;
			}
			EditorGUILayout.Space();
		}EditorGUILayout.EndVertical();
		EditorGUILayout.Space();
	}

	public void FightGroupsBlock(FightsGroup[] fights, Action<FightsGroup[]> callback){
		List<string> characterNames = new List<string>();
		for (int i = 0; i < globalInfo.characters.Length; ++i){
			if (globalInfo.characters[i] != null){
				characterNames.Add(globalInfo.characters[i].name);
			}else{
				characterNames.Add ("Character " + i);
			}
		}

		EditorGUILayout.BeginVertical();{
			//EditorGUI.indentLevel += 1;
			for (int i = 0; i < fights.Length; ++i){
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical(arrayElementStyle);{
					EditorGUILayout.Space();

					EditorGUILayout.BeginHorizontal();{
						fights[i].name = EditorGUILayout.TextField("Group Name:", fights[i].name);

						if (GUILayout.Button("", "PaneOptions")){
							PaneOptions<FightsGroup>(fights, fights[i], delegate (FightsGroup[] newElement) { if (callback != null)callback(newElement); });
							return;
						}
					}EditorGUILayout.EndHorizontal();


					fights[i].mode = (FightsGroupMode) EditorGUILayout.EnumPopup("Fight Mode:", fights[i].mode);

					if (fights[i].mode == FightsGroupMode.FightAgainstSeveralOpponentsInTheGroupInRandomOrder){
						fights[i].maxFights = EditorGUILayout.IntSlider("How many opponents?", fights[i].maxFights, 1, Mathf.Max(fights[i].opponents.Length, 1));
					}

					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						fights[i].showOpponentsInEditor = EditorGUILayout.Foldout(fights[i].showOpponentsInEditor, "Opponents", foldStyle);

						if (fights[i].showOpponentsInEditor){
							this.StoryModeBattleBlock(fights, i, characterNames.ToArray(), delegate(FightsGroup[] obj) {
								if (callback != null) callback(obj);
							});
						}
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
					EditorGUILayout.Space();
				}EditorGUILayout.EndVertical();
			}
			//EditorGUI.indentLevel -= 1;
			
			if (StyledButton("New Group")){
				fights = AddElement<FightsGroup>(fights, new FightsGroup());
			}
		}EditorGUILayout.EndVertical();

		if (callback != null) callback(fights);
	}

	public void StoryModeBattleBlock(FightsGroup[] fights, int fightIndex, string[] characterNames, Action<FightsGroup[]> callback){
		this.StoryModeBattleBlock(fights[fightIndex].opponents, characterNames, delegate(StoryModeBattle[] obj) {
			int previousLength = fights[fightIndex].opponents.Length;
			int currentLength = obj.Length;
			
			
			fights[fightIndex].opponents = obj;
			
			
			if (currentLength == previousLength + 1){
				fights[fightIndex].opponents[previousLength].opponentCharacterIndex = (previousLength % globalInfo.characters.Length);
				fights[fightIndex].opponents[previousLength].possibleStagesIndexes.Add(previousLength % globalInfo.stages.Length);
			}
			

			if (callback != null) callback(fights);
		});
	}
	
	public void StoryModeBattleBlock(StoryModeBattle[] battles, string[] characterNames, Action<StoryModeBattle[]> callback){
		EditorGUILayout.BeginVertical();{
			EditorGUI.indentLevel += 1;
			for (int i = 0; i < battles.Length; ++i){
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical(subArrayElementStyle);{
					EditorGUILayout.Space();
					EditorGUILayout.BeginHorizontal();{
						battles[i].opponentCharacterIndex = EditorGUILayout.Popup("Opponent:", battles[i].opponentCharacterIndex, characterNames);

						if (GUILayout.Button("", "PaneOptions")){
							PaneOptions<StoryModeBattle>(battles, battles[i], delegate (StoryModeBattle[] newElement) { if (callback != null)callback(newElement); });
							return;
						}
					}EditorGUILayout.EndHorizontal();

					string[] stageNames = new string[globalInfo.stages.Length];
					int stageFlags = 0;
					
					for (int j = 0; j < globalInfo.stages.Length; ++j){
						if (globalInfo.stages[j] != null && !string.IsNullOrEmpty(globalInfo.stages[j].stageName)){
							stageNames[j] = globalInfo.stages[j].stageName;
						}else{
							stageNames[j] = "Stage " + j;
						}

						if (battles[i].possibleStagesIndexes.Contains(j)){
							stageFlags |= 1<<j;
						}
					}
					
					stageFlags = EditorGUILayout.MaskField("Possible Stages:", stageFlags, stageNames);
					battles[i].possibleStagesIndexes.Clear();
					
					for (int j = 0; j < globalInfo.stages.Length; ++j){
						
						if ((stageFlags & (1<<j)) != 0){
							battles[i].possibleStagesIndexes.Add(j);
						}
					}

					EditorGUILayout.BeginHorizontal();{
						battles[i].conversationBeforeBattle = (StoryModeScreen) EditorGUILayout.ObjectField("Before the battle:", battles[i].conversationBeforeBattle, typeof(StoryModeScreen), true);
						EditorGUI.BeginDisabledGroup(DisableScreenButton(battles[i].conversationBeforeBattle) || !guiScreensOptions);{
							ScreenButton(battles[i].conversationBeforeBattle);
						} EditorGUI.EndDisabledGroup();
					} EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();{
						battles[i].conversationAfterBattle = (StoryModeScreen) EditorGUILayout.ObjectField("After the battle:", battles[i].conversationAfterBattle, typeof(StoryModeScreen), true);
						EditorGUI.BeginDisabledGroup(DisableScreenButton(battles[i].conversationAfterBattle) || !guiScreensOptions);{
							ScreenButton(battles[i].conversationAfterBattle);
						} EditorGUI.EndDisabledGroup();
					} EditorGUILayout.EndHorizontal();

//					battles[i].conversationBeforeBattle = (StoryModeScreen) EditorGUILayout.ObjectField("Before the battle:", battles[i].conversationBeforeBattle, typeof(StoryModeScreen), true);
//					battles[i].conversationAfterBattle = (StoryModeScreen) EditorGUILayout.ObjectField("After the battle:", battles[i].conversationAfterBattle, typeof(StoryModeScreen), true);

					EditorGUILayout.Space();
				}EditorGUILayout.EndVertical();
			}
			EditorGUI.indentLevel -= 1;
			
			if (StyledButton("New Opponent")){
				battles = AddElement<StoryModeBattle>(battles, new StoryModeBattle());
			}
		}EditorGUILayout.EndVertical();
		
		if (callback != null)callback(battles);
	}

	public InputReferences[] PlayerInputsBlock(InputReferences[] inputReferences){
		bool controlFreakInstalled = UFE.isControlFreakInstalled;
		bool cInputInstalled = UFE.isCInputInstalled;
		
		EditorGUIUtility.labelWidth = 180;
		EditorGUILayout.BeginVertical(subGroupStyle);{
			for (int i = 0; i < inputReferences.Length; i ++){
				EditorGUILayout.Space();
				EditorGUILayout.BeginVertical(arrayElementStyle);{
					EditorGUILayout.Space();
					EditorGUILayout.BeginHorizontal();{
						inputReferences[i].inputType = (InputType) EditorGUILayout.EnumPopup("Input Type:", inputReferences[i].inputType, enumStyle);
						if (GUILayout.Button("", "PaneOptions")){
							if (inputReferences.Equals(globalInfo.player1_Inputs)){
								PaneOptions<InputReferences>(globalInfo.player1_Inputs, globalInfo.player1_Inputs[i], delegate (InputReferences[] newElement) { globalInfo.player1_Inputs = newElement; });
							}else{
								PaneOptions<InputReferences>(globalInfo.player2_Inputs, globalInfo.player2_Inputs[i], delegate (InputReferences[] newElement) { globalInfo.player2_Inputs = newElement; });
							}
							//PaneOptions<InputReferences>(inputReferences, inputReferences[i], delegate (InputReferences[] newElement) { inputReferences = newElement; });
						}
					}EditorGUILayout.EndHorizontal();

					if (inputReferences[i].inputType == InputType.Button){
						inputReferences[i].engineRelatedButton = (ButtonPress) EditorGUILayout.EnumPopup("UFE Button Reference:", inputReferences[i].engineRelatedButton, enumStyle);
					}

					if (cInputInstalled && globalInfo.inputOptions.inputManagerType == InputManagerType.cInput){
						if (inputReferences[i].inputType == InputType.Button){
							inputReferences[i].inputButtonName = EditorGUILayout.TextField("cInput Button Name:", inputReferences[i].inputButtonName);
							inputReferences[i].cInputPositiveDefaultKey = EditorGUILayout.TextField("cInput Default Key:", inputReferences[i].cInputPositiveDefaultKey);
							inputReferences[i].cInputPositiveAlternativeKey = EditorGUILayout.TextField("cInput Alternative Key:", inputReferences[i].cInputPositiveAlternativeKey);
						}else{
							inputReferences[i].inputButtonName = EditorGUILayout.TextField("cInput Axis Name:", inputReferences[i].inputButtonName);
							EditorGUILayout.Space();
							EditorGUILayout.LabelField("cInput:");

							inputReferences[i].cInputPositiveKeyName = EditorGUILayout.TextField("Positive Value -> Button:", inputReferences[i].cInputPositiveKeyName);
							inputReferences[i].cInputPositiveDefaultKey = EditorGUILayout.TextField("Positive Value -> Default Key:", inputReferences[i].cInputPositiveDefaultKey);
							inputReferences[i].cInputPositiveAlternativeKey = EditorGUILayout.TextField("Positive Value -> Alt Key:", inputReferences[i].cInputPositiveAlternativeKey);
							EditorGUILayout.Space();
							inputReferences[i].cInputNegativeKeyName = EditorGUILayout.TextField("Negative Value -> Button:", inputReferences[i].cInputNegativeKeyName);
							inputReferences[i].cInputNegativeDefaultKey = EditorGUILayout.TextField("Negative Value -> Default Key:", inputReferences[i].cInputNegativeDefaultKey);
							inputReferences[i].cInputNegativeAlternativeKey = EditorGUILayout.TextField("Negative Value -> Alt Key:", inputReferences[i].cInputNegativeAlternativeKey);
						}
#if !UFE_BASIC
					}else if (controlFreakInstalled && globalInfo.inputOptions.inputManagerType == InputManagerType.ControlFreak){
						if (inputReferences[i].inputType == InputType.Button){
							inputReferences[i].inputButtonName = EditorGUILayout.TextField("CF Button Name:", inputReferences[i].inputButtonName);
						}else{
							inputReferences[i].inputButtonName = EditorGUILayout.TextField("CF Axis Name:", inputReferences[i].inputButtonName);
						}
#endif
					}else{
						if (inputReferences[i].inputType == InputType.Button){
							inputReferences[i].inputButtonName = EditorGUILayout.TextField("Input Manager Reference:", inputReferences[i].inputButtonName);
						}else{
							inputReferences[i].inputButtonName = EditorGUILayout.TextField("Keyboard Axis Reference:", inputReferences[i].inputButtonName);
							inputReferences[i].joystickAxisName = EditorGUILayout.TextField("Joystick Axis Reference:", inputReferences[i].joystickAxisName);
						}
					}
					
					if (inputReferences[i].engineRelatedButton != ButtonPress.Start){
						string label1 = null;
						string label2 = null;
						if (inputReferences[i].inputType == InputType.Button){
							label1 = "Button Icon:";
						}else if (inputReferences[i].inputType == InputType.HorizontalAxis){
							label1 = "Axis Right Icon:";
							label2 = "Axis Left Icon:";
						}else if (inputReferences[i].inputType == InputType.VerticalAxis){
							label1 = "Axis Up Icon:";
							label2 = "Axis Down Icon:";
						}

						EditorGUILayout.BeginHorizontal();{
							EditorGUILayout.LabelField(label1, GUILayout.Width(160));
							inputReferences[i].inputViewerIcon1 = (Texture2D)EditorGUILayout.ObjectField(inputReferences[i].inputViewerIcon1, typeof(Texture2D), true);
							inputReferences[i].activeIcon = inputReferences[i].inputViewerIcon1;
						}EditorGUILayout.EndHorizontal();

						if (label2 != null){
							EditorGUILayout.BeginHorizontal();{
								EditorGUILayout.LabelField(label2, GUILayout.Width(160));
								inputReferences[i].inputViewerIcon2 = (Texture2D)EditorGUILayout.ObjectField(inputReferences[i].inputViewerIcon2, typeof(Texture2D), true);
							}EditorGUILayout.EndHorizontal();
						}
					}

					EditorGUILayout.Space();
				}EditorGUILayout.EndVertical();
			}
			
			if (StyledButton("New Input"))
				inputReferences = AddElement<InputReferences>(inputReferences, new InputReferences());

		}EditorGUILayout.EndVertical();
		EditorGUILayout.Space();
		EditorGUIUtility.labelWidth = 150;

		return inputReferences;
	}


	private Vector2 ReturnRange(CharacterDistance distance, int size){
		if (distance == CharacterDistance.Any) return new Vector2(0, 100);
		if (size == 2){
			if ((int)distance < (int)CharacterDistance.Mid) return new Vector2(0, 50);
			if ((int)distance >= (int)CharacterDistance.Mid) return new Vector2(51, 100);
		}else if (size == 3){
			if ((int)distance < (int)CharacterDistance.Mid) return new Vector2(0, 30);
			if (distance == CharacterDistance.Mid) return new Vector2(31, 70);
			if ((int)distance > (int)CharacterDistance.Mid) return new Vector2(71, 100);
		}else if (size == 4){
			if ((int)distance == (int)CharacterDistance.VeryClose) return new Vector2(0, 25);
			if ((int)distance <= (int)CharacterDistance.Mid) return new Vector2(26, 50);
			if ((int)distance == (int)CharacterDistance.Far) return new Vector2(51, 75);
			if ((int)distance == (int)CharacterDistance.VeryFar) return new Vector2(76, 100);
		}else if (size > 4){
			if ((int)distance == (int)CharacterDistance.VeryClose) return new Vector2(0, 20);
			if ((int)distance == (int)CharacterDistance.Close) return new Vector2(21, 40);
			if ((int)distance == (int)CharacterDistance.Mid) return new Vector2(41, 60);
			if ((int)distance == (int)CharacterDistance.VeryFar) return new Vector2(61, 80);
			if ((int)distance == (int)CharacterDistance.VeryFar) return new Vector2(81, 100);
		}
		
		
		return Vector2.zero;
	}

	private string DistanceToString(int index, int size){
		if (size == 2){
			if (index == 0) return "Close";
			if (index == 1) return "Far";
		}else if (size == 3){
			if (index == 0) return "Close";
			if (index == 1) return "Mid";
			if (index == 2) return "Far";
		}else if (size == 4){
			if (index == 0) return "Very Close";
			if (index == 1) return "Close";
			if (index == 2) return "Mid";
			if (index == 3) return "Far";
		}else if (size > 4){
			float position = (float)(index + 1)/(float)size;
			if (position <= .2f) return "Very Close";
			if (position <= .4f) return "Close";
			if (position <= .6f) return "Mid";
			if (position <= .8f) return "Far";
			if (position > .8f) return "Very Far";
		}


		return "Any";
	}

	private void DisableableSlider(string description, ref bool toggle, ref float slider, float minValue, float maxValue){
		EditorGUILayout.BeginHorizontal();{
			toggle = EditorGUILayout.Toggle(toggle, GUILayout.Width(40));
			EditorGUI.BeginDisabledGroup(!toggle);{
				slider = EditorGUILayout.Slider(description, slider, minValue, maxValue);
			}EditorGUI.EndDisabledGroup();
		}EditorGUILayout.EndHorizontal();
	}

	private void SubGroupTitle(string _name){
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(_name);
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
	}

	public void CInputPreferences(){
		EditorGUILayout.BeginVertical(subGroupStyle);{
			EditorGUILayout.BeginVertical(arrayElementStyle);{
				EditorGUI.indentLevel += 1;
				EditorGUILayout.Space();
				globalInfo.inputOptions.cInputAllowDuplicates = EditorGUILayout.Toggle("Allow Duplicates:", globalInfo.inputOptions.cInputAllowDuplicates);
				globalInfo.inputOptions.cInputGravity = EditorGUILayout.FloatField("Gravity:", globalInfo.inputOptions.cInputGravity);
				globalInfo.inputOptions.cInputSensitivity = EditorGUILayout.FloatField("Sensibility:", globalInfo.inputOptions.cInputSensitivity);
				globalInfo.inputOptions.cInputDeadZone = EditorGUILayout.FloatField("Dead Zone:", globalInfo.inputOptions.cInputDeadZone);
				globalInfo.inputOptions.cInputSkin = EditorGUILayout.ObjectField("Skin:", globalInfo.inputOptions.cInputSkin, typeof(GUISkin), false) as GUISkin;
				EditorGUILayout.Space();
				EditorGUI.indentLevel -= 1;
			}EditorGUILayout.EndVertical();
		}EditorGUILayout.EndVertical();
	}

	public void ControlFreakPreferences(){
		EditorGUILayout.BeginVertical(subGroupStyle);{
			EditorGUILayout.BeginVertical(arrayElementStyle);{
				EditorGUI.indentLevel += 1;
				EditorGUILayout.Space();
				globalInfo.inputOptions.controlFreakPrefab = EditorGUILayout.ObjectField("Prefab:", globalInfo.inputOptions.controlFreakPrefab, typeof(GameObject), false) as GameObject;
				globalInfo.inputOptions.controlFreakDeadZone = EditorGUILayout.FloatField("Dead Zone:", globalInfo.inputOptions.controlFreakDeadZone);
				EditorGUILayout.Space();
				EditorGUI.indentLevel -= 1;
			}EditorGUILayout.EndVertical();
		}EditorGUILayout.EndVertical();
	}
	
	public void GUIBarBlock(string label, GUIBarOptions guiBar){
		guiBar.editorToggle = EditorGUILayout.Foldout(guiBar.editorToggle, label, foldStyle);
		if (guiBar.editorToggle){
			EditorGUILayout.BeginVertical(subGroupStyle);{
				EditorGUILayout.Space();
				EditorGUI.indentLevel += 1;

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Background Image:");
				guiBar.backgroundImage = (Texture2D) EditorGUILayout.ObjectField(guiBar.backgroundImage, typeof(Texture2D), false, GUILayout.Width(320), GUILayout.Height(40));
				//guiBar.backgroundColor = EditorGUILayout.ColorField("Color:", guiBar.backgroundColor);
				guiBar.backgroundRect = EditorGUILayout.RectField("Pixel Inset:", guiBar.backgroundRect);
				if (guiBar.backgroundRect.width == 0 && guiBar.backgroundImage != null) guiBar.backgroundRect.width = guiBar.backgroundImage.width;
				if (guiBar.backgroundRect.height == 0 && guiBar.backgroundImage != null) guiBar.backgroundRect.height = guiBar.backgroundImage.height;
				//guiBar.backgroundRect.x = 0;

				EditorGUILayout.Space();
				
				EditorGUILayout.LabelField("Fill Image:");
				guiBar.fillImage = (Texture2D) EditorGUILayout.ObjectField(guiBar.fillImage, typeof(Texture2D), false, GUILayout.Width(320), GUILayout.Height(36));
				//guiBar.fillColor = EditorGUILayout.ColorField("Color:", guiBar.fillColor);
				guiBar.fillRect = EditorGUILayout.RectField("Pixel Inset (relative):", guiBar.fillRect);
				if (guiBar.fillRect.width == 0 && guiBar.fillImage != null) guiBar.fillRect.width = guiBar.fillImage.width;
				if (guiBar.fillRect.height == 0 && guiBar.fillImage != null) guiBar.fillRect.height = guiBar.fillImage.height;
				
				EditorGUILayout.Space();

				if (guiBar.previewToggle){
					if (guiBar.bgPreview == null){
						guiBar.bgPreview = new GameObject(label + " Background");
						guiBar.bgPreview.transform.position = Vector3.zero;
						guiBar.bgPreview.transform.localScale = Vector3.zero;
						guiBar.bgPreview.AddComponent<GUITexture>();


						guiBar.fillPreview = new GameObject(label + " Fill");
						guiBar.fillPreview.transform.position = Vector3.forward;
						guiBar.fillPreview.transform.localScale = Vector3.zero;
						guiBar.fillPreview.AddComponent<GUITexture>();
					}

					guiBar.bgPreview.GetComponent<GUITexture>().pixelInset = new Rect(0, 0, guiBar.backgroundRect.width, guiBar.backgroundRect.height);
					guiBar.bgPreview.GetComponent<GUITexture>().texture = guiBar.backgroundImage;
					guiBar.fillPreview.GetComponent<GUITexture>().pixelInset = guiBar.fillRect;
					guiBar.fillPreview.GetComponent<GUITexture>().texture = guiBar.fillImage;
					if (StyledButton("Close Preview")) {
						Clear();
						guiBar.previewToggle = false;
					}
				}else{
					if (StyledButton("Preview")) guiBar.previewToggle = true;
				}

				EditorGUI.indentLevel -= 1;
				EditorGUILayout.Space();
				
			}EditorGUILayout.EndVertical();
		}
	}

	public void StyledMinMaxSlider (string label, ref int minValue, ref int maxValue, int minLimit, int maxLimit, int indentLevel) {
		int indentSpacing = 25 * indentLevel;
		//indentSpacing += 30;
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		
		if (minValue < 0) minValue = 0;
		if (maxValue < 2) maxValue = 2;
		if (maxValue > maxLimit) maxValue = maxLimit;
		if (minValue == maxValue) minValue --;
		float minValueFloat = (float) minValue;
		float maxValueFloat = (float) maxValue;
		float minLimitFloat = (float) minLimit;
		float maxLimitFloat = (float) maxLimit;
		
		Rect tempRect = GUILayoutUtility.GetRect(1, 10);
		
		Rect rect = new Rect(indentSpacing,tempRect.y, Screen.width - indentSpacing - 100, 20);
		//Rect rect = new Rect(indentSpacing + 15,tempRect.y, Screen.width - indentSpacing - 70, 20);
		float fillLeftPos = ((rect.width/maxLimitFloat) * minValueFloat) + rect.x;
		float fillRightPos = ((rect.width/maxLimitFloat) * maxValueFloat) + rect.x;
		float fillWidth = fillRightPos - fillLeftPos;
		
		fillWidth += (rect.width/maxLimitFloat);
		fillLeftPos -= (rect.width/maxLimitFloat);
		
		// Border
		GUI.Box(rect, "", borderBarStyle);
		
		// Overlay
		GUI.Box(new Rect(fillLeftPos, rect.y, fillWidth, rect.height), new GUIContent(), fillBarStyle1);
		
		// Text
		//GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
		//centeredStyle.alignment = TextAnchor.UpperCenter;
		labelStyle.alignment = TextAnchor.UpperCenter;
		GUI.Label(rect, label + " between "+ Mathf.Floor(minValueFloat)+" and "+Mathf.Floor(maxValueFloat), labelStyle);
		labelStyle.alignment = TextAnchor.MiddleCenter;
		
		// Slider
		rect.y += 10;
		rect.x = indentLevel * 10;
		rect.width = (Screen.width - (indentLevel * 10) - 100);
		
		EditorGUI.MinMaxSlider(rect, ref minValueFloat, ref maxValueFloat, minLimitFloat, maxLimitFloat);
		minValue = (int) minValueFloat;
		maxValue = (int) maxValueFloat;
		
		tempRect = GUILayoutUtility.GetRect(1, 20);
	}

	public void PaneOptions<T> (T[] elements, T element, System.Action<T[]> callback){
		if (elements == null || elements.Length == 0) return;
		GenericMenu toolsMenu = new GenericMenu();
		
		if ((elements[0] != null && elements[0].Equals(element)) || (elements[0] == null && element == null) || elements.Length == 1){
			toolsMenu.AddDisabledItem(new GUIContent("Move Up"));
			toolsMenu.AddDisabledItem(new GUIContent("Move To Top"));
		}else {
			toolsMenu.AddItem(new GUIContent("Move Up"), false, delegate() {callback(MoveElement<T>(elements, element, -1));});
			toolsMenu.AddItem(new GUIContent("Move To Top"), false, delegate() {callback(MoveElement<T>(elements, element, -elements.Length));});
		}
		if ((elements[elements.Length - 1] != null && elements[elements.Length - 1].Equals(element)) || elements.Length == 1){
			toolsMenu.AddDisabledItem(new GUIContent("Move Down"));
			toolsMenu.AddDisabledItem(new GUIContent("Move To Bottom"));
		}else{
			toolsMenu.AddItem(new GUIContent("Move Down"), false, delegate() {callback(MoveElement<T>(elements, element, 1));});
			toolsMenu.AddItem(new GUIContent("Move To Bottom"), false, delegate() {callback(MoveElement<T>(elements, element, elements.Length));});
		}
		
		toolsMenu.AddSeparator("");
		
		if (element != null && element is System.ICloneable){
			toolsMenu.AddItem(new GUIContent("Copy"), false, delegate() {callback(CopyElement<T>(elements, element));});
		}else{
			toolsMenu.AddDisabledItem(new GUIContent("Copy"));
		}
		
		if (element != null && CloneObject.objCopy != null && CloneObject.objCopy.GetType() == typeof(T)){
			toolsMenu.AddItem(new GUIContent("Paste"), false, delegate() {callback(PasteElement<T>(elements, element));});
		}else{
			toolsMenu.AddDisabledItem(new GUIContent("Paste"));
		}
		
		toolsMenu.AddSeparator("");
		
		if (!(element is System.ICloneable)){
			toolsMenu.AddDisabledItem(new GUIContent("Duplicate"));
		}else{
			toolsMenu.AddItem(new GUIContent("Duplicate"), false, delegate() {callback(DuplicateElement<T>(elements, element));});
		}
		toolsMenu.AddItem(new GUIContent("Remove"), false, delegate() {callback(RemoveElement<T>(elements, element));});
		
		toolsMenu.ShowAsContext();
		EditorGUIUtility.ExitGUI();
	}
	
	public T[] RemoveElement<T> (T[] elements, T element) {
		List<T> elementsList = new List<T>(elements);
		elementsList.Remove(element);
		return elementsList.ToArray();
	}
	
	public T[] AddElement<T> (T[] elements, T element) {
		List<T> elementsList = new List<T>(elements);
		elementsList.Add(element);
		return elementsList.ToArray();
	}
	
	public T[] CopyElement<T> (T[] elements, T element) {
		CloneObject.objCopy = (object)(element as ICloneable).Clone();
		return elements;
	}
	
	public T[] PasteElement<T> (T[] elements, T element) {
		if (CloneObject.objCopy == null) return elements;
		List<T> elementsList = new List<T>(elements);
		elementsList.Insert(elementsList.IndexOf(element) + 1, (T)CloneObject.objCopy);
		CloneObject.objCopy = null;
		return elementsList.ToArray();
	}
	
	public T[] DuplicateElement<T> (T[] elements, T element) {
		List<T> elementsList = new List<T>(elements);
		elementsList.Insert(elementsList.IndexOf(element) + 1, (T)(element as ICloneable).Clone());
		return elementsList.ToArray();
	}
	
	public T[] MoveElement<T> (T[] elements, T element, int steps) {
		List<T> elementsList = new List<T>(elements);
		int newIndex = Mathf.Clamp(elementsList.IndexOf(element) + steps, 0, elements.Length - 1);
		elementsList.Remove(element);
		elementsList.Insert(newIndex, element);
		return elementsList.ToArray();
	}
}