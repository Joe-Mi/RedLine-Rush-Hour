using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradeCaruosel : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [Header("UpgradeMenu_Handler")]
    [SerializeField] UpgradeMenu_Handler UpgradeMenuHandler;

    [Header("Hierarchy Setup")]
    [SerializeField] private RectTransform contentParent;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    [Header("Wheel Dimensions")]
    [Tooltip("The radius of the wheel. Higher numbers push images further away from the center.")]
    [SerializeField] private float wheelRadius = 300f;
    [Tooltip("The angle step between items in degrees. 30-40 works well for a slight arc.")]
    [SerializeField] private float itemSpacingAngle = 35f;

    [Header("Visual Effects")]
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float minScale = 0.8f;
    [Tooltip("The color tint given to unselected background items.")]
    [SerializeField] private Color dimmedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private float transitionSpeed = 10f;

    private RectTransform[] items;
    private Image[] itemImages;
    private int _TargetIndex = 0;
    public int TargetIndex
    {
        get => _TargetIndex;
        set
        {
            // 1. Only run the code if the value actually changed
            if (_TargetIndex == value) return;

            _TargetIndex = value;
            Debug.Log($"test ActiveIndex: {_TargetIndex}");

            // 2. Call your separate script function automatically
            // (Assuming you have a reference to the script component)
            if (UpgradeMenuHandler != null)
            {
                UpgradeMenuHandler.SetActive(_TargetIndex);
            }
        }
    }

    private float currentInterpolatedIndex = 0f;
    private bool isDragging = false;

    void Start()
    {
        int childCount = contentParent.childCount;
        items = new RectTransform[childCount];
        itemImages = new Image[childCount];

        for (int i = 0; i < childCount; i++)
        {
            items[i] = contentParent.GetChild(i) as RectTransform;
            itemImages[i] = items[i].GetComponent<Image>();
        }

        if (nextButton != null) nextButton.onClick.AddListener(NextItem);
        if (prevButton != null) prevButton.onClick.AddListener(PrevItem);

        UpdateLayout();
    }

    void Update()
    {
        if (!isDragging)
        {
            // Smoothly slide toward the selected menu index
            currentInterpolatedIndex = Mathf.Lerp(currentInterpolatedIndex, TargetIndex, Time.deltaTime * transitionSpeed);
            UpdateLayout();
        }
    }

    private void UpdateLayout()
    {
        if (items == null || items.Length == 0) return;

        for (int i = 0; i < items.Length; i++)
        {
            // Calculate where this item sits relative to the current virtual center index
            float relativeIndex = i - currentInterpolatedIndex;

            // Convert to radians. Adding 90 degrees (Mathf.PI / 2) centers the active item at the top/front.
            float angleRad = (relativeIndex * itemSpacingAngle * Mathf.Deg2Rad) + (Mathf.PI / 2f);

            // 1. Plot coordinates flatly on a 2D wheel layout
            float posX = Mathf.Cos(angleRad) * wheelRadius;
            float posY = (Mathf.Sin(angleRad) * wheelRadius) - wheelRadius; // Subtract radius to drop the wheel's center downwards

            items[i].anchoredPosition = new Vector2(posX, posY);

            // 2. Proximity calculation (1.0 = exactly centered, approaching 0.0 = far edges)
            float proximityToCenter = Mathf.Cos((relativeIndex * itemSpacingAngle) * Mathf.Deg2Rad);
            proximityToCenter = Mathf.Clamp01(proximityToCenter);

            // 3. Apply Scaling
            float targetScale = Mathf.Lerp(minScale, maxScale, proximityToCenter);
            items[i].localScale = new Vector3(targetScale, targetScale, 1f);

            // 4. Smoothly Darken Unselected Elements
            //if (itemImages[i] != null)
            //{
            //    itemImages[i].color = Color.Lerp(dimmedColor, Color.white, proximityToCenter);
            //}
        }

    }

    public void NextItem()
    {
        if (TargetIndex < items.Length - 1)
        {
            TargetIndex++;
        }
        else {
            TargetIndex = 0;
        }
        
    }

    public void PrevItem()
    {
        if (TargetIndex > 0)
        {
            TargetIndex--;
        }
        else
        {
            TargetIndex = items.Length - 1;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        // Map pixel dragging to carousel index movement
        float indexDelta = (eventData.delta.x / Screen.width) * (wheelRadius / itemSpacingAngle);
        currentInterpolatedIndex -= indexDelta;
        currentInterpolatedIndex = Mathf.Clamp(currentInterpolatedIndex, 0f, items.Length - 1);
        UpdateLayout();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        TargetIndex = Mathf.RoundToInt(currentInterpolatedIndex);
    }
}


