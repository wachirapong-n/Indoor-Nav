using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine.UIElements;
using UnityEngine.XR.OpenXR.NativeTypes;

public class Navigator : MonoBehaviour
{

    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject firstFloor;
    [SerializeField] private GameObject predPosition;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private Transform XROringin;
    [SerializeField] private GetValueFromDropdown dropdown;
    private string targetImage = "seq/"+ MainManager.testImg +".jpg";

    private string version = MainManager.version;
    private LineRenderer line;
    private Vector3 targetRoom;
    private GameObject spawnedPlayer;
    private bool pathActive = false;
    private bool DEBUG = MainManager.DEBUG;
    private Vector3 degree;
    private int count = 0;
    void Start()
    {
        firstFloor.SetActive(false);
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
                    Vector3[] adjustedCorners = new Vector3[path.corners.Length];

                    for (int i = 0; i < path.corners.Length; i++)
                    {
                        adjustedCorners[i] = path.corners[i] + new Vector3(0, 0.2f, 0);
                    }

                    line.positionCount = adjustedCorners.Length;
                    line.SetPositions(adjustedCorners);
                    line.enabled = true;
                    errorText.text = ""; 
                    errorLogged = false; 
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
    }


    public void Spawn(float[] output)
    {
        if (count != 0)
        {
            firstFloor.transform.position = new Vector3(0, -12.7f, 0);
            firstFloor.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }

        Quaternion predictedQuat = new Quaternion(output[4], output[5], output[6], output[3]);
        Quaternion currAngle = firstFloor.transform.rotation;
        Vector3 currEulerAngle = currAngle.eulerAngles;
        Vector3 predictedEuler = predictedQuat.eulerAngles;
        Vector3 rotation = new Vector3(0, predictedEuler.y + currEulerAngle.y, 0);
        
        Vector3 pos = new Vector3(-output[0], 1, -output[2]);
        pos = Quaternion.Euler(0,rotation.y, 0 ) * pos;
        firstFloor.transform.rotation = Quaternion.Euler(rotation);
        firstFloor.transform.position += pos;
        firstFloor.transform.position += new Vector3(0, -1.0f, 0);
        
        targetRoom = dropdown.GetValueDropdown();
        spawnedPlayer = playerObject;
        // SpawnPlate(output);
        firstFloor.SetActive(true);

        pathActive = true;
        StartCoroutine(UpdatePath());
        count++;

    }
    void SpawnPlate(float[] output)
    {
        
        Vector3 predPos = new Vector3(-output[0], output[1], -output[2]);
        Instantiate(predPosition, predPos, Quaternion.identity);

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
    

}



