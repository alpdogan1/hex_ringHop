using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TetrisRun
{
    public class Exploder: MonoBehaviour
    {
        [SerializeField] private float _Magnitude = 1;
        [SerializeField] private LeanTweenType _ResetEasing = LeanTweenType.easeInOutSine;
        [SerializeField, ReadOnly] private Rigidbody[] _children;
        [SerializeField, ReadOnly] private Vector3[] _childrenOriginalPositions;

        private void OnValidate()
        {
            _children = GetComponentsInChildren<Rigidbody>();
            _childrenOriginalPositions = _children.Select(rigidbody1 => rigidbody1.transform.localPosition).ToArray();
        }

        [Button]
        public void ExplodeNow()
        {
            foreach (var child in _children)
            {
                child.AddForce(Random.insideUnitSphere * _Magnitude, ForceMode.Impulse);
            }
        }

        [Button]
        public void Reset()
        {
            var childrenCurrentPos = _children.Select(rigidbody1 => rigidbody1.transform.localPosition).ToArray();

            _children.ForEach(rigidbody1 => rigidbody1.velocity = Vector3.zero);
            
            LeanTween.value(gameObject, 0, 1, .3f).setOnUpdate(val =>
            {
                for (var index = 0; index < _children.Length; index++)
                {
                    var child = _children[index];
                    child.transform.localPosition = Vector3.Lerp(childrenCurrentPos[index], _childrenOriginalPositions[index], val);
                }
            }).setEase(_ResetEasing);
        }
    }
}