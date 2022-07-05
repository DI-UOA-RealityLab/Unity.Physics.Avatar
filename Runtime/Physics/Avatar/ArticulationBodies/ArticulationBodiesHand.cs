
namespace NKUA.DI.RealityLab.Physics.Avatar
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    public class ArticulationBodiesHand : MonoBehaviour
    {
        [SerializeField]
        private GameObject animatedHandModel;
        public GameObject AnimatedHandModel {get {return animatedHandModel;} set {animatedHandModel = value;}}

        [SerializeField]
        private ArticulationBodiesConfiguration articulationBodiesConfiguration;
        public ArticulationBodiesConfiguration ArticulationBodiesConfiguration {get {return articulationBodiesConfiguration;} set {articulationBodiesConfiguration = value;}}

        [SerializeField]
        private LayerMask noCollisionLayer;
        public LayerMask NoCollisionLayer {get {return noCollisionLayer;} set {noCollisionLayer = value;}}

        [SerializeField]
        private GameObject cloneHandWithPhysics;
        public GameObject CloneHandWithPhysics {get {return cloneHandWithPhysics;} set {cloneHandWithPhysics = value;}}

        [SerializeField]
        private Hand hand;
        public Hand Hand {get {return hand;} set {hand = value;}}

        private ArticulationBody RootArticulationBody;
        private ArticulationBody PalmPositionXArticulationBody;
        private ArticulationBody PalmPositionYArticulationBody;
        private ArticulationBody PalmPositionZArticulationBody;
        private ArticulationBody PalmRotationXArticulationBody;
        private ArticulationBody PalmRotationYArticulationBody;
        private ArticulationBody PalmRotationZArticulationBody;
        private ArticulationBody PalmRotationArticulationBody;
        private Transform RootTransform;
        private Transform PalmPositionXTransform;
        private Transform PalmPositionYTransform;
        private Transform PalmPositionZTransform;
        private Transform PalmRotationXTransform;
        private Transform PalmRotationYTransform;
        private Transform PalmRotationZTransform;
        private Transform PalmRotationTransform;
        private List<ArticulationBody> ArticulationBodiesList = new List<ArticulationBody>();
        private Dictionary<string, Transform> FingerBoneTransformDict = new Dictionary<string, Transform>();
        private Collider[] HandColliders;
        private bool ResetInProcess;

        // [ContextMenu("Setup Hand!")]
        // void DoSomething()
        // {
        //     SetupHand();
        // }

        void Awake()
        {
            SetupHand();
        }

        void OnEnable()
        {
            ArticulationBodiesConfiguration.OnConfigurationChange.AddListener(ChangeConfiguration);
        }

        void OnDisable()
        {
            ArticulationBodiesConfiguration.OnConfigurationChange.RemoveListener(ChangeConfiguration);
        }

        void FixedUpdate()
        {
            StabilizeHand();
        }

        void SetupHand()
        {
            CleanHandParentTransforms();

            CreateHandHierarchy();

            SetupHandArticulationBodies();

            SetupColliderProperties();

            AddArticulationBodyFollowerComponents();
        }

        void SetupHandArticulationBodies()
        {
            SetupRootArticulationBody();

            SetupPalmArticulationBodies();

            SetupFingerBoneArticulationBodies();
        }

        void CreateHandHierarchy()
        {
            RootTransform = CreateRootGameObject(Hand.Palm.Root);

            string palmRootName = Hand.Palm.Root.name;

            PalmPositionXTransform = CreateNonRootGameObject(palmRootName + ".PositionX", RootTransform, null, true);
            PalmPositionYTransform = CreateNonRootGameObject(palmRootName + ".PositionY", PalmPositionXTransform, null, true);
            PalmPositionZTransform = CreateNonRootGameObject(palmRootName + ".PositionZ", PalmPositionYTransform, Hand.Palm.Root.parent.gameObject, true);

            if (ArticulationBodiesConfiguration.UseWristAnchorRotation)
            {
                PalmRotationTransform = CreateNonRootGameObject(palmRootName + ".Rotation", PalmPositionZTransform, Hand.Palm.Root.gameObject, true);
            }
            else
            {
                PalmRotationYTransform = CreateNonRootGameObject(palmRootName + ".RotationY", PalmPositionZTransform, null, true);
                PalmRotationXTransform = CreateNonRootGameObject(palmRootName + ".RotationX", PalmRotationYTransform, null, true);
                PalmRotationZTransform = CreateNonRootGameObject(palmRootName + ".RotationZ", PalmRotationXTransform, Hand.Palm.Root.gameObject, true);

                Vector3 defaultPalmPosition = new Vector3(
                    Hand.Palm.Root.localPosition.x,
                    Hand.Palm.Root.localPosition.y,
                    Hand.Palm.Root.localPosition.z
                );
                Vector3 defaultPalmRotation = new Vector3(
                    Hand.Palm.Root.localEulerAngles.x,
                    Hand.Palm.Root.localEulerAngles.y,
                    Hand.Palm.Root.localEulerAngles.z
                );

                PalmRotationYTransform.SetLocalRotation(new Vector3(0, defaultPalmRotation.y, 0));
                PalmRotationXTransform.SetLocalRotation(new Vector3(defaultPalmRotation.x, 0, 0));
                PalmRotationZTransform.SetLocalRotation(new Vector3(0, 0, defaultPalmRotation.z));

                PalmRotationYTransform.SetLocalPosition(defaultPalmPosition);
            }

            foreach (Finger finger in Hand.FingersList)
            {
                foreach (var (bone, index) in finger.BonesList.WithIndex())
                {
                    string boneNameY = bone.Root.name + ".RotationY";
                    string boneNameX = bone.Root.name + ".RotationX";
                    string boneNameZ = bone.Root.name + ".RotationZ";

                    Vector3 defaultBonePosition = new Vector3(
                        bone.Root.localPosition.x,
                        bone.Root.localPosition.y,
                        bone.Root.localPosition.z
                    );
                    Vector3 defaultBoneRotation = new Vector3(
                        bone.Root.localEulerAngles.x,
                        bone.Root.localEulerAngles.y,
                        bone.Root.localEulerAngles.z
                    );

                    if (index >= finger.BonesList.Count - 2 && index < finger.BonesList.Count)
                    {
                        FingerBoneTransformDict.Add(boneNameZ, CreateNonRootGameObject(boneNameZ, bone.Root.parent, bone.Root.gameObject, true));

                        FingerBoneTransformDict[boneNameZ].SetLocalRotation(defaultBoneRotation);

                        FingerBoneTransformDict[boneNameZ].SetLocalPosition(defaultBonePosition);
                    }
                    else
                    {
                        FingerBoneTransformDict.Add(boneNameY, CreateNonRootGameObject(boneNameY, bone.Root.parent));
                        FingerBoneTransformDict.Add(boneNameX, CreateNonRootGameObject(boneNameX, FingerBoneTransformDict[boneNameY], null, true));
                        FingerBoneTransformDict.Add(boneNameZ, CreateNonRootGameObject(boneNameZ, FingerBoneTransformDict[boneNameX], bone.Root.gameObject, true));

                        FingerBoneTransformDict[boneNameY].SetLocalRotation(new Vector3(0, defaultBoneRotation.y, 0));
                        FingerBoneTransformDict[boneNameX].SetLocalRotation(new Vector3(defaultBoneRotation.x, 0, 0));
                        FingerBoneTransformDict[boneNameZ].SetLocalRotation(new Vector3(0, 0, defaultBoneRotation.z));

                        FingerBoneTransformDict[boneNameY].SetLocalPosition(defaultBonePosition);
                    }
                }
            }
        }

        Transform CreateRootGameObject(Transform palmRoot)
        {
            GameObject rootGameObject = new GameObject("HandRoot");

            rootGameObject.transform.parent = palmRoot.parent.parent;
            palmRoot.parent.parent = rootGameObject.transform;

            rootGameObject.transform.ResetLocalTransform();
            palmRoot.ResetLocalTransform();

            return rootGameObject.transform;
        }

        Transform CreateNonRootGameObject(string objectName, Transform parent, GameObject bodyGameObject = null, bool resetLocalPosition = false)
        {
            if (bodyGameObject == null)
            {
                bodyGameObject = new GameObject(objectName);
            }
            else
            {
                bodyGameObject.name = objectName;
            }

            Vector3 oldLocalPosition = new Vector3(
                bodyGameObject.transform.localPosition.x,
                bodyGameObject.transform.localPosition.y,
                bodyGameObject.transform.localPosition.z
            );

            bodyGameObject.transform.parent = parent;
            bodyGameObject.transform.localPosition = oldLocalPosition;

            bodyGameObject.transform.ResetLocalRotation();

            if (resetLocalPosition)
            {
                bodyGameObject.transform.ResetLocalPosition();
            }

            return bodyGameObject.transform;
        }

        void SetupColliderProperties()
        {
            Hand.Palm.Collider.material = ArticulationBodiesConfiguration.HandPhysicMaterial;

            if (Hand.Palm.MeshCollider)
            {
                Hand.Palm.MeshCollider.material = ArticulationBodiesConfiguration.HandPhysicMaterial;
            }

            HandColliders = Hand.Palm.Root.parent.GetComponentsInChildren<Collider>();
            foreach (Collider collider in HandColliders)
            {
                collider.material = ArticulationBodiesConfiguration.HandPhysicMaterial;

                Physics.IgnoreCollision(Hand.Palm.Collider, collider, true);

                if (Hand.Palm.MeshCollider)
                {
                    Physics.IgnoreCollision(Hand.Palm.MeshCollider, collider, true);
                }
            }

            foreach (ArticulationBody ab in ArticulationBodiesList)
            {
                if (!ab.name.EndsWith("RotationZ") && !ab.name.EndsWith("Root"))
                {
                    SphereCollider sc = ab.gameObject.AddComponent<SphereCollider>();
                    sc.radius = 0.005f;

                    ab.gameObject.layer = (int) Mathf.Log(noCollisionLayer.value, 2);
                }
            }
        }

        void CleanHandParentTransforms()
        {
            Transform currentParent = Hand.Palm.Root.parent;

            while (currentParent)
            {
                currentParent.ResetLocalTransform();

                currentParent = currentParent.parent;
            }
        }

        void SetupRootArticulationBody()
        {
            RootArticulationBody = RootTransform.gameObject.AddComponent<ArticulationBody>();
            ArticulationBodiesList.Add(RootArticulationBody);

            SetupBaseArticulationBodyConfiguration(RootArticulationBody, ArticulationBodiesConfiguration.Root);
            RootArticulationBody.immovable = true;
        }

        void SetupPalmArticulationBodies()
        {
            SetupPalmPositionArticulationBody(ref PalmPositionXArticulationBody, PalmPositionXTransform.gameObject);
            SetupPalmPositionArticulationBody(ref PalmPositionYArticulationBody, PalmPositionYTransform.gameObject);
            SetupPalmPositionArticulationBody(ref PalmPositionZArticulationBody, PalmPositionZTransform.gameObject);

            if (ArticulationBodiesConfiguration.UseWristAnchorRotation)
            {
                PalmRotationArticulationBody = SetupArticulationBodyComponent(PalmRotationTransform.gameObject);
                SetupArticulationBodyConfiguration(PalmRotationArticulationBody, ArticulationBodiesConfiguration.Palm);
                SetupFixedArticulationJoint(PalmRotationArticulationBody);
            }
            else
            {
                SetupPalmRevoluteRotationArticulationBody(ref PalmRotationYArticulationBody, PalmRotationYTransform.gameObject);
                SetupPalmRevoluteRotationArticulationBody(ref PalmRotationXArticulationBody, PalmRotationXTransform.gameObject);
                SetupPalmRevoluteRotationArticulationBody(ref PalmRotationZArticulationBody, PalmRotationZTransform.gameObject);

                PalmRotationZArticulationBody.mass = ArticulationBodiesConfiguration.Palm.Mass;
            }
        }

        ArticulationBody SetupArticulationBodyComponent(GameObject bodyGameObject)
        {
            ArticulationBody articulationBody;

            // Collider bodyGameObjectCollider = bodyGameObject.GetComponent<Collider>();
            // if (!bodyGameObjectCollider)
            // {
            //     bodyGameObjectCollider = bodyGameObject.AddComponent<SphereCollider>();
            //     ((SphereCollider)bodyGameObjectCollider).radius = 0.005f;
            // }

            if (!bodyGameObject.GetComponent<ArticulationBody>())
            {
                bodyGameObject.AddComponent<ArticulationBody>();
            }

            articulationBody = bodyGameObject.GetComponent<ArticulationBody>();
            ArticulationBodiesList.Add(articulationBody);

            return articulationBody;
        }

        void SetupPalmPositionArticulationBody(ref ArticulationBody articulationBody, GameObject positionGameObject)
        {
            articulationBody = SetupArticulationBodyComponent(positionGameObject);

            SetupArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.Palm);
            SetupBaseArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.Root);

            SetupPrismaticArticulationJoint(articulationBody, ArticulationBodiesConfiguration.Palm.PositionDrives);
        }

        void SetupPalmRevoluteRotationArticulationBody(ref ArticulationBody articulationBody, GameObject rotationGameObject)
        {
            articulationBody = SetupArticulationBodyComponent(rotationGameObject);

            // SetupArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.FingerBone);
            SetupArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.Palm);
            SetupBaseArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.Root);

            // SetupAllAxesRevoluteArticulationJoint(articulationBody, articulationBodiesConfiguration.FingerBone.RotationDrives);
            SetupAllAxesRevoluteArticulationJoint(articulationBody, articulationBodiesConfiguration.Palm.RotationDrives);
        }

        void SetupPrismaticArticulationJoint(ArticulationBody articulationBody, ArticulationDriveConfiguration articulationDriveConfiguration)
        {
            articulationBody.jointType = ArticulationJointType.PrismaticJoint;
            articulationBody.anchorRotation = Quaternion.identity;

            if (articulationBody.name.EndsWith("X"))
            {
                articulationBody.linearLockX = ArticulationDofLock.FreeMotion;
                articulationBody.linearLockY = ArticulationDofLock.LockedMotion;
                articulationBody.linearLockZ = ArticulationDofLock.LockedMotion;

                var xDrive = articulationBody.xDrive;
                articulationBody.xDrive = SetupArticulationDrive(xDrive, articulationDriveConfiguration);
            }
            if (articulationBody.name.EndsWith("Y"))
            {
                articulationBody.linearLockX = ArticulationDofLock.LockedMotion;
                articulationBody.linearLockY = ArticulationDofLock.FreeMotion;
                articulationBody.linearLockZ = ArticulationDofLock.LockedMotion;

                var yDrive = articulationBody.yDrive;
                articulationBody.yDrive = SetupArticulationDrive(yDrive, articulationDriveConfiguration);
            }
            if (articulationBody.name.EndsWith("Z"))
            {
                articulationBody.linearLockX = ArticulationDofLock.LockedMotion;
                articulationBody.linearLockY = ArticulationDofLock.LockedMotion;
                articulationBody.linearLockZ = ArticulationDofLock.FreeMotion;

                var zDrive = articulationBody.zDrive;
                articulationBody.zDrive = SetupArticulationDrive(zDrive, articulationDriveConfiguration);
            }
        }

        void SetupRevoluteArticulationJoint(ArticulationBody articulationBody, ArticulationDriveConfiguration articulationDriveConfiguration)
        {
            articulationBody.jointType = ArticulationJointType.RevoluteJoint;
            articulationBody.twistLock = ArticulationDofLock.FreeMotion;

            var xDrive = articulationBody.xDrive;
            articulationBody.xDrive = SetupArticulationDrive(xDrive, articulationDriveConfiguration);
        }

        void SetupSingleAxisRevoluteArticulationJoint(ArticulationBody articulationBody, ArticulationDriveConfiguration articulationDriveConfiguration)
        {
            SetupRevoluteArticulationJoint(articulationBody, articulationDriveConfiguration);

            articulationBody.anchorRotation = Quaternion.Euler(0, 90, 180);
        }

        void SetupAllAxesRevoluteArticulationJoint(ArticulationBody articulationBody, ArticulationDriveConfiguration articulationDriveConfiguration)
        {
            SetupRevoluteArticulationJoint(articulationBody, articulationDriveConfiguration);

            if (articulationBody.name.EndsWith("X"))
            {
                articulationBody.anchorRotation = Quaternion.Euler(0, 0, 0);
            }
            if (articulationBody.name.EndsWith("Y"))
            {
                articulationBody.anchorRotation = Quaternion.Euler(0, 0, 90);
            }
            if (articulationBody.name.EndsWith("Z"))
            {
                articulationBody.anchorRotation = Quaternion.Euler(0, 270, 0);
            }
        }

        void SetupFixedArticulationJoint(ArticulationBody articulationBody)
        {
            articulationBody.jointType = ArticulationJointType.FixedJoint;
            articulationBody.matchAnchors = false;
            articulationBody.anchorRotation = Quaternion.identity;
        }

        void SetupFingerBoneArticulationBodies()
        {
            foreach (Finger finger in Hand.FingersList)
            {
                foreach (var (bone, index) in finger.BonesList.WithIndex())
                {
                    SetupBone(bone, index, finger.BonesList.Count);
                }
            }
        }

        void SetupBone(Bone bone, int index, int listCount)
        {
            // ArticulationBody articulationBody = SetupArticulationBodyComponent(bone.Root.name, bone.Root.parent, bone.Root.gameObject);

            // SetupArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.FingerBone);

            // ArticulationBodyFollower articulationBodyFollower = SetupArticulationBodyFollowerComponent(bone.Root.gameObject, AnimatedHandModel.transform);

            // SetupSphericalJoint(articulationBody);

            // SetupFixedArticulationJoint(articulationBody, articulationBodyFollower);

            string boneNameY = bone.Root.name.ReplaceAt(bone.Root.name.Length - 1, 'Y');
            string boneNameX = bone.Root.name.ReplaceAt(bone.Root.name.Length - 1, 'X');
            string boneNameZ = bone.Root.name;

            if (index >= listCount - 2 && index < listCount)
            {
                ArticulationBody articulationBodyZ = SetupBoneRevoluteRotationArticulationBody(FingerBoneTransformDict[boneNameZ].gameObject, true);
            }
            else
            {
                ArticulationBody articulationBodyY = SetupBoneRevoluteRotationArticulationBody(FingerBoneTransformDict[boneNameY].gameObject);
                articulationBodyY.mass = ArticulationBodiesConfiguration.Root.Mass;

                ArticulationBody articulationBodyX = SetupBoneRevoluteRotationArticulationBody(FingerBoneTransformDict[boneNameX].gameObject);
                articulationBodyX.mass = ArticulationBodiesConfiguration.Root.Mass;

                ArticulationBody articulationBodyZ = SetupBoneRevoluteRotationArticulationBody(FingerBoneTransformDict[boneNameZ].gameObject);
            }

            // foreach(Transform t in bone.Root)
            // {
            //     if (t != articulationBodyX.transform)
            //     {
            //         t.parent = articulationBodyZ.transform;
            //     }
            // }
        }

        ArticulationBody SetupBoneRevoluteRotationArticulationBody(GameObject boneGameObject, bool isSingleAxis = false)
        {
            ArticulationBody articulationBody = SetupArticulationBodyComponent(boneGameObject);

            SetupArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.FingerBone);

            if (isSingleAxis)
            {
                SetupSingleAxisRevoluteArticulationJoint(articulationBody, articulationBodiesConfiguration.FingerBone.RotationDrives);
            }
            else
            {
                SetupAllAxesRevoluteArticulationJoint(articulationBody, articulationBodiesConfiguration.FingerBone.RotationDrives);
            }

            return articulationBody;
        }

        void SetupSphericalJoint(ArticulationBody articulationBody)
        {
            articulationBody.jointType = ArticulationJointType.SphericalJoint;
            articulationBody.swingYLock = ArticulationDofLock.FreeMotion;
            articulationBody.swingZLock = ArticulationDofLock.FreeMotion;
            articulationBody.twistLock = ArticulationDofLock.FreeMotion;

            var xDrive = articulationBody.xDrive;
            articulationBody.xDrive = SetupArticulationDrive(xDrive, ArticulationBodiesConfiguration.FingerBone.RotationDrives);

            var yDrive = articulationBody.yDrive;
            articulationBody.yDrive = SetupArticulationDrive(yDrive, ArticulationBodiesConfiguration.FingerBone.RotationDrives);

            var zDrive = articulationBody.zDrive;
            articulationBody.zDrive = SetupArticulationDrive(zDrive, ArticulationBodiesConfiguration.FingerBone.RotationDrives);
        }

        void AddArticulationBodyFollowerComponents()
        {
            // SetupArticulationBodyFollowerComponent(PalmPositionXTransform,gameObject, AnimatedHandModel.transform);
            ArticulationBodyFollower palmArticulationBodyFollower;

            if (ArticulationBodiesConfiguration.UseWristAnchorRotation)
            {
                palmArticulationBodyFollower = PalmRotationTransform.gameObject.AddComponent<ArticulationBodyFollower>();

                palmArticulationBodyFollower.ArticulationBody = PalmRotationArticulationBody;
                palmArticulationBodyFollower.RotateUsingParentAnchor = true;
            }
            else
            {
                palmArticulationBodyFollower = PalmRotationZTransform.gameObject.AddComponent<ArticulationBodyFollower>();

                palmArticulationBodyFollower.YAxisRotationBody = PalmRotationYArticulationBody;
                palmArticulationBodyFollower.XAxisRotationBody = PalmRotationXArticulationBody;
                palmArticulationBodyFollower.ZAxisRotationBody = PalmRotationZArticulationBody;

                palmArticulationBodyFollower.ArticulationBody = PalmRotationZArticulationBody;
                palmArticulationBodyFollower.UseWorldRotation = true;
            }

            SetupArticulationBodyFollowerTarget(palmArticulationBodyFollower, AnimatedHandModel.transform);

            // spherical joint
            // articulationBodyFollower.AllAxesRotationBody = PalmRotationArticulationBody;

            // prismatic joint
            palmArticulationBodyFollower.XAxisPositionBody = PalmPositionXArticulationBody;
            palmArticulationBodyFollower.YAxisPositionBody = PalmPositionYArticulationBody;
            palmArticulationBodyFollower.ZAxisPositionBody = PalmPositionZArticulationBody;

            foreach (Finger finger in Hand.FingersList)
            {
                foreach (var (bone, index) in finger.BonesList.WithIndex())
                {
                    string boneNameY = bone.Root.name.ReplaceAt(bone.Root.name.Length - 1, 'Y');
                    string boneNameX = bone.Root.name.ReplaceAt(bone.Root.name.Length - 1, 'X');
                    string boneNameZ = bone.Root.name;

                    ArticulationBodyFollower boneArticulationBodyFollower = FingerBoneTransformDict[boneNameZ].gameObject.AddComponent<ArticulationBodyFollower>();
                    SetupArticulationBodyFollowerTarget(boneArticulationBodyFollower, AnimatedHandModel.transform);

                    // revolute joint
                    if (index < finger.BonesList.Count - 2)
                    {
                        boneArticulationBodyFollower.YAxisRotationBody = FingerBoneTransformDict[boneNameY].GetComponent<ArticulationBody>();
                        boneArticulationBodyFollower.XAxisRotationBody = FingerBoneTransformDict[boneNameX].GetComponent<ArticulationBody>();
                    }
                    boneArticulationBodyFollower.ZAxisRotationBody = bone.Root.GetComponent<ArticulationBody>();

                    // set the articulation body
                    boneArticulationBodyFollower.ArticulationBody = boneArticulationBodyFollower.ZAxisRotationBody;
                }
            }
        }

        void SetupArticulationBodyFollowerTarget(ArticulationBodyFollower articulationBodyFollower, Transform targetTransformContainer)
        {
            int i = articulationBodyFollower.name.LastIndexOf('.');
            articulationBodyFollower.Target = targetTransformContainer.FindExactChildRecursive(articulationBodyFollower.name.Substring(0, i));
        }

        void SetupBaseArticulationBodyConfiguration(ArticulationBody articulationBody, ArticulationBodyBaseConfiguration articulationBodyBaseConfiguration)
        {
            articulationBody.mass = articulationBodyBaseConfiguration.Mass;
            articulationBody.useGravity = articulationBodyBaseConfiguration.UseGravity;
            articulationBody.collisionDetectionMode = articulationBodyBaseConfiguration.CollisionDetectionMode;

            SetupStabilitySettings(articulationBody);
        }

        void SetupStabilitySettings(ArticulationBody articulationBody)
        {
            articulationBody.maxLinearVelocity = ArticulationBodiesConfiguration.MaxLinearVelocity;
            articulationBody.maxAngularVelocity = ArticulationBodiesConfiguration.MaxAngularVelocity;
            articulationBody.maxDepenetrationVelocity = ArticulationBodiesConfiguration.MaxDepenetrationVelocity;
            articulationBody.maxJointVelocity = ArticulationBodiesConfiguration.MaxJointVelocity;

            // articulationBody.anchorPosition = Vector3.zero;
            // articulationBody.anchorRotation = Quaternion.identity;
            // articulationBody.parentAnchorPosition = Vector3.zero;
            // articulationBody.parentAnchorRotation = Quaternion.identity;
        }

        void SetupArticulationBodyConfiguration(ArticulationBody articulationBody, ArticulationBodyConfiguration articulationBodyConfiguration)
        {
            SetupBaseArticulationBodyConfiguration(articulationBody, (ArticulationBodyBaseConfiguration) articulationBodyConfiguration);

            articulationBody.linearDamping = articulationBodyConfiguration.LinearDamping;
            articulationBody.angularDamping = articulationBodyConfiguration.AngularDamping;
            articulationBody.jointFriction = articulationBodyConfiguration.JointFriction;
        }

        ArticulationDrive SetupArticulationDrive(ArticulationDrive articulationDrive, ArticulationDriveConfiguration articulationDriveConfiguration)
        {
            articulationDrive.stiffness = articulationDriveConfiguration.Stiffness;
            articulationDrive.damping = articulationDriveConfiguration.Damping;
            articulationDrive.forceLimit = articulationDriveConfiguration.ForceLimit;

            return articulationDrive;
        }

        void ChangeConfiguration()
        {
            foreach (ArticulationBody articulationBody in ArticulationBodiesList)
            {
                if (articulationBody == RootArticulationBody)
                {
                    SetupBaseArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.Root);
                }
                else if (articulationBody == PalmPositionXArticulationBody || articulationBody == PalmPositionYArticulationBody || articulationBody == PalmPositionZArticulationBody)
                {
                    SetupArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.Palm);
                    SetupBaseArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.Root);

                    SetupPrismaticArticulationJoint(articulationBody, ArticulationBodiesConfiguration.Palm.PositionDrives);
                }
                else if (articulationBody == PalmRotationXArticulationBody || articulationBody == PalmRotationYArticulationBody || articulationBody == PalmRotationZArticulationBody)
                {
                    SetupArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.Palm);
                    SetupBaseArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.Root);

                    SetupAllAxesRevoluteArticulationJoint(articulationBody, articulationBodiesConfiguration.Palm.RotationDrives);

                    if (articulationBody.name.EndsWith("Z"))
                    {
                        articulationBody.mass = ArticulationBodiesConfiguration.Palm.Mass;
                    }
                }
                else
                {
                    SetupArticulationBodyConfiguration(articulationBody, ArticulationBodiesConfiguration.FingerBone);

                    SetupRevoluteArticulationJoint(articulationBody, ArticulationBodiesConfiguration.FingerBone.RotationDrives);

                    if (articulationBody.name.EndsWith("Y") || articulationBody.name.EndsWith("X"))
                    {
                        articulationBody.mass = ArticulationBodiesConfiguration.Root.Mass;
                    }
                }
            }
        }

        void ResetAllVelocities()
        {
            foreach (ArticulationBody articulationBody in ArticulationBodiesList)
            {
                articulationBody.velocity = Vector3.zero;
                articulationBody.angularVelocity = Vector3.zero;
            }
        }

        void StabilizeHand()
        {
            if (ResetInProcess)
            {
                ResetInProcess = false;
                return;
            }

            foreach (ArticulationBody body in ArticulationBodiesList)
            {
                // if(body.velocity.magnitude > articulationBodiesConfiguration.MaxLinearVelocity)
                //     Debug.Log(body.name + " velocity magnitude: " + body.velocity.magnitude);
                if (body.velocity.magnitude > articulationBodiesConfiguration.MaxLinearVelocity ||
                    body.angularVelocity.magnitude > articulationBodiesConfiguration.MaxAngularVelocity)
                {
                    PerformBodyReset(body);
                }
            }
        }

        void PerformBodyReset(ArticulationBody body)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            ResetInProcess = true;
        }
    }
}
