using CuttingRoom.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceButtonUIController : MonoBehaviour
{
    private readonly Dictionary<int, List<Vector3>> buttonTransforms = new Dictionary<int, List<Vector3>>()
    {
        {1, new List<Vector3>() { new Vector3(0, 0, 0) } },
        {2, new List<Vector3>() { new Vector3(-100, 0, 0), new Vector3(100, 0, 0) } },
        {3, new List<Vector3>() { new Vector3(-200, 0, 0), new Vector3(0, 0, 0), new Vector3(200, 0, 0) } },
        {4, new List<Vector3>() { new Vector3(-300, 0, 0), new Vector3(-100, 0, 0), new Vector3(100, 0, 0), new Vector3(200, 0, 0) } }
    };

    public Canvas parentCanvas = null;

    public GameObject buttonUIPrefab = null;

    public int numberOfButtons = 0;

    private List<GameObject> buttons = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        if (parentCanvas != null && buttonUIPrefab != null && buttonTransforms.ContainsKey(numberOfButtons))
        {
            List<Vector3> transforms = buttonTransforms[numberOfButtons];
            for (int i = 1; i <= numberOfButtons; ++i)
            {
                Vector3 buttonTransform = transforms[i];
                //buttonTransform += parentCanvas.transform.localPosition;
                GameObject newButton = Instantiate(buttonUIPrefab, buttonTransform, Quaternion.identity);
                newButton.transform.SetParent(parentCanvas.transform);
                buttons.Add(newButton);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
