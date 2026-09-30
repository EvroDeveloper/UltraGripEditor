using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SLZ.Marrow;
using SLZ.Marrow.Utilities;
using UnityEditor;
using UnityEngine;

namespace UltraGripEditor
{
    public class PryGeneratorWizard : ScriptableWizard
    {
        public HandPose targetHandPose;
        public PryGeneratorUtility.PryLimit handRotationLimit;
        public bool createBackup;

        [MenuItem("Stress Level Zero/Void Tools/Hand Pose Pry Generator")]
        static void CreateWizard()
        {
            var wizard = ScriptableWizard.DisplayWizard<PryGeneratorWizard>("HandPose Pry Generator", "Update Pose");
            if(Selection.activeObject is HandPose handPose) wizard.targetHandPose = handPose;
            else if(Selection.activeGameObject != null && Selection.activeGameObject.TryGetComponent(out Grip grip)) wizard.targetHandPose = grip.handPose;
        }

        [MenuItem("Assets/HandPose/Generate Pry Poses")]
        static void InitFromGrip()
        {
            HandPose handPose = Selection.activeObject as HandPose;
            if(handPose == null) return;
            var wizard = ScriptableWizard.DisplayWizard<PryGeneratorWizard>("HandPose Pry Generator", "Update Pose", "Cancel");
            wizard.targetHandPose = handPose;
        }

        [MenuItem("Assets/HandPose/Generate Pry Poses", true)]
        static bool ValidateInitFromGrip()
        {
            return Selection.activeObject is HandPose;
        }

        void OnWizardCreate()
        {
            if(createBackup)
            {
                BackUpHandPose(targetHandPose);
                targetHandPose.GeneratePryPoses(handRotationLimit);
            }
            else if(EditorUtility.DisplayDialog("HandPose Pry Generator", "This will overwrite all the hand pose data with rotated copies of the resting pose. Are you sure you want to continue?", "Yes", "Cancel"))
            {
                targetHandPose.GeneratePryPoses(handRotationLimit);
            }
        }

        void OnWizardOtherButton()
        {
            Close();
        }

        protected override bool DrawWizardGUI()
        {
            EditorGUILayout.HelpBox("This tool will overwrite all pry poses with rotated copies of the first pose in each radius. Only use if you know what you are doing.", MessageType.Info);
            bool b = base.DrawWizardGUI();
            if(!createBackup)
                EditorGUILayout.HelpBox("No backup will be created, hand pose data may be overwritten and lost", MessageType.Warning);
            return b;
        }

        static void BackUpHandPose(HandPose handPose)
        {
            HandPose backupHandPose = Instantiate(handPose);
            backupHandPose.name = handPose.name + " (Backup)";

            string targetHandPosePath = AssetDatabase.GetAssetPath(handPose);

            if (!string.IsNullOrEmpty(targetHandPosePath))
            {
                targetHandPosePath = Path.Combine(Path.GetDirectoryName(targetHandPosePath), $"{backupHandPose.name}.asset");
            }
            else
            {
                targetHandPosePath = Path.Combine("Assets", $"{backupHandPose.name}.asset");
            }

            AssetDatabase.CreateAsset(backupHandPose, targetHandPosePath);
        }
    }

    public static class PryGeneratorUtility
    {
        public enum PryLimit
        {
            ZeroDegrees,
            FifteenDegrees,
            ThirtyDegrees,
            SixtyDegrees,
        }

        [MenuItem("Tools/EvroDev/Generate30Pry")]
        public static void RunOnSelectedGrip()
        {
            if(Selection.activeObject is HandPose handPose)
            {
                handPose.GeneratePryPoses(PryLimit.ThirtyDegrees);
            }
        }

        public static void GeneratePryPoses(this HandPose handPose, PryLimit newPryLimit)
        {
            Undo.RecordObject(handPose, "Generate Pry Poses");
            if(newPryLimit == PryLimit.ZeroDegrees)
            {
                handPose.swingRotationLimit = 0f;
                handPose.twistRotationLimit = 0f;
                for (int i = 0; i < handPose.poseData.Length; i++)
                {
                    HandPose.PoseDataGroup poseDataGroup = handPose.poseData[i];

                    HandPose.PoseData zeroPryPoseData = FindZeroPryPoseData(poseDataGroup);

                    poseDataGroup.poseArray = new HandPose.PoseData[] {zeroPryPoseData};
                    handPose.poseData[i] = poseDataGroup;
                }
            }
            if(newPryLimit == PryLimit.FifteenDegrees)
            {
                handPose.swingRotationLimit = 15f;
                handPose.twistRotationLimit = 15f;
            }
            if(newPryLimit == PryLimit.ThirtyDegrees)
            {
                handPose.swingRotationLimit = 30f;
                handPose.twistRotationLimit = 30f;
                for (int i = 0; i < handPose.poseData.Length; i++)
                {
                    HandPose.PoseDataGroup poseDataGroup = handPose.poseData[i];

                    HandPose.PoseData zeroPryPoseData = FindZeroPryPoseData(poseDataGroup);

                    List<HandPose.PoseData> generatedPoseData = new();
                    generatedPoseData.Add(zeroPryPoseData);
                    generatedPoseData.Add(RotatePoseDataHandles(zeroPryPoseData, new Vector3(-15f, 0f, 0f)));
                    generatedPoseData.Add(RotatePoseDataHandles(zeroPryPoseData, new Vector3(-30f, 0f, 0f)));
                    generatedPoseData.Add(RotatePoseDataHandles(zeroPryPoseData, new Vector3(15f, 0f, 0f)));
                    generatedPoseData.Add(RotatePoseDataHandles(zeroPryPoseData, new Vector3(30f, 0f, 0f)));
                    generatedPoseData.Add(RotatePoseDataHandles(zeroPryPoseData, new Vector3(0f, -15f, 0f)));
                    generatedPoseData.Add(RotatePoseDataHandles(zeroPryPoseData, new Vector3(0f, -30f, 0f)));
                    generatedPoseData.Add(RotatePoseDataHandles(zeroPryPoseData, new Vector3(0f, 15f, 0f)));
                    generatedPoseData.Add(RotatePoseDataHandles(zeroPryPoseData, new Vector3(0f, 30f, 0f)));
                    generatedPoseData.Add(RotatePoseDataHandles(zeroPryPoseData, new Vector3(0f, 0f, -15f)));
                    generatedPoseData.Add(RotatePoseDataHandles(zeroPryPoseData, new Vector3(0f, 0f, -30f)));
                    generatedPoseData.Add(RotatePoseDataHandles(zeroPryPoseData, new Vector3(0f, 0f, 15f)));
                    generatedPoseData.Add(RotatePoseDataHandles(zeroPryPoseData, new Vector3(0f, 0f, 30f)));

                    poseDataGroup.poseArray = generatedPoseData.ToArray();
                    handPose.poseData[i] = poseDataGroup;
                }
            }
        }

        public static HandPose.PoseData RotatePoseDataHandles(HandPose.PoseData poseData, Vector3 pryVector)
        {
            poseData.nativePry = pryVector;

            SimpleTransform rightHandle = poseData.rightHandle;
            rightHandle.rotation *= Quaternion.Euler(Vector3.Scale(pryVector, new Vector3(-1, 1, -1)));
            poseData.rightHandle = rightHandle;
            poseData.invRightHandle = rightHandle.inverse;

            SimpleTransform leftHandle = poseData.leftHandle;
            leftHandle.rotation *= Quaternion.Euler(Vector3.Scale(pryVector, new Vector3(-1, -1, 1)));
            poseData.leftHandle = leftHandle;
            poseData.invLeftHandle = leftHandle.inverse;
            return poseData;
        }

        public static HandPose.PoseData FindZeroPryPoseData(HandPose.PoseDataGroup poseDataGroup)
        {
            HandPose.PoseData zeroPryPoseData;
            try
            {
                zeroPryPoseData = poseDataGroup.poseArray.First((p) => p.nativePry == Vector3.zero);
            }
            catch(Exception ex)
            {
                //Debug.LogError("[PryGeneratorUtility]: Could not find pose data with native pry of (0, 0, 0) on hand pose " + handPose.name + ". Using index 0");
                zeroPryPoseData = poseDataGroup.poseArray[0];
            }

            return zeroPryPoseData;
        }

        public static bool DisplayWarning()
        {
            return EditorUtility.DisplayDialog("Ultra Grip Editor", "Warning: Generating pry values is DESTRUCTIVE. Make sure you have backed up your hand pose", "Ok", "Cancel");
        }
    }
}