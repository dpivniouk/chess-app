using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapCamera : MonoBehaviour
{
    public Camera minimapCamera;
    public GameObject minimapTargetImage;

    void Start()
    {
        CameraFit();
        SetMinimapTexture();
    }

    void CameraFit()
    {
        float viewportX = 0f;
        float viewportY = 0f;

        minimapCamera.orthographicSize = 8f;
        minimapCamera.aspect = 1f;

        minimapCamera.transform.localPosition = new Vector3(viewportX, viewportY,-10f);
    }

    void SetMinimapTexture()
    {
        int minimapImageWidth;
        int minimapImageHeight;
        float targetAspect = 576f / 480f;

        if (minimapCamera.aspect >= targetAspect)
        {
            minimapImageWidth = 576;
            minimapImageHeight = (int)(576 / minimapCamera.aspect);
        }
        else
        {
            minimapImageHeight = 480;
            minimapImageWidth = (int)(480 * minimapCamera.aspect);
        }

        RenderTexture minimapImage = new RenderTexture(minimapImageWidth, minimapImageHeight, 0);
        minimapImage.Create();

        minimapCamera.targetTexture = minimapImage;

        minimapTargetImage.GetComponent<RawImage>().texture = minimapImage;
        minimapTargetImage.GetComponent<RawImage>().SetNativeSize();

        minimapTargetImage.GetComponent<BoxCollider2D>().size = new Vector2(minimapImageWidth, minimapImageHeight);
    }

}
