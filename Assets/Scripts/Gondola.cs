using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gondola : MonoBehaviour
{
    [SerializeField] Transform[] estantes;
    [SerializeField] ObjetoAgarrable[] pobladores;

    void OnDrawGizmos() {
        foreach (Transform estante in estantes)
        {    
            foreach (Transform posicion in estante)
            {
                Gizmos.DrawWireSphere(posicion.position,0.05f);       
            }
        }
    }

    void Awake(){
        PoblarEstantes();
    }

    void PoblarEstantes(){
        int i = 0;
        foreach (Transform estante in estantes)
        {
            i = Random.Range(0,pobladores.Length);
            Vector3 euler = pobladores[i].transform.localEulerAngles;
            foreach (Transform posicion in estante)
            {
                if(Random.Range(0f,1f) < .7f) Instantiate(pobladores[i], posicion.position + Vector3.up * 0.25f, Quaternion.Euler(euler));
            }
            //i++;
            //if(i >= pobladores.Length) i = 0;
        }
    }
}
