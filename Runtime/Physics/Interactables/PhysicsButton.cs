
namespace NKUA.DI.RealityLab.Physics.Interactables
{
    using System.Collections.Generic;
    using UnityEngine;

    public class PhysicsButton : MonoBehaviour
    {
        public Transform Movable;
        public Transform Target;
        public float PressedThreshold = 0.001f;
        public float ReleasedThreshold = 0.01f;
        public List<ResetPhysicsObject> ObjectsToResetOnButtonPress;

        Vector3 MovableOldPosition;
        bool IsPressed;

        void Start()
        {
            MovableOldPosition = Movable.position;
        }

        void Update()
        {
            if (MovableOldPosition == Movable.position)
            {
                return;
            }

            if ((Movable.position - Target.position).magnitude < PressedThreshold)
            {
                if (!IsPressed)
                {
                    foreach (ResetPhysicsObject rpo in ObjectsToResetOnButtonPress)
                    {
                        rpo.ResetTransform();
                    }

                    IsPressed = true;
                }
            }
            else
            {
                if ((Movable.position - Target.position).magnitude > ReleasedThreshold)
                {
                    IsPressed = false;
                }
            }

            MovableOldPosition = Movable.position;
        }
    }
}
