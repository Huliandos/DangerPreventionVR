using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    [Serializable]
    public struct InjurySlideStruct
    {
        public Injury injury;
        public Sprite slideTexture;

        public InjurySlideStruct(Injury injury, Sprite slideTexture)
        {
            this.injury = injury;
            this.slideTexture = slideTexture;
        }
    }
    
    public class WhiteboardController : MonoBehaviour
    {
        [SerializeField] 
        private Image imageObject;
        
        [SerializeField] 
        private List<InjurySlideStruct> clipboards;

        private void Awake()
        {
            if (imageObject == null)
                imageObject = GetComponentInChildren<Image>();
        }

        public void ShowSlide(Injury causedInjury)
        {
            List<Sprite> showableSlides = clipboards
                .FindAll(entry => entry.injury == causedInjury)
                .Select(entry => entry.slideTexture)
                .ToList();
            
            Sprite firstTextureSlide = showableSlides.FirstOrDefault();
            if (firstTextureSlide == null) return;
            if (imageObject == null) imageObject = GetComponentInChildren<Image>();

            imageObject.sprite = firstTextureSlide;
        }
    }
}