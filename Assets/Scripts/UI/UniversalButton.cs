using UnityEngine;
using MSEngine.Saves.Configs;
using MSEngine.Scenes.Editors;
using MSEngine.Scenes;

namespace MSEngine.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public sealed class UniversalButton : MonoBehaviour
    {
        public ObjectConfig Config;
        public PartCategory Category;

        public void SetCategory()
        {
            transform.parent.parent.GetComponent<ShipEditor>().UI.SetCategory(Category);
        }

        public void RotateParts()
        {
            transform.parent.parent.parent.GetComponent<ShipEditor>().UI.RotateParts();
        }

        public void FlipPartsX()
        {
            transform.parent.parent.parent.GetComponent<ShipEditor>().UI.FlipPartsX();
        }

        public void FlipPartsY()
        {
            transform.parent.parent.parent.GetComponent<ShipEditor>().UI.FlipPartsY();
        }

        public void LoadEditor()
        {
            Scene.LoadShipEditor();
        }
    }
}