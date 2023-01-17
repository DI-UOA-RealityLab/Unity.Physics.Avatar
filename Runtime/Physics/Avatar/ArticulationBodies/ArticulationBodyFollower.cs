
namespace NKUA.DI.RealityLab.Physics.Avatar
{
    using UnityEngine;

    public class ArticulationBodyFollower : MonoBehaviour
    {
        public ArticulationBody ArticulationBody;
        public Transform Target;

        [Header("Prismatic Joint Settings")]
        public ArticulationBody XAxisPositionBody;
        public ArticulationBody YAxisPositionBody;
        public ArticulationBody ZAxisPositionBody;

        [Header("Revolute Joint Settings")]
        public ArticulationBody YAxisRotationBody;
        public ArticulationBody XAxisRotationBody;
        public ArticulationBody ZAxisRotationBody;
        public bool UseWorldRotation;

        [Header("Spherical Joint Settings")]
        public ArticulationBody AllAxesRotationBody;
        public bool RotateUsingParentAnchor;

        Vector3 StartingPosition, PositionDelta;
        Vector3 RotationDelta, StartingRotation;
        float CurrentAngleInRange;
        float CurrentAngleDifferenceInRange;
        int OperationSign = 1;

        void Start()
        {
            StartingPosition = transform.position;

            if (!UseWorldRotation)
            {
                StartingRotation = Target.localRotation.eulerAngles;
            }
        }

        void Update()
        {
            if (ArticulationBody.transform.localScale.x != Target.localScale.x ||
                ArticulationBody.transform.localScale.y != Target.localScale.y ||
                ArticulationBody.transform.localScale.z != Target.localScale.z)
            {
                ArticulationBody.transform.localScale = new Vector3(
                    Target.localScale.x,
                    Target.localScale.y,
                    Target.localScale.z
                );
            }
        }

        void FixedUpdate()
        {
            if (XAxisPositionBody || YAxisPositionBody || ZAxisPositionBody)
            {
                PositionDelta = Target.position - StartingPosition;
            }

            if (XAxisPositionBody)
            {
                // set joint Drive of X axis to new position
                var xDrive = XAxisPositionBody.xDrive;
                xDrive.target = PositionDelta.x;
                XAxisPositionBody.xDrive = xDrive;
            }

            if (YAxisPositionBody)
            {
                // set joint Drive of Y axis to new position
                var yDrive = YAxisPositionBody.yDrive;
                yDrive.target = PositionDelta.y;
                YAxisPositionBody.yDrive = yDrive;
            }

            if (ZAxisPositionBody)
            {
                // set joint Drive of Z axis to new position
                var zDrive = ZAxisPositionBody.zDrive;
                zDrive.target = PositionDelta.z;
                ZAxisPositionBody.zDrive = zDrive;
            }

            if (YAxisRotationBody || XAxisRotationBody || ZAxisRotationBody || AllAxesRotationBody)
            {
                if (UseWorldRotation)
                {
                    RotationDelta = Target.rotation.eulerAngles;
                }
                else
                {
                    RotationDelta = new Vector3(
                        Target.localRotation.eulerAngles.x - StartingRotation.x,
                        Target.localRotation.eulerAngles.y - StartingRotation.y,
                        Target.localRotation.eulerAngles.z - StartingRotation.z
                    );
                }
            }

            // The axes have to be rotated in a specific order to work properly: Y->X->Z
            if (YAxisRotationBody)
            {
                // set joint Drive to new rotation
                var xDrive = YAxisRotationBody.xDrive;
                xDrive.target = FixAngleJump(xDrive, YAxisRotationBody.twistLock, RotationDelta.y, xDrive.target);
                YAxisRotationBody.xDrive = xDrive;
            }

            if (XAxisRotationBody)
            {
                // set joint Drive to new rotation
                var xDrive = XAxisRotationBody.xDrive;
                xDrive.target = FixAngleJump(xDrive, XAxisRotationBody.twistLock, RotationDelta.x, xDrive.target);
                XAxisRotationBody.xDrive = xDrive;
            }

            if (ZAxisRotationBody)
            {
                // set joint Drive to new rotation
                var xDrive = ZAxisRotationBody.xDrive;
                xDrive.target = FixAngleJump(xDrive, ZAxisRotationBody.twistLock, RotationDelta.z, xDrive.target);
                ZAxisRotationBody.xDrive = xDrive;
            }

            if (RotateUsingParentAnchor)
            {
                ArticulationBody.parentAnchorRotation = Target.rotation;
            }
            else if (AllAxesRotationBody)
            {
                var yDrive = AllAxesRotationBody.yDrive;
                yDrive.target = FixAngleJump(yDrive, AllAxesRotationBody.swingYLock, RotationDelta.y, yDrive.target);
                AllAxesRotationBody.yDrive = yDrive;

                var zDrive = AllAxesRotationBody.zDrive;
                zDrive.target = FixAngleJump(zDrive, AllAxesRotationBody.swingZLock, RotationDelta.z, zDrive.target);
                AllAxesRotationBody.zDrive = zDrive;

                var xDrive = AllAxesRotationBody.xDrive;
                xDrive.target = FixAngleJump(xDrive, AllAxesRotationBody.twistLock, RotationDelta.x, xDrive.target);
                AllAxesRotationBody.xDrive = xDrive;
            }
        }

        float FixAngleJump(ArticulationDrive drive, ArticulationDofLock dofLock, float targetAngleInRange, float currentAngle)
        {
            CurrentAngleInRange = currentAngle % 360f;
            if (CurrentAngleInRange < 0)
            {
                CurrentAngleInRange += 360;
            }
            if (targetAngleInRange < 0)
            {
                targetAngleInRange += 360;
            }
            CurrentAngleDifferenceInRange = Mathf.Abs(CurrentAngleInRange - targetAngleInRange);

            if (CurrentAngleDifferenceInRange < 0.1f)
            {
                return currentAngle;
            }

            if (CurrentAngleDifferenceInRange > 180f)
            {
                if (CurrentAngleInRange <= targetAngleInRange)
                {
                    OperationSign = -1;
                    CurrentAngleDifferenceInRange = CurrentAngleInRange + (360f - targetAngleInRange);
                }
                else
                {
                    OperationSign = 1;
                    CurrentAngleDifferenceInRange = targetAngleInRange + (360f - CurrentAngleInRange);
                }
            }
            else
            {
                if (CurrentAngleInRange <= targetAngleInRange)
                {
                    OperationSign = 1;
                }
                else
                {
                    OperationSign = -1;
                }
            }

            currentAngle += (float)OperationSign * CurrentAngleDifferenceInRange;

            return currentAngle;
        }
    }
}

