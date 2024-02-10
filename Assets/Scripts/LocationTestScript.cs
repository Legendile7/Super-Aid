using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationTestScript : MonoBehaviour
{
    void Start()
    {
        Input.location.Start(10f, 0.1f);
    }

    void Update()
    {
        // Check if location service is running
        if (Input.location.status == LocationServiceStatus.Running)
        {
            // Get the current location
            float latitude = Input.location.lastData.latitude;
            float longitude = Input.location.lastData.longitude;

            // Print or use the location data as needed
            Debug.Log("Latitude: " + latitude + ", Longitude: " + longitude);
        }
    }

    void OnDisable()
    {
        // Stop location service when the script is disabled
        Input.location.Stop();
    }
}
