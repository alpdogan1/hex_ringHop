﻿using System;
using System.Linq;
using Cureviz.View.Common.TweenAlphaSetActive;
using ElephantSDK;
using GameAnalyticsSDK;
using Hex.Modules;
using Sirenix.OdinInspector;
using TMPro;
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

    [Title("UI Management")]
    [SerializeField, Required] private CanvasGroupTweenAlphaSetActiveHandler _UiPanel;
    [SerializeField, Required] private CanvasGroupTweenAlphaSetActiveHandler _WinPanel;
    [SerializeField, Required] private CanvasGroupTweenAlphaSetActiveHandler _LosePanel;
    [SerializeField, Required] private GameObject _Logo;
    [SerializeField, Required] private TextMeshProUGUI _RigLeftText;
    [SerializeField, Required] private TextMeshProUGUI _LevelText;
    [SerializeField, Required] private CanvasGroupTweenAlphaSetActiveHandler _LevelTextPanel;
    [SerializeField, Required] private float _LevelTextDuration = 1.5f;

    private HopRig ActiveRig
    {
        get
        {
            if (_currentRigIndex > _Rigs.Length - 1 || _currentRigIndex < 0) return null;
            return _Rigs[_currentRigIndex];
        }
    }

    private static bool IsDown => Input.GetMouseButtonDown(0);

    private void Awake()
    {
        GameAnalytics.Initialize();
    }

    private void Update()
    {
        if (_isPlaying && IsDown)
            TriggerActiveRig();
    }

    [Button]
    public void StartGame()
    {
        _WinPanel.SetIsActive(false);
        _LosePanel.SetIsActive(false);
        _Logo.SetActive(false);
        _LevelManager.LoadCurrentLevelScene(() =>
        {
            // _Rigs = FindObjectsOfType<HopRig>();
            var sortedRigs = FindObjectsOfType<HopRig>().ToList();
            sortedRigs.Sort((rig1, rig2)=> rig1.transform.position.x - rig2.transform.position.x < 0 ? -1 : 1);
            _Rigs = sortedRigs.ToArray();
            
            DeactivateRig(ActiveRig);
            
            _currentRigIndex = 0;
            UpdateRigLeftText();

            ActivateRig(ActiveRig);
        
            _isPlaying = true;
            MoveCam();
        
            _UiPanel.SetIsActive(false);
            _LevelText.text = $"LVL{_LevelManager.CurrentLevelIndex + 1}";
            _LevelTextPanel.SetIsActive(true);
            LeanTween.delayedCall(gameObject, _LevelTextDuration, () => _LevelTextPanel.SetIsActive(false));
        });

        Elephant.LevelStarted(_LevelManager.CurrentLevelIndex);
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, _LevelManager.CurrentLevelIndex.ToString());
    }


    public void StopGame(bool isSuccess)
    {
        if(!_isPlaying) return;
        _LosePanel.SetIsActive(!isSuccess);
        _WinPanel.SetIsActive(isSuccess);

        DeactivateRig(ActiveRig);
        _currentRigIndex = 0;
        // MoveCam();
        _isPlaying = false;
        _Logo.SetActive(true);
        _UiPanel.SetIsActive(true);

        if (isSuccess)
        {
            Elephant.LevelCompleted(_LevelManager.CurrentLevelIndex);
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, _LevelManager.CurrentLevelIndex.ToString());
        }
        else
        {
            
            Elephant.LevelFailed(_LevelManager.CurrentLevelIndex);
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, _LevelManager.CurrentLevelIndex.ToString());
        }
        
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
            StopGame(false);
        }
    }

    private void IterateRig()
    {
        if(!_isPlaying) return;
        DeactivateRig(ActiveRig);
        _currentRigIndex++;
        UpdateRigLeftText();

        if (_currentRigIndex > _Rigs.Length - 1)
        {
            StopGame(true);
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

    private void UpdateRigLeftText()
    {
        if (!_isPlaying) _RigLeftText.text = "";
        _RigLeftText.text = (_Rigs.Length - _currentRigIndex).ToString();
    }
}
