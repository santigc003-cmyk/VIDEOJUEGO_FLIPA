using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public enum EstadoSigilo { DePie, Agachado, Escondido }
    public EstadoSigilo estadoActual;

    [Header("Movimiento")]
    public float velocidad = 5f;
    public float velocidadAgachado = 2.5f;
    public float aceleracion = 10f;
    public float friccion = 12f;
    public float gravedad = -9.81f;

    [Header("Mouse")]
    public float sensibilidadMouse = 2f;
    public Transform camara;
    public float limiteVertical = 80f;

    [Header("Sigilo")]
    public SphereCollider esferaDeteccion;
    public float radioDePie = 6f;
    public float radioAgachado = 3.5f;
    public float radioEscondido = 1.5f;

    private CharacterController controller;
    private Vector3 velocidadActual;
    private float velocidadY;
    private float rotacionX;
    private bool enZonaEscondite;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        esferaDeteccion.isTrigger = true;
        CambiarEstado(EstadoSigilo.DePie);
    }

    void Update()
    {
        Mirar();
        ControlarSigilo();
        Mover();
        ActualizarEsfera();
    }

    // =========================
    // MIRAR
    // =========================
    void Mirar()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadMouse * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadMouse * 100f * Time.deltaTime;

        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -limiteVertical, limiteVertical);

        camara.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // =========================
    // MOVIMIENTO
    // =========================
    void Mover()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        float velocidadObjetivo =
            estadoActual == EstadoSigilo.Agachado || estadoActual == EstadoSigilo.Escondido
            ? velocidadAgachado
            : velocidad;

        Vector3 direccion = (transform.right * h + transform.forward * v).normalized;

        if (direccion.magnitude > 0)
        {
            velocidadActual = Vector3.MoveTowards(
                velocidadActual,
                direccion * velocidadObjetivo,
                aceleracion * Time.deltaTime
            );
        }
        else
        {
            velocidadActual = Vector3.MoveTowards(
                velocidadActual,
                Vector3.zero,
                friccion * Time.deltaTime
            );
        }

        if (controller.isGrounded && velocidadY < 0)
            velocidadY = -2f;

        velocidadY += gravedad * Time.deltaTime;

        Vector3 movimientoFinal = velocidadActual;
        movimientoFinal.y = velocidadY;

        controller.Move(movimientoFinal * Time.deltaTime);
    }

    // =========================
    // SIGILO
    // =========================
    void ControlarSigilo()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (enZonaEscondite)
                CambiarEstado(EstadoSigilo.Escondido);
            else
                CambiarEstado(EstadoSigilo.Agachado);
        }
        else
        {
            CambiarEstado(EstadoSigilo.DePie);
        }
    }

    void CambiarEstado(EstadoSigilo nuevo)
    {
        estadoActual = nuevo;
    }

    void ActualizarEsfera()
    {
        switch (estadoActual)
        {
            case EstadoSigilo.DePie:
                esferaDeteccion.radius = radioDePie;
                break;
            case EstadoSigilo.Agachado:
                esferaDeteccion.radius = radioAgachado;
                break;
            case EstadoSigilo.Escondido:
                esferaDeteccion.radius = radioEscondido;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZonaEscondite"))
            enZonaEscondite = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ZonaEscondite"))
            enZonaEscondite = false;
    }
}
