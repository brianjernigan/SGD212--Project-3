using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace HunterScripts
{
    public class HunterCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Card Display Settings")]
        [SerializeField] private MeshRenderer cardMeshRenderer;
        [SerializeField] private TMP_Text cardRankText;
        [SerializeField] private TMP_Text cardCostText;

        private HunterCardData cardData;
        public HunterCardData CardData => cardData;

        private Vector3 originalLocalPosition;
        private Quaternion originalLocalRotation;
        private Transform originalParent;
        private Camera mainCamera;

        private Animator animator;
        private bool isSelected = false;
        public bool IsSelected => isSelected;

        [Header("Hover Animation Settings")]
        [SerializeField] private Vector3 hoverOffset = new Vector3(0, 0.2f, 0); // Move amount when hovered
        [SerializeField] private float hoverScale = 1.1f; // Scale factor when hovered
        [SerializeField] private float animationSpeed = 0.2f; // Speed of hover animation

        [Header("Bobbing Animation Settings")]
        [SerializeField] private float bobbingAmplitude = 0.1f; // Height of the bobbing
        [SerializeField] private float bobbingFrequency = 1f; // Speed of the bobbing

        [Header("Selection Rotation Settings")]
        [SerializeField] private Vector3 selectedRotation = new Vector3(0, 0, 0); // Rotation when selected
        [SerializeField] private Vector3 deselectedRotation = new Vector3(0, 0, 0); // Rotation when not selected
        [SerializeField] private float rotationSpeed = 0.3f; // Speed of rotation animation
        [SerializeField] private Vector3 selectedOffset = new Vector3(0, 0.5f, 0); // Offset when selected


        private bool isHovered = false;
        private Coroutine hoverCoroutine;
        private Coroutine rotationCoroutine;
        private float bobbingTimer = 0f;

        private void Start()
        {
            Debug.Log("HunterCardUI Start() called on " + gameObject.name);

            // Since there's no Canvas, we don't need to find it
            originalParent = transform.parent;
            originalLocalPosition = transform.localPosition;
            originalLocalRotation = transform.localRotation;
            animator = GetComponent<Animator>();

            if (animator == null)
            {
                Debug.LogWarning("HunterCardUI: No Animator component found on " + gameObject.name);
            }

            // Get the main camera
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("HunterCardUI: Main Camera not found. Ensure your main camera is tagged as 'MainCamera'.");
            }

            // Start the bobbing animation
            StartCoroutine(BobbingAnimation());
        }

        public void InitializeCard(HunterCardData data)
        {
            Debug.Log("HunterCardUI: InitializeCard called on " + gameObject.name);

            if (data == null)
            {
                Debug.LogError("HunterCardUI: InitializeCard - cardData is null.");
                return;
            }

            cardData = data;
            if (cardMeshRenderer != null)
            {
                cardMeshRenderer.material = data.cardMaterial;
                Debug.Log("HunterCardUI: Card material set.");
            }
            else
            {
                Debug.LogError("HunterCardUI: cardMeshRenderer is not assigned.");
            }

            if (cardRankText != null)
            {
                cardRankText.text = data.cardRank.ToString();
                Debug.Log("HunterCardUI: Card rank set to " + data.cardRank);
            }
            else
            {
                Debug.LogError("HunterCardUI: cardRankText is not assigned.");
            }

            if (cardCostText != null)
            {
                cardCostText.text = data.cardCost.ToString();
                Debug.Log("HunterCardUI: Card cost set to " + data.cardCost);
            }
            else
            {
                Debug.LogError("HunterCardUI: cardCostText is not assigned.");
            }

            if (animator != null)
            {
                animator.SetTrigger("DrawCard");
                Debug.Log("HunterCardUI: Triggered DrawCard animation.");
            }
        }

        // Hover Enter
        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("HunterCardUI: OnPointerEnter called on " + gameObject.name);

            if (!isHovered)
            {
                isHovered = true;
                if (hoverCoroutine != null)
                {
                    StopCoroutine(hoverCoroutine);
                }
                hoverCoroutine = StartCoroutine(AnimateHover(originalLocalPosition + hoverOffset, Vector3.one * hoverScale));
            }
        }

        // Hover Exit
        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("HunterCardUI: OnPointerExit called on " + gameObject.name);

            if (isHovered)
            {
                isHovered = false;
                if (hoverCoroutine != null)
                {
                    StopCoroutine(hoverCoroutine);
                }
                hoverCoroutine = StartCoroutine(AnimateHover(originalLocalPosition, Vector3.one));
            }
        }

        // Coroutine to animate the position and scale for hover effects
        private IEnumerator AnimateHover(Vector3 targetPosition, Vector3 targetScale)
        {
            Debug.Log("HunterCardUI: AnimateHover started. Target Position: " + targetPosition + ", Target Scale: " + targetScale);
            Vector3 startPosition = transform.localPosition;
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < animationSpeed)
            {
                transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / animationSpeed);
                transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / animationSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = targetPosition;
            transform.localScale = targetScale;
            Debug.Log("HunterCardUI: AnimateHover completed. Position: " + targetPosition + ", Scale: " + targetScale);
        }

        // Coroutine for bobbing animation
        private IEnumerator BobbingAnimation()
        {
            Debug.Log("HunterCardUI: BobbingAnimation started on " + gameObject.name);

            while (true)
            {
                float bobbingOffset = Mathf.Sin(bobbingTimer * bobbingFrequency) * bobbingAmplitude;
                bobbingTimer += Time.deltaTime;

                if (!isHovered)
                {
                    transform.localPosition = originalLocalPosition + new Vector3(0, bobbingOffset, 0);
                }
                else
                {
                    transform.localPosition = originalLocalPosition + hoverOffset + new Vector3(0, bobbingOffset * 0.5f, 0);
                }

                yield return null;
            }
        }

        // Begin Drag
        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("HunterCardUI: OnBeginDrag called on " + gameObject.name);

            originalLocalPosition = transform.localPosition;
            originalLocalRotation = transform.localRotation;
            transform.SetParent(originalParent, true); // Keep parent as original
            animator?.SetTrigger("StartDrag");
            Debug.Log("HunterCardUI: Triggered StartDrag animation.");

            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
                Debug.Log("HunterCardUI: Stopped existing rotation coroutine.");
            }
        }

        // Dragging
        public void OnDrag(PointerEventData eventData)
        {
            // Debug.Log("HunterCardUI: OnDrag called on " + gameObject.name);

            if (mainCamera == null)
            {
                Debug.LogError("HunterCardUI: Main Camera not assigned.");
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(eventData.position);
            Plane plane = new Plane(Vector3.up, originalParent.position); // Assuming the cards are on the XZ plane
            float distance;

            if (plane.Raycast(ray, out distance))
            {
                Vector3 point = ray.GetPoint(distance);
                transform.position = point;
            }
            else
            {
                Debug.LogWarning("HunterCardUI: Raycast did not hit the plane.");
            }
        }

        // End Drag
        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("HunterCardUI: OnEndDrag called on " + gameObject.name);

            if (mainCamera == null)
            {
                Debug.LogError("HunterCardUI: Main Camera not assigned.");
                ReturnToHand();
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(eventData.position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("HunterCardUI: Raycast hit " + hit.collider.gameObject.name);
                if (hit.collider.CompareTag("ActionZone"))
                {
                    Debug.Log("HunterCardUI: Hit ActionZone. Playing card.");
                    PlayCard();
                    return;
                }
            }
            else
            {
                Debug.Log("HunterCardUI: Raycast did not hit any collider.");
            }

            Debug.Log("HunterCardUI: Returning card to hand.");
            ReturnToHand();
        }

        // Click to Select/Deselect
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Debug.Log("HunterCardUI: LMB clicked on card: " + gameObject.name);
                ToggleSelection();
                Debug.Log("HunterCardUI: ToggleSelection called on " + gameObject.name);
            }
            else
            {
                Debug.Log("HunterCardUI: Non-LMB click detected on card: " + gameObject.name);
            }
        }

       public void ToggleSelection()
        {
            isSelected = !isSelected;

            // Stop any existing coroutine for positioning and rotation
            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
            }

            if (isSelected)
            {
                // Move the card up with the selected rotation
                StartCoroutine(MoveCardToPosition(originalLocalPosition + selectedOffset, Quaternion.Euler(selectedRotation)));

                // Deselect other cards in hand
                HunterHandManager.Instance.DeselectOtherCards(this);
            }
            else
            {
                // If deselected, return to the original position and rotation
                StartCoroutine(ReturnToOriginalPosition());
            }
        }


        private IEnumerator MoveCardToPosition(Vector3 targetPosition, Quaternion targetRotation)
        {
            Vector3 startPosition = transform.localPosition;
            Quaternion startRotation = transform.localRotation;
            float elapsed = 0f;

            while (elapsed < animationSpeed)
            {
                transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsed / animationSpeed);
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / animationSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = targetPosition;
            transform.localRotation = targetRotation;
        }


        private IEnumerator RotateCard(Vector3 targetEulerAngles)
        {
            Debug.Log("HunterCardUI: RotateCard coroutine started. Target Euler Angles: " + targetEulerAngles);
            Quaternion targetRotationQuat = Quaternion.Euler(targetEulerAngles);
            Quaternion startRotation = transform.localRotation;
            float elapsed = 0f;

            while (elapsed < rotationSpeed)
            {
                transform.localRotation = Quaternion.Slerp(startRotation, targetRotationQuat, elapsed / rotationSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localRotation = targetRotationQuat;
            Debug.Log("HunterCardUI: RotateCard coroutine completed. Current Rotation: " + transform.localRotation.eulerAngles);
        }

        private void PlayCard()
        {
            Debug.Log("HunterCardUI: PlayCard called on " + gameObject.name);

            HunterHandManager.Instance?.PlayCard(this);
            HunterCardEffectManager.Instance?.PlayPlayEffect(transform.position);

            if (animator != null)
            {
                animator.SetTrigger("PlayCard");
                Debug.Log("HunterCardUI: Triggered PlayCard animation.");
            }

            Destroy(gameObject, 0.5f);
            Debug.Log("HunterCardUI: Card will be destroyed in 0.5 seconds.");
        }

        private void ReturnToHand()
        {
            Debug.Log("HunterCardUI: ReturnToHand called on " + gameObject.name);

            if (animator != null)
            {
                animator.SetTrigger("ReturnToHand");
                Debug.Log("HunterCardUI: Triggered ReturnToHand animation.");
            }

            transform.SetParent(originalParent, true);
            StopAllCoroutines();
            Debug.Log("HunterCardUI: Stopped all coroutines.");

            StartCoroutine(AnimateHover(originalLocalPosition, Vector3.one));
            StartCoroutine(RotateCard(deselectedRotation));
        }

        // Optional: Add OnMouseDown for additional testing
        private void OnMouseDown()
        {
            Debug.Log("HunterCardUI: OnMouseDown detected on " + gameObject.name);
            ToggleSelection();
        }

        private IEnumerator ReturnToOriginalPosition()
        {
            yield return MoveCardToPosition(originalLocalPosition, originalLocalRotation); // Reuse the updated coroutine for consistency
        }



    }
}
