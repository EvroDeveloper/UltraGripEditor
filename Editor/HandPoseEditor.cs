using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLZ.Marrow;
using SLZ.Marrow.Utilities;
using System;
using UnityEditor.Overlays;
using UnityEditor;
using UnityEngine.UIElements;
using UltraGripEditor;

using static GripVisualizer;

[Overlay(typeof(SceneView), id: ID_OVERLAY_HANDPOSEEDITOR, displayName: "Hand Pose Editor")]
public class HandPoseEditorOverlay : Overlay, ITransientOverlay
{
    private const string ID_OVERLAY_HANDPOSEEDITOR = "hand-pose-editor-overlay";
    private string VISUALTREE_PATH = AssetDatabase.GUIDToAssetPath("2859e1c9b07c64a178cb6eba348ff0bf");
    private string VISUALIZER_PATH = AssetDatabase.GUIDToAssetPath("bf692f29001bd42aab4da2220b211fb0");
    private TargetGrip lastSelectedTargetGrip;
    private GameObject visualizerObject;
    private GripVisualizer visualizer;

    private int overlayMode = 0;
    private SelectedHand selectedHand = SelectedHand.Left;
    private int radiusIndex = 0;
    private int pryIndex = 0;
    private VisualElement visualizerGroup;
    private SliderInt radiusSlider;
    private SliderInt prySlider;

    private bool _lastVisible = false;
    public bool visible
    {
        get
        {
            if (Selection.activeGameObject != null)
            {
                TargetGrip potentialGrip = Selection.activeGameObject.GetComponent<TargetGrip>();
                if (potentialGrip != null && potentialGrip.handPose != null)
                {
                    if(!_lastVisible || potentialGrip != lastSelectedTargetGrip)
                    {
                        lastSelectedTargetGrip = potentialGrip;
                        OnGripSelected(lastSelectedTargetGrip);
                    }
                    OnUpdated(true);
                    _lastVisible = true;
                    return true;
                }
            }
            OnUpdated(false);
            if(_lastVisible) OnGripDeselected();
            _lastVisible = false;
            return false;
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if(visualizer == null) return;
        if(visualizer.targetGrip == null) return;
        if(overlayMode == 2) DrawEditorHandles();
    }

    void DrawEditorHandles()
    {
        Vector3 position = visualizer.viewingHandReferences.hand.position;
        Quaternion rotation = visualizer.viewingHandReferences.hand.rotation;
        Handles.TransformHandle(ref position, ref rotation);
        HandleConversion.HandleConfiguration resultingHandle = HandleConversion.WorldToGripHandle(visualizer.targetGrip, SimpleTransform.Create(position, rotation), visualizer.viewingHand);
        if(visualizer.viewingHand == SelectedHand.Left)
        {
            visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray[pryIndex].leftHandle = resultingHandle.handle;
            visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray[pryIndex].invLeftHandle = resultingHandle.invHandle;
            visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray[pryIndex].leftArtHandle = resultingHandle.artHandle;
        }
        else
        {
            visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray[pryIndex].rightHandle = resultingHandle.handle;
            visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray[pryIndex].invRightHandle = resultingHandle.invHandle;
            visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray[pryIndex].rightArtHandle = resultingHandle.artHandle;
        }
    }

    private void OnGripSelected(TargetGrip grip)
    {
        if(visualizerObject == null) CreateVisualizer();
        visualizer.targetGrip = grip;

        radiusIndex = Math.Clamp(radiusIndex, 0, visualizer.targetGrip.handPose.poseData.Length - 1);
        pryIndex = Math.Clamp(pryIndex, 0, visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray.Length - 1);

        if(radiusSlider != null)
        {
            radiusSlider.highValue = visualizer.targetGrip.handPose.poseData.Length - 1;
            radiusSlider.value = radiusIndex;
        }
        if(prySlider != null)
        {
            prySlider.highValue = visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray.Length - 1;
            prySlider.value = pryIndex;
        }
    }

    private void OnGripDeselected()
    {
        visualizer.targetGrip = null;
        visualizer.UpdateFingers();
    }

    private void OnUpdated(bool isGripSelected)
    {
        if(isGripSelected) visualizer.UpdateFingers();
    }

    public override VisualElement CreatePanelContent()
    {
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(VISUALTREE_PATH);
        VisualElement tree = visualTree.Instantiate();
        VisualElement rootVisualElement = new VisualElement();

        DropdownField modeDropdown = tree.Q<DropdownField>("ModeDropdown");
        modeDropdown.RegisterCallback<ChangeEvent<string>>(evt =>
        {
           overlayMode = modeDropdown.index;
           visualizer.show = overlayMode != 0;
           UpdatePanelContent();
        });
        modeDropdown.index = overlayMode;

        visualizerGroup = tree.Q<VisualElement>("VisualizeMode");
        
        DropdownField visHandDropdown = tree.Q<DropdownField>("VisualizeHandDropdown");
        visHandDropdown.RegisterCallback<ChangeEvent<string>>(evt =>
        {
            selectedHand = (SelectedHand)visHandDropdown.index;
            visualizer.viewingHand = selectedHand;
        });
        visHandDropdown.index = (int)selectedHand;

        prySlider = tree.Q<SliderInt>("VisualizePoseSlider");
        Label pryLabel = tree.Q<Label>("VisualizePryVector");
        prySlider.RegisterCallback<ChangeEvent<int>>(evt =>
        {
            pryIndex = evt.newValue;
            pryLabel.text = FormatVectorInts(visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray[pryIndex].nativePry);
            SetVisualizerPry(pryIndex);
        });
        prySlider.highValue = visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray.Length - 1;
        prySlider.value = pryIndex;
        pryLabel.text = FormatVectorInts(visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray[pryIndex].nativePry);

        radiusSlider = tree.Q<SliderInt>("VisualizeRadiusSlider");
        Label radiusLabel = tree.Q<Label>("VisualizeRadiusNumber");
        radiusSlider.RegisterCallback<ChangeEvent<int>>(evt =>
        {
            radiusIndex = evt.newValue;
            radiusLabel.text = $"{visualizer.targetGrip.handPose.poseData[radiusIndex].radius}";
            SetVisualizerRadius(radiusIndex);
            if(prySlider != null)
            {
                prySlider.highValue = visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray.Length - 1;
                pryLabel.text = FormatVectorInts(visualizer.targetGrip.handPose.poseData[radiusIndex].poseArray[pryIndex].nativePry);
            }
        });
        radiusSlider.highValue = visualizer.targetGrip.handPose.poseData.Length - 1;
        radiusSlider.value = radiusIndex;
        radiusLabel.text = $"{visualizer.targetGrip.handPose.poseData[radiusIndex].radius}";

        
        UpdatePanelContent();

        rootVisualElement.Add(tree);
        return rootVisualElement;
    }

    void UpdatePanelContent()
    {
        if (overlayMode == 0)
        {
            visualizerGroup.style.display = DisplayStyle.None;
        }
        else if (overlayMode == 1)
        {
            visualizerGroup.style.display = DisplayStyle.Flex;
        }
        else if (overlayMode == 2)
        {
            visualizerGroup.style.display = DisplayStyle.None;
        }
    }

    public override void OnCreated()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        base.OnCreated();
    }

    void CreateVisualizer()
    {
        if(visualizerObject != null) GameObject.DestroyImmediate(visualizerObject);
        GameObject visualizerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VISUALIZER_PATH);
        visualizerObject = GameObject.Instantiate(visualizerPrefab);
        visualizerObject.hideFlags = /*HideFlags.HideInHierarchy |*/ HideFlags.DontSave;
        visualizer = visualizerObject.GetComponent<GripVisualizer>();
        visualizer.show = overlayMode != 0;
        visualizer.viewingHand = selectedHand;
        SetVisualizerRadius(radiusIndex);
        SetVisualizerPry(pryIndex);
    }

    void DestroyVisualizer()
    {
        if(visualizerObject != null)
            GameObject.DestroyImmediate(visualizerObject);
    }

    void SetVisualizerRadius(int index)
    {
        if(visualizer == null) return;

        visualizer.radiusIndex = index;
        visualizer.UpdateFingers();
    }

    void SetVisualizerPry(int index)
    {
        if(visualizer == null) return;

        visualizer.pryIndex = index;
        visualizer.UpdateFingers();
    }

    public override void OnWillBeDestroyed()
    {
        DestroyVisualizer();
        base.OnWillBeDestroyed();
    }

    private string FormatVectorInts(Vector3 vec)
    {
        return $"{(int)vec.x}, {(int)vec.y}, {(int)vec.z}";
    }
}