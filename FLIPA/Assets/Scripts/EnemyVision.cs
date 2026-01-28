using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Movimiento")]
    public NavMeshAgent agent;
    public float velocidadPatrulla = 3.5f;
    public float velocidadPersecucion = 7f;

    [Header("Patrulla")]
    public Transform[] puntosPatrulla;
    private int indiceActual;

    [Header("Vision")]
    public Transform ojos;
    public LayerMask capasObstaculos;

    [Header("Distancias")]
    public float distanciaActualizacionDestino = 0.5f; // distancia mínima para actualizar destino
    public float radioNavMesh = 1.5f; // radio para buscar punto alcanzable

    private Transform jugador;
    private bool persiguiendo;
    private Vector3 destinoActual;

    void Start()
    {
        agent.speed = velocidadPatrulla;
        agent.stoppingDistance = 0.5f;
        agent.autoBraking = true;
        agent.autoRepath = true;

        IrSiguientePunto();
    }

    void Update()
    {
        if (persiguiendo && jugador != null)
        {
            // Buscamos un punto alcanzable cerca del jugador
            Vector3 puntoSeguro = ObtenerPuntoNavMeshCercano(jugador.position);

            // Solo actualizamos si cambia suficiente distancia
            if (Vector3.Distance(destinoActual, puntoSeguro) > distanciaActualizacionDestino)
            {
                destinoActual = puntoSeguro;
                agent.SetDestination(destinoActual);
            }

            return;
        }

        // Patrulla normal
        if (!agent.pathPending && agent.remainingDistance <= 0.4f)
        {
            IrSiguientePunto();
        }
    }

    void IrSiguientePunto()
    {
        if (puntosPatrulla == null || puntosPatrulla.Length == 0)
            return;

        agent.SetDestination(puntosPatrulla[indiceActual].position);
        indiceActual = (indiceActual + 1) % puntosPatrulla.Length;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerDetection"))
            return;

        Transform posibleJugador = other.transform.root;

        if (TieneLineaDeVision(posibleJugador))
        {
            jugador = posibleJugador;
            persiguiendo = true;
            agent.speed = velocidadPersecucion;
        }
    }

    bool TieneLineaDeVision(Transform objetivo)
    {
        Vector3 origen = ojos.position;
        Vector3 destino = objetivo.position + Vector3.up * 1.5f;

        if (Physics.Linecast(origen, destino, out RaycastHit hit, capasObstaculos))
        {
            if (!hit.transform.CompareTag("Player"))
                return false;
        }

        return true;
    }

    Vector3 ObtenerPuntoNavMeshCercano(Vector3 objetivo)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(objetivo, out hit, radioNavMesh, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // Si falla, el agente se queda donde está
        return transform.position;
    }
}


