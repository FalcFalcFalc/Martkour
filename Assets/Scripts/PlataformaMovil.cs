using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaMovil : MonoBehaviour
{
    [SerializeField] Transform[] posiciones;
    int index = 0;
    float time = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time+=Time.deltaTime;
        if(time >= 5){
            time -= 5;
            SiguientePosicion();
        }
    }

    void SiguientePosicion(){
        index++;
        if(index > posiciones.Length -1){
            index = 0;
        }
        LeanTween.move(this.gameObject,posiciones[index],2f);
    }
}
