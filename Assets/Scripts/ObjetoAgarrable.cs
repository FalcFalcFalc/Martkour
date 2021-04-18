using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjetoAgarrable : MonoBehaviour
{
    Quaternion rotOrig;

    void Start()
    {
        rotOrig = transform.rotation;
    }

    bool selected = false, agarrado = false, reorientando = false;

    public void ToggleOutline()
    {
        selected = !selected;
    }

    public void ToggleAgarrar()
    {
        agarrado = !agarrado;
        if(agarrado) StartCoroutine(Reorientar());
    }

    IEnumerator Reorientar(){
        reorientando = true;
        Quaternion rotActual = transform.rotation;
        float longitud = 20;
        for (int i = 0; i < longitud; i++)
        {
            float easeado = Mathf.Cos(((i/longitud)*Mathf.PI)/2);
                  easeado = 1 - easeado * easeado;
            transform.rotation = Quaternion.Lerp(rotActual,rotOrig,easeado);
            yield return new WaitForSeconds(1/longitud);
        }
        reorientando = false;
    }

    void Update() {
        if(agarrado && !reorientando && transform.rotation != rotOrig){
            transform.rotation = rotOrig;
        }
    }
}
