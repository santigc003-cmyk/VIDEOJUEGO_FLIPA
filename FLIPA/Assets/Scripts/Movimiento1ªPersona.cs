using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSControllerFluido : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float aceleracion = 10f;
    public float friccion = 12f;
    public float gravedad = -9.81f;

    [Header("Mouse")]
    public float sensibilidadMouse = 2f;
    public Transform camara;
    public float limiteVertical = 80f;

    private CharacterController controller;
    private Vector3 velocidadActual;
    private float velocidadY;
    private float rotacionX;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Mirar();
        Mover();
    }

    void Mirar()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadMouse * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadMouse * 100f * Time.deltaTime;

        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -limiteVertical, limiteVertical);

        camara.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Mover()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 direccion = (transform.right * h + transform.forward * v).normalized;

        if (direccion.magnitude > 0)
        {
            // Aceleramos mientras hay input
            velocidadActual = Vector3.MoveTowards(
                velocidadActual,
                direccion * velocidad,
                aceleracion * Time.deltaTime
            );
        }
        else
        {
            // Frenado natural (inercia corta)
            velocidadActual = Vector3.MoveTowards(
                velocidadActual,
                Vector3.zero,
                friccion * Time.deltaTime
            );
        }

        // Gravedad
        if (controller.isGrounded && velocidadY < 0)
            velocidadY = -2f;

        velocidadY += gravedad * Time.deltaTime;

        Vector3 movimientoFinal = velocidadActual;
        movimientoFinal.y = velocidadY;

        controller.Move(movimientoFinal * Time.deltaTime);
    }
}
