using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour {

	public OrthoCamera ScreenShot;

	String[] arguments;

	public float movementSpeed = 7.0f; // speed that the 
	public float turnSpeed = 4.0f; // degrees per second.
	public float upDownRange = 20.0f;
	public GameObject YellowTrail;
	public GameObject TextObject;
	public Camera Camera1;
	public Camera Camera2;

	public static Camera Camera3;
	public static Camera Camera4;
	float verticalRotation;
	bool rightclicked = false;
	public static bool Overview = false;
	public static int CurrentTask = 0;
	public static float StartTime;
	public static float FinishTime;

	public static int PharmisistClick = 0;
	public static int SalesPersonClick = 0;

	public static List<float> MapViewCount = new List<float> ();

	string FileSaveName="dataOutput"; //file save name
	string ExamineerName;
	string DistractVersion;
	string PictureSaveName;
	string RunNumber="0";

	int ClickCount = 0;

	string directory;

	private string secretKey = "mySecretKey";
	public string addScoreURL = "http://localhost/VEGSBackend/VEGSBeackend/VEGS.php?";
	public string highscoreURL = "http://localhost/VEGSBackend/VEGSBeackend/VEGS.php?";
	public Text nameTextInput;
	public Text scoreTextInput;
	public Text nameResultText;
	public Text scoreResultText;
	int interval = 5; //frequency of database calls
	float nextTime = 0;
	int counter = 0;



	CharacterController CC;

	
	// Use this for initialization
	void Start () {
		arguments = Environment.GetCommandLineArgs();
		
		//Temporarily removed to run without VEGS startup program - add back in for actual runs
		/*if (arguments.Length > 1) {
			FileSaveName = arguments [1];
			PictureSaveName = arguments [1];
			int StringCount = PictureSaveName.Length;
			PictureSaveName = PictureSaveName.Remove(StringCount - 4, 4);
			PictureSaveName = PictureSaveName + "_R" + arguments[4].ToString() + ".png";
			DistractVersion = arguments [2];
			ExamineerName = arguments [3];
			RunNumber = arguments[4];
			Debug.Log(FileSaveName);
		}
		else{ //saving screen shots*/
			FileSaveName = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
			FileSaveName = FileSaveName + ".txt";
			PictureSaveName = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
			PictureSaveName = PictureSaveName + ".png";
			DistractVersion = "None";
			ExamineerName = "None Provided";
			RunNumber = "1";
			Debug.Log (FileSaveName);
//		}
		CC = GetComponent<CharacterController> ();
		Camera1.enabled = true;
		Camera2.enabled = false;
		InvokeRepeating ("DrawTrail", 2, 0.5F);
		Camera3 = Camera1;
		Camera4 = Camera2;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.P))
        {
			SaveToDatabase();
        }
		if (Input.GetKeyDown(KeyCode.Escape)|| Input.GetKeyDown(KeyCode.Q))
		{
			SaveToDatabase();
			WritetoFile();
			Application.Quit();
		}

		if (Input.GetKeyDown (KeyCode.O)) {
			if(!Overview){
				PlayerMove.MapViewCount.Add (Time.time);
				Camera1.enabled = false;
				Camera2.enabled = true;
				Overview = true;
			}
			else{
				Camera2.enabled = false;
				Camera1.enabled = true;
				Overview = false;
			}
		}


		float rotLeftRight = Input.GetAxis ("Horizontal");
		transform.Rotate (0, rotLeftRight * turnSpeed, 0);//uncommented

		if (Input.GetMouseButtonDown (1)) {
				rightclicked = true;
				}
		if (Input.GetMouseButtonUp (1)) {
				rightclicked = false;
				}

		float forwardSpeed = Input.GetAxis ("Vertical");
		Vector3 speed = new Vector3 (0, 0, forwardSpeed * movementSpeed);

		speed = transform.rotation * speed;



		CC.SimpleMove (speed);//uncommented

		float z = Input.GetAxis ("Vertical") * Time.deltaTime * movementSpeed;//changed speed to movementSpeed since speed is a Vector3
		transform.Translate (0, 0, z);//uncommented

		float turnAngle = turnSpeed * Input.GetAxis ("Horizontal") * Time.deltaTime;//uncommented
		transform.Rotate (0, turnAngle, 0);//uncommented

		//Debug.Log ("X: " + (Mathf.Log (CC.transform.position.x) - 3) + " Z: " + CC.transform.position.x);

	}

	void LateUpdate () {
		if (rightclicked) {
				verticalRotation -= Input.GetAxis ("Mouse Y") * turnSpeed;
				verticalRotation = Mathf.Clamp (verticalRotation, -upDownRange, upDownRange);
				Camera.main.transform.localRotation = Quaternion.Euler (verticalRotation, 0, 0);
		}
	}

	void DrawTrail(){//what does this do?
		if (Input.GetAxis ("Vertical") > 0) {
						Vector3 PlayerPoistion = new Vector3 (CC.transform.position.x, 20.0f, CC.transform.position.z);
						if(CurrentTask == 0){
							GameObject go = (GameObject) Instantiate (YellowTrail, PlayerPoistion, CC.transform.rotation);
							go.GetComponent<Renderer>().material.color = Color.yellow;
						}
						else if(CurrentTask == 1){
							GameObject go2 = (GameObject) Instantiate (YellowTrail, PlayerPoistion, CC.transform.rotation);
							go2.GetComponent<Renderer>().material.color = Color.red;
						}
						else if(CurrentTask == 2){
							GameObject go3 = (GameObject) Instantiate (YellowTrail, PlayerPoistion, CC.transform.rotation);
							go3.GetComponent<Renderer>().material.color = Color.green;
						}	
				}
	}



	public void WritetoFile(){ //what will be written to file - where is the file being saved?
		directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

		string filePath = System.IO.Path.Combine(directory, @"Data\");
		filePath = System.IO.Path.Combine(filePath, FileSaveName);

		string filePath2 = System.IO.Path.Combine(directory, @"ScreenShots\");
		filePath2 = System.IO.Path.Combine(filePath2, PictureSaveName);


		ScreenShot.TakeScreenShot (filePath2);
		string Text1;


		if (!File.Exists (filePath)) { //check to see if file exists. if not exist, then create new file
			Text1 = ExamineerName + Environment.NewLine;
			File.WriteAllText (filePath, Text1);
		} 
		else {//if file already exists, then append to examiner's file
			Text1 = ExamineerName + Environment.NewLine;
			File.AppendAllText(filePath, Text1);
		}
			//high, low, or none
			Text1 = DistractVersion + Environment.NewLine;
			File.AppendAllText(filePath, Text1);

			//run number
			Text1 = RunNumber + Environment.NewLine;
			File.AppendAllText(filePath, Text1);

			//Clicked on Pharmists For First Time Seconds
			Text1 = Math.Round(StartTime, 2).ToString() + Environment.NewLine;
			File.AppendAllText(filePath, Text1);
		Text1 = Math.Round(StartTime, 2).ToString() + " ";

		//Picked up Persction Time Seconds
		string Text2 = Math.Round(FinishTime, 2).ToString() + Environment.NewLine;
			File.AppendAllText(filePath, Text2);
		Text2 = Math.Round(FinishTime, 2).ToString() + " ";

		//Basket Total
		string Text3 = Math.Round(InventoryList.BasketTotal, 2).ToString() + Environment.NewLine;
			File.AppendAllText(filePath, Text3);
		Text3 = Math.Round(InventoryList.BasketTotal, 2).ToString() + " ";

		//Atm WithDraw 1
		string Text4 = Math.Round(InventoryList.ATM_Time1, 2).ToString() + Environment.NewLine;
			File.AppendAllText(filePath, Text4);
		Text4 = Math.Round(InventoryList.ATM_Time1, 2).ToString() + " ";

		//Atm WithDraw 2
		string Text5 = Math.Round(InventoryList.ATM_Time2, 2).ToString() + Environment.NewLine;
			File.AppendAllText(filePath, Text5);
		Text5 = Math.Round(InventoryList.ATM_Time2, 2).ToString() + " ";

		//Atm WithDraw 3
		string Text6 = Math.Round(InventoryList.ATM_Time3, 2).ToString() + Environment.NewLine;
			File.AppendAllText(filePath, Text6);
		Text6 = Math.Round(InventoryList.ATM_Time3, 2).ToString() + " ";

		//Number of time the playe clicked on the pharmisist
		string Text7 = PharmisistClick.ToString() + Environment.NewLine;
			File.AppendAllText(filePath, Text7);
		Text7 = PharmisistClick.ToString() + " ";

		//Number of Time the player clicked on the Sales Person
		string Text8 = SalesPersonClick.ToString() + Environment.NewLine;
			File.AppendAllText(filePath, Text8);
		Text8 = SalesPersonClick.ToString() + " ";

		//Number of Time the player clicked on the Sales Person
		string Text9 = InventoryList.ShoppingListTimes.Count.ToString() + Environment.NewLine;
			File.AppendAllText(filePath, Text9);
		Text9 = InventoryList.ShoppingListTimes.Count.ToString() + " ";

		//Number of Time the player clicked on the Sales Person
		string Text10 = MapViewCount.Count.ToString() + Environment.NewLine;
			File.AppendAllText(filePath, Text10);
		
			
			//Divider - anything below not yet sent to database
			string Text11 = "Cart" + Environment.NewLine;
			File.AppendAllText(filePath, Text11);

			//for each inventory in inventory list
			foreach (Inventory list in InventoryList.inventory){
				Text3 = "";
				Text3 = list.Name + "," + Math.Round(list.Costs,2) + "," + list.Qty + "," + Math.Round(list.TimeStamp,2) + Environment.NewLine;
				File.AppendAllText (filePath, Text3);
			}
			//Divider
			Text3 = "ShopList" + Environment.NewLine;
			File.AppendAllText(filePath, Text3);

			int ShoppingListCount = 1;
			foreach (float num in InventoryList.ShoppingListTimes) {
				Text3 = "";
				Text3 = num.ToString() + Environment.NewLine;
				File.AppendAllText (filePath, Text3);
				ShoppingListCount++;
			}

			//Divider
			Text3 = "Map" + Environment.NewLine;
			File.AppendAllText(filePath, Text3);

			//what is mapViewCounts?
			int MapViewCounts = 1;
			foreach (float num in MapViewCount){
				Text3 = "";
				Text3 = num.ToString() + Environment.NewLine;
				File.AppendAllText (filePath, Text3);
				MapViewCounts++;
			}

			Text3 = "End" + Environment.NewLine;
		File.AppendAllText(filePath, Text3);

        Application.Quit();
    }

	public void SaveToDatabase()
	{
		string Text1 = Math.Round(StartTime, 2).ToString() + " ";

		//Picked up Persction Time Seconds
		string Text2 = Math.Round(FinishTime, 2).ToString() + " ";

		//Basket Total
		string Text3 = Math.Round(InventoryList.BasketTotal, 2).ToString() + " ";

		//Atm WithDraw 1
		string Text4 = Math.Round(InventoryList.ATM_Time1, 2).ToString() + " ";

		//Atm WithDraw 2
		string Text5 = Math.Round(InventoryList.ATM_Time2, 2).ToString() + " ";

		//Atm WithDraw 3
		string Text6 = Math.Round(InventoryList.ATM_Time3, 2).ToString() + " ";

		//Number of time the playe clicked on the pharmisist
		string Text7 = PharmisistClick.ToString() + " ";

		//Number of Time the player clicked on the Sales Person
		string Text8 = SalesPersonClick.ToString() + " ";

		//Number of Time the player clicked on the Sales Person
		string Text9 = InventoryList.ShoppingListTimes.Count.ToString() + " ";

		//Number of Time the player clicked on the Sales Person
		string Text10 = MapViewCount.Count.ToString();

		ExamineerName = "none";
		//PostData(ExamineerName + " ", DistractVersion + " ", RunNumber + " ", Text1, Text2, Text3, Text4, Text5, Text6, Text7, Text8, Text9, Text10);
		StartCoroutine(PostData(ExamineerName + " ", DistractVersion + " ", RunNumber + " ", Text1, Text2, Text3, Text4, Text5, Text6, Text7, Text8, Text9, Text10));
		Debug.Log(ExamineerName + " " + DistractVersion + " " + RunNumber + " " + Text1 + Text2 + Text3 + Text4 + Text5 + Text6 + Text7 + Text8 + Text9 + Text10);
		Debug.Log(HashInput(ExamineerName + " " + DistractVersion + " " + RunNumber + " " + Text1 + Text2 + Text3 + Text4 + Text5 + Text6 + Text7 + Text8 + Text9 + Text10));
	}



		IEnumerator PostData(string field1, string field2, string field3, string field4, string field5, string field6, string field7, string field8, string field9, string field10, string field11, string field12, string field13)
	{
		Debug.Log("Posting data");
		string hash = HashInput(field1 + field2 + field3 + field4 + field5 + field6 + field7 + field8 + field9 + field10 + field11 + field12 + field13 + secretKey);
		string post_url = addScoreURL+ "name=" + UnityWebRequest.EscapeURL(field1) + "&version=" + UnityWebRequest.EscapeURL(field2) + "&runNumber=" + UnityWebRequest.EscapeURL(field3) + "&start=" + UnityWebRequest.EscapeURL(field4) + "&finish=" + UnityWebRequest.EscapeURL(field5) + "&basketTotal=" + UnityWebRequest.EscapeURL(field6) + "&ATM1=" + UnityWebRequest.EscapeURL(field7) + "&ATM2=" + UnityWebRequest.EscapeURL(field8) + "&ATM3=" + UnityWebRequest.EscapeURL(field9) + "&numPharmacist=" + UnityWebRequest.EscapeURL(field10) + "&numSales=" + UnityWebRequest.EscapeURL(field11) + "&numShoppingList=" + UnityWebRequest.EscapeURL(field12) + "&numMap=" + UnityWebRequest.EscapeURL(field13) + "&hash=" + hash;
		//string post_url = addScoreURL + "name=" + field1 + "&version=" + field2 + "&runNumber=" + field3 + "&start=" + field4 + "&finish=" + field5 + "&basketTotal=" + field6 + "&ATM1=" + field7 + "&ATM2=" + field8 + "&ATM3=" + field9 + "&numPharmacist=" + field10 + "&numSales=" + field11 + "&numShoppingList=" + field12 + "&numMap=" + field13 + "&hash=" + hash;
		UnityWebRequest hs_post = UnityWebRequest.Post(post_url, hash);
		Debug.Log(post_url);
		yield return hs_post.SendWebRequest();
		if (hs_post.error != null)
			Debug.Log("There was an error posting the location data: " + hs_post.error);
	}
	public string HashInput(string input)
	{
		SHA256Managed hm = new SHA256Managed();
		byte[] hashValue = hm.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
		string hash_convert = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
		return hash_convert;
	}

}
