using System;
using System.Linq;
using Cureviz.View.Common.TweenAlphaSetActive;
using Hex.Modules;
using Sirenix.OdinInspector;
using UnityEngine;

public class Game : MonoBehaviour
{
    #region Development
#if UNITY_EDITOR
    [Button]
    private void GetCamOffset(Transform rig)
    {
        _RigOffset = _Camera.transform.position - rig.position;
    }
#endif
    #endregion
    
    [SerializeField, Required] private HopRig[] _Rigs;
    [SerializeField, Required] private GameObject _Camera;
    [SerializeField] private Vector3 _RigOffset;
    [SerializeField] private float _CameraMoveDuration = .3f;
    [SerializeField] private float _RigIterationDelay = .3f;
    [SerializeField, Required] private SceneLevelManager _LevelManager;

    [ReadOnly, ShowInInspector] private int _currentRigIndex = -1;
    [ReadOnly, ShowInInspector] private bool _isPlaying;
    
    [SerializeField, Required] private CanvasGroupTweenAlphaSetActiveHandler _UiPanel;

    private HopRig ActiveRig
    {
        get
        {
            if (_currentRigIndex > _Rigs.Length - 1 || _currentRigIndex < 0) return null;
            return _Rigs[_currentRigIndex];
        }
    }

    private static bool IsDown => Input.GetMouseButtonDown(0);

    private void Update()
    {
        if (_isPlaying && IsDown)
            TriggerActiveRig();
    }

    [Button]
    public void StartGame()
    {
        _LevelManager.LoadCurrentLevelScene(() =>
        {
            // _Rigs = FindObjectsOfType<HopRig>();
            var sortedRigs = FindObjectsOfType<HopRig>().ToList();
            sortedRigs.Sort((rig1, rig2)=> rig1.transform.position.x - rig2.transform.position.x < 0 ? -1 : 1);
            _Rigs = sortedRigs.ToArray();
            
            DeactivateRig(ActiveRig);
            _currentRigIndex = 0;
            ActivateRig(ActiveRig);
        
            _isPlaying = true;
            MoveCam();
        
            _UiPanel.SetIsActive(false);
        });
    }

    public void StopGame()
    {
        if(!_isPlaying) return;
        DeactivateRig(ActiveRig);
        _currentRigIndex = 0;
        MoveCam();
        _isPlaying = false;
        _UiPanel.SetIsActive(true);
    }
    
    private void DeactivateRig(HopRig rig)
    {
        if(!rig) return;
        rig.Finished -= RigFinished;
        rig.SetIsActive(false);
    }

    private void ActivateRig(HopRig rig)
    {
        if(!rig) return;
        rig.Finished += RigFinished;
        rig.SetIsActive(true);
    }

    private void RigFinished(bool isSuccess)
    {
        if (isSuccess)
        {
            IterateRig();
        }
        else
        {
            StopGame();
        }
    }

    private void IterateRig()
    {
        if(!_isPlaying) return;
        DeactivateRig(ActiveRig);
        _currentRigIndex++;

        if (_currentRigIndex > _Rigs.Length - 1)
        {
            StopGame();
            _LevelManager.IterateLevel();
            return;
        }
        
        LeanTween.delayedCall(_RigIterationDelay, () =>
        {
            ActivateRig(ActiveRig);
            MoveCam();
        });
        
    }

    private void MoveCam()
    {
        LeanTween.move(_Camera, ActiveRig.transform.position + _RigOffset, _CameraMoveDuration).setEase(LeanTweenType.easeInOutSine);
    }

    private void TriggerActiveRig()
    {
        if (!ActiveRig)
        {
            Debug.Log("Finished All Rigs");
        }
        ActiveRig.Trigger();
    }
}
