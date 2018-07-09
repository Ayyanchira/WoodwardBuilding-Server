using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class DeviceSceneSwitcher : MonoBehaviour {

    public bool revertDetection;
    public GameObject[] hololensObjects;
    public GameObject[] desktopObjects;

    // Use this for initialization
    void Start()
    {
        var isHololens = XRSettings.loadedDeviceName.Equals("WindowsMR");
        if (isHololens)
        {
            Debug.Log("Hololens Detected");
        }
        else
        {
            Debug.Log("Atleeast something is printing");
        }

        if (revertDetection)
        {
            isHololens = !isHololens;

        }

        foreach (var hololensObject in hololensObjects)
        {
            hololensObject.SetActive(isHololens);
        }

        foreach (var desktopObject in desktopObjects)
        {
            desktopObject.SetActive(!isHololens);
        }
    }
}
