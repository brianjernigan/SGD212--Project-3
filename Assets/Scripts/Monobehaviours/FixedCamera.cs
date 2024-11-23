using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectRatio : MonoBehaviour
{
    private const float TargetAspect = 16f / 9f;

    private void Start()
    {
        SetAspectRatio();
    }

    private void SetAspectRatio()
    {
        var windowAspect = (float)Screen.width / Screen.height;
        
        var scaleHeight = windowAspect / TargetAspect;

        var cameraComponent = GetComponent<Camera>();

        if (scaleHeight < 1.0f)
        {
            var rect = cameraComponent.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            cameraComponent.rect = rect;
        }
        else
        {
            var scaleWidth = 1.0f / scaleHeight;

            var rect = cameraComponent.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            cameraComponent.rect = rect;
        }
    }
}

