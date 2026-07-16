using UnityEngine;

namespace FPSTrainingRoom.Training
{
    [ExecuteAlways]
    public class TagSetup : MonoBehaviour
    {
        [Header("Tag Configuration")]
        public bool setupOnAwake = true;

        [Header("Required Tags")]
        public string[] requiredTags = new string[]
        {
            "Player", "Enemy", "Target",
            "Head", "Chest", "Arm", "Leg",
            "Concrete", "Metal", "Flesh"
        };

        protected virtual void Awake()
        {
            if (setupOnAwake && Application.isPlaying)
                EnsureTagsExist();
        }

        public virtual void EnsureTagsExist()
        {
            foreach (string tag in requiredTags)
            {
                if (!IsTagDefined(tag))
                {
                    Debug.LogWarning(
                        $"Tag '{tag}' is not defined in the project. " +
                        $"Please add it in Edit > Project Settings > Tags and Layers.");
                }
            }

            if (!IsLayerDefined("Target"))
            {
                Debug.LogWarning(
                    "Layer 'Target' is not defined. Please add it in " +
                    "Edit > Project Settings > Tags and Layers.");
            }
        }

        protected virtual bool IsTagDefined(string tag)
        {
            try
            {
                GameObject.FindGameObjectWithTag(tag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected virtual bool IsLayerDefined(string layerName)
        {
            return LayerMask.NameToLayer(layerName) != -1;
        }

        public virtual void ApplyTags(GameObject[] objects, string tag)
        {
            foreach (var obj in objects)
            {
                if (obj != null)
                    obj.tag = tag;
            }
        }
    }
}
