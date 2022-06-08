
namespace NKUA.DI.RealityLab.Physics.Avatar
{
    using UnityEngine;

    [System.Serializable]
    public class Palm
    {
        [SerializeField]
        private Transform root;
        public Transform Root {get {return root;} set {root = value;}}

        [SerializeField]
        private BoxCollider collider;
        public BoxCollider Collider {get {return collider;} set {collider = value;}}

        [SerializeField]
        private Vector3 colliderCenter;
        public Vector3 ColliderCenter {get {return colliderCenter;} set {colliderCenter = value;}}

        [SerializeField]
        private Vector3 colliderSize;
        public Vector3 ColliderSize {get {return colliderSize;} set {colliderSize = value;}}

        [SerializeField]
        private MeshCollider meshCollider;
        public MeshCollider MeshCollider {get {return meshCollider;} set {meshCollider = value;}}
    }
}
