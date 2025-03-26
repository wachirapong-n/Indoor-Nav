using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationBehaviour : MonoBehaviour
{

    private float rotationSpeed = 50f; // ความเร็วในการหมุน
    private float floatSpeed = 2f; // ความเร็วในการลอย
    private float floatHeight = 0.15f; // ความสูงของการลอยขึ้นลง
    private GameObject destination; // ตัวแปรเก็บออบเจ็กต์ปลายทาง
    private Vector3 initialPosition; // ตำแหน่งเริ่มต้นของปลายทาง
    private float timeOffset; // การเลื่อนเวลาเพื่อให้เกิดการลอยที่แตกต่างกัน

    private void Update()
    {
        // ถ้ามี destination 
        if (destination != null)
        {
            // หมุน destination รอบแกน Y
            destination.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            
            // คำนวณตำแหน่ง Y ใหม่สำหรับการลอยขึ้นลง
            float newY = initialPosition.y + Mathf.Sin(Time.time * floatSpeed + timeOffset) * floatHeight;
            
            // set ตำแหน่งใหม่ให้กับ destination
            destination.transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
        }
    }

    // set ค่าของ destination
    public void SetDestination(GameObject dest)
    {
        destination = dest;
        initialPosition = dest.transform.position; // บันทึกตำแหน่งเริ่มต้น
        timeOffset = Random.Range(0f, Mathf.PI * 2); // กำหนดการเลื่อนเวลาแบบสุ่มเพื่อให้การลอยไม่เหมือนกัน
        Activate(true); // เปิดใช้งาน destination
    }

    // เปิดหรือปิด destination
    public void Activate(bool active)
    {
        if (destination != null)
        {
            destination.SetActive(active); // กำหนดให้ปลายทางทำงานหรือไม่ทำงาน
        }
    }
}