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
  - ใช้ output จาก prediction เพื่อเรียกฟังก์ชัน `Spawn`  
  - นำ index 0, 2 ไปเป็น position `(x, z)`  
  - นำ index 3-6 ไปแปลงเป็น `Euler Angle` และคำนวณ `rotation`  
  - คำนวณเส้นทางของผู้ใช้กับจุดหมายเพื่อสร้าง path  

- **DestinationBehavior**  
  - ทำให้จุดหมายที่เลือกสามารถหมุนและขยับขึ้นลงได้  

- **PlayerPosition**  
  - แสดงตำแหน่งและการหมุนของกล้องปัจจุบัน  

- **GetValueDropdown**  
  - คืนค่า position ของจุดหมายจาก dropdown selection  

---

# Getting Started
1. ดาวน์โหลดโปรเจกต์แล้วเปิด Unity  
2. ไปที่ `File > Build Setting` เลือก **Android** แล้วกด **Switch Platform**  
3. ไปที่ `Scenes` (มุมล่างซ้าย) แล้วดับเบิ้ลคลิกที่ `MainScene`  
4. ตรวจสอบ Hierarchy ของโปรเจกต์ที่มุมบนซ้าย  
5. เข้าไปที่ `Scripts > MainManager`  
6. ดูที่ `Inspector` ทางด้านขวา แล้วค้นหา `Main Manager (Script)`  
7. หาก `My Model` ขึ้นว่า `missing` ให้ลากไฟล์ model `.onnx` ไปวาง  

---
