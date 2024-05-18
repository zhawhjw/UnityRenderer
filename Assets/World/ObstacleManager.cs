using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public string jsonSource = "[{\"position\": [2.6260000617293002, 24.092370369268863], \"radius\": [1.5]}, {\"position\": [13.165934993561873, 41.51359525851718], \"radius\": [1.5]}, {\"position\": [47.12643995869444, 0.34703943956067573], \"radius\": [1.5]}, {\"position\": [45.62537470118529, 8.570224936742077], \"radius\": [1.5]}, {\"position\": [9.046611824797917, 20.838835071231575], \"radius\": [1.5]}, {\"position\": [35.90279317092517, 40.93314827859458], \"radius\": [1.5]}, {\"position\": [28.942070833622086, 16.76435628158831], \"radius\": [1.5]}, {\"position\": [39.262555238583694, 15.829866564548894], \"radius\": [1.5]}, {\"position\": [24.98701683392477, 17.15281150814051], \"radius\": [1.5]}, {\"position\": [1.4775011357223644, 30.794928030155244], \"radius\": [1.5]}, {\"position\": [25.277425857298436, 7.995918999593737], \"radius\": [1.5]}, {\"position\": [13.146982194700268, 32.36375060395713], \"radius\": [1.5]}, {\"position\": [37.41661615390303, 36.26340214014247], \"radius\": [1.5]}, {\"position\": [23.293108022150893, 24.58970100856659], \"radius\": [1.5]}, {\"position\": [1.9457102348215793, 38.74429089046262], \"radius\": [1.5]}, {\"position\": [10.273139810030147, 25.021541253368508], \"radius\": [1.5]}, {\"position\": [19.173629390433334, 7.379225887572205], \"radius\": [1.5]}, {\"position\": [11.08516887803977, 13.5075313996046], \"radius\": [1.5]}, {\"position\": [23.722518771685465, 44.64363147558175], \"radius\": [1.5]}, {\"position\": [21.445072394658517, 18.102315514680996], \"radius\": [1.5]}]";

    void Start()
    {
        //List< Dictionary< string, List<float> > > weatherForecast = JsonSerializer.Deserialize< List< Dictionary< string, List<float> > > >(jsonSource);
    }
}
