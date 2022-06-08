
namespace NKUA.DI.RealityLab.Physics.Avatar
{
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class Hand
    {
        public enum HandType
        {
            Left,
            Right
        }

        [SerializeField]
        private HandType type;
        public HandType Type {get {return type;} set {type = value;}}

        [SerializeField]
        private Palm palm;
        public Palm Palm {get {return palm;} set {palm = value;}}

        [SerializeField]
        private List<Finger> fingersList;
        public List<Finger> FingersList {get {return fingersList;} set {fingersList = value;}}
    }
}
