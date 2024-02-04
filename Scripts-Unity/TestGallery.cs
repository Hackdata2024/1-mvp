using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using TMPro;
public class TestGallery : MonoBehaviour
{
    public string imageURL; 
	public string nameURL = "https://twis.in/shop/apis/text.php";
	[SerializeField]Texture2D texture;
    byte[] data;
    public string playerName;
    public InputField playerNameInput;
    public GameObject ImageCube;
    [SerializeField] string imageName;
    [SerializeField] bool setUpComplete;
    public static TestGallery instance;
    public TMP_Text notification;
    private void Awake()
    {
        instance = this;    
    }
    private void Start()
    {
        playerNameInput.text  = PlayerPrefs.GetString("PlayerName", "Unknown");
        
    }
    public void PickImage( )
	{
	NativeGallery.Permission permission = NativeGallery.GetImageFromGallery( ( path ) =>
	{
		Debug.Log( "Image path: " + path );
		if( path != null )
		{
			// Create Texture from selected image
			texture = NativeGallery.LoadImageAtPath( path);
            data = File.ReadAllBytes(path);
            imageName = Path.GetFileName(path);
            Texture2D temp_texture = new Texture2D((int)ImageCube.GetComponent<MeshRenderer>().bounds.size.x, (int)ImageCube.GetComponent<MeshRenderer>().bounds.size.y);
            temp_texture.LoadImage(data);
            Material material = ImageCube.GetComponent<Renderer>().material;
            material.mainTexture = texture;

            if ( texture == null )
			{
				Debug.Log( "Couldn't load texture from " + path );
				return;
			}
		}
	} );

	Debug.Log( "Permission result: " + permission );
	}


    /// <summary>
    /// //////////////////////////////////////
    /// </summary
    

    void CleanNotification()
    {
        notification.text = "";
    }
    IEnumerator UploadImage()
    {

        PickImage();
        notification.text = "Uploading Image";
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", data, $"{imageName}"); 

        using (UnityWebRequest www = UnityWebRequest.Post(imageURL, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                notification.text = "Error uploading image";
                Debug.LogError(www.error);
                Invoke("CleanNotification", 2.5f);
            }
            else
            {
                notification.text = "Image uploaded successfully";
                Debug.Log("Image uploaded successfully: " + www.downloadHandler.text);
                Invoke("CleanNotification", 2.5f);
            }
        }
    }

    public void StartUpload()
    {
        StartCoroutine(UploadImage());
    } 



    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////////////////
    /// </summary>

    public void StartUploadName()
    {
        playerName = PlayerPrefs.GetString("PlayerName", "Unknown");
        setUpComplete = PlayerPrefs.GetInt("setUpComplete", 0) >= 1 ? true : false;
        if (!setUpComplete) StartCoroutine(UploadName());
        else 
        {
            imageURL = $"https://twis.in/shop/apis/snudata/{playerName}/index.php";
            Debug.Log("already setup");
            Debug.Log(playerName);
            Debug.Log(imageURL);
        }
    }

    IEnumerator UploadName()
    {
        
        string name = string.Concat(playerName.Where(c => !char.IsWhiteSpace(c))).ToLower();
        WWWForm form = new WWWForm();
        form.AddField("textData", name);
        using (UnityWebRequest www = UnityWebRequest.Post(nameURL, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("name uploaded successfully: " + www.downloadHandler.text);
                imageURL = $"https://twis.in/shop/apis/snudata/{name}/index.php";
                playerName = name;
                Debug.Log(imageURL);
                Debug.Log(playerName);
                PlayerPrefs.SetInt("setUpComplete", 2);
            }
        }
    }
}
