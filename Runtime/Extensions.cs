
namespace NKUA.DI.RealityLab
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public static class Extensions
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) => self?.Select((item, index) => (item, index)) ?? new List<(T, int)>();

        public static string ReplaceAt(this string input, int index, char newChar)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }

        public static Transform FindExactChildRecursive(this Transform parent, string objectName)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Equals(objectName))
                    return child;

                var result = FindExactChildRecursive(child, objectName);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static void ResetLocalPosition(this Transform gameObjectTransform)
        {
            gameObjectTransform.localPosition = Vector3.zero;
        }

        public static void ResetLocalRotation(this Transform gameObjectTransform)
        {
            gameObjectTransform.localRotation = Quaternion.identity;
        }

        public static void ResetLocalScale(this Transform gameObjectTransform)
        {
            gameObjectTransform.localScale = Vector3.one;
        }

        public static void ResetLocalTransform(this Transform gameObjectTransform)
        {
            ResetLocalPosition(gameObjectTransform);
            ResetLocalRotation(gameObjectTransform);
            ResetLocalScale(gameObjectTransform);
        }

        public static void SetLocalPosition(this Transform targetTransform, Vector3 sourcePosition)
        {
            targetTransform.localPosition = new Vector3(
                sourcePosition.x,
                sourcePosition.y,
                sourcePosition.z
            );
        }

        public static void SetLocalRotation(this Transform targetTransform, Vector3 sourceRotation)
        {
            targetTransform.localRotation = Quaternion.Euler(
                sourceRotation.x,
                sourceRotation.y,
                sourceRotation.z
            );
        }

        public static Vector3 GetNewVector3(this Vector3 target)
        {
            return new Vector3(target.x, target.y, target.z);
        }
    }
}
