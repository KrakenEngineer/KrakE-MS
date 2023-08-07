using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MSEngine.Saves.Configs;

namespace MSEngine.UI
{
    public sealed class PartPanel : MonoBehaviour
    {
        private void Start()
        {
            SetCategory(PartCategory.Control);
        }

        public void SetCategory(PartCategory category)
        {
            if (category == PartCategory.Count)
                throw new System.Exception("PartCategory.Count is NOT a category, it is count of categories");

            transform.GetChild(0).gameObject.GetComponent<Text>().text = category.ToString();
            ClearParts();

            PartCategoryData data = DataLoader.GetPartCategory(category);
            foreach (var part in data.Parts)
                GeneratePart(part.Value);
        }

        private void ClearParts()
        {
            foreach (Transform child in transform.GetChild(1))
                Destroy(child.gameObject);
        }

        private void GeneratePart(ObjectConfig config)
        {
            GameObject obj = Instantiate(DataLoader.GetUIPrefab(UIPrefabAssignment.PartButton), transform.GetChild(1)) as GameObject;
            obj.GetComponent<Image>().sprite = config.Sprite;
            obj.GetComponent<UniversalButton>().Config = config;
        }
    }
}