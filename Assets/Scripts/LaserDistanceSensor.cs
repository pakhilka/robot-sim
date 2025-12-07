using UnityEngine;

public class LaserDistanceSensor : MonoBehaviour
{
    [Tooltip("Максимальная дистанция проверки вперёд")]
    public float maxDistance = 20f;

    [Tooltip("Слои, которые считаем препятствиями")]
    public LayerMask obstacleMask;

    /// <summary>
    /// Возвращает расстояние до ближайшего препятствия впереди.
    /// Если ничего не нашли – возвращаем maxDistance.
    /// </summary>
    public float GetDistance()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, obstacleMask))
        {
            return hit.distance;
        }

        return maxDistance;
    }

    // Для наглядности – рисуем луч в редакторе
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        Gizmos.DrawLine(origin, origin + direction * maxDistance);
    }
}
