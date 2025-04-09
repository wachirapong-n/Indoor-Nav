using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR.OpenXR.NativeTypes;

public class Navigator : MonoBehaviour
{

    [SerializeField] private GameObject csb; // model CSB
    [SerializeField] private TMP_Text errorText; // ข้อความ error ของ mainCamera กับ targetRoom
    [SerializeField] private TMP_Text distanceText; // ข้อความแสดงระยะทางปัจจุบัน
    [SerializeField] private GameObject popupNotification; // popup แจ้งเตือน
    [SerializeField] private Transform mainCamera;
    [SerializeField] private GetValueFromDropdown dropdown; 
    [SerializeField] private DestinationBehaviour destination; 

    private LineRenderer line; // ตัวแสดงเส้นทาง
    private Vector3 targetRoom; // ห้องที่ต้องการไป
    private bool pathActive; // สถานะของเส้นทางที่กำลังคำนวณ

    void Start()
    {
        pathActive = false;
        csb.SetActive(false); // set ให้ model CSB ล่องหน
        line = transform.GetComponent<LineRenderer>();
        line.enabled = false;
    }

    private IEnumerator UpdatePath()
    {
        // กำหนดเวลาในการ update path
        WaitForSeconds wait = new WaitForSeconds(0.25f);
        NavMeshPath path = new NavMeshPath();
        bool errorLogged = false;
        float yOffset = 0.1f; // ปรับแกน Y ของ path ให้สูงขึ้น 

        // loop ในขณะที่ pathActive == true
        while (pathActive)
        {
            // คำนวณ ตำแหน่ง mainCamera ไปยังห้องที่เลือก
            if (NavMesh.CalculatePath(mainCamera.position, targetRoom, NavMesh.AllAreas, path))
            {
                float distance = Vector3.Distance(mainCamera.position, targetRoom);
            
                // แก้ไขมุมของ path
                Vector3[] modifiedCorners = new Vector3[path.corners.Length];
                for (int i = 0; i < path.corners.Length; i++)
                {
                    modifiedCorners[i] = path.corners[i] + new Vector3(0, yOffset, 0); 
                }

                // แสดง path ใน LineRenderer
                line.positionCount = modifiedCorners.Length;
                line.SetPositions(modifiedCorners);
                line.enabled = true;
                
                // set ค่า error ให้เป็น ว่าง เพราะไม่เกิดการ error
                errorText.text = ""; 
                errorLogged = false; 

                // แสดงระยะทางของปัจจุบัน
                distanceText.text = $"Distance: {distance:F2} m";
            
                // ถ้า mainCamera ห่างจากห้องที่เลือกไม่ถึง 6 เมตรแสดงข้อความแจ้งเตือน
                if (distance < 6f)
                {
                    ShowPopup("You're almost there! \nJust a few more steps to go.");
                }
                else
                {
                    HidePopup();
                }
            }
            // ถ้าคำนวณเส้นทางไม่ได้ แสดง error
            else if (!errorLogged)
            {
                errorText.text = $"Can't calculate path\n{mainCamera.position}\n{targetRoom}"; 
                errorLogged = true;
            }
        
            yield return wait;
        }
        // pathActive เป็น false
        // reset ทุุกค่าให้เป็นค่าเริ่มต้น
        line.enabled = false;
        errorText.text = "";
        distanceText.text = "";
    }

    public void Spawn(float[] output)
    {
        // แปลง Quaternion ของ prediction ให้เป็น euler angle
        Quaternion predictedQuat = new Quaternion(output[4], output[5], output[6], output[3]);
        Vector3 predictedEuler = predictedQuat.eulerAngles;
 
        // แปลง Quaternion ของ model CSB ให้เป็น euler angle
        Quaternion currAngle = csb.transform.rotation;
        Vector3 currEulerAngle = currAngle.eulerAngles;
        
        // แปลง Quaternion ของ mainCamera ให้เป็น euler angle
        Quaternion playerAngle = mainCamera.rotation;
        Vector3 playerEulerAngle = playerAngle.eulerAngles;
        
        // นำ euler angle ของแต่ละอันมารวมกัน
        Vector3 rotation = new Vector3(0, predictedEuler.y + currEulerAngle.y + playerEulerAngle.y, 0);
        
        // แปลง output ของ prediction ให้เป็น position
        Debug.Log($"{output[0]} {output[2]}");
        Vector3 pos = new Vector3(
            output[0],
            output[1],
            output[2]);

        
        // fixed ค่า postion ของ model CSB สำหรับทดสอบระบบนำทางเฉยๆโดยไม่มี prediction มาเกี่ยวข้อง
        // Vector3 pos = new Vector3(
        //     -7f,
        //     -1.131562f,
        //     -2f);
        
        
        // position ใหม่หลังจากมีค่า rotation มาเกี่ยวข้อง
        pos = Quaternion.Euler(0,rotation.y, 0 ) * pos;

        // set position, rotation ของ model CSB
        csb.transform.position += pos;
        csb.transform.rotation = Quaternion.Euler(rotation);
        
        // set ค่า position ของห้องที่เลือกจะไป
        targetRoom = dropdown.GetValueDropdown();

        // set model CSB ให้ทำงาน
        csb.SetActive(true);
        
        // set GameObject ของห้องทุกห้องให้ล่องหน
        DeactivateAllRooms();
        
        // set ให้ห้องที่เลือกจะไป ทำงาน และ set ให้ขยับได้
        GameObject destRoom = dropdown.GetRoomObjectDropdown();
        destination.SetDestination(destRoom);
        destination.Activate(true);
        
        // เปิดใช้งาน path ในการนำทาง
        pathActive = true;
        StartCoroutine(UpdatePath());

    }
    
    
    // แสดง popup ข้อความ
    private void ShowPopup(string message)
    {
        if (popupNotification != null)
        {
            popupNotification.SetActive(true);
            popupNotification.GetComponentInChildren<TMP_Text>().text = message; 
        }
    }

    // ซ่อน popup
    private void HidePopup()
    {
        if (popupNotification != null)
        {
            popupNotification.SetActive(false);
        }
    }

    // ซ่อน GameObject ของห้องทั้งหมด
    public void DeactivateAllRooms()
    {
        Transform allFloors = csb.transform.Find("AllFloors");
        foreach (Transform floor in allFloors.transform)
        {
            Transform rooms = floor.Find("Rooms");

            foreach (Transform room in rooms) 
            {
                room.gameObject.SetActive(false);
            }
        }
    }

    // reset ให้ทุกค่ากลับไปเป็นค่าเดิม เมื่อกดปุ่ม "Reset" บนหน้าจอ
    public void Reset()
    {
        csb.transform.position = new Vector3(0, 0, 0);
        // csb.transform.rotation = Quaternion.Euler(new Vector3(0, -85, 0));
        csb.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        line.enabled = false;
        pathActive = false;
        csb.SetActive(false);
        destination.Activate(false);
    }

    // จบการทำงานของโปรแกรม โดย reset และแสดงข้อความ เมื่อกดปุ่ม "Finish" บนหน้าจอ
    public void Finish()
    {
        // เช็คว่า ระยะห่างของ mainCamera กับห้องเป่้าหมาย ไม่ถึง 3เมตร
        // ถ้าเกิน 3เมตร จะกดปุ่ม "Finish" บนหน้าจอแล้วไม่เกิดอะไรขึ้น
        float distance = Vector3.Distance(mainCamera.position, targetRoom);
        if (distance < 3f)
        {
            Reset();
            ShowPopup("Great job! You've successfully reached your destination.");
            // ปิด popup เมื่อเวลาผ่านไป 5 วินาที
            StartCoroutine(HidePopupAfterDelay(5f));
        }
    }

    // ปิด popup เมื่อเวลาผ่านไป delay วินาที
    private IEnumerator HidePopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        popupNotification.SetActive(false);
    }
}
