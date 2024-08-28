/* 
    Readme:
        Calse: MovementManager
        Fecha Modificación: 27/08/2024
        objetivo: Lograr el movimiento de la palanca del yoke y rotacion del timon segun muestra dada
        Metodos: HandleYokeRotation, HandleControlWheelRotation
        Corrutinas: ReturnToInitialYokeRotation, ReturnToInitialControlWheelRotation
        Autor: Juan David Castro Samia
*/

using System.Collections;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [Header("Configuración del Yoke")]
    [SerializeField] private Transform yokeObject; // Objeto que será rotado con el clic
    [SerializeField] private float yokeRotationSpeed = 1.5f; // Velocidad de rotación del yoke
    [SerializeField] private float yokeMinRotation = -19f; // Límite inferior de rotación en el eje X para el yoke
    [SerializeField] private float yokeMaxRotation = 20f; // Límite superior de rotación en el eje X para el yoke
    [SerializeField] private float yokeReturnTime = 0.7f; // Tiempo que toma volver a la rotación inicial del yoke

    [Header("Configuración del Control Wheel")]
    [SerializeField] private Transform controlWheelObject; // Objeto que será rotado con las teclas A y D
    [SerializeField] private float controlWheelRotationSpeed = 100f; // Velocidad de rotación del control wheel
    [SerializeField] private float controlWheelMinRotation = -45f; // Límite inferior de rotación en el eje X para el control wheel
    [SerializeField] private float controlWheelMaxRotation = 45f; // Límite superior de rotación en el eje X para el control wheel
    [SerializeField] private float controlWheelReturnTime = 0.8f; // Tiempo que toma volver a la rotación inicial del control wheel

    private bool isDragging = false; // Indica si el yoke está siendo arrastrado
    private Quaternion initialYokeRotation; // Almacena la rotación inicial del yoke
    private Coroutine yokeReturnCoroutine; // Referencia a la coroutine de retorno del yoke

    private float initialControlWheelRotationX; // Almacena la rotación inicial en el eje X del control wheel
    private Coroutine controlWheelReturnCoroutine; // Referencia a la coroutine de retorno del control wheel

    void Start()
    {
        // Almacena la rotación inicial del yoke y del control de wheel
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
        HandleYokeRotation(); // Maneja la rotación del yoke
        HandleControlWheelRotation(); // Maneja la rotación del control wheel
    }

    // Maneja la rotación del yoke basado en el arrastre del mouse
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

                // Si ya hay una coroutine de retorno en ejecución, detenerla
                if (yokeReturnCoroutine != null)
                {
                    StopCoroutine(yokeReturnCoroutine);
                    yokeReturnCoroutine = null;
                }
            }
        }

        // Detiene el arrastre al soltar el botón del mouse
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            // Inicia la coroutine para devolver el yoke a su rotación inicial
            yokeReturnCoroutine = StartCoroutine(ReturnToInitialYokeRotation());
        }

        // Si se está arrastrando, rota el yoke dentro de los límites definidos
        if (isDragging)
        {
            float mouseY = Input.GetAxis("Mouse Y");
            float newRotationX = yokeObject.localEulerAngles.x - mouseY * yokeRotationSpeed;

            // Ajusta la rotación para que esté dentro del rango [yokeMinRotation, yokeMaxRotation]
            if (newRotationX > 180) newRotationX -= 360;
            newRotationX = Mathf.Clamp(newRotationX, yokeMinRotation, yokeMaxRotation);

            yokeObject.localEulerAngles = new Vector3(newRotationX, yokeObject.localEulerAngles.y, yokeObject.localEulerAngles.z);
        }
    }

    // Corrutina para interpolar la rotación del yoke hacia la rotación inicial
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

        // Asegurar que la rotación final sea exactamente la rotación inicial
        yokeObject.localRotation = initialYokeRotation;
    }

    // Maneja la rotación del control wheel basado en las teclas A y D
    private void HandleControlWheelRotation()
    {
        if (controlWheelObject == null) return;

        // Obtener la rotación actual en el eje X
        float currentRotationX = controlWheelObject.localEulerAngles.x;

        // Convertir la rotación a un rango de -180 a 180 grados
        if (currentRotationX > 180f)
        {
            currentRotationX -= 360f;
        }

        // Verifica si se está manteniendo pulsada la tecla A
        if (Input.GetKey(KeyCode.A))
        {
            // Si se está retornando, detener la coroutine
            if (controlWheelReturnCoroutine != null)
            {
                StopCoroutine(controlWheelReturnCoroutine);
                controlWheelReturnCoroutine = null;
            }

            // Calcular la nueva rotación
            float newRotationX = currentRotationX - controlWheelRotationSpeed * Time.deltaTime;

            // Limitar la nueva rotación al rango definido
            if (newRotationX < controlWheelMinRotation)
            {
                newRotationX = controlWheelMinRotation;
            }

            // Aplicar la nueva rotación
            controlWheelObject.localEulerAngles = new Vector3(newRotationX, controlWheelObject.localEulerAngles.y, 
                controlWheelObject.localEulerAngles.z);
        }
        // Verifica si se está manteniendo pulsada la tecla D
        else if (Input.GetKey(KeyCode.D))
        {
            // Si se está retornando, detener la coroutine
            if (controlWheelReturnCoroutine != null)
            {
                StopCoroutine(controlWheelReturnCoroutine);
                controlWheelReturnCoroutine = null;
            }

            // Calcular la nueva rotación
            float newRotationX = currentRotationX + controlWheelRotationSpeed * Time.deltaTime;

            // Limitar la nueva rotación al rango definido
            if (newRotationX > controlWheelMaxRotation)
            {
                newRotationX = controlWheelMaxRotation;
            }

            // Aplicar la nueva rotación
            controlWheelObject.localEulerAngles = new Vector3(newRotationX, controlWheelObject.localEulerAngles.y, 
                controlWheelObject.localEulerAngles.z);
        }
        else
        {
            // Si no se están presionando las teclas, iniciar la coroutine para volver a la rotación inicial
            if (controlWheelReturnCoroutine == null)
            {
                controlWheelReturnCoroutine = StartCoroutine(ReturnToInitialControlWheelRotation());
            }
        }
    }

    // Corrutina para interpolar la rotación del control wheel hacia la rotación inicial
    private IEnumerator ReturnToInitialControlWheelRotation()
    {
        float currentRotationX = controlWheelObject.localEulerAngles.x;

        // Convertir la rotación actual a un rango de -180 a 180 grados
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

            // Limitar la rotación al rango definido
            if (newRotationX < controlWheelMinRotation) newRotationX = controlWheelMinRotation;
            if (newRotationX > controlWheelMaxRotation) newRotationX = controlWheelMaxRotation;

            controlWheelObject.localEulerAngles = new Vector3(newRotationX, controlWheelObject.localEulerAngles.y, 
                controlWheelObject.localEulerAngles.z);
            yield return null;
        }

        // Asegurar que la rotación final sea exactamente la rotación inicial
        controlWheelObject.localEulerAngles = new Vector3(initialControlWheelRotationX, controlWheelObject.localEulerAngles.y,
            controlWheelObject.localEulerAngles.z);

        controlWheelReturnCoroutine = null;
    }
}
