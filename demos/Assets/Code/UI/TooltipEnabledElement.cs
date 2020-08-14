using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections;

namespace Goldenwere.Unity.UI
{
    /// <summary>
    /// Describes how the tooltip should be anchored in terms of attachment
    /// </summary>
    public enum AnchorMode
    {
        /// <summary>
        /// Tooltip follows cursor
        /// </summary>
        AttachedToCursor,

        /// <summary>
        /// Tooltip stays fixed and positions based on element
        /// </summary>
        AttachedToElement
    }

    /// <summary>
    /// Defines how the tooltip should be anchored in terms of positioning (first half describes vertical positioning, second half describes horizontal positioning)
    /// </summary>
    public enum AnchorPosition
    {
        TopLeft,
        TopMiddle,
        TopRight,
        CenterLeft,
        CenterMiddle,
        CenterRight,
        BottomLeft,
        BottomMiddle,
        BottomRight
    }

    /// <summary>
    /// Defines where an arrow goes when the AnchorPosition is CenterMiddle
    /// </summary>
    public enum MiddlePosition
    {
        Top,
        Bottom
    }

    /// <summary>
    /// Defines how the tooltip should be transitioned
    /// </summary>
    public enum TransitionMode
    {
        None,
        Fade
    }

    /// <summary>
    /// Adds a tooltip to a UI element
    /// </summary>
    public class TooltipEnabledElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Fields
#pragma warning disable 0649
        [Tooltip         ("Defines how the tooltip is attached")]
        [SerializeField] private AnchorMode     anchorMode;
        [Tooltip         ("The default anchor position. If the tooltip text overflows with this anchor, will change to another one if needed")]
        [SerializeField] private AnchorPosition anchorPosition;
        [Tooltip         ("Sets where the tooltip arrow goes when using the CenterMiddle setting in anchorPosition. Has no effect for other settings or when there is no arrow available")]
        [SerializeField] private MiddlePosition arrowDefaultPositionAtMiddle;
        [Tooltip         ("Needed in order to ensure proper tooltip positioning; can be left unassigned as long as the UI element itself is attached to a canvas")]
        [SerializeField] private Camera         cameraThatRendersCanvas;
        [Tooltip         ("Optional string to provide if cannot attach camera in inspector (e.g. prefabbed UI elements instantiated at runtime)")]
        [SerializeField] private string         cameraThatRendersCanvasName;
        [Tooltip         ("Needed in order to ensure proper tooltip positioning as well as attaching tooltip to canvas")]
        [SerializeField] private Canvas         canvasToBeAttachedTo;
        [Range(00f,10f)] [Tooltip               ("Delay between triggering the tooltip and transitioning it into existence")]
        [SerializeField] private float          tooltipDelay;
        [Range(0.01f,1)] [Tooltip               ("Determines how much the tooltip anchors to the left/right when AnchorPosition is one of the left/right settings (has no effect on Middle settings)")]
        [SerializeField] private float          tooltipHorizontalFactor = 1;
        [Tooltip         ("Padding between the edges of the tooltip and text element, done in traditional CSS order: Top, Right, Bottom, Left")]
        [SerializeField] private Vector4        tooltipPadding;
        [Tooltip         ("Prefab which the topmost gameobject can be resized based on text and contains a text element that can be set\n" +
                          "Note: Make sure that the text element has the horizontal+vertical stretch anchor preset and equivalent padding on all sides," +
                          "as this class depends on the left padding when determining container height + bottom padding\n" +
                          "Make sure that the container uses the center+center anchor preset, as this class needs to use its own anchor method due to depending on cursor position")]
        [SerializeField] private GameObject     tooltipPrefab;
        [Tooltip         ("The text to display in the tooltip")]
        [SerializeField] private string         tooltipText;
        [Tooltip         ("Values used if defining a string that needs formatting. Leave blank if no formatting is done inside tooltipText")]
        [SerializeField] private double[]       tooltipValues;
        [Range(000,100)] [Tooltip               ("How long tooltip transitions last (only used if transitionMode isn't set to None")]
        [SerializeField] private float          transitionDuration;
        [Tooltip         ("The curve for animating transitions when transitioning into existence")]
        [SerializeField] private AnimationCurve transitionCurveIn = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [Tooltip         ("The curve for animating transitions when transitioning out of existence")]
        [SerializeField] private AnimationCurve transitionCurveOut = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [Tooltip         ("How the tooltip is transitioned/animated into/out of existence")]
        [SerializeField] private TransitionMode transitionMode;
#pragma warning restore 0649
        /**************/ private bool           isActive;
        /**************/ private bool           isInitialized;
        /**************/ private TooltipPrefab  tooltipInstance;
        #endregion
        #region Methods
        /// <summary>
        /// Sets up the tooltip at start
        /// </summary>
        private void Start()
        {
            if (!isInitialized)
                Initialize();
            SetText();

            if (anchorMode == AnchorMode.AttachedToElement)
                tooltipInstance.RTransform.anchoredPosition = PositionTooltipToElement();
        }

        /// <summary>
        /// Set position of tooltip at Update
        /// </summary>
        private void Update()
        {
            if (isActive)
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                    SetActive(false);

                else if (anchorMode == AnchorMode.AttachedToCursor && PositionTooltipToCursor(out Vector2 newPos))
                    tooltipInstance.RTransform.anchoredPosition = newPos;
            }
        }

        /// <summary>
        /// Destroys the tooltip when the enabled element itself is destroyed
        /// </summary>
        private void OnDestroy()
        {
            Destroy(tooltipInstance);
        }

        /// <summary>
        /// Initializes the tooltip; this is separate from Start in case SetText is called externally before Start gets a chance to run
        /// </summary>
        private void Initialize()
        {
            if (cameraThatRendersCanvas == null)
                if (cameraThatRendersCanvasName != null && cameraThatRendersCanvasName != "")
                    cameraThatRendersCanvas = GameObject.Find(cameraThatRendersCanvasName).GetComponent<Camera>();
                else
                    cameraThatRendersCanvas = Camera.main;
            if (canvasToBeAttachedTo == null)
                canvasToBeAttachedTo = gameObject.GetComponentInParents<Canvas>();

            if (anchorMode == AnchorMode.AttachedToCursor)
                tooltipInstance = Instantiate(tooltipPrefab, canvasToBeAttachedTo.transform).GetComponent<TooltipPrefab>();
            else
                tooltipInstance = Instantiate(tooltipPrefab, GetComponent<RectTransform>()).GetComponent<TooltipPrefab>();
            isActive = tooltipInstance.gameObject.activeSelf;
            SetActive(false, TransitionMode.None);
            isInitialized = true;

            // Override sizing/anchors in prefab in case they may conflict with tooltipping system
            // Padding is a serialized variable, the anchor for text must be vertical+horizontal stretch for it to work properly
            // Ensure arrow is set to center+center so that positioning is correct
            tooltipInstance.Text.rectTransform.offsetMin = new Vector2(tooltipPadding.w, tooltipPadding.z);
            tooltipInstance.Text.rectTransform.offsetMax = new Vector2(-tooltipPadding.y, -tooltipPadding.x);
            tooltipInstance.Text.rectTransform.anchorMin = new Vector2(0, 0);
            tooltipInstance.Text.rectTransform.anchorMax = new Vector2(1, 1);

            if (tooltipInstance.ArrowEnabled)
            {
                tooltipInstance.Arrow.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                tooltipInstance.Arrow.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            }
        }

        /// <summary>
        /// OnPointerEnter, enable the tooltip
        /// </summary>
        public void OnPointerEnter(PointerEventData data)
        {
            StopAllCoroutines();
            if (tooltipDelay > 0)
                StartCoroutine(DelayOpening());
            else
                SetActive(true);
        }

        /// <summary>
        /// OnPointerExit, disable the tooltip
        /// </summary>
        public void OnPointerExit(PointerEventData data)
        {
            StopAllCoroutines();
            SetActive(false);
        }

        /// <summary>
        /// Set the tooltip's colors with this method
        /// </summary>
        /// <param name="background">Color applied to any non-text graphic</param>
        /// <param name="foreground">Color applied to any text graphic</param>
        public void SetColors(Color background, Color foreground)
        {
            if (!isInitialized)
                Initialize();

            Graphic[] graphics = tooltipInstance.GetComponentsInChildren<Graphic>();
            foreach(Graphic graphic in graphics)
            {
                if (graphic.GetType().Namespace == "TMPro")
                    graphic.color = foreground;
                else
                    graphic.color = background;
            }
        }

        /// <summary>
        /// Updates the tooltip text with a new tooltip and optional new values (required if the tooltip has formated values)
        /// </summary>
        /// <param name="newTooltip">The new tooltip string value</param>
        /// <param name="newValues">New values to display in the tooltip if the string requires formatting</param>
        public void UpdateText(string newTooltip, double[] newValues = null)
        {
            tooltipText = newTooltip;
            tooltipValues = newValues;
            SetText();
        }

        /// <summary>
        /// Positions the tooltip to the element for AnchorMode.AttachedToCursor
        /// </summary>
        /// <returns>The position of the tooltip</returns>
        private bool PositionTooltipToCursor(out Vector2 newPos)
        {
            bool didHit = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasToBeAttachedTo.transform as RectTransform, Mouse.current.position.ReadValue(), cameraThatRendersCanvas, out newPos);

            switch (anchorPosition)
            {
                case AnchorPosition.TopLeft:
                    newPos.x += tooltipInstance.RTransform.sizeDelta.x / 2 * tooltipHorizontalFactor;
                    newPos.y -= tooltipInstance.RTransform.sizeDelta.y / 2;
                    if (tooltipInstance.ArrowEnabled)
                        newPos.y -= tooltipInstance.Arrow.rectTransform.sizeDelta.y;
                    break;
                case AnchorPosition.TopMiddle:
                    newPos.y -= tooltipInstance.RTransform.sizeDelta.y / 2;
                    if (tooltipInstance.ArrowEnabled)
                        newPos.y -= tooltipInstance.Arrow.rectTransform.sizeDelta.y;
                    break;
                case AnchorPosition.TopRight:
                    newPos.x -= tooltipInstance.RTransform.sizeDelta.x / 2 * tooltipHorizontalFactor;
                    newPos.y -= tooltipInstance.RTransform.sizeDelta.y / 2;
                    if (tooltipInstance.ArrowEnabled)
                        newPos.y -= tooltipInstance.Arrow.rectTransform.sizeDelta.y;
                    break;
                case AnchorPosition.CenterLeft:
                    newPos.x += tooltipInstance.RTransform.sizeDelta.x / 2 * tooltipHorizontalFactor;
                    if (tooltipInstance.ArrowEnabled)
                        newPos.x += tooltipInstance.Arrow.rectTransform.sizeDelta.x;
                    break;
                case AnchorPosition.CenterRight:
                    newPos.x -= tooltipInstance.RTransform.sizeDelta.x / 2 * tooltipHorizontalFactor;
                    if (tooltipInstance.ArrowEnabled)
                        newPos.x -= tooltipInstance.Arrow.rectTransform.sizeDelta.x;
                    break;
                case AnchorPosition.BottomLeft:
                    newPos.x += tooltipInstance.RTransform.sizeDelta.x / 2 * tooltipHorizontalFactor;
                    newPos.y += tooltipInstance.RTransform.sizeDelta.y / 2;
                    if (tooltipInstance.ArrowEnabled)
                        newPos.y += tooltipInstance.Arrow.rectTransform.sizeDelta.y;
                    break;
                case AnchorPosition.BottomMiddle:
                    newPos.y += tooltipInstance.RTransform.sizeDelta.y / 2;
                    if (tooltipInstance.ArrowEnabled)
                        newPos.y += tooltipInstance.Arrow.rectTransform.sizeDelta.y;
                    break;
                case AnchorPosition.BottomRight:
                    newPos.x -= tooltipInstance.RTransform.sizeDelta.x / 2 * tooltipHorizontalFactor;
                    newPos.y += tooltipInstance.RTransform.sizeDelta.y / 2;
                    if (tooltipInstance.ArrowEnabled)
                        newPos.y += tooltipInstance.Arrow.rectTransform.sizeDelta.y;
                    break;

                case AnchorPosition.CenterMiddle:
                default:
                    // Do nothing in this case - newPos should already be centered if the notes for tooltipPrefab are followed
                    break;
            }

            #region Position clamp-to-screen
            Rect canvasRect = (canvasToBeAttachedTo.transform as RectTransform).rect;
            if (newPos.x < canvasRect.xMin + tooltipInstance.RTransform.sizeDelta.x / 2)
                newPos.x = canvasRect.xMin + tooltipInstance.RTransform.sizeDelta.x / 2;
            if (newPos.x + tooltipInstance.RTransform.sizeDelta.x / 2 > canvasRect.xMax)
                newPos.x = canvasRect.xMax - tooltipInstance.RTransform.sizeDelta.x / 2;
            if (newPos.y < canvasRect.yMin + tooltipInstance.RTransform.sizeDelta.y / 2)
                newPos.y = canvasRect.yMin + tooltipInstance.RTransform.sizeDelta.y / 2;
            if (newPos.y + tooltipInstance.RTransform.sizeDelta.y / 2 > canvasRect.yMax)
                newPos.y = canvasRect.yMax - tooltipInstance.RTransform.sizeDelta.y / 2;
            #endregion

            if (tooltipInstance.ArrowEnabled)
            {
                switch (anchorPosition)
                {
                    case AnchorPosition.TopLeft:
                    case AnchorPosition.TopMiddle:
                    case AnchorPosition.TopRight:
                        tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(0,
                            (tooltipInstance.RTransform.sizeDelta.y / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.y / 2));
                        tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 180);
                        break;
                    case AnchorPosition.CenterLeft:
                        tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(
                            -((tooltipInstance.RTransform.sizeDelta.x / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.x / 2)), 0);
                        tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, -90);
                        break;
                    case AnchorPosition.CenterMiddle:
                        if (arrowDefaultPositionAtMiddle == MiddlePosition.Top)
                        {
                            tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(0,
                                (tooltipInstance.RTransform.sizeDelta.y / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.y / 2));
                            tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 180);
                        }
                        else
                        {
                            tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(0,
                                -(tooltipInstance.RTransform.sizeDelta.y / 2) - (tooltipInstance.Arrow.rectTransform.sizeDelta.y / 2));
                            tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
                        }
                        break;
                    case AnchorPosition.CenterRight:
                        tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(
                            ((tooltipInstance.RTransform.sizeDelta.x / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.x / 2)), 0);
                        tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                    case AnchorPosition.BottomLeft:
                    case AnchorPosition.BottomMiddle:
                    case AnchorPosition.BottomRight:
                    default:
                        tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(0,
                            -((tooltipInstance.RTransform.sizeDelta.y / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.y / 2)));
                        tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
                        break;
                }
            }

            return didHit;
        }

        /// <summary>
        /// Positions the tooltip to the element for AnchorMode.AttachedToElement
        /// </summary>
        /// <returns>The position of the tooltip</returns>
        private Vector2 PositionTooltipToElement()
        {
            RectTransform thisRect = GetComponent<RectTransform>();
            Vector2 newPos = Vector2.zero;

            switch (anchorPosition)
            {
                case AnchorPosition.TopLeft:
                    newPos.x -= ((tooltipInstance.RTransform.sizeDelta.x / 2) + (thisRect.sizeDelta.x / 2)) * tooltipHorizontalFactor;
                    newPos.y += (tooltipInstance.RTransform.sizeDelta.y / 2) + (thisRect.sizeDelta.y / 2);

                    if (tooltipInstance.ArrowEnabled)
                    {
                        newPos.y += (tooltipInstance.Arrow.rectTransform.sizeDelta.y);
                        tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(0,
                            -((tooltipInstance.RTransform.sizeDelta.y / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.y / 2)));
                        tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                    break;
                case AnchorPosition.TopMiddle:
                    newPos.y += (tooltipInstance.RTransform.sizeDelta.y / 2) + (thisRect.sizeDelta.y / 2);

                    if (tooltipInstance.ArrowEnabled)
                    {
                        newPos.y += (tooltipInstance.Arrow.rectTransform.sizeDelta.y);
                        tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(0,
                            -((tooltipInstance.RTransform.sizeDelta.y / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.y / 2)));
                        tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                    break;
                case AnchorPosition.TopRight:
                    newPos.x += ((tooltipInstance.RTransform.sizeDelta.x / 2) + (thisRect.sizeDelta.x / 2)) * tooltipHorizontalFactor;
                    newPos.y += (tooltipInstance.RTransform.sizeDelta.y / 2) + (thisRect.sizeDelta.y / 2);

                    if (tooltipInstance.ArrowEnabled)
                    {
                        newPos.y += (tooltipInstance.Arrow.rectTransform.sizeDelta.y);
                        tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(0,
                            -((tooltipInstance.RTransform.sizeDelta.y / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.y / 2)));
                        tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                    break;
                case AnchorPosition.CenterLeft:
                    newPos.x -= ((tooltipInstance.RTransform.sizeDelta.x / 2) + (thisRect.sizeDelta.x / 2)) * tooltipHorizontalFactor + (tooltipInstance.Arrow.rectTransform.sizeDelta.x * 2);

                    if (tooltipInstance.ArrowEnabled)
                    {
                        newPos.x += (tooltipInstance.Arrow.rectTransform.sizeDelta.y);
                        tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(
                            ((tooltipInstance.RTransform.sizeDelta.x / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.x / 2)), 0);
                        tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 90);
                    }
                    break;
                case AnchorPosition.CenterRight:
                    newPos.x += ((tooltipInstance.RTransform.sizeDelta.x / 2) + (thisRect.sizeDelta.x / 2)) * tooltipHorizontalFactor + (tooltipInstance.Arrow.rectTransform.sizeDelta.x * 2);

                    if (tooltipInstance.ArrowEnabled)
                    {
                        newPos.x += (tooltipInstance.Arrow.rectTransform.sizeDelta.y);
                        tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(
                            -((tooltipInstance.RTransform.sizeDelta.x / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.x / 2)), 0);
                        tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, -90);
                    }
                    break;
                case AnchorPosition.BottomLeft:
                    newPos.x -= ((tooltipInstance.RTransform.sizeDelta.x / 2) + (thisRect.sizeDelta.x / 2)) * tooltipHorizontalFactor;
                    newPos.y -= (tooltipInstance.RTransform.sizeDelta.y / 2) + (thisRect.sizeDelta.y / 2);

                    if (tooltipInstance.ArrowEnabled)
                    {
                        newPos.y -= (tooltipInstance.Arrow.rectTransform.sizeDelta.y);
                        tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(0,
                            ((tooltipInstance.RTransform.sizeDelta.y / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.y / 2)));
                        tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 180);
                    }
                    break;
                case AnchorPosition.BottomMiddle:
                    newPos.y -= (tooltipInstance.RTransform.sizeDelta.y / 2) + (thisRect.sizeDelta.y / 2);

                    if (tooltipInstance.ArrowEnabled)
                    {
                        newPos.y -= (tooltipInstance.Arrow.rectTransform.sizeDelta.y);
                        tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(0,
                            ((tooltipInstance.RTransform.sizeDelta.y / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.y / 2)));
                        tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 180);
                    }
                    break;
                case AnchorPosition.BottomRight:
                    newPos.x += ((tooltipInstance.RTransform.sizeDelta.x / 2) + (thisRect.sizeDelta.x / 2)) * tooltipHorizontalFactor;
                    newPos.y -= (tooltipInstance.RTransform.sizeDelta.y / 2) + (thisRect.sizeDelta.y / 2);

                    if (tooltipInstance.ArrowEnabled)
                    {
                        newPos.y -= (tooltipInstance.Arrow.rectTransform.sizeDelta.y);
                        tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(0,
                            ((tooltipInstance.RTransform.sizeDelta.y / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.y / 2)));
                        tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 180);
                    }
                    break;

                case AnchorPosition.CenterMiddle:
                default:
                    if (tooltipInstance.ArrowEnabled)
                    {
                        if (arrowDefaultPositionAtMiddle == MiddlePosition.Bottom)
                        {
                            newPos.y += (tooltipInstance.Arrow.rectTransform.sizeDelta.y);
                            tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(0,
                                -((tooltipInstance.RTransform.sizeDelta.y / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.y / 2)));
                            tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 0);
                        }

                        else
                        {
                            newPos.y -= (tooltipInstance.Arrow.rectTransform.sizeDelta.y);
                            tooltipInstance.Arrow.rectTransform.anchoredPosition = new Vector2(0,
                                ((tooltipInstance.RTransform.sizeDelta.y / 2) + (tooltipInstance.Arrow.rectTransform.sizeDelta.y / 2)));
                            tooltipInstance.Arrow.rectTransform.rotation = Quaternion.Euler(0, 0, 180);
                        }
                    }
                    break;
            }

            return newPos;
        }

        /// <summary>
        /// Activates/deactivates the tooltip, which engages in transitions if the tooltip's active state is different from the new state
        /// </summary>
        /// <param name="_isActive">Whether to activate or deactivate the tooltip</param>
        private void SetActive(bool _isActive)
        {
            SetActive(_isActive, transitionMode);
        }

        /// <summary>
        /// Activates/deactivates the tooltip, which engages in transitions if the tooltip's active state is different from the new state
        /// <para>This overload overrides the mode defined for the element</para>
        /// </summary>
        /// <param name="_isActive">Whether to activate or deactivate the tooltip</param>
        /// <param name="mode">The mode of transition to use for animation</param>
        private void SetActive(bool _isActive, TransitionMode mode)
        {
            if (isActive != _isActive || !isActive)
            {
                isActive = _isActive;
                switch (mode)
                {
                    case TransitionMode.Fade:
                        if (!tooltipInstance.gameObject.activeSelf)
                            tooltipInstance.gameObject.SetActive(true);
                        if (!isActive && tooltipInstance.CGroup.alpha > 0 || isActive)
                            StartCoroutine(TransitionFade(isActive));
                        break;
                    case TransitionMode.None:
                    default:
                        tooltipInstance.gameObject.SetActive(isActive);
                        break;
                }
            }
        }

        /// <summary>
        /// Sets the text element text to the stored tooltipText and resizes the container
        /// </summary>
        private void SetText()
        {
            if (!isInitialized)
                Initialize();

            if (tooltipValues != null && tooltipValues.Length > 0)
                tooltipInstance.Text.text = string.Format(tooltipText, tooltipValues.Cast<object>().ToArray()).RepairSerializedEscaping();
            else
                tooltipInstance.Text.text = tooltipText.RepairSerializedEscaping();

            tooltipInstance.RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                tooltipInstance.Text.preferredHeight + tooltipInstance.Text.rectTransform.offsetMin.y * 2);
        }

        /// <summary>
        /// Coroutine for delaying the opening of the tooltip
        /// </summary>
        private IEnumerator DelayOpening()
        {
            yield return new WaitForSeconds(tooltipDelay);
            SetActive(true);
        }

        /// <summary>
        /// Coroutine for the Fade transition
        /// </summary>
        /// <param name="_isActive">Determines whether to fade in or out</param>
        private IEnumerator TransitionFade(bool _isActive)
        {
            float t = 0;
            while (t <= transitionDuration)
            {
                if (_isActive)
                    tooltipInstance.CGroup.alpha = transitionCurveIn.Evaluate(t / transitionDuration);
                else
                    tooltipInstance.CGroup.alpha = transitionCurveOut.Evaluate(t / transitionDuration);

                yield return null;
                t += Time.deltaTime;
            }

            if (_isActive)
                tooltipInstance.CGroup.alpha = 1;
            else
                tooltipInstance.CGroup.alpha = 0;
        }
        #endregion
    }
}
