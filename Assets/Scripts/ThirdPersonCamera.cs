using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target; // Takip edilecek hedef (Player objesi)
    public float distance = 5.0f; // Hedef ile kamera arasındaki mesafe
    public float height = 2.0f; // Kameranın hedefin ne kadar yukarısında olacağı
    public float smoothSpeed = 0.125f; // Kameranın hedefe yumuşak takibi
    public float mouseSensitivity = 2f; // Fare hassasiyeti

    private float currentYaw = 0.0f; // Yatay dönüş açısı (Yaw)
    private float currentPitch = 0.0f; // Dikey dönüş açısı (Pitch)

    void LateUpdate() // Kameranın hareketleri diğer objelerin hareketlerinden sonra olmalı
    {
        if (target == null) return;

        // Fare girdisi al
        currentYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Dikey bakışı sınırla (kameranın çok aşağı veya çok yukarı bakmasını engelle)
        currentPitch = Mathf.Clamp(currentPitch, -30f, 60f); // Örnek değerler, kendine göre ayarla

        // Kamera için hedef dönüşü hesapla
        // Quaternion.Euler(pitch, yaw, 0): Kameranın dikey (pitch) ve yatay (yaw) dönüşünü belirler
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        // Kameranın hedefe göre konumunu hesapla
        // -Vector3.forward * distance: Hedefin arkasında belirli bir mesafe
        Vector3 desiredPosition = target.position + (rotation * -Vector3.forward * distance) + (Vector3.up * height);

        // Kamerayı yumuşak bir şekilde istenen konuma hareket ettir
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Kamerayı hedefe doğru döndür
        transform.LookAt(target.position + Vector3.up * (height / 2)); // Hedefin orta kısmına bak
    }
}