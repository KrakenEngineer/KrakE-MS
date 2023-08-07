using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MSEngine.UI
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Indicator : MonoBehaviour
    {
        [SerializeField] private IndicatorSettings _settings;
        [SerializeField] private float _value;

        public IndicatorMode Mode => _settings.Mode;
        public Color DefaultColor => _settings.DefaultColor;
        public Vector3 StartPoint => _settings.StartPoint;
        public Vector3 StartScale => _settings.StartScale;

        private void Start()
        {
            transform.localPosition = StartPoint;
            transform.localEulerAngles = Vector3.zero;
            transform.localScale = StartScale;
            GetComponent<SpriteRenderer>().color = DefaultColor;
        }

        public static Indicator Create(float startValue, Transform parent, IndicatorSettings settings)
        {
            if (settings.Mode == IndicatorMode.None)
                throw new System.Exception("Can't make an indicator without mode");

            var indicatorObject = new GameObject($"*indicator {settings.Mode}*");
            indicatorObject.transform.parent = parent;
            indicatorObject.AddComponent<SpriteRenderer>().sprite = DataLoader.GetSprite("WhiteSquare");

            var output = indicatorObject.AddComponent<Indicator>();
            output._settings = settings;
            output.TrySetValue(Mathf.Clamp01(startValue));

            return output;
        }

        public bool TrySetValue(float value)
        {
            if (0 > value || 1 < value)
                return false;

            switch (Mode)
            {
                case IndicatorMode.Vertical:
                    ScaleVertical(value);
                    break;

                case IndicatorMode.Horizontal:
                    ScaleHorizontal(value);
                    break;

                case IndicatorMode.Scalable:
                    Scale(value);
                    break;

                case IndicatorMode.Transparent:
                    SetTransparency(value);
                    break;

                default:
                    return false;
            }

            _value = value;
            return true;
        }

        private void ScaleVertical(float value)
        {
            transform.localPosition = new Vector3(StartPoint.x, StartPoint.y * value, 0);
            transform.localScale = new Vector3(StartScale.x, StartScale.y * value, 0);
        }

        private void ScaleHorizontal(float value)
        {
            transform.localPosition = new Vector3(StartPoint.x * value, StartPoint.y, 0);
            transform.localScale = new Vector3(StartScale.x * value, StartScale.y, 0);
        }

        private void Scale(float value)
        {
            transform.localScale = new Vector3(StartScale.x * value, StartScale.y * value, 0);
        }

        private void SetTransparency(float value)
        {
            GetComponent<SpriteRenderer>().color =
                new Color(DefaultColor.r, DefaultColor.g, DefaultColor.b, DefaultColor.a * value);
        }
    }

    [System.Serializable]
    public struct IndicatorSettings
    {
        [SerializeField] private IndicatorMode _mode;
        [SerializeField] private Color _defaultColor;
        [SerializeField] private Vector3 _startPoint;
        [SerializeField] private Vector3 _startScale;

        public IndicatorSettings(IndicatorMode mode, Color color, Vector3 startPoint, Vector3 startScale)
        {
            _mode = mode;
            _defaultColor = color;
            _startPoint = startPoint;
            _startScale = startScale;
        }

        public IndicatorMode Mode => _mode;
        public Color DefaultColor => _defaultColor;
        public Vector3 StartPoint => _startPoint;
        public Vector3 StartScale => _startScale;
    }

    public enum IndicatorMode
    {
        None,
        Vertical,
        Horizontal,
        Scalable,
        Transparent
    }
}