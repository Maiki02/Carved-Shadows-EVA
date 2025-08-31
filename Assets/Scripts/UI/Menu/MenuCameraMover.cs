using UnityEngine;

public class MenuCameraMover : MonoBehaviour
{
    [Tooltip("Lista de puntos a recorrer en secuencia")]
    public Transform[] points;
    [Tooltip("Velocidad de movimiento en unidades/segundo")]
    public float speed = 0.5f;

    private int currentIndex = 0;

    void Start()
    {
        if (points != null && points.Length > 0)
            transform.position = points[0].position;
    }

    void Update()
    {
        if (points == null || points.Length == 0) return;

        // Mueve hacia el punto actual
        Vector3 target = points[currentIndex].position;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Si llegamos, avanzamos al siguiente (con wrap-around)
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            currentIndex = (currentIndex + 1) % points.Length;
        }
    }
}
