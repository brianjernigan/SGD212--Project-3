using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace HunterScripts
{
    public class HunterCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private MeshRenderer cardMeshRenderer;
        [SerializeField] private TMP_Text cardRankText;
        [SerializeField] private TMP_Text cardCostText;

        private HunterCardData cardData;
        public HunterCardData CardData => cardData;

        private Vector3 originalPosition;
        private Transform originalParent;
        private Canvas canvas;

        private Animator animator;
        private bool isSelected = false;
        public bool IsSelected => isSelected;

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
            originalParent = transform.parent;

            animator = GetComponent<Animator>();
        }

        public void InitializeCard(HunterCardData data)
        {
            if (data == null)
            {
                Debug.LogError("InitializeCard: cardData is null.");
                return;
            }

            cardData = data;

            if (cardMeshRenderer != null)
            {
                cardMeshRenderer.material = data.cardMaterial;
            }
            else
            {
                Debug.LogError("InitializeCard: cardMeshRenderer is not assigned.");
            }

            if (cardRankText != null)
            {
                cardRankText.text = data.cardRank.ToString();
            }
            else
            {
                Debug.LogError("InitializeCard: cardRankText is not assigned.");
            }

            if (cardCostText != null)
            {
                cardCostText.text = data.cardCost.ToString();
            }
            else
            {
                Debug.LogError("InitializeCard: cardCostText is not assigned.");
            }

            if (animator != null)
            {
                animator.SetTrigger("DrawCard");
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalPosition = transform.position;
            transform.SetParent(canvas.transform, true);
            animator?.SetTrigger("StartDrag");
        }

        public void OnDrag(PointerEventData eventData)
        {
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                transform.position = hit.point;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("ActionZone"))
                {
                    PlayCard();
                    return;
                }
            }

            ReturnToHand();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ToggleSelection();
        }

        private void ToggleSelection()
        {
            isSelected = !isSelected;
            if (animator != null)
            {
                animator.SetBool("IsSelected", isSelected);
            }
        }

        private void PlayCard()
        {
            HunterHandManager.Instance?.PlayCard(this);
            HunterCardEffectManager.Instance?.PlayPlayEffect(transform.position);
            animator?.SetTrigger("PlayCard");
            Destroy(gameObject);
        }

        private void ReturnToHand()
        {
            if (animator != null)
            {
                animator.SetTrigger("ReturnToHand");
            }
            transform.position = originalPosition;
            transform.SetParent(originalParent, true);
        }

        public IEnumerator AnimateMove(Vector3 startPos, Vector3 endPos, float duration, System.Action onComplete = null)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = endPos;

            onComplete?.Invoke();
        }
    }
}
