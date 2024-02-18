using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Splotch.UserInterface
{
    public class SplotchGUI : MonoBehaviour
    {
        public static void Load()
        {
            GameObject gameObject = new GameObject("splotch-gui", typeof(SplotchGUI));
            DontDestroyOnLoad(gameObject);
        }

        void OnGUI()
        {
            
        }

        public static Popup ShowPopup(string title, string text)
        {
            GameObject gameObject = new GameObject($"popup {title}", typeof(Popup));
            DontDestroyOnLoad(gameObject);
            Popup popup = gameObject.GetComponent<Popup>();

            popup.title = title;
            popup.text = text;
            return popup;
        }

        public static Popup ShowPopup(string text)
        {
            return ShowPopup(text, "");
        }
    }

    public class Popup : MonoBehaviour
    {
        public string title;
        public string text;
        public int width = 300;
        public int textOffset = 20;
        public Rect okbutton = new Rect(0, 0, 75, 20);

        void OnGUI()
        {
            var textStyle = new GUIStyle();
            textStyle.alignment = TextAnchor.MiddleLeft;

            var height = (int)textStyle.CalcHeight(new GUIContent(text), width);
            var box = new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height + textOffset + okbutton.height);
            var infotext = box;
            infotext.y += textOffset;
            infotext.height -= textOffset + okbutton.height;

            box.width += 40;
            box.x -= 20;

            okbutton.x = box.x + box.width - okbutton.width - 10;
            okbutton.y = box.y + box.height - okbutton.height - 10;

            var bgcolor = Color.gray;
            bgcolor.a = 0.7F;
            GUI.backgroundColor = bgcolor;

            GUI.Box(box, title);

            

            GUI.color = Color.white;
            GUI.TextField(infotext, text, textStyle);
            if (GUI.Button(okbutton, "ok"))
            {
                Destroy(gameObject);
            }
        }
    }
}
