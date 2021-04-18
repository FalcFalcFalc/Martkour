using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interactuar : MonoBehaviour
{
    [SerializeField] FirstPersonCameraController fpcc;
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] LayerMask mascaraAgarrables;
    [SerializeField] LayerMask mascaraRaycast;
    [SerializeField] LayerMask mascaraPuerta;
    [SerializeField] Image reticula;
    [SerializeField] Gradient colorReticula;
    
    RaycastHit rcAgarrar;
    ObjetoAgarrable agarrarAnterior = null;
    Transform pickedUp = null;
    bool puerta = false;
    int evaluate = 0;

    void Update() {
        ChequearAgarrar();
        if(puerta) pickedUp.GetComponent<Rigidbody>().AddTorque(Vector3.up * -30 * Input.GetAxis("Mouse X"), ForceMode.Force);

    }

    void IntercambiarOutline(ObjetoAgarrable nuevo){
        if(agarrarAnterior != null) agarrarAnterior.ToggleOutline();
        agarrarAnterior = nuevo;
        if(agarrarAnterior != null) agarrarAnterior.ToggleOutline();
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        if(rcAgarrar.point != Vector3.zero) Gizmos.DrawSphere(rcAgarrar.point,0.05f);
    }

    void CambiarReticula(int value){
        if(evaluate != value)
        {
            LeanTween.cancel(reticula.GetComponent<RectTransform>());
            LeanTween.color(reticula.GetComponent<RectTransform>(), colorReticula.Evaluate(value),0.5f).setEase(LeanTweenType.easeOutCubic);
            evaluate = value;
        }
    }

    void ChequearAgarrar() {
        bool agr = false, prt = false;
        if(Physics.Raycast(transform.position,transform.forward,out rcAgarrar, 2.5f, mascaraRaycast)){
            Transform target = rcAgarrar.transform;
            agr = FalcTools.IsGameObjectInLayerMask(target.gameObject,mascaraAgarrables);
            prt = FalcTools.IsGameObjectInLayerMask(target.gameObject,mascaraPuerta);
            if(agarrarAnterior != target)
            {
                //IntercambiarOutline(target.GetComponent<ObjetoAgarrable>());
            }
            if(Input.GetMouseButtonDown(0) && pickedUp == null)
            {
                if(agr){
                    Agarrar(target);
                }
                if(prt){
                    Abrir(target);
                }
            }
        }
        if(pickedUp != null){
            if(Input.GetMouseButtonUp(0) || puerta && Vector3.Distance(transform.position, pickedUp.position) > 3) {
                //si soltó click, suelta
                //si se agarro una puerta, y se alejó lo suficiente, suelta
                Soltar(); 
            }
        }
        if(agr || prt){
            CambiarReticula(1);
        }
        else
        {
            CambiarReticula(0);
        }
    }

    void Abrir(Transform target){
        pickedUp = target; //mouse input .x
        puerta = true;
        fpcc.enabled = false;
    }

    void Agarrar(Transform target){
        pickedUp = target;
        pickedUp.GetComponent<Rigidbody>().isKinematic = true;
        pickedUp.GetComponent<ObjetoAgarrable>().ToggleAgarrar();
        pickedUp.parent = transform;   
    }

    void Soltar(){
        pickedUp.parent = null;
        pickedUp.GetComponent<Rigidbody>().velocity = rigidBody.velocity;
        pickedUp.GetComponent<Rigidbody>().isKinematic = false;
        if(!puerta) pickedUp.GetComponent<ObjetoAgarrable>().ToggleAgarrar();
        pickedUp = null;
        puerta = false;
        fpcc.enabled = true;
    }
}
