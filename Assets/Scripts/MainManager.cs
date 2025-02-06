using System;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
public class MainManager : MonoBehaviour
{
    [SerializeField] private TMP_Text outputValue;
    [SerializeField] private NNModel myModel;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Camera ARcamera;
    [SerializeField] private Image displayImage;
    
    private Model _runtimeModel;
    private IWorker _worker;
    public static bool DEBUG = false;
    public Navigator navigate;
    public static string testImg = "IMG_7228_5";
    public static string version = "V2";


    void Start()
    {
        _runtimeModel = ModelLoader.Load(myModel, true);
        if (DEBUG)
        {
            _runtimeModel = ModelLoader.Load(myModel);
        }
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _runtimeModel, true);
        var count = _runtimeModel.outputs.Count;

    }

    public void Predict()
    {
        if (DEBUG)
        {

            byte[] fileData = File.ReadAllBytes("Assets/Temp/" + version + "/" + testImg + ".jpg");
            Texture2D image = new Texture2D(224, 224);
            image.LoadImage(fileData);
            Sprite spriteImg = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.5f, 0.5f)); 

            displayImage.sprite = spriteImg;

            float[,,,] tensorImg = ConvertImagesToTensor(image);
            using var tensor = new Tensor(1, image.height,image.width,3, tensorImg);
            _worker.Execute(tensor);
            Tensor outputTensor = _worker.PeekOutput();
            
            // SaveFloatArrayAsImage(tensorImg,$"{Application.dataPath}/Temp/tensor.png");
            float[] outputArray = outputTensor.AsFloats();
            string[] txt = { "x: ", "y: ", "z: ", "qw: ", "qx: ", "qy: ", "qz: " };
            string outputTxt = "Output:\n";
            for (int i = 0; i < outputArray.Length ; i++)
            {
                outputTxt += txt[i] + outputArray[i] + "\n";
            }
            outputValue.text = outputTxt;
            navigate.Spawn(outputArray);
            
            tensor.Dispose();
            outputTensor.Dispose();
        }
        else
        {
            Texture2D image = GetImage();
            Texture2D resizedImg = Resized(image);
            Texture2D cropImg = ImageCrop(resizedImg, 224, 224);
            Sprite spriteImg = Sprite.Create(cropImg, new Rect(0, 0, cropImg.width, cropImg.height), new Vector2(0.5f, 0.5f)); 
            displayImage.sprite = spriteImg;
            
            float[,,,] tensorImg = ConvertImagesToTensor(cropImg);
            // // SaveFloatArrayAsImage(tensorImg, "Assets/Temp/img.jpg");
            //
            // byte[] img1 = resizedImg.EncodeToPNG();
            // File.WriteAllBytes($"{Application.dataPath}/Temp/resize.png", img1);
            //
            // byte[] img2 = cropImg.EncodeToPNG();
            // File.WriteAllBytes($"{Application.dataPath}/Temp/crop.png", img2);
            using var tensor = new Tensor(1, cropImg.height, cropImg.width, 3, tensorImg);
            _worker.Execute(tensor);
            Tensor outputTensor = _worker.PeekOutput();
            
            float[] outputArray = outputTensor.AsFloats();
            string[] txt = { "x: ", "y: ", "z: ", "qw: ", "qx: ", "qy: ", "qz: " };
            string outputTxt = "Output:\n";
            for (int i = 0; i < outputArray.Length ; i++)
            {
                outputTxt += txt[i] + outputArray[i] + "\n";
            }
            outputValue.text = outputTxt;
            navigate.Spawn(outputArray);
            
            tensor.Dispose();
            outputTensor.Dispose();
        }
    }
    

    public void SaveFloatArrayAsImage(float[,,,] floatArray, string filePath)
    {
    
        Texture2D texture = new Texture2D(224, 224);
        Color[] pixels = new Color[224 * 224];
        for (int i = 0 ; i < 224 ; i++) {
            for (int j = 0 ; j < 224 ; j++)
            {

                float r = floatArray[0,i,j,0];     
                float g = floatArray[0,i,j,1]; 
                float b = floatArray[0,i,j,2]; 
                pixels[i] = new Color(r, g, b);
                if ( (i == 0 && j == 0) || 
                    (i == 0 && j == 223) || 
                    (i == 223 && j == 0) || 
                    (i == 223 && j == 223)) 
                {
                    Debug.LogError($"i: {i} j: {j} color: {pixels[i]}");
                 
                }
                

            }
        }   
        texture.SetPixels(pixels);
        texture.Apply(); 
        
        byte[] jpgBytes = texture.EncodeToJPG();
        
        File.WriteAllBytes(filePath, jpgBytes);
    
        Debug.Log($"Image saved to {filePath}");
    }
    private Texture2D GetImage()
    {
        int swidth = Screen.width;
        int sheight = Screen.height;

        RenderTexture rt = new RenderTexture(swidth, sheight, 24);
        ARcamera.targetTexture = rt;
        var currentRT = RenderTexture.active;
        RenderTexture.active = ARcamera.targetTexture;

        ARcamera.Render();

        Texture2D image = new Texture2D(swidth, sheight);
        image.ReadPixels(new Rect(0, 0, swidth, sheight), 0, 0);
        image.Apply();

        RenderTexture.active = currentRT;
        ARcamera.targetTexture = null;
        rt.Release();

        return image;
    }
    
    private float[,,,] ConvertImagesToTensor(Texture2D image)
    {
        float[,,,] tensor = new float[1,image.width, image.height,3];
        Color[] pixels = image.GetPixels();
        Debug.LogError(pixels.Length);
        for (int i = 0 ; i < image.width ; i++) {
            for (int j = 0 ; j < image.height ; j++)
            {
                Color temp = image.GetPixel(i, j);
                tensor[0, image.height - 1 - j, i, 0] = temp.r;
                tensor[0, image.height - 1 - j, i, 1] = temp.g;
                tensor[0, image.height - 1 - j, i, 2] = temp.b;

            }
        }   
        return tensor;
    }
    
    private void OnDestroy()
    {
        _worker?.Dispose();
    }
    
    private Texture2D Resized(Texture2D texture2D)
    {
        int h = texture2D.height;
        int w = texture2D.width;
        float ratio = (float)h / w;
        int targetH = Mathf.RoundToInt(ratio * 256);
        int targetW = 256;
        
        RenderTexture rt=new RenderTexture(targetW, targetH,24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D,rt);
        Texture2D result=new Texture2D(targetW,targetH);
        result.ReadPixels(new Rect(0,0,targetW,targetH),0,0);
        result.Apply();

        int cropH = 256;
        int centerY = result.height / 2;
        int startY = Mathf.Max(0, centerY - cropH / 2);
        Texture2D cropTexture2D = new Texture2D(targetW, cropH);
        Color[] pixels = result.GetPixels(0, startY, targetW, cropH);
        cropTexture2D.SetPixels(pixels);
        cropTexture2D.Apply();
        
        return cropTexture2D;
    }

    private Texture2D ImageCrop(Texture2D resizedImg, int targetWidth, int targetHeight)
    {
        Texture2D croppedTexture = new Texture2D(targetWidth, targetHeight);
        int offsetX = (resizedImg.width - targetWidth) / 2;
        int offsetY = (resizedImg.height - targetHeight) / 2;
        croppedTexture.SetPixels(resizedImg.GetPixels(offsetX, offsetY, targetWidth, targetHeight));
        croppedTexture.Apply();
        return croppedTexture;
    }
    
    private float[] ConvertByteArrayToFloat32Array(byte[] byteArray)
    {
        // Calculate how many floats we can fit in the byte array
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(byteArray);

        // Get the width and height of the image
        int width = texture.width;
        int height = texture.height;

        // Extract the pixel colors
        Color[] pixels = texture.GetPixels();

        // Convert Color[] to float[] (RGB)
        float[] floatArray = new float[pixels.Length * 3]; // 3 floats per pixel (R, G, B)

        for (int i = 0; i < pixels.Length; i++)
        {
            floatArray[i * 3] = pixels[i].r;  // Red
            floatArray[i * 3 + 1] = pixels[i].g;  // Green
            floatArray[i * 3 + 2] = pixels[i].b;  // Blue
        }

        return floatArray;
    }




}