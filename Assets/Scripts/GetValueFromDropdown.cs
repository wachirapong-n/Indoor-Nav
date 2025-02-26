using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GetValueFromDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private List<Target> targetRoom = new List<Target>();

    public Vector3 GetValueDropdown()
    {
        
        Vector3 targetPosition = Vector3.zero;
        
        int indx = dropdown.value;
        string selectedValue = dropdown.options[indx].text;
        Target selectedRoom = targetRoom.Find(x => x.Name.Equals(selectedValue));
        targetPosition = selectedRoom.PositionObject.transform.position;
        return targetPosition;
    }

    public GameObject GetRoomObjectDropdown()
    {
        int indx = dropdown.value;
        string selectedValue = dropdown.options[indx].text;
        Target selectedRoom = targetRoom.Find(x => x.Name.Equals(selectedValue));
        return selectedRoom.PositionObject;
    }

}
