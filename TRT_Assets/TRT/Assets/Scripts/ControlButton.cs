using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using UnityEngine.UI;

public class ControlButton : MonoBehaviour
{
    // mono which attaches to a control button image and stores information
    // like the keybinds it corresponds to and allows for setting the color
    
    public List<InControl.Key> BoundKeyboard { get; set; } = new List<InControl.Key>() { };
    public List<InControl.Mouse> BoundMouse { get; set; } = new List<InControl.Mouse>() { };
    public List<InControl.InputControlType> BoundController { get; set; } = new List<InControl.InputControlType>() { };
    private string _name = "";
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
    void Start()
    {
        if (string.IsNullOrWhiteSpace(this.Name))
        {
            this.Name = this.gameObject.name;
        }
        this.image = this.GetComponent<Image>();
    }
    public void SetColor(Color color)
    {
        this.image = this.GetComponent<Image>();
        if (this.image != null) { this.image.color = color; }
    }
}
