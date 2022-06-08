
namespace NKUA.DI.RealityLab.Physics.Avatar
{
    using System.Collections.Generic;
    using UnityEngine;

    public enum FingerType
    {
        Thumb,
        Index,
        Middle,
        Ring,
        Pinky
    }

    [System.Serializable]
    public class Finger
    {
        [SerializeField]
        private Transform root;
        public Transform Root {get {return root;} set {root = value;}}

        [SerializeField]
        private FingerType type;
        public FingerType Type {get {return type;} set {type = value;}}

        [SerializeField]
        private float defaultBoneWidth;
        public float DefaultBoneWidth {get {return defaultBoneWidth;} set {defaultBoneWidth = value;}}

        [SerializeField]
        private Shape defaultBoneShape;
        public Shape DefaultBoneShape {get {return defaultBoneShape;} set {defaultBoneShape = value;}}

        [SerializeField]
        private List<Bone> bonesList;
        public List<Bone> BonesList {get {return bonesList;} set {bonesList = value;}}

        public void ApplyDefaultValuesToBones()
        {
            foreach (Bone bone in bonesList)
            {
                bone.Width = DefaultBoneWidth;
                bone.Shape = DefaultBoneShape;
            }
        }
    }
}
