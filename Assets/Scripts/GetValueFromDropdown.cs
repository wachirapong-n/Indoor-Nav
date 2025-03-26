using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class GetValueFromDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown; // dropdown ที่ให้ผู้ใช้เลือกห้อง
    [SerializeField] private List<Target> targetRoom = new List<Target>(); // รายการห้องทั้งหมดที่เลือกได้
    
    // สำหรับดึงค่า position ของห้องที่ถูกเลือก
    // เพื่อใช้ในการคำนวณเส้นทาง
    public Vector3 GetValueDropdown()
    {
        Vector3 targetPosition = Vector3.zero; 
        int indx = dropdown.value;
        string selectedValue = dropdown.options[indx].text; 

        Target selectedRoom = targetRoom.Find(x => x.Name.Equals(selectedValue));
        targetPosition = selectedRoom.PositionObject.transform.position;
        return targetPosition;
    }

    // สำหรับดึง GameObject ของห้องที่ถูกเลือก
    // เพื่อใช้ในการทำให้ gameObject ของห้องที่ถูกเลือกขยับได้
    public GameObject GetRoomObjectDropdown()
    {
        int indx = dropdown.value;
        string selectedValue = dropdown.options[indx].text;

        Target selectedRoom = targetRoom.Find(x => x.Name.Equals(selectedValue));
        return selectedRoom.PositionObject;
    }


}