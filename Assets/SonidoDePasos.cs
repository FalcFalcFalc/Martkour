using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonidoDePasos : MonoBehaviour
{
    [SerializeField] AudioClip[] sonidos;
    float pasosDados = 0;
    AudioSource audioSource;
    float anguloAnterior = 0;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
        //itemAnterior = transform.GetComponentsInChildren<Transform>()[3];
    }

    void Update(){
        bool pregunta = transform.localEulerAngles.x > pasosDados * 90;
        print(transform.localEulerAngles.x + " > " + pasosDados * 90 + " -> " + pregunta);
        if(pregunta && transform.localEulerAngles.x != anguloAnterior){
            pasosDados++;
            if(pasosDados > 3){
                pasosDados = 0;
            }
            int index = Mathf.RoundToInt(Random.Range(0,3));
            audioSource.clip = sonidos[index];
            audioSource.Play();
            anguloAnterior = transform.localEulerAngles.x;
        }
    }
}
