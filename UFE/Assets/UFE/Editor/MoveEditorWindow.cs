using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

public class MoveEditorWindow : EditorWindow {
	public static MoveEditorWindow moveEditorWindow;
	public static MoveInfo sentMoveInfo;
	
	private MoveInfo moveInfo;
	private int fpsTemp;
	private Vector2 scrollPos;
	
	private GameObject characterPrefab;
	private GameObject opCharacterPrefab;
	private GameObject projectilePrefab;
	private float animationSpeedTemp;
	private int totalFramesTemp;
	private int totalFramesGlobal;
	private float animFrame;
	private float animTime;
	private bool animIsPlaying;
	private bool smoothPreview;
	private GlobalInfo globalInfo = null;

	private float camTime;
	private float prevCamTime;
	private float camStart;
	public Vector3 initialCamPosition;
	public Quaternion initialCamRotation;
	public float initialFieldOfView;

	
	private bool possibleStancesToggle;
	private bool buttonSequenceToggle;
	private bool buttonExecutionToggle;
	
	private bool previousMovesToggle;
	private bool nextMovesToggle;
	private bool blockableAreaToggle;
	private bool hitStunToggle;
	private bool damageToggle;
	private bool forceToggle;
	private bool selfForceToggle;
	private bool opponentForceToggle;
	private bool pullEnemyInToggle;
	private bool pullSelfInToggle;
	private bool hitsToggle;
	private bool hurtBoxToggle;
	private bool bodyPartsToggle;
	private bool opTransformToggle;
	
	
	private bool generalOptions;
	private bool animationOptions;
	private bool opOverrideOptions;
	private bool inputOptions;
	private bool moveLinksOptions;
	private bool particleEffectsOptions;
	private bool selfAppliedForceOptions;
	private bool slowMoOptions;
	private bool soundOptions;
	private bool stanceOptions;
	private bool textWarningOptions;
	private bool cameraOptions;
	private bool pullInOptions;
	private bool activeFramesOptions;
	private bool invincibleFramesOptions;
	private bool armorOptions;
	private bool classificationOptions;
	private bool playerConditions;
	private bool projectileOptions;

	
	private bool characterWarning;
	private string errorMsg;
	
	private string titleStyle;
	private string addButtonStyle;
	private string borderBarStyle;
	private string rootGroupStyle;
	private string subGroupStyle;
	private string arrayElementStyle;
	private string subArrayElementStyle;
	private string toggleStyle;
	private string foldStyle;
	private string enumStyle;
	private string fillBarStyle1;
	private string fillBarStyle2;
	private string fillBarStyle3;
	private string fillBarStyle4;
	private string fillBarStyle5;
	private GUIStyle labelStyle;
	private GameObject emulatedPlayer;
	private GameObject emulatedCamera;

	
	[MenuItem("Window/U.F.E./Move Editor")]
	[CanEditMultipleObjects]
	public static void Init(){
		moveEditorWindow = EditorWindow.GetWindow<MoveEditorWindow>(false, "Move", true);
		moveEditorWindow.Show();
		EditorWindow.FocusWindowIfItsOpen<SceneView>();
		Camera sceneCam = GameObject.FindObjectOfType<Camera>();
		if (sceneCam != null){
			moveEditorWindow.initialFieldOfView = sceneCam.fieldOfView;
			moveEditorWindow.initialCamPosition = sceneCam.transform.position;
			moveEditorWindow.initialCamRotation = sceneCam.transform.rotation;
		}
		moveEditorWindow.Populate();
	}
	
	void OnSelectionChange(){
		Populate();
		Repaint();
		Clear(false, false);
	}
	
	void OnEnable(){
		Populate();
	}
	
	void OnFocus(){
		Populate();
	}

	void OnDisable(){
		Clear(true, true, true, true, true);
	}

	void OnDestroy(){
		Clear(true, true, true, true, true);
	}

	void OnLostFocus(){
		//Clear(true, false);
	}
	
	void Clear(bool destroyChar, bool resetCam){
		Clear(destroyChar, resetCam, true, false);
	}
	
	void Clear(bool destroyChar, bool resetCam, bool p1, bool p2){
		Clear(destroyChar, resetCam, p1, p2, false);
	}

	void Clear(bool destroyChar, bool resetCam, bool p1, bool p2, bool projectile){
		if (moveInfo == null) return;
		if (destroyChar){
			if (p1 && characterPrefab != null) {
				Editor.DestroyImmediate(characterPrefab);
				characterPrefab = null;
				Editor.DestroyImmediate(emulatedPlayer);
				emulatedPlayer = null;
			}
			if (p2 && opCharacterPrefab != null){
				Editor.DestroyImmediate(opCharacterPrefab);
				opCharacterPrefab = null;
			}
			if (emulatedCamera != null) {
				Camera sceneCam = GameObject.FindObjectOfType<Camera>();
				if (sceneCam.transform.parent == emulatedCamera.transform) sceneCam.transform.parent = sceneCam.transform.parent.parent;
				Editor.DestroyImmediate(emulatedCamera);
				emulatedCamera = null;
			}
			if (moveInfo.cameraMovements != null && moveInfo.cameraMovements.Length > 0)
				foreach(CameraMovement camMove in moveInfo.cameraMovements) camMove.previewToggle = false;
			
			totalFramesGlobal = 0;
		}

		if (projectile && projectilePrefab != null){
			Editor.DestroyImmediate(projectilePrefab);
			projectilePrefab = null;
		}

		if (resetCam){
			Camera sceneCam = GameObject.FindObjectOfType<Camera>();
			if (sceneCam != null){
				sceneCam.fieldOfView = initialFieldOfView;
				sceneCam.transform.position = initialCamPosition;
				sceneCam.transform.rotation = sceneCam.transform.rotation;
			}
		}
	}
	
	void helpButton(string page){
		if (GUILayout.Button("?", GUILayout.Width(18), GUILayout.Height(18))) 
			Application.OpenURL("http://www.ufe3d.com/doku.php/"+ page);
	}

	void Update(){
		if (EditorApplication.isPlayingOrWillChangePlaymode) {
			if (characterPrefab != null){
				Editor.DestroyImmediate(characterPrefab);
				characterPrefab = null;
			}
			if (opCharacterPrefab != null){
				Editor.DestroyImmediate(opCharacterPrefab);
				opCharacterPrefab = null;
			}
			if (emulatedCamera != null) {
				Editor.DestroyImmediate(emulatedCamera);
				emulatedCamera = null;
			}
			if (projectilePrefab != null) {
				Editor.DestroyImmediate(projectilePrefab);
				projectilePrefab = null;
			}
		}
	}

	void Populate(){
		//initialFieldOfView = 16;
		//initialCamPosition = new Vector3(0,8,-34);
		//initialCamRotation = Quaternion.Euler(new Vector3(6,0,0));
		// Style Definitions
		titleStyle = "MeTransOffRight";
		addButtonStyle = "CN CountBadge";
		rootGroupStyle = "GroupBox";
		subGroupStyle = "ObjectFieldThumb";
		arrayElementStyle = "flow overlay box";
		subArrayElementStyle = "HelpBox";
		foldStyle = "Foldout";
		enumStyle = "MiniPopup";
		toggleStyle = "BoldToggle";
		borderBarStyle = "ProgressBarBack";
		fillBarStyle1 = "ProgressBarBar";
		fillBarStyle2 = "flow node 2 on";
		fillBarStyle3 = "flow node 4 on";
		fillBarStyle4 = "flow node 6 on";
		fillBarStyle5 = "flow node 5 on";

		labelStyle = new GUIStyle();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.fontStyle = FontStyle.Bold;
		labelStyle.normal.textColor = Color.white;

		if (sentMoveInfo != null){
			EditorGUIUtility.PingObject( sentMoveInfo );
			Selection.activeObject = sentMoveInfo;
			sentMoveInfo = null;
		}

		UFE.isAiAddonInstalled = UFE.IsInstalled("RuleBasedAI");

		if (moveInfo != null) {
			// 1.0.6 -> 1.1 Update
			if (moveInfo.possibleStates.Length > 0 && moveInfo.selfConditions.possibleMoveStates.Length == 0){
				foreach(PossibleStates possibleState in moveInfo.possibleStates){
					PossibleMoveStates pTemp = new PossibleMoveStates();
					pTemp.possibleState = possibleState;
					moveInfo.selfConditions.possibleMoveStates = AddElement<PossibleMoveStates>(moveInfo.selfConditions.possibleMoveStates, pTemp);
				}
				System.Array.Clear(moveInfo.possibleStates, 0, moveInfo.possibleStates.Length);
			}

			foreach(SoundEffect soundEffect in moveInfo.soundEffects){
				if (soundEffect.sound != null && soundEffect.sounds.Length == 0){
					soundEffect.sounds = AddElement<AudioClip>(soundEffect.sounds, soundEffect.sound);
					soundEffect.sound = null;
				}
			}
		}

		UnityEngine.Object[] selection = Selection.GetFiltered(typeof(MoveInfo), SelectionMode.Assets);
		if (selection.Length > 0){
			if (selection[0] == null) return;
			moveInfo = (MoveInfo) selection[0];
			fpsTemp = moveInfo.fps;
			animationSpeedTemp = moveInfo.animationSpeed;
			totalFramesTemp = moveInfo.totalFrames;
		}

		if (moveInfo != null && moveInfo.frameLinks != null && moveInfo.frameLinks.Length > 0){
			foreach(FrameLink frameLink in moveInfo.frameLinks){
				if (frameLink.linkableMoves != null && frameLink.linkableMoves.Length == 0) {
					frameLink.linkableMoves = AddElement<MoveInfo>(frameLink.linkableMoves, new MoveInfo());
				}
			}
		}
	}
	
	public void OnGUI(){
		if (moveInfo == null){
			GUILayout.BeginHorizontal("GroupBox");
			GUILayout.Label("Select a move file first or create a new move.","CN EntryInfo");
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			if (GUILayout.Button("Create new move"))
				ScriptableObjectUtility.CreateAsset<MoveInfo> ();
			return;
		}

		//EditorGUIUtility.labelWidth = 150;
		GUIStyle fontStyle = new GUIStyle();
		fontStyle.font = (Font) EditorGUIUtility.Load("EditorFont.TTF");
		fontStyle.fontSize = 30;
		fontStyle.alignment = TextAnchor.UpperCenter;
		fontStyle.normal.textColor = Color.white;
		fontStyle.hover.textColor = Color.white;
		EditorGUILayout.BeginVertical(titleStyle);{
			EditorGUILayout.BeginHorizontal();{
				EditorGUILayout.LabelField("", (moveInfo.moveName == ""? "New Move":moveInfo.moveName) , fontStyle, GUILayout.Height(32));
				helpButton("move:start");
			}EditorGUILayout.EndHorizontal();
		}EditorGUILayout.EndVertical();

		if (moveInfo.animationClip != null){
			moveInfo.totalFrames = (int)Mathf.Abs(Mathf.Floor((moveInfo.fps * moveInfo.animationClip.length) / moveInfo.animationSpeed));
			totalFramesTemp = (int)Mathf.Abs(Mathf.Floor((moveInfo.fps * moveInfo.animationClip.length) / animationSpeedTemp));
			if (moveInfo.totalFrames > totalFramesGlobal) totalFramesGlobal = moveInfo.totalFrames;
		}


		// Begin General Options
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);{
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					generalOptions = EditorGUILayout.Foldout(generalOptions, "General", foldStyle);
					helpButton("move:general");
				}EditorGUILayout.EndHorizontal();

				if (generalOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 180;

						moveInfo.moveName = EditorGUILayout.TextField("Move Name:", moveInfo.moveName);
						moveInfo.description = EditorGUILayout.TextField("Move Description:", moveInfo.description);

						EditorGUILayout.BeginHorizontal();{
							string unsaved = fpsTemp != moveInfo.fps ? "*":"";
							fpsTemp = EditorGUILayout.IntSlider("FPS Architecture:"+ unsaved, fpsTemp, 10, 120);
							EditorGUI.BeginDisabledGroup(fpsTemp == moveInfo.fps);{
								if (StyledButton("Apply")) moveInfo.fps = fpsTemp;
							}EditorGUI.EndDisabledGroup();

						}EditorGUILayout.EndHorizontal();

						
						SubGroupTitle("Behaviour");
						EditorGUIUtility.labelWidth = 230;
						moveInfo.ignoreGravity = EditorGUILayout.Toggle("Ignore Gravity", moveInfo.ignoreGravity, toggleStyle);
						moveInfo.ignoreFriction = EditorGUILayout.Toggle("Ignore Friction", moveInfo.ignoreFriction, toggleStyle);
						moveInfo.cancelMoveWheLanding = EditorGUILayout.Toggle("Cancel Move On Landing", moveInfo.cancelMoveWheLanding, toggleStyle);
						moveInfo.autoCorrectRotation = EditorGUILayout.Toggle("Auto Correct Rotation", moveInfo.autoCorrectRotation, toggleStyle);
						if (moveInfo.autoCorrectRotation){
							moveInfo.frameWindowRotation = EditorGUILayout.IntField("Frame Window:", Mathf.Clamp(moveInfo.frameWindowRotation, 0, moveInfo.totalFrames));
						}
						
						EditorGUIUtility.labelWidth = 150;
						EditorGUILayout.Space();

						EditorGUI.indentLevel -= 1;

					}EditorGUILayout.EndVertical();
				}

			}EditorGUILayout.EndVertical();
			// End General Options

			
			// Begin Gauge Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					moveInfo.gaugeToggle = EditorGUILayout.Foldout(moveInfo.gaugeToggle, "Gauge/Meter Options", foldStyle);
					helpButton("move:gauge");
				}EditorGUILayout.EndHorizontal();
				
				if (moveInfo.gaugeToggle){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 2;
						//EditorGUIUtility.labelWidth = 200;
						moveInfo.gaugeGainOnHit = StyledSlider("Gauge Gain on Hit", moveInfo.gaugeGainOnHit, EditorGUI.indentLevel, 0, 400);
						moveInfo.gaugeGainOnMiss = StyledSlider("Gauge Gain on Cast", moveInfo.gaugeGainOnMiss, EditorGUI.indentLevel, 0, 400);
						moveInfo.gaugeGainOnBlock = StyledSlider("Gauge Gain on Block", moveInfo.gaugeGainOnBlock, EditorGUI.indentLevel, 0, 400);
						moveInfo.opGaugeGainOnHit = StyledSlider("Op. Gauge Gain on Hit", moveInfo.opGaugeGainOnHit, EditorGUI.indentLevel, 0, 400);
						moveInfo.opGaugeGainOnBlock = StyledSlider("Op. Gauge Gain on Block", moveInfo.opGaugeGainOnBlock, EditorGUI.indentLevel, 0, 400);
						moveInfo.opGaugeGainOnParry = StyledSlider("Op. Gauge Gain on Parry", moveInfo.opGaugeGainOnParry, EditorGUI.indentLevel, 0, 400);
						moveInfo.gaugeUsage = StyledSlider("Gauge Required", moveInfo.gaugeUsage, EditorGUI.indentLevel, 0, 400);
						EditorGUI.indentLevel -= 2;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();


			// Begin Animation Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					animationOptions = EditorGUILayout.Foldout(animationOptions, "Animation", foldStyle);
					helpButton("move:animation");
				}EditorGUILayout.EndHorizontal();

				if (animationOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						EditorGUIUtility.labelWidth = 200;

						SubGroupTitle("Animation Options");
						moveInfo.animationClip = (AnimationClip) EditorGUILayout.ObjectField("Animation Clip:", moveInfo.animationClip, typeof(UnityEngine.AnimationClip), true);
						if (moveInfo.animationClip != null){
							moveInfo.wrapMode = (WrapMode)EditorGUILayout.EnumPopup("Wrap Mode:",moveInfo.wrapMode, enumStyle);
							
							moveInfo.disableHeadLook = EditorGUILayout.Toggle("Disable Head Look", moveInfo.disableHeadLook, toggleStyle);
							moveInfo.applyRootMotion = EditorGUILayout.Toggle("Apply Root Motion", moveInfo.applyRootMotion, toggleStyle);
							if (moveInfo.applyRootMotion){
								moveInfo.rootMotionNode = (BodyPart)EditorGUILayout.EnumPopup("Root Motion Node:", moveInfo.rootMotionNode, enumStyle);
                                moveInfo.forceGrounded = EditorGUILayout.Toggle("Force Grounded", moveInfo.forceGrounded, toggleStyle);
							}

							
							EditorGUI.indentLevel += 1;
							EditorGUILayout.Space();
							string unsaved = animationSpeedTemp != moveInfo.animationSpeed ? "*":"";
							animationSpeedTemp = StyledSlider("Animation Speed"+ unsaved, animationSpeedTemp, EditorGUI.indentLevel, -5, 5);
							EditorGUILayout.BeginHorizontal();{
								unsaved = totalFramesTemp != moveInfo.totalFrames ? "*":"";
								EditorGUILayout.LabelField("Total frames:"+ unsaved, totalFramesTemp.ToString());
								if (StyledButton("Apply")) moveInfo.animationSpeed = animationSpeedTemp;
							}EditorGUILayout.EndHorizontal();
							EditorGUILayout.Space();
							EditorGUI.indentLevel -= 1;


							SubGroupTitle("Blending");
							moveInfo.overrideBlendingIn = EditorGUILayout.Toggle("Override Blending (In)", moveInfo.overrideBlendingIn, toggleStyle);
							if (moveInfo.overrideBlendingIn){
								moveInfo.blendingIn = EditorGUILayout.FloatField("Blend In Duration:", moveInfo.blendingIn);
							}
							
							moveInfo.overrideBlendingOut = EditorGUILayout.Toggle("Override Blending (Out)", moveInfo.overrideBlendingOut, toggleStyle);
							if (moveInfo.overrideBlendingOut){
								moveInfo.blendingOut = EditorGUILayout.FloatField("Blend Out Duration:", moveInfo.blendingOut);
							}
							EditorGUILayout.Space();

							
							SubGroupTitle("Orientation");
							EditorGUIUtility.labelWidth = 230;
							moveInfo.forceMirrorLeft = EditorGUILayout.Toggle("Mirror Animation (Left)", moveInfo.forceMirrorLeft, toggleStyle);
							moveInfo.invertRotationLeft = EditorGUILayout.Toggle("Rotate Character (Left)", moveInfo.invertRotationLeft, toggleStyle);
							
							EditorGUILayout.Space();
							moveInfo.forceMirrorRight = EditorGUILayout.Toggle("Mirror Animation (Right)", moveInfo.forceMirrorRight, toggleStyle);
							moveInfo.invertRotationRight = EditorGUILayout.Toggle("Rotate Character (Right)", moveInfo.invertRotationRight, toggleStyle);
							EditorGUILayout.Space();

							
							SubGroupTitle("Preview");
							EditorGUIUtility.labelWidth = 180;
							GameObject newCharacterPrefab = (GameObject) EditorGUILayout.ObjectField("Character Prefab:", moveInfo.characterPrefab, typeof(UnityEngine.GameObject), true);
							if (newCharacterPrefab != null && moveInfo.characterPrefab != newCharacterPrefab && !EditorApplication.isPlayingOrWillChangePlaymode){
								if (PrefabUtility.GetPrefabType(newCharacterPrefab) != PrefabType.Prefab){
									characterWarning = true;
									errorMsg = "This character is not a prefab.";
								}else if (newCharacterPrefab.GetComponent<HitBoxesScript>() == null){
									characterWarning = true;
									errorMsg = "This character doesn't have hitboxes!\n Please add the HitboxScript and try again.";
								}else{
									characterWarning = false;
									moveInfo.characterPrefab = newCharacterPrefab;
								}
							}else if (moveInfo.characterPrefab != newCharacterPrefab && EditorApplication.isPlayingOrWillChangePlaymode){
								characterWarning = true;
								errorMsg = "You can't change this field while in play mode.";
							}else if (newCharacterPrefab == null) moveInfo.characterPrefab = null;

							if (characterPrefab == null){
								if (StyledButton("Animation Preview")){
									if (moveInfo.characterPrefab == null) {
										characterWarning = true;
										errorMsg = "Drag a character into 'Character Prefab' first.";
									}else if (EditorApplication.isPlayingOrWillChangePlaymode){
										characterWarning = true;
										errorMsg = "You can't preview animations while in play mode.";
									}else{
										characterWarning = false;
										EditorCamera.SetPosition(Vector3.up * 4);
										EditorCamera.SetRotation(Quaternion.identity);
										EditorCamera.SetOrthographic(true);
										EditorCamera.SetSize(10);

										characterPrefab = (GameObject) PrefabUtility.InstantiatePrefab(moveInfo.characterPrefab);
										characterPrefab.transform.position = new Vector3(0,0,0);
									}
								}

								if (characterWarning){
									GUILayout.BeginHorizontal("GroupBox");
									GUILayout.FlexibleSpace();
									GUILayout.Label(errorMsg,"CN EntryWarn");
									GUILayout.FlexibleSpace();
									GUILayout.EndHorizontal();
								}
							}else {
								EditorGUI.indentLevel += 1;
								if (smoothPreview){
									animFrame = StyledSlider("Animation Frames", animFrame, EditorGUI.indentLevel, 0, totalFramesGlobal);
								}else{
									animFrame = StyledSlider("Animation Frames", (int)animFrame, EditorGUI.indentLevel, 0, totalFramesGlobal);
								}
								EditorGUI.indentLevel -= 1;
								
								if (cameraOptions){
									GUILayout.BeginHorizontal("GroupBox");
									GUILayout.Label("You must close 'Cinematic Options' first.","CN EntryError");
									GUILayout.EndHorizontal();
								}

								smoothPreview = EditorGUILayout.Toggle("Smooth Preview", smoothPreview, toggleStyle);
								AnimationSampler(characterPrefab, moveInfo.animationClip, 0, true, true, moveInfo.forceMirrorLeft, moveInfo.invertRotationLeft);
								
								EditorGUILayout.Space();
								
								EditorGUILayout.BeginHorizontal();{
									if (StyledButton("Reset Scene View")){
										EditorCamera.SetPosition(Vector3.up * 4);
										EditorCamera.SetRotation(Quaternion.identity);
										EditorCamera.SetOrthographic(true);
										EditorCamera.SetSize(10);
									}
									if (StyledButton("Close Preview")) Clear (true, true);
								}EditorGUILayout.EndHorizontal();
								
								EditorGUILayout.Space();
							}
						}
						EditorGUI.indentLevel -= 1;
						EditorGUIUtility.labelWidth = 150;

					}EditorGUILayout.EndVertical();
				}else if (characterPrefab != null && !cameraOptions){
					Clear (true, true);
				}
				
			}EditorGUILayout.EndVertical();
			// End Animation Options


			// Begin Active Frame Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					activeFramesOptions = EditorGUILayout.Foldout(activeFramesOptions, "Active Frames", EditorStyles.foldout);
					helpButton("move:activeframes");
				}EditorGUILayout.EndHorizontal();
				
				if (activeFramesOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						
						// Hits Toggle
						hitsToggle = EditorGUILayout.Foldout(hitsToggle, "Hits ("+ moveInfo.hits.Length +")", EditorStyles.foldout);
						if (hitsToggle){
							EditorGUILayout.BeginVertical(subGroupStyle);{
								EditorGUI.indentLevel += 1;
								List<Vector3> castingValues = new List<Vector3>();
								foreach(Hit hit in moveInfo.hits) {
									castingValues.Add(new Vector3(hit.activeFramesBegin, hit.activeFramesEnds, (hit.hitConfirmType == HitConfirmType.Throw? 1: 0)));
								}
								StyledMarker("Frame Data Timeline", castingValues.ToArray(), moveInfo.totalFrames, 
								             EditorGUI.indentLevel, true);
								
								for (int i = 0; i < moveInfo.hits.Length; i ++){
									EditorGUILayout.Space();
									EditorGUILayout.BeginVertical(arrayElementStyle);{
										EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();{
											StyledMinMaxSlider("Active Frames", ref moveInfo.hits[i].activeFramesBegin, ref moveInfo.hits[i].activeFramesEnds, 1, moveInfo.totalFrames, EditorGUI.indentLevel);
											if (GUILayout.Button("", "PaneOptions")){
												PaneOptions<Hit>(moveInfo.hits, moveInfo.hits[i], delegate (Hit[] newElement) { moveInfo.hits = newElement; });
											}
										}EditorGUILayout.EndHorizontal();
										
										EditorGUIUtility.labelWidth = 180;
										
										EditorGUILayout.Space();
										
										moveInfo.hits[i].hitConfirmType = (HitConfirmType) EditorGUILayout.EnumPopup("Hit Confirm Type:", moveInfo.hits[i].hitConfirmType, enumStyle);

										EditorGUILayout.Space();
										
										// Hurt Boxes Toggle
										int amount = moveInfo.hits[i].hurtBoxes != null ? moveInfo.hits[i].hurtBoxes.Length : 0;
										moveInfo.hits[i].hurtBoxesToggle = EditorGUILayout.Foldout(moveInfo.hits[i].hurtBoxesToggle, "Hurt Boxes ("+ amount +")", EditorStyles.foldout);
										if (moveInfo.hits[i].hurtBoxesToggle){
											EditorGUILayout.BeginVertical(subGroupStyle);{
												EditorGUI.indentLevel += 1;
												if (amount > 0){
													for (int y = 0; y < moveInfo.hits[i].hurtBoxes.Length; y ++){
														EditorGUILayout.BeginVertical(subArrayElementStyle);{
															EditorGUILayout.BeginHorizontal();{
																moveInfo.hits[i].hurtBoxes[y].bodyPart = (BodyPart)EditorGUILayout.EnumPopup("Body Part:", moveInfo.hits[i].hurtBoxes[y].bodyPart, enumStyle);
																if (GUILayout.Button("", "PaneOptions")){
																	PaneOptions<HurtBox>(moveInfo.hits[i].hurtBoxes, moveInfo.hits[i].hurtBoxes[y], delegate (HurtBox[] newElement) { moveInfo.hits[i].hurtBoxes = newElement; });
																}
															}EditorGUILayout.EndHorizontal();
															
															moveInfo.hits[i].hurtBoxes[y].shape = (HitBoxShape) EditorGUILayout.EnumPopup("Shape:", moveInfo.hits[i].hurtBoxes[y].shape, enumStyle);
															if (moveInfo.hits[i].hurtBoxes[y].shape == HitBoxShape.circle){
																moveInfo.hits[i].hurtBoxes[y].radius = EditorGUILayout.FloatField("Radius:", moveInfo.hits[i].hurtBoxes[y].radius);
																moveInfo.hits[i].hurtBoxes[y].offSet = EditorGUILayout.Vector2Field("Off Set:", moveInfo.hits[i].hurtBoxes[y].offSet);
															}else{
																moveInfo.hits[i].hurtBoxes[y].rect = EditorGUILayout.RectField("Rectangle:", moveInfo.hits[i].hurtBoxes[y].rect);
																
																EditorGUIUtility.labelWidth = 200;
																bool tmpFollowXBounds = moveInfo.hits[i].hurtBoxes[y].followXBounds;
																bool tmpFollowYBounds = moveInfo.hits[i].hurtBoxes[y].followYBounds;
																
																moveInfo.hits[i].hurtBoxes[y].followXBounds = EditorGUILayout.Toggle("Follow Bounds (X)", moveInfo.hits[i].hurtBoxes[y].followXBounds);
																moveInfo.hits[i].hurtBoxes[y].followYBounds = EditorGUILayout.Toggle("Follow Bounds (Y)", moveInfo.hits[i].hurtBoxes[y].followYBounds);
																
																if (tmpFollowXBounds != moveInfo.hits[i].hurtBoxes[y].followXBounds)
																	moveInfo.hits[i].hurtBoxes[y].rect.width = moveInfo.hits[i].hurtBoxes[y].followXBounds ? 0 : 4;
																if (tmpFollowYBounds != moveInfo.hits[i].hurtBoxes[y].followYBounds)
																	moveInfo.hits[i].hurtBoxes[y].rect.height = moveInfo.hits[i].hurtBoxes[y].followYBounds ? 0 : 4;
																
																EditorGUIUtility.labelWidth = 150;
															}

															EditorGUILayout.Space();
														}EditorGUILayout.EndVertical();
													}
												}
												if (StyledButton("New Hurt Box"))
													moveInfo.hits[i].hurtBoxes = AddElement<HurtBox>(moveInfo.hits[i].hurtBoxes, new HurtBox());
												
												EditorGUI.indentLevel -= 1;
											}EditorGUILayout.EndVertical();
										}
										
										EditorGUILayout.Space();
										
										if (moveInfo.hits[i].hitConfirmType == HitConfirmType.Hit){
											moveInfo.hits[i].continuousHit = EditorGUILayout.Toggle("Continuous Hit", moveInfo.hits[i].continuousHit, toggleStyle);
											if (moveInfo.hits[i].continuousHit){
												moveInfo.hits[i].spaceBetweenHits = (Sizes) EditorGUILayout.EnumPopup("Space Between Hits:", moveInfo.hits[i].spaceBetweenHits, enumStyle);
											}
											moveInfo.hits[i].armorBreaker = EditorGUILayout.Toggle("Armor Breaker", moveInfo.hits[i].armorBreaker, toggleStyle);

											moveInfo.hits[i].hitType = (HitType) EditorGUILayout.EnumPopup("Hit Type:", moveInfo.hits[i].hitType, enumStyle);

											moveInfo.hits[i].hitStrength = (HitStrengh) EditorGUILayout.EnumPopup("Hit Strength:", moveInfo.hits[i].hitStrength, enumStyle);
											
											moveInfo.hits[i].cornerPush = EditorGUILayout.Toggle("Corner Push", moveInfo.hits[i].cornerPush, toggleStyle);
											moveInfo.hits[i].overrideHitEffects = EditorGUILayout.Toggle("Override Hit Effects", moveInfo.hits[i].overrideHitEffects, toggleStyle);
											if (moveInfo.hits[i].overrideHitEffects) HitOptionBlock("Hit Effects", moveInfo.hits[i].hitEffects);
											
											// Hit Conditions Toggle
											moveInfo.hits[i].hitConditionsToggle = EditorGUILayout.Foldout(moveInfo.hits[i].hitConditionsToggle, "Hit Conditions", EditorStyles.foldout);
											if (moveInfo.hits[i].hitConditionsToggle){
												EditorGUILayout.BeginVertical(subGroupStyle);{
													EditorGUIUtility.labelWidth = 180;
													EditorGUI.indentLevel += 1;
													EditorGUILayout.LabelField("Opponent can be:");
													moveInfo.hits[i].groundHit = EditorGUILayout.Toggle("-Standing/Crouching", moveInfo.hits[i].groundHit, toggleStyle);
													EditorGUI.BeginDisabledGroup(moveInfo.hits[i].hitType == HitType.MidKnockdown || moveInfo.hits[i].hitType == HitType.HighKnockdown || moveInfo.hits[i].hitType == HitType.Sweep);{
														moveInfo.hits[i].airHit = EditorGUILayout.Toggle("-In the Air", moveInfo.hits[i].airHit, toggleStyle);
													}EditorGUI.EndDisabledGroup();
													moveInfo.hits[i].stunHit = EditorGUILayout.Toggle("-Stunned", moveInfo.hits[i].stunHit, toggleStyle);
													moveInfo.hits[i].downHit = EditorGUILayout.Toggle("-Down", moveInfo.hits[i].downHit, toggleStyle);
													EditorGUI.indentLevel -= 1;
													EditorGUIUtility.labelWidth = 150;
												}EditorGUILayout.EndVertical();
											}

											
											// Damage Toggle
											moveInfo.hits[i].damageOptionsToggle = EditorGUILayout.Foldout(moveInfo.hits[i].damageOptionsToggle, "Damage Options", EditorStyles.foldout);
											if (moveInfo.hits[i].damageOptionsToggle){
												EditorGUILayout.BeginVertical(subGroupStyle);{
													EditorGUI.indentLevel += 1;
													moveInfo.hits[i].damageType = (DamageType) EditorGUILayout.EnumPopup("Damage Type:", moveInfo.hits[i].damageType, enumStyle);
													moveInfo.hits[i].damageOnHit = EditorGUILayout.FloatField("Damage on Hit:", moveInfo.hits[i].damageOnHit);
													moveInfo.hits[i].damageOnBlock = EditorGUILayout.FloatField("Damage on Block:", moveInfo.hits[i].damageOnBlock);
													moveInfo.hits[i].damageScaling = EditorGUILayout.Toggle("Damage Scaling", moveInfo.hits[i].damageScaling, toggleStyle);
													moveInfo.hits[i].doesntKill = EditorGUILayout.Toggle("Hit Doesn't Kill", moveInfo.hits[i].doesntKill, toggleStyle);
													EditorGUI.indentLevel -= 1;
												}EditorGUILayout.EndVertical();
											}
											
											// Hit Stun Toggle
											moveInfo.hits[i].hitStunOptionsToggle = EditorGUILayout.Foldout(moveInfo.hits[i].hitStunOptionsToggle, "Hit Stun Options", EditorStyles.foldout);
											if (moveInfo.hits[i].hitStunOptionsToggle){
												EditorGUILayout.BeginVertical(subGroupStyle);{
													EditorGUI.indentLevel += 1;
													moveInfo.hits[i].hitStunType = (HitStunType) EditorGUILayout.EnumPopup("Hit Stun Type:", moveInfo.hits[i].hitStunType, enumStyle);
													moveInfo.hits[i].resetPreviousHitStun = EditorGUILayout.Toggle("Reset Hit Stun", moveInfo.hits[i].resetPreviousHitStun, toggleStyle);
													EditorGUI.BeginDisabledGroup(moveInfo.hits[i].hitType == HitType.MidKnockdown || moveInfo.hits[i].hitType == HitType.HighKnockdown || moveInfo.hits[i].hitType == HitType.Sweep);{
														if (moveInfo.hits[i].hitStunType == HitStunType.FrameAdvantage){
															EditorGUILayout.LabelField("Frame Advantage on Hit:");
															moveInfo.hits[i].frameAdvantageOnHit = EditorGUILayout.IntSlider("", moveInfo.hits[i].frameAdvantageOnHit, -40, 120);
														}else{
															moveInfo.hits[i].hitStunOnHit = EditorGUILayout.FloatField("Hit Stun on Hit:", moveInfo.hits[i].hitStunOnHit);
															if (moveInfo.hits[i].hitStunType == HitStunType.Frames) moveInfo.hits[i].hitStunOnHit = Mathf.Ceil(moveInfo.hits[i].hitStunOnHit);
														}
													}EditorGUI.EndDisabledGroup();
													if (moveInfo.hits[i].hitStunType == HitStunType.FrameAdvantage){
														EditorGUILayout.LabelField("Frame Advantage on Block:");
														moveInfo.hits[i].frameAdvantageOnBlock = EditorGUILayout.IntSlider("", moveInfo.hits[i].frameAdvantageOnBlock, -40, 120);
													}else{
														moveInfo.hits[i].hitStunOnBlock = EditorGUILayout.FloatField("Hit Stun on Block:", moveInfo.hits[i].hitStunOnBlock);
														if (moveInfo.hits[i].hitStunType == HitStunType.Frames) moveInfo.hits[i].hitStunOnBlock = Mathf.Ceil(moveInfo.hits[i].hitStunOnBlock);
													}
													EditorGUI.indentLevel -= 1;
												}EditorGUILayout.EndVertical();
											}
											
											// Force Toggle
											moveInfo.hits[i].forceOptionsToggle = EditorGUILayout.Foldout(moveInfo.hits[i].forceOptionsToggle, "Force Options", EditorStyles.foldout);
											if (moveInfo.hits[i].forceOptionsToggle){
												EditorGUILayout.BeginVertical(subGroupStyle);{
													EditorGUI.indentLevel += 1;
													opponentForceToggle = EditorGUILayout.Foldout(opponentForceToggle, "Opponent", EditorStyles.foldout);
													if (opponentForceToggle){
														EditorGUI.indentLevel += 1;
														EditorGUI.BeginDisabledGroup(moveInfo.hits[i].hitType == HitType.MidKnockdown || moveInfo.hits[i].hitType == HitType.HighKnockdown || moveInfo.hits[i].hitType == HitType.Sweep);{
															moveInfo.hits[i].resetPreviousHorizontalPush = EditorGUILayout.Toggle("Reset X Forces", moveInfo.hits[i].resetPreviousHorizontalPush, toggleStyle);
															moveInfo.hits[i].resetPreviousVerticalPush = EditorGUILayout.Toggle("Reset Y Forces", moveInfo.hits[i].resetPreviousVerticalPush, toggleStyle);
															moveInfo.hits[i].pushForce = EditorGUILayout.Vector2Field("Applied Force", moveInfo.hits[i].pushForce);
														}EditorGUI.EndDisabledGroup();
														EditorGUI.indentLevel -= 1;
													}
													selfForceToggle = EditorGUILayout.Foldout(selfForceToggle, "Self", EditorStyles.foldout);
													if (selfForceToggle){
														EditorGUI.indentLevel += 1;
														moveInfo.hits[i].resetPreviousHorizontal = EditorGUILayout.Toggle("Reset X Forces", moveInfo.hits[i].resetPreviousHorizontal, toggleStyle);
														moveInfo.hits[i].resetPreviousVertical = EditorGUILayout.Toggle("Reset Y Forces", moveInfo.hits[i].resetPreviousVertical, toggleStyle);
														moveInfo.hits[i].appliedForce = EditorGUILayout.Vector2Field("Applied Force", moveInfo.hits[i].appliedForce);
														EditorGUI.indentLevel -= 1;
													}
													EditorGUI.indentLevel -= 1;
												}EditorGUILayout.EndVertical();
											}
											
											// Pull In Toggle
											moveInfo.hits[i].pullInToggle = EditorGUILayout.Foldout(moveInfo.hits[i].pullInToggle, "Pull In Options", EditorStyles.foldout);
											if (moveInfo.hits[i].pullInToggle){
												EditorGUILayout.BeginVertical(subGroupStyle);{
													EditorGUI.indentLevel += 1;
													pullEnemyInToggle = EditorGUILayout.Foldout(pullEnemyInToggle, "Opponent Towards Self", EditorStyles.foldout);
													if (pullEnemyInToggle){
														EditorGUI.indentLevel += 1;
														moveInfo.hits[i].pullEnemyIn.speed = EditorGUILayout.IntSlider("Speed:", moveInfo.hits[i].pullEnemyIn.speed, 1, 100);
														moveInfo.hits[i].pullEnemyIn.characterBodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part (self):", moveInfo.hits[i].pullEnemyIn.characterBodyPart, enumStyle);
														moveInfo.hits[i].pullEnemyIn.enemyBodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part (enemy):", moveInfo.hits[i].pullEnemyIn.enemyBodyPart, enumStyle);
														moveInfo.hits[i].pullEnemyIn.targetDistance = EditorGUILayout.FloatField("Distance:", moveInfo.hits[i].pullEnemyIn.targetDistance);
														moveInfo.hits[i].pullEnemyIn.forceStand = EditorGUILayout.Toggle("Force Ground Stand", moveInfo.hits[i].pullEnemyIn.forceStand, toggleStyle);
														EditorGUI.indentLevel -= 1;
													}
													pullSelfInToggle = EditorGUILayout.Foldout(pullSelfInToggle, "Self Towards Opponent", EditorStyles.foldout);
													if (pullSelfInToggle){
														EditorGUI.indentLevel += 1;
														moveInfo.hits[i].pullSelfIn.speed = EditorGUILayout.IntSlider("Speed:", moveInfo.hits[i].pullSelfIn.speed, 1, 100);
														moveInfo.hits[i].pullSelfIn.characterBodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part (self):", moveInfo.hits[i].pullSelfIn.characterBodyPart, enumStyle);
														moveInfo.hits[i].pullSelfIn.enemyBodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part (enemy):", moveInfo.hits[i].pullSelfIn.enemyBodyPart, enumStyle);
														moveInfo.hits[i].pullEnemyIn.targetDistance = EditorGUILayout.FloatField("Distance:", moveInfo.hits[i].pullSelfIn.targetDistance);
														moveInfo.hits[i].pullSelfIn.forceStand = EditorGUILayout.Toggle("Force Ground Stand", moveInfo.hits[i].pullSelfIn.forceStand, toggleStyle);
														EditorGUI.indentLevel -= 1;
													}
													EditorGUI.indentLevel -= 1;
												}EditorGUILayout.EndVertical();
											}
											EditorGUIUtility.labelWidth = 150;
										}else if (moveInfo.hits[i].hitConfirmType == HitConfirmType.Throw){
											
											EditorGUIUtility.labelWidth = 180;
											// Hit Conditions Toggle
											moveInfo.hits[i].hitConditionsToggle = EditorGUILayout.Foldout(moveInfo.hits[i].hitConditionsToggle, "Hit Conditions", EditorStyles.foldout);
											if (moveInfo.hits[i].hitConditionsToggle){
												EditorGUILayout.BeginVertical(subGroupStyle);{
													EditorGUI.indentLevel += 1;
													EditorGUILayout.LabelField("Opponent can be:");
													moveInfo.hits[i].groundHit = EditorGUILayout.Toggle("-Standing/Crouching", moveInfo.hits[i].groundHit, toggleStyle);
													moveInfo.hits[i].airHit = EditorGUILayout.Toggle("-In the Air", moveInfo.hits[i].airHit, toggleStyle);
													moveInfo.hits[i].stunHit = EditorGUILayout.Toggle("-Stunned", moveInfo.hits[i].stunHit, toggleStyle);
													moveInfo.hits[i].downHit = EditorGUILayout.Toggle("-Down", moveInfo.hits[i].downHit, toggleStyle);
													EditorGUI.indentLevel -= 1;
												}EditorGUILayout.EndVertical();
											}

											EditorGUILayout.Space();
											
											moveInfo.hits[i].throwMove = (MoveInfo) EditorGUILayout.ObjectField("Throw Move Confirm:", moveInfo.hits[i].throwMove, typeof(MoveInfo), false);
											EditorGUILayout.BeginHorizontal();{
												EditorGUILayout.Space();
												if (GUILayout.Button("Open Move", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Width(250)})) {
													MoveEditorWindow.sentMoveInfo = moveInfo.hits[i].throwMove;
													MoveEditorWindow.Init();
												}
												EditorGUILayout.Space();
											}EditorGUILayout.EndHorizontal();
											
											EditorGUILayout.Space();
											
											moveInfo.hits[i].techable = EditorGUILayout.Toggle("Techable", moveInfo.hits[i].techable, toggleStyle);
											if (moveInfo.hits[i].techable){
												moveInfo.hits[i].techMove = (MoveInfo) EditorGUILayout.ObjectField("Tech Move:", moveInfo.hits[i].techMove, typeof(MoveInfo), false);
												EditorGUILayout.BeginHorizontal();{
													EditorGUILayout.Space();
													if (GUILayout.Button("Open Move", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Width(250)})) {
														MoveEditorWindow.sentMoveInfo = moveInfo.hits[i].techMove;
														MoveEditorWindow.Init();
													}
													EditorGUILayout.Space();
												}EditorGUILayout.EndHorizontal();
											}
											
											EditorGUIUtility.labelWidth = 150;
										}
										
										EditorGUILayout.Space();
									}EditorGUILayout.EndVertical();
								}
								
								if (StyledButton("New Hit"))
									moveInfo.hits = AddElement<Hit>(moveInfo.hits, new Hit());
								
								EditorGUI.indentLevel -= 1;
							}EditorGUILayout.EndVertical();
						}
						
						// Blockable Area Toggle
						blockableAreaToggle = EditorGUILayout.Foldout(blockableAreaToggle, "Blockable Area", EditorStyles.foldout);
						if (blockableAreaToggle){
							EditorGUILayout.BeginVertical(subGroupStyle);{
								EditorGUI.indentLevel += 1;
								StyledMinMaxSlider("Active Frames", ref moveInfo.blockableArea.activeFramesBegin, ref moveInfo.blockableArea.activeFramesEnds, 1, moveInfo.totalFrames, EditorGUI.indentLevel);
								
								moveInfo.blockableArea.bodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part:", moveInfo.blockableArea.bodyPart, enumStyle);
								moveInfo.blockableArea.shape = (HitBoxShape) EditorGUILayout.EnumPopup("Shape:", moveInfo.blockableArea.shape, enumStyle);
								if (moveInfo.blockableArea.shape == HitBoxShape.circle){
									moveInfo.blockableArea.radius = EditorGUILayout.FloatField("Radius:", moveInfo.blockableArea.radius);
									moveInfo.blockableArea.offSet = EditorGUILayout.Vector2Field("Off Set:", moveInfo.blockableArea.offSet);
								}else{
									moveInfo.blockableArea.rect = EditorGUILayout.RectField("Rectangle:", moveInfo.blockableArea.rect);
								}

								EditorGUI.indentLevel -= 1;
							}EditorGUILayout.EndVertical();
						}
						EditorGUI.indentLevel -= 1;
						
					}EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
			// End Active Frame Options

#if !UFE_BASIC
			// Begin Opponent Override Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					opOverrideOptions = EditorGUILayout.Foldout(opOverrideOptions, "Opponent Override ("+ moveInfo.opponentOverride.Length +")", foldStyle);
					helpButton("move:opponentoverride");
				}EditorGUILayout.EndHorizontal();
				
				if (opOverrideOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(OpponentOverride opponentOverride in moveInfo.opponentOverride) 
							castingValues.Add(opponentOverride.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);
						
						for (int i = 0; i < moveInfo.opponentOverride.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								SubGroupTitle("Casting Options");
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									moveInfo.opponentOverride[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.opponentOverride[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<OpponentOverride>(moveInfo.opponentOverride, moveInfo.opponentOverride[i], delegate (OpponentOverride[] newElement) { moveInfo.opponentOverride = newElement; });
									}
								}EditorGUILayout.EndHorizontal();

								EditorGUIUtility.labelWidth = 180;
								
								opTransformToggle = EditorGUILayout.Foldout(opTransformToggle, "Move To Position", EditorStyles.foldout);
								if (opTransformToggle){
									EditorGUILayout.BeginVertical(subArrayElementStyle);{
										moveInfo.opponentOverride[i].position = EditorGUILayout.Vector3Field("Position", moveInfo.opponentOverride[i].position);
										moveInfo.opponentOverride[i].blendSpeed = EditorGUILayout.IntSlider("Move Speed", moveInfo.opponentOverride[i].blendSpeed, 1, 100);
										EditorGUILayout.Space();
									}EditorGUILayout.EndVertical();
								}
								moveInfo.opponentOverride[i].overrideHitAnimations = EditorGUILayout.Toggle("Override Hit Animations", moveInfo.opponentOverride[i].overrideHitAnimations, toggleStyle);
								moveInfo.opponentOverride[i].resetAppliedForces = EditorGUILayout.Toggle("Reset All Applied Forces", moveInfo.opponentOverride[i].resetAppliedForces, toggleStyle);
								moveInfo.opponentOverride[i].stun = EditorGUILayout.Toggle("Stun", moveInfo.opponentOverride[i].stun, toggleStyle);
								if (moveInfo.opponentOverride[i].stun){
									moveInfo.opponentOverride[i].stunTime = EditorGUILayout.FloatField("Stun time (frames)", moveInfo.opponentOverride[i].stunTime);
									moveInfo.opponentOverride[i].standUpOptions = (StandUpOptions)EditorGUILayout.EnumPopup("Stand Up Animation: ",moveInfo.opponentOverride[i].standUpOptions, enumStyle);
								}

								EditorGUILayout.Space();
								SubGroupTitle("Move Override");
								EditorGUILayout.Space();

								moveInfo.opponentOverride[i].move = (MoveInfo) EditorGUILayout.ObjectField("Default Move:", moveInfo.opponentOverride[i].move, typeof(MoveInfo), false);

								moveInfo.opponentOverride[i].movesToggle = EditorGUILayout.Foldout(moveInfo.opponentOverride[i].movesToggle, "Character Specific Move ("+ moveInfo.opponentOverride[i].characterSpecificMoves.Length +")", EditorStyles.foldout);
								if (moveInfo.opponentOverride[i].movesToggle){
									EditorGUI.indentLevel += 1;
									EditorGUILayout.BeginVertical(subGroupStyle);{
										EditorGUIUtility.labelWidth = 180;
										for (int k = 0; k < moveInfo.opponentOverride[i].characterSpecificMoves.Length; k ++){
											EditorGUILayout.Space();
											EditorGUILayout.BeginVertical(subArrayElementStyle);{
												EditorGUILayout.Space();
												EditorGUILayout.BeginHorizontal();{
													moveInfo.opponentOverride[i].characterSpecificMoves[k].characterName = EditorGUILayout.TextField("Character Name:", moveInfo.opponentOverride[i].characterSpecificMoves[k].characterName);
													if (GUILayout.Button("", "PaneOptions")){
														PaneOptions<CharacterSpecificMoves>(moveInfo.opponentOverride[i].characterSpecificMoves, moveInfo.opponentOverride[i].characterSpecificMoves[k], delegate (CharacterSpecificMoves[] newElement) { moveInfo.opponentOverride[i].characterSpecificMoves = newElement; });
													}
												}EditorGUILayout.EndHorizontal();
												moveInfo.opponentOverride[i].characterSpecificMoves[k].move = (MoveInfo) EditorGUILayout.ObjectField("Move:", moveInfo.opponentOverride[i].characterSpecificMoves[k].move, typeof(MoveInfo), false);
												EditorGUILayout.Space();
											}EditorGUILayout.EndVertical();
										}
										EditorGUILayout.Space();
										
										if (StyledButton("New Character Specific Move"))
											moveInfo.opponentOverride[i].characterSpecificMoves = AddElement<CharacterSpecificMoves>(moveInfo.opponentOverride[i].characterSpecificMoves, null);
										
										EditorGUILayout.Space();
										
									}EditorGUILayout.EndVertical();
									
									EditorGUI.indentLevel -= 1;
									EditorGUILayout.Space();
								}
								
								EditorGUILayout.Space();
								SubGroupTitle("Orientation");
								EditorGUILayout.Space();
								
								EditorGUIUtility.labelWidth = 200;
								if (moveInfo.opponentOverride[i].move != null){
									moveInfo.opponentOverride[i].move.forceMirrorLeft = EditorGUILayout.Toggle("Mirror Animation (Left)", moveInfo.opponentOverride[i].move.forceMirrorLeft, toggleStyle);
									moveInfo.opponentOverride[i].move.invertRotationLeft = EditorGUILayout.Toggle("Rotate Character (Left)", moveInfo.opponentOverride[i].move.invertRotationLeft, toggleStyle);
									
									EditorGUILayout.Space();
									moveInfo.opponentOverride[i].move.forceMirrorRight = EditorGUILayout.Toggle("Mirror Animation (Right)", moveInfo.opponentOverride[i].move.forceMirrorRight, toggleStyle);
									moveInfo.opponentOverride[i].move.invertRotationRight = EditorGUILayout.Toggle("Rotate Character (Right)", moveInfo.opponentOverride[i].move.invertRotationRight, toggleStyle);
									EditorGUILayout.Space();
								}else{
									EditorGUI.BeginDisabledGroup(true);{
										bool dummyVar = false;
										dummyVar = EditorGUILayout.Toggle("Mirror Animation (Left)", dummyVar, toggleStyle);
										dummyVar = EditorGUILayout.Toggle("Rotate Character (Left)", dummyVar, toggleStyle);
										
										EditorGUILayout.Space();
										dummyVar = EditorGUILayout.Toggle("Mirror Animation (Right)", dummyVar, toggleStyle);
										dummyVar = EditorGUILayout.Toggle("Rotate Character (Right)", dummyVar, toggleStyle);
										EditorGUILayout.Space();
									}EditorGUI.EndDisabledGroup();
								}
								
								SubGroupTitle("Preview");
								if (moveInfo.opponentOverride[i].move != null && opCharacterPrefab == null){
									if (StyledButton("Animation Preview")){
										if (moveInfo.opponentOverride[i].move.characterPrefab == null) {
											characterWarning = true;
											errorMsg = "'Character Prefab' for this move not found.";
										}else if (EditorApplication.isPlayingOrWillChangePlaymode){
											characterWarning = true;
											errorMsg = "You can't preview animations while in play mode.";
											//}else if (moveInfo.animationClip.isAnimatorMotion){
											//	characterWarning = true;
											//	errorMsg = "This animation must be marked as\n'Legacy' in order to be previewed.";
										}else{
											characterWarning = false;
											EditorCamera.SetPosition(Vector3.up * 4);
											EditorCamera.SetRotation(Quaternion.identity);
											EditorCamera.SetOrthographic(true);
											EditorCamera.SetSize(10);
											
											foreach(OpponentOverride opOverride in moveInfo.opponentOverride)
												opOverride.animationPreview = false;
											
											moveInfo.opponentOverride[i].animationPreview = true;
											
											opCharacterPrefab = (GameObject) PrefabUtility.InstantiatePrefab(moveInfo.opponentOverride[i].move.characterPrefab);
										}
									}
									
									if (characterWarning){
										GUILayout.BeginHorizontal("GroupBox");
										GUILayout.FlexibleSpace();
										GUILayout.Label(errorMsg,"CN EntryWarn");
										GUILayout.FlexibleSpace();
										GUILayout.EndHorizontal();
									}
								}else if (opCharacterPrefab != null && moveInfo.opponentOverride[i].animationPreview) {

									if (moveInfo.opponentOverride[i].move.totalFrames > totalFramesGlobal)
										totalFramesGlobal = moveInfo.opponentOverride[i].move.totalFrames;

									EditorGUI.indentLevel += 1;
									if (smoothPreview){
										animFrame = StyledSlider("Animation Frames", animFrame, EditorGUI.indentLevel, 0, totalFramesGlobal);
									}else{
										animFrame = StyledSlider("Animation Frames", (int)animFrame, EditorGUI.indentLevel, 0, totalFramesGlobal);
									}
									EditorGUI.indentLevel -= 1;
									
									if (cameraOptions){
										GUILayout.BeginHorizontal("GroupBox");
										GUILayout.Label("You must close 'Camera Preview' first.","CN EntryError");
										GUILayout.EndHorizontal();
									}
									
									EditorGUIUtility.labelWidth = 200;
									smoothPreview = EditorGUILayout.Toggle("Smooth Preview", smoothPreview, toggleStyle);
									
									opCharacterPrefab.transform.position = moveInfo.opponentOverride[i].position;
									AnimationSampler(opCharacterPrefab, 
									                 moveInfo.opponentOverride[i].move.animationClip,
									                 moveInfo.opponentOverride[i].castingFrame, true, false,
									                 moveInfo.opponentOverride[i].move.forceMirrorLeft,
									                 moveInfo.opponentOverride[i].move.invertRotationLeft);
									
									EditorGUILayout.Space();
									
									EditorGUILayout.BeginHorizontal();{
										if (StyledButton("Reset Scene View")){
											EditorCamera.SetPosition(Vector3.up * 4);
											EditorCamera.SetRotation(Quaternion.identity);
											EditorCamera.SetOrthographic(true);
											EditorCamera.SetSize(10);
										}
										if (StyledButton("Close Preview")) Clear (true, true, false, true);
									}EditorGUILayout.EndHorizontal();
									
									EditorGUILayout.Space();
								}

								EditorGUIUtility.labelWidth = 150;
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
						if (StyledButton("New Override"))
							moveInfo.opponentOverride = AddElement<OpponentOverride>(moveInfo.opponentOverride, new OpponentOverride());
						
						EditorGUI.indentLevel -= 1;
						
					}EditorGUILayout.EndVertical();

				}else if (characterPrefab != null && !cameraOptions){
					Clear (true, false, false, true);
				}

			}EditorGUILayout.EndVertical();
			// End Opponent Override Options
#endif
			
			
			// Player Conditions
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					playerConditions = EditorGUILayout.Foldout(playerConditions, "Player Conditions", EditorStyles.foldout);
					helpButton("move:playerconditions");
				}EditorGUILayout.EndHorizontal();
				
				if (playerConditions){
					PlayerConditionsGroup("Self", moveInfo.selfConditions);
					PlayerConditionsGroup("Opponent", moveInfo.opponentConditions);
				}
			}EditorGUILayout.EndVertical();
			// End Player Conditions


			// Begin Input Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					inputOptions = EditorGUILayout.Foldout(inputOptions, "Input", EditorStyles.foldout);
					helpButton("move:input");
				}EditorGUILayout.EndHorizontal();

				if (inputOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						moveInfo.chargeMove = EditorGUILayout.Toggle("Charge Move", moveInfo.chargeMove, toggleStyle);
						if (moveInfo.chargeMove){
							moveInfo.chargeTiming = Mathf.Max(EditorGUILayout.FloatField("Charge Timing:", moveInfo.chargeTiming), 0);
						}

						moveInfo.allowInputLeniency = EditorGUILayout.Toggle("Allow Input Leniency", moveInfo.allowInputLeniency, toggleStyle);
						if (moveInfo.allowInputLeniency){
							moveInfo.leniencyBuffer = EditorGUILayout.IntSlider("Leniency Input Buffer:", moveInfo.leniencyBuffer, 1, 8);
						}
						
						// Button Sequence
						buttonSequenceToggle = EditorGUILayout.Foldout(buttonSequenceToggle, "Button Sequences ("+ moveInfo.buttonSequence.Length +")", EditorStyles.foldout);
						if (buttonSequenceToggle){
							EditorGUILayout.BeginVertical(subGroupStyle);{
								EditorGUI.indentLevel += 1;
								for (int i = 0; i < moveInfo.buttonSequence.Length; i ++){
									EditorGUILayout.Space();
									EditorGUILayout.BeginVertical(arrayElementStyle);{
										EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();{
											moveInfo.buttonSequence[i] = (ButtonPress)EditorGUILayout.EnumPopup("Button:", moveInfo.buttonSequence[i], enumStyle);
											if (GUILayout.Button("", "PaneOptions")){
												PaneOptions<ButtonPress>(moveInfo.buttonSequence, moveInfo.buttonSequence[i], delegate (ButtonPress[] newElement) { moveInfo.buttonSequence = newElement; });
											}
										}EditorGUILayout.EndHorizontal();
										EditorGUILayout.Space();
									}EditorGUILayout.EndVertical();
								}
								EditorGUILayout.Space();
								if (StyledButton("New Button Sequence"))
									moveInfo.buttonSequence = AddElement<ButtonPress>(moveInfo.buttonSequence, ButtonPress.Foward);
								
								EditorGUI.indentLevel -= 1;
							}EditorGUILayout.EndVertical();
						}
						
						// Button Execution
						buttonExecutionToggle = EditorGUILayout.Foldout(buttonExecutionToggle, "Button Executions ("+ moveInfo.buttonExecution.Length +")", EditorStyles.foldout);
						if (buttonExecutionToggle){
							EditorGUILayout.BeginVertical(subGroupStyle);{
								EditorGUI.indentLevel += 1;
								moveInfo.onPressExecution = EditorGUILayout.Toggle("On Button Press", moveInfo.onPressExecution, toggleStyle);
								moveInfo.onReleaseExecution = EditorGUILayout.Toggle("On Button Release", moveInfo.onReleaseExecution, toggleStyle);
								for (int i = 0; i < moveInfo.buttonExecution.Length; i ++){
									EditorGUILayout.Space();
									EditorGUILayout.BeginVertical(arrayElementStyle);{
										EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();{
											moveInfo.buttonExecution[i] = (ButtonPress)EditorGUILayout.EnumPopup("Button:", moveInfo.buttonExecution[i], enumStyle);
											if (GUILayout.Button("", "PaneOptions")){
												PaneOptions<ButtonPress>(moveInfo.buttonExecution, moveInfo.buttonExecution[i], delegate (ButtonPress[] newElement) { moveInfo.buttonExecution = newElement; });
											}
										}EditorGUILayout.EndHorizontal();
										EditorGUILayout.Space();
									}EditorGUILayout.EndHorizontal();
								}
								EditorGUILayout.Space();
								if (StyledButton("New Button Execution"))
									moveInfo.buttonExecution = AddElement<ButtonPress>(moveInfo.buttonExecution, ButtonPress.Button1);

								EditorGUI.indentLevel -= 1;
							}EditorGUILayout.EndVertical();
						}
						EditorGUI.indentLevel -= 1;
					}EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
			// End Input Options

#if !UFE_BASIC
			// Begin Move Link Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					moveLinksOptions = EditorGUILayout.Foldout(moveLinksOptions, "Chain Moves", EditorStyles.foldout);
					helpButton("move:chainmoves");
				}EditorGUILayout.EndHorizontal();

				if (moveLinksOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						
						// Previous Moves
						previousMovesToggle = EditorGUILayout.Foldout(previousMovesToggle, "Required Moves ("+ moveInfo.previousMoves.Length +")", EditorStyles.foldout);
						if (previousMovesToggle){
							EditorGUILayout.BeginVertical(subGroupStyle);{
								EditorGUI.indentLevel += 1;
								for (int i = 0; i < moveInfo.previousMoves.Length; i ++){
									EditorGUILayout.Space();
									EditorGUILayout.BeginVertical(arrayElementStyle);{
										EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();{
											moveInfo.previousMoves[i] = (MoveInfo) EditorGUILayout.ObjectField("Move:", moveInfo.previousMoves[i], typeof(MoveInfo), false);
											if (GUILayout.Button("", "PaneOptions")){
												PaneOptions<MoveInfo>(moveInfo.previousMoves, moveInfo.previousMoves[i], delegate (MoveInfo[] newElement) { moveInfo.previousMoves = newElement; });
											}
										}EditorGUILayout.EndHorizontal();
										EditorGUILayout.Space();
									}EditorGUILayout.EndHorizontal();
								}
								EditorGUILayout.Space();
								if (StyledButton("New Required Move"))
									moveInfo.previousMoves = AddElement<MoveInfo>(moveInfo.previousMoves, null);
								
								EditorGUILayout.Space();
								EditorGUI.indentLevel -= 1;

							}EditorGUILayout.EndVertical();
						}
						
						
						// Move Links
						nextMovesToggle = EditorGUILayout.Foldout(nextMovesToggle, "Move Links ("+ moveInfo.frameLinks.Length +")", EditorStyles.foldout);
						if (nextMovesToggle){
							EditorGUILayout.BeginVertical(subGroupStyle);{
								EditorGUI.indentLevel += 1;

								for (int i = 0; i < moveInfo.frameLinks.Length; i ++){
									
									EditorGUILayout.Space();
									EditorGUILayout.BeginVertical(arrayElementStyle);{
										EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();{
											StyledMinMaxSlider("Frame Links", ref moveInfo.frameLinks[i].activeFramesBegins, ref moveInfo.frameLinks[i].activeFramesEnds, 1, moveInfo.totalFrames, EditorGUI.indentLevel);
											if (GUILayout.Button("", "PaneOptions")){
												PaneOptions<FrameLink>(moveInfo.frameLinks, moveInfo.frameLinks[i], delegate (FrameLink[] newElement) { moveInfo.frameLinks = newElement; });
											}
										}EditorGUILayout.EndHorizontal();
										EditorGUILayout.Space();
										EditorGUILayout.Space();
										
										
										moveInfo.frameLinks[i].linkType = (LinkType) EditorGUILayout.EnumPopup("Link Conditions:", moveInfo.frameLinks[i].linkType, enumStyle);

										if (moveInfo.frameLinks[i].linkType == LinkType.HitConfirm){
											moveInfo.frameLinks[i].hitConfirmToggle = EditorGUILayout.Foldout(moveInfo.frameLinks[i].hitConfirmToggle, "Hit Confirm Options", EditorStyles.foldout);
											if (moveInfo.frameLinks[i].hitConfirmToggle){
												EditorGUI.indentLevel += 1;

												EditorGUILayout.BeginVertical(subGroupStyle);{
													EditorGUIUtility.labelWidth = 180;
													moveInfo.frameLinks[i].onStrike = EditorGUILayout.Toggle("On Strike", moveInfo.frameLinks[i].onStrike, toggleStyle);
													moveInfo.frameLinks[i].onBlock = EditorGUILayout.Toggle("On Block", moveInfo.frameLinks[i].onBlock, toggleStyle);
													moveInfo.frameLinks[i].onParry = EditorGUILayout.Toggle("On Parry", moveInfo.frameLinks[i].onParry, toggleStyle);
													EditorGUIUtility.labelWidth = 150;

												}EditorGUILayout.EndVertical();
												EditorGUI.indentLevel -= 1;
											}
										}else if (moveInfo.frameLinks[i].linkType == LinkType.CounterMove){
											moveInfo.frameLinks[i].counterMoveToggle = EditorGUILayout.Foldout(moveInfo.frameLinks[i].counterMoveToggle, "Counter Move Options", EditorStyles.foldout);
											if (moveInfo.frameLinks[i].counterMoveToggle){
												EditorGUI.indentLevel += 1;
												EditorGUIUtility.labelWidth = 180;
												EditorGUILayout.BeginVertical(subGroupStyle);{
													moveInfo.frameLinks[i].counterMoveType = (CounterMoveType) EditorGUILayout.EnumPopup("Filter Type:", moveInfo.frameLinks[i].counterMoveType, enumStyle);

													if (moveInfo.frameLinks[i].counterMoveType == CounterMoveType.MoveFilter){
														moveInfo.frameLinks[i].anyHitStrength = EditorGUILayout.Toggle("Any Hit Strength", moveInfo.frameLinks[i].anyHitStrength, toggleStyle);
														EditorGUI.BeginDisabledGroup(moveInfo.frameLinks[i].anyHitStrength);{
															moveInfo.frameLinks[i].hitStrength = (HitStrengh) EditorGUILayout.EnumPopup("Hit Strength:", moveInfo.frameLinks[i].hitStrength, enumStyle);
														}EditorGUI.EndDisabledGroup();
														
														moveInfo.frameLinks[i].anyStrokeHitBox = EditorGUILayout.Toggle("Any Stroke Hit Box", moveInfo.frameLinks[i].anyStrokeHitBox, toggleStyle);
														EditorGUI.BeginDisabledGroup(moveInfo.frameLinks[i].anyStrokeHitBox);{
															moveInfo.frameLinks[i].hitBoxType = (HitBoxType) EditorGUILayout.EnumPopup("Stroke Hit Box:", moveInfo.frameLinks[i].hitBoxType, enumStyle);
														}EditorGUI.EndDisabledGroup();
														
														moveInfo.frameLinks[i].anyHitType = EditorGUILayout.Toggle("Any Hit Type", moveInfo.frameLinks[i].anyHitType, toggleStyle);
														EditorGUI.BeginDisabledGroup(moveInfo.frameLinks[i].anyHitType);{
															moveInfo.frameLinks[i].hitType = (HitType) EditorGUILayout.EnumPopup("Hit Type:", moveInfo.frameLinks[i].hitType, enumStyle);
														}EditorGUI.EndDisabledGroup();
													}else{
														moveInfo.frameLinks[i].counterMoveFilter = (MoveInfo) EditorGUILayout.ObjectField("Move:", moveInfo.frameLinks[i].counterMoveFilter, typeof(MoveInfo), false);
													}

													moveInfo.frameLinks[i].disableHitImpact = EditorGUILayout.Toggle("Disable Hit Impact", moveInfo.frameLinks[i].disableHitImpact, toggleStyle);
												}EditorGUILayout.EndVertical();

												EditorGUIUtility.labelWidth = 150;
												EditorGUI.indentLevel -= 1;
											}
										}

										moveInfo.frameLinks[i].linkableMovesToggle = EditorGUILayout.Foldout(moveInfo.frameLinks[i].linkableMovesToggle, "Linked Moves ("+ moveInfo.frameLinks[i].linkableMoves.Length +")", EditorStyles.foldout);
										if (moveInfo.frameLinks[i].linkableMovesToggle){
											EditorGUI.indentLevel += 1;
											EditorGUILayout.BeginVertical(subGroupStyle);{
												EditorGUIUtility.labelWidth = 250;
												moveInfo.frameLinks[i].ignoreInputs = EditorGUILayout.Toggle("Ignore Inputs (Auto Execution)", moveInfo.frameLinks[i].ignoreInputs, toggleStyle);
												moveInfo.frameLinks[i].ignorePlayerConditions = EditorGUILayout.Toggle("Ignore Player Conditions", moveInfo.frameLinks[i].ignorePlayerConditions, toggleStyle);
												
												EditorGUI.BeginDisabledGroup(moveInfo.frameLinks[i].ignoreInputs);{
													moveInfo.frameLinks[i].allowBuffer = EditorGUILayout.Toggle("Allow Buffer", moveInfo.frameLinks[i].allowBuffer, toggleStyle);
												}EditorGUI.EndDisabledGroup();
												
												EditorGUIUtility.labelWidth = 150;
												moveInfo.frameLinks[i].nextMoveStartupFrame = Mathf.Max(1,EditorGUILayout.IntField("Startup Frame:", moveInfo.frameLinks[i].nextMoveStartupFrame));


												for (int k = 0; k < moveInfo.frameLinks[i].linkableMoves.Length; k ++){
													EditorGUILayout.Space();
													EditorGUILayout.BeginVertical(subArrayElementStyle);{
														EditorGUILayout.Space();
														EditorGUILayout.BeginHorizontal();{
															moveInfo.frameLinks[i].linkableMoves[k] = (MoveInfo) EditorGUILayout.ObjectField("Move:", moveInfo.frameLinks[i].linkableMoves[k], typeof(MoveInfo), false);
															if (GUILayout.Button("", "PaneOptions")){
																PaneOptions<MoveInfo>(moveInfo.frameLinks[i].linkableMoves, moveInfo.frameLinks[i].linkableMoves[k], delegate (MoveInfo[] newElement) { moveInfo.frameLinks[i].linkableMoves = newElement; });
															}
														}EditorGUILayout.EndHorizontal();
														EditorGUILayout.Space();
													}EditorGUILayout.EndVertical();
												}
												EditorGUILayout.Space();

												if (StyledButton("New Move"))
													moveInfo.frameLinks[i].linkableMoves = AddElement<MoveInfo>(moveInfo.frameLinks[i].linkableMoves, null);
												
												EditorGUILayout.Space();

											}EditorGUILayout.EndVertical();

											EditorGUI.indentLevel -= 1;
											EditorGUILayout.Space();
										}

									}EditorGUILayout.EndVertical();
								}
								EditorGUILayout.Space();

								if (StyledButton("New Link"))
									moveInfo.frameLinks = AddElement<FrameLink>(moveInfo.frameLinks, null);
								
								EditorGUILayout.Space();
								EditorGUI.indentLevel -= 1;

							}EditorGUILayout.EndVertical();
						}
						EditorGUI.indentLevel -= 1;

					}EditorGUILayout.EndVertical();
				}
				//GUILayout.Box("", "TimeScrubber", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
				// End Move Link Options
				
				
			}EditorGUILayout.EndVertical();
			// End Move Link Options


			// Begin Camera Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					cameraOptions = EditorGUILayout.Foldout(cameraOptions, "Cinematic Options ("+ moveInfo.cameraMovements.Length +")", EditorStyles.foldout);
					helpButton("move:cinematics");
				}EditorGUILayout.EndHorizontal();
				
				if (cameraOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(CameraMovement cameraMovement in moveInfo.cameraMovements) 
							castingValues.Add(cameraMovement.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);
						
						for (int i = 0; i < moveInfo.cameraMovements.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								SubGroupTitle("Casting Options");
								EditorGUILayout.Space();

								EditorGUILayout.BeginHorizontal();{
									moveInfo.cameraMovements[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.cameraMovements[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<CameraMovement>(moveInfo.cameraMovements, moveInfo.cameraMovements[i], delegate (CameraMovement[] newElement) { moveInfo.cameraMovements = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								
								Camera sceneCam = GameObject.FindObjectOfType<Camera>();

								CinematicType typeTemp = moveInfo.cameraMovements[i].cinematicType;
								moveInfo.cameraMovements[i].cinematicType = (CinematicType) EditorGUILayout.EnumPopup("Cinematic Type:", moveInfo.cameraMovements[i].cinematicType, enumStyle);
								if (typeTemp != moveInfo.cameraMovements[i].cinematicType) Clear(true, true);

								EditorGUIUtility.labelWidth = 215;
								moveInfo.cameraMovements[i].myAnimationSpeed = EditorGUILayout.Slider("Character Animation Speed (%):", moveInfo.cameraMovements[i].myAnimationSpeed, 0, 100);
								moveInfo.cameraMovements[i].opAnimationSpeed = EditorGUILayout.Slider("Opponent Animation Speed (%):", moveInfo.cameraMovements[i].opAnimationSpeed, 0, 100);
								EditorGUIUtility.labelWidth = 150;
								moveInfo.cameraMovements[i].freezePhysics = EditorGUILayout.Toggle("Freeze Physics", moveInfo.cameraMovements[i].freezePhysics, toggleStyle);

								if (moveInfo.cameraMovements[i].cinematicType == CinematicType.CameraEditor){
									moveInfo.cameraMovements[i].duration = Mathf.Clamp(EditorGUILayout.FloatField("Duration (seconds):", moveInfo.cameraMovements[i].duration), .1f, 99999);
									
									EditorGUILayout.Space();
									SubGroupTitle("Camera Path");
									EditorGUILayout.Space();
									
									moveInfo.cameraMovements[i].camSpeed = EditorGUILayout.Slider("Movement Speed:", moveInfo.cameraMovements[i].camSpeed, 1, 100);

									if (globalInfo){
										initialFieldOfView = globalInfo.cameraOptions.initialFieldOfView;
										initialCamPosition = globalInfo.cameraOptions.initialDistance;
										initialCamRotation = Quaternion.Euler(globalInfo.cameraOptions.initialRotation);
									}
									initialCamPosition = EditorGUILayout.Vector3Field("Initial Position:", initialCamPosition);
									initialCamRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Initial Rotation:", initialCamRotation.eulerAngles));
									initialFieldOfView = EditorGUILayout.Slider("Initial Field of View:", initialFieldOfView, 1, 179);
									
									EditorGUIUtility.labelWidth = 130;
									EditorGUILayout.BeginHorizontal();{
										globalInfo = (GlobalInfo) EditorGUILayout.ObjectField("Copy from a file:", globalInfo, typeof(GlobalInfo), false, GUILayout.Width(260));
										EditorGUI.BeginDisabledGroup(globalInfo == null);{
											if (StyledButton("Clear")) globalInfo = null;
										}EditorGUI.EndDisabledGroup();
									}EditorGUILayout.EndHorizontal();
									EditorGUIUtility.labelWidth = 150;
									
									EditorGUILayout.Space();
									
									GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

									moveInfo.cameraMovements[i].position = EditorGUILayout.Vector3Field("Final Position:", moveInfo.cameraMovements[i].position);
									moveInfo.cameraMovements[i].rotation = EditorGUILayout.Vector3Field("Final Rotation:", moveInfo.cameraMovements[i].rotation);
									moveInfo.cameraMovements[i].fieldOfView = EditorGUILayout.Slider("Final Field of View:", moveInfo.cameraMovements[i].fieldOfView, 1, 179);
									
									if (StyledButton("Snap Current Camera Info")){
										moveInfo.cameraMovements[i].fieldOfView = sceneCam.fieldOfView;
										if (emulatedPlayer != null){
											moveInfo.cameraMovements[i].position = emulatedPlayer.transform.InverseTransformPoint(sceneCam.transform.position);
										}else{
											moveInfo.cameraMovements[i].position = sceneCam.transform.position;
										}
										moveInfo.cameraMovements[i].rotation = sceneCam.transform.localRotation.eulerAngles;
									}
									//if (character != null && StyledButton("Apply external camera reposition"))
									//	moveInfo.cameraMovements[i].position = character.transform.TransformPoint(initialCamPosition);
								}else if (moveInfo.cameraMovements[i].cinematicType == CinematicType.Prefab){
									moveInfo.cameraMovements[i].prefab = (GameObject) EditorGUILayout.ObjectField("Prefab:", moveInfo.cameraMovements[i].prefab, typeof(UnityEngine.GameObject), true);
									moveInfo.cameraMovements[i].duration = Mathf.Clamp(EditorGUILayout.FloatField("Duration (seconds):", moveInfo.cameraMovements[i].duration), .1f, 99999);
								}else if (moveInfo.cameraMovements[i].cinematicType == CinematicType.AnimationFile){
									moveInfo.cameraMovements[i].animationClip = (AnimationClip) EditorGUILayout.ObjectField("Animation Clip:", moveInfo.cameraMovements[i].animationClip, typeof(UnityEngine.AnimationClip), true);
									moveInfo.cameraMovements[i].camAnimationSpeed = Mathf.Clamp(EditorGUILayout.FloatField("Animation Speed:", moveInfo.cameraMovements[i].camAnimationSpeed), .1f, 99999);
									
									moveInfo.cameraMovements[i].gameObjectPosition = EditorGUILayout.Vector3Field("Parent Game Object Position:", moveInfo.cameraMovements[i].gameObjectPosition);
									moveInfo.cameraMovements[i].position = EditorGUILayout.Vector3Field("Child Camera Position (relative):", moveInfo.cameraMovements[i].position);
									moveInfo.cameraMovements[i].rotation = EditorGUILayout.Vector3Field("Child Camera Rotation:", moveInfo.cameraMovements[i].rotation);
									moveInfo.cameraMovements[i].fieldOfView = EditorGUILayout.Slider("Field of View:", moveInfo.cameraMovements[i].fieldOfView, 1, 179);
									moveInfo.cameraMovements[i].blendSpeed = EditorGUILayout.Slider("Blend Speed:", moveInfo.cameraMovements[i].blendSpeed, 1, 100);
								}

								EditorGUILayout.Space();
								if (characterPrefab == null && !moveInfo.cameraMovements[i].previewToggle && moveInfo.cameraMovements[i].cinematicType != CinematicType.Prefab){
									if (StyledButton("Camera Preview")){
										bool otherPreviewsOpen = false;
										foreach(CameraMovement camMove in moveInfo.cameraMovements) 
											if (camMove.previewToggle) otherPreviewsOpen = true;
										
										
										if (moveInfo.characterPrefab == null) {
											characterWarning = true;
											errorMsg = "You must have a character assigned under Animation -> Character Prefab.";
										}else if (EditorApplication.isPlayingOrWillChangePlaymode){
											characterWarning = true;
											errorMsg = "You can't preview animations while in play mode.";
										}else if (moveInfo.cameraMovements[i].cinematicType == CinematicType.AnimationFile &&
										          moveInfo.cameraMovements[i].animationClip == null){
											characterWarning = true;
											errorMsg = "No animation assigned.";
										}else if (otherPreviewsOpen){
											characterWarning = true;
											errorMsg = "You can open only one camera preview at the time.";
										}else{
											characterWarning = false;
											
											System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
											System.Type type = assembly.GetType("UnityEditor.GameView");
											EditorWindow gameview = EditorWindow.GetWindow(type);
											gameview.Focus();
											//EditorCamera.SetPosition(Vector3.up * 4);
											//EditorCamera.SetRotation(Quaternion.identity);
											//EditorCamera.SetOrthographic(true);
											//EditorCamera.SetSize(8);


											characterPrefab = (GameObject) PrefabUtility.InstantiatePrefab(moveInfo.characterPrefab);
											emulatedPlayer = new GameObject();
											emulatedPlayer.name = "Emulated Player";
											emulatedPlayer.transform.position = new Vector3(-2,0,0);
											characterPrefab.transform.parent = emulatedPlayer.transform;
											camTime = 0;
											moveInfo.cameraMovements[i].previewToggle = true;
											
											if (moveInfo.cameraMovements[i].cinematicType == CinematicType.AnimationFile){
												emulatedCamera = new GameObject();
												emulatedCamera.name = "Emulated Camera Parent";
												emulatedCamera.transform.position = emulatedPlayer.transform.TransformPoint(moveInfo.cameraMovements[i].gameObjectPosition);

												sceneCam.transform.parent = emulatedCamera.transform;
												sceneCam.transform.localPosition = moveInfo.cameraMovements[i].position;
												sceneCam.transform.rotation = Quaternion.Euler(moveInfo.cameraMovements[i].rotation);
											}
										}
									}
									
									if (characterWarning){
										GUILayout.BeginHorizontal("GroupBox");
										GUILayout.Label(errorMsg,"CN EntryWarn");
										GUILayout.EndHorizontal();
									}
								}else if (moveInfo.cameraMovements[i].previewToggle){
									EditorGUILayout.BeginVertical(subGroupStyle);{
										EditorGUILayout.Space();
										
										EditorGUI.indentLevel += 1;
										
										if (animFrame == 0) animFrame = moveInfo.cameraMovements[i].castingFrame;

										if (moveInfo.cameraMovements[i].cinematicType == CinematicType.CameraEditor){
											camTime = StyledSlider("Timeline", camTime, EditorGUI.indentLevel, 0, moveInfo.cameraMovements[i].duration);
											Vector3 targetPos = emulatedPlayer.transform.TransformPoint(moveInfo.cameraMovements[i].position);
											Quaternion targetRot = Quaternion.Euler(moveInfo.cameraMovements[i].rotation);
											float targetFoV = moveInfo.cameraMovements[i].fieldOfView;
											
											float camTimeSpeedModifier = camTime * moveInfo.cameraMovements[i].camSpeed;
											float journey = camTimeSpeedModifier/moveInfo.cameraMovements[i].duration;
											if (journey > 1) journey = 1;
											
											if (sceneCam != null && prevCamTime != camTime){
												sceneCam.transform.position = Vector3.Lerp(initialCamPosition, targetPos, journey);
												sceneCam.transform.localRotation = Quaternion.Slerp(initialCamRotation, targetRot, journey);
												sceneCam.fieldOfView = Mathf.Lerp(initialFieldOfView, targetFoV, journey);
											}
										}else if (moveInfo.cameraMovements[i].cinematicType == CinematicType.AnimationFile){
											if (emulatedCamera == null) {
												Clear(true, true);
												return;
											}
											sceneCam.fieldOfView = moveInfo.cameraMovements[i].fieldOfView;
											camTime = StyledSlider("Timeline", camTime, EditorGUI.indentLevel, 0, moveInfo.cameraMovements[i].animationClip.length);
											AnimationSampler(emulatedCamera, moveInfo.cameraMovements[i].animationClip, moveInfo.cameraMovements[i].castingFrame, moveInfo.cameraMovements[i].camAnimationSpeed);
										}
										prevCamTime = camTime;

										/*if (moveInfo.cameraMovements[i].freezeAnimation) {
											animFrame = moveInfo.cameraMovements[i].castingFrame;
										}else{
											animFrame = moveInfo.cameraMovements[i].castingFrame + Mathf.Floor(camTime * moveInfo.fps);
										}*/
										animFrame = moveInfo.cameraMovements[i].castingFrame + (Mathf.Floor(camTime * moveInfo.fps) * 
										                                                        (moveInfo.cameraMovements[i].myAnimationSpeed/100));
										AnimationSampler(characterPrefab, moveInfo.animationClip, 0, true, true, moveInfo.forceMirrorLeft, moveInfo.invertRotationLeft);


										
										if (StyledButton("Close Preview")) {
											Clear (true, true);
											EditorWindow.FocusWindowIfItsOpen<SceneView>();
										}
										
										EditorGUI.indentLevel -= 1;
										EditorGUILayout.Space();
										
									}EditorGUILayout.EndVertical();
								}
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
						if (StyledButton("New Cinematic"))
							moveInfo.cameraMovements = AddElement<CameraMovement>(moveInfo.cameraMovements, new CameraMovement());
						
						EditorGUI.indentLevel -= 1;
					}EditorGUILayout.EndVertical();
					
				}else if (!animationOptions && characterPrefab != null){
					Clear (true, true);
				}
				
			}EditorGUILayout.EndVertical();
			// End Camera Options
#endif


			// Begin Particle Effects Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					particleEffectsOptions = EditorGUILayout.Foldout(particleEffectsOptions, "Particle Effects ("+ moveInfo.particleEffects.Length +")", EditorStyles.foldout);
					helpButton("move:particleeffects");
				}EditorGUILayout.EndHorizontal();

				if (particleEffectsOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(MoveParticleEffect particleEffect in moveInfo.particleEffects) 
							castingValues.Add(particleEffect.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);
							
						for (int i = 0; i < moveInfo.particleEffects.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									moveInfo.particleEffects[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.particleEffects[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<MoveParticleEffect>(moveInfo.particleEffects, moveInfo.particleEffects[i], delegate (MoveParticleEffect[] newElement) { moveInfo.particleEffects = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								if (moveInfo.particleEffects[i].particleEffect == null) moveInfo.particleEffects[i].particleEffect = new ParticleInfo();
								moveInfo.particleEffects[i].particleEffect.prefab = (GameObject) EditorGUILayout.ObjectField("Particle Effect:", moveInfo.particleEffects[i].particleEffect.prefab, typeof(UnityEngine.GameObject), true);
								moveInfo.particleEffects[i].particleEffect.duration = EditorGUILayout.FloatField("Duration (seconds):", moveInfo.particleEffects[i].particleEffect.duration);
								moveInfo.particleEffects[i].particleEffect.stick = EditorGUILayout.Toggle("Sticky", moveInfo.particleEffects[i].particleEffect.stick, toggleStyle);
								moveInfo.particleEffects[i].particleEffect.bodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part:", moveInfo.particleEffects[i].particleEffect.bodyPart, enumStyle);
								moveInfo.particleEffects[i].particleEffect.offSet = EditorGUILayout.Vector3Field("Off Set (relative):", moveInfo.particleEffects[i].particleEffect.offSet);
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
						if (StyledButton("New Particle Effect"))
							moveInfo.particleEffects = AddElement<MoveParticleEffect>(moveInfo.particleEffects, new MoveParticleEffect());
							
						EditorGUI.indentLevel -= 1;

					}EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
			// End Particle Effects Options


			// Begin Sound Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					soundOptions = EditorGUILayout.Foldout(soundOptions, "Sound Effects ("+ moveInfo.soundEffects.Length +")", EditorStyles.foldout);
					helpButton("move:soundeffects");
				}EditorGUILayout.EndHorizontal();

				if (soundOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(SoundEffect soundEffect in moveInfo.soundEffects) 
							castingValues.Add(soundEffect.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);
							
						for (int i = 0; i < moveInfo.soundEffects.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									moveInfo.soundEffects[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.soundEffects[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<SoundEffect>(moveInfo.soundEffects, moveInfo.soundEffects[i], delegate (SoundEffect[] newElement) { moveInfo.soundEffects = newElement; });
									}
								}EditorGUILayout.EndHorizontal();

								moveInfo.soundEffects[i].soundEffectsToggle = EditorGUILayout.Foldout(moveInfo.soundEffects[i].soundEffectsToggle, "Possible Sound Effects ("+ moveInfo.soundEffects[i].sounds.Length +")", EditorStyles.foldout);
								if (moveInfo.soundEffects[i].soundEffectsToggle){
									EditorGUILayout.BeginVertical(subGroupStyle);{
										EditorGUIUtility.labelWidth = 150;
										for (int k = 0; k < moveInfo.soundEffects[i].sounds.Length; k ++){
											EditorGUILayout.Space();
											EditorGUILayout.BeginVertical(subArrayElementStyle);{
												EditorGUILayout.Space();
												EditorGUILayout.BeginHorizontal();{
													moveInfo.soundEffects[i].sounds[k] = (AudioClip) EditorGUILayout.ObjectField("Audio Clip:", moveInfo.soundEffects[i].sounds[k], typeof(UnityEngine.AudioClip), true);
													if (GUILayout.Button("", "PaneOptions")){
														PaneOptions<AudioClip>(moveInfo.soundEffects[i].sounds, moveInfo.soundEffects[i].sounds[k], delegate (AudioClip[] newElement) { moveInfo.soundEffects[i].sounds = newElement; });
													}
												}EditorGUILayout.EndHorizontal();
												EditorGUILayout.Space();
											}EditorGUILayout.EndVertical();
										}
										if (StyledButton("New Sound Effect"))
											moveInfo.soundEffects[i].sounds = AddElement<AudioClip>(moveInfo.soundEffects[i].sounds, new AudioClip());
										
									}EditorGUILayout.EndVertical();
								}

								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
						if (StyledButton("New Sound Effect"))
							moveInfo.soundEffects = AddElement<SoundEffect>(moveInfo.soundEffects, new SoundEffect());

						EditorGUI.indentLevel -= 1;
						
					}EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
			// End Sound Options


			// Begin In Game Alert Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					textWarningOptions = EditorGUILayout.Foldout(textWarningOptions, "Text Alerts ("+ moveInfo.inGameAlert.Length +")", foldStyle);
					helpButton("global:textalerts");
				}EditorGUILayout.EndHorizontal();
				
				if (textWarningOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(InGameAlert inGameAlert in moveInfo.inGameAlert) 
							castingValues.Add(inGameAlert.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);
						
						for (int i = 0; i < moveInfo.inGameAlert.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									moveInfo.inGameAlert[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.inGameAlert[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<InGameAlert>(moveInfo.inGameAlert, moveInfo.inGameAlert[i], delegate (InGameAlert[] newElement) { moveInfo.inGameAlert = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								moveInfo.inGameAlert[i].alert = EditorGUILayout.TextField("Message:", moveInfo.inGameAlert[i].alert);
								EditorGUILayout.Space();

							}EditorGUILayout.EndVertical();
							EditorGUILayout.Space();
						}
						
						if (StyledButton("New Alert"))
							moveInfo.inGameAlert = AddElement<InGameAlert>(moveInfo.inGameAlert, new InGameAlert());
						
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();
			// End In Game Alert Options

			
			// Begin Stance Changes
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					stanceOptions = EditorGUILayout.Foldout(stanceOptions, "Stance Changes ("+ moveInfo.stanceChanges.Length +")", foldStyle);
					helpButton("global:stancechanges");
				}EditorGUILayout.EndHorizontal();
				
				if (stanceOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(StanceChange stanceChange in moveInfo.stanceChanges) 
							castingValues.Add(stanceChange.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);
						
						for (int i = 0; i < moveInfo.stanceChanges.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									moveInfo.stanceChanges[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.stanceChanges[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<StanceChange>(moveInfo.stanceChanges, moveInfo.stanceChanges[i], delegate (StanceChange[] newElement) { moveInfo.stanceChanges = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								moveInfo.stanceChanges[i].newStance = (CombatStances)EditorGUILayout.EnumPopup("New Stance:", moveInfo.stanceChanges[i].newStance, enumStyle);
								EditorGUILayout.Space();
								
							}EditorGUILayout.EndVertical();
							EditorGUILayout.Space();
						}
						
						if (StyledButton("New Stance Change"))
							moveInfo.stanceChanges = AddElement<StanceChange>(moveInfo.stanceChanges, new StanceChange());
						
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();
			// End Stance Changes


			// Begin Self Applied Force Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					selfAppliedForceOptions = EditorGUILayout.Foldout(selfAppliedForceOptions, "Self Applied Forces ("+ moveInfo.appliedForces.Length +")", EditorStyles.foldout);
					helpButton("move:selfappliedforce");
				}EditorGUILayout.EndHorizontal();

				if (selfAppliedForceOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(AppliedForce appliedForce in moveInfo.appliedForces) 
							castingValues.Add(appliedForce.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);
							
						for (int i = 0; i < moveInfo.appliedForces.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									moveInfo.appliedForces[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.appliedForces[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<AppliedForce>(moveInfo.appliedForces, moveInfo.appliedForces[i], delegate (AppliedForce[] newElement) { moveInfo.appliedForces = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								moveInfo.appliedForces[i].resetPreviousHorizontal = EditorGUILayout.Toggle("Reset X Force", moveInfo.appliedForces[i].resetPreviousHorizontal, toggleStyle);
								moveInfo.appliedForces[i].resetPreviousVertical = EditorGUILayout.Toggle("Reset Y Force", moveInfo.appliedForces[i].resetPreviousVertical, toggleStyle);
								moveInfo.appliedForces[i].force = EditorGUILayout.Vector2Field("Force Applied:", moveInfo.appliedForces[i].force);
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
						if (StyledButton("New Applied Force"))
							moveInfo.appliedForces = AddElement<AppliedForce>(moveInfo.appliedForces, new AppliedForce());

						EditorGUI.indentLevel -= 1;
						
					}EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
			// End Self Applied Force Options


			// Begin Slow Mo Effect Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					slowMoOptions = EditorGUILayout.Foldout(slowMoOptions, "Slow Motion Effects ("+ moveInfo.slowMoEffects.Length +")", EditorStyles.foldout);
					helpButton("move:slowmoeffects");
				}EditorGUILayout.EndHorizontal();
				
				if (slowMoOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(SlowMoEffect slowMoEffect in moveInfo.slowMoEffects) 
							castingValues.Add(slowMoEffect.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);
						
						for (int i = 0; i < moveInfo.slowMoEffects.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									moveInfo.slowMoEffects[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.slowMoEffects[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<SlowMoEffect>(moveInfo.slowMoEffects, moveInfo.slowMoEffects[i], delegate (SlowMoEffect[] newElement) { moveInfo.slowMoEffects = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								moveInfo.slowMoEffects[i].duration = EditorGUILayout.FloatField("Duration (Seconds):", moveInfo.slowMoEffects[i].duration);
								moveInfo.slowMoEffects[i].percentage = EditorGUILayout.Slider("Speed (%):", moveInfo.slowMoEffects[i].percentage, 0, 100);
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
						if (StyledButton("New Slow Motion Effect"))
							moveInfo.slowMoEffects = AddElement<SlowMoEffect>(moveInfo.slowMoEffects, new SlowMoEffect());
						
						EditorGUI.indentLevel -= 1;
						
					}EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
			// End Slow Mo Effect Options


			// Begin Armor Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					armorOptions = EditorGUILayout.Foldout(armorOptions, "Armor Options", EditorStyles.foldout);
					helpButton("move:armor");
				}EditorGUILayout.EndHorizontal();
				
				if (armorOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 2;
						StyledMinMaxSlider("Armor Frames", ref moveInfo.armorOptions.activeFramesBegin, ref moveInfo.armorOptions.activeFramesEnds, 1, moveInfo.totalFrames, EditorGUI.indentLevel);
						
						EditorGUI.indentLevel -= 1;
						EditorGUIUtility.labelWidth = 200;
						moveInfo.armorOptions.hitAbsorption = Mathf.Max(0,EditorGUILayout.IntField("Hit Absorption:", moveInfo.armorOptions.hitAbsorption));

						EditorGUI.BeginDisabledGroup(moveInfo.armorOptions.hitAbsorption == 0);{
						moveInfo.armorOptions.damageAbsorption = EditorGUILayout.IntSlider("Damage Absorption (%):", moveInfo.armorOptions.damageAbsorption, 0, 100);

						moveInfo.armorOptions.overrideHitEffects = EditorGUILayout.Toggle("Override Hit Effects", moveInfo.armorOptions.overrideHitEffects, toggleStyle);
						if (moveInfo.armorOptions.overrideHitEffects) HitOptionBlock("Hit Effects", moveInfo.armorOptions.hitEffects);

						bodyPartsToggle = EditorGUILayout.Foldout(bodyPartsToggle, "Non Affected Body Parts ("+ moveInfo.armorOptions.nonAffectedBodyParts.Length +")", EditorStyles.foldout);
						if (bodyPartsToggle){
							EditorGUILayout.BeginVertical(subGroupStyle);{
								EditorGUILayout.Space();
								EditorGUI.indentLevel += 1;
								if (moveInfo.armorOptions.nonAffectedBodyParts != null){
									for (int y = 0; y < moveInfo.armorOptions.nonAffectedBodyParts.Length; y ++){
										EditorGUILayout.Space();
										EditorGUILayout.BeginVertical(subArrayElementStyle);{
											EditorGUILayout.BeginHorizontal();{
												moveInfo.armorOptions.nonAffectedBodyParts[y] = (BodyPart)EditorGUILayout.EnumPopup("Body Part:", moveInfo.armorOptions.nonAffectedBodyParts[y], enumStyle);
												if (GUILayout.Button("", "PaneOptions")){
													PaneOptions<BodyPart>(moveInfo.armorOptions.nonAffectedBodyParts, moveInfo.armorOptions.nonAffectedBodyParts[y], delegate (BodyPart[] newElement) { moveInfo.armorOptions.nonAffectedBodyParts = newElement; });
												}
											}EditorGUILayout.EndHorizontal();
											EditorGUILayout.Space();
										}EditorGUILayout.EndVertical();
									}
								}
								if (StyledButton("New Body Part"))
									moveInfo.armorOptions.nonAffectedBodyParts = AddElement<BodyPart>(moveInfo.armorOptions.nonAffectedBodyParts, BodyPart.none);
								
								EditorGUI.indentLevel -= 1;
							}EditorGUILayout.EndVertical();
						}
						EditorGUIUtility.labelWidth = 150;
						EditorGUILayout.Space();
						}EditorGUI.EndDisabledGroup();

						EditorGUI.indentLevel -= 1;
					}EditorGUILayout.EndVertical();
				}

			}EditorGUILayout.EndVertical();
			// End Armor Options


			// Begin Invincible Frames Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					invincibleFramesOptions = EditorGUILayout.Foldout(invincibleFramesOptions, "Invincible Frames ("+ moveInfo.invincibleBodyParts.Length +")", EditorStyles.foldout);
					helpButton("move:invincibleframes");
				}EditorGUILayout.EndHorizontal();

				if (invincibleFramesOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<Vector3> castingValues = new List<Vector3>();
						foreach(InvincibleBodyParts invBodyPart in moveInfo.invincibleBodyParts) 
							castingValues.Add(new Vector3(invBodyPart.activeFramesBegin, invBodyPart.activeFramesEnds));
						StyledMarker("Invincible Frames Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel, false);

						EditorGUI.indentLevel += 1;
						for (int i = 0; i < moveInfo.invincibleBodyParts.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									StyledMinMaxSlider("Invincible Frames", ref moveInfo.invincibleBodyParts[i].activeFramesBegin, ref moveInfo.invincibleBodyParts[i].activeFramesEnds, 1, moveInfo.totalFrames, EditorGUI.indentLevel);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<InvincibleBodyParts>(moveInfo.invincibleBodyParts, moveInfo.invincibleBodyParts[i], delegate (InvincibleBodyParts[] newElement) { moveInfo.invincibleBodyParts = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								
								EditorGUILayout.Space();
								
								EditorGUIUtility.labelWidth = 240;
								moveInfo.invincibleBodyParts[i].completelyInvincible = EditorGUILayout.Toggle("Completely Invincible", moveInfo.invincibleBodyParts[i].completelyInvincible, toggleStyle);
								moveInfo.invincibleBodyParts[i].ignoreBodyColliders = EditorGUILayout.Toggle("Ignore Body Colliders", moveInfo.invincibleBodyParts[i].ignoreBodyColliders, toggleStyle);
								EditorGUIUtility.labelWidth = 150;

								if (!moveInfo.invincibleBodyParts[i].completelyInvincible){
									bodyPartsToggle = EditorGUILayout.Foldout(bodyPartsToggle, "Body Parts ("+ moveInfo.invincibleBodyParts[i].bodyParts.Length +")", EditorStyles.foldout);
									if (bodyPartsToggle){
										EditorGUILayout.Space();
										EditorGUILayout.BeginVertical(subGroupStyle);{
											EditorGUI.indentLevel += 1;
											if (moveInfo.invincibleBodyParts[i].bodyParts != null){
												for (int y = 0; y < moveInfo.invincibleBodyParts[i].bodyParts.Length; y ++){
													EditorGUILayout.Space();
													EditorGUILayout.BeginVertical(subArrayElementStyle);{
														EditorGUILayout.BeginHorizontal();{
															moveInfo.invincibleBodyParts[i].bodyParts[y] = (BodyPart)EditorGUILayout.EnumPopup("Body Part:", moveInfo.invincibleBodyParts[i].bodyParts[y], enumStyle);
															if (GUILayout.Button("", "PaneOptions")){
																PaneOptions<BodyPart>(moveInfo.invincibleBodyParts[i].bodyParts, moveInfo.invincibleBodyParts[i].bodyParts[y], delegate (BodyPart[] newElement) { moveInfo.invincibleBodyParts[i].bodyParts = newElement; });
															}
														}EditorGUILayout.EndHorizontal();
														EditorGUILayout.Space();
													}EditorGUILayout.EndVertical();
												}
											}

											if (StyledButton("New Body Part"))
												moveInfo.invincibleBodyParts[i].bodyParts = AddElement<BodyPart>(moveInfo.invincibleBodyParts[i].bodyParts, BodyPart.none);

											EditorGUI.indentLevel -= 1;
										}EditorGUILayout.EndHorizontal();
									}
								}
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
						if (StyledButton("New Invincible Frame Group"))
							moveInfo.invincibleBodyParts = AddElement<InvincibleBodyParts>(moveInfo.invincibleBodyParts, new InvincibleBodyParts());

						EditorGUI.indentLevel -= 2;
						
					}EditorGUILayout.EndVertical();
				}

			}EditorGUILayout.EndVertical();
			// End Invincible Frames Options

			
			// Begin Projectile Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					projectileOptions = EditorGUILayout.Foldout(projectileOptions, "Projectiles ("+ moveInfo.projectiles.Length +")", EditorStyles.foldout);
					helpButton("move:projectiles");
				}EditorGUILayout.EndHorizontal();

				if (projectileOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(Projectile projectile in moveInfo.projectiles) 
							castingValues.Add(projectile.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);
							
						for (int i = 0; i < moveInfo.projectiles.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									moveInfo.projectiles[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.projectiles[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<Projectile>(moveInfo.projectiles, moveInfo.projectiles[i], delegate (Projectile[] newElement) { moveInfo.projectiles = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								EditorGUILayout.Space();
								SubGroupTitle("Prefabs");
								EditorGUILayout.Space();
								moveInfo.projectiles[i].projectilePrefab = (GameObject) EditorGUILayout.ObjectField("Projectile Prefab:", moveInfo.projectiles[i].projectilePrefab, typeof(UnityEngine.GameObject), true);
								moveInfo.projectiles[i].impactPrefab = (GameObject) EditorGUILayout.ObjectField("Impact Prefab:", moveInfo.projectiles[i].impactPrefab, typeof(UnityEngine.GameObject), true);

								if (moveInfo.projectiles[i].impactPrefab != null){
									EditorGUIUtility.labelWidth = 190;
									moveInfo.projectiles[i].impactDuration = EditorGUILayout.FloatField("Impact Duration (seconds):", moveInfo.projectiles[i].impactDuration);
									EditorGUIUtility.labelWidth = 150;
								}

								if (projectilePrefab == null){
									if (StyledButton("Preview")){
										if (moveInfo.projectiles[i].projectilePrefab == null) {
											characterWarning = true;
											errorMsg = "'Projectile Prefab' not found.";
										}else if (EditorApplication.isPlayingOrWillChangePlaymode){
											characterWarning = true;
											errorMsg = "You can't preview while in play mode.";
										}else{
											characterWarning = false;
											EditorCamera.SetPosition(Vector3.up * 4);
											EditorCamera.SetRotation(Quaternion.identity);
											EditorCamera.SetOrthographic(true);
											EditorCamera.SetSize(10);
											
											foreach(Projectile projectile in moveInfo.projectiles)
												projectile.preview = false;
											
											moveInfo.projectiles[i].preview = true;
											
											projectilePrefab = (GameObject) PrefabUtility.InstantiatePrefab(moveInfo.projectiles[i].projectilePrefab);
											ProjectileSampler(projectilePrefab, moveInfo.projectiles[i]);
										}
									}
								}else{
									EditorGUILayout.Space();
									EditorGUILayout.Space();
									EditorGUILayout.BeginHorizontal();{
										if (StyledButton("Reset Scene View")){
											EditorCamera.SetPosition(Vector3.up * 4);
											EditorCamera.SetRotation(Quaternion.identity);
											EditorCamera.SetOrthographic(true);
											EditorCamera.SetSize(10);
										}
										if (StyledButton("Close Preview")) {
											Editor.DestroyImmediate(projectilePrefab);
											projectilePrefab = null;
										}
									}EditorGUILayout.EndHorizontal();
									EditorGUILayout.Space();
								}

								EditorGUI.BeginDisabledGroup(moveInfo.projectiles[i].projectilePrefab == null);{
									EditorGUILayout.Space();
									SubGroupTitle("Casting Options");
									EditorGUILayout.Space();
									EditorGUIUtility.labelWidth = 180;
									moveInfo.projectiles[i].bodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part Origin:", moveInfo.projectiles[i].bodyPart, enumStyle);
									moveInfo.projectiles[i].fixedZAxis = EditorGUILayout.Toggle("Ignore z axis variation", moveInfo.projectiles[i].fixedZAxis);
									EditorGUIUtility.labelWidth = 150;
									moveInfo.projectiles[i].castingOffSet = EditorGUILayout.Vector3Field("Casting Off Set:", moveInfo.projectiles[i].castingOffSet);
									moveInfo.projectiles[i].speed = EditorGUILayout.IntSlider("Speed:", moveInfo.projectiles[i].speed, 1, 100);
									moveInfo.projectiles[i].directionAngle = EditorGUILayout.IntSlider("Direction (Angle):", moveInfo.projectiles[i].directionAngle, -180, 180);
									moveInfo.projectiles[i].duration = EditorGUILayout.FloatField("Duration (Seconds):", moveInfo.projectiles[i].duration);

									
									EditorGUILayout.Space();
									EditorGUILayout.Space();
									SubGroupTitle("Hit Area");
									EditorGUILayout.Space();
									moveInfo.projectiles[i].hurtBox.shape = (HitBoxShape) EditorGUILayout.EnumPopup("Shape:", moveInfo.projectiles[i].hurtBox.shape, enumStyle);
									if (moveInfo.projectiles[i].hurtBox.shape == HitBoxShape.circle){
										moveInfo.projectiles[i].hurtBox.radius = EditorGUILayout.FloatField("Radius:", moveInfo.projectiles[i].hurtBox.radius);
										moveInfo.projectiles[i].hurtBox.offSet = EditorGUILayout.Vector2Field("Off Set:", moveInfo.projectiles[i].hurtBox.offSet);
									}else{
										moveInfo.projectiles[i].hurtBox.rect = EditorGUILayout.RectField("Rectangle:", moveInfo.projectiles[i].hurtBox.rect);
										
										EditorGUIUtility.labelWidth = 200;
										bool tmpFollowXBounds = moveInfo.projectiles[i].hurtBox.followXBounds;
										bool tmpFollowYBounds = moveInfo.projectiles[i].hurtBox.followYBounds;
										
										moveInfo.projectiles[i].hurtBox.followXBounds = EditorGUILayout.Toggle("Follow Projectile Bounds (X)", moveInfo.projectiles[i].hurtBox.followXBounds);
										moveInfo.projectiles[i].hurtBox.followYBounds = EditorGUILayout.Toggle("Follow Projectile Bounds (Y)", moveInfo.projectiles[i].hurtBox.followYBounds);
										
										if (tmpFollowXBounds != moveInfo.projectiles[i].hurtBox.followXBounds)
											moveInfo.projectiles[i].hurtBox.rect.width = moveInfo.projectiles[i].hurtBox.followXBounds ? 0 : 4;
										if (tmpFollowYBounds != moveInfo.projectiles[i].hurtBox.followYBounds)
											moveInfo.projectiles[i].hurtBox.rect.height = moveInfo.projectiles[i].hurtBox.followYBounds ? 0 : 4;
										
										EditorGUIUtility.labelWidth = 150;
									}

									
									EditorGUILayout.Space();
									EditorGUILayout.Space();
									SubGroupTitle("Blockable Area");
									EditorGUILayout.Space();
									moveInfo.projectiles[i].unblockable = EditorGUILayout.Toggle("Unblockable", moveInfo.projectiles[i].unblockable, toggleStyle);
									EditorGUI.BeginDisabledGroup(moveInfo.projectiles[i].unblockable);{
										moveInfo.projectiles[i].blockableArea.shape = (HitBoxShape) EditorGUILayout.EnumPopup("Shape:", moveInfo.projectiles[i].blockableArea.shape, enumStyle);
										if (moveInfo.projectiles[i].blockableArea.shape == HitBoxShape.circle){
											moveInfo.projectiles[i].blockableArea.radius = EditorGUILayout.FloatField("Radius:", moveInfo.projectiles[i].blockableArea.radius);
											moveInfo.projectiles[i].blockableArea.offSet = EditorGUILayout.Vector2Field("Off Set:", moveInfo.projectiles[i].blockableArea.offSet);
										}else{
											moveInfo.projectiles[i].blockableArea.rect = EditorGUILayout.RectField("Rectangle:", moveInfo.projectiles[i].blockableArea.rect);
											}
									}EditorGUI.EndDisabledGroup();

									
									EditorGUILayout.Space();
									EditorGUILayout.Space();
									SubGroupTitle("Hit Conditions");
									EditorGUILayout.Space();
									moveInfo.projectiles[i].projectileCollision = EditorGUILayout.Toggle("Hit projectiles", moveInfo.projectiles[i].projectileCollision, toggleStyle);
									moveInfo.projectiles[i].groundHit = EditorGUILayout.Toggle("Hit ground opponent", moveInfo.projectiles[i].groundHit, toggleStyle);
									moveInfo.projectiles[i].airHit = EditorGUILayout.Toggle("Hit air opponent", moveInfo.projectiles[i].airHit, toggleStyle);
									moveInfo.projectiles[i].downHit = EditorGUILayout.Toggle("Hit down opponent", moveInfo.projectiles[i].downHit, toggleStyle);
									moveInfo.projectiles[i].hitType = (HitType)EditorGUILayout.EnumPopup("Hit Type:", moveInfo.projectiles[i].hitType, enumStyle);
									
									
									EditorGUILayout.Space();
									EditorGUILayout.Space();
									SubGroupTitle("Collision Options");
									EditorGUILayout.Space();
									moveInfo.projectiles[i].totalHits = EditorGUILayout.IntField("Total Hits:", moveInfo.projectiles[i].totalHits);
									if (moveInfo.projectiles[i].totalHits > 1) moveInfo.projectiles[i].spaceBetweenHits = (Sizes) EditorGUILayout.EnumPopup("Space Between Hits:", moveInfo.projectiles[i].spaceBetweenHits, enumStyle);
									moveInfo.projectiles[i].pushForce = EditorGUILayout.Vector2Field("Push Force:", moveInfo.projectiles[i].pushForce);
									moveInfo.projectiles[i].hitStrength = (HitStrengh)EditorGUILayout.EnumPopup("Hit Strength:", moveInfo.projectiles[i].hitStrength, enumStyle);

									moveInfo.projectiles[i].overrideHitEffects = EditorGUILayout.Toggle("Override Hit Effects", moveInfo.projectiles[i].overrideHitEffects, toggleStyle);
									if (moveInfo.projectiles[i].overrideHitEffects) HitOptionBlock("Hit Effects", moveInfo.projectiles[i].hitEffects);
									
									EditorGUILayout.Space();
									EditorGUILayout.Space();
									// Damage Toggle
									moveInfo.projectiles[i].damageOptionsToggle = EditorGUILayout.Foldout(moveInfo.projectiles[i].damageOptionsToggle, "Damage Options", EditorStyles.foldout);
									if (moveInfo.projectiles[i].damageOptionsToggle){
										EditorGUILayout.BeginVertical(subGroupStyle);{
											EditorGUI.indentLevel += 1;
											moveInfo.projectiles[i].damageType = (DamageType) EditorGUILayout.EnumPopup("Damage Type:", moveInfo.projectiles[i].damageType, enumStyle);
											moveInfo.projectiles[i].damageOnHit = EditorGUILayout.FloatField("Damage on Hit:", moveInfo.projectiles[i].damageOnHit);
											moveInfo.projectiles[i].damageOnBlock = EditorGUILayout.FloatField("Damage on Block:", moveInfo.projectiles[i].damageOnBlock);
											moveInfo.projectiles[i].damageScaling = EditorGUILayout.Toggle("Damage Scaling", moveInfo.projectiles[i].damageScaling, toggleStyle);
											EditorGUI.indentLevel -= 1;
										}EditorGUILayout.EndVertical();
									}
									
									// Hit Stun Toggle
									moveInfo.projectiles[i].hitStunOptionsToggle = EditorGUILayout.Foldout(moveInfo.projectiles[i].hitStunOptionsToggle, "Hit Stun Options", EditorStyles.foldout);
									if (moveInfo.projectiles[i].hitStunOptionsToggle){
										EditorGUILayout.BeginVertical(subGroupStyle);{
											EditorGUI.indentLevel += 1;
											moveInfo.projectiles[i].resetPreviousHitStun = EditorGUILayout.Toggle("Reset Hit Stun", moveInfo.projectiles[i].resetPreviousHitStun, toggleStyle);
											moveInfo.projectiles[i].hitStunOnHit = EditorGUILayout.IntField("Hit Stun on Hit:", moveInfo.projectiles[i].hitStunOnHit);
											moveInfo.projectiles[i].hitStunOnBlock = EditorGUILayout.IntField("Hit Stun on Block:", moveInfo.projectiles[i].hitStunOnBlock);
											EditorGUI.indentLevel -= 1;
										}EditorGUILayout.EndVertical();
									}

                                    // Move Links Toggle
                                    moveInfo.projectiles[i].moveLinksToggle = EditorGUILayout.Foldout(moveInfo.projectiles[i].moveLinksToggle, "Move Override", EditorStyles.foldout);
                                    if (moveInfo.projectiles[i].moveLinksToggle) {
                                        EditorGUILayout.BeginVertical(subGroupStyle);
                                        {
                                            EditorGUI.indentLevel += 1;
                                            moveInfo.projectiles[i].moveLinkOnStrike = (MoveInfo)EditorGUILayout.ObjectField("On Strike:", moveInfo.projectiles[i].moveLinkOnStrike, typeof(MoveInfo), true);
                                            moveInfo.projectiles[i].moveLinkOnBlock = (MoveInfo)EditorGUILayout.ObjectField("On Block:", moveInfo.projectiles[i].moveLinkOnBlock, typeof(MoveInfo), true);
                                            moveInfo.projectiles[i].moveLinkOnParry = (MoveInfo)EditorGUILayout.ObjectField("On Parry:", moveInfo.projectiles[i].moveLinkOnParry, typeof(MoveInfo), true);
                                            moveInfo.projectiles[i].forceGrounded = EditorGUILayout.Toggle("Force Grounded", moveInfo.projectiles[i].forceGrounded, toggleStyle);
                                            EditorGUI.indentLevel -= 1;
                                        } EditorGUILayout.EndVertical();
                                    }

                                    EditorGUILayout.Space();
								}EditorGUI.EndDisabledGroup();

							}EditorGUILayout.EndVertical();
							EditorGUILayout.Space();
						}
						if (StyledButton("New Projectile"))
							moveInfo.projectiles = AddElement<Projectile>(moveInfo.projectiles, new Projectile());
							
						EditorGUI.indentLevel -= 1;
					
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();
			// End Projectile Options

			
			// Begin AI Definitions
			if (UFE.isAiAddonInstalled){
				EditorGUILayout.BeginVertical(rootGroupStyle);{
					EditorGUILayout.BeginHorizontal();{
						classificationOptions = EditorGUILayout.Foldout(classificationOptions, "A.I. Definitions", EditorStyles.foldout);
						helpButton("move:aidefinitions");
					}EditorGUILayout.EndHorizontal();
					
					if (classificationOptions){
						EditorGUILayout.BeginVertical(subGroupStyle);{
							EditorGUILayout.Space();
							EditorGUI.indentLevel += 1;
							
							EditorGUIUtility.labelWidth = 180;
							moveInfo.moveClassification.attackType = (AttackType)EditorGUILayout.EnumPopup("Move Type:",moveInfo.moveClassification.attackType, enumStyle);
							moveInfo.moveClassification.preferableDistance = (CharacterDistance)EditorGUILayout.EnumPopup("Range/Ideal Distance:",moveInfo.moveClassification.preferableDistance, enumStyle);
							EditorGUIUtility.labelWidth = 150;

							EditorGUILayout.Space();

							EditorGUI.BeginDisabledGroup(moveInfo.hits.Length == 0);{
								if (moveInfo.hits.Length == 0){
									SubGroupTitle("This move has no active frames.");
								}else{
									SubGroupTitle("Attack Definitions");
								}
								EditorGUILayout.Space();

								moveInfo.moveClassification.hitType = (HitType)EditorGUILayout.EnumPopup("Hit Type:",moveInfo.moveClassification.hitType, enumStyle);
								moveInfo.moveClassification.hitConfirmType = (HitConfirmType)EditorGUILayout.EnumPopup("Hit Confirm Type:",moveInfo.moveClassification.hitConfirmType, enumStyle);
							
								moveInfo.moveClassification.startupSpeed = (FrameSpeed)EditorGUILayout.EnumPopup("Startup Speed:",moveInfo.moveClassification.startupSpeed, enumStyle);
								moveInfo.moveClassification.recoverySpeed = (FrameSpeed)EditorGUILayout.EnumPopup("Recovery Speed:",moveInfo.moveClassification.recoverySpeed, enumStyle);
							}EditorGUI.EndDisabledGroup();

							if (StyledButton("Auto Detect")){
								if (moveInfo.hits.Length == 0){
									moveInfo.moveClassification.attackType = AttackType.Neutral;
									moveInfo.moveClassification.preferableDistance = CharacterDistance.Any;
								}

								if (moveInfo.appliedForces.Length > 0 && moveInfo.appliedForces[0].force.x > 10){
									moveInfo.moveClassification.attackType = AttackType.ForwardLauncher;
									moveInfo.moveClassification.preferableDistance = CharacterDistance.Mid;
								}
								
								if (moveInfo.appliedForces.Length > 0 && moveInfo.appliedForces[0].force.x < -10){
									moveInfo.moveClassification.attackType = AttackType.BackLauncher;
									moveInfo.moveClassification.preferableDistance = CharacterDistance.VeryClose;
								}

								if (moveInfo.hits.Length > 0 && moveInfo.buttonSequence.Length == 0){
									moveInfo.moveClassification.attackType = AttackType.NormalAttack;
									moveInfo.moveClassification.preferableDistance = CharacterDistance.Close;
								}

								if (moveInfo.hits.Length > 0 && moveInfo.appliedForces.Length > 0 && moveInfo.appliedForces[0].force.y > 15){
									moveInfo.moveClassification.attackType = AttackType.AntiAir;
									moveInfo.moveClassification.preferableDistance = CharacterDistance.Close;
								}
								
								if (moveInfo.hits.Length == 0 && moveInfo.projectiles.Length > 0){
									moveInfo.moveClassification.attackType = AttackType.Projectile;
									moveInfo.moveClassification.preferableDistance = CharacterDistance.Far;
								}
								
								if (moveInfo.appliedForces.Length > 0 && moveInfo.appliedForces[0].force.y < -20 
								    && moveInfo.selfConditions.possibleMoveStates.Length > 0
								    && (moveInfo.selfConditions.possibleMoveStates[0].possibleState == PossibleStates.StraightJump ||
								 	moveInfo.selfConditions.possibleMoveStates[0].possibleState == PossibleStates.BackJump ||
								 	moveInfo.selfConditions.possibleMoveStates[0].possibleState == PossibleStates.ForwardJump)){

									moveInfo.moveClassification.attackType = AttackType.Dive;
									moveInfo.moveClassification.preferableDistance = CharacterDistance.Close;
								}


								if (moveInfo.hits.Length > 0){
									moveInfo.moveClassification.hitType = moveInfo.hits[0].hitType;
									moveInfo.moveClassification.hitConfirmType = moveInfo.hits[0].hitConfirmType;

									if (moveInfo.moveClassification.hitConfirmType == HitConfirmType.Throw)
										moveInfo.moveClassification.preferableDistance = CharacterDistance.VeryClose;

									if (moveInfo.startUpFrames <= 4) moveInfo.moveClassification.startupSpeed = FrameSpeed.VeryFast;
									else if (moveInfo.startUpFrames <= 7) moveInfo.moveClassification.startupSpeed = FrameSpeed.Fast;
									else if (moveInfo.startUpFrames <= 11) moveInfo.moveClassification.startupSpeed = FrameSpeed.Normal;
									else if (moveInfo.startUpFrames <= 15) moveInfo.moveClassification.startupSpeed = FrameSpeed.Slow;
									else if (moveInfo.startUpFrames > 15) moveInfo.moveClassification.startupSpeed = FrameSpeed.VerySlow;
									
									if (moveInfo.recoveryFrames <= 4) moveInfo.moveClassification.recoverySpeed = FrameSpeed.VeryFast;
									else if (moveInfo.recoveryFrames <= 7) moveInfo.moveClassification.recoverySpeed = FrameSpeed.Fast;
									else if (moveInfo.recoveryFrames <= 11) moveInfo.moveClassification.recoverySpeed = FrameSpeed.Normal;
									else if (moveInfo.recoveryFrames <= 15) moveInfo.moveClassification.recoverySpeed = FrameSpeed.Slow;
									else if (moveInfo.recoveryFrames > 15) moveInfo.moveClassification.recoverySpeed = FrameSpeed.VerySlow;
								}
							}

							GaugeUsage gaugeUsageTemp = GaugeUsage.None;
							if (moveInfo.gaugeUsage < 150) gaugeUsageTemp = GaugeUsage.Quarter;
							else if (moveInfo.gaugeUsage < 300) gaugeUsageTemp = GaugeUsage.Half;
							else if (moveInfo.gaugeUsage < 400) gaugeUsageTemp = GaugeUsage.ThreeQuarters;
							else if (moveInfo.gaugeUsage >= 400) gaugeUsageTemp = GaugeUsage.All;
							moveInfo.moveClassification.gaugeUsage = gaugeUsageTemp;

							EditorGUI.indentLevel -= 1;
						}EditorGUILayout.EndVertical();
					}
				}EditorGUILayout.EndVertical();
			}
			// End AI Definitions

		}EditorGUILayout.EndScrollView();

		if (GUI.changed) {
			Undo.RecordObject(moveInfo, "Move Editor Modify");
			EditorUtility.SetDirty(moveInfo);
		}
	}
	
	private void PlayerConditionsGroup(string _name, PlayerConditions playerConditions){
		EditorGUILayout.BeginVertical(subGroupStyle);{
			SubGroupTitle(_name);
			EditorGUI.indentLevel += 1;
			playerConditions.basicMovesToggle = EditorGUILayout.Foldout(playerConditions.basicMovesToggle, "Basic Moves Filter ("+ playerConditions.basicMoveLimitation.Length +")", EditorStyles.foldout);
			if (playerConditions.basicMovesToggle){
				EditorGUILayout.BeginVertical(subGroupStyle);{
					EditorGUILayout.Space();
					EditorGUI.indentLevel += 1;
					if (playerConditions.basicMoveLimitation != null){
						for (int y = 0; y < playerConditions.basicMoveLimitation.Length; y ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(subArrayElementStyle);{
								EditorGUILayout.BeginHorizontal();{
									playerConditions.basicMoveLimitation[y] = (BasicMoveReference)EditorGUILayout.EnumPopup("Basic Move:", playerConditions.basicMoveLimitation[y], enumStyle);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<BasicMoveReference>(playerConditions.basicMoveLimitation, playerConditions.basicMoveLimitation[y], delegate (BasicMoveReference[] newElement) { playerConditions.basicMoveLimitation = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
					}
					if (StyledButton("New Basic Move"))
						playerConditions.basicMoveLimitation = AddElement<BasicMoveReference>(playerConditions.basicMoveLimitation, BasicMoveReference.Idle);

				}EditorGUILayout.EndVertical();

				EditorGUI.indentLevel -= 1;
			}
			
			
			playerConditions.statesToggle = EditorGUILayout.Foldout(playerConditions.statesToggle, "Possible States ("+ playerConditions.possibleMoveStates.Length +")", EditorStyles.foldout);
			if (playerConditions.statesToggle){
				EditorGUILayout.BeginVertical(subGroupStyle);{
					EditorGUI.indentLevel += 1;
					for (int i = 0; i < playerConditions.possibleMoveStates.Length; i ++){
						EditorGUILayout.Space();
						EditorGUILayout.BeginVertical(subArrayElementStyle);{
							EditorGUILayout.Space();
							EditorGUILayout.BeginHorizontal();{
								playerConditions.possibleMoveStates[i].possibleState = (PossibleStates)EditorGUILayout.EnumPopup("State:", playerConditions.possibleMoveStates[i].possibleState, enumStyle);
								if (GUILayout.Button("", "PaneOptions")){
									PaneOptions<PossibleMoveStates>(playerConditions.possibleMoveStates, playerConditions.possibleMoveStates[i], delegate (PossibleMoveStates[] newElement) { playerConditions.possibleMoveStates = newElement; });
								}
							}EditorGUILayout.EndHorizontal();
							
							playerConditions.possibleMoveStates[i].opponentDistance = (CharacterDistance)EditorGUILayout.EnumPopup("Opponent Distance:", playerConditions.possibleMoveStates[i].opponentDistance, enumStyle);
							if (playerConditions.possibleMoveStates[i].opponentDistance == CharacterDistance.Any) {
								playerConditions.possibleMoveStates[i].proximityRangeBegins = 0;
								playerConditions.possibleMoveStates[i].proximityRangeEnds = 100;
							}else if (playerConditions.possibleMoveStates[i].opponentDistance == CharacterDistance.VeryClose) {
								playerConditions.possibleMoveStates[i].proximityRangeBegins = 0;
								playerConditions.possibleMoveStates[i].proximityRangeEnds = 25;
							}else if (playerConditions.possibleMoveStates[i].opponentDistance == CharacterDistance.Close) {
								playerConditions.possibleMoveStates[i].proximityRangeBegins = 0;
								playerConditions.possibleMoveStates[i].proximityRangeEnds = 50;
							}else if (playerConditions.possibleMoveStates[i].opponentDistance == CharacterDistance.Mid) {
								playerConditions.possibleMoveStates[i].proximityRangeBegins = 40;
								playerConditions.possibleMoveStates[i].proximityRangeEnds = 60;
							}else if (playerConditions.possibleMoveStates[i].opponentDistance == CharacterDistance.Far) {
								playerConditions.possibleMoveStates[i].proximityRangeBegins = 50;
								playerConditions.possibleMoveStates[i].proximityRangeEnds = 100;
							}else if (playerConditions.possibleMoveStates[i].opponentDistance == CharacterDistance.VeryFar) {
								playerConditions.possibleMoveStates[i].proximityRangeBegins = 75;
								playerConditions.possibleMoveStates[i].proximityRangeEnds = 100;
							}
							
							int pArcBeginsTemp = playerConditions.possibleMoveStates[i].proximityRangeBegins;
							int pArcEndsTemp = playerConditions.possibleMoveStates[i].proximityRangeEnds;
							EditorGUI.indentLevel += 2;
							StyledMinMaxSlider("Proximity", ref playerConditions.possibleMoveStates[i].proximityRangeBegins, ref playerConditions.possibleMoveStates[i].proximityRangeEnds, 0, 100, EditorGUI.indentLevel);
							EditorGUI.indentLevel -= 2;
							if (playerConditions.possibleMoveStates[i].proximityRangeBegins != pArcBeginsTemp ||
							    playerConditions.possibleMoveStates[i].proximityRangeEnds != pArcEndsTemp){
								playerConditions.possibleMoveStates[i].opponentDistance = CharacterDistance.Other;
							}
							
							EditorGUILayout.Space();
							
							if (playerConditions.possibleMoveStates[i].possibleState == PossibleStates.StraightJump ||
							    playerConditions.possibleMoveStates[i].possibleState == PossibleStates.ForwardJump ||
							    playerConditions.possibleMoveStates[i].possibleState == PossibleStates.BackJump){
								playerConditions.possibleMoveStates[i].jumpArc = (JumpArc)EditorGUILayout.EnumPopup("Jump Arc:", playerConditions.possibleMoveStates[i].jumpArc, enumStyle);
								if (playerConditions.possibleMoveStates[i].jumpArc == JumpArc.Any) {
									playerConditions.possibleMoveStates[i].jumpArcBegins = 0;
									playerConditions.possibleMoveStates[i].jumpArcEnds = 100;
								}else if (playerConditions.possibleMoveStates[i].jumpArc == JumpArc.TakeOff) {
									playerConditions.possibleMoveStates[i].jumpArcBegins = 0;
									playerConditions.possibleMoveStates[i].jumpArcEnds = 35;
								}else if (playerConditions.possibleMoveStates[i].jumpArc == JumpArc.Jumping) {
									playerConditions.possibleMoveStates[i].jumpArcBegins = 0;
									playerConditions.possibleMoveStates[i].jumpArcEnds = 60;
								}else if (playerConditions.possibleMoveStates[i].jumpArc == JumpArc.Top) {
									playerConditions.possibleMoveStates[i].jumpArcBegins = 30;
									playerConditions.possibleMoveStates[i].jumpArcEnds = 70;
								}else if (playerConditions.possibleMoveStates[i].jumpArc == JumpArc.Falling) {
									playerConditions.possibleMoveStates[i].jumpArcBegins = 50;
									playerConditions.possibleMoveStates[i].jumpArcEnds = 100;
								}else if (playerConditions.possibleMoveStates[i].jumpArc == JumpArc.Landing) {
									playerConditions.possibleMoveStates[i].jumpArcBegins = 65;
									playerConditions.possibleMoveStates[i].jumpArcEnds = 100;
								}
								
								int arcBeginsTemp = playerConditions.possibleMoveStates[i].jumpArcBegins;
								int arcEndsTemp = playerConditions.possibleMoveStates[i].jumpArcEnds;
								EditorGUI.indentLevel += 2;
								StyledMinMaxSlider("Jump Arc (%)", ref playerConditions.possibleMoveStates[i].jumpArcBegins, ref playerConditions.possibleMoveStates[i].jumpArcEnds, 0, 100, EditorGUI.indentLevel);
								EditorGUI.indentLevel -= 2;
								if (playerConditions.possibleMoveStates[i].jumpArcBegins != arcBeginsTemp ||
								    playerConditions.possibleMoveStates[i].jumpArcEnds != arcEndsTemp){
									playerConditions.possibleMoveStates[i].jumpArc = JumpArc.Other;
								}
								
							}else if (playerConditions.possibleMoveStates[i].possibleState == PossibleStates.Stand){
								playerConditions.possibleMoveStates[i].standBy = EditorGUILayout.Toggle("Idle", playerConditions.possibleMoveStates[i].standBy, toggleStyle);
								playerConditions.possibleMoveStates[i].movingForward = EditorGUILayout.Toggle("Moving Forward", playerConditions.possibleMoveStates[i].movingForward, toggleStyle);
								playerConditions.possibleMoveStates[i].movingBack = EditorGUILayout.Toggle("Moving Back", playerConditions.possibleMoveStates[i].movingBack, toggleStyle);
							}
							
							
							if (playerConditions.possibleMoveStates[i].possibleState != PossibleStates.Down){
								playerConditions.possibleMoveStates[i].blocking = EditorGUILayout.Toggle("Blocking", playerConditions.possibleMoveStates[i].blocking, toggleStyle);
								playerConditions.possibleMoveStates[i].stunned = EditorGUILayout.Toggle("Stunned", playerConditions.possibleMoveStates[i].stunned, toggleStyle);
							}
							
							EditorGUILayout.Space();
						}EditorGUILayout.EndVertical();
					}
					if (StyledButton("New Possible Move State"))
						playerConditions.possibleMoveStates = AddElement<PossibleMoveStates>(playerConditions.possibleMoveStates, null);
					
					EditorGUI.indentLevel -= 1;
				}EditorGUILayout.EndVertical();

			}
			EditorGUI.indentLevel -= 1;
			
		}EditorGUILayout.EndVertical();
	}

	private void HitOptionBlock(string label, HitTypeOptions hit){
		hit.editorToggle = EditorGUILayout.Foldout(hit.editorToggle, label, foldStyle);
		if (hit.editorToggle){
			EditorGUILayout.BeginVertical(subGroupStyle);{
				EditorGUILayout.Space();
				EditorGUI.indentLevel += 1;
				
				hit.hitParticle = (GameObject) EditorGUILayout.ObjectField("Particle Effect:", hit.hitParticle, typeof(UnityEngine.GameObject), true);
				hit.killTime = EditorGUILayout.FloatField("Effect Duration:", hit.killTime);
				hit.hitSound = (AudioClip) EditorGUILayout.ObjectField("Sound Effect:", hit.hitSound, typeof(UnityEngine.AudioClip), true);
				hit.freezingTime = EditorGUILayout.FloatField("Freezing Time:", hit.freezingTime);
				hit.animationSpeed = EditorGUILayout.FloatField("Animation Speed (%):", hit.animationSpeed);
				hit.shakeCharacterOnHit = EditorGUILayout.Toggle("Shake Character On Hit", hit.shakeCharacterOnHit);
				hit.shakeCameraOnHit = EditorGUILayout.Toggle("Shake Camera On Hit", hit.shakeCameraOnHit);
				hit.shakeDensity = EditorGUILayout.FloatField("Shake Density:", hit.shakeDensity);
				
				EditorGUI.indentLevel -= 1;
				EditorGUILayout.Space();
				
			}EditorGUILayout.EndVertical();
		}
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

	public int StyledSlider (string label, int targetVar, int indentLevel, int minValue, int maxValue) {
		int indentSpacing = 25 * indentLevel;			
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		Rect tempRect = GUILayoutUtility.GetRect(1, 10);

		Rect rect = new Rect(indentSpacing,tempRect.y, Screen.width - indentSpacing - 100, 20);
		EditorGUI.ProgressBar(rect, Mathf.Abs((float)targetVar/maxValue), label);
		
		tempRect = GUILayoutUtility.GetRect(1, 20);
		rect.y += 10;
		rect.x = indentLevel * 10;
		rect.width = (Screen.width - (indentLevel * 10) - 100) + 55; // Changed for 4.3;

		return EditorGUI.IntSlider(rect, "", targetVar, minValue, maxValue);
	}
	
	public float StyledSlider (string label, float targetVar, int indentLevel, float minValue, float maxValue) {
		int indentSpacing = 25 * indentLevel;
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		Rect tempRect = GUILayoutUtility.GetRect(1, 10);

		Rect rect = new Rect(indentSpacing,tempRect.y, Screen.width - indentSpacing - 100, 20);
		EditorGUI.ProgressBar(rect, Mathf.Abs((float)targetVar/maxValue), label);
		
		tempRect = GUILayoutUtility.GetRect(1, 20);
		rect.y += 10;
		rect.x = indentLevel * 10;
		rect.width = (Screen.width - (indentLevel * 10) - 100) + 55; // Changed for 4.3;

		return EditorGUI.Slider(rect, "", targetVar, minValue, maxValue);
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
	
	public void StyledMinMaxSlider (string label, ref int minValue, ref int maxValue, int minLimit, int maxLimit, int indentLevel) {
		int indentSpacing = 25 * indentLevel;
		//indentSpacing += 30;
		EditorGUILayout.Space();
		EditorGUILayout.Space();

        if (minValue < minLimit) minValue = minLimit;
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
	
	public void StyledMarker (string label, int[] locations, int maxValue, int indentLevel) {
		if (indentLevel == 1) indentLevel++;
		int indentSpacing = 25 * indentLevel;
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		Rect tempRect = GUILayoutUtility.GetRect(1, 20);
		Rect rect = new Rect(indentSpacing,tempRect.y, Screen.width - indentSpacing - 60, 20);
		
		// Border
		GUI.Box(rect, "", borderBarStyle);
		
		// Overlay
		foreach(int i in locations){
			float xPos = ((rect.width/(float)maxValue) * (float)i) + rect.x;
			if (xPos + 5 > rect.width + rect.x) xPos -= 5;
			GUI.Box(new Rect(xPos, rect.y, 5, rect.height), new GUIContent(), fillBarStyle2);
		}
		
		// Text
		GUI.Label(rect, new GUIContent(label), labelStyle);
		
		tempRect = GUILayoutUtility.GetRect(1, 20);
	}

	public void StyledMarker (string label, Vector3[] locations, int maxValue, int indentLevel, bool fillBounds) {
		if (indentLevel == 1) indentLevel++;
		int indentSpacing = 25 * indentLevel;
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		Rect tempRect = GUILayoutUtility.GetRect(1, 20);
		Rect rect = new Rect(indentSpacing,tempRect.y, Screen.width - indentSpacing - 60, 20);
		
		// Border
		GUI.Box(rect, "", borderBarStyle);

		if (fillBounds && locations.Length > 0){
			float firstLeftPos = ((rect.width/maxValue) * locations[0].x);
			firstLeftPos -= (rect.width/maxValue);
			GUI.Box(new Rect(rect.x, rect.y, firstLeftPos, rect.height), new GUIContent(), fillBarStyle3);
		}

		// Overlay
		float fillLeftPos = 0;
		float fillRightPos = 0;
		foreach(Vector3 i in locations){
			fillLeftPos = ((rect.width/maxValue) * i.x) + rect.x;
			fillRightPos = ((rect.width/maxValue) * i.y) + rect.x;

			float fillWidth = fillRightPos - fillLeftPos;
			fillWidth += (rect.width/maxValue);
			fillLeftPos -= (rect.width/maxValue);

			if (i.z > 0){
				GUI.Box(new Rect(fillLeftPos, rect.y, fillWidth, rect.height), new GUIContent(), fillBarStyle5);
			}else{
				GUI.Box(new Rect(fillLeftPos, rect.y, fillWidth, rect.height), new GUIContent(), fillBarStyle2);
			}
		}

		if (fillBounds && locations.Length > 0){
			float fillWidth = rect.width - fillRightPos + rect.x;
			GUI.Box(new Rect(fillRightPos, rect.y, fillWidth, rect.height), new GUIContent(), fillBarStyle4);
		}

		// Text
		GUI.Label(rect, new GUIContent(label), labelStyle);

		if (fillBounds && locations.Length > 0){
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal(subArrayElementStyle);{
				labelStyle.normal.textColor = Color.yellow;
				moveInfo.startUpFrames = (moveInfo.hits[0].activeFramesBegin - 1);
				GUILayout.Label("Start Up: "+ (moveInfo.startUpFrames + 1), labelStyle);
				labelStyle.normal.textColor = Color.cyan;
				moveInfo.activeFrames = (moveInfo.hits[moveInfo.hits.Length - 1].activeFramesEnds - moveInfo.hits[0].activeFramesBegin);
				GUILayout.Label("Active: "+ moveInfo.activeFrames, labelStyle);
				labelStyle.normal.textColor = Color.red;
				moveInfo.recoveryFrames = (moveInfo.totalFrames - moveInfo.hits[moveInfo.hits.Length - 1].activeFramesEnds + 1);
				GUILayout.Label("Recovery: "+ moveInfo.recoveryFrames, labelStyle);
			}GUILayout.EndHorizontal();
		}
		labelStyle.normal.textColor = Color.white;

		//GUI.skin.label.normal.textColor = new Color(.706f, .706f, .706f, 1);
		tempRect = GUILayoutUtility.GetRect(1, 20);
	}
	
	public void AnimationSampler(GameObject targetObj, AnimationClip animationClip, int castingFrame, float speed){
		animTime = (((float)(animFrame - castingFrame)/moveInfo.fps) * speed);
		//animTime = ((float)(animFrame + startupFrame - castingFrame)/moveInfo.fps);
		if (animTime < 0) animTime = 0;
		
		Animator animator = targetObj.GetComponent<Animator>();
		if (animator == null) animator = (Animator) targetObj.AddComponent(typeof(Animator));
		if (animator.runtimeAnimatorController == null) 
			animator.runtimeAnimatorController = (RuntimeAnimatorController) Resources.Load("controller1");

		if (animFrame > castingFrame) animationClip.SampleAnimation(targetObj, animTime);
	}

	public void ProjectileSampler(GameObject prefab, Projectile projectile){
		ProjectileMoveScript projectileMoveScript = prefab.AddComponent<ProjectileMoveScript>();
		
		projectileMoveScript.data = projectile;
		projectileMoveScript.blockableArea = projectile.blockableArea;
		projectileMoveScript.hurtBox = projectile.hurtBox;
		projectileMoveScript.UpdateRenderer();
	}

	public void AnimationSampler(GameObject targetChar, AnimationClip animationClip, int castingFrame, bool loadHitBoxes, bool loadHurtBoxes, bool mirror, bool invertRotation){
		if (moveInfo.animationSpeed < 0){
			animTime = (((animFrame - castingFrame)/moveInfo.fps) * moveInfo.animationSpeed) + animationClip.length;
		}else{
			animTime = ((animFrame - castingFrame)/moveInfo.fps) * moveInfo.animationSpeed;
			//if (animTime > animationClip.length) animTime = animationClip.length;
		}
		if (animTime < 0) animTime = 0;

		Animator animator = targetChar.GetComponent<Animator>();
		if (animator == null) animator = (Animator) targetChar.AddComponent(typeof(Animator));
		if (animator.runtimeAnimatorController == null) 
			animator.runtimeAnimatorController = (RuntimeAnimatorController) Resources.Load("controller1");
		
		if (loadHitBoxes){
			HitBoxesScript hitBoxesScript = targetChar.GetComponent<HitBoxesScript>();
			hitBoxesScript.UpdateRenderer();

			// BEGIN INVERT ROTATION AND MIRROR OPTIONS
			if (invertRotation && !hitBoxesScript.previewInvertRotation){
				hitBoxesScript.previewInvertRotation = true;
				
				Vector3 rotationDirection = targetChar.transform.rotation.eulerAngles;
				rotationDirection.y += 180;
				targetChar.transform.rotation = Quaternion.Euler(rotationDirection);
			}else if (!invertRotation && hitBoxesScript.previewInvertRotation){
				hitBoxesScript.previewInvertRotation = false;
				
				Vector3 rotationDirection = targetChar.transform.rotation.eulerAngles;
				rotationDirection.y -= 180;
				targetChar.transform.rotation = Quaternion.Euler(rotationDirection);
			}

			if (hitBoxesScript.previewMirror != mirror){
				hitBoxesScript.previewMirror = mirror;
				targetChar.transform.localScale = new Vector3(-targetChar.transform.localScale.x, targetChar.transform.localScale.y, targetChar.transform.localScale.z);
			}
			// END INVERT ROTATION AND MIRROR OPTIONS

			if (loadHurtBoxes){
				foreach (Hit hit in moveInfo.hits){
					if (animFrame >= hit.activeFramesBegin && animFrame < hit.activeFramesEnds) {
						if (hit.hurtBoxes.Length > 0){
							hitBoxesScript.hitConfirmType = hit.hitConfirmType;
							hitBoxesScript.activeHurtBoxes = hit.hurtBoxes;
						}
						break;
					}else{
						hitBoxesScript.activeHurtBoxes = null;
					}
				}

				if (animFrame >= moveInfo.blockableArea.activeFramesBegin && animFrame < moveInfo.blockableArea.activeFramesEnds) {
					hitBoxesScript.blockableArea = moveInfo.blockableArea;
				}else{
					hitBoxesScript.blockableArea = null;
				}

				foreach (InvincibleBodyParts invincibleBodyPart in moveInfo.invincibleBodyParts){
					if (animFrame >= invincibleBodyPart.activeFramesBegin && animFrame < invincibleBodyPart.activeFramesEnds) {
						if (invincibleBodyPart.completelyInvincible){
							hitBoxesScript.HideHitBoxes(true);
						}else{
							if (invincibleBodyPart.bodyParts != null && invincibleBodyPart.bodyParts.Length > 0)
								hitBoxesScript.HideHitBoxes(hitBoxesScript.GetHitBoxes(invincibleBodyPart.bodyParts), true);
						}
						break;
					}else{
						if (invincibleBodyPart.completelyInvincible){
							hitBoxesScript.HideHitBoxes(false);
						}else{
							if (invincibleBodyPart.bodyParts != null && invincibleBodyPart.bodyParts.Length > 0)
								hitBoxesScript.HideHitBoxes(hitBoxesScript.GetHitBoxes(invincibleBodyPart.bodyParts), false);
						}
					}
				}
			}

			if (animFrame == 0 || animFrame == moveInfo.totalFrames) hitBoxesScript.HideHitBoxes(false);
		}
		if (animFrame > castingFrame) animationClip.SampleAnimation(targetChar, animTime);

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
