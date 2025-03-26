using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
// สำหรับเป็น structure ที่เก็บ ชื่อ, ตำแหน่ง ของห้องเป้าหมาย
public class Target
{
    public string Name;
    public GameObject PositionObject;
}
