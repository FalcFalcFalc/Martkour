using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent (typeof(Outline))]
public class ObjetoAgarrable : MonoBehaviour
{
    //Outline ol;
    // Start is called before the first frame update
    void Start()
    {
        //ol = GetComponent<Outline>();
    }

    bool selected = false;

    public void ToggleOutline()
    {
        selected = !selected;
        /*if (selected) {
            Outline ol = gameObject.AddComponent<Outline>();
            ol.OutlineColor = Color.yellow;
            ol.OutlineWidth = 6;
            ol.OutlineMode = Outline.Mode.OutlineAndSilhouette;
        }
        else Destroy(GetComponent<Outline>());*/
    }
}
