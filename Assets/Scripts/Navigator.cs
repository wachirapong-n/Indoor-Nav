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
    [SerializeField] private GameObject coin;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject firstFloor;
    [SerializeField] private GameObject truePosition;
    [SerializeField] private GameObject predPosition;
    [SerializeField] private GameObject CSBPointCloud;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_Text TruePosition;
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

    void Start()
    {
        firstFloor.SetActive(false);
        CSBPointCloud.SetActive(false);
        coin.SetActive(false);
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
            // Vector3 playerPos = playerObject.transform.position;
            // Vector3 coinPos = coin.transform.position;
            //
            // Debug.Log($"Player Position: {playerPos}, Coin Position: {coinPos}");
            // Debug.Log(NavMesh.CalculatePath(playerObject.transform.position, coin.transform.position, NavMesh.AllAreas, path));
            //
            // NavMeshHit playerHit;
            // NavMeshHit coinHit;
            //
            // bool playerOnNavMesh = NavMesh.SamplePosition(playerObject.transform.position, out playerHit, 1.0f, NavMesh.AllAreas);
            // bool coinOnNavMesh = NavMesh.SamplePosition(coin.transform.position, out coinHit, 1.0f, NavMesh.AllAreas);
            //
            // Debug.Log($"Player on NavMesh: {playerOnNavMesh}");
            // Debug.Log($"Coin on NavMesh: {coinOnNavMesh}");

            if (spawnedPlayer != null && targetRoom != null)
            {
                if (NavMesh.CalculatePath(spawnedPlayer.transform.position, targetRoom,
                        NavMesh.AllAreas, path))
                {
                    line.positionCount = path.corners.Length;
                    line.SetPositions(path.corners);
                    line.enabled = true;
                    errorText.text = ""; 
                    errorLogged = false; 
                }
                else if (!errorLogged)
                {
                    errorText.text = $"Cant to calculate path\n{spawnedPlayer.transform.position}\n{targetRoom}"; 
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
        // playerObject.transform.position = spawnPlayer;

        float x, z, cX, cZ;
        cX = coin.transform.localPosition.x;
        cZ = coin.transform.localPosition.z;
        x = firstFloor.transform.position.x;
        z = firstFloor.transform.position.z;
        Debug.Log($"coin!: {cX}, {cZ}");

        // float[] truePosArr = GetTruePosition();
        Quaternion predictedQuat = new Quaternion(output[4], output[5], output[6], output[3]);
        // Quaternion predictedQuat = new Quaternion(truePosArr[4], truePosArr[5], truePosArr[6], truePosArr[3]);
        Quaternion currAngle = firstFloor.transform.rotation;
        Vector3 currEulerAngle = currAngle.eulerAngles;
        Vector3 predictedEuler = predictedQuat.eulerAngles;
        Vector3 rotation = new Vector3(0, predictedEuler.y + currEulerAngle.y, 0);
        Debug.Log($"predict: {rotation}");

        // Quaternion trueQuat = new Quaternion(truePosArr[4], truePosArr[5], truePosArr[6], truePosArr[3]);
        // Quaternion trueQuat = new Quaternion(output[4], output[5], output[6], output[3]);
        // Vector3 trueEuler = trueQuat.eulerAngles;
        // Vector3 trueRotation = new Vector3(0, trueEuler.y , 0);
        // Debug.Log($"True Rotate: {trueRotation}");
        // CSBPointCloud.transform.rotation = Quaternion.Euler(trueRotation);

        // Vector3 pos = new Vector3(-(x + cX), -11.6f, z+cZ);
        Vector3 pos = new Vector3(-output[0], 1, -output[2]);
        pos = Quaternion.Euler(0,rotation.y, 0 ) * pos;
        firstFloor.transform.rotation = Quaternion.Euler(rotation);
        firstFloor.transform.position = pos;

        

        // Vector3 truePos = new Vector3(-truePosArr[0], truePosArr[1], -truePosArr[2]);
        // CSBPointCloud.transform.position = truePos;
        targetRoom = dropdown.GetValueDropdown();
        spawnedPlayer = playerObject;
        SpawnPlate(output);
        firstFloor.SetActive(true);
        coin.SetActive(true);
        pathActive = true;
        StartCoroutine(UpdatePath());
        firstFloor.isStatic = true;

    }

    void SpawnPlate(float[] output)
    {
        if (DEBUG)
        {
            float[] truePosArr = GetTruePosition();
            string[] txt = { "x: ", "y: ", "z: ", "qw: ", "qx: ", "qy: ", "qz: " };
        
            string outputTxt = "True Position:\n";
            for (int i = 0; i < truePosArr.Length ; i++)
            {
                outputTxt += txt[i] + truePosArr[i] + "\n";
            }
            TruePosition.text = outputTxt;
            Vector3 truePos = new Vector3(-truePosArr[0], truePosArr[1], -truePosArr[2]);
            Instantiate(truePosition, truePos, Quaternion.identity);
        }
        
        Vector3 predPos = new Vector3(-output[0], output[1], -output[2]);
        Instantiate(predPosition, predPos, Quaternion.identity);

    }
    
    public void DisableClonedObjects()
    {
        if (line.enabled)
        {
            line.enabled = false;
        }
        
        firstFloor.transform.position = new Vector3(0, 0, 0);
        firstFloor.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

        firstFloor.SetActive(false);
        coin.SetActive(false);
        pathActive = false;
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

