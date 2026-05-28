#if UNITY_EDITOR
using UnityEngine;
using SLZ.Marrow;
using UnityEditor;

namespace UltraGripEditor
{
    [ExecuteInEditMode]
    public class PersistentGripVisualizer : MonoBehaviour
    {
        private GameObject activeVisualizerObject;
        private GripVisualizer activeVisualizer;

        public GripVisualizer.SelectedHand hand;

        void Awake()
        {
            if(activeVisualizerObject == null) CreateVisualizer();
        }

        void CreateVisualizer()
        {
            GameObject visualizerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("bf692f29001bd42aab4da2220b211fb0"));
            
            activeVisualizerObject = GameObject.Instantiate(visualizerPrefab);
            activeVisualizerObject.hideFlags = /*HideFlags.HideInHierarchy |*/ HideFlags.DontSave;
            activeVisualizerObject.transform.SetParent(transform);
            activeVisualizerObject.transform.localPosition = Vector3.zero;
            activeVisualizerObject.transform.localRotation = Quaternion.identity;

            activeVisualizer = activeVisualizerObject.GetComponent<GripVisualizer>();
            activeVisualizer.targetGrip = GetComponent<TargetGrip>();
            activeVisualizer.show = true;
            activeVisualizer.viewingHand = hand;
            activeVisualizer.radiusIndex = 0;
            activeVisualizer.pryIndex = 0;
        }

        void OnValidate()
        {
            if(activeVisualizerObject == null) CreateVisualizer();
            activeVisualizer.viewingHand = hand;
            activeVisualizer.UpdateFingers();
        }

        void OnEnable()
        {
            if(activeVisualizerObject == null) CreateVisualizer();
            activeVisualizer.show = true;
            activeVisualizer.UpdateFingers();
        }

        void OnDisable()
        {
            activeVisualizer.show = false;
            activeVisualizer.UpdateFingers();
        }

        void OnDestroy()
        {
            DestroyImmediate(activeVisualizer);
        }
    }
}
#endif