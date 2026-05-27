using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLZ.Marrow;
using SLZ.Marrow.Utilities;

public class GripVisualizer : MonoBehaviour
{
    public class PoseDataReference
    {
        public HandPose.PoseData poseData;

        public PoseDataReference(HandPose.PoseData data)
        {
            this.poseData = data;
        }
    }

    public enum SelectedHand
    {
        Left,
        Right
    }

    [System.Serializable]
    public class HandReference
    {
        public GameObject handMesh;
        public Transform handBase;
        public Transform hand;
        public Transform index1;
        public Transform index2;
        public Transform index3;
        public Transform middle1;
        public Transform middle2;
        public Transform middle3;
        public Transform ring1;
        public Transform ring2;
        public Transform ring3;
        public Transform pinky1;
        public Transform pinky2;
        public Transform pinky3;
        public Transform thumb1;
        public Transform thumb2;
        public Transform thumb3;
        public SimpleTransform handBaseToHand;

        private Transform[] _moveableBoneList;
        public Transform[] MoveableBoneList
        {
            get
            {
                if(_moveableBoneList == null)
                {
                    _moveableBoneList = new Transform[]
                    {
                        hand, index1, index2, index3, middle1, middle2, middle3, ring1, ring2, ring3, pinky1, pinky2, pinky3, thumb1, thumb2, thumb3
                    };
                }
                return _moveableBoneList;
            }
        }
    }

    public bool show = true;
    public TargetGrip targetGrip;
    public PoseDataReference poseDataOverride { get; set; }
    public HandPose.PoseDataGroup CurrentPoseDataGroup
    {
        get
        {
            if(targetGrip == null) return default;
            if(targetGrip.handPose == null) return default;
            return targetGrip.handPose.poseData[radiusIndex];
        }
    }
    public HandPose.PoseData CurrentPoseData
    {
        get
        {
            if(poseDataOverride != null) return poseDataOverride.poseData;
            return CurrentPoseDataGroup.poseArray[pryIndex];
        }
    }

    public Transform gripTarget
    {
        get
        {
            if(targetGrip == null) return null;
            if(targetGrip.targetTransform == null) return targetGrip.transform;
            return targetGrip.targetTransform;
        }
    }

    public SelectedHand viewingHand;

    public int radiusIndex;
    public int pryIndex;

    public HandReference rightHandReferences;
    public HandReference leftHandReferences;
    public HandReference viewingHandReferences => viewingHand == SelectedHand.Left ? leftHandReferences : rightHandReferences;

    public void SetPoseData(int groupIndex, int dataIndex, HandPose.PoseData data)
    {
        if(targetGrip == null) return;
        if(targetGrip.handPose == null) return;

        targetGrip.handPose.poseData[groupIndex].poseArray[dataIndex] = data;
    }

    public void SetPoseData(HandPose.PoseData data)
    {
        SetPoseData(radiusIndex, pryIndex, data);
    }

    void SetActiveHands(bool left, bool right)
    {
        leftHandReferences.handMesh.SetActive(left);
        rightHandReferences.handMesh.SetActive(right);
    }

    void OnValidate()
    {
        UpdateFingers();
    }
    
    [ContextMenu("Update Vis")]
    public void UpdateFingers()
    {
        if(!show) { SetActiveHands(false, false); return; }
        if(targetGrip == null) { SetActiveHands(false, false); return; }
        if(targetGrip.handPose == null) { SetActiveHands(false, false); return; }

        Transform gripTarget = targetGrip.targetTransform;
        if(gripTarget == null) gripTarget = targetGrip.transform;
        HandPose handPose = targetGrip.handPose;

        radiusIndex = Mathf.Clamp(radiusIndex, 0, handPose.poseData.Length - 1);
        pryIndex = Mathf.Clamp(pryIndex, 0, handPose.poseData[radiusIndex].poseArray.Length - 1);

        var selectedPry = CurrentPoseData;

        bool viewingLeft = viewingHand == SelectedHand.Left;

        SetActiveHands(viewingLeft, !viewingLeft);

        var handle = viewingLeft ? selectedPry.leftHandle : selectedPry.rightHandle;
        var artHandle = viewingLeft ? selectedPry.leftArtHandle : selectedPry.rightArtHandle;

        artHandle = handle;

        SimpleTransform target = SimpleTransform.Create(gripTarget);
        Quaternion handToArtRot = viewingLeft ? Quaternion.Euler(-90, 90, 0) : Quaternion.Euler(90, -90, 0);
        SimpleTransform artHandInWorld = target.Transform(artHandle.inverse).Transform(Vector3.zero, handToArtRot);

        HandReference visualHand = viewingLeft ? leftHandReferences : rightHandReferences;
        SimpleTransform handBaseInWorld = artHandInWorld.Transform(visualHand.handBaseToHand.inverse);

        visualHand.handBase.SetPositionAndRotation(handBaseInWorld.position, handBaseInWorld.rotation);

        visualHand.index1.localRotation = selectedPry.index1;
        visualHand.index2.localRotation = Quaternion.AngleAxis(selectedPry.index2, Vector3.forward);
        visualHand.index3.localRotation = Quaternion.AngleAxis(selectedPry.index3, Vector3.forward);

        visualHand.middle1.localRotation = selectedPry.middle1;
        visualHand.middle2.localRotation = Quaternion.AngleAxis(selectedPry.middle2, Vector3.forward);
        visualHand.middle3.localRotation = Quaternion.AngleAxis(selectedPry.middle3, Vector3.forward);

        visualHand.ring1.localRotation = selectedPry.ring1;
        visualHand.ring2.localRotation = Quaternion.AngleAxis(selectedPry.ring2, Vector3.forward);
        visualHand.ring3.localRotation = Quaternion.AngleAxis(selectedPry.ring3, Vector3.forward);

        visualHand.pinky1.localRotation = selectedPry.pinky1;
        visualHand.pinky2.localRotation = Quaternion.AngleAxis(selectedPry.pinky2, Vector3.forward);
        visualHand.pinky3.localRotation = Quaternion.AngleAxis(selectedPry.pinky3, Vector3.forward);

        visualHand.thumb1.localRotation = selectedPry.thumb1;
        visualHand.thumb2.localRotation = Quaternion.AngleAxis(selectedPry.thumb2, Vector3.forward);
        visualHand.thumb3.localRotation = Quaternion.AngleAxis(selectedPry.thumb3, Vector3.forward);
    }
}
