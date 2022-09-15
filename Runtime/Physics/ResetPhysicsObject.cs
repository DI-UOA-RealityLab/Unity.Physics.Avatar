
namespace NKUA.DI.RealityLab.Physics
{
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class ResetPhysicsObject : MonoBehaviour
    {
        Rigidbody RigidbodyComponent;
        Vector3 StartingPosition;
        Vector3 StartingRotation;
        bool PerformReset;

        void Start()
        {
            RigidbodyComponent = GetComponent<Rigidbody>();

            StartingPosition = RigidbodyComponent.transform.position.GetNewVector3();

            StartingRotation = RigidbodyComponent.transform.rotation.eulerAngles.GetNewVector3();
        }

        public void ResetTransform()
        {
            PerformReset = true;
        }

        void FixedUpdate()
        {
            if (PerformReset)
            {
                bool isKinematicOn = false;
                if (RigidbodyComponent.isKinematic)
                {
                    isKinematicOn = true;
                }
                else
                {
                    RigidbodyComponent.isKinematic = true;
                }

                RigidbodyComponent.transform.position = StartingPosition.GetNewVector3();
                RigidbodyComponent.transform.rotation = Quaternion.Euler(StartingRotation.GetNewVector3());

                if (!isKinematicOn)
                {
                    RigidbodyComponent.isKinematic = false;
                }

                PerformReset = false;
            }
        }
    }
}
