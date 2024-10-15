using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Settings;
using GameManagers;
using TMPro;
using System.Text.RegularExpressions;
using static UnityEngine.Rendering.DebugUI;

namespace UI
{
    class ChatPanel : BasePanel
    {
        private InputField _inputField;
        private TMP_InputField inpu2;
        private GameObject _panel;
        private List<GameObject> _lines = new List<GameObject>();
        private GameObject scrollbox;
        private TMP_InputField scrolltext;
        private TextMeshProUGUI scrollfont;
        protected override string ThemePanel => "ChatPanel";
        protected Transform _caret;
        public bool IgnoreNextActivation;


        public override void Setup(BasePanel parent = null)
        {
            //ScrollboxV2
            scrollbox = transform.Find("Scrollboxv2").gameObject;
            scrolltext = scrollbox.transform.Find("TextBox").GetComponent<TMP_InputField>();
            scrollfont = scrolltext.transform.Find("Text Area").transform.Find("Text").GetComponent<TextMeshProUGUI>();
            scrollfont.fontSize = 20;
            scrollbox.GetComponent<LayoutElement>().preferredHeight = SettingsManager.UISettings.ChatHeight.Value;
            scrolltext.onValueChanged.AddListener(delegate { FieldValueChange(); });


            _inputField = transform.Find("InputField").GetComponent<InputField>();
            _panel = transform.Find("Content/Panel").gameObject;
            transform.Find("Content").GetComponent<LayoutElement>().preferredHeight = SettingsManager.UISettings.ChatHeight.Value;
            var style = new ElementStyle(fontSize: 20, themePanel: ThemePanel);
            _inputField.colors = UIManager.GetThemeColorBlock(style.ThemePanel, "InputField", "Input");
            _inputField.transform.Find("Text").GetComponent<Text>().color = UIManager.GetThemeColor(style.ThemePanel, "InputField", "InputTextColor");
            _inputField.selectionColor = UIManager.GetThemeColor(style.ThemePanel, "InputField", "InputSelectionColor");
            transform.GetComponent<RectTransform>().sizeDelta = new Vector2(SettingsManager.UISettings.ChatWidth.Value, 0f);
            _inputField.onEndEdit.AddListener((string text) => OnEndEdit(text));
            _inputField.text = "";

            //InputV2
            inpu2 = transform.Find("InputV2").GetComponent<TMP_InputField>();
            inpu2.colors = UIManager.GetThemeColorBlock(style.ThemePanel, "InputField", "Input");
            inpu2.transform.Find("Text Area").transform.Find("Text").GetComponent<TextMeshProUGUI>().color = UIManager.GetThemeColor(style.ThemePanel, "InputField", "InputTextColor");
            inpu2.selectionColor = UIManager.GetThemeColor(style.ThemePanel, "InputField", "InputSelectionColor");
            inpu2.onEndEdit.AddListener((string text)=>OnEndEdit(text));
            inpu2.text = "";

            if (SettingsManager.UISettings.ChatWidth.Value == 0f)
            {
                _inputField.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
                inpu2.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
            }
            
            //Disabling the current chatbox and input
            transform.Find("Content").gameObject.SetActive(false);
            _inputField.gameObject.SetActive(false);
            
            Sync();
        }

        public void Sync()
        {
            foreach (GameObject go in _lines)
                Destroy(go);
            _lines.Clear();
            scrolltext.text = "";
            scrollfont.fontSize = SettingsManager.UISettings.ChatFontSize.Value;
            int bgsetting = SettingsManager.UISettings.ChatBackground.Value;
            if (bgsetting > 0) {
                Image chatbg = scrollbox.transform.Find("TextBox").GetComponent<Image>();
                Color newColor = chatbg.color;
                if (bgsetting == 1)
                {
                    newColor.r = 1f;
                    newColor.g = 1f;
                    newColor.b = 1f;
                    newColor.a = 1f;
                }
                else if (bgsetting == 2)
                {
                    newColor.a = 0f;
                }
                chatbg.color = newColor;
            }
            AddLines(ChatManager.Lines);
        }

        public void Activate()
        {
            //Inpu2 lines
            inpu2.Select();
            inpu2.ActivateInputField();

            //_inputField.Select();
            //_inputField.ActivateInputField();
        }

        public bool IsInputActive()
        {
            return inpu2.isFocused;
            //return _inputField.isFocused;
        }

        public void OnEndEdit(string text)
        {
            if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter))
                return;

            string input = inpu2.text;
            inpu2.text = "";

            //string input = _inputField.text;
            //_inputField.text = "";

            IgnoreNextActivation = SettingsManager.InputSettings.General.Chat.ContainsEnter();     
            ChatManager.HandleInput(input);
        }

        public void AddLine(string line)
        {
            _lines.Add(CreateLine(line));
            scrolltext.text = scrolltext.text + "<br>" + LinkTagger(line);
            Canvas.ForceUpdateCanvases();
            ClearExcessLines();
        }

        public void AddLines(List<string> lines)
        {
            foreach (string line in lines)
            {
                _lines.Add(CreateLine(line));
                scrolltext.text = scrolltext.text + "<br>" + LinkTagger(line);
            }
            Canvas.ForceUpdateCanvases();
            ClearExcessLines();
        }

        protected void ClearExcessLines()
        {
            int maxHeight = SettingsManager.UISettings.ChatHeight.Value;
            float currentHeight = 0;
            for (int i = 0; i < _lines.Count; i++)
                currentHeight += _lines[i].GetComponent<RectTransform>().sizeDelta.y;
            float heightToRemove = Mathf.Max(currentHeight - maxHeight, 0f);
            while (heightToRemove > 0f && _lines.Count > 0)
            {
                float height = _lines[0].GetComponent<RectTransform>().sizeDelta.y;
                heightToRemove -= height;
                if (heightToRemove > 0f)
                {
                    Destroy(_lines[0]);
                    _lines.RemoveAt(0);
                }
            }
        }

        public void FieldValueChange()
        {
            if(!scrolltext.isFocused)
            scrolltext.verticalScrollbar.value = 1f;
        }

        public string LinkTagger(string text)
        {
            Regex r = new Regex(@"\b(?:https?://|www\.)\S+\b");

            foreach (Match m in r.Matches(text))
            {
                text = text.Replace(m.Value, string.Format("<link><color=#2b5ca1><u>{0}</u></color></link>", m.Value));
            }
            return text;
        }    

        private void Update()
        {
            if (!_caret && inpu2 != null)
            {
                _caret = inpu2.transform.Find(inpu2.transform.name + " Input Caret");
                if (_caret)
                {
                    var graphic = _caret.GetComponent<Graphic>();
                    if (!graphic)
                        _caret.gameObject.AddComponent<Image>();
                }
            }
        }

        protected GameObject CreateLine(string text)
        {
            var style = new ElementStyle(fontSize: SettingsManager.UISettings.ChatFontSize.Value, themePanel: ThemePanel);
            GameObject line = ElementFactory.CreateDefaultLabel(_panel.transform, style, text, alignment: TextAnchor.MiddleLeft);
            line.GetComponent<Text>().color = UIManager.GetThemeColor(style.ThemePanel, "TextColor", "Default");
            return line;
        }
    }
}
