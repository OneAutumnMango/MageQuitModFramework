using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MageQuitModFramework.UI
{
    public class DynamicModMenu : MonoBehaviour
    {
        private GameObject _menuPanel;
        private Transform _contentContainer;
        private Dictionary<string, GameObject> _modPanels = new();
        private Dictionary<string, GameObject> _optionsPanels = new();

        public void Initialize()
        {
            CreateMenuStructure();
            BuildModList();
        }

        private void CreateMenuStructure()
        {
            _menuPanel = UIComponents.CreatePanel(transform, "ModMenuPanel", 400, 600);
            _menuPanel.GetComponent<Image>().color = StyleManager.BackgroundColor;

            var scrollView = UIComponents.CreateScrollView(_menuPanel.transform, 400, 600);
            _contentContainer = scrollView.GetChild(0).GetChild(0);
        }

        private void BuildModList()
        {
            foreach (Transform child in _contentContainer)
                Destroy(child.gameObject);

            _modPanels.Clear();
            _optionsPanels.Clear();

            foreach (var modEntry in ModUIRegistry.GetAllMods())
            {
                CreateModEntry(modEntry);
            }
        }

        private void CreateModEntry(ModUIEntry modEntry)
        {
            var modPanel = UIComponents.CreatePanel(_contentContainer, $"{modEntry.ModName}_Panel", 380, 40);
            modPanel.GetComponent<Image>().color = StyleManager.PanelColor;
            _modPanels[modEntry.ModName] = modPanel;

            var headerButton = modPanel.AddComponent<Button>();
            headerButton.onClick.AddListener(() => ToggleModPanel(modEntry));

            var headerLayout = modPanel.AddComponent<HorizontalLayoutGroup>();
            headerLayout.padding = new RectOffset(10, 10, 5, 5);
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childControlWidth = false;
            headerLayout.childControlHeight = false;

            var arrow = UIComponents.CreateText(modPanel.transform, $"{modEntry.ModName}_Arrow", 
                modEntry.IsExpanded ? "▼" : "►", 20);
            arrow.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 30);

            var title = UIComponents.CreateText(modPanel.transform, $"{modEntry.ModName}_Title", 
                modEntry.ModName, 18);
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 30);

            var optionsContainer = UIComponents.CreatePanel(_contentContainer, $"{modEntry.ModName}_Options", 380, 0);
            optionsContainer.GetComponent<Image>().color = StyleManager.BackgroundColor;
            optionsContainer.SetActive(modEntry.IsExpanded);
            _optionsPanels[modEntry.ModName] = optionsContainer;

            var layout = optionsContainer.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(15, 10, 5, 5);
            layout.spacing = 5;
            layout.childControlHeight = false;
            layout.childControlWidth = true;

            if (modEntry.BuildOptionsUI != null)
            {
                modEntry.BuildOptionsUI(optionsContainer.transform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(optionsContainer.GetComponent<RectTransform>());
            }
        }

        private void ToggleModPanel(ModUIEntry modEntry)
        {
            modEntry.IsExpanded = !modEntry.IsExpanded;

            if (_optionsPanels.TryGetValue(modEntry.ModName, out var optionsPanel))
            {
                optionsPanel.SetActive(modEntry.IsExpanded);
            }

            if (_modPanels.TryGetValue(modEntry.ModName, out var modPanel))
            {
                var arrow = modPanel.transform.Find($"{modEntry.ModName}_Arrow")?.GetComponent<Text>();
                if (arrow != null)
                    arrow.text = modEntry.IsExpanded ? "▼" : "►";
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentContainer.GetComponent<RectTransform>());
        }

        public void RefreshModList()
        {
            BuildModList();
        }

        public void Show()
        {
            if (_menuPanel != null)
                _menuPanel.SetActive(true);
        }

        public void Hide()
        {
            if (_menuPanel != null)
                _menuPanel.SetActive(false);
        }

        public void Toggle()
        {
            if (_menuPanel != null)
                _menuPanel.SetActive(!_menuPanel.activeSelf);
        }
    }
}
