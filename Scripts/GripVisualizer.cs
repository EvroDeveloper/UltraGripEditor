using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLZ.Marrow;
using SLZ.Marrow.Utilities;

public class GripVisualizer : MonoBehaviour
{
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
    }

    public bool show = true;
    public TargetGrip targetGrip;

    public SelectedHand viewingHand;

    public int radiusIndex;
    public int pryIndex;

    public HandReference rightHandReferences;
    public HandReference leftHandReferences;

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

        var selectedPry = handPose.poseData[radiusIndex].poseArray[pryIndex];

        bool viewingLeft = viewingHand == SelectedHand.Left;

        SetActiveHands(viewingLeft, !viewingLeft);

        var handle = viewingLeft ? selectedPry.leftHandle : selectedPry.rightHandle;
        var artHandle = viewingLeft ? selectedPry.leftArtHandle : selectedPry.rightArtHandle;

        SimpleTransform target = SimpleTransform.Create(gripTarget);
        SimpleTransform artHandInWorld = target.Transform(artHandle.inverse);

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
