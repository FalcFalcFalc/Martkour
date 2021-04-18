using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float speed = 50;
    [SerializeField] float airSpeed = .5f;
    float velo;
    Vector3 target;
    Vector2 inputs;

    void Start()
    {
        if(!rb) rb=GetComponent<Rigidbody>();
        pesoAnterior = rb.mass;
        camPosicionOriginal = cam.transform.localPosition;
    }

    void Update()
    {
        ChequearPiso();
        ChequearSalto();
        
        sprint = ChequearSprint();

        ChequearDeslizarse();

        ChequearLedges();
        ChequearBobbing();
    }

    void FixedUpdate()
    {
        velo = FalcTools.NullY(rb.velocity).magnitude;

        inputs.x = Input.GetAxis("Horizontal");
        inputs.y = Input.GetAxis("Vertical");

        target = (orientacion.right * inputs.x + orientacion.forward * inputs.y).normalized * speed * sprint;
        if(enElPiso)
        {
            ChequearEscaleras();
            ChequearSlope();
            target *= (!deslizandose ? (soltoElBoton ? 1 : 0.5f) : 0);   
                        // Si se está deslizando, no se agrega velocidad,
                        // si acaba de deslizarse, tiene la velocidad reducida
            cicloDePasos.Rotate(Vector3.right, velo * Mathf.Sign(inputs.y));
        }
        else
        {
            float momentumSalto = Vector3.Dot(target.normalized, rb.velocity.normalized);
            momentumSalto = FalcTools.Remap(momentumSalto,-1,1,0,1);
            momentumSalto *= momentumSalto;
            target *= momentumSalto * airSpeed;
        }
        ChequearPared();
        rb.AddForce(target, ForceMode.Acceleration);
        
        if(rb.velocity != Vector3.zero) {
            particlePivot.localRotation = Quaternion.LookRotation(rb.velocity);
        }
        float alfa = (velo - particulasThreshold);
        alfa = alfa*alfa*alfa/(velocidadParaParticulasOpacas);
        alfa = Mathf.Clamp01(alfa)/2;
        particle.startColor = new Color(particle.main.startColor.color.r,
                                        particle.main.startColor.color.g,
                                        particle.main.startColor.color.b,
                                        alfa);
        if(noPuedeAgarrarse){
            noPuedeAgarrarse = !(rb.velocity.y < 0);
        } 
    }

    [Header("Salto")]
    [SerializeField] KeyCode teclaSalto = KeyCode.Space;
    [SerializeField] float altitudDeSalto = 150;
    [SerializeField] float tiempoDeAcumulado = 0.75f;
    [Range(1,4)]
    [SerializeField] float potenciaSaltoCargado = 2f;
    [Range(0.00001f,0.99999f)]
    [SerializeField] float alturaArrodillado = 0.66666f;
    bool saltar = false;
    float fuerzaAcumulada = 0;
    float alturaAnterior;

    void Saltar(float altitud)
    {
        rb.WakeUp();
        rb.velocity += Mathf.Sqrt(-2 * Physics.gravity.y * altitud) * transform.up;
        fuerzaAcumulada = 0;
        enElPiso = false;
    }

    void Arrodillarse()
    {
        fuerzaAcumulada += Time.deltaTime;
        fuerzaAcumulada = Mathf.Clamp(fuerzaAcumulada,0,tiempoDeAcumulado);
        float fuerzaEaseada = 1 - Mathf.Abs((1-fuerzaAcumulada/tiempoDeAcumulado)*(1-fuerzaAcumulada/tiempoDeAcumulado)*(1-fuerzaAcumulada/tiempoDeAcumulado));
                                            //imaginate usar pow(x,y) kjjj
        fuerzaEaseada = FalcTools.Remap(fuerzaEaseada, 0, 1, 1, alturaArrodillado);
        transform.localScale = new Vector3(1, fuerzaEaseada, 1);
    }

    void ChequearSalto()
    {
        if(Input.GetKeyDown(teclaSalto) || Input.GetKey(teclaSalto)){
            Arrodillarse(); //carga el salto
        }
        if(Input.GetKeyUp(teclaSalto)){
            if(enElPiso){
                fuerzaAcumulada = FalcTools.Remap(fuerzaAcumulada,0,tiempoDeAcumulado,1f,potenciaSaltoCargado);
                Saltar(altitudDeSalto * fuerzaAcumulada);
            }
            LeanTween.scale(transform.gameObject,Vector3.one,.25f).setEase(LeanTweenType.easeOutBack);
        }
    }

    [Header("Trepar Bordes")]
    [SerializeField] Transform ledgeCheck;
    [SerializeField] LayerMask ledgeCheckMask;
    [SerializeField] float distanciaDeTrepado;
    [SerializeField] float ledgeCheckRadius = 0.4f;
    bool agarrado = false;
    bool noPuedeAgarrarse = false;

    void ChequearLedges()
    {
        if(agarrado){
            if(inputs.sqrMagnitude > 0){
                rb.isKinematic = false;
                Saltar(distanciaDeTrepado);
                agarrado = false;
                noPuedeAgarrarse = true;
            }
            else if(Input.GetKeyDown(KeyCode.Space))
            {
                noPuedeAgarrarse = true;
                rb.isKinematic = false;
                agarrado = false;
            }
        }
        else if(!noPuedeAgarrarse && !enElPiso)
        {
            bool puedeSubir = Physics.CheckSphere(ledgeCheck.position, ledgeCheckRadius, ledgeCheckMask, QueryTriggerInteraction.Collide);
            if(puedeSubir && !agarrado){
                rb.isKinematic = true;
                agarrado = true;
            }
        }
    }

    [Header("Animar Pasos en la Camara")]
    [SerializeField] float walkingBobbingSpeed = 14f;
    [SerializeField] float bobbingAmount = 0.05f;
    Vector3 camPosicionOriginal;
    float timer = 0;

    void ChequearBobbing()
    {
        Vector3 posicionActualizada = orientacion.right;
        if(velo == 0 || deslizandose){
            timer = 0;
        }
        else {
            timer += Time.deltaTime * walkingBobbingSpeed * velo * (enElPiso ? 1:0.1f);
            
            posicionActualizada =   transform.up        * Mathf.Sin(timer) + 
                                    orientacion.right   * Mathf.Cos(timer / 2);
        }
        cam.transform.localPosition = camPosicionOriginal + posicionActualizada * velo * bobbingAmount;
    }

    [Header("Deslizarse")]
    [SerializeField] KeyCode teclaDeslizarse = KeyCode.LeftControl;
    [SerializeField] float pesoTarget = 3;
    [SerializeField] float alturaDeslizando = .375f;
    [SerializeField] float velocidadMinima = 5;
    [SerializeField] float conversionVelocidad = .3f;
    [SerializeField] Vector2 pesoDirecciones = new Vector2(1,.5f);
    [SerializeField] float recoverySpeed = 2;    
    [SerializeField] float dragDeslizarse = 3;
    [SerializeField] float delayDez = .25f;
    Coroutine cancelarButtonPress;
    float pesoAnterior;
    bool deslizandose = false, soltoElBoton = true, slidePrompt = false;

    void ChequearDeslizarse()
    {
        if(Input.GetKeyDown(teclaDeslizarse)){
            slidePrompt = true; // si se preciona C, el personaje intentará deslizarse
        }
        if(Input.GetKeyUp(teclaDeslizarse)){
            slidePrompt = false;
            if(cancelarButtonPress == null) cancelarButtonPress = StartCoroutine(DelayDeslizamiento());
            TerminarSlide();
        }
        else if(velo < recoverySpeed && deslizandose && !enSlope){
            if(cancelarButtonPress != null) StopCoroutine(cancelarButtonPress);
            soltoElBoton = true;
            cancelarButtonPress = null;
            TerminarSlide();
        }
        if(slidePrompt && (velo >= velocidadMinima || enSlope) && enElPiso && !deslizandose && soltoElBoton){
            // el personaje va a deslizarse si está en el piso, no se está deslizando todavia,
            // la primera vez que haya tocado el botón y si la velocidad es elevada o está sobre una superficie inclinada
            LeanTween.scale(transform.gameObject,new Vector3(1,alturaDeslizando,1),.25f).setEase(LeanTweenType.easeOutBack);
            deslizandose = true;
            slidePrompt = false;
            soltoElBoton = false;
            pesoAnterior = rb.mass;
            rb.mass = pesoTarget;
            Vector3 dir =   pesoDirecciones.x * (FalcTools.NullY(rb.velocity)) +
                            pesoDirecciones.y * (FalcTools.NullY(orientacion.forward));
            rb.AddForce(dir.normalized/2 * (FalcTools.NullY(rb.velocity)+Vector3.up * rb.velocity.y/2).sqrMagnitude * conversionVelocidad,ForceMode.Impulse);
        }
    }

    void TerminarSlide()
    {
            LeanTween.scale(transform.gameObject,Vector3.one,.25f).setEase(LeanTweenType.easeOutBack);
            deslizandose = false;
            rb.mass = pesoAnterior;
            tiempoCorriendo = 0;
    }

    IEnumerator DelayDeslizamiento()
    {
        yield return new WaitForSeconds(delayDez);
        soltoElBoton = true;
        cancelarButtonPress = null;
    }

    [Header("Escaleras")]
    [SerializeField] Transform checkContraPared;   
    [SerializeField] LayerMask escalerasCheckMask; 
    [SerializeField] float stepHeight;    
    RaycastHit escalerasRayHit1,escalerasRayHit2;

    void ChequearEscaleras()
    {
        if(Physics.Raycast(checkContraPared.position,orientacion.forward,out escalerasRayHit1,.75f,escalerasCheckMask) && !deslizandose){
            if(!Physics.Raycast(checkContraPared.position + Vector3.up * stepHeight,orientacion.forward,out escalerasRayHit2,.25f,escalerasCheckMask) && inputs.y > 0){
                target += (3 * transform.up + 2 * orientacion.forward) * speed/5f;
            }
        }
    }

    [Header("Piso Inclinado")]
    [SerializeField] LayerMask slopeMask;
    RaycastHit slopeDetection;
    Vector3 movimientoEnSlope;
    bool enSlope = false;
    Coroutine dormir;

    void ChequearSlope()
    {
        enSlope = (Physics.Raycast(transform.position, Vector3.down, out slopeDetection,2,slopeMask) && slopeDetection.normal !=Vector3.up);
        if(enSlope){
            if(inputs.sqrMagnitude == 0 && !deslizandose){
                /*if(rb.IsSleeping()){
                    rb.Sleep();
                }
                else if(dormir == null)
                {
                    dormir = StartCoroutine(DormirRB());
                }*/
            }
            else
            {
                /*if(dormir != null) {
                    StopCoroutine(dormir);
                    dormir = null;
                }
                
                //NO FUNCA
                rb.WakeUp();*/
                target = Vector3.ProjectOnPlane(target, slopeDetection.normal) + target.normalized * 0;
            }
        }
    }

    IEnumerator DormirRB(){
        yield return new WaitForSeconds(0.1f);

        rb.Sleep();
        dormir = null;
    }

    [Header("Reducir Friccion de Paredes")]
    [SerializeField] LayerMask maskParedes;
    bool tocandoLaPared = false;
    Vector3 normalPared;
    int contactosPared = 0;

    void OnCollisionEnter(Collision other) {
        if(FalcTools.IsGameObjectInLayerMask(other.gameObject, maskParedes)){
            contactosPared++;
            ActualizarVectorPared(other);
        }
    }

    void ActualizarVectorPared(Collision colision){
        int cantContactos = colision.contacts.Length;
        if(cantContactos > 0){
            if(contactosPared == 1){
                normalPared = colision.GetContact(0).normal;
            }
            else if(contactosPared > 1)
            {
                //conseguir un vector "normal" para que la salida sea un vector que:
                        // para todas las paredes: dot(v, p[i].n) > 0;
                normalPared = Vector3.zero;
                for (int i = 0; i < cantContactos; i++)
                {
                    normalPared += colision.GetContact(0).normal;
                }
                normalPared = normalPared.normalized;
            }
        }        
    }

    void OnCollisionExit(Collision other) {
        if(FalcTools.IsGameObjectInLayerMask(other.gameObject, maskParedes)){
            contactosPared--;
            ActualizarVectorPared(other);
        }
    }

    void ChequearPared(){
        if(contactosPared > 0 && Vector3.Dot(target,normalPared) <= 0 && normalPared != Vector3.zero){ 
            //si es que se está tocando la pared, y la normal de la pared es contraria a la direccion de movimiento:
            target = Vector3.ProjectOnPlane(target,normalPared);
        }
    }

    [Header("Correr")]
    [SerializeField] KeyCode teclaCorrer = KeyCode.LeftShift;
    [SerializeField] float sprintMultiplier = 3;
    [SerializeField] float tiempoParaFullSprint = 3;
    [SerializeField] ParticleSystem particle;
    [SerializeField] Transform particlePivot;    
    [SerializeField] float particulasThreshold;
    [SerializeField] float velocidadParaParticulasOpacas;
    float sprint;
    float tiempoCorriendo = 0;

    float ChequearSprint()
    {
        if(enElPiso){
            if((Input.GetKey(teclaCorrer)) && velo > 0 && Vector3.Dot(rb.velocity,orientacion.forward) > .6f && !deslizandose){
                //si esta shifteando y en el piso, empieza a aumentar la velocidad
                tiempoCorriendo += Time.deltaTime;
            }
            else
            {
                //si no esta shifteando y esta en el piso, baja la velocidad
                tiempoCorriendo -= Time.deltaTime * 5f;
            }
        }
        //sin importar si esta o no shifteando y está en el aire, la velocidad no sube ni baja

        tiempoCorriendo = Mathf.Clamp(tiempoCorriendo,0,tiempoParaFullSprint);
       
        return FalcTools.Remap(tiempoCorriendo, 0, tiempoParaFullSprint, 1, sprintMultiplier);
    }

    [Header("Piso")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundCheckMask;
    [SerializeField] float groundCheckRadius = 0.4f;
    [SerializeField] bool enElPiso;
    [SerializeField] float groundDrag = 6;
    [SerializeField] float airDrag = 2;
    Coroutine cancelarCoyote ;

    void ChequearPiso()
    {
        if(Physics.CheckSphere(groundCheck.position,groundCheckRadius,groundCheckMask)){
            if(cancelarCoyote != null)  StopCoroutine(cancelarCoyote);
            cancelarCoyote = null;
            enElPiso = true; //entonces esta en el piso
        }
        else
        {
            if(cancelarCoyote == null) cancelarCoyote = StartCoroutine(CoyoteTime()); //si no, hagamos le coyotemite
        }
        rb.drag = (enElPiso ? (deslizandose ? dragDeslizarse : groundDrag) : airDrag);
    }

    IEnumerator CoyoteTime()
    {
        yield return new WaitForSeconds(0.1f);
        enElPiso = false;
    }    

    [Header("Dependencias")]
    [SerializeField] Transform cam;
    Rigidbody rb;
    [SerializeField] Transform orientacion;
    [SerializeField] Transform cicloDePasos;

    void OnDrawGizmos()
    {
        //Debug Trepar
        Gizmos.color = !agarrado ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(ledgeCheck.position,ledgeCheckRadius);
        //Debug Pisar
        Gizmos.color = !enElPiso ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position,groundCheckRadius);
        //Debug Pasos
        Gizmos.color = Color.white;
        foreach (Transform item in cicloDePasos)
        {
            Gizmos.DrawSphere(item.position,0.1f);
        }
        //Debug Velocidad
        if (rb) Gizmos.DrawRay(transform.position,rb.velocity);
        //Debug Direccion intendida
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position,target);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(slopeDetection.point, slopeDetection.normal);
        Gizmos.color = Color.magenta;
        //Debug Escalera
        if (escalerasRayHit1.transform) Gizmos.DrawLine(checkContraPared.position,escalerasRayHit1.point);
        if (escalerasRayHit2.transform) Gizmos.DrawLine(checkContraPared.position + Vector3.up * stepHeight, escalerasRayHit2.point);
    }
}
