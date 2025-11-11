using UnityEngine;
using TMPro;

public class FollowCanvas : MonoBehaviour
{
    
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    
    public float textAppearDistance = 6f;
    public float textDisappearDistance = 2f;
    public float textAppearSpeed = 0.1f;
    
    private CanvasGroup canvasGroup;

    void Start()
    {

        
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Update()
    {
        transform.LookAt(transform.position + playerCamera.transform.rotation * Vector3.forward);
          
        if (Vector3.Distance(player.position, this.transform.position) < textDisappearDistance)
            HideText();
        else if (Vector3.Distance(player.position, this.transform.position) < textAppearDistance)
            ShowText();
        else 
            HideText();
        
    }

    private void ShowText()
    {
        var col = textMeshPro.color;
            
        col.a = Mathf.Lerp(col.a, 1f, textAppearSpeed);
        textMeshPro.color = col;
    }
        
    private void HideText()
    {
        var col = textMeshPro.color;
            
        col.a = Mathf.Lerp(col.a, 0f, textAppearSpeed);
        textMeshPro.color = col;
    }
    
}