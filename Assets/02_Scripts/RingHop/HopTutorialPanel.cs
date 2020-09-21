using System;
using System.Collections.Generic;
using System.Linq;
using Cureviz.View.Common.TweenAlphaSetActive;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RingHop
{
    public class HopTutorialPanel: MonoBehaviour
    {
        [SerializeField, Required] private int _Phase;
        [SerializeField, Required] private CanvasGroupTweenAlphaSetActiveHandler _Panel;
        
        private static HopTutorialPanel _currentActivePanel;
        private static readonly List<HopTutorialPanel> Panels = new List<HopTutorialPanel>();

        private void Awake()
        {
            Panels.Add(this);
        }

        public static void SetPhaseVisible(int phase)
        {
            if (_currentActivePanel && phase != _currentActivePanel._Phase)
            {
                _currentActivePanel._Panel.SetIsActive(false);
            }

            var newPanel = Panels.FirstOrDefault(panel => panel._Phase == phase);
            newPanel?._Panel.SetIsActive(true);
            _currentActivePanel = newPanel;
        }

        public static void SetAllInvisible()
        {
            _currentActivePanel?._Panel.SetIsActive(false);
        }
    }
}