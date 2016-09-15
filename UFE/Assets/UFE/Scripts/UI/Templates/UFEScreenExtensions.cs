using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UFEScreenExtensions{
	public static Selectable FindFirstSelectable(this UFEScreen screen){
		List<Selectable> selectables = Selectable.allSelectables;
		Transform firstSelectableTransform = null;
		Selectable firstSelectable = null;

		for (int i = 0; i < selectables.Count; ++i){
			Selectable currentSelectable = selectables[i];

			if(
				currentSelectable != null && 
				currentSelectable.gameObject.activeInHierarchy && 
				currentSelectable.IsInteractable()
			){
				Transform currentTransform = currentSelectable.transform;

				if(
					firstSelectable == null ||
					firstSelectableTransform == null ||
					currentTransform.position.y > firstSelectableTransform.position.y ||
					(
						currentTransform.position.y == firstSelectableTransform.position.y &&
						currentTransform.position.x < firstSelectableTransform.position.x
					)
				){
					firstSelectable = currentSelectable;
					firstSelectableTransform = currentTransform;
				}
			}
		}

		return firstSelectable;
	}

	public static GameObject FindFirstSelectableGameObject(this UFEScreen screen){
		Selectable selectable = screen.FindFirstSelectable();
		return selectable != null ? selectable.gameObject : null;
	}

	public static void DefaultNavigationSystem(
		this UFEScreen screen, 
		AudioClip selectSound = null,
		AudioClip moveCursorSound = null,
		Action cancelAction = null,
		AudioClip cancelSound = null
	){
		// Retrieve the controller assigned to each player
		AbstractInputController p1InputController = UFE.GetPlayer1Controller();
		AbstractInputController p2InputController = UFE.GetPlayer2Controller();

		// Retrieve the values of the horizontal and vertical axis
		float p1HorizontalAxis = p1InputController.GetAxisRaw(p1InputController.horizontalAxis);
		float p1VerticalAxis = p1InputController.GetAxisRaw(p1InputController.verticalAxis);
		bool p1AxisDown = 
			p1InputController.GetButtonDown(p1InputController.horizontalAxis) ||
			p1InputController.GetButtonDown(p1InputController.verticalAxis);

		float p2HorizontalAxis = p2InputController.GetAxisRaw(p2InputController.horizontalAxis);
		float p2VerticalAxis = p2InputController.GetAxisRaw(p2InputController.verticalAxis);
		bool p2AxisDown = 
			p2InputController.GetButtonDown(p2InputController.horizontalAxis) ||
			p2InputController.GetButtonDown(p2InputController.verticalAxis);

		// Check if we should change the selected option
		if (p1AxisDown){
			screen.MoveCursor(new Vector3(p1HorizontalAxis, p1VerticalAxis), moveCursorSound);
		}

		if (p1InputController.GetButtonDown(UFE.config.inputOptions.confirmButton)){
			screen.SelectOption(selectSound);
		}else if (p1InputController.GetButtonDown(UFE.config.inputOptions.cancelButton)){
			if (cancelSound != null)UFE.PlaySound(cancelSound);
			if (cancelAction != null) cancelAction();
		}else{
			if (p2AxisDown){
				screen.MoveCursor(new Vector3(p2HorizontalAxis, p2VerticalAxis), moveCursorSound);
			}

			if (p2InputController.GetButtonDown(UFE.config.inputOptions.confirmButton)){
				screen.SelectOption(selectSound);
			}else if (p2InputController.GetButtonDown(UFE.config.inputOptions.cancelButton)){
				if (cancelSound != null)UFE.PlaySound(cancelSound);
				if (cancelAction != null)cancelAction();
			}
		}
	}

	public static void HighlightOption(this UFEScreen screen, Selectable option, BaseEventData pointer = null){
		screen.HighlightOption(option != null ? option.gameObject : null, pointer);
	}

	public static void HighlightOption(this UFEScreen screen, GameObject option, BaseEventData pointer = null){
		UFE.eventSystem.SetSelectedGameObject(option, pointer);
	}

	public static void MoveCursor(this UFEScreen screen, Vector3 direction, AudioClip moveCursorSound = null){
		GameObject currentGameObject = UFE.eventSystem.currentSelectedGameObject;
		GameObject nextGameObject = null;

		if (currentGameObject != null && currentGameObject.activeInHierarchy){
			Selectable currentSelectableObject = currentGameObject.GetComponent<Selectable>();
			
			if (currentSelectableObject != null && currentSelectableObject.IsInteractable()){
				Selectable nextSelectableObject = currentSelectableObject.FindSelectable(direction);
				
				if (nextSelectableObject != null){
					nextGameObject = nextSelectableObject.gameObject;
				}
			}
		}

		if (nextGameObject == null){
			nextGameObject = screen.FindFirstSelectableGameObject();
		}
		
		if (currentGameObject != nextGameObject){
			if (moveCursorSound != null) UFE.PlaySound(moveCursorSound);
			screen.HighlightOption(nextGameObject);
		}
	}
	
	public static void SelectOption(this UFEScreen screen, AudioClip selectSound = null){
		// Retrieve the current selected object...
		GameObject currentGameObject = UFE.eventSystem.currentSelectedGameObject;
		if (currentGameObject != null){
			// Check if it's a button...
			Button currentButton = currentGameObject.GetComponent<Button>();
			if (currentButton != null){
				// In that case, raise the "On Click" event
				if (currentButton.onClick != null){
					if (selectSound != null) UFE.PlaySound(selectSound);
					currentButton.onClick.Invoke();
				}
			}else{
				// Otherwise, check if it's a toggle...
				Toggle currentToggle = currentGameObject.GetComponent<Toggle>();
				if (currentToggle != null){
					// In that case, change the state of the toggle...
					currentToggle.isOn = !currentToggle.isOn;
				}
			}
		}else{
			currentGameObject = screen.FindFirstSelectableGameObject();
			if (selectSound != null) UFE.PlaySound(selectSound);
			screen.HighlightOption(currentGameObject);
		}
	}
}
