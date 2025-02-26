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

    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject csb;
    [SerializeField] private GameObject predPosition;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private GameObject popupNotification;
    [SerializeField] private Transform XROringin;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private GetValueFromDropdown dropdown;
    [SerializeField] private DestinationBehaviour destination;
    private string targetImage = "seq/"+ MainManager.testImg +".jpg";
    private string version = MainManager.version;
    private LineRenderer line;
    private Vector3 targetRoom;
    private GameObject spawnedPlayer;
    private bool pathActive = false;
    private int count = 0;
    void Start()
    {
        csb.SetActive(false);
        playerObject.GetComponent<Renderer>().enabled = false;
        line = transform.GetComponent<LineRenderer>();
        line.enabled = false;

    }
    private IEnumerator UpdatePath()
    {
        WaitForSeconds wait = new WaitForSeconds(0.25f);
        NavMeshPath path = new NavMeshPath();
        bool errorLogged = false;
    
        while (pathActive)
        {
            if (spawnedPlayer != null && targetRoom != null)
            {
                if (NavMesh.CalculatePath(spawnedPlayer.transform.position, targetRoom, NavMesh.AllAreas, path))
                {
                    
                    float distance = Vector3.Distance(spawnedPlayer.transform.position, targetRoom);
                    line.positionCount = path.corners.Length;
                    line.SetPositions(path.corners);
                    line.enabled = true;
                    errorText.text = ""; 
                    errorLogged = false; 
                    distanceText.text = $"Distance: {distance:F2} m";
                    
                    if (distance < 6f)
                    {
                        ShowPopup("You're almost there! \nJust a few more steps to go.");
                    }
                    else
                    {
                        HidePopup();
                    }
                }
                else if (!errorLogged)
                {
                    errorText.text = $"Can't calculate path\n{spawnedPlayer.transform.position}\n{targetRoom}"; 
                    errorLogged = true;
                }
            }
            yield return wait;
        }
    
        line.enabled = false;
        errorText.text = "";
        distanceText.text = "";
    }

    
    public void Spawn(float[] output)
    {
        if (count != 0)
        {
            csb.transform.position = new Vector3(0, 0, 0);
            csb.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        
        float y = csb.transform.position.y;

        Quaternion predictedQuat = new Quaternion(output[4], output[5], output[6], output[3]);
        Quaternion currAngle = csb.transform.rotation;
        Quaternion playerAngle = mainCamera.rotation;
        Vector3 playerEulerAngle = playerAngle.eulerAngles;
        Vector3 currEulerAngle = currAngle.eulerAngles;
        Vector3 predictedEuler = predictedQuat.eulerAngles;
        Vector3 rotation = new Vector3(0, predictedEuler.y + currEulerAngle.y + playerEulerAngle.y, 0);
        
        Vector3 pos = new Vector3(-output[0], 0, -output[2]);
        pos = Quaternion.Euler(0,rotation.y, 0 ) * pos;
        csb.transform.rotation = Quaternion.Euler(rotation);
        csb.transform.position = pos;
        csb.transform.position += new Vector3(0, y - 7.7f, 0);
        csb.transform.position += new Vector3(mainCamera.transform.position.x,
            mainCamera.transform.position.y - 0.25f,
            mainCamera.transform.position.z);
        
        csb.transform.rotation = Quaternion.Euler(rotation);
        
        targetRoom = dropdown.GetValueDropdown();
        spawnedPlayer = playerObject;
        
        csb.SetActive(true);
        DeactivateAllRooms();
        
        GameObject destRoom = dropdown.GetRoomObjectDropdown();
        destination.SetDestination(destRoom);
        destination.Activate(true);
        
        pathActive = true;
        
        StartCoroutine(UpdatePath());
        
        count++;

    }
    
    private float[] GetTruePosition()
    {
        float[] truePosArr = new float[7];
        float x=0, y=0, z=0, qx=0, qy=0, qz=0, qw=0;
        bool found = false;
        bool isShow = false;
        string[] lines = File.ReadAllLines("Assets/Resources/train_csbF3_crop_10_" + version + ".txt");
        foreach (var li in lines)
        {
            if (li.StartsWith(targetImage))
            {
                string[] parts = li.Split(' ');
                if (parts.Length >= 8)
                {
                    if (!isShow)
                    {
                        Debug.Log(parts[0]);
                        isShow = true;
                    }
                    x = float.Parse(parts[1]);
                    y = float.Parse(parts[2]);
                    z = float.Parse(parts[3]);
                    qw = float.Parse(parts[4]);
                    qx = float.Parse(parts[5]);
                    qy = float.Parse(parts[6]);
                    qz = float.Parse(parts[7]);
                    found = true;
                    break;
                }
            }
    
        }
        if (found)
        {
            truePosArr[0] = x;
            truePosArr[1] = y;
            truePosArr[2] = z;
            truePosArr[3] = qw;
            truePosArr[4] = qx;
            truePosArr[5] = qy;
            truePosArr[6] = qz;
        }
        else
        {
            Debug.LogWarning($"Image '{targetImage}' not found in the file.");
        }
        
        return truePosArr;
    }
    
    private void ShowPopup(string message)
    {
        if (popupNotification != null)
        {
            popupNotification.SetActive(true);
            popupNotification.GetComponentInChildren<TMP_Text>().text = message; 

        }
    }

    private void HidePopup()
    {
        if (popupNotification != null)
        {
            popupNotification.SetActive(false);
        }
    }
    
    public void DeactivateAllRooms()
    {
        if (csb == null)
        {
            Debug.LogError("Building is not assigned!");
            return;
        }
        Transform allFloors = csb.transform.Find("AllFloors");
        foreach (Transform floor in allFloors.transform)
        {
            Transform rooms = floor.Find("Rooms"); 
            if (rooms != null)
            {
                foreach (Transform room in rooms) 
                {
                    room.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning($"Rooms not found under {floor.name}");
            }
        }
    }

}



