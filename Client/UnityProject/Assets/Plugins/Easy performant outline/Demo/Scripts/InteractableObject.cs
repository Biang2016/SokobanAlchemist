using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#pragma warning disable CS0649

#if EPO_DOTWEEN
using DG.Tweening;
#endif

namespace EPOOutline.Demo
{
    public class InteractableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField]
        private AudioClip interactionSound;

        [SerializeField]
        private bool affectOutlinable = true;

        private Outlinable outlinable;

        private void Start()
        {
            if (!affectOutlinable)
                return;

            outlinable = GetComponent<Outlinable>();
#if !EPO_DOTWEEN
            outlinable.enabled = false;
#else
            outlinable.FrontParameters.DOColor(new Color(0, 0, 1, 0), 0.0f);
            outlinable.FrontParameters.DODilateShift(1.0f, 0.0f);
            outlinable.FrontParameters.DOBlurShift(0.0f, 0.0f);
#endif
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!affectOutlinable)
                return;

#if !EPO_DOTWEEN
            outlinable.enabled = true;
#else
            outlinable.FrontParameters.DOKill(true);
            outlinable.FrontParameters.DOColor(new Color(0, 1, 0, 1), 0.5f);
            outlinable.FrontParameters.DOBlurShift(1.0f, 0.5f).SetDelay(0.5f);
            outlinable.FrontParameters.DODilateShift(0.0f, 0.5f).SetDelay(0.5f);
            outlinable.FrontParameters.DOColor(new Color(1, 1, 0, 1), 0.5f).SetDelay(1.0f);
#endif
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!affectOutlinable)
                return;

#if !EPO_DOTWEEN
            outlinable.enabled = false;
#else
            outlinable.FrontParameters.DOKill(true);
            outlinable.FrontParameters.DOBlurShift(0.0f, 0.5f);
            outlinable.FrontParameters.DODilateShift(1.0f, 0.5f);
            outlinable.FrontParameters.DOColor(new Color(0, 0, 1, 0), 0.5f).SetDelay(0.5f);
#endif
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            AudioSource.PlayClipAtPoint(interactionSound, transform.position, 1.0f);
        }
    }
}