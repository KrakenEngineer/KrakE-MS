using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#region Game engine usings
using MSEngine.UI;
using MSEngine.Spaceships;
using MSEngine.Saves.Configs;
using MSEngine.Saves.Spaceships;
#endregion

namespace MSEngine.Scenes.Editors
{
    public sealed class ShipEditor : Scene
    {
        [SerializeField] private Vector2Int _fieldSize;

        internal PartField _field;
        internal ShipEditorIO _ui;

        internal GameObject CurrentPart => _ui.CurrentPart;

        public Vector2Int FieldSize => _fieldSize;
        public ShipEditorIO UI => _ui;
        public List<ShipPart> PartsList => _field.PartsList;
        public ShipPart[,] Field => _field.Parts;

        protected override void Awake()
        {
            base.Awake();
            Object panelPrefab = DataLoader.GetUIPrefab(UIPrefabAssigment.Panel);
            PartPanel panel = (Instantiate(panelPrefab, transform) as GameObject).GetComponent<PartPanel>();

            foreach (var key in Constants.PartCategories)
                Instantiate(DataLoader.GetUIPrefab(key), transform.GetChild(1));

            _field = new PartField(_fieldSize);
            _ui = new ShipEditorIO(this, panel, transform.GetChild(0).gameObject.GetComponent<Camera>());
        }

        private void Start()
        {
            if (Buffer.LastLaunchedShip != null)
                LoadShip(Buffer.LastLaunchedShip);
        }

        private void Update()
        {
            if (CurrentPart != null)
                _ui.TryDragPart();

            if (DataLoader.GetKey(KeyAssignment.EditorCopyPart) && Input.GetMouseButton(0))
                _ui.TryCopyPart(_ui.mousePosition);

            else if (Input.GetMouseButtonDown(0))
                if (!_ui.TryPressPartButton())
                    _ui.TryPickPart();

            if (Input.GetMouseButton(1))
                _ui.TryDestroyPart(_ui.mousePosition);
        }

        public void Launch()
        {
            _ui.Launch();
        }

        internal void LoadShip(ShipSaveData data)
        {
            data.Load(ShipLoadMode.Editor, transform, out List<ShipPart> s_parts);

            var parts = new List<ShipPart>();
            foreach (var part in s_parts)
                _field.TryRelease(parts[parts.Count - 1], this);

            foreach (var part in parts)
                if (part != null)
                    part.GenerateNeighboursList(_field);
        }

        internal GameObject CreatePart(ObjectConfig config)
        {
            if (config.Type != ConfigExtensionType.Part) return null;

            var configCopy = config.Copy();
            Transform part = configCopy.MakeObject().transform;
            part.GetComponent<ShipPart>().Orient(_ui.Orientation);

            return part.gameObject;
        }
    }

    public sealed class ShipEditorIO
    {
        public ShipEditorIO(ShipEditor editor, PartPanel panel, Camera camera)
        {
            if (editor == null)
                throw new System.Exception("Can't bind ShipEditorUI to null ShipEditor");
            if (panel == null)
                throw new System.Exception("Can't bind ShipEditorUI to null PartPanel");
            if (camera == null)
                throw new System.Exception("Can't bind ShipEditorUI to null Camera");

            _editor = editor;
            _partPanel = panel;
            _camera = camera;
        }

        internal PartCategory Category { get; private set; } = PartCategory.Control;
        internal PartOrientation Orientation = PartOrientation.Up;

        internal GameObject CurrentPart = null;
        private Camera _camera;
        private ShipEditor _editor;
        private PartPanel _partPanel;

        public Vector3 mousePosition => _camera.ScreenToWorldPoint(Input.mousePosition);

        public void FlipPartsX()
        {
            Orientation = Orientation.FlipX();
            OrientAllParts(Orientation);
        }

        public void FlipPartsY()
        {
            Orientation = Orientation.FlipY();
            OrientAllParts(Orientation);
        }

        public void RotateParts()
        {
            Orientation = Orientation.RotateClockwise();
            OrientAllParts(Orientation);
        }

        public void SetCategory(PartCategory category)
        {
            Category = category;
            _partPanel.SetCategory(category);
            OrientAllParts(Orientation);
        }

        internal void Launch()
        {
            Buffer.LastLaunchedShip = ShipSaveData.Save("*last launched*", "", _editor.PartsList);
            Scene.Load(SceneAssigment.MainTest);
        }

        internal bool TryPressPartButton()
        {
            bool output = false;
            RaycastHit2D hit = Physics2D.Raycast(Input.mousePosition, Vector2.zero);
            if (hit.collider != null)
                if (hit.collider.gameObject.TryGetComponent(out UniversalButton button))
                    if (button.Config != null)
                        output = TrySetPart(button.Config);

            return output;
        }

        internal bool TryPickPart()
        {
            bool output = _editor._field.TryGetPart(mousePosition, out ShipPart part);
            if (output)
            {
                CurrentPart = part.gameObject;
                _editor._field.Pick(part);
            }

            return output;
        }

        internal bool TryDragPart()
        {
            Vector3 offset = Vector3.back * (Constants.CameraDepth - ShipConstants.PartDepth);
            if (!Input.GetMouseButton(0))
            {
                if (_editor._field.TryRelease(CurrentPart.GetComponent<ShipPart>(), _editor))
                    return false;

                CurrentPart.transform.position = mousePosition + offset;
                return true;
            }

            CurrentPart.transform.position = mousePosition + offset;
            return true;
        }

        internal bool TryCopyPart(Vector3 position)
        {
            if (CurrentPart != null)
                return _editor._field.TryRelease(CurrentPart.GetComponent<ShipPart>(), _editor);

            else if (_editor._field.TryGetPart(position, out ShipPart part))
            {
                CurrentPart = _editor.CreatePart(part.Config);
                return true; 
            }

            return false;
        }

        internal bool TryDestroyPart(Vector3 position)
        {
            if (CurrentPart != null)
            {
                Object.Destroy(CurrentPart.gameObject);
                return true;
            }

            else if (_editor._field.TryGetPart(position, out ShipPart part))
            {
                Object.Destroy(part.gameObject);
                return true;
            }

            return false;
        }

        internal bool TrySetPart(ObjectConfig config)
        {
            if (config.Type == ConfigExtensionType.Part)
            {
                CurrentPart = _editor.CreatePart(config);
                return true;
            }

            return false;
        }

        private void OrientAllParts(PartOrientation orientation)
        {
            foreach (Transform transform in _partPanel.transform.GetChild(1))
                OrientPart(orientation, transform);
        }

        private void OrientPart(PartOrientation orientation, Transform part)
        {
            int flip = orientation.Flip ? -1 : 1;
            part.localScale = new Vector3(Mathf.Abs(part.localScale.x) * flip, Mathf.Abs(part.localScale.y), 0);
            part.eulerAngles = new Vector3(0, 0, -(int)orientation.Direction * 90);
        }
    }
}