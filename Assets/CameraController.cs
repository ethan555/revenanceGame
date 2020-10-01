using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class CameraController : MonoBehaviour
{
    #region Camera
        public static CameraController instance;
        public Transform cameraTransform;
        public Vector3 zoomAmount;
        private Vector3 zoom;
        private Vector3 zoomVelocity;
        public float minZoom;
        public float maxZoom;
        public float zoomRate;
    #endregion
    #region Panning variables
        public float panSpeed;
        public float panBorderThickness;
        public float panRate;
        public Vector2 panLimit;
    #endregion
    #region Position/Rotation/Movement
        private Vector3 position;
        private Vector3 velocity;
        private Quaternion rotation;
        private Quaternion deriv;
        public bool rotateSmooth = false;
        public float rotationSetAmount;
        public float rotationSmoothAmount;
        public float rotateRate;
    #endregion

    private void Start() {
        instance = this;
        position = transform.position;
        rotation = transform.rotation;
        zoom = cameraTransform.localPosition;
    }

    private void Update() {
        HandleInput();

        position.x = Mathf.Clamp(position.x, -panLimit.x, panLimit.x);
        position.z = Mathf.Clamp(position.z, -panLimit.y, panLimit.y);
        zoom.y = Mathf.Clamp(zoom.y, minZoom, maxZoom);
        zoom.z = Mathf.Clamp(zoom.z, -maxZoom, -minZoom);

        transform.position = Vector3.SmoothDamp(transform.position, position, ref velocity, panRate);
        transform.rotation = QuaternionUtil.SmoothDamp(transform.rotation, rotation, ref deriv, rotateRate);

        cameraTransform.localPosition = Vector3.SmoothDamp(cameraTransform.localPosition, zoom, ref zoomVelocity, zoomRate);
    }

    private void HandleInput() {
        Vector3 diff;
        if (Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - panBorderThickness) {
            diff = transform.forward * panSpeed * Time.deltaTime;
            position += diff;
        }
        if (Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= panBorderThickness) {
            diff = transform.forward * panSpeed * Time.deltaTime;
            position -= diff;
        }
        if (Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - panBorderThickness) {
            diff = transform.right * panSpeed * Time.deltaTime;
            position += diff;
        }
        if (Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= panBorderThickness) {
            diff = transform.right * panSpeed * Time.deltaTime;
            position -= diff;
        }
        if (Input.GetKeyDown("p")) {
            rotateSmooth = !rotateSmooth;
        }
        if (!rotateSmooth) {
            if (Input.GetKeyDown("q")) {
                rotation *= Quaternion.Euler(Vector3.up * rotationSetAmount);
            }
            if (Input.GetKeyDown("e")) {
                rotation *= Quaternion.Euler(Vector3.up * -rotationSetAmount);
            }
        } else {
            if (Input.GetKey("q")) {
                rotation *= Quaternion.Euler(Vector3.up * rotationSmoothAmount);
            }
            if (Input.GetKey("e")) {
                rotation *= Quaternion.Euler(Vector3.up * -rotationSmoothAmount);
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        zoom += scroll * zoomAmount;
    }
}
