using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TestLocationService : MonoBehaviour {

    #region Location data
    public Text lat;
    public Text LatNS;
    public Text LongWE;
    public Text alt;
    public Text Haccu;
    public Text Vaccu;
    public Text lon;
    public Text Tim;

    public Text Speed;
    //Compass Data
    public Text Mag;
    public Text head;
    public Text trueHead;

    public Text Cuenca;

    public bool ServiceRunning { get; private set; }
    public bool DataCompass { get; private set; }

    public Image rotateobject;

    #region Find out the speed
    float P1lat;
    float P1lon;
    double P1t;
    float P2lat;
    float P2lon;
    double P2t;
    #endregion
    #endregion

    Compass brujula;

    //Initial service
    //Stop service
    //get Status
    //GEt geo data location
    //set up mesuring


    void Start()
    {
        brujula = new Compass();
        DataCompass = false;

        ServiceRunning = false;
        transform.GetComponent<Image>().color = Color.red;


        P1lat = 0;
        P1lon = 0;
        P1t = 0;
        P2lat = 0;
        P2lon = 0;
        P2t = 0;

    }


    IEnumerator GetDatas()
    {

        while (ServiceRunning)
        {
            //get the new point
            P1lat = Input.location.lastData.latitude;
            lat.text = P1lat.ToString();
            if (P1lat > 0) { LatNS.text = "N"; }
            else
            {
                LatNS.text = "S";
            }

            P1lon = Input.location.lastData.longitude;
            lon.text = P2lon.ToString();
            if (P1lon > 0) { LongWE.text = "E"; }
            else
            {
                LongWE.text = "W";
            }

            alt.text = Input.location.lastData.altitude.ToString();
            Haccu.text = Input.location.lastData.horizontalAccuracy.ToString();
            Vaccu.text = Input.location.lastData.verticalAccuracy.ToString();

            P1t = Input.location.lastData.timestamp;
            Tim.text = P1t.ToString();

            Mag.text = brujula.magneticHeading.ToString();
            head.text = brujula.headingAccuracy.ToString();
            trueHead.text = brujula.trueHeading.ToString();

            //if ((P2lat != 0) && (P1t != P2t))
            //{
                Speed.text = DistanceAndSpeed(P1lat, P1lon, P2lat, P2lon, P1t, P2t).ToString();

                //40.0704° N, 2.1374° W
                Cuenca.text = GradosPoint2(P1lat, P1lon, 40.0704f, 2.1374f).ToString();
            //}

            //store the last point
            P2lat = P1lat;
            P2lon = P1lon;
            P2t = P1t;

            yield return new WaitForSeconds(0.1f);

        }

    }

    IEnumerator GetDataCompass()
    {
        while (DataCompass)
        {

            // Orient an object to point to magnetic north.
            rotateobject.rectTransform.rotation = Quaternion.Euler(0, 0, Input.compass.magneticHeading);

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator StartLocation()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield break;

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            print("Unable to determine device location");
            yield break;
        }
        else
        {
            //// Access granted and location value could be retrieved
            //print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

            ServiceRunning = true;
            brujula.enabled = true;
            DataCompass = true;
            transform.GetComponent<Image>().color = Color.green;

            StartCoroutine("GetDatas");
            StartCoroutine("GetDataCompass");

        }


    }


    public void StartService()
    {
        ServiceRunning = false;
        StartCoroutine("StartLocation");

    }

    public void StopService()
    {
        ServiceRunning = false;
        DataCompass = false;
        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
        transform.GetComponent<Image>().color = Color.red;
    }

    public void Exit()
    {
        if (ServiceRunning) StopService();

        Application.Quit();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lat1"></param>
    /// <param name="lon1"></param>
    /// <param name="lat2"></param>
    /// <param name="lon2"></param>
    /// <param name="T1"></param>
    /// <param name="T2"></param>
    /// <returns>Speed</returns>
    public float DistanceAndSpeed(float lat1, float lon1, float lat2, float lon2, double T1, double T2)
    {
        // Convert degrees to radians
        lat1 = lat1 * Mathf.PI / 180.0f;
        lon1 = lon1 * Mathf.PI / 180.0f;

        lat2 = lat2 * Mathf.PI / 180.0f;
        lon2 = lon2 * Mathf.PI / 180.0f;

        // radius of earth in metres
        float r = 6378100;

        // P
        float rho1 = r * Mathf.Cos(lat1);
        float z1 = r * Mathf.Sin(lat1);
        float x1 = rho1 * Mathf.Cos(lon1);
        float y1 = rho1 * Mathf.Sin(lon1);

        // Q
        float rho2 = r * Mathf.Cos(lat2);
        float z2 = r * Mathf.Sin(lat2);
        float x2 = rho2 * Mathf.Cos(lon2);
        float y2 = rho2 * Mathf.Sin(lon2);

        // Dot product
        float dot = (x1 * x2 + y1 * y2 + z1 * z2);
        float cos_theta = dot / (r * r);

        float theta = Mathf.Acos(cos_theta);

        // Distance in Metres
        float dist = r * theta;


        float speed_mps = 0f;
        float speed_kph = 0f;

        if ((T1>0)&&(T2>0)&&(T2>T1))
        {
            double time_s = (T2 - T1) / 1000.0;
             speed_mps = (float)(dist / time_s);
             speed_kph = (speed_mps * 3600.0f) / 1000.0f; 
        }

        return  speed_kph;

    }


#region degrees with respect other point
    public float GradosPoint2(float lat1, float lon1, float lat2, float lon2)
    {
        float y = Mathf.Sin(lon2 - lon1) * Mathf.Cos(lat2);
        float x = Mathf.Cos(lat1) * Mathf.Sin(lat2) -
                Mathf.Sin(lat1) * Mathf.Cos(lat2) * Mathf.Cos(lon2 - lon1);


        return RadiansToDegrees( Mathf.Atan2(y, x));


    }

    private float RadiansToDegrees(float radians){
        return radians * (180 / Mathf.PI);
    }
#endregion
}
