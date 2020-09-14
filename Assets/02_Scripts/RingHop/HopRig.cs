using System;
using System.Collections;
using System.Collections.Generic;
using RingHop;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = System.Random;

public class HopRig : MonoBehaviour
{
    public Action<bool> Finished;
    
    [SerializeField, Required] private ColliderWithEvent _SuccessCollider, _FailCollider;
    [SerializeField, Required] private GameObject _Cube, _Ring;
    [SerializeField, Required] private Transform _CubeDestinationPointRef;
    [SerializeField, Required] private float _RingHeight = 1, _CubeHeight = 1, _CubeJumpDuration = .5f, _RingJumpDuration = .5f;
    [SerializeField, Required] private AnimationCurve _CubeJumpCurve, _RingJumpCurve;
    [Space]
    [SerializeField, Required] private GameObject _TrajectoryPointPrefab;
    [SerializeField, Required] private int _TrajectoryCount = 100;
    [Space]
    [SerializeField, Required] private float _RandomFloatRange = 10;
    [SerializeField, Required] private LeanTweenType _RandomFloatEase = LeanTweenType.linear;
    [SerializeField, Required] private float _RandomFloatDuration = 1;
    [SerializeField, Required] private float _DrawDelay = .05f;

    [ShowInInspector, ReadOnly] private int _phase;
    private Vector3 _cubeStartPos;
    private Vector3 _ringStartPos;
    private List<GameObject> _trajectories = new List<GameObject>();

    private void Awake()
    {
        _cubeStartPos = _Cube.transform.position;
        _ringStartPos = _Ring.transform.position;
        
        _SuccessCollider.DidTouch += () =>
        {
            Finished?.Invoke(true);

        };
        _FailCollider.DidTouch += () =>
        {
            Finished?.Invoke(false);
            var pos = UnityEngine.Random.onUnitSphere * _RandomFloatRange;
            LeanTween.move(_Cube, pos, _RandomFloatDuration).setEase(_RandomFloatEase);
        };
    }

    private void Start()
    {
        CreateTrajectory();
        SetIsActive(false);
    }

    /*public void OnPointerDown(PointerEventData eventData)
    {
        Activate();
    }*/

    public void Trigger()
    {
        if (_phase == 0)
        {
            LeanTween.cancel(_Ring);
            LeanTween.value(_Ring, val =>
            {
                var heightRange = _RingHeight - _ringStartPos.y;
                var height = _ringStartPos.y + (heightRange * val);
                _Ring.transform.position = new Vector3(_Ring.transform.position.x, height, _Ring.transform.position.z);
            }, 0, 1, _RingJumpDuration).setEase(_RingJumpCurve).setOnComplete(() => _phase = 0);
        }
        else
        {
            LeanTween.cancel(_Cube);
            LeanTween.value(_Cube, val =>
            {
                var pos = GetCubePos(val);

                _Cube.transform.position = pos;
            }, 0, 1, _CubeJumpDuration);
        }

        _phase = _phase == 0 ? 1 : 0;
    }

    private Vector3 GetCubePos(float val)
    {
        var jumpVal = _CubeJumpCurve.Evaluate(val);

        var heightRange = _CubeHeight - _cubeStartPos.y;
        var height = _cubeStartPos.y + (heightRange * jumpVal);

        var pos = Vector3.Lerp(_cubeStartPos, _CubeDestinationPointRef.position, val);
        pos.y = height;
        return pos;
    }

    private void CreateTrajectory()
    {
        for (var i = 0; i < _TrajectoryCount; i++)
        {
            var val = (float)i /_TrajectoryCount;

            var pos = GetCubePos(val);

            _trajectories.Add(Instantiate(_TrajectoryPointPrefab, pos, Quaternion.identity));
            // Debug.DrawRay(pos, Vector3.up, Color.blue, 1);

        }
    }

    private IEnumerator SetTrajectoryActive(bool isActive)
    {
        foreach (var trajectory in _trajectories)
        {
            trajectory.SetActive(isActive);
            yield return new WaitForSeconds(_DrawDelay);
        }
    }

    public void SetIsActive(bool isActive)
    {
        _SuccessCollider.gameObject.SetActive(isActive);
        _FailCollider.gameObject.SetActive(isActive);
        StartCoroutine(SetTrajectoryActive(isActive));
    }
}
