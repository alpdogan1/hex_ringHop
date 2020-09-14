using UnityEngine;

namespace Cureviz.View.Common.TweenAlphaSetActive
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteTweenAlphaSetActiveHandler : TweenAlphaSetActiveHandler
    {
        private SpriteRenderer _renderer;

        public SpriteRenderer Renderer
        {
            get
            {
                if (_renderer == null)
                {
                    _renderer = GetComponent<SpriteRenderer>();
                }
                return _renderer;
            }
        }


        protected override Color CurrentColor
        {
            get { return Renderer.color; }
        }


        protected override void DoTween(bool isActive)
        {
        
            CurrentTweenId = LeanTween.color(gameObject, isActive ? ActiveColor : DisabledColor, Duration).setEase(Easing)
                .setOnComplete(() => { CurrentTweenId = -1; }).uniqueId;
        }
    }
}