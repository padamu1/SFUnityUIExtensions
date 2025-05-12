using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SFUnityUIExtensions.Runtime.Core;

namespace SFUnityUIExtensions.Runtime.Components
{
    [RequireComponent(typeof(RectTransform))]
    public class RangeSlider : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        #region SerializeFields

        [Space]
        [SerializeField, ReadOnly] private RectTransform rectTransform;
        [SerializeField, ReadOnly] private CanvasScaler canvasScaler;

        [Space]
        [SerializeField] private RectTransform leftHandle;
        [SerializeField] private RectTransform rightHandle;
        [SerializeField] private RectTransform fillAmount;

        [Space]
        [SerializeField] private float minValue;
        [SerializeField] private float maxValue;

        [Space]
        [SerializeField] private float value1;
        [SerializeField] private float value2;

        [Space]
        public UnityEvent<float, float> onValueChanged;

        [Space]
        [SerializeField] private bool roundToInt;

        #endregion

        private int moveValueNo;

        private void Awake()
        {
            Initialize();
        }


        private void OnValidate()
        {
            ValidateValue();

            Initialize();
        }

        private void Initialize()
        {
            rectTransform = this.GetComponent<RectTransform>();
            canvasScaler = this.GetComponentInParent<CanvasScaler>();

            var prevAnchorMax = rectTransform.anchorMax;

            rectTransform.anchorMax = new Vector2(rectTransform.anchorMin.x != rectTransform.anchorMax.x ? rectTransform.anchorMin.x : rectTransform.anchorMax.x, 
                rectTransform.anchorMin.y != rectTransform.anchorMax.y ? rectTransform.anchorMin.y : rectTransform.anchorMax.y);

            if (prevAnchorMax != rectTransform.anchorMax)
            {
                Debug.LogWarning("RangeSlider dosen't support stretch UI");
            }

            if (fillAmount == null)
            {
                Debug.LogError("Fill Amount not set");
                return;
            }

            SetSldierRectSetting();

            ValidateMinMaxValue();

            UpdateSlider();
        }

        private void SetSldierRectSetting()
        {
            fillAmount.pivot = Vector2.one * 0.5f;
            fillAmount.anchorMin = Vector2.zero;
            fillAmount.anchorMax = Vector2.one;

            if (leftHandle != null)
            {
                leftHandle.pivot = Vector2.one * 0.5f;
                leftHandle.anchorMin = new Vector2(0, 0.5f);
                leftHandle.anchorMax = new Vector2(0, 0.5f);
            }

            if (rightHandle != null)
            {
                rightHandle.pivot = Vector2.one * 0.5f;
                rightHandle.anchorMin = new Vector2(1f, 0.5f);
                rightHandle.anchorMax = new Vector2(1f, 0.5f);
            }
        }

        private void ValidateMinMaxValue()
        {
            if (minValue >= maxValue)
            {
                minValue = maxValue - 1;
            }
        }

        private void ValidateValue()
        {
            if (value1 < value2)
            {
                value1 = Mathf.Clamp(value1, minValue, maxValue);
                value2 = (value2 < minValue || value2 > maxValue) ? maxValue : value2;
            }
            else
            {
                value2 = Mathf.Clamp(value2, minValue, maxValue);
                value1 = (value1 < minValue || value1 > maxValue) ? maxValue : value1;
            }
        }
        
        private void UpdateSlider()
        {
            var sizeDelta = rectTransform.sizeDelta;
            var diff = maxValue - minValue;

            if (Mathf.Approximately(diff, 0f))
                return;

            float normalizedMin = Mathf.Clamp01((Mathf.Min(value1, value2) - minValue) / diff);
            float normalizedMax = Mathf.Clamp01((maxValue - Mathf.Max(value1, value2)) / diff);

            float offsetMinX = sizeDelta.x * normalizedMin;
            float offsetMaxX = sizeDelta.x * normalizedMax;

            fillAmount.offsetMin = new Vector2(offsetMinX, fillAmount.offsetMin.y);
            fillAmount.offsetMax = new Vector2(-1 * offsetMaxX, fillAmount.offsetMax.y);

            UpdateHandle();

            onValueChanged?.Invoke(Mathf.Min(value1, value2), Mathf.Max(value1, value2));
        }

        private void UpdateHandle()
        {
            if (leftHandle != null)
            {
                leftHandle.anchoredPosition = new Vector2(fillAmount.offsetMin.x, leftHandle.anchoredPosition.y);
            }

            if (rightHandle != null)
            {
                rightHandle.anchoredPosition = new Vector2(fillAmount.offsetMax.x, rightHandle.anchoredPosition.y);
            }
        }

        public void SetMinMaxValue(float minValue, float maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            ValidateMinMaxValue();
            UpdateSlider();
        }

        public void SetValue(float value1, float value2)
        {
            this.value1 = value1;
            this.value2 = value2;
            UpdateSlider();
        }

        private void ChangeValueWithDrag(Vector2 pos)
        {
            var value = (pos.x + rectTransform.sizeDelta.x / 2) / rectTransform.sizeDelta.x;

            value = value * (this.maxValue - this.minValue) + minValue;

            if (moveValueNo == 1)
            {
                value1 = value;
            }
            else if (moveValueNo == 2)
            {
                value2 = value;
            }

        }

        private Vector2 PointerToCanvasPosition(Vector2 pointer)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(this.rectTransform, pointer, null, out Vector2 pos);
            return pos;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var position = eventData.position;
            SelectMoveValue(PointerToCanvasPosition(position));
        }

        private void SelectMoveValue(Vector2 pos)
        {
            var value = (pos.x + rectTransform.sizeDelta.x / 2) / rectTransform.sizeDelta.x;

            value = value * (this.maxValue - this.minValue) + minValue;

            float diff1 = Mathf.Abs(value - value1);
            float diff2 = Mathf.Abs(value - value2);

            if (diff1 <= diff2)
            {
                moveValueNo = 1;
            }
            else
            {
                moveValueNo = 2;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            var position = eventData.position;
            ChangeValueWithDrag(PointerToCanvasPosition(position));
            ValidateValue();
            UpdateSlider();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (roundToInt)
            {
                value1 = Mathf.RoundToInt(value1);
                value2 = Mathf.RoundToInt(value2);
                UpdateSlider();
            }
        }
    }
}
