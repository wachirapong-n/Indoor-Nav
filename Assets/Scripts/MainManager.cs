using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    // [SerializeField] private TMP_Text inputValue;
    [SerializeField] private TMP_Text outputValue;
    [SerializeField] private NNModel myModel;
    // [SerializeField] private GameObject truePosition;
    [SerializeField] private GameObject predPosition;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Camera ARcamera;
    [SerializeField] private GameObject firstFloor;
    
    // private int _lengthInput = 10;
    private Model _runtimeModel;
    private IWorker _worker;
    private string _output;
    
    // Start is called before the first frame update
    void Start()
    {
        _runtimeModel = ModelLoader.Load(myModel);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _runtimeModel);
        var count = _runtimeModel.outputs.Count;
        _output = _runtimeModel.outputs[count - 1];
    }

    public void Predict()
    {
        
 
        int width = Screen.width;
        int height = Screen.height;
        RenderTexture rt = new RenderTexture(width, height, 24);
        ARcamera.targetTexture = rt;
        var currentRT = RenderTexture.active;
        RenderTexture.active = ARcamera.targetTexture;

        ARcamera.Render();

        Texture2D image = new Texture2D(width, height);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        RenderTexture.active = currentRT;
        Texture2D resizedImg = Resize(image, 256, 256);
        Texture2D croppedImg = ImageCropping(resizedImg, 224, 224);
        
        
        // byte[] fileData = File.ReadAllBytes("Assets/Temp/demo.jpg");
        // Texture2D image = new Texture2D(224, 224);
        // image.LoadImage(fileData);
        
        float[] tensorImg = ConvertImagesToTensor(croppedImg);
        using var tensor = new Tensor(1, 224, 224, 3, tensorImg);
        _worker.Execute(tensor);
        
        Tensor outputTensor = _worker.PeekOutput(_output);
        
        float[] outputArray = outputTensor.AsFloats();
        outputValue.text = outputArray.Aggregate("Output: \n", (current, t) => current + (t + "\n"));
        SpawnDot(outputArray);
        tensor.Dispose();
        outputTensor.Dispose();
    }
    

    private float[] ConvertImagesToTensor(Texture2D image)
    {
        float[] tensor = new float[224 * 224 * 3];
        
        Color32[] pixels = image.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 pixel = pixels[i];
            tensor[i * 3 + 0] = pixel.r / 255.0f;
            tensor[i * 3 + 1] = pixel.g / 255.0f;
            tensor[i * 3 + 2] = pixel.b / 255.0f;
        }

        return tensor;
    }
    private void OnDestroy()
    {
        _worker?.Dispose();
    }

    private void SpawnDot(float[] output)
    {

        // Vector3 spawnPos = new Vector3(-3.05517620962f, -4.37806443577f, 8.4095785043f);
        Vector3 spawnPred = new Vector3(output[2], output[1], output[0]);
        Vector3 spawnPlayer = new Vector3(output[2], output[1] + 0.5f, output[0]);
        Vector3 spawn1StFloor = new Vector3(output[2] + 2.18f, output[1], output[0] + -3.55f);
        // Instantiate(truePosition, spawnPos, Quaternion.identity);
        Instantiate(predPosition, spawnPred, Quaternion.identity);
        Instantiate(firstFloor, spawn1StFloor, Quaternion.identity);
        // Instantiate(playerObject, spawnPlayer, Quaternion.identity);
    }
    
    private Texture2D Resize(Texture2D texture2D,int targetX,int targetY)
    {
        RenderTexture rt=new RenderTexture(targetX, targetY,24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D,rt);
        Texture2D result=new Texture2D(targetX,targetY);
        result.ReadPixels(new Rect(0,0,targetX,targetY),0,0);
        result.Apply();
        return result;
    }
    private Texture2D ImageCropping(Texture2D resizedImg, int targetWidth, int targetHeight)
    {
        Texture2D croppedTexture = new Texture2D(targetWidth, targetHeight);
        int offsetX = (resizedImg.width - targetWidth) / 2;
        int offsetY = (resizedImg.height - targetHeight) / 2;
        croppedTexture.SetPixels(resizedImg.GetPixels(offsetX, offsetY, targetWidth, targetHeight));
        croppedTexture.Apply();
        return croppedTexture;
    }
}