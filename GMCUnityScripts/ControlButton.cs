using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using UnityEngine.UI;
using System;
using System.Linq;
namespace GMCUnityScripts
{

    [Serializable]
    public class ControlButton : MonoBehaviour
    {
        // mono which attaches to a control button image and stores information
        // like the keybinds it corresponds to and allows for setting the color

        [HideInInspector]
        public List<InControl.Key> BoundKeyboard = new List<InControl.Key>() { };
        [HideInInspector]
        public List<InControl.Mouse> BoundMouse = new List<InControl.Mouse>() { };
        [HideInInspector]
        public List<InControl.InputControlType> BoundController = new List<InControl.InputControlType>() { };
        [HideInInspector]
        private string _name = "";
        [HideInInspector]
        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._name))
                {
                    this._name = this.gameObject.name;
                }
                return this._name;
            }
            set
            {
                this._name = value;
            }

        }
        private Image image;
        void SetControlToClosestMatch()
        {
            List<string> allEnums = new List<string>() { };
            allEnums.AddRange(Enum.GetNames(typeof(Key)));
            allEnums.AddRange(Enum.GetNames(typeof(Mouse)));
            allEnums.AddRange(Enum.GetNames(typeof(InputControlType)));
            string closest = allEnums.OrderBy(n => Distance(this.gameObject.name.ToLower(), n.ToLower())).First();
            if (Enum.TryParse(closest, out InControl.InputControlType type))
            {
                this.BoundController.Add(type);
            }
            else if (Enum.TryParse(closest, out InControl.Key key))
            {
                this.BoundKeyboard.Add(key);
            }
            else if (Enum.TryParse(closest, out InControl.Mouse mouse))
            {
                this.BoundMouse.Add(mouse);
            }
        }
        void Reset()
        {
            if (!this.BoundMouse.Any() && !this.BoundKeyboard.Any() && !this.BoundController.Any())
            {
                this.SetControlToClosestMatch();
            }
        }
        void Start()
        {
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                this.Name = this.gameObject.name;
            }
            if (!this.BoundMouse.Any() && !this.BoundKeyboard.Any() && !this.BoundController.Any())
            {
                this.SetControlToClosestMatch();
            }
            this.image = this.GetComponent<Image>();
        }
        public void SetColor(Color color)
        {
            this.image = this.GetComponent<Image>();
            if (this.image != null) { this.image.color = color; }
        }
        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int Distance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Mathf.Min(
                        Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}
