using UnityEngine;
using TMPro;

public class PositionDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text positionText; 
    [SerializeField] private Transform Camera; 
    void Update()
    {
        // แสดง position, rotation ปัจจุบันของ Camera 
        Vector3 currentPosition = Camera.position;
        Quaternion currentRotation = Camera.rotation;
        Vector3 euler = currentRotation.eulerAngles;
        positionText.text = $"x: {currentPosition.x:F2}\ny: {currentPosition.y:F2}\nz: {currentPosition.z:F2}" +
                            $"\nqw: {currentRotation.w:F2}\nqx: {currentRotation.x:F2}\nqy: " +
                            $"{currentRotation.y:F2}\nqz: {currentRotation.z:F2} \n" +
                            $"{euler}";
    
    }
}