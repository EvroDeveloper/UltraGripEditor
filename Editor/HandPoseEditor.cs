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
    private VisualElement editorGroup;
    private VisualElement editOff;
    private VisualElement editOn;
    private SliderInt radiusSlider;
    private SliderInt prySlider;
    private PoseDataReference currentEditingPose;
    private Transform currentEditingBone;

    private Stack<HandPose.PoseData> editingPoseUndoStack = new Stack<HandPose.PoseData>();
    private Stack<HandPose.PoseData> editingPoseRedoStack = new Stack<HandPose.PoseData>();

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
        if(currentEditingPose != null)
        {
            Transform[] transforms = visualizer.viewingHandReferences.MoveableBoneList;

            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if(editingPoseUndoStack.Count == 0 || !editingPoseUndoStack.Peek().Equals(currentEditingPose.poseData))
                {
                    editingPoseUndoStack.Push(currentEditingPose.poseData);
                    editingPoseRedoStack.Clear();
                }
            }

            EditorGUI.BeginChangeCheck();

            foreach(Transform bone in transforms)
            {
                float boneHandleSize = HandleUtility.GetHandleSize(bone.position);

                if(bone != currentEditingBone)
                {
                    float handleSize = boneHandleSize * 0.15f;
                    Handles.color = Color.green;
                    if(Handles.Button(bone.position, Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
                    {
                        currentEditingBone = bone;
                    }
                }
                else
                {
                    // This bone is SELECTED. #Goated
                    float handleSize = boneHandleSize * 0.25f;
                    Handles.color = Color.red;
                    if(Handles.Button(bone.position, Quaternion.identity, handleSize, handleSize, Handles.SphereHandleCap))
                    {
                        currentEditingBone = null;
                        continue;
                    }

                    if(bone == visualizer.viewingHandReferences.hand)
                    {
                        Vector3 position = visualizer.viewingHandReferences.hand.position;
                        Quaternion rotation = visualizer.viewingHandReferences.hand.rotation;
                        Handles.TransformHandle(ref position, ref rotation);

                        if((visualizer.viewingHandReferences.hand.position - position).sqrMagnitude < 0.00001 && Quaternion.Angle(visualizer.viewingHandReferences.hand.rotation, rotation) < 0.1) continue;

                        // Sometimes hand drifts off when not doing anything because im converting the positions in a lot of different spaces. Uhh fix maybe bc yea
                        if((visualizer.viewingHandReferences.hand.position - position).sqrMagnitude < 0.00001) position = visualizer.viewingHandReferences.hand.position;
                        if(Quaternion.Angle(visualizer.viewingHandReferences.hand.rotation, rotation) < 0.1) rotation = visualizer.viewingHandReferences.hand.rotation;

                        HandleConversion.HandleConfiguration resultingHandle = HandleConversion.WorldToGripHandle(visualizer.targetGrip, SimpleTransform.Create(position, rotation), visualizer.viewingHand);
                        if(visualizer.viewingHand == SelectedHand.Left)
                        {
                            currentEditingPose.poseData.leftHandle = resultingHandle.handle;
                            currentEditingPose.poseData.invLeftHandle = resultingHandle.invHandle;
                            currentEditingPose.poseData.leftArtHandle = resultingHandle.artHandle;
                        }
                        else
                        {
                            currentEditingPose.poseData.rightHandle = resultingHandle.handle;
                            currentEditingPose.poseData.invRightHandle = resultingHandle.invHandle;
                            currentEditingPose.poseData.rightArtHandle = resultingHandle.artHandle;
                        }
                    }
                    else if(bone == visualizer.viewingHandReferences.index1)
                    {
                        Quaternion rotation = bone.rotation;
                        bone.rotation = Handles.RotationHandle(rotation, bone.position);
                        currentEditingPose.poseData.index1 = bone.localRotation;
                    }
                    else if(bone == visualizer.viewingHandReferences.middle1)
                    {
                        Quaternion rotation = bone.rotation;
                        bone.rotation = Handles.RotationHandle(rotation, bone.position);
                        currentEditingPose.poseData.middle1 = bone.localRotation;
                    }
                    else if(bone == visualizer.viewingHandReferences.ring1)
                    {
                        Quaternion rotation = bone.rotation;
                        bone.rotation = Handles.RotationHandle(rotation, bone.position);
                        currentEditingPose.poseData.ring1 = bone.localRotation;
                    }
                    else if(bone == visualizer.viewingHandReferences.pinky1)
                    {
                        Quaternion rotation = bone.rotation;
                        bone.rotation = Handles.RotationHandle(rotation, bone.position);
                        currentEditingPose.poseData.pinky1 = bone.localRotation;
                    }
                    else if(bone == visualizer.viewingHandReferences.thumb1)
                    {
                        Quaternion rotation = bone.rotation;
                        bone.rotation = Handles.RotationHandle(rotation, bone.position);
                        currentEditingPose.poseData.thumb1 = bone.localRotation;
                    }
                    else if(bone == visualizer.viewingHandReferences.index2)
                    {
                        Handles.color = Color.blue;
                        DrawAngleHandle(ref currentEditingPose.poseData.index2, bone.position, bone.rotation, bone.forward, boneHandleSize);
                    }
                    else if(bone == visualizer.viewingHandReferences.index3)
                    {
                        Handles.color = Color.blue;
                        DrawAngleHandle(ref currentEditingPose.poseData.index3, bone.position, bone.rotation, bone.forward, boneHandleSize);
                    }
                    else if(bone == visualizer.viewingHandReferences.middle2)
                    {
                        Handles.color = Color.blue;
                        DrawAngleHandle(ref currentEditingPose.poseData.middle2, bone.position, bone.rotation, bone.forward, boneHandleSize);
                    }
                    else if(bone == visualizer.viewingHandReferences.middle3)
                    {
                        Handles.color = Color.blue;
                        DrawAngleHandle(ref currentEditingPose.poseData.middle3, bone.position, bone.rotation, bone.forward, boneHandleSize);
                    }
                    else if(bone == visualizer.viewingHandReferences.ring2)
                    {
                        Handles.color = Color.blue;
                        DrawAngleHandle(ref currentEditingPose.poseData.ring2, bone.position, bone.rotation, bone.forward, boneHandleSize);
                    }
                    else if(bone == visualizer.viewingHandReferences.ring3)
                    {
                        Handles.color = Color.blue;
                        DrawAngleHandle(ref currentEditingPose.poseData.ring3, bone.position, bone.rotation, bone.forward, boneHandleSize);
                    }
                    else if(bone == visualizer.viewingHandReferences.pinky2)
                    {
                        Handles.color = Color.blue;
                        DrawAngleHandle(ref currentEditingPose.poseData.pinky2, bone.position, bone.rotation, bone.forward, boneHandleSize);
                    }
                    else if(bone == visualizer.viewingHandReferences.pinky3)
                    {
                        Handles.color = Color.blue;
                        DrawAngleHandle(ref currentEditingPose.poseData.pinky3, bone.position, bone.rotation, bone.forward, boneHandleSize);
                    }
                    else if(bone == visualizer.viewingHandReferences.thumb2)
                    {
                        Handles.color = Color.blue;
                        DrawAngleHandle(ref currentEditingPose.poseData.thumb2, bone.position, bone.rotation, bone.forward, boneHandleSize);
                    }
                    else if(bone == visualizer.viewingHandReferences.thumb3)
                    {
                        Handles.color = Color.blue;
                        DrawAngleHandle(ref currentEditingPose.poseData.thumb3, bone.position, bone.rotation, bone.forward, boneHandleSize);
                    }
                }
            }

            if(EditorGUI.EndChangeCheck())
            {
            }
        }
    }

    void UndoEdit()
    {
        if(editingPoseUndoStack.Count == 0) return;

        HandPose.PoseData latestPose = editingPoseUndoStack.Pop();
        editingPoseRedoStack.Push(latestPose);

        currentEditingPose.poseData = latestPose;
    }

    void RedoEdit()
    {
        if(editingPoseRedoStack.Count == 0) return;

        HandPose.PoseData latestRedo = editingPoseRedoStack.Pop();
        editingPoseUndoStack.Push(latestRedo);

        currentEditingPose.poseData = latestRedo;
    }

    float DrawAngleHandle(ref float angle, Vector3 position, Quaternion rotation, Vector3 axis, float size)
    {
        Quaternion newRot = Handles.Disc(rotation, position, axis, size, false, 0f);

        Quaternion deltaRot = newRot * Quaternion.Inverse(rotation);
        deltaRot.ToAngleAxis(out float deltaAngle, out Vector3 deltaAxis);

        if (deltaAngle > 180f)
            deltaAngle -= 360f;

        if (Vector3.Dot(deltaAxis, axis) < 0f)
            deltaAngle = -deltaAngle;

        angle += deltaAngle;

        if (angle > 180f)
            angle -= 360f;

        return deltaAngle;
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

        editorGroup = tree.Q<VisualElement>("EditMode");
        editOff = tree.Q<VisualElement>("EditOff");
        editOn = tree.Q<VisualElement>("EditOn");

        Button startEditButton = tree.Q<Button>("BeginEditing");
        startEditButton.clicked += StartEditing;
        Button cancelEditButton = tree.Q<Button>("CancelEditing");
        cancelEditButton.clicked += CancelEditing;
        Button saveEditButton = tree.Q<Button>("SaveEditing");
        saveEditButton.clicked += EndEditing;

        Button editUndo = tree.Q<Button>("EditUndo");
        editUndo.clicked += UndoEdit;
        Button editRedo = tree.Q<Button>("EditRedo");
        editRedo.clicked += RedoEdit;

        Button editMirrorHandle = tree.Q<Button>("EditMirrorHandle");
        editMirrorHandle.clicked += MirrorHandle;

        UpdatePanelContent();

        rootVisualElement.Add(tree);
        return rootVisualElement;
    }

    void MirrorHandle()
    {
        if(currentEditingPose == null) return;

        Vector3 position = visualizer.viewingHandReferences.hand.position;
        Quaternion rotation = visualizer.viewingHandReferences.hand.rotation;

        HandleConversion.HandleConfiguration currentHandleConfig = HandleConversion.WorldToGripHandle(visualizer.targetGrip, new SimpleTransform(position, rotation), visualizer.viewingHand);
        HandleConversion.HandleConfiguration flippedHandleConfig = HandleConversion.FlipHandle(currentHandleConfig);
        
        if(visualizer.viewingHand == SelectedHand.Left)
        {
            // Update current just to be safe
            currentEditingPose.poseData.leftHandle = currentHandleConfig.handle;
            currentEditingPose.poseData.invLeftHandle = currentHandleConfig.invHandle;
            currentEditingPose.poseData.leftArtHandle = currentHandleConfig.artHandle;

            // Apply flipped handle
            currentEditingPose.poseData.rightHandle = flippedHandleConfig.handle;
            currentEditingPose.poseData.invRightHandle = flippedHandleConfig.invHandle;
            currentEditingPose.poseData.rightArtHandle = flippedHandleConfig.artHandle;
        }
        else
        {
            currentEditingPose.poseData.leftHandle = flippedHandleConfig.handle;
            currentEditingPose.poseData.invLeftHandle = flippedHandleConfig.invHandle;
            currentEditingPose.poseData.leftArtHandle = flippedHandleConfig.artHandle;

            currentEditingPose.poseData.rightHandle = currentHandleConfig.handle;
            currentEditingPose.poseData.invRightHandle = currentHandleConfig.invHandle;
            currentEditingPose.poseData.rightArtHandle = currentHandleConfig.artHandle;
        }
    }

    void StartEditing()
    {
        PoseDataReference startingPoseData = new PoseDataReference(visualizer.CurrentPoseData);
        if(startingPoseData == null) return;

        currentEditingPose = startingPoseData;
        visualizer.poseDataOverride = currentEditingPose;

        editOff.style.display = DisplayStyle.None;
        editOn.style.display = DisplayStyle.Flex;
    }

    void EndEditing()
    {
        visualizer.SetPoseData(currentEditingPose.poseData);

        editingPoseUndoStack.Clear();
        editingPoseRedoStack.Clear();

        currentEditingPose = null;
        currentEditingBone = null;
        visualizer.poseDataOverride = null;
        editOff.style.display = DisplayStyle.Flex;
        editOn.style.display = DisplayStyle.None;
    }

    void CancelEditing()
    {
        editingPoseUndoStack.Clear();
        editingPoseRedoStack.Clear();

        currentEditingPose = null;
        currentEditingBone = null;
        visualizer.poseDataOverride = null;
        editOff.style.display = DisplayStyle.Flex;
        editOn.style.display = DisplayStyle.None;
    }

    void UpdatePanelContent()
    {
        if (overlayMode == 0)
        {
            visualizerGroup.style.display = DisplayStyle.None;
            editorGroup.style.display = DisplayStyle.None;
        }
        else if (overlayMode == 1)
        {
            visualizerGroup.style.display = DisplayStyle.Flex;
            editorGroup.style.display = DisplayStyle.None;
        }
        else if (overlayMode == 2)
        {
            visualizerGroup.style.display = DisplayStyle.None;
            editorGroup.style.display = DisplayStyle.Flex;
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