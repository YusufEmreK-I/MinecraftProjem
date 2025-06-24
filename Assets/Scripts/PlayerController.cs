using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Hareket Hız Ayarları
    public float moveSpeed = 5f;
    public float sprintSpeedMultiplier = 1.5f;
    public float jumpForce = 8f; // CharacterController ile biraz daha yüksek bir değer gerekebilir
    public float gravity = -9.81f; // Yerçekimi kuvveti (Unity'nin Physics.gravity.y değeri)

    // Komponent Referansları
    private CharacterController controller; // CharacterController bileşeni
    private Transform cameraTransform; // ThirdPersonCamera'nın bağlı olduğu kamera

    // Hareket için iç değişkenler
    private Vector3 moveDirection = Vector3.zero; // Karakterin anlık hareket yönü
    private float verticalVelocity = 0; // Yerçekimi için dikey hız


    void Start()
    {
        controller = GetComponent<CharacterController>(); // CharacterController bileşenini al
        cameraTransform = Camera.main.transform; // Sahnemizin ana kamerasını al

        // Fare imlecini gizle ve kitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Sadece giriş (input) işlemleri ve çerçeveye bağlı güncellemeler için
    void Update()
    {
        // ESC tuşu ile fare imlecini geri getir
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Fizik tabanlı hareket ve diğer güncellemeler için
    void FixedUpdate()
    {
        // CharacterController kendi isGrounded özelliğine sahip
        // Eğer yere değiyorsak ve dikey hızımız negatifse (düşüyorsak), dikey hızı sıfırla veya çok küçük negatif yap
        if (controller.isGrounded)
        {
            verticalVelocity = -0.5f; // Yerle bağlantıyı korumak için küçük bir negatif değer
        }

        // WASD girişlerini al
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Hareket yönünü kameranın baktığı yöne göre hesapla
        // Y eksenindeki rotasyonu sıfırlayarak sadece yatay yönü alıyoruz (zemine paralel hareket)
        Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;

        moveDirection = cameraForward * vertical + cameraRight * horizontal;
        moveDirection.Normalize(); // Çapraz hareket hızını sabit tut

        // Koşma kontrolü
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= sprintSpeedMultiplier;
        }

        moveDirection *= currentSpeed; // Hızı hareket yönüne uygula

        // Zıplama
        if (Input.GetButtonDown("Jump") && controller.isGrounded) // Sadece yere değiyorsak zıpla
        {
            // Zıplama için gerekli başlangıç dikey hızı hesapla
            // Formül: v = sqrt(h * -2 * g) -> h: jumpForce, g: gravity
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // Yerçekimi uygula (dikey hızı her karede artır)
        verticalVelocity += gravity * Time.deltaTime; // Time.deltaTime ile kare hızından bağımsız hale getir

        // Dikey hızı genel hareket yönüne ekle
        moveDirection.y = verticalVelocity;

        // CharacterController ile karakteri hareket ettir
        // CharacterController.Move() çarpışmaları kendi başına halleder ve takılmayı engeller.
        controller.Move(moveDirection * Time.deltaTime);

        // Karakterin YATAY DÖNÜŞÜNÜ hareket yönüne göre yap (kameranın baktığı yöne doğru döner)
        if (moveDirection.magnitude > 0.1f) // Yeterince hareket ediyorsa (sıfır değilse)
        {
            // Sadece yatay hareket yönüne baksın
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0, moveDirection.z));
            // Karakteri yumuşakça hedefe doğru döndür
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
    // HandleLook() fonksiyonu artık burada yok, çünkü ThirdPersonCamera onu zaten yönetiyor.
}