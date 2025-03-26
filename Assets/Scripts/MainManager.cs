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

    [SerializeField] private TMP_Text outputValue; // แสดง output จากการ predict ของ model
    [SerializeField] private NNModel myModel; // model สำหรับ prediction
    [SerializeField] private Camera ARcamera; // กล้อง AR
    [SerializeField] private Image displayImage; // ภาพที่ได้จากการ snapshot > resize > crop center
    
    private Model _runtimeModel; 
    private IWorker _worker; // ตัวประมวลผล model
    public static bool DEBUG = false; // สำหรับ Debug
    public Navigator navigate; // ตัวจัดการเส้นทาง
    public static string testImg = "IMG_9767_7"; // ไฟล์ภาพสำหรับทดสอบ
    public static string version = "V3"; // version ของ model และภาพ ที่จะใช้

    void Start()
    {
        // โหลด model
        _runtimeModel = ModelLoader.Load(myModel, true);
        
        if (DEBUG)
        {
            _runtimeModel = ModelLoader.Load(myModel);
        }

        // สร้าง Worker สำหรับประมวลผล model
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _runtimeModel, true);
    }

    public void Predict()
    {
        //ถ้าเป็น Debug mode จะอ่านไฟล์ภาพจากเครื่อง PC
        //ถ้าไม่ จะ snapshot ภาพจากมือถือ
        if (DEBUG)
        {
            // โหลดภาพจากไฟล์
            byte[] fileData = File.ReadAllBytes("Assets/Temp/" + version + "/" + testImg + ".jpg");
            Texture2D image = new Texture2D(224, 224);
            image.LoadImage(fileData);

            // สร้าง Sprite เพื่อนำภาพไปแสดงผลบนหน้าจอ (มุมบนขวา)
            Sprite spriteImg = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.5f, 0.5f)); 
            displayImage.sprite = spriteImg;

            // แปลงภาพเป็น Tensor
            float[,,,] tensorImg = ConvertImagesToTensor(image);
            using var tensor = new Tensor(1, image.height, image.width, 3, tensorImg);
            
            // รัน model
            _worker.Execute(tensor);
            Tensor outputTensor = _worker.PeekOutput();
            
            // แสดงค่าผลลัพธ์จากการทำนายของ model
            float[] outputArray = outputTensor.AsFloats();
            string[] txt = { "x: ", "y: ", "z: ", "qw: ", "qx: ", "qy: ", "qz: " };
            string outputTxt = "Output:\n";
            for (int i = 0; i < outputArray.Length; i++)
            {
                outputTxt += txt[i] + outputArray[i] + "\n";
            }
            outputValue.text = outputTxt;

            // เรียกใช้งานระบบนำทาง
            navigate.Spawn(outputArray);
            
            tensor.Dispose();
            outputTensor.Dispose();
        }
        else
        {
            // ถ่ายภาพจากกล้อง AR
            Texture2D image = GetImage();
            Texture2D resizedImg = Resized(image);
            Texture2D cropImg = ImageCrop(resizedImg, 224, 224);
            
            // แสดงภาพที่ resize, crop แล้ว บน UI
            Sprite spriteImg = Sprite.Create(cropImg, new Rect(0, 0, cropImg.width, cropImg.height), new Vector2(0.5f, 0.5f)); 
            displayImage.sprite = spriteImg;
            
            // แปลงภาพเป็น Tensor
            float[,,,] tensorImg = ConvertImagesToTensor(cropImg);
            using var tensor = new Tensor(1, cropImg.height, cropImg.width, 3, tensorImg);
            
            // รัน model
            _worker.Execute(tensor);
            Tensor outputTensor = _worker.PeekOutput();
            
            // แสดงค่าผลลัพธ์จากการทำนายของ model
            float[] outputArray = outputTensor.AsFloats();
            string[] txt = { "x: ", "y: ", "z: ", "qw: ", "qx: ", "qy: ", "qz: " };
            string outputTxt = "Output:\n";
            for (int i = 0; i < outputArray.Length; i++)
            {
                outputTxt += txt[i] + outputArray[i] + "\n";
            }
            outputValue.text = outputTxt;

            // เรียกใช้งานระบบนำทาง
            navigate.Spawn(outputArray);
            
            tensor.Dispose();
            outputTensor.Dispose();
        }
    }
    
    
    // ถ่ายภาพจากกล้อง AR
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
    
    
    // แปลงภาพเป็น Tensor 
    private float[,,,] ConvertImagesToTensor(Texture2D image)
    {

        float[,,,] tensor = new float[1, image.width, image.height, 3];
        
        for (int i = 0; i < image.width; i++) {
            for (int j = 0; j < image.height; j++)
            {
                Color temp = image.GetPixel(i, j);
                tensor[0, image.height - 1 - j, i, 0] = temp.r;
                tensor[0, image.height - 1 - j, i, 1] = temp.g;
                tensor[0, image.height - 1 - j, i, 2] = temp.b;
            }
        }   
        return tensor;
    }
    
    
    // ล้างข้อมูล Worker เมื่อเลิกใช้งาน
    private void OnDestroy()
    {
        _worker?.Dispose();
    }
    
    
    // resize ให้เป็น 256x256 ด้วยอัตราส่วนคงที่
    private Texture2D Resized(Texture2D texture2D)
    {
        int h = texture2D.height;
        int w = texture2D.width;
        int targetH, targetW;
        float ratio = (float)Mathf.Max(h, w) / Mathf.Min(h, w);
        
        if (h > w)
        {
            targetH = Mathf.RoundToInt(ratio * 256);
            targetW = 256;
        }
        else
        {
            targetW = Mathf.RoundToInt(ratio * 256);
            targetH = 256;
        }
        
        RenderTexture rt = new RenderTexture(targetW, targetH, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        
        Texture2D result = new Texture2D(targetW, targetH);
        result.ReadPixels(new Rect(0, 0, targetW, targetH), 0, 0);
        result.Apply();

        return result;
    }

    // crop รูปให้เป็น 224x224 จากตรงกลาง
    private Texture2D ImageCrop(Texture2D resizedImg, int targetWidth, int targetHeight)
    {
        
        Texture2D croppedTexture = new Texture2D(targetWidth, targetHeight);
        int offsetX = (resizedImg.width - targetWidth) / 2;
        int offsetY = (resizedImg.height - targetHeight) / 2;
        croppedTexture.SetPixels(resizedImg.GetPixels(offsetX, offsetY, targetWidth, targetHeight));
        croppedTexture.Apply();
        return croppedTexture;
    }
}
