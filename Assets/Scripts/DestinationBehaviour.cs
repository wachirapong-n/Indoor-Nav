using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationBehaviour : MonoBehaviour
{
    private float rotationSpeed = 50f;
    private float floatSpeed = 2f;
    private float floatHeight = 0.15f; 
    private GameObject destination;
    private Vector3 initialPosition;
    private float timeOffset;
    private void Update()
    {
        if (destination != null) 
        {
            destination.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            float newY = initialPosition.y + Mathf.Sin(Time.time * floatSpeed + timeOffset) * floatHeight;
            destination.transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
        }
    }

    public void SetDestination(GameObject dest)
    {
        destination = dest;
        initialPosition = dest.transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2); 
        Activate(true);
    }

    public void Activate(bool active)
    {
        if (destination != null)
        {
            destination.SetActive(active);
        }
    }
}