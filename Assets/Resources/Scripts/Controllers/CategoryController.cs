using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Trivia.GameManager;

namespace Trivia
{
    public class CategoryController : MonoBehaviour
    {
        [SerializeField] Transform _template;

        void Start() => StartCoroutine(Initialize());

        IEnumerator Initialize()
        {
            _template.gameObject.SetActive(false);

            while (Instance == null)
                yield return null;

            var list = Instance.GetAllCatagories().OrderBy(c => c.Name).ToList();

            foreach (var catagory in list)
            {
                var panel = Instantiate(_template, transform);
                SetToggle(panel, catagory.Active);
                SetName(panel, catagory.Name);
                SetImage(panel, catagory.Icon);
                StartCoroutine(SetCount(panel, catagory));
                panel.gameObject.SetActive(true);
            }

            Instance.m_TriviaConfiguraton.CategoryConfigChangedEvent += RefreshCategory;
        }

        void RefreshCategory(Category category)
        {
            print("event: " + category); 
        }

        void SetToggle(Transform panel, bool isActive)
        {
            var toggle = panel.GetComponentInChildren<Toggle>(true);
            toggle.isOn = isActive;
        }

        void SetName(Transform panel, string name)
        {
            var nameObj = panel.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(c => c.name == "Name");

            if (nameObj != null)
                nameObj.text = name;
        }

        void SetImage(Transform panel, Sprite image)
        {
            var imageObj = panel.GetComponentsInChildren<Image>(true).FirstOrDefault(c => c.name == "Image");

            if (imageObj != null)
                imageObj.sprite = image;
        }

        IEnumerator SetCount(Transform panel, CategoryModel category)
        {
            while (!category.TextQuestions.m_QuestionsLoaded)
                yield return null;

            var countObj = panel.GetComponentsInChildren<TextMeshProUGUI>(true).FirstOrDefault(c => c.name == "count");
            if (countObj != null)
                countObj.text = category.TextQuestions.TotalQuestions.ToString();
        }

    }
}