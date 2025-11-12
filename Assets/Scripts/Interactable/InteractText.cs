using System;
using TMPro;
using UnityEngine;

namespace Interactable
{
    public class InteractText: MonoBehaviour
    {
        private Transform _player;
        private Camera _camera;
        public TextMeshProUGUI interactText;
        public float textAppearDistance = 6f;
        public float textAppearSpeed = 0.1f;


        private void Start()
        {
            _player = GameManager.GetPlayer();
            _camera = Camera.main;
            interactText = Extensions.GetChildRecursive("InteractText", transform).GetComponent<TextMeshProUGUI>();
            var col = interactText.color;
            col.a = 0f;
            interactText.color = col;
        }
        
        private void Update()
        {
            transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward);
            
            if (Vector3.Distance(_player.position, this.transform.position) < textAppearDistance)
                ShowText();
            else 
                HideText();
        }
        
        private void ShowText()
        {
            var col = interactText.color;
            
            col.a = Mathf.Lerp(col.a, 1f, textAppearSpeed);
            interactText.color = col;
        }
        
        private void HideText()
        {
            var col = interactText.color;
            
            col.a = Mathf.Lerp(col.a, 0f, textAppearSpeed);
            interactText.color = col;
        }
    }
}