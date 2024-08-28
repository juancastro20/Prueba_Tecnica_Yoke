/* 
    Readme:
        Calse: MovementManager
        Fecha Modificaci�n: 27/08/2024
        objetivo: Lograr el movimiento de la palanca del yoke y rotacion del timon segun muestra dada
        Metodos: HandleYokeRotation, HandleControlWheelRotation
        Corrutinas: ReturnToInitialYokeRotation, ReturnToInitialControlWheelRotation
        Autor: Juan David Castro Samia
*/

using System.Collections;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [Header("Configuraci�n del Yoke")]
    [SerializeField] private Transform yokeObject; // Objeto que ser� rotado con el clic
    [SerializeField] private float yokeRotationSpeed = 1.5f; // Velocidad de rotaci�n del yoke
    [SerializeField] private float yokeMinRotation = -19f; // L�mite inferior de rotaci�n en el eje X para el yoke
    [SerializeField] private float yokeMaxRotation = 20f; // L�mite superior de rotaci�n en el eje X para el yoke
    [SerializeField] private float yokeReturnTime = 0.7f; // Tiempo que toma volver a la rotaci�n inicial del yoke

    [Header("Configuraci�n del Control Wheel")]
    [SerializeField] private Transform controlWheelObject; // Objeto que ser� rotado con las teclas A y D
    [SerializeField] private float controlWheelRotationSpeed = 100f; // Velocidad de rotaci�n del control wheel
    [SerializeField] private float controlWheelMinRotation = -45f; // L�mite inferior de rotaci�n en el eje X para el control wheel
    [SerializeField] private float controlWheelMaxRotation = 45f; // L�mite superior de rotaci�n en el eje X para el control wheel
    [SerializeField] private float controlWheelReturnTime = 0.8f; // Tiempo que toma volver a la rotaci�n inicial del control wheel

    private bool isDragging = false; // Indica si el yoke est� siendo arrastrado
    private Quaternion initialYokeRotation; // Almacena la rotaci�n inicial del yoke
    private Coroutine yokeReturnCoroutine; // Referencia a la coroutine de retorno del yoke

    private float initialControlWheelRotationX; // Almacena la rotaci�n inicial en el eje X del control wheel
    private Coroutine controlWheelReturnCoroutine; // Referencia a la coroutine de retorno del control wheel

    void Start()
    {
        // Almacena la rotaci�n inicial del yoke y del control de wheel
        if (yokeObject != null)
        {
            initialYokeRotation = yokeObject.localRotation;
        }

        if (controlWheelObject != null)
        {
            initialControlWheelRotationX = controlWheelObject.localEulerAngles.x;
        }
    }

    void Update()
    {
        HandleYokeRotation(); // Maneja la rotaci�n del yoke
        HandleControlWheelRotation(); // Maneja la rotaci�n del control wheel
    }

    // Maneja la rotaci�n del yoke basado en el arrastre del mouse
    private void HandleYokeRotation()
    {
        if (yokeObject == null) return;

        // Inicia el arrastre si se hace clic en el yoke
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == yokeObject)
            {
                isDragging = true;

                // Si ya hay una coroutine de retorno en ejecuci�n, detenerla
                if (yokeReturnCoroutine != null)
                {
                    StopCoroutine(yokeReturnCoroutine);
                    yokeReturnCoroutine = null;
                }
            }
        }

        // Detiene el arrastre al soltar el bot�n del mouse
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            // Inicia la coroutine para devolver el yoke a su rotaci�n inicial
            yokeReturnCoroutine = StartCoroutine(ReturnToInitialYokeRotation());
        }

        // Si se est� arrastrando, rota el yoke dentro de los l�mites definidos
        if (isDragging)
        {
            float mouseY = Input.GetAxis("Mouse Y");
            float newRotationX = yokeObject.localEulerAngles.x - mouseY * yokeRotationSpeed;

            // Ajusta la rotaci�n para que est� dentro del rango [yokeMinRotation, yokeMaxRotation]
            if (newRotationX > 180) newRotationX -= 360;
            newRotationX = Mathf.Clamp(newRotationX, yokeMinRotation, yokeMaxRotation);

            yokeObject.localEulerAngles = new Vector3(newRotationX, yokeObject.localEulerAngles.y, yokeObject.localEulerAngles.z);
        }
    }

    // Corrutina para interpolar la rotaci�n del yoke hacia la rotaci�n inicial
    private IEnumerator ReturnToInitialYokeRotation()
    {
        Quaternion currentRotation = yokeObject.localRotation;
        float time = 0f;

        while (time < yokeReturnTime)
        {
            time += Time.deltaTime;
            yokeObject.localRotation = Quaternion.Lerp(currentRotation, initialYokeRotation, time / yokeReturnTime);
            yield return null;
        }

        // Asegurar que la rotaci�n final sea exactamente la rotaci�n inicial
        yokeObject.localRotation = initialYokeRotation;
    }

    // Maneja la rotaci�n del control wheel basado en las teclas A y D
    private void HandleControlWheelRotation()
    {
        if (controlWheelObject == null) return;

        // Obtener la rotaci�n actual en el eje X
        float currentRotationX = controlWheelObject.localEulerAngles.x;

        // Convertir la rotaci�n a un rango de -180 a 180 grados
        if (currentRotationX > 180f)
        {
            currentRotationX -= 360f;
        }

        // Verifica si se est� manteniendo pulsada la tecla A
        if (Input.GetKey(KeyCode.A))
        {
            // Si se est� retornando, detener la coroutine
            if (controlWheelReturnCoroutine != null)
            {
                StopCoroutine(controlWheelReturnCoroutine);
                controlWheelReturnCoroutine = null;
            }

            // Calcular la nueva rotaci�n
            float newRotationX = currentRotationX - controlWheelRotationSpeed * Time.deltaTime;

            // Limitar la nueva rotaci�n al rango definido
            if (newRotationX < controlWheelMinRotation)
            {
                newRotationX = controlWheelMinRotation;
            }

            // Aplicar la nueva rotaci�n
            controlWheelObject.localEulerAngles = new Vector3(newRotationX, controlWheelObject.localEulerAngles.y, 
                controlWheelObject.localEulerAngles.z);
        }
        // Verifica si se est� manteniendo pulsada la tecla D
        else if (Input.GetKey(KeyCode.D))
        {
            // Si se est� retornando, detener la coroutine
            if (controlWheelReturnCoroutine != null)
            {
                StopCoroutine(controlWheelReturnCoroutine);
                controlWheelReturnCoroutine = null;
            }

            // Calcular la nueva rotaci�n
            float newRotationX = currentRotationX + controlWheelRotationSpeed * Time.deltaTime;

            // Limitar la nueva rotaci�n al rango definido
            if (newRotationX > controlWheelMaxRotation)
            {
                newRotationX = controlWheelMaxRotation;
            }

            // Aplicar la nueva rotaci�n
            controlWheelObject.localEulerAngles = new Vector3(newRotationX, controlWheelObject.localEulerAngles.y, 
                controlWheelObject.localEulerAngles.z);
        }
        else
        {
            // Si no se est�n presionando las teclas, iniciar la coroutine para volver a la rotaci�n inicial
            if (controlWheelReturnCoroutine == null)
            {
                controlWheelReturnCoroutine = StartCoroutine(ReturnToInitialControlWheelRotation());
            }
        }
    }

    // Corrutina para interpolar la rotaci�n del control wheel hacia la rotaci�n inicial
    private IEnumerator ReturnToInitialControlWheelRotation()
    {
        float currentRotationX = controlWheelObject.localEulerAngles.x;

        // Convertir la rotaci�n actual a un rango de -180 a 180 grados
        if (currentRotationX > 180f)
        {
            currentRotationX -= 360f;
        }

        float time = 0f;

        while (time < controlWheelReturnTime)
        {
            // Si se presiona A o D durante el retorno, detener la coroutine
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                yield break; // Detiene la coroutine inmediatamente
            }

            time += Time.deltaTime;
            float newRotationX = Mathf.Lerp(currentRotationX, initialControlWheelRotationX, time / controlWheelReturnTime);

            // Limitar la rotaci�n al rango definido
            if (newRotationX < controlWheelMinRotation) newRotationX = controlWheelMinRotation;
            if (newRotationX > controlWheelMaxRotation) newRotationX = controlWheelMaxRotation;

            controlWheelObject.localEulerAngles = new Vector3(newRotationX, controlWheelObject.localEulerAngles.y, 
                controlWheelObject.localEulerAngles.z);
            yield return null;
        }

        // Asegurar que la rotaci�n final sea exactamente la rotaci�n inicial
        controlWheelObject.localEulerAngles = new Vector3(initialControlWheelRotationX, controlWheelObject.localEulerAngles.y,
            controlWheelObject.localEulerAngles.z);

        controlWheelReturnCoroutine = null;
    }
}
