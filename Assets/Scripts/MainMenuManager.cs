using UnityEngine;
using Firebase.Database;
using System.Collections;
using UnityEngine.Networking;
using System;
using UnityEngine.SceneManagement;
using VoxelBusters.EssentialKit;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Text nameText;
    DatabaseReference databaseReference;

    [Header("Debug Options")]
    public bool disableDuplicateIdCheck;
    public bool debugUserID;
    public string userID;
    public bool debugCoords;
    public float debugLatitude;
    public float debugLongitude;
    public bool debugDistance;

    //Actual non debug vars
    [Header("Changing these only works in windows editor")]
    public float latitude = 0f;
    public float longitude = 0f;

    float recLatitude = 0f;
    float recLongitude = 0f;

    int counter = 0;
    string apiKey = "AIzaSyA_T8ZTrCAaPe7zVrrvozwyt9tGsPs7fug";
    
    // Create a class to represent the structure of your JSON data
    [Serializable]
    public class NotificationData
    {
        public string origin;
        public string destination;
    }

    [System.Serializable]
    public class GoogleMapsResponse
    {
        public string[] destination_addresses;
        public string[] origin_addresses;
        public Row[] rows;
        public string status;
    }

    [System.Serializable]
    public class Row
    {
        public Element[] elements;
    }

    [System.Serializable]
    public class Element
    {
        public Distance distance;
        public Duration duration;
        public string status;
    }

    [System.Serializable]
    public class Distance
    {
        public string text;
        public int value;
    }

    [System.Serializable]
    public class Duration
    {
        public string text;
        public int value;
    }

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
                GoogleMapsResponse response = JsonUtility.FromJson<GoogleMapsResponse>(responseText);

                if (response != null && response.rows.Length > 0 && response.rows[0].elements.Length > 0)
                {
                    int durationValue = response.rows[0].elements[0].duration.value;
                    Debug.Log("Duration Value: " + durationValue);
                    if (durationValue < 600)
                    {
                        Debug.Log("Duration is less than 10 minutes, sending notification");
                        float distanceValue = response.rows[0].elements[0].distance.value;
                        float miles = distanceValue / 1609.344f;
                        SendNotification(miles, origins, destinations);
                    }
                    else
                    {
                        Debug.Log("Duration is more than 10 minutes.");
                        if (debugDistance)
                        {
                            float distanceValue = response.rows[0].elements[0].distance.value;
                            float miles = distanceValue / 1609.344f;
                            SendNotification(miles, origins, destinations);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Failed to parse JSON response or response is empty.");
                }
            }
        }
    }
    void SendNotification(float distanceValue, string origin, string destination)
    {
        Debug.Log("Sending notification");
        /*
        var notification = new AndroidNotification();
        notification.Title = "NEW ALERT!";
        notification.Text = "A new help request is available in your area. Distance: " + distanceValue + " miles";
        notification.FireTime = System.DateTime.Now.AddSeconds(1);
        notification.IntentData = "&origin=" + origin + "&destination=" + destination;
        var id = AndroidNotificationCenter.SendNotification(notification, "default");
        */
        AlertDialog dialog = AlertDialog.CreateInstance();
        dialog.Title = "NEW ALERT!";
        dialog.Message = "A new help request is available in your area. Distance: " + distanceValue + " miles";
        dialog.AddButton("View Directions", () => {
            Debug.Log("Yes button clicked");
            Application.OpenURL("https://www.google.com/maps/dir/?api=1" + "&origin=" + origin + "&destination=" + destination);
        });
        dialog.AddCancelButton("Dismiss", () => {
            Debug.Log("Cancel button clicked");
        });
        dialog.Show(); //Show the dialog
    }
    private void Start()
    {
        /*
        var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
        if (notificationIntentData != null)
        {
            Debug.Log("Data Found!");
            var notification = notificationIntentData.Notification;
            string url = "https://www.google.com/maps/dir/?api=1" + notification.IntentData;
            Application.OpenURL(url);
        }
        */
        nameText.text = "Hello " + PlayerPrefs.GetString("SavedName", "User") + "!";
        if (!debugUserID)
        {
            userID = PlayerPrefs.GetString("UID", "0");
        }
        Input.location.Start(10f, 0.1f);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        databaseReference.OrderByChild("requests").ChildAdded += HandleChildAdded;
        /*
        var args = NotificationCenterArgs.Default;
        args.AndroidChannelId = "default";
        args.AndroidChannelName = "Notifications";
        args.AndroidChannelDescription = "Main notifications";
        NotificationCenter.Initialize(args);
        StartCoroutine(Init());
        */
    }

    /*
    private void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
            if (notificationIntentData != null)
            {
                notificationIntentData = null;
                Debug.Log("Data Found!");
                var notification = notificationIntentData.Notification;
                string url = "https://www.google.com/maps/dir/?api=1" + notification.IntentData;
                Application.OpenURL(url);
            }
        }
    }
    */
    /*
    IEnumerator Init()
    {
        var request = NotificationCenter.RequestPermission();
        if (request.Status == NotificationsPermissionStatus.RequestPending)
            yield return request;
        Debug.Log("Permission result: " + request.Status);
    }
    internal void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionDeniedAndDontAskAgain");
    }

    internal void PermissionCallbacks_PermissionGranted(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionGranted");
    }

    internal void PermissionCallbacks_PermissionDenied(string permissionName)
    {
        Debug.Log($"{permissionName} PermissionCallbacks_PermissionDenied");
    }
    */
    [System.Serializable]
    public class UserLocation
    {
        public float latitude;
        public float longitude;
    }
    public void SignOut()
    {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.SignOut();
        SceneManager.LoadScene(0);
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

            Debug.Log("Child added: Key = " + childKey  + ", Value = " + childValue);

            //JSON conversion (might fuck up the project)
            int idStartIndex = childValue.IndexOf('"') + 1;
            int idEndIndex = childValue.IndexOf('"', idStartIndex);
            string newId = childValue.Substring(idStartIndex, idEndIndex - idStartIndex);

            if (disableDuplicateIdCheck || !string.Equals(newId, userID))
            {
                // Extracting latitude
                int latStartIndex = childValue.IndexOf("latitude") + "latitude".Length + 2; // Adding 2 for ": "
                int latEndIndex = childValue.IndexOf(",", latStartIndex);
                string latitudeStr = childValue.Substring(latStartIndex, latEndIndex - latStartIndex);
                recLatitude = float.Parse(latitudeStr);

                // Extracting longitude
                int lonStartIndex = childValue.IndexOf("longitude") + "longitude".Length + 2; // Adding 2 for ": "
                int lonEndIndex = childValue.IndexOf("}", lonStartIndex);
                string longitudeStr = childValue.Substring(lonStartIndex, lonEndIndex - lonStartIndex);
                recLongitude = float.Parse(longitudeStr);



                // Now you can use the parsed values
                Debug.Log("ID: " + newId);
                Debug.Log("Latitude: " + latitude);
                Debug.Log("Longitude: " + longitude);

                Debug.Log("Latitude: " + latitude + ", Longitude: " + longitude);
                GetLocationOnListen();
                string origin = latitude + "," + longitude;
                Debug.Log("Origin: " + origin);
                string destination = recLatitude + "," + recLongitude;
                Debug.Log("Destination: " + destination);
                StartCoroutine(SendRequest(origin, destination));
            }
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
        if (!debugCoords)
            user.SetData(latitude, longitude);
        string json = JsonUtility.ToJson(user);
        databaseReference.Child("requests").Child(userID.ToString()).RemoveValueAsync().ContinueWith(task =>
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
        databaseReference.Child("requests").Child(userID.ToString()).SetRawJsonValueAsync(json);
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
#if UNITY_EDITOR_WIN
        writeNewUser();
#endif
    }

    public void LoadScene(int id)
    {
        SceneManager.LoadScene(id);
    }
}
