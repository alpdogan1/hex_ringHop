using System;
using System.Collections;
using System.Collections.Generic;
using RingHop;
using Sirenix.OdinInspector;
using TetrisRun;
using UnityEngine;
using Random = System.Random;

public class HopRig : MonoBehaviour
{
    public Action<bool> Finished;
    
    [SerializeField, Required] private Exploder _Exploder;
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
    [SerializeField] private float _RingFreezeDuration = .2f;
    [Space]
    [SerializeField, Required] private ParticleSystem _CubeParticle, _RingParticle, _SuccessParticle,  _CubeActiveParticle, _RingActiveParticle;
    [SerializeField, Required]private float _CubeParticleDuration;
    [Space]
    [SerializeField] private bool _IsTutorial;
    [SerializeField, ShowIf("_IsTutorial")] private float _TutorialDelay = .4f;
    [Space]
    [ShowInInspector, ReadOnly] private int _phase;
    
    private Vector3 _cubeStartPos;
    private Vector3 _ringStartPos;
    private List<GameObject> _trajectories = new List<GameObject>();
    private bool _pauseTween;
    private int? _currentTutorialTweenId;
    private bool _isTutorialWait;

    private void Awake()
    {
        _cubeStartPos = _Cube.transform.position;
        _ringStartPos = _Ring.transform.position;
        
        _SuccessCollider.DidTouch += () =>
        {
            LeanTween.pause(_Ring);
            // _pauseTween = true;
            LeanTween.delayedCall(_RingFreezeDuration, () =>
            {
                LeanTween.resume(_Ring);
                // _pauseTween = false;
            });
            
            Finished?.Invoke(true);

            if (_SuccessParticle)
                _SuccessParticle.Play();
        };
        _FailCollider.DidTouch += () =>
        {
            Finished?.Invoke(false);
            var pos = UnityEngine.Random.onUnitSphere * _RandomFloatRange;
            LeanTween.cancel(_Cube);
            // LeanTween.move(_Cube, pos, _RandomFloatDuration).setEase(_RandomFloatEase);
            _Exploder.ExplodeNow();
        };
        
        _CubeParticle.Stop();
        _RingParticle.Stop();
        
        CreateTrajectory();
        SetIsActive(false);
    }

    /*public void OnPointerDown(PointerEventData eventData)
    {
        Activate();
    }*/

    public void Trigger()
    {
        if(_IsTutorial && _isTutorialWait) return;
        
        if (_phase == 0)
        {
            _RingActiveParticle.Play();
            LeanTween.cancel(_Ring);
            var cubeTweenId = LeanTween.value(_Ring, val =>
            {
                if(_pauseTween) return;
                var heightRange = _RingHeight - _ringStartPos.y;
                var height = _ringStartPos.y + (heightRange * val);
                _Ring.transform.position = new Vector3(_Ring.transform.position.x, height, _Ring.transform.position.z);
            }, 0, 1, _RingJumpDuration).setEase(_RingJumpCurve)
                .setOnComplete(() =>
                {
                    _phase = 0;
                    _RingActiveParticle.Stop();
                    UpdateParticles();
                }).id;


            if (_IsTutorial)
            {
                _isTutorialWait = true;
                
                LeanTween.delayedCall(gameObject, _TutorialDelay, () =>
                {
                    _isTutorialWait = false;
                    LeanTween.pause(cubeTweenId);
                    _currentTutorialTweenId = cubeTweenId;
                    HopTutorialPanel.SetPhaseVisible(1);
                });
            }
            
        }
        else
        {
            if(_currentTutorialTweenId.HasValue)
            {
                LeanTween.resume(_currentTutorialTweenId.Value);
                _currentTutorialTweenId = null;
            }
            
            _CubeActiveParticle.Play();
            LeanTween.delayedCall(gameObject, _CubeParticleDuration, () => _CubeActiveParticle.Stop());
            LeanTween.cancel(_Cube);
            LeanTween.value(_Cube, val =>
            {
                var pos = GetCubePos(val);

                _Cube.transform.position = pos;
            }, 0, 1, _CubeJumpDuration);
        }

        _phase = _phase == 0 ? 1 : 0;

        // Particles
        UpdateParticles();
    }

    private void UpdateParticles()
    {
        if (_phase == 0)
        {
            _CubeParticle.Stop();
            _RingParticle.Play();
        }
        else
        {
            _CubeParticle.Play();
            _RingParticle.Stop();
        }
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

        if(isActive && _IsTutorial)
        {
            HopTutorialPanel.SetPhaseVisible(0);
        }

        if (!isActive && _IsTutorial)
        {
            HopTutorialPanel.SetAllInvisible();
        }

        // Particles
        if (isActive)
            _RingParticle.Play();
        else
            _RingParticle.Stop();
        
        _CubeParticle.Stop();
    }
}
