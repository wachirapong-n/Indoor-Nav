using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GetValueFromDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown2ndFloor;
    [SerializeField] private TMP_Dropdown dropdown3rdFloor;
    [SerializeField] private List<Target> targetRoom2nd = new List<Target>();
    [SerializeField] private List<Target> targetRoom3rd = new List<Target>();
    public Vector3 GetValueDropdown()
    {
        Debug.LogError("5678");

        Vector3 targetPosition = Vector3.zero;
        
        int indx = dropdown2ndFloor.value;
        string selectedValue = dropdown2ndFloor.options[indx].text;
        Target selectedRoom = targetRoom2nd.Find(x => x.Name.Equals(selectedValue));
        if (selectedRoom == null)
        {
            indx = dropdown3rdFloor.value;
            selectedValue = dropdown3rdFloor.options[indx].text;
            selectedRoom = targetRoom3rd.Find(x => x.Name.Equals(selectedValue));
        }
        targetPosition = selectedRoom.PositionObject.transform.position;
        return targetPosition;
    }
    
}
