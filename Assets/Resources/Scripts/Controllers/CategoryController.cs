using System;
using System.Collections;
using System.Collections.Generic;
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

        List<CategoryModel> _categoryList;

        void Start() => StartCoroutine(Initialize());

        IEnumerator Initialize()
        {
            _template.gameObject.SetActive(false);

            while (Instance == null)
                yield return null;

            _categoryList = Instance.GetAllCatagories().OrderBy(c => c.Name).ToList();

            foreach (var model in _categoryList)
            {
                var panel = Instantiate(_template, transform);
                panel.name = model.Type.ToString();

                SetToggle(panel, model.Active);
                SetName(panel, model.Name);
                SetImage(panel, model.Icon);
                StartCoroutine(SetCount(panel, model));
                panel.gameObject.SetActive(true);
            }

            Instance.m_TriviaConfiguraton.CategoryConfigChangedEvent += RefreshCategory;
        }

        void RefreshCategory(Category category)
        {
            var panel = transform.Find(category.ToString());
            var model = _categoryList.First(c=> c.Type.Equals(category));

            StartCoroutine(SetCount(panel, model));
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