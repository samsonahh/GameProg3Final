using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Samson
{
    public class SkyboxManager : MonoBehaviour
    {
        private Material skyboxMaterial;

        [SerializeField] private float rotationSpeed = 2f;
        private float currentAngle;

        // Start is called before the first frame update
        void Start()
        {
            skyboxMaterial = new Material(RenderSettings.skybox);
            RenderSettings.skybox = skyboxMaterial;
        }

        // Update is called once per frame
        void Update()
        {
            currentAngle += Time.deltaTime * rotationSpeed;
            currentAngle %= 360f;
            skyboxMaterial.SetFloat("_Rotation", currentAngle);
        }
    }
}
