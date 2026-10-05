using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(ARPlaneManager))]
public class SceneController : MonoBehaviour
{

       // DEPRECATED : WAS USEFUL AT THE BEGGINING OF THE PROJECT TO UNDERSTAND SOME THINGS

    [SerializeField]
    private InputActionReference _rightActivateAction;

    [SerializeField]
    private InputActionReference _leftActivateAction;

    [SerializeField]
    private XRRayInteractor _leftRayInteractor;

    [SerializeField]
    private XRRayInteractor _rightRayInteractor;

    [SerializeField]
    private XRDirectInteractor _leftDirectInteractor;

    [SerializeField]
    private XRDirectInteractor _rightDirectInteractor;

    private ARPlaneManager _planeManager;

    // Start is called before the first frame update
    void Start()
    {
        _planeManager = GetComponent<ARPlaneManager>();

        if (_planeManager is null)
        {
            Debug.LogError("-> Can't find ARPlaneManager");
        }

        _planeManager.planesChanged += OnPlanesChanged;
        _rightActivateAction.action.performed += OnRightActivateAction;
        _leftActivateAction.action.performed += OnLeftActivateAction;
    }

    private void OnLeftActivateAction(InputAction.CallbackContext obj)
    {
        CheckIfRayHitsCollider();
    }

    private void CheckIfRayHitsCollider()
    {
        if (_leftRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            Debug.Log("-> Hit detected - name : " + hit.transform.name);
            Quaternion rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
        }
        else {
            Debug.Log("-> No hit detected");
        }
    }

    private void OnRightActivateAction(InputAction.CallbackContext obj) {
        SpawnGrappableCube();
    }

    private void SpawnGrappableCube() {
        Debug.Log("-> SceneController::SpawnGrappableCube()");

        Vector3 spawnPoint;

        foreach (var plane in _planeManager.trackables)
        {
            if (plane.classification == PlaneClassification.Table) {
                spawnPoint = plane.transform.position;
                spawnPoint.y += 0.125f;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool isLeftRayHovering = _leftRayInteractor.TryGetHitInfo(out Vector3 leftPosition, out Vector3 leftNormal, out int leftNumber, out bool leftValid);
        bool isRightRayHovering = _rightRayInteractor.TryGetHitInfo(out Vector3 rightPosition, out Vector3 rightNormal, out int rightNumber, out bool rightValid);

        if (!isLeftRayHovering)
        {
            _leftRayInteractor.enabled = false;
            _leftDirectInteractor.enabled = true;
        }
        else {
            _leftRayInteractor.enabled = true;
            _leftDirectInteractor.enabled = false;
        }

        if (!isRightRayHovering)
        {
            _rightRayInteractor.enabled = false;
            _rightDirectInteractor.enabled = true;
        }
        else
        {
            _rightRayInteractor.enabled = true;
            _rightDirectInteractor.enabled = false;
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args) {
        if (args.added.Count > 0)
        {
            foreach (var plane in _planeManager.trackables)
            {
                PrintPlaneLabel(plane);
            }

        }
    }

    private void PrintPlaneLabel(ARPlane plane) {
        string label = plane.classification.ToString();
        string log = $"Plane ID: {plane.trackableId}, Label : {label}";
        Debug.Log(log);
    }

    private void OnDestroy()
    {
        Debug.Log("-> SceneController::Destroy()");
        _planeManager.planesChanged -= OnPlanesChanged;
        _rightActivateAction.action.performed -= OnRightActivateAction;
        _leftActivateAction.action.performed -= OnLeftActivateAction;
    }
}
