
namespace NKUA.DI.RealityLab.Editor.Physics.Avatar
{
    using UnityEditor;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using NKUA.DI.RealityLab.Physics.Avatar;

    [CustomEditor(typeof(ArticulationBodiesHand), true)]
    public class ArticulationBodiesHandEditor : Editor {
        private ArticulationBodiesHand ArticulationBodiesHand;

        SerializedProperty AnimatedHandModelProperty;
        SerializedProperty ArticulationBodiesConfigurationProperty;
        SerializedProperty NoCollisionLayerProperty;
        SerializedProperty CloneHandWithPhysicsProperty;
        SerializedProperty HandProperty;


        SerializedProperty HandTypeProperty;
        SerializedProperty HandPalmProperty;
        SerializedProperty HandFingersProperty;

        SerializedProperty HandPalmRootProperty;
        SerializedProperty HandPalmColliderProperty;
        SerializedProperty HandPalmColliderCenterProperty;
        SerializedProperty HandPalmColliderSizeProperty;
        SerializedProperty HandPalmMeshColliderProperty;

        private bool HandFoldout = true;
        private bool PalmFoldout = true;
        private bool FingersFoldout = true;
        int FingerListSize = 0;
        private Dictionary<int, bool> FingerListFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, bool> BonesFoldouts = new Dictionary<int, bool>();
        private Dictionary<int, int> BoneListSizes = new Dictionary<int, int>();
        private Dictionary<string, bool> BoneListFoldouts = new Dictionary<string, bool>();

        private void OnEnable()
        {
            ArticulationBodiesHand = (ArticulationBodiesHand)target;

            AnimatedHandModelProperty = serializedObject.FindProperty("animatedHandModel");
            ArticulationBodiesConfigurationProperty = serializedObject.FindProperty("articulationBodiesConfiguration");
            NoCollisionLayerProperty = serializedObject.FindProperty("noCollisionLayer");
            CloneHandWithPhysicsProperty = serializedObject.FindProperty("cloneHandWithPhysics");
            HandProperty = serializedObject.FindProperty("hand");

            HandTypeProperty = HandProperty.FindPropertyRelative("type");
            HandPalmProperty = HandProperty.FindPropertyRelative("palm");
            HandFingersProperty = HandProperty.FindPropertyRelative("fingersList");

            HandPalmRootProperty = HandPalmProperty.FindPropertyRelative("root");
            HandPalmColliderProperty = HandPalmProperty.FindPropertyRelative("collider");
            HandPalmColliderCenterProperty = HandPalmProperty.FindPropertyRelative("colliderCenter");
            HandPalmColliderSizeProperty = HandPalmProperty.FindPropertyRelative("colliderSize");
            HandPalmMeshColliderProperty = HandPalmProperty.FindPropertyRelative("meshCollider");
        }

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();

            DrawArticulationBodiesHandProperties();

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawArticulationBodiesHandProperties()
        {
            AnimatedHandModelProperty.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField(
                "Animated Hand Model",
                AnimatedHandModelProperty.objectReferenceValue,
                typeof(GameObject),
                true
            );
            HandTypeProperty.enumValueIndex = (int)(Hand.HandType)EditorGUILayout.EnumPopup("Hand Type", (Hand.HandType)HandTypeProperty.enumValueIndex);
            if (ArticulationBodiesHand.AnimatedHandModel != (GameObject)AnimatedHandModelProperty.objectReferenceValue ||
                (Hand.HandType)HandTypeProperty.enumValueIndex != ArticulationBodiesHand.Hand.Type)
            {
                OnAnimatedHandChange();
            }

            EditorGUILayout.PropertyField(ArticulationBodiesConfigurationProperty);

            EditorGUILayout.PropertyField(NoCollisionLayerProperty);

            if (AnimatedHandModelProperty.objectReferenceValue)
            {
                HandFoldout = EditorGUILayout.Foldout(HandFoldout, "Hand", true);
                if (HandFoldout)
                {
                    EditorGUI.indentLevel++;

                    PalmFoldout = EditorGUILayout.Foldout(PalmFoldout, "Palm", true);
                    if (PalmFoldout)
                    {
                        EditorGUI.indentLevel++;

                        DrawPalmProperties();

                        EditorGUI.indentLevel--;
                    }

                    FingersFoldout = EditorGUILayout.Foldout(FingersFoldout, "Fingers", true);
                    if (FingersFoldout)
                    {
                        EditorGUI.indentLevel++;

                        ManageListSize(ref FingerListSize, HandFingersProperty);

                        for (int i = 0; i < HandFingersProperty.arraySize; i++)
                        {
                            if (!FingerListFoldouts.ContainsKey(i)) FingerListFoldouts.Add(i, true);
                            FingerListFoldouts[i] = EditorGUILayout.Foldout(FingerListFoldouts[i], "Finger " + (i + 1), true);
                            if (FingerListFoldouts[i])
                            {
                                EditorGUI.indentLevel++;

                                DrawFingerProperties(HandFingersProperty.GetArrayElementAtIndex(i), i);

                                EditorGUI.indentLevel--;
                            }
                        }

                        EditorGUI.indentLevel--;
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }

        private void OnAnimatedHandChange()
        {
            // if a cloned physics hand exists destroy it
            if (ArticulationBodiesHand.CloneHandWithPhysics)
            {
                Undo.DestroyObjectImmediate(CloneHandWithPhysicsProperty.objectReferenceValue);
                CloneHandWithPhysicsProperty.objectReferenceValue = null;
            }

            if (!AnimatedHandModelProperty.objectReferenceValue)
            {
                return;
            }

            // Create a clone of target hand that will be used to add physics to it
            CloneHandWithPhysicsProperty.objectReferenceValue = Instantiate((GameObject)AnimatedHandModelProperty.objectReferenceValue);
            ((GameObject)CloneHandWithPhysicsProperty.objectReferenceValue).transform.SetParent(ArticulationBodiesHand.transform);

            // Disable the animator component on the cloned hand root if any
            Animator cloneHandWithPhysicsAnimator = ((GameObject)CloneHandWithPhysicsProperty.objectReferenceValue).GetComponent<Animator>();
            if (cloneHandWithPhysicsAnimator)
            {
                cloneHandWithPhysicsAnimator.enabled = false;
            }
            else
            {
                // If there isn't an animator component on the cloned hand root search the children and if any disable it
                cloneHandWithPhysicsAnimator = ((GameObject)CloneHandWithPhysicsProperty.objectReferenceValue).GetComponentInChildren<Animator>();
                if (cloneHandWithPhysicsAnimator)
                {
                    cloneHandWithPhysicsAnimator.enabled = false;
                }
            }

            Undo.RegisterCreatedObjectUndo(((GameObject)CloneHandWithPhysicsProperty.objectReferenceValue), "Created cloned hand with physics");
        }

        private void DrawPalmProperties()
        {
            HandPalmRootProperty.objectReferenceValue = (Transform)EditorGUILayout.ObjectField(
                "Root",
                HandPalmRootProperty.objectReferenceValue,
                typeof(Transform),
                true
            );
            if ((Transform)HandPalmRootProperty.objectReferenceValue != ArticulationBodiesHand.Hand.Palm.Root)
            {
                OnPalmRootChange();
            }

            HandPalmColliderCenterProperty.vector3Value = EditorGUILayout.Vector3Field(
                "Collider Center",
                HandPalmColliderCenterProperty.vector3Value
            );
            if (HandPalmColliderCenterProperty.vector3Value != ArticulationBodiesHand.Hand.Palm.ColliderCenter)
            {
                OnPalmColliderCenterChange();
            }

            HandPalmColliderSizeProperty.vector3Value = EditorGUILayout.Vector3Field(
                "Collider Size",
                HandPalmColliderSizeProperty.vector3Value
            );
            if (HandPalmColliderSizeProperty.vector3Value != ArticulationBodiesHand.Hand.Palm.ColliderSize)
            {
                OnPalmColliderSizeChange();
            }

            EditorGUILayout.PropertyField(HandPalmMeshColliderProperty);
        }

        private void OnPalmRootChange()
        {
            if (HandPalmColliderProperty.objectReferenceValue)
            {
                Undo.DestroyObjectImmediate(HandPalmColliderProperty.objectReferenceValue);
                HandPalmColliderProperty.objectReferenceValue = null;
            }

            if (!HandPalmRootProperty.objectReferenceValue)
            {
                return;
            }

            HandPalmColliderProperty.objectReferenceValue = ((Transform)HandPalmRootProperty.objectReferenceValue).gameObject.AddComponent<BoxCollider>();
            ((BoxCollider)HandPalmColliderProperty.objectReferenceValue).center = HandPalmColliderCenterProperty.vector3Value;
            ((BoxCollider)HandPalmColliderProperty.objectReferenceValue).size = HandPalmColliderSizeProperty.vector3Value;

            Undo.RegisterCreatedObjectUndo(HandPalmColliderProperty.objectReferenceValue, "Created hand palm collider");
        }

        private void OnPalmColliderCenterChange()
        {
            if (!HandPalmColliderProperty.objectReferenceValue)
            {
                return;
            }

            Undo.RecordObject(((BoxCollider)HandPalmColliderProperty.objectReferenceValue), "Change center of hand palm collider");
            ((BoxCollider)HandPalmColliderProperty.objectReferenceValue).center = HandPalmColliderCenterProperty.vector3Value;
        }

        private void OnPalmColliderSizeChange()
        {
            if (!HandPalmColliderProperty.objectReferenceValue)
            {
                return;
            }

            Undo.RecordObject(((BoxCollider)HandPalmColliderProperty.objectReferenceValue), "Change size of hand palm collider");
            ((BoxCollider)HandPalmColliderProperty.objectReferenceValue).size = HandPalmColliderSizeProperty.vector3Value;
        }

        private void DrawFingerProperties(SerializedProperty fingerProperty, int fingerIndex)
        {
            SerializedProperty fingerRootProperty = fingerProperty.FindPropertyRelative("root");
            fingerRootProperty.objectReferenceValue = (Transform)EditorGUILayout.ObjectField(
                "Root",
                fingerRootProperty.objectReferenceValue,
                typeof(Transform),
                true
            );
            if (fingerIndex >= 0 && fingerIndex < ArticulationBodiesHand.Hand.FingersList.Count &&
                (Transform)fingerRootProperty.objectReferenceValue != ArticulationBodiesHand.Hand.FingersList[fingerIndex].Root)
            {
                OnFingerRootChange(fingerProperty, fingerRootProperty);
            }

            SerializedProperty fingerTypeProperty = fingerProperty.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(fingerTypeProperty);

            SerializedProperty fingerDefaultBoneWidthProperty = fingerProperty.FindPropertyRelative("defaultBoneWidth");
            EditorGUILayout.PropertyField(fingerDefaultBoneWidthProperty);
            // GUILayout.EndHorizontal();

            SerializedProperty fingerDefaultBoneShapeProperty = fingerProperty.FindPropertyRelative("defaultBoneShape");
            EditorGUILayout.PropertyField(fingerDefaultBoneShapeProperty);

            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(EditorGUIUtility.labelWidth));
            if (GUILayout.Button("Apply default bone values", GUILayout.Height(25)))
            {
                OnApplyDefaultBoneValues(fingerProperty);
            }
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel++;

            if (!BonesFoldouts.ContainsKey(fingerIndex)) BonesFoldouts.Add(fingerIndex, true);
            BonesFoldouts[fingerIndex] = EditorGUILayout.Foldout(BonesFoldouts[fingerIndex], "Bones", true);
            if (BonesFoldouts[fingerIndex])
            {
                EditorGUI.indentLevel++;

                SerializedProperty fingerBonesListProperty = fingerProperty.FindPropertyRelative("bonesList");
                if (!BoneListSizes.ContainsKey(fingerIndex)) BoneListSizes.Add(fingerIndex, 0);
                BoneListSizes.TryGetValue(fingerIndex, out int boneListSize);
                ManageListSize(ref boneListSize, fingerBonesListProperty);

                for (int boneIndex = 0; boneIndex < fingerBonesListProperty.arraySize; boneIndex++)
                {
                    if (!BoneListFoldouts.ContainsKey(fingerIndex + "," + boneIndex)) BoneListFoldouts.Add(fingerIndex + "," + boneIndex, true);
                    BoneListFoldouts[fingerIndex + "," + boneIndex] = EditorGUILayout.Foldout(BoneListFoldouts[fingerIndex + "," + boneIndex], "Bone " + (boneIndex + 1), true);
                    if (BoneListFoldouts[fingerIndex + "," + boneIndex])
                    {
                        EditorGUI.indentLevel++;

                        DrawBoneProperties(fingerBonesListProperty.GetArrayElementAtIndex(boneIndex), fingerIndex, boneIndex);

                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        void OnApplyDefaultBoneValues(SerializedProperty fingerProperty)
        {
            SerializedProperty fingerBonesListProperty = fingerProperty.FindPropertyRelative("bonesList");

            for (int boneIndex = 0; boneIndex < fingerBonesListProperty.arraySize; boneIndex++)
            {
                fingerBonesListProperty.GetArrayElementAtIndex(boneIndex).FindPropertyRelative("width").floatValue = fingerProperty.FindPropertyRelative("defaultBoneWidth").floatValue;
                fingerBonesListProperty.GetArrayElementAtIndex(boneIndex).FindPropertyRelative("shape").enumValueIndex = fingerProperty.FindPropertyRelative("defaultBoneShape").enumValueIndex;

                OnBoneRootChange(fingerBonesListProperty.GetArrayElementAtIndex(boneIndex), fingerBonesListProperty.GetArrayElementAtIndex(boneIndex).FindPropertyRelative("root"));
            }
        }

        private void OnFingerRootChange(SerializedProperty fingerProperty, SerializedProperty fingerRootProperty)
        {
            SerializedProperty currentFingerBoneList = fingerProperty.FindPropertyRelative("bonesList");
            // destroy all bone colliders and then clear the list
            for (int i = 0; i < currentFingerBoneList.arraySize; i++)
            {
                SerializedProperty boneColliderProperty = currentFingerBoneList.GetArrayElementAtIndex(i).FindPropertyRelative("collider");
                if (boneColliderProperty != null && boneColliderProperty.objectReferenceValue)
                {
                    Undo.DestroyObjectImmediate(boneColliderProperty.objectReferenceValue);
                    boneColliderProperty.objectReferenceValue = null;
                }
            }
            currentFingerBoneList.ClearArray();

            // If there is still a collider on the Root it means that another bone is connected
            // or something has gone wrong so set the root to null and stop the operation here.
            if (fingerRootProperty.objectReferenceValue && ((Transform)fingerRootProperty.objectReferenceValue).GetComponent<Collider>())
            {
                fingerRootProperty.objectReferenceValue = null;
                return;
            }

            // if root set to null don't do anything else after bonesList got cleared
            if (!fingerRootProperty.objectReferenceValue)
            {
                return;
            }

            // auto-assign finger type automatically by the name of the new root
            foreach (FingerType fingerType in Enum.GetValues(typeof(FingerType)))
            {
                if (((Transform)fingerRootProperty.objectReferenceValue).name.IndexOf(fingerType.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    fingerProperty.FindPropertyRelative("type").enumValueIndex = (int)fingerType;
                }
            }

            // create bonesList automatically and set the appropriate and default properties to each one of them
            Transform currentRootBone = ((Transform)fingerRootProperty.objectReferenceValue);
            int boneIndex = 0;
            while (currentRootBone.childCount > 0)
            {
                float boneLength = (currentRootBone.position - currentRootBone.GetChild(0).position).magnitude;

                currentFingerBoneList.InsertArrayElementAtIndex(boneIndex);

                currentFingerBoneList.GetArrayElementAtIndex(boneIndex).FindPropertyRelative("root").objectReferenceValue = currentRootBone;
                currentFingerBoneList.GetArrayElementAtIndex(boneIndex).FindPropertyRelative("length").floatValue = boneLength;
                currentFingerBoneList.GetArrayElementAtIndex(boneIndex).FindPropertyRelative("width").floatValue = fingerProperty.FindPropertyRelative("defaultBoneWidth").floatValue;
                currentFingerBoneList.GetArrayElementAtIndex(boneIndex).FindPropertyRelative("shape").enumValueIndex = fingerProperty.FindPropertyRelative("defaultBoneShape").enumValueIndex;
                currentFingerBoneList.GetArrayElementAtIndex(boneIndex).FindPropertyRelative("collider").objectReferenceValue = null;

                OnBoneRootChange(currentFingerBoneList.GetArrayElementAtIndex(boneIndex), currentFingerBoneList.GetArrayElementAtIndex(boneIndex).FindPropertyRelative("root"));

                currentRootBone = currentRootBone.GetChild(0);
                boneIndex++;
            }
        }

        private void DrawBoneProperties(SerializedProperty boneProperty, int fingerIndex, int boneIndex)
        {
            SerializedProperty boneRootProperty = boneProperty.FindPropertyRelative("root");
            boneRootProperty.objectReferenceValue = (Transform)EditorGUILayout.ObjectField(
                "Root",
                boneRootProperty.objectReferenceValue,
                typeof(Transform),
                true
            );
            if (fingerIndex >= 0 && fingerIndex < ArticulationBodiesHand.Hand.FingersList.Count &&
                boneIndex >= 0 && boneIndex < ArticulationBodiesHand.Hand.FingersList[fingerIndex].BonesList.Count &&
                (Transform)boneRootProperty.objectReferenceValue != ArticulationBodiesHand.Hand.FingersList[fingerIndex].BonesList[boneIndex].Root)
            {
                OnBoneRootChange(boneProperty, boneRootProperty);
            }

            GUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(true);

            SerializedProperty boneLengthProperty = boneProperty.FindPropertyRelative("length");
            EditorGUILayout.PropertyField(boneLengthProperty);

            EditorGUI.EndDisabledGroup();

            SerializedProperty boneWidthProperty = boneProperty.FindPropertyRelative("width");
            boneWidthProperty.floatValue = EditorGUILayout.FloatField("Width", boneWidthProperty.floatValue);
            if (fingerIndex >= 0 && fingerIndex < ArticulationBodiesHand.Hand.FingersList.Count &&
                boneIndex >= 0 && boneIndex < ArticulationBodiesHand.Hand.FingersList[fingerIndex].BonesList.Count &&
                boneWidthProperty.floatValue != ArticulationBodiesHand.Hand.FingersList[fingerIndex].BonesList[boneIndex].Width)
            {
                OnBoneWidthChange(boneProperty, boneWidthProperty);
            }

            GUILayout.EndHorizontal();

            SerializedProperty boneShapeProperty = boneProperty.FindPropertyRelative("shape");
            boneShapeProperty.intValue = (int)(Shape)EditorGUILayout.EnumPopup("Shape", (Shape)boneShapeProperty.intValue);
            if (fingerIndex >= 0 && fingerIndex < ArticulationBodiesHand.Hand.FingersList.Count &&
                boneIndex >= 0 && boneIndex < ArticulationBodiesHand.Hand.FingersList[fingerIndex].BonesList.Count &&
                (Shape)boneShapeProperty.enumValueIndex != ArticulationBodiesHand.Hand.FingersList[fingerIndex].BonesList[boneIndex].Shape)
            {
                OnBoneRootChange(boneProperty, boneRootProperty);
            }

            EditorGUI.BeginDisabledGroup(true);

            SerializedProperty boneColliderProperty = boneProperty.FindPropertyRelative("collider");
            EditorGUILayout.PropertyField(boneColliderProperty);

            EditorGUI.EndDisabledGroup();
        }

        private void OnBoneRootChange(SerializedProperty boneProperty, SerializedProperty boneRootProperty)
        {
            SerializedProperty boneColliderProperty = boneProperty.FindPropertyRelative("collider");
            // If there is still a collider on the Root it means that another bone is connected
            // or something has gone wrong so set the root to null and stop the operation here.
            if (boneRootProperty.objectReferenceValue && !boneColliderProperty.objectReferenceValue && ((Transform)boneRootProperty.objectReferenceValue).GetComponent<Collider>())
            {
                boneRootProperty.objectReferenceValue = null;
                return;
            }

            if (boneColliderProperty.objectReferenceValue)
            {
                Undo.DestroyObjectImmediate(boneColliderProperty.objectReferenceValue);
                boneColliderProperty.objectReferenceValue = null;
            }

            if (!boneRootProperty.objectReferenceValue)
            {
                return;
            }

            SerializedProperty boneLengthProperty = boneProperty.FindPropertyRelative("length");
            SerializedProperty boneWidthProperty = boneProperty.FindPropertyRelative("width");
            SerializedProperty boneShapeProperty = boneProperty.FindPropertyRelative("shape");

            Transform rootBone = ((Transform)boneRootProperty.objectReferenceValue);
            if (rootBone.childCount > 0)
            {
                boneLengthProperty.floatValue = (rootBone.position - rootBone.GetChild(0).position).magnitude;
            }

            Hand.HandType handType = (Hand.HandType) HandTypeProperty.enumValueIndex;
            Vector3 newCenter = new Vector3(boneLengthProperty.floatValue/2f, 0, 0);
            if (handType == Hand.HandType.Left)
            {
                newCenter.x = -newCenter.x;
            }

            if ((Shape)boneShapeProperty.enumValueIndex == Shape.Box)
            {
                boneColliderProperty.objectReferenceValue = ((Transform)boneRootProperty.objectReferenceValue).gameObject.AddComponent<BoxCollider>();
                ((BoxCollider)boneColliderProperty.objectReferenceValue).center = newCenter;
                ((BoxCollider)boneColliderProperty.objectReferenceValue).size = new Vector3(boneLengthProperty.floatValue, boneWidthProperty.floatValue, boneWidthProperty.floatValue);
            }
            else if ((Shape)boneShapeProperty.enumValueIndex == Shape.Capsule)
            {
                boneColliderProperty.objectReferenceValue = ((Transform)boneRootProperty.objectReferenceValue).gameObject.AddComponent<CapsuleCollider>();
                ((CapsuleCollider)boneColliderProperty.objectReferenceValue).center = newCenter;
                ((CapsuleCollider)boneColliderProperty.objectReferenceValue).radius = boneWidthProperty.floatValue;
                ((CapsuleCollider)boneColliderProperty.objectReferenceValue).height = boneLengthProperty.floatValue;
                ((CapsuleCollider)boneColliderProperty.objectReferenceValue).direction = 0;
            }
            else if ((Shape)boneShapeProperty.enumValueIndex == Shape.Sphere)
            {
                boneColliderProperty.objectReferenceValue = ((Transform)boneRootProperty.objectReferenceValue).gameObject.AddComponent<SphereCollider>();
                ((SphereCollider)boneColliderProperty.objectReferenceValue).center = newCenter;
                ((SphereCollider)boneColliderProperty.objectReferenceValue).radius = boneWidthProperty.floatValue;
            }

            Undo.RegisterCreatedObjectUndo(boneColliderProperty.objectReferenceValue, "Created bone collider");
        }

        private void OnBoneWidthChange(SerializedProperty boneProperty, SerializedProperty boneWidthProperty)
        {
            SerializedProperty boneColliderProperty = boneProperty.FindPropertyRelative("collider");
            if (!boneColliderProperty.objectReferenceValue)
            {
                return;
            }

            SerializedProperty boneLengthProperty = boneProperty.FindPropertyRelative("length");
            SerializedProperty boneShapeProperty = boneProperty.FindPropertyRelative("shape");

            if ((Shape)boneShapeProperty.enumValueIndex == Shape.Box)
            {
                Undo.RecordObject(((BoxCollider)boneColliderProperty.objectReferenceValue), "Change size of bone box collider");
                ((BoxCollider)boneColliderProperty.objectReferenceValue).size = new Vector3(boneLengthProperty.floatValue, boneWidthProperty.floatValue, boneWidthProperty.floatValue);
            }
            else if ((Shape)boneShapeProperty.enumValueIndex == Shape.Capsule)
            {
                Undo.RecordObject(((CapsuleCollider)boneColliderProperty.objectReferenceValue), "Change radious of bone capsule collider");
                ((CapsuleCollider)boneColliderProperty.objectReferenceValue).radius = boneWidthProperty.floatValue;
            }
            else if ((Shape)boneShapeProperty.enumValueIndex == Shape.Capsule)
            {
                Undo.RecordObject(((SphereCollider)boneColliderProperty.objectReferenceValue), "Change radious of bone sphere collider");
                ((SphereCollider)boneColliderProperty.objectReferenceValue).radius = boneWidthProperty.floatValue;
            }
        }

        void ManageListSize(ref int listSize, SerializedProperty listProperty)
        {
            listSize = listProperty.arraySize;
            listSize = EditorGUILayout.IntField("List Size", listSize);

            if (listSize != listProperty.arraySize)
            {
                while (listSize > listProperty.arraySize)
                {
                    listProperty.InsertArrayElementAtIndex(listProperty.arraySize);

                    UpdateList(listProperty, true);
                }
                while (listSize < listProperty.arraySize)
                {
                    UpdateList(listProperty);

                    listProperty.DeleteArrayElementAtIndex(listProperty.arraySize - 1);
                }
            }
        }

        void UpdateList(SerializedProperty listProperty, bool isInsertion = false)
        {
            if (listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).FindPropertyRelative("root") != null)
            {
                listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).FindPropertyRelative("root").objectReferenceValue = null;

                if (listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).type.Equals("Finger"))
                {
                    listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).FindPropertyRelative("bonesList").arraySize = 0;

                    OnFingerRootChange(
                        listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1),
                        listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).FindPropertyRelative("root")
                    );
                }

                if (listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).type.Equals("Bone"))
                {
                    if (isInsertion)
                    {
                        listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).FindPropertyRelative("collider").objectReferenceValue = null;
                    }

                    OnBoneRootChange(
                        listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1),
                        listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).FindPropertyRelative("root")
                    );
                }
            }
        }
    }
}
