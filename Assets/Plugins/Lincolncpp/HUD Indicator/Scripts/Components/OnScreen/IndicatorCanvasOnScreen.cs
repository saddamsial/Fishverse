using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace LincolnCpp.HUDIndicator
{
    public class IndicatorCanvasOnScreen : IndicatorCanvas
    {

        private IndicatorOnScreen indicatorOnScreen;

        // Icon variables
        private RawImage rawImage;
        private RectTransform rectTransform;
        private IndicatorIconStyle style;


        public override void Create(Indicator indicator, IndicatorRenderer renderer)
        {
            base.Create(indicator, renderer);

            indicatorOnScreen = indicator as IndicatorOnScreen;

            // Get indicator style
            style = indicatorOnScreen.style;

            // Create game object
            gameObject = new GameObject($"IndicatorOnScreen:{indicator.gameObject.name}");
            gameObject.transform.SetParent(renderer.transform);

            // Setup rect transform
            rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Create icon image
            rawImage = gameObject.AddComponent<RawImage>();

            destanceGameObject = new GameObject($"IndicatorOffScreen:Text:{indicator.gameObject.name}");
            destanceGameObject.transform.SetParent(gameObject.transform);
            distanceText = destanceGameObject.AddComponent<TMPro.TextMeshProUGUI>();
            distanceText.fontSize = 17;
            distanceText.alignment = TMPro.TextAlignmentOptions.BottomGeoAligned;
            distanceText.rectTransform.SetHeight(4);

            // Update icon style
            UpdateStyle();
        }

        public override void LateUpdate()
        {
            if (!active) return;

            if (IsVisible())
            {
                UpdateStyle();
                UpdatePosition();
                UpdateDistance();
            }
            else
            {
                if (gameObject.activeSelf)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void UpdateStyle()
        {
            rectTransform.sizeDelta = new Vector2(style.width, style.height);
            rawImage.texture = style.texture;
            rawImage.color = style.color;
        }

        private void UpdatePosition()
        {
            Rect rendererRect = renderer.GetRect();
            Vector3 pos = renderer.GetRectTransform().InverseTransformPoint(renderer.camera.WorldToScreenPoint(indicator.gameObject.transform.position + indicatorOnScreen.offset));

            rendererRect.x += style.width / 2f;
            rendererRect.y += style.height / 2f;
            rendererRect.width -= style.width;
            rendererRect.height -= style.height;

            // On-screen (Show)
            if (pos.z >= 0 && pos.x >= rendererRect.x && pos.x <= rendererRect.x + rendererRect.width && pos.y >= rendererRect.y && pos.y <= rendererRect.y + rendererRect.height)
            {
                gameObject.SetActive(true);

                rectTransform.position = renderer.GetRectTransform().TransformPoint(new Vector3(pos.x, pos.y, 0));
            }
            // Off-screen (Hide)
            else
            {
                gameObject.SetActive(false);
            }

        }
    }
}
