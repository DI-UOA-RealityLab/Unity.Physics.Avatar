
namespace NKUA.DI.RealityLab.Editor.Physics.Avatar
{
    using UnityEditor;
    using UnityEngine;
    using System.Linq;
    using NKUA.DI.RealityLab.Editor;
    using NKUA.DI.RealityLab.Physics.Avatar;

    [CustomPropertyDrawer(typeof(Finger))]
    public class FingerDrawer: PropertyDrawer
    {
        float buttonHeight = 20f;
        float buttonMarginLeft = 60f;
        float buttonMarginRight = 80f;
        float buttonMarginTop = -22f;
        float buttonMarginBottom = 5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded && property.FindPropertyRelative("bonesList").isExpanded)
            {
                // Calculate button rect
                var buttonRect = new Rect(
                    position.xMin + buttonMarginLeft,
                    position.yMax - buttonHeight - buttonMarginBottom,
                    position.width - buttonMarginLeft - buttonMarginRight,
                    buttonHeight
                );

                if (GUI.Button(buttonRect, "Apply default values"))
                {
                    var obj = ((ArticulationBodiesHand)property.serializedObject.targetObject).Hand.FingersList.ToArray<Finger>();
                    var index = property.propertyPath.GetIndexFromArrayPropertyPath();
                    Finger finger = ((Finger[])obj)[index];
                    finger.ApplyDefaultValuesToBones();
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded && property.FindPropertyRelative("bonesList").isExpanded)
                return EditorGUI.GetPropertyHeight(property) + buttonHeight + buttonMarginTop + buttonMarginBottom;
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}
