using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgarrarObjeto : MonoBehaviour
{
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] LayerMask mascaraAgarrables;
    RaycastHit rcAgarrar;
    ObjetoAgarrable agarrarAnterior = null;
    Transform pickedUp = null;

    void Update() {

        ChequearAgarrar();
    }

    void IntercambiarOutline(ObjetoAgarrable nuevo){
        if(agarrarAnterior != null) agarrarAnterior.ToggleOutline();
        agarrarAnterior = nuevo;
        if(agarrarAnterior != null) agarrarAnterior.ToggleOutline();
    }
    
    void OnDrawGizmos() {
        Gizmos.DrawSphere(rcAgarrar.point,0.1f);
    }

    void ChequearAgarrar() {
        if(Physics.Raycast(transform.position,transform.forward,out rcAgarrar, 2.5f, mascaraAgarrables)){

            Transform target = rcAgarrar.transform;
            if(agarrarAnterior != target)
            {
                //IntercambiarOutline(target.GetComponent<ObjetoAgarrable>());
            }
            if(Input.GetMouseButtonDown(0) && target.GetComponent<ObjetoAgarrable>() && pickedUp == null) 
                Agarrar(target.GetComponent<ObjetoAgarrable>());
            if(Input.GetMouseButtonUp(0) && pickedUp != null) {
                Soltar(); 
            }
        }
        else 
        {
            //IntercambiarOutline(null);
            if(pickedUp != null && Input.GetMouseButtonUp(0)) {
                Soltar();
            }
        }
    }

    void Agarrar(ObjetoAgarrable target){
        pickedUp = target.transform;
        pickedUp.GetComponent<Rigidbody>().isKinematic = true;
        pickedUp.GetComponent<ObjetoAgarrable>().ToggleAgarrar();
        pickedUp.parent = transform;   
    }

    void Soltar(){
        pickedUp.parent = null;
        pickedUp.GetComponent<Rigidbody>().velocity = rigidBody.velocity;
        pickedUp.GetComponent<Rigidbody>().isKinematic = false;
        pickedUp.GetComponent<ObjetoAgarrable>().ToggleAgarrar();
        pickedUp = null;
    }
}
