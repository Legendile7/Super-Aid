using UnityEngine;
using Firebase.Database;
using System.Collections;
using UnityEngine.Networking;

public class MainMenuManager : MonoBehaviour
{
    DatabaseReference databaseReference;
    public bool debugMode;
    public int debugUserID;
    public float debugLatitude;
    public float debugLongitude;

    //Actual non debug vars
    private float latitude = 0f;
    private float longitude = 0f;

    float recLatitude = 0f;
    float recLongitude = 0f;

    int counter = 0;
    string apiKey = "AIzaSyA_T8ZTrCAaPe7zVrrvozwyt9tGsPs7fug";

    // Method to send the request to the Google Distance Matrix API
    public IEnumerator SendRequest(string origins, string destinations)
    {

        // Construct the URL for the Distance Matrix API request
        string url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={origins}&destinations={destinations}&units=imperial&key={apiKey}";
        Debug.Log(url);
        // Create a UnityWebRequest object to send the request
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error sending request: " + webRequest.error);
            }
            else
            {
                // Parse the response data
                string responseText = webRequest.downloadHandler.text;
                Debug.Log("Response received: " + responseText);
            }
        }
    }
    private void Start()
    {
        Input.location.Start(10f, 0.1f);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        databaseReference.OrderByChild("requests").ChildAdded += HandleChildAdded;
    }
    void HandleChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        if (counter > 0)
        {
            Debug.Log("Some Shit happened!");
            DataSnapshot snapshot = args.Snapshot;
            string childKey = snapshot.Key;
            string childValue = snapshot.GetRawJsonValue();

            Debug.Log("Child added: Key = " + childKey + ", Value = " + childValue);
            // Assuming childValue is formatted as "[{"latitude":315,"longitude":123}]"
            // Remove the square brackets to get the actual JSON object
            string jsonObject = childValue.Trim('[', ']');

            // Deserialize the JSON object using JsonUtility
            LocationData locationData = JsonUtility.FromJson<LocationData>(jsonObject);

            // Access latitude and longitude
            recLatitude = locationData.latitude;
            recLongitude = locationData.longitude;

            Debug.Log("Latitude: " + latitude + ", Longitude: " + longitude);
            GetLocationOnListen();
            string origin = latitude + "," + longitude;
            Debug.Log("Origin: " + origin);
            string destination = recLatitude + "," + recLongitude;
            Debug.Log("Destination: " + destination);
            StartCoroutine(SendRequest(origin, destination));
        }
        else
            counter = 1;
        // Do something with the data in args.Snapshot
    }
    [System.Serializable]
    public class LocationData
    {
        public float latitude;
        public float longitude;
    }
    private void writeNewUser()
    {
        Data user = new Data(debugLatitude, debugLongitude); ;
        if (!debugMode)
            user.SetData(latitude, longitude);
        string json = JsonUtility.ToJson(user);
        databaseReference.Child("requests").Child(debugUserID.ToString()).RemoveValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Node removed successfully");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("Failed to remove node: " + ": " + task.Exception);
            }
        });
        databaseReference.Child("requests").Child(debugUserID.ToString()).SetRawJsonValueAsync(json);
    }
    private void GetLocationOnListen()
    {
        // Check if location service is running
        if (Input.location.status == LocationServiceStatus.Running)
        {
            // Get the current location
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;

            // Print or use the location data as needed
            Debug.Log("Latitude: " + latitude + ", Longitude: " + longitude);
        }
    }
    public void GetLocationOnClick()
    {
        // Check if location service is running
        if (Input.location.status == LocationServiceStatus.Running)
        {
            // Get the current location
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;

            // Print or use the location data as needed
            Debug.Log("Latitude: " + latitude + ", Longitude: " + longitude);

            //Write to database
            writeNewUser();
        }
    }
}
