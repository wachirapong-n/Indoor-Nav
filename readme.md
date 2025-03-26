# Unity Version
**2023.2.7f1**

---

# Requirements
- ไฟล์ model ที่เป็น `.onnx`

---

# Assets Folder
- **Materials** - เก็บ Material ต่างๆ  
- **Models** - เก็บ model ที่ใช้ในการ prediction  
- **Pcx** - ใช้สำหรับ execute point cloud เพื่อให้ Unity อ่าน point cloud ได้  
- **Prefabs** - สำหรับเก็บ point cloud และ CSB model ที่สร้างเองใน Unity  
- **ProBuilder Data** - สำหรับสร้าง GameObject ที่เป็นบันไดขั้น  
- **Scenes** - เก็บ `MainScene`  
- **Scripts** - เก็บ Script  
- **Temp** - เก็บไฟล์รูปที่ใช้ในการทดสอบ  

---

# Hierarchy in Unity
### **Canvas (UI แสดงผลบนหน้าจอ)**
- **Finish Button (Legacy)** - UI ปุ่ม Finish  
- **Reset Button (Legacy)** - UI ปุ่ม Reset  
- **Image (TMP)** - UI แสดงภาพที่ได้จาก snapshot  
- **Output Text (TMP)** - UI แสดงผลลัพธ์จากการ prediction  
- **Start Button (Legacy)** - UI ปุ่ม Start  
- **Debug (TMP)** - UI แสดงข้อความ error จากการหาเส้นทาง  
- **Dropdown** - UI dropdown ให้เลือกจุดหมาย  
- **PlayerPosition (TMP)** - UI แสดงตำแหน่งและการหมุนของกล้อง  
- **Distance (TMP)** - UI แสดงระยะห่างระหว่างผู้ใช้กับจุดหมาย  
- **Popup Notification** - UI แสดงแจ้งเตือนเมื่อผู้ใช้เข้าใกล้จุดหมาย  

### **CSB**  
- CSB model ที่สร้างขึ้นเองใน Unity (Scale = 1:1)  

### **Scripts (เก็บสคริปต์ต่างๆ)**
- **MainManager**  
  - ทำการ snapshot รูป, resize เป็น 256x256, crop เป็น 224x224  
  - แปลงรูปเป็น Tensor 4 มิติ (batch_size, width, height, channel)  
  - นำ Tensor เป็น input ของ model เพื่อทำ prediction  
  - ส่ง output ไปยัง `Navigator` เพื่อทำการ spawn CSB model  

- **Navigation** 
  - เก็บไฟล์ Navigator
  - ใช้ output จาก prediction เพื่อเรียกฟังก์ชัน `Spawn`  
  - นำ index 0, 2 ไปเป็น `position` `(x, z)`  
  - นำ index 3-6 ไปแปลงเป็น `Euler Angle`
  - จากนั้น นำ Euler Angle ของผู้ใช้, CSB model, index 3-6 มารวมกันและคำนวณ `rotation`  
  - ในส่วนของ `position`  คือ นำ `rotation`  ที่ได้จากขั้นตอนก่อนหน้าไปคูณกับ `position` ที่ได้จาก prediction ก็จะได้ `position` ใหม่ที่เกิดการหมุนของตึก
  - set ให้จุดหมายที่ผู้ใช้เลือกสามารถขยับได้ โดยจะขยับโดยการหมุนตามแกน Yและขยับขึ้นลง
  - คำนวณเส้นทางของผู้ใช้กับจุดหมายเพื่อสร้าง path  

- **DestinationBehavior**  
  - ทำให้จุดหมายที่เลือกสามารถหมุนและขยับขึ้นลงได้  

- **PlayerPosition**  
  - แสดงตำแหน่งและการหมุนของกล้องปัจจุบัน  

- **GetValueDropdown**  
  - คืนค่า `position` ของจุดหมาย เพื่อที่จะสามารถรู้ได้ว่าจุดหมายที่เลือกใน dropdown มี position อะไร

---

# Getting Started
1. ดาวน์โหลดโปรเจกต์แล้วเปิด Unity  
2. ไปที่ `File > Build Setting` เลือก **Android** แล้วกด **Switch Platform**  
3. ไปที่ `Scenes` (มุมล่างซ้าย) แล้วดับเบิ้ลคลิกที่ `MainScene`  
4. ตรวจสอบ Hierarchy ของโปรเจกต์ที่มุมบนซ้าย  
5. เข้าไปที่ `Scripts > MainManager`  
6. ดูที่ `Inspector` ทางด้านขวา แล้วหา `Main Manager (Script)`  
7. `My Model` ขึ้นว่า `missing` ให้ลากไฟล์ model `.onnx` ไปวาง  

---

# ขั้นตอนการทำ

## 3D CSB Model

1. **สร้างชั้น 1**
   - สร้าง `GameObject` ที่เป็น `Cube`
   - ปรับ `Scale (x=54, y=0.1, z=36)` เพื่อสร้าง `floor`
2. **สร้างกำแพง**
   - ใช้ `GameObject` และปรับ `Scale` ตามแบบแปลนของตึก CSB
3. **แบ่งห้อง**
   - นำกำแพงมาวางให้เป็นห้องๆ และเว้นช่องว่างไว้สำหรับทางเข้า (ไม่มีประตู)
4. **สร้างบันได**
   - ใช้ `Pro Builder` สร้าง `GameObject` สำหรับบันได
   - ปรับ `Scale` ให้ตรงกับแบบแปลน แล้วนำไปวางเพื่อเชื่อมต่อไปยังชั้น 2
5. **สร้างชั้น 2**
   - ทำซ้ำเหมือนกับชั้น 1
6. **สร้างชั้น 3**
   - ทำซ้ำเหมือนกับชั้น 1 แต่ไม่มีการสร้างบันได
7. **สร้างห้องปลายทาง (Destination Room)**
   - สร้าง `GameObject Cube` ด้วย `Scale (x, y, z) = 0.3`
8. **วางห้องปลายทาง**
   - นำ `Destination Room` ไปวางในแต่ละห้อง

---

## Navigation

1. สร้าง **`NavMesh Surface`** บน `3D CSB Model`
2. **ตั้งค่าคุณสมบัติ**
   - `Radius = 0.1`
   - `Height = 1`
   - `Step Height = 1`
3. สร้าง **`Script Navigator.cs`**
   - ใช้ `LineRenderer` ที่มีอยู่ใน Unity
4. สร้างฟังก์ชัน **`UpdatePath`**
   - ใช้ `render path` เชื่อมระหว่างผู้ใช้กับห้องปลายทาง

---

## Spawning 3D CSB Model

1. ปิดการใช้งาน **`3D CSB Model`**
2. สร้างฟังก์ชัน ****`Spawn`**** ใน **`Navigator.cs`**
3. รับค่า ****`prediction output`**** ของ **`index 4, 5, 6, 3`**
   - แปลงเป็น `Quaternion` และ `Euler Angle`
4. แปลง ****`Quaternion`**** ของผู้ใช้ และ ****`3D CSB Model`**** เป็น **`Euler Angle`**
5. รวม ****`Euler Angle`**** และนำไปเป็น ****`rotation`**** ของ **`3D CSB Model`**
6. รับค่า ****`prediction output`**** ของ **`index 0, 2`**
   - ใช้เป็น `position x, z`
7. คำนวณ **`position ใหม่`**
   - นำ `rotation` ไปคูณกับ `position` เพื่อให้ `3D CSB Model` หมุนถูกต้อง
8. **ซ่อนทุก ****************************************`Destination Room`**************************************** ยกเว้นห้องที่ผู้ใช้เลือก**
9. เปิดการใช้งาน **`3D CSB Model`**

---

## Importing Prediction Model

1. ไปที่ `Window > Package Manager` และค้นหา `Barracuda` แล้วติดตั้ง
2. คัดลอกไฟล์ `.onnx` ไปที่ `Assets/Models`
3. โหลดและใช้งาน `.onnx` ด้วยโค้ดต่อไปนี้:

```csharp
using Unity.Barracuda;

public class MainManager : MonoBehaviour
{
    [SerializeField] private NNModel myModel;
    
    private Model _runtimeModel;
    private IWorker _worker;

    void Start()
    {
        _runtimeModel = ModelLoader.Load(myModel);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _runtimeModel);

    }

    public void Predict()
    {
        float[,,,] tensorImg = ConvertImagesToTensor(image);
        using var tensor = new Tensor(1, cropImg.height, cropImg.width, 3, tensorImg);
        
        _worker.Execute(tensor);
        
        Tensor outputTensor = _worker.PeekOutput(_output);
        
        tensor.Dispose();
        outputTensor.Dispose();
    }
    
}

```
4. สร้าง GameObject เปล่าๆ แล้วตั้งชื่อว่า `MainManager` แล้วนำ script ไฟล์ไปใส่
5. นำไฟล์ .onnx ไปวางที่ `MainManager (GameObject) > Inspector > MainManager (Script) > My Model` 
6. สร้างปุ่ม `Navigation` ที่เป็น UI Button
7. ไปที่ปุ่ม `Navigation > Inspector > On Click () > Runtime  Only` แล้วลาก `MainManager (GameObject)` ไปวาง จากนั้นเลือก ฟังก์ชัน `Predict`
---

## Snapshot, Resize, Crop

### 1. ฟังก์ชัน `GetImage`

```csharp
private Texture2D GetImage()
{
    // เก็บค่า width และ height ของหน้าจอ
    int swidth = Screen.width;
    int sheight = Screen.height;

    // สร้าง RenderTexture เพื่อใช้เป็นพื้นที่บันทึกภาพจากกล้อง
    RenderTexture rt = new RenderTexture(swidth, sheight, 24);

    // ตั้งค่าให้กล้องบันทึกภาพลงใน RenderTexture
    ARcamera.targetTexture = rt;

    // เก็บค่า RenderTexture ปัจจุบันไว้ เพื่อเรียกคืนภายหลัง
    var currentRT = RenderTexture.active;

    // กำหนดให้ RenderTexture.active เป็น RenderTexture ของกล้อง
    RenderTexture.active = ARcamera.targetTexture;

    // ให้กล้องเรนเดอร์ภาพไปที่ RenderTexture
    ARcamera.Render();

    // สร้าง Texture2D ขนาดเท่ากับหน้าจอ
    Texture2D image = new Texture2D(swidth, sheight);

    // อ่านค่าพิกเซลจาก RenderTexture แล้วบันทึกลง Texture2D
    image.ReadPixels(new Rect(0, 0, swidth, sheight), 0, 0);
    image.Apply(); // อัปเดตข้อมูล Texture2D

    // กู้คืนค่า RenderTexture.active ที่บันทึกไว้
    RenderTexture.active = currentRT;

    // นำ RenderTexture ออกจากกล้อง
    ARcamera.targetTexture = null;

    // ปล่อยหน่วยความจำที่ใช้
    rt.Release();

    // return Texture2D ที่สร้างขึ้น
    return image;
}
```

### 2. ฟังก์ชัน `Resized`

```csharp
private Texture2D Resized(Texture2D texture2D)
{
    // รับค่า h และ w ของภาพต้นฉบับ
    int h = texture2D.height;
    int w = texture2D.width;
    
    int targetH, targetW;
    
    // คำนวณอัตราส่วนของภาพ โดยใช้ด้านที่ใหญ่กว่าหารด้วยด้านที่เล็กกว่า
    float ratio = (float)Mathf.Max(h, w) / Mathf.Min(h, w);
    
    // ปรับขนาดภาพให้ด้านที่เล็กที่สุดเป็น 256 และรักษาอัตราส่วนให้คงที่
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
    
    // สร้าง RenderTexture ใหม่
    RenderTexture rt = new RenderTexture(targetW, targetH, 24);
    RenderTexture.active = rt; // กำหนดให้ RenderTexture เป็น active

    // คัดลอกข้อมูลจาก Texture2D ต้นฉบับไปยัง RenderTexture
    Graphics.Blit(texture2D, rt);
    
    // สร้าง Texture2D ใหม่
    Texture2D result = new Texture2D(targetW, targetH);

    // อ่านค่า pixed จาก RenderTexture แล้วบันทึกลง Texture2D ใหม่
    result.ReadPixels(new Rect(0, 0, targetW, targetH), 0, 0);
    result.Apply(); // อัปเดตข้อมูล Texture2D

    // คืนค่า Texture2D ที่ถูกปรับขนาดแล้ว
    return result;
}
```

### 3. ฟังก์ชัน `ImageCrop`

```csharp
private Texture2D ImageCrop(Texture2D resizedImg, int targetWidth, int targetHeight)
{
    // สร้าง Texture2D ใหม่เพื่อเก็บภาพที่ถูก crop
    Texture2D croppedTexture = new Texture2D(targetWidth, targetHeight);

    // คำนวณจุดเริ่มต้นของการครอบภาพ เพื่อให้ได้ภาพที่อยู่ตรงกลาง
    int offsetX = (resizedImg.width - targetWidth) / 2;
    int offsetY = (resizedImg.height - targetHeight) / 2;

    // คัดลอกพิกเซลจากภาพต้นฉบับโดยใช้ค่าชดเชยที่คำนวณได้
    croppedTexture.SetPixels(resizedImg.GetPixels(offsetX, offsetY, targetWidth, targetHeight));

    // อัปเดต Texture2D เพื่อให้เห็นผลของการครอบภาพ
    croppedTexture.Apply();

    // คืนค่า Texture2D ที่ถูกครอบแล้ว
    return croppedTexture;
}
```

