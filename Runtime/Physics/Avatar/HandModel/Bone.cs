
namespace NKUA.DI.RealityLab.Physics.Avatar
{
    using UnityEngine;

    public enum Shape
    {
        Box,
        Capsule,
        Sphere
    }

    [System.Serializable]
    public class Bone
    {
        [SerializeField]
        private Transform root;
        public Transform Root {get {return root;} set {root = value;}}

        [SerializeField]
        private float length;
        public float Length {get {return length;} set {length = value;}}

        [SerializeField]
        private float width;
        public float Width {get {return width;} set {width = value;}}

        [SerializeField]
        private Shape shape;
        public Shape Shape {get {return shape;} set {shape = value;}}

        [SerializeField]
        private Collider collider;
        public Collider Collider {get {return collider;} set {collider = value;}}

        public Bone(Transform newRoot, float newLength, float newWidth, Shape newShape)
        {
            Length = newLength;
            Width = newWidth;
            Shape = newShape;
            Root = newRoot;
        }
    }
}
