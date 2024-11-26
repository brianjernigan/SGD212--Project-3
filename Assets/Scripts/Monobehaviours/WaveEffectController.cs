using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEffectController : MonoBehaviour
{
    private float _waveAmplitude = 20f;
    private float _waveSpeed = 1f;
    
    private bool _isEnabled;
    private bool _isActive;

    private Coroutine _waveCoroutine;
    
    // public void StartWaveEffect()
    // {
    //     if (_isEnabled && !_isActive)
    //     {
    //         _isActive = true;
    //         _waveCoroutine = StartCoroutine(WaveEffectCoroutine());
    //     }
    // }
    
    // public void StopWaveEffect()
    // {
    //     if (_isActive)
    //     {
    //         _isActive = false;
    //         if (_waveCoroutine != null)
    //         {
    //             StopCoroutine(_waveCoroutine);
    //             _waveCoroutine = null;
    //         }
    //
    //         var playerHand = GameManager.Instance.PlayerHand;
    //
    //         // Smoothly reset card positions after stopping the wave
    //         foreach (var card in playerHand.CardsInHand)
    //         {
    //             // ???
    //             if (card?.UI?.transform != null)
    //             {
    //                 StartCoroutine(SmoothResetPosition(card.UI.transform, playerHand.CardsInHand.IndexOf(card)));
    //             }
    //         }
    //     }
    // }
    
    // private IEnumerator WaveEffectCoroutine()
    // {
    //     var time = 0f;
    //
    //     while (_isActive && _isEnabled)
    //     {
    //         time += Time.deltaTime * _waveSpeed;
    //
    //         var playerHand = GameManager.Instance.PlayerHand;
    //         
    //         // Adjust the position of each card in the hand area
    //         for (var i = 0; i < playerHand.CardsInHand.Count; i++)
    //         {
    //             var card = playerHand.CardsInHand[i];
    //             if (card?.UI?.transform is not null)
    //             {
    //                 var originalPosition = CalculateCardPosition(i, playerHand.NumCardsInHand, GameManager.Instance.Hand.transform.position);
    //                 var yOffset = Mathf.Sin(time + i * 0.5f) * _waveAmplitude; // Offset each card slightly for smooth wave
    //                 card.UI.transform.position = originalPosition + new Vector3(0, yOffset, 0);
    //             }
    //         }
    //
    //         yield return null;
    //     }
    // }
    
    // private IEnumerator SmoothResetPosition(Transform cardTransform, int cardIndex)
    // {
    //     var playerHand = GameManager.Instance.PlayerHand;
    //     var targetPosition = CalculateCardPosition(cardIndex, playerHand.NumCardsInHand, GameManager.Instance.Hand.transform.position);
    //     var startPosition = cardTransform.position;
    //
    //     var duration = 0.5f; // Time to reset smoothly
    //     var elapsedTime = 0f;
    //
    //     while (elapsedTime < duration)
    //     {
    //         elapsedTime += Time.deltaTime;
    //         var t = elapsedTime / duration;
    //         t = t * t * (3f - 2f * t); // Smoothstep interpolation
    //
    //         cardTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
    //         yield return null;
    //     }
    //
    //     cardTransform.position = targetPosition;
    // }
}
