using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(VideoIntroScreen))]
public class VideoFileEditor : Editor {

	public override void OnInspectorGUI(){
        VideoIntroScreen videoIntroScreen = (VideoIntroScreen)target;


        EditorGUIUtility.labelWidth = 140;
        
        videoIntroScreen.videoPlatform = (VideoIntroScreen.VideoPlatform)EditorGUILayout.EnumPopup("File Location:", videoIntroScreen.videoPlatform, "MiniPopup");

        if (videoIntroScreen.videoPlatform == VideoIntroScreen.VideoPlatform.StreamingAssetsFolder
            || videoIntroScreen.videoPlatform == VideoIntroScreen.VideoPlatform.OtherFolders) {
            videoIntroScreen.otherPlatformsPath = EditorGUILayout.TextField("File Name:", videoIntroScreen.otherPlatformsPath);
        } else if (videoIntroScreen.videoPlatform == VideoIntroScreen.VideoPlatform.Web) {
            videoIntroScreen.otherPlatformsPath = EditorGUILayout.TextField("File URL:", videoIntroScreen.otherPlatformsPath);
        }

        videoIntroScreen.mobilePlatformsPath = EditorGUILayout.TextField("Mobile:", videoIntroScreen.mobilePlatformsPath);

        EditorGUILayout.Space();
        EditorGUIUtility.labelWidth = 170;
        videoIntroScreen.skippable = EditorGUILayout.Toggle("Skippable", videoIntroScreen.skippable);
        videoIntroScreen.stopMusicAfterSkippingVideo = EditorGUILayout.Toggle("Stop Music After Skipping", videoIntroScreen.stopMusicAfterSkippingVideo);


        videoIntroScreen.videoScaleMode = (VideoIntroScreen.VideoScaleMode)EditorGUILayout.EnumPopup("Scale Mode:", videoIntroScreen.videoScaleMode, "MiniPopup");
        if (videoIntroScreen.videoScaleMode == VideoIntroScreen.VideoScaleMode.Custom) {
            videoIntroScreen.customRect = EditorGUILayout.RectField(videoIntroScreen.customRect);
        }

        videoIntroScreen.delayBeforePlayingVideo = EditorGUILayout.FloatField("Delay Before Playing:", videoIntroScreen.delayBeforePlayingVideo);
        videoIntroScreen.delayAfterSkippingVideo = EditorGUILayout.FloatField("Delay After Skipping:", videoIntroScreen.delayAfterSkippingVideo);

        //videoIntroScreen.guiTextureColor = EditorGUILayout.ColorField("Background Color:", videoIntroScreen.guiTextureColor);
        
        EditorUtility.SetDirty(videoIntroScreen);
	}
}
