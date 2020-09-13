using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class HopRig : MonoBehaviour, IPointerDownHandler
{
    [SerializeField, Required] private GameObject _Cube, _Ring;
    [SerializeField, Required] private Transform _CubeDestinationPointRef;
    [SerializeField, Required] private float _RingHeight = 1, _CubeHeight = 1, _CubeJumpDuration = .5f, _RingJumpDuration = .5f;
    [SerializeField, Required] private AnimationCurve _CubeJumpCurve, _RingJumpCurve;
    [Space]
    [SerializeField, Required] private GameObject _TrajectoryPointPrefab;
    [SerializeField, Required] private int _TrajectoryCount = 100;

    
    [ShowInInspector, ReadOnly] private int _phase;
    private Vector3 _cubeStartPos;
    private Vector3 _ringStartPos;

    private void Awake()
    {
        _cubeStartPos = _Cube.transform.position;
        _ringStartPos = _Ring.transform.position;
    }

    private void Start()
    {
        DrawTrajectory();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_phase == 0)
        {
            LeanTween.cancel(_Ring);
            LeanTween.value(_Ring, val =>
            {
                var heightRange = _RingHeight - _ringStartPos.y;
                var height = _ringStartPos.y + (heightRange * val);
                _Ring.transform.position = new Vector3(_Ring.transform.position.x, height, _Ring.transform.position.z);

            }, 0, 1, _RingJumpDuration).setEase(_RingJumpCurve);
            
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

    private void DrawTrajectory()
    {
        for (var i = 0; i < _TrajectoryCount; i++)
        {
            var val = (float)i /_TrajectoryCount;

            var pos = GetCubePos(val);

            Instantiate(_TrajectoryPointPrefab, pos, Quaternion.identity);
            // Debug.DrawRay(pos, Vector3.up, Color.blue, 1);

        }
    }
}
