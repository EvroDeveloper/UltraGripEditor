using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLZ.Marrow;
using SLZ.Marrow.Utilities;

namespace UltraGripEditor
{
    public static class PoseLerper
    {
        public static HandPose.PoseDataGroup GetPoseGroupAtRadius(this HandPose handPose, float radius)
        {
            HandPose.PoseDataGroup[] groups = handPose.poseData;
            if(radius <= groups[0].radius) return groups[0];
            if(radius >= groups[groups.Length - 1].radius) return groups[groups.Length - 1];

            HandPose.PoseDataGroup lower = default;
            HandPose.PoseDataGroup higher = default;
            for(int i = 0; i < groups.Length - 1; i++)
            {
                if(groups[i].radius <= radius && groups[i+1].radius > radius)
                {
                    lower = groups[i];
                    higher = groups[i+1];
                    break;
                }
            }
            float remapped = (radius - lower.radius) / (higher.radius - lower.radius);

            return Lerp(lower, higher, remapped);
        }

        public static HandPose.PoseDataGroup Lerp(HandPose.PoseDataGroup l, HandPose.PoseDataGroup r, float t)
        {
            HandPose.PoseDataGroup output = new HandPose.PoseDataGroup();
            output.radius = Mathf.Lerp(l.radius, r.radius, t);
            List<HandPose.PoseData> poseDatas = new List<HandPose.PoseData>();
            for(int i = 0; i < l.poseArray.Length; i++)
            {
                poseDatas.Add(Lerp(l.poseArray[i], r.poseArray[i], t));
            }
            output.poseArray = poseDatas.ToArray();
            return output;
        }

        public static HandPose.PoseData Lerp(HandPose.PoseData l, HandPose.PoseData r, float t)
        {
            HandPose.PoseData output = new HandPose.PoseData
            {
                nativePry = l.nativePry,

                index1 = Quaternion.Slerp(l.index1, r.index1, t),
                middle1 = Quaternion.Slerp(l.middle1, r.middle1, t),
                ring1 = Quaternion.Slerp(l.ring1, r.ring1, t),
                pinky1 = Quaternion.Slerp(l.pinky1, r.pinky1, t),
                thumb1 = Quaternion.Slerp(l.thumb1, r.thumb1, t),

                index2 = Mathf.Lerp(l.index2, r.index2, t),
                index3 = Mathf.Lerp(l.index3, r.index3, t),
                middle2 = Mathf.Lerp(l.middle2, r.middle2, t),
                middle3 = Mathf.Lerp(l.middle3, r.middle3, t),
                ring2 = Mathf.Lerp(l.ring2, r.ring2, t),
                ring3 = Mathf.Lerp(l.ring3, r.ring3, t),
                pinky2 = Mathf.Lerp(l.pinky2, r.pinky2, t),
                pinky3 = Mathf.Lerp(l.pinky3, r.pinky3, t),
                thumb2 = Mathf.Lerp(l.thumb2, r.thumb2, t),
                thumb3 = Mathf.Lerp(l.thumb3, r.thumb3, t),

                leftHandle = SimpleTransform.Lerp(l.leftHandle, r.leftHandle, t),
                invLeftHandle = SimpleTransform.Lerp(l.invLeftHandle, r.invLeftHandle, t),
                leftArtHandle = SimpleTransform.Lerp(l.leftArtHandle, r.leftArtHandle, t),
                rightHandle = SimpleTransform.Lerp(l.rightHandle, r.rightHandle, t),
                invRightHandle = SimpleTransform.Lerp(l.invRightHandle, r.invRightHandle, t),
                rightArtHandle = SimpleTransform.Lerp(l.rightArtHandle, r.rightArtHandle, t),
            };
            return output;
        }
    }
}