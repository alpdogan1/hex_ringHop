using UnityEngine;

namespace Cureviz.View.Common.TweenAlphaSetActive
{
    [RequireComponent(typeof(MeshRenderer))]
    public class MeshTweenAlphaSetActiveHandler : TweenAlphaSetActiveHandler
    {
    
        private MeshRenderer _renderer;
    
        public MeshRenderer Renderer
        {
            get
            {
                if (_renderer == null)
                {
                    _renderer = GetComponent<MeshRenderer>();
                }
                return _renderer;
            }
        }
    
        protected override Color CurrentColor
        {
            get
            {
                return Renderer.material.color;
            }
        }

        protected override void DoTween(bool isActive)
        {
            CurrentTweenId = LeanTween.color(gameObject, isActive ? ActiveColor : DisabledColor, Duration).setEase(Easing)
                .setOnComplete(() => { CurrentTweenId = -1; }).uniqueId;
        }
    }
}