using UnityEngine;
using SLZ.Marrow;
using SLZ.Marrow.Utilities;
using SLZ.Marrow.Interaction;

namespace UltraGripEditor
{
    public class HandleConversion
    {
        public enum MirrorType
        {
            X,
            Y,
            Z,
        }
        public class HandleConfiguration
        {
            public SimpleTransform handle;
            public SimpleTransform invHandle;
            public SimpleTransform artHandle;
        }

        public static HandleConfiguration WorldToGripHandle(TargetGrip grip, SimpleTransform artHandInWorld, GripVisualizer.SelectedHand selectedHand)
        {
            SimpleTransform gripTargetWorld = SimpleTransform.Create(GetGripTarget(grip));

            Quaternion handToArtRot = selectedHand == GripVisualizer.SelectedHand.Left ? Quaternion.Euler(-90, 90, 0) : Quaternion.Euler(90, -90, 0);
            SimpleTransform handInWorld = artHandInWorld.Transform(Vector3.zero, Quaternion.Inverse(handToArtRot));
            
            SimpleTransform handToGrip = handInWorld.InverseTransform(gripTargetWorld);
            SimpleTransform artHandToGrip = artHandInWorld.InverseTransform(gripTargetWorld);
            return new HandleConfiguration()
            {
                handle = handToGrip,
                invHandle = handToGrip.inverse,
                artHandle = artHandToGrip
            };
        }

        // public static (HandleConfiguration, HandleConfiguration) WorldToGripHandle_BothHands(TargetGrip grip, SimpleTransform artHandInWorld, Handedness basedHandedness, MirrorType mirrorType)
        // {
        //     if(basedHandedness != Handedness.Left && basedHandedness != Handedness.Right) return (null, null);

        //     HandleConfiguration baseHandleConfig = WorldToGripHandle(grip, artHandInWorld);
        // }

        private static Transform GetGripTarget(TargetGrip grip)
        {
            if(grip == null) return null;
            if(grip.targetTransform == null) return grip.transform;
            return grip.targetTransform;
        }
    }
}