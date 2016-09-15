using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;


public class CharacterEditorWindow : EditorWindow {
	public static CharacterEditorWindow characterEditorWindow;
	public static CharacterInfo sentCharacterInfo;
	private CharacterInfo characterInfo;

	private Vector2 scrollPos;
	private GameObject character;
	private HitBoxesScript hitBoxesScript;
	private bool characterPreviewToggle;

	private int bloodTypeChoice;
	private string[] bloodTypeChoices = new string[]{"Unknown","O-","O+","A-","A+","B-","B+","AB-","AB+"};
	
	private bool debugOptions;
	private bool hitBoxesOption;
	private bool transformToggle;
	private bool hitBoxesToggle;
	private bool bendingSegmentsToggle;
	private bool nonAffectedJointsToggle;

	private bool physicsOption;
	private bool headLookOption;
	private bool moveSetOption;
	private bool aiInstructionsOption;
	private bool characterWarning;
	private string errorMsg;


	private string titleStyle;
	private string addButtonStyle;
	private string rootGroupStyle;
	private string subGroupStyle;
	private string arrayElementStyle;
	private string subArrayElementStyle;
	private string toggleStyle;
	private string foldStyle;
	private string enumStyle;

	[MenuItem("Window/U.F.E./Character Editor")]
	public static void Init(){
		characterEditorWindow = EditorWindow.GetWindow<CharacterEditorWindow>(false, "Character", true);
		characterEditorWindow.Show();
		characterEditorWindow.Populate();
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
		ClosePreview();
	}
	
	void OnDestroy(){
		ClosePreview();
	}
	
	void OnLostFocus(){
		//ClosePreview();
	}

	public void PreviewCharacter(){
		characterPreviewToggle = true;
		EditorCamera.SetPosition(Vector3.up * 3.5f);
		EditorCamera.SetRotation(Quaternion.identity);
		EditorCamera.SetOrthographic(true);
		EditorCamera.SetSize(8);
		if (character == null){
			character = (GameObject) PrefabUtility.InstantiatePrefab(characterInfo.characterPrefab);
			character.transform.position = new Vector3(-2,0,0);
		}
		hitBoxesScript = character.GetComponent<HitBoxesScript>();
		hitBoxesScript.UpdateRenderer();
	}
	
	public void ClosePreview(){
		characterPreviewToggle = false;
		if (character != null){
			DestroyImmediate(character);
			character = null;
		}
		hitBoxesScript = null;
	}

	void helpButton(string page){
		if (GUILayout.Button("?", GUILayout.Width(18), GUILayout.Height(18))) 
			Application.OpenURL("http://www.ufe3d.com/doku.php/"+ page);
	}

	void Update(){
		if (EditorApplication.isPlayingOrWillChangePlaymode && character != null) {
			ClosePreview();
		}
	}

	void Populate(){
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
        
		UFE.isAiAddonInstalled = UFE.IsInstalled("RuleBasedAI");

		if (sentCharacterInfo != null){
			EditorGUIUtility.PingObject( sentCharacterInfo );
			Selection.activeObject = sentCharacterInfo;
			sentCharacterInfo = null;
		}

		UnityEngine.Object[] selection = Selection.GetFiltered(typeof(CharacterInfo), SelectionMode.Assets);
		if (selection.Length > 0){
			if (selection[0] == null) return;
			//characterInfoSO = new SerializedObject(selection[0]);
			characterInfo = (CharacterInfo) selection[0];
		}
	}
	
	public void OnGUI(){
		if (characterInfo == null){
			GUILayout.BeginHorizontal("GroupBox");
			GUILayout.Label("Select a character file or create a new character.","CN EntryInfo");
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			if (GUILayout.Button("Create new character"))
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
				EditorGUILayout.LabelField("", (characterInfo.characterName == ""? "New Character":characterInfo.characterName) , fontStyle, GUILayout.Height(32));
				helpButton("character:start");
			}EditorGUILayout.EndHorizontal();
		}EditorGUILayout.EndVertical();
		
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);{
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					characterInfo.profilePictureSmall = (Texture2D) EditorGUILayout.ObjectField(characterInfo.profilePictureSmall, typeof(Texture2D), false, GUILayout.Width(100), GUILayout.Height(122));

					EditorGUILayout.BeginVertical();{
						EditorGUIUtility.labelWidth = 90;
						characterInfo.characterName = EditorGUILayout.TextField("Name:", characterInfo.characterName);
						characterInfo.age = EditorGUILayout.IntField("Age:", characterInfo.age);
						bloodTypeChoice = EditorGUILayout.Popup("Blood Type:", bloodTypeChoice, bloodTypeChoices);
						characterInfo.bloodType = bloodTypeChoices[bloodTypeChoice];
						characterInfo.gender = (Gender) EditorGUILayout.EnumPopup("Gender:", characterInfo.gender);
						characterInfo.height = EditorGUILayout.FloatField("Height:", characterInfo.height);
						characterInfo.lifePoints = EditorGUILayout.IntField("Life Points:", characterInfo.lifePoints);
						characterInfo.maxGaugePoints = EditorGUILayout.IntField("Max Gauge:", characterInfo.maxGaugePoints);
					} EditorGUILayout.EndVertical();

				}EditorGUILayout.EndHorizontal();
				EditorGUIUtility.labelWidth = 180;
				EditorGUILayout.LabelField("Portrail Big:");
				characterInfo.profilePictureBig = (Texture2D) EditorGUILayout.ObjectField(characterInfo.profilePictureBig, typeof(Texture2D), false);
				EditorGUILayout.Space();

				characterInfo.enableAlternativeColor = EditorGUILayout.Toggle("Enable alternative color", characterInfo.enableAlternativeColor);
				if (characterInfo.enableAlternativeColor) characterInfo.alternativeColor = EditorGUILayout.ColorField("Alternative Color:", characterInfo.alternativeColor);
				characterInfo.selectionSound = (AudioClip) EditorGUILayout.ObjectField("Character Selection Sound:", characterInfo.selectionSound, typeof(UnityEngine.AudioClip), false);
				characterInfo.deathSound = (AudioClip) EditorGUILayout.ObjectField("Death Sound:", characterInfo.deathSound, typeof(UnityEngine.AudioClip), false);

				EditorGUILayout.Space();
				EditorGUIUtility.labelWidth = 200;
				GUILayout.Label("Description:");
				Rect rect = GUILayoutUtility.GetRect(50, 90);
				EditorStyles.textField.wordWrap = true;
				characterInfo.characterDescription = EditorGUI.TextArea(rect, characterInfo.characterDescription);
				
				EditorGUILayout.Space();
				EditorGUIUtility.labelWidth = 150;

				EditorGUILayout.Space();
			}EditorGUILayout.EndVertical();


			// Hit Boxes
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					hitBoxesOption = EditorGUILayout.Foldout(hitBoxesOption, "Hit Box Setup", foldStyle);
					helpButton("character:hitbox");
				}EditorGUILayout.EndHorizontal();

				if (hitBoxesOption){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
					
						characterInfo.characterPrefab = (GameObject) EditorGUILayout.ObjectField("Character Prefab:", characterInfo.characterPrefab, typeof(UnityEngine.GameObject), true);
						if (characterInfo.characterPrefab != null){
							if (PrefabUtility.GetPrefabType(characterInfo.characterPrefab) != PrefabType.Prefab){
								characterWarning = true;
								errorMsg = "This character is not a prefab.";
								characterInfo.characterPrefab = null;
								ClosePreview();
							}else if (characterInfo.characterPrefab.GetComponent<HitBoxesScript>() == null){
								characterWarning = true;
								errorMsg = "This character doesn't have hitboxes!\n Please add the HitboxesScript and try again.";
								characterInfo.characterPrefab = null;
								ClosePreview();
							}else if (character != null && EditorApplication.isPlayingOrWillChangePlaymode) {
								characterWarning = true;
								errorMsg = "You can't change this field while in play mode.";
								ClosePreview();
							}else {
								characterWarning = false;
								if (character != null && characterInfo.characterPrefab.name != character.name) ClosePreview();
							}
						}

						if (characterWarning){
							GUILayout.BeginHorizontal("GroupBox");
							GUILayout.Label(errorMsg, "CN EntryWarn");
							GUILayout.EndHorizontal();
						}

						if (characterInfo.characterPrefab != null){
							if (!characterPreviewToggle){
								if (StyledButton("Open Character")) {
									EditorWindow.FocusWindowIfItsOpen<SceneView>();
									PreviewCharacter();
								}
							}else{
								if (StyledButton("Close Character")) ClosePreview();
							}
							
							if (character != null){
								EditorGUILayout.BeginVertical(subGroupStyle);{	
									EditorGUILayout.Space();
									transformToggle = EditorGUILayout.Foldout(transformToggle, "Transform", EditorStyles.foldout);
									if (transformToggle){
										EditorGUILayout.BeginVertical(subGroupStyle);{
											EditorGUI.indentLevel += 1;
											character.transform.position = EditorGUILayout.Vector3Field("Position", character.transform.position);
											character.transform.rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", character.transform.rotation.eulerAngles));
											character.transform.localScale = EditorGUILayout.Vector3Field("Scale", character.transform.localScale);
											EditorGUI.indentLevel -= 1;
										}EditorGUILayout.EndVertical();
									}
									
									EditorGUILayout.Space();

									EditorGUI.BeginDisabledGroup(FindTransform("Head") == null);{
										if (StyledButton("Auto-Setup for Mixamo Auto-rigger")){
											if (EditorUtility.DisplayDialog("Replace Hitboxes?", 
											                                "This action will delete your current hitbox setup. Are you sure you want to replace it?",
											                                "Yes", "No")){
												hitBoxesScript.hitBoxes = new HitBox[0];
												for(int i = 0; i <= 14; i ++){
													HitBox newHitBox = new HitBox();
													if (i == 0){
														newHitBox.bodyPart = BodyPart.head;
														newHitBox.collisionType = CollisionType.bodyCollider;
														newHitBox.type = HitBoxType.high;
														newHitBox.radius = .7f;
														newHitBox.position = FindTransform("Head");
													/*}else if (i == 1){
														newHitBox.bodyPart = BodyPart.neck;
														newHitBox.collisionType = CollisionType.noCollider;
														newHitBox.type = HitBoxType.high;
														newHitBox.radius = .2f;
														newHitBox.position = FindTransform("Neck");*/
													}else if (i == 1){
														newHitBox.bodyPart = BodyPart.upperTorso;
														newHitBox.collisionType = CollisionType.bodyCollider;
														newHitBox.type = HitBoxType.high;
														newHitBox.radius = .9f;
														newHitBox.position = FindTransform("Spine2");
													}else if (i == 2){
														newHitBox.bodyPart = BodyPart.lowerTorso;
														newHitBox.collisionType = CollisionType.bodyCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox.radius = .9f;
														newHitBox.position = FindTransform("Spine");
													}else if (i == 3){
														newHitBox.bodyPart = BodyPart.leftUpperArm;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.high;
														newHitBox.radius = .5f;
														newHitBox.position = FindTransform("LeftArm");
													}else if (i == 4){
														newHitBox.bodyPart = BodyPart.rightUpperArm;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.high;
														newHitBox.radius = .5f;
														newHitBox.position = FindTransform("RightArm");
													}else if (i == 5){
														newHitBox.bodyPart = BodyPart.leftForearm;
														newHitBox.collisionType = CollisionType.noCollider;
														newHitBox.radius = .5f;
														newHitBox.position = FindTransform("LeftForeArm");
													}else if (i == 6){
														newHitBox.bodyPart = BodyPart.rightForearm;
														newHitBox.collisionType = CollisionType.noCollider;
														newHitBox.radius = .5f;
														newHitBox.position = FindTransform("RightForeArm");
													}else if (i == 7){
														newHitBox.bodyPart = BodyPart.leftHand;
														newHitBox.collisionType = CollisionType.noCollider;
														newHitBox.radius = .5f;
														newHitBox.position = FindTransform("LeftHand");
													}else if (i == 8){
														newHitBox.bodyPart = BodyPart.rightHand;
														newHitBox.collisionType = CollisionType.noCollider;
														newHitBox.radius = .5f;
														newHitBox.position = FindTransform("RightHand");
													}else if (i == 9){
														newHitBox.bodyPart = BodyPart.leftThigh;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox.radius = .5f;
														newHitBox.position = FindTransform("LeftUpLeg");
													}else if (i == 10){
														newHitBox.bodyPart = BodyPart.rightThigh;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox.radius = .5f;
														newHitBox.position = FindTransform("RightUpLeg");
													}else if (i == 11){
														newHitBox.bodyPart = BodyPart.leftCalf;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox.radius = .5f;
														newHitBox.position = FindTransform("LeftLeg");
													}else if (i == 12){
														newHitBox.bodyPart = BodyPart.rightCalf;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox.radius = .5f;
														newHitBox.position = FindTransform("RightLeg");
													}else if (i == 13){
														newHitBox.bodyPart = BodyPart.leftFoot;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox.radius = .5f;
														newHitBox.position = FindTransform("LeftFoot");
													}else if (i == 14){
														newHitBox.bodyPart = BodyPart.rightFoot;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox.radius = .5f;
														newHitBox.position = FindTransform("RightFoot");
													}
													hitBoxesScript.hitBoxes = AddElement<HitBox>(hitBoxesScript.hitBoxes, newHitBox);
												}
											}
										}
									}EditorGUI.EndDisabledGroup();
									
									hitBoxesToggle = EditorGUILayout.Foldout(hitBoxesToggle, "Hit Boxes", EditorStyles.foldout);
									if (hitBoxesToggle){
										EditorGUILayout.BeginVertical(subGroupStyle);{
											hitBoxesScript.collisionBoxSize = characterInfo.physics.groundCollisionMass;
											for (int i = 0; i < hitBoxesScript.hitBoxes.Length; i ++){
												EditorGUILayout.Space();
												EditorGUILayout.BeginVertical(subArrayElementStyle);{
													EditorGUILayout.Space();
													EditorGUILayout.BeginHorizontal();{
														hitBoxesScript.hitBoxes[i].bodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part:", hitBoxesScript.hitBoxes[i].bodyPart, enumStyle);
														if (GUILayout.Button("", "PaneOptions")){
															PaneOptions<HitBox>(hitBoxesScript.hitBoxes, hitBoxesScript.hitBoxes[i], delegate (HitBox[] newElement) { hitBoxesScript.hitBoxes = newElement; });
														}
													}EditorGUILayout.EndHorizontal();
													hitBoxesScript.hitBoxes[i].position = (Transform) EditorGUILayout.ObjectField("Link:", hitBoxesScript.hitBoxes[i].position, typeof(UnityEngine.Transform), true);

													hitBoxesScript.hitBoxes[i].shape = (HitBoxShape) EditorGUILayout.EnumPopup("Shape:", hitBoxesScript.hitBoxes[i].shape, enumStyle);
													if (hitBoxesScript.hitBoxes[i].shape == HitBoxShape.circle){
														hitBoxesScript.hitBoxes[i].radius = EditorGUILayout.Slider("Radius:", hitBoxesScript.hitBoxes[i].radius, .1f, 10);
														hitBoxesScript.hitBoxes[i].offSet = EditorGUILayout.Vector2Field("Off Set:", hitBoxesScript.hitBoxes[i].offSet);
													}else{
														hitBoxesScript.hitBoxes[i].rect = EditorGUILayout.RectField("Rectangle:", hitBoxesScript.hitBoxes[i].rect);
														
														EditorGUIUtility.labelWidth = 200;
														bool tmpFollowXBounds = hitBoxesScript.hitBoxes[i].followXBounds;
														bool tmpFollowYBounds = hitBoxesScript.hitBoxes[i].followYBounds;

														hitBoxesScript.hitBoxes[i].followXBounds = EditorGUILayout.Toggle("Follow Character Bounds (X)", hitBoxesScript.hitBoxes[i].followXBounds);
														hitBoxesScript.hitBoxes[i].followYBounds = EditorGUILayout.Toggle("Follow Character Bounds (Y)", hitBoxesScript.hitBoxes[i].followYBounds);

														if (tmpFollowXBounds != hitBoxesScript.hitBoxes[i].followXBounds)
															hitBoxesScript.hitBoxes[i].rect.width = hitBoxesScript.hitBoxes[i].followXBounds ? 0 : 4;
														if (tmpFollowYBounds != hitBoxesScript.hitBoxes[i].followYBounds)
															hitBoxesScript.hitBoxes[i].rect.height = hitBoxesScript.hitBoxes[i].followYBounds ? 0 : 4;

														EditorGUIUtility.labelWidth = 150;
													}

													hitBoxesScript.hitBoxes[i].collisionType = (CollisionType) EditorGUILayout.EnumPopup("Collision Type:", hitBoxesScript.hitBoxes[i].collisionType, enumStyle);
													EditorGUI.BeginDisabledGroup(hitBoxesScript.hitBoxes[i].collisionType == CollisionType.noCollider || hitBoxesScript.hitBoxes[i].collisionType == CollisionType.throwCollider);{
														hitBoxesScript.hitBoxes[i].type = (HitBoxType) EditorGUILayout.EnumPopup("Hit Box Type:", hitBoxesScript.hitBoxes[i].type, enumStyle);
													}EditorGUI.EndDisabledGroup();
													EditorGUILayout.Space();
												}EditorGUILayout.EndVertical();
											}
											if (StyledButton("New Hit Box"))
												hitBoxesScript.hitBoxes = AddElement<HitBox>(hitBoxesScript.hitBoxes, new HitBox());
											
										}EditorGUILayout.EndVertical();
									}
									
									EditorGUILayout.Space();
									EditorGUILayout.BeginHorizontal();{
										if (StyledButton("Reset Scene View")){
											EditorCamera.SetPosition(Vector3.up * 3.5f);
											EditorCamera.SetRotation(Quaternion.identity);
											EditorCamera.SetOrthographic(true);
											EditorCamera.SetSize(5);
										}
										if (StyledButton("Apply Changes")){
											PrefabUtility.ReplacePrefab(character, PrefabUtility.GetPrefabParent(character), ReplacePrefabOptions.ConnectToPrefab);
										}
									}EditorGUILayout.EndHorizontal();
									EditorGUILayout.Space();
								}EditorGUILayout.EndVertical();
							}
						}

						EditorGUI.indentLevel -= 1;
					}EditorGUILayout.EndVertical();
				}else{
					ClosePreview();
				}
			}EditorGUILayout.EndVertical();

			// Physics
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					physicsOption = EditorGUILayout.Foldout(physicsOption, "Physics", foldStyle);
					helpButton("character:physics");
				}EditorGUILayout.EndHorizontal();

				if (physicsOption){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						EditorGUIUtility.labelWidth = 170;
						SubGroupTitle("Horizontal Movement");
						characterInfo.physics.moveForwardSpeed = EditorGUILayout.FloatField("Move Forward Speed:", characterInfo.physics.moveForwardSpeed);
						characterInfo.physics.moveBackSpeed = EditorGUILayout.FloatField("Move Back Speed:", characterInfo.physics.moveBackSpeed);
						characterInfo.physics.highMovingFriction = EditorGUILayout.Toggle("High Moving Friction", characterInfo.physics.highMovingFriction);
						characterInfo.physics.friction = EditorGUILayout.FloatField("Friction:", characterInfo.physics.friction);
						EditorGUILayout.Space();

						SubGroupTitle("Jump Options");
						characterInfo.physics.canJump = EditorGUILayout.Toggle("Enable Jump", characterInfo.physics.canJump);
						EditorGUI.BeginDisabledGroup(!characterInfo.physics.canJump);{
							characterInfo.physics.jumpForce = EditorGUILayout.FloatField("Jump Force:", characterInfo.physics.jumpForce);
							characterInfo.physics.jumpDistance = EditorGUILayout.FloatField("Jump Distance:", characterInfo.physics.jumpDistance);
							characterInfo.physics.jumpDelay = EditorGUILayout.IntField("Jump Delay (frames):", characterInfo.physics.jumpDelay);
							characterInfo.physics.landingDelay = EditorGUILayout.IntField("Landing Delay (frames):", characterInfo.physics.landingDelay);
							characterInfo.physics.multiJumps = EditorGUILayout.IntField("Air Jumps:", characterInfo.physics.multiJumps);
						}EditorGUI.EndDisabledGroup();
						EditorGUILayout.Space();

						SubGroupTitle("Mass Variation");
						characterInfo.physics.weight = EditorGUILayout.FloatField("Character's Weight:", characterInfo.physics.weight);
						characterInfo.physics.groundCollisionMass = EditorGUILayout.FloatField("Ground Collision Mass:", characterInfo.physics.groundCollisionMass);
						characterInfo.physics.cumulativeForce = EditorGUILayout.Toggle("Cumulative Force", characterInfo.physics.cumulativeForce);
						EditorGUIUtility.labelWidth = 150;
						EditorGUILayout.Space();

						EditorGUI.indentLevel -= 1;
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

#if !UFE_BASIC
			// Head Look
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					headLookOption = EditorGUILayout.Foldout(headLookOption, "Head Look", foldStyle);
					helpButton("character:headlook");
				}EditorGUILayout.EndHorizontal();

				if (headLookOption){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;

						characterInfo.headLook.enabled = EditorGUILayout.Toggle("Enabled", characterInfo.headLook.enabled);
						
						EditorGUI.BeginDisabledGroup(!characterInfo.headLook.enabled);{
							bendingSegmentsToggle = EditorGUILayout.Foldout(bendingSegmentsToggle, "Bending Segments", EditorStyles.foldout);
							if (bendingSegmentsToggle){
								EditorGUILayout.BeginVertical(subGroupStyle);{
									for (int i = 0; i < characterInfo.headLook.segments.Length; i ++){
										EditorGUILayout.Space();
										EditorGUILayout.BeginVertical(subArrayElementStyle);{
											EditorGUILayout.Space();
											EditorGUILayout.BeginHorizontal();{
												characterInfo.headLook.segments[i].bodyPart = (BodyPart) EditorGUILayout.EnumPopup("Affected Body Part:", characterInfo.headLook.segments[i].bodyPart, enumStyle);
												if (GUILayout.Button("", "PaneOptions")){
													PaneOptions<BendingSegment>(characterInfo.headLook.segments, characterInfo.headLook.segments[i], delegate (BendingSegment[] newElement) { characterInfo.headLook.segments = newElement; });
												}
											}EditorGUILayout.EndHorizontal();
											EditorGUIUtility.labelWidth = 190;
											characterInfo.headLook.segments[i].responsiveness = EditorGUILayout.FloatField("Responsiveness:", characterInfo.headLook.segments[i].responsiveness);
											characterInfo.headLook.segments[i].thresholdAngleDifference = EditorGUILayout.FloatField("Threshold Angle Difference:", characterInfo.headLook.segments[i].thresholdAngleDifference);
											characterInfo.headLook.segments[i].bendingMultiplier = EditorGUILayout.FloatField("Bending Multiplier:", characterInfo.headLook.segments[i].bendingMultiplier);
											characterInfo.headLook.segments[i].maxAngleDifference = EditorGUILayout.FloatField("Max Angle Difference:", characterInfo.headLook.segments[i].maxAngleDifference);
											characterInfo.headLook.segments[i].maxBendingAngle = EditorGUILayout.FloatField("Max Bending Angle:", characterInfo.headLook.segments[i].maxBendingAngle);
											EditorGUIUtility.labelWidth = 150;
											EditorGUILayout.Space();
										}EditorGUILayout.EndVertical();
									}
									if (StyledButton("New Bending Segment"))
										characterInfo.headLook.segments = AddElement<BendingSegment>(characterInfo.headLook.segments, new BendingSegment());
									
								}EditorGUILayout.EndVertical();
							}
							
							nonAffectedJointsToggle = EditorGUILayout.Foldout(nonAffectedJointsToggle, "Non Affected Joints", EditorStyles.foldout);
							if (nonAffectedJointsToggle){
								EditorGUILayout.BeginVertical(subGroupStyle);{
									for (int i = 0; i < characterInfo.headLook.nonAffectedJoints.Length; i ++){
										EditorGUILayout.Space();
										EditorGUILayout.BeginVertical(subArrayElementStyle);{
											EditorGUILayout.Space();
											EditorGUILayout.BeginHorizontal();{
												characterInfo.headLook.nonAffectedJoints[i].bodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part:", characterInfo.headLook.nonAffectedJoints[i].bodyPart, enumStyle);
												if (GUILayout.Button("", "PaneOptions")){
													PaneOptions<NonAffectedJoints>(characterInfo.headLook.nonAffectedJoints, characterInfo.headLook.nonAffectedJoints[i], delegate (NonAffectedJoints[] newElement) { characterInfo.headLook.nonAffectedJoints = newElement; });
												}
											}EditorGUILayout.EndHorizontal();
											characterInfo.headLook.nonAffectedJoints[i].effect = EditorGUILayout.FloatField("Weight:", characterInfo.headLook.nonAffectedJoints[i].effect);
											EditorGUILayout.Space();
										}EditorGUILayout.EndVertical();
									}
									if (StyledButton("New Non Affected Joint"))
										characterInfo.headLook.nonAffectedJoints = AddElement<NonAffectedJoints>(characterInfo.headLook.nonAffectedJoints, new NonAffectedJoints());
									
								}EditorGUILayout.EndVertical();
							}

							characterInfo.headLook.effect = EditorGUILayout.FloatField("Global Weight:", characterInfo.headLook.effect);
							characterInfo.headLook.target = (BodyPart) EditorGUILayout.EnumPopup("Look At Opponent's:", characterInfo.headLook.target, enumStyle);
							characterInfo.headLook.overrideAnimation = EditorGUILayout.Toggle("Override Animation", characterInfo.headLook.overrideAnimation);
							characterInfo.headLook.disableOnHit = EditorGUILayout.Toggle("Disable On Hit", characterInfo.headLook.disableOnHit);

						}EditorGUI.EndDisabledGroup();

						EditorGUILayout.Space();
						EditorGUI.indentLevel -= 1;
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();
#endif

			// Move Sets
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					moveSetOption = EditorGUILayout.Foldout(moveSetOption, "Move Sets ("+ characterInfo.moves.Length +")", foldStyle);
					helpButton("character:movesets");
				}EditorGUILayout.EndHorizontal();

				if (moveSetOption){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						// content
                        EditorGUIUtility.labelWidth = 200;
						characterInfo.executionTiming = EditorGUILayout.FloatField("Execution Timing:", characterInfo.executionTiming);
						characterInfo.possibleAirMoves = EditorGUILayout.IntField("Possible Air Moves:", characterInfo.possibleAirMoves);
						characterInfo.blendingTime = EditorGUILayout.FloatField("Blending Duration:", characterInfo.blendingTime);
                        EditorGUIUtility.labelWidth = 150;

						EditorGUILayout.Space();

						characterInfo.animationType = (AnimationType)EditorGUILayout.EnumPopup("Animation Type:", characterInfo.animationType, enumStyle);
						if (characterInfo.animationType == AnimationType.Mecanim){
							characterInfo.avatar = (Avatar)EditorGUILayout.ObjectField("Avatar:", characterInfo.avatar, typeof(Avatar), false);
						}

						//EditorGUI.BeginDisabledGroup(characterInfo.animationType == AnimationType.Mecanim);{
							characterInfo.animationFlow = (AnimationFlow) EditorGUILayout.EnumPopup("Animation Control:", characterInfo.animationFlow, enumStyle);
						//}EditorGUI.EndDisabledGroup();

						//EditorGUI.indentLevel += 1;
						EditorGUILayout.Space();
						SubGroupTitle("Combat Stances");

						for (int i = 0; i < characterInfo.moves.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									characterInfo.moves[i].combatStance = (CombatStances)EditorGUILayout.EnumPopup("Stance Number:", characterInfo.moves[i].combatStance, enumStyle);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<MoveSetData>(characterInfo.moves, characterInfo.moves[i], delegate (MoveSetData[] newElement) { characterInfo.moves = newElement; });
									}
								}EditorGUILayout.EndHorizontal();

								
								characterInfo.moves[i].cinematicIntro = (MoveInfo)EditorGUILayout.ObjectField("Cinematic Intro:", characterInfo.moves[i].cinematicIntro, typeof(MoveInfo), false);
								characterInfo.moves[i].cinematicOutro = (MoveInfo)EditorGUILayout.ObjectField("Cinematic Outro:", characterInfo.moves[i].cinematicOutro, typeof(MoveInfo), false);

                                EditorGUILayout.Space();

								characterInfo.moves[i].basicMovesToggle = EditorGUILayout.Foldout(characterInfo.moves[i].basicMovesToggle, "Basic Moves", foldStyle);
								if (characterInfo.moves[i].basicMovesToggle){
									EditorGUILayout.BeginVertical(subGroupStyle);{
										//EditorGUI.indentLevel += 1;
										EditorGUILayout.Space();
                                        
                                        characterInfo.moves[i].enabledBasicMovesToggle = EditorGUILayout.Foldout(characterInfo.moves[i].enabledBasicMovesToggle, "Enabled Moves", foldStyle);
                                        if (characterInfo.moves[i].enabledBasicMovesToggle) {
                                            EditorGUILayout.BeginVertical(subArrayElementStyle);
                                            {
                                                EditorGUI.indentLevel += 1;
                                                EditorGUILayout.Space();
                                                characterInfo.moves[i].basicMoves.moveEnabled = EditorGUILayout.Toggle("Move", characterInfo.moves[i].basicMoves.moveEnabled);
                                                characterInfo.moves[i].basicMoves.jumpEnabled = EditorGUILayout.Toggle("Jump", characterInfo.moves[i].basicMoves.jumpEnabled);
                                                characterInfo.moves[i].basicMoves.crouchEnabled = EditorGUILayout.Toggle("Crouch", characterInfo.moves[i].basicMoves.crouchEnabled);
                                                characterInfo.moves[i].basicMoves.blockEnabled = EditorGUILayout.Toggle("Block", characterInfo.moves[i].basicMoves.blockEnabled);
                                                characterInfo.moves[i].basicMoves.parryEnabled = EditorGUILayout.Toggle("Parry", characterInfo.moves[i].basicMoves.parryEnabled);
                                                EditorGUILayout.Space();
                                                EditorGUI.indentLevel -= 1;
                                            }
                                            EditorGUILayout.EndVertical();
                                        }
                                        EditorGUILayout.Space();

										SubGroupTitle("Standard Animations");
                                        basicMoveBlock("Idle (*)", characterInfo.moves[i].basicMoves.idle, WrapMode.Loop, false, true, false, false);
                                        EditorGUI.BeginDisabledGroup(!characterInfo.moves[i].basicMoves.moveEnabled);{
                                            basicMoveBlock("Move Forward (*)", characterInfo.moves[i].basicMoves.moveForward, WrapMode.Loop, false, true, false, false);
                                            basicMoveBlock("Move Back (*)", characterInfo.moves[i].basicMoves.moveBack, WrapMode.Loop, false, true, false, false);
                                        } EditorGUI.EndDisabledGroup();
                                        EditorGUI.BeginDisabledGroup(!characterInfo.moves[i].basicMoves.crouchEnabled);{
                                            basicMoveBlock("Crouching (*)", characterInfo.moves[i].basicMoves.crouching, WrapMode.Loop, false, true, false, false);
                                        } EditorGUI.EndDisabledGroup();
                                        
                                        EditorGUI.BeginDisabledGroup(!characterInfo.moves[i].basicMoves.jumpEnabled);{
                                            SubGroupTitle("Jump Animations");
                                            basicMoveBlock("Take Off", characterInfo.moves[i].basicMoves.takeOff, WrapMode.ClampForever, true, true, false, true);
                                            basicMoveBlock("Jump Straight (*)", characterInfo.moves[i].basicMoves.jumpStraight, WrapMode.ClampForever, true, true, false, false);
                                            basicMoveBlock("Jump Back", characterInfo.moves[i].basicMoves.jumpBack, WrapMode.ClampForever, true, true, false, false);
                                            basicMoveBlock("Jump Forward", characterInfo.moves[i].basicMoves.jumpForward, WrapMode.ClampForever, true, true, false, false);
                                            basicMoveBlock("Fall Straight (*)", characterInfo.moves[i].basicMoves.fallStraight, WrapMode.ClampForever, true, true, false, false);
                                            basicMoveBlock("Fall Back", characterInfo.moves[i].basicMoves.fallBack, WrapMode.ClampForever, true, true, false, false);
                                            basicMoveBlock("Fall Forward", characterInfo.moves[i].basicMoves.fallForward, WrapMode.ClampForever, true, true, false, false);
                                            basicMoveBlock("Landing", characterInfo.moves[i].basicMoves.landing, WrapMode.Once, true, true, false, false);
                                        } EditorGUI.EndDisabledGroup();
                                        
                                        EditorGUI.BeginDisabledGroup(!characterInfo.moves[i].basicMoves.blockEnabled);{
                                            SubGroupTitle("Block Animations");
                                            basicMoveBlock("Standing Pose", characterInfo.moves[i].basicMoves.blockingHighPose, WrapMode.ClampForever, false, true, false, false);
                                            basicMoveBlock("Standing High Hit", characterInfo.moves[i].basicMoves.blockingHighHit, WrapMode.Once, true, true, true, false);
                                            basicMoveBlock("Standing Low Hit", characterInfo.moves[i].basicMoves.blockingLowHit, WrapMode.Once, true, true, true, false);
                                            basicMoveBlock("Crouching Pose", characterInfo.moves[i].basicMoves.blockingCrouchingPose, WrapMode.ClampForever, false, true, false, false);
                                            basicMoveBlock("Crouching Hit", characterInfo.moves[i].basicMoves.blockingCrouchingHit, WrapMode.Once, true, true, true, false);
                                            basicMoveBlock("Air Pose", characterInfo.moves[i].basicMoves.blockingAirPose, WrapMode.ClampForever, false, true, false, false);
                                            basicMoveBlock("Air Hit", characterInfo.moves[i].basicMoves.blockingAirHit, WrapMode.Once, true, true, true, false);
                                        } EditorGUI.EndDisabledGroup();
                                        
                                        EditorGUI.BeginDisabledGroup(!characterInfo.moves[i].basicMoves.parryEnabled);{
                                            SubGroupTitle("Parry Animations");
                                            basicMoveBlock("Standing High Hit", characterInfo.moves[i].basicMoves.parryHigh, WrapMode.Once, true, true, false, false);
                                            basicMoveBlock("Standing Low Hit", characterInfo.moves[i].basicMoves.parryLow, WrapMode.Once, true, true, false, false);
                                            basicMoveBlock("Crouching Hit", characterInfo.moves[i].basicMoves.parryCrouching, WrapMode.Once, true, true, false, false);
                                            basicMoveBlock("Air Hit", characterInfo.moves[i].basicMoves.parryAir, WrapMode.Once, true, true, false, false);
                                        } EditorGUI.EndDisabledGroup();

                                        SubGroupTitle("Hit Animations");
                                        basicMoveBlock("Standing High Hit (*)", characterInfo.moves[i].basicMoves.getHitHigh, WrapMode.Once, true, true, true, true);
                                        basicMoveBlock("Standing Low Hit", characterInfo.moves[i].basicMoves.getHitLow, WrapMode.Once, true, true, true, true);
                                        EditorGUI.BeginDisabledGroup(!characterInfo.moves[i].basicMoves.crouchEnabled);{
                                            basicMoveBlock("Crouching Hit (*)", characterInfo.moves[i].basicMoves.getHitCrouching, WrapMode.Once, true, true, true, true);
                                        } EditorGUI.EndDisabledGroup();
                                        basicMoveBlock("Standing High Hit (Knockdown)", characterInfo.moves[i].basicMoves.getHitHighKnockdown, WrapMode.ClampForever, false, true, false, true);
                                        basicMoveBlock("Standing Low Hit (Knockdown)", characterInfo.moves[i].basicMoves.getHitHighLowKnockdown, WrapMode.ClampForever, false, true, false, true);
                                        basicMoveBlock("Sweep (Knockdown)", characterInfo.moves[i].basicMoves.getHitSweep, WrapMode.ClampForever, false, true, false, true);
                                        basicMoveBlock("Crumple (Knockdown)", characterInfo.moves[i].basicMoves.getHitCrumple, WrapMode.ClampForever, true, true, false, true);
                                        basicMoveBlock("Knock Back (Knockdown)", characterInfo.moves[i].basicMoves.getHitKnockBack, WrapMode.ClampForever, true, true, false, true);
                                        basicMoveBlock("Air Hit/Juggle (Knockdown)", characterInfo.moves[i].basicMoves.getHitAir, WrapMode.ClampForever, true, true, false, true);

                                        SubGroupTitle("Fall & Recover Animations");
                                        basicMoveBlock("Down (*)", characterInfo.moves[i].basicMoves.fallDown, WrapMode.ClampForever, false, true, false, false);
                                        basicMoveBlock("Ground Bounce", characterInfo.moves[i].basicMoves.bounce, WrapMode.ClampForever, false, true, false, false);
                                        basicMoveBlock("Falling (After Ground Bounce)", characterInfo.moves[i].basicMoves.fallingFromBounce, WrapMode.ClampForever, true, true, false, true);
                                        basicMoveBlock("Stand Up (*)", characterInfo.moves[i].basicMoves.standUp, WrapMode.ClampForever, true, true, false, false);

										//EditorGUI.indentLevel -= 1;
										EditorGUILayout.Space();

                                        GUILayout.Label("* Required", "MiniBoldLabel");
										
									}EditorGUILayout.EndVertical();
								}

								characterInfo.moves[i].attackMovesToggle = EditorGUILayout.Foldout(characterInfo.moves[i].attackMovesToggle, "Attack & Special Moves ("+ characterInfo.moves[i].attackMoves.Length +")", foldStyle);
								if (characterInfo.moves[i].attackMovesToggle){
									EditorGUILayout.BeginVertical(subGroupStyle);{
										EditorGUILayout.Space();
										EditorGUI.indentLevel += 1;
										
										for (int y = 0; y < characterInfo.moves[i].attackMoves.Length; y ++){
											EditorGUILayout.Space();
											EditorGUILayout.BeginVertical(subArrayElementStyle);{
												EditorGUILayout.Space();
												EditorGUIUtility.labelWidth = 120;
												EditorGUILayout.BeginHorizontal();{
													characterInfo.moves[i].attackMoves[y] = (MoveInfo)EditorGUILayout.ObjectField("Move File:", characterInfo.moves[i].attackMoves[y], typeof(MoveInfo), false);
													if (GUILayout.Button("", "PaneOptions")){
														PaneOptions<MoveInfo>(characterInfo.moves[i].attackMoves, characterInfo.moves[i].attackMoves[y], delegate (MoveInfo[] newElement) { characterInfo.moves[i].attackMoves = newElement; });
													}
												}EditorGUILayout.EndHorizontal();
												EditorGUIUtility.labelWidth = 150;
												
												if (GUILayout.Button("Open in the Move Editor")) {
													MoveEditorWindow.sentMoveInfo = characterInfo.moves[i].attackMoves[y];
													MoveEditorWindow.Init();
												}
											}EditorGUILayout.EndVertical();
										}
										EditorGUILayout.Space();
										if (StyledButton("New Move"))
											characterInfo.moves[i].attackMoves = AddElement<MoveInfo>(characterInfo.moves[i].attackMoves, null);

										EditorGUILayout.Space();
										EditorGUI.indentLevel -= 1;
									}EditorGUILayout.EndVertical();
								}
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
						EditorGUILayout.Space();
						if (StyledButton("New Combat Stance"))
							characterInfo.moves = AddElement<MoveSetData>(characterInfo.moves, new MoveSetData());
						
						EditorGUILayout.Space();
						EditorGUI.indentLevel -= 1;

					}EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
			
			// AI Instructions
			if (UFE.isAiAddonInstalled){
				EditorGUILayout.BeginVertical(rootGroupStyle);{
					EditorGUILayout.BeginHorizontal();{
						aiInstructionsOption = EditorGUILayout.Foldout(aiInstructionsOption, "AI Instructions ("+ characterInfo.aiInstructionsSet.Length +")", foldStyle);
						helpButton("character:aiinstructions");
					}EditorGUILayout.EndHorizontal();
					
					if (aiInstructionsOption){
						EditorGUILayout.BeginVertical(subGroupStyle);{
							EditorGUILayout.Space();
							EditorGUI.indentLevel += 1;
							
							for (int i = 0; i < characterInfo.aiInstructionsSet.Length; i ++){
								EditorGUILayout.Space();
								EditorGUILayout.BeginVertical(arrayElementStyle);{
									EditorGUILayout.Space();
									EditorGUILayout.BeginHorizontal();{
										characterInfo.aiInstructionsSet[i].behavior = (AIBehavior) EditorGUILayout.EnumPopup("Behavior:", characterInfo.aiInstructionsSet[i].behavior, enumStyle);
										if (GUILayout.Button("", "PaneOptions")){
											PaneOptions<AIInstructionsSet>(characterInfo.aiInstructionsSet, characterInfo.aiInstructionsSet[i], delegate (AIInstructionsSet[] newElement) { characterInfo.aiInstructionsSet = newElement; });
										}
									}EditorGUILayout.EndHorizontal();
									characterInfo.aiInstructionsSet[i].aiInfo = (ScriptableObject)EditorGUILayout.ObjectField("Instructions File:", characterInfo.aiInstructionsSet[i].aiInfo, typeof(ScriptableObject), false);
									EditorGUILayout.Space();
								}EditorGUILayout.EndVertical();
								EditorGUILayout.Space();
							}
							
							EditorGUI.indentLevel -= 1;
							
							if (StyledButton("New Instruction"))
								characterInfo.aiInstructionsSet = AddElement<AIInstructionsSet>(characterInfo.aiInstructionsSet, new AIInstructionsSet());
							
						}EditorGUILayout.EndVertical();
					}
				}EditorGUILayout.EndVertical();
			}

		}EditorGUILayout.EndScrollView();


		if (GUI.changed) {
			Undo.RecordObject(characterInfo, "Character Editor Modify");
			EditorUtility.SetDirty(characterInfo);
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

	public void basicMoveBlock(string label, BasicMoveInfo basicMove, WrapMode wrapMode, bool autoSpeed, bool hasSound, bool hasHitStrength, bool invincible){
		basicMove.editorToggle = EditorGUILayout.Foldout(basicMove.editorToggle, label, foldStyle);

        //GUIStyle foldoutStyle;
        //foldoutStyle = new GUIStyle(EditorStyles.foldout);
        //foldoutStyle.normal.textColor = Color.cyan;
        //basicMove.editorToggle = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), basicMove.editorToggle, label, true, foldoutStyle);
		
        if (basicMove.editorToggle){
            EditorGUILayout.BeginVertical(subArrayElementStyle);
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel += 1;
                EditorGUIUtility.labelWidth = 180;
                if (hasHitStrength) {
                    string required = label.IndexOf("*") != -1 ? " (*)" : "";
                    basicMove.clip1 = (AnimationClip)EditorGUILayout.ObjectField("Weak Hit Clip"+ required +":", basicMove.clip1, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip2 = (AnimationClip)EditorGUILayout.ObjectField("Medium Hit Clip:", basicMove.clip2, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip3 = (AnimationClip)EditorGUILayout.ObjectField("Heavy Hit Clip:", basicMove.clip3, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip4 = (AnimationClip)EditorGUILayout.ObjectField("Custom 1 Hit Clip:", basicMove.clip4, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip5 = (AnimationClip)EditorGUILayout.ObjectField("Custom 2 Hit Clip:", basicMove.clip5, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip6 = (AnimationClip)EditorGUILayout.ObjectField("Custom 3 Hit Clip:", basicMove.clip6, typeof(UnityEngine.AnimationClip), false);
                } else if (label == "Idle (*)") {
                    basicMove.clip1 = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip (*):", basicMove.clip1, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip2 = (AnimationClip)EditorGUILayout.ObjectField("Resting Clip 1:", basicMove.clip2, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip3 = (AnimationClip)EditorGUILayout.ObjectField("Resting Clip 2:", basicMove.clip3, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip4 = (AnimationClip)EditorGUILayout.ObjectField("Resting Clip 3:", basicMove.clip4, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip5 = (AnimationClip)EditorGUILayout.ObjectField("Resting Clip 4:", basicMove.clip5, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip6 = (AnimationClip)EditorGUILayout.ObjectField("Resting Clip 5:", basicMove.clip6, typeof(UnityEngine.AnimationClip), false);
                    basicMove.restingClipInterval = EditorGUILayout.FloatField("Resting Interval:", basicMove.restingClipInterval);
                } else if (label == "Stand Up (*)") {
                    basicMove.clip1 = (AnimationClip)EditorGUILayout.ObjectField("Default Clip (*):", basicMove.clip1, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip2 = (AnimationClip)EditorGUILayout.ObjectField("High Knockdown Clip:", basicMove.clip2, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip3 = (AnimationClip)EditorGUILayout.ObjectField("Low Knockdown Clip:", basicMove.clip3, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip4 = (AnimationClip)EditorGUILayout.ObjectField("Sweep Clip:", basicMove.clip4, typeof(UnityEngine.AnimationClip), false);
                    basicMove.clip5 = (AnimationClip)EditorGUILayout.ObjectField("Crumple Clip:", basicMove.clip5, typeof(UnityEngine.AnimationClip), false);
                } else {
                    basicMove.clip1 = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip:", basicMove.clip1, typeof(UnityEngine.AnimationClip), false);
                }

                if (autoSpeed) {
                    basicMove.autoSpeed = EditorGUILayout.Toggle("Auto Speed", basicMove.autoSpeed, toggleStyle);
                } else {
                    basicMove.autoSpeed = false;
                }

                if (basicMove.autoSpeed) {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField("Animation Speed:", "(Automatic)");
                    EditorGUI.EndDisabledGroup();
                } else {
                    basicMove.animationSpeed = EditorGUILayout.FloatField("Animation Speed:", basicMove.animationSpeed);
                }

                EditorGUILayout.BeginHorizontal();
                {
                    basicMove.wrapMode = (WrapMode)EditorGUILayout.EnumPopup("Wrap Mode:", basicMove.wrapMode, enumStyle);
                    if (basicMove.wrapMode == WrapMode.Default) basicMove.wrapMode = wrapMode;
                    if (GUILayout.Button("Default", "minibutton", GUILayout.Width(60))) basicMove.wrapMode = wrapMode;

                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                EditorGUILayout.Space();

                basicMove.overrideBlendingIn = EditorGUILayout.Toggle("Override Blending (In)", basicMove.overrideBlendingIn, toggleStyle);
                if (basicMove.overrideBlendingIn) {
                    basicMove.blendingIn = EditorGUILayout.FloatField("Blend In Duration:", basicMove.blendingIn);
                }

                basicMove.overrideBlendingOut = EditorGUILayout.Toggle("Override Blending (Out)", basicMove.overrideBlendingOut, toggleStyle);
                if (basicMove.overrideBlendingOut) {
                    basicMove.blendingOut = EditorGUILayout.FloatField("Blend Out Duration:", basicMove.blendingOut);
                }

                if (invincible) basicMove.invincible = EditorGUILayout.Toggle("Hide hitboxes", basicMove.invincible, toggleStyle);

                basicMove.disableHeadLook = EditorGUILayout.Toggle("Disalbe Head Look", basicMove.disableHeadLook, toggleStyle);

                EditorGUILayout.Space();
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                EditorGUILayout.Space();

                basicMove.particleEffect.editorToggle = EditorGUILayout.Foldout(basicMove.particleEffect.editorToggle, "Particle Effect", foldStyle);
                if (basicMove.particleEffect.editorToggle) {
                    EditorGUILayout.BeginVertical(subGroupStyle);
                    {
                        EditorGUILayout.Space();
                        basicMove.particleEffect.prefab = (GameObject)EditorGUILayout.ObjectField("Particle Prefab:", basicMove.particleEffect.prefab, typeof(UnityEngine.GameObject), true);
                        basicMove.particleEffect.duration = EditorGUILayout.FloatField("Duration (seconds):", basicMove.particleEffect.duration);
                        basicMove.particleEffect.stick = EditorGUILayout.Toggle("Sticky", basicMove.particleEffect.stick, toggleStyle);
                        basicMove.particleEffect.bodyPart = (BodyPart)EditorGUILayout.EnumPopup("Body Part:", basicMove.particleEffect.bodyPart, enumStyle);
                        basicMove.particleEffect.offSet = EditorGUILayout.Vector3Field("Off Set (relative):", basicMove.particleEffect.offSet);

                        EditorGUILayout.Space();
                    } EditorGUILayout.EndVertical();
                }
                if (hasSound) {
                    basicMove.soundEffectsToggle = EditorGUILayout.Foldout(basicMove.soundEffectsToggle, "Possible Sound Effects (" + basicMove.soundEffects.Length + ")", EditorStyles.foldout);
                    if (basicMove.soundEffectsToggle) {
                        EditorGUILayout.BeginVertical(subGroupStyle);
                        {
                            basicMove.continuousSound = EditorGUILayout.Toggle("Continuous Sound", basicMove.continuousSound, toggleStyle);
                            EditorGUILayout.Space();

                            EditorGUIUtility.labelWidth = 150;
                            for (int i = 0; i < basicMove.soundEffects.Length; i++) {
                                EditorGUILayout.Space();
                                EditorGUILayout.BeginVertical(subArrayElementStyle);
                                {
                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        basicMove.soundEffects[i] = (AudioClip)EditorGUILayout.ObjectField("Audio Clip:", basicMove.soundEffects[i], typeof(UnityEngine.AudioClip), true);
                                        if (GUILayout.Button("", "PaneOptions")) {
                                            PaneOptions<AudioClip>(basicMove.soundEffects, basicMove.soundEffects[i], delegate(AudioClip[] newElement) { basicMove.soundEffects = newElement; });
                                        }
                                    } EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.Space();
                                } EditorGUILayout.EndVertical();
                            }
                            if (StyledButton("New Sound Effect"))
                                basicMove.soundEffects = AddElement<AudioClip>(basicMove.soundEffects, new AudioClip());

                        } EditorGUILayout.EndVertical();
                    }
                }

                EditorGUI.indentLevel -= 1;
                EditorGUILayout.Space();

            } EditorGUILayout.EndVertical();
		}
	}

	public Transform FindTransform(string searchString){
		if (character == null) return null;
		Transform[] transformChildren = character.GetComponentsInChildren<Transform>();
		Transform found;
		foreach(Transform transformChild in transformChildren){
			found = transformChild.Find("mixamorig:"+ searchString);
			if (found == null) found = transformChild.Find(character.name + ":" + searchString);
			if (found == null) found = transformChild.Find(searchString);
			if (found != null) return found;
		}
		return null;
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