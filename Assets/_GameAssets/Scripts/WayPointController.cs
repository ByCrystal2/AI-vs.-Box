using System.Collections.Generic;
using UnityEngine;

public class WayPointController : MonoBehaviour
{
    [SerializeField] List<Transform> wayPoints = new List<Transform>();
    public static WayPointController instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        FillPoints();
    }
    public Transform GetRandomWayPoint()
    {
        return wayPoints[Random.Range(0, wayPoints.Count)];
    }
    void FillPoints()
    {
        int length = transform.childCount;
        for (int i = 0; i < length; i++)
        {
            Transform childPoint = transform.GetChild(i);
            wayPoints.Add(childPoint);
        }
    }
}
