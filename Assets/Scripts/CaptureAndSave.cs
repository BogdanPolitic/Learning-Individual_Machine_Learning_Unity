using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Windows;
using System.Drawing;

public class CaptureAndSave : MonoBehaviour
{
    [SerializeField] private CharacterSoundPlayer _characterSoundPlayer;

    // Grab the camera's view when this variable is true.
    bool captureAllowed;

    GameObject[] allGOs;
    public GameObject captureTarget;
    GameObject captureBackground;

    GameModes gameStatus;
    public GameModes.Modes currentMode;

    Camera _camera;
    RenderTexture objectCaptureRenderTexture;

    Dictionary<string, int> captures;
    int numberOfCaptures;
    UIScript _UIScript;

    Dictionary<string, byte> objIds;
    string[] idObjs;
    NeuralNetwork neuralNetwork;
    int trainingVariety;
    int bucketSize = 1;

    public Dictionary<string, GameObject> objs;

    byte[] exampleBitmapArray;

    // dimensiunile imaginilor primite de reteaua neurala:
    int imageDimensionX = 28;
    int imageDimensionY = 28;

    int continuousCapturingPeriod = 20;


    GameObject showResultText;

    bool IsChildOf(GameObject child, GameObject parent)
    {
        GameObject tmp = child;

        while (tmp.transform.parent != null)
        {
            tmp = tmp.transform.parent.gameObject;
            if (GameObject.ReferenceEquals(tmp, parent))
                break;
        }

        return GameObject.ReferenceEquals(tmp, parent);
    }

    void Start()
    {
        RenderPipelineManager.endFrameRendering += OnEndFrameRendering;

        _camera = GetComponent<Camera>();
        objectCaptureRenderTexture = new RenderTexture(imageDimensionX, imageDimensionY, 24);

        allGOs = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
        captures = new Dictionary<string, int>();
        _UIScript = GameObject.Find("UIScript").GetComponent<UIScript>();
        captureBackground = GameObject.Find("CaptureBackground");
        gameStatus = GameObject.Find("GameModes").GetComponent<GameModes>();
        showResultText = GameObject.Find("ShowResultText");
        showResultText.GetComponent<Text>().text = "";

        captureBackground.GetComponent<MeshRenderer>().enabled = false;

        objIds = new Dictionary<string, byte>();
        objIds["Sofa"] = 0;
        objIds["TV"] = 1;
        objIds["Sink"] = 2;
        objIds["Bath"] = 3;
        objIds["Seat"] = 4;

        idObjs = new string[objIds.Count];
        idObjs[0] = "Sofa";
        idObjs[1] = "TV";
        idObjs[2] = "Sink";
        idObjs[3] = "Bath";
        idObjs[4] = "Seat";

        objs = new Dictionary<string, GameObject>();
        objs["Sofa"] = GameObject.Find("Sofa");
        objs["TV"] = GameObject.Find("TV");
        objs["Sink"] = GameObject.Find("Sink");
        objs["Bath"] = GameObject.Find("Bath");
        objs["Seat"] = GameObject.Find("ToiletSeat");

        //trainingVariety = objIds.Count;
        trainingVariety = 2;

        neuralNetwork = GameObject.Find("NeuralNetwork").GetComponent<NeuralNetwork>();
        StartCoroutine(LateStart(2 * Time.deltaTime));
    }

    IEnumerator LateStart(float waitTime) {
        neuralNetwork.SetTrainingVariety(trainingVariety);
        neuralNetwork.SetIOLayersSizes(imageDimensionY * imageDimensionX, trainingVariety);
        neuralNetwork.InitializeCoeficients();

        yield return new WaitForSeconds(waitTime);
    }

    void Update()
    {
        currentMode = _UIScript.currentMode;

        ReadImg();

        //Press space to start the screen grab
        if ((Input.GetKeyDown(KeyCode.Space) || AutomaticCapture()) &&
        	(currentMode == GameModes.Modes.Training || currentMode == GameModes.Modes.Test))
        {
            if (captureTarget == null)
            {
                captureAllowed = false;
                Debug.Log("Focus at an object before taking capture! Make sure the editor camera does not focus on trainable objects, or better switch the editor camera off the screen while performing training!");
                return;
            }
            
            if (currentMode == GameModes.Modes.Training && 
            	!objIds.ContainsKey(_UIScript.objName)) {
            	captureAllowed = false;
            	Debug.Log("There's no object category called " + _UIScript.objName);
            	return;
            }

            if (!AutomaticCapture())
                _characterSoundPlayer.PlayPhotoShootingSound();

            captureAllowed = true;
            foreach (GameObject o in allGOs)
            {
                if (!GameObject.ReferenceEquals(o, captureTarget) 
                    && !IsChildOf(o, captureTarget)
                    && !GameObject.ReferenceEquals(o, captureBackground))
                {
                    MeshRenderer mr = o.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        mr.enabled = false;
                    }
                }
            }

            captureBackground.GetComponent<MeshRenderer>().enabled = true;
            _camera.targetTexture = objectCaptureRenderTexture;
        }

        if (currentMode == GameModes.Modes.MenuToScene)
        	numberOfCaptures = 0;

        if (currentMode == GameModes.Modes.SceneToMenu)
        	if (numberOfCaptures >= bucketSize)
            	neuralNetwork.TrainNetworkAndWipeInputData(20, bucketSize, 1.0f);
            else
            	Debug.Log("More training is needed!");

        //if (Input.GetKeyDown(KeyCode.H)) {
        //	neuralNetwork.ReverseTrainingData();
        //}
    }

    bool AutomaticCapture() {
    	return 	currentMode == GameModes.Modes.Training && 
    			_UIScript.CC_ON &&
    			Time.frameCount % continuousCapturingPeriod == 0;
    }

    // Encode = greyPixel = (r + g + b) / 3
    // Resize = M x N => X x Y
    float[] EncodeAndResizeImage(Texture2D image, int newSizeX, int newSizeY) {
    	float[] resizedImageArray = new float[newSizeX * newSizeY];

    	for (int y = 0; y < newSizeY; y++) {
    		for (int x = 0; x < newSizeX; x++) {
    			int left = (int)Mathf.Ceil(x * (image.width / newSizeX));
    			int right = (int)Mathf.Ceil((x + 1) * (image.width / newSizeX));
    			int top = (int)Mathf.Floor(y * (image.height / newSizeY));
    			int bottom = (int)Mathf.Floor((y + 1) * (image.height / newSizeY));

    			float sum = 0;

    			for (int xk = left; xk <= right; xk++) {
    				for (int yk = top; yk <= bottom; yk++) {
    					//sum += (int)(image.GetPixel(xk, yk).r + image.GetPixel(xk, yk).g + image.GetPixel(xk, yk).b) / 3;
    					sum += image.GetPixel(xk, yk).r;
    				}
    			}

    			sum /= ((right - left + 1) * (bottom - top + 1));
    			resizedImageArray[y * newSizeX + x] = sum;
    		}
    	}

    	return resizedImageArray;
    }

    // given an image with pixels' values in range [0...1], it outputs the image with each pixel an 8bit, by multiplying each by 256
    byte[] UnitImageTo8BitImage(float[] unitImage, int newSizeX, int newSizeY) {
    	byte[] image8Bit = new byte[newSizeX * newSizeY];
    	for (int y = 0; y < newSizeY; y++) {
    		for (int x = 0; x < newSizeX; x++) {
    			image8Bit[y * newSizeX + x] = (byte)(unitImage[y * newSizeX + x] * 256.0f);
    		}
    	}
    	return image8Bit;
    }

    Texture2D BitmapToTexture(float[] bitmap, int width, int height) {
    	Texture2D txt = new Texture2D(width, height, TextureFormat.RGB24, false);	// TextureFormat.Alpha8
    	for (int y = 0; y < height; y++)
    		for (int x = 0; x < width; x++) {
    			UnityEngine.Color color = new UnityEngine.Color(bitmap[y * width + x], bitmap[y * width + x], bitmap[y * width + x], 1);
    			txt.SetPixel(x, y, color);
    		}
    	txt.Apply();

    	return txt;
    }

    byte[] Filter1(byte[] bitmap, int width, int height, int eps) {	// average around an eps(ilon)
    	byte[] outputImg = new byte[bitmap.Length];

    	float avgFloat = 0;
    	for (int y = 0; y < height; y++) {
    		for (int x = 0; x < width; x++) {
    			avgFloat += (float)bitmap[y * width + x];
    		}
    	}
    	avgFloat /= bitmap.Length;
    	byte avg = (byte)avgFloat;

    	for (int y = 0; y < height; y++) {
    		for (int x = 0; x < width; x++) {
    			if (Mathf.Abs(bitmap[y * width + x] - avg) < eps)
    				outputImg[y * width + x] = avg;
    			else
    				outputImg[y * width + x] = bitmap[y * width + x];
    		}
    	}

    	return outputImg;
    }

    byte[] Filter2(byte[] bitmap, int width, int height, int desiredValue) {	// fill with desired value
    	byte[] outputImg = new byte[bitmap.Length];

    	for (int y = 0; y < height; y++) {
    		for (int x = 0; x < width; x++) {
    			outputImg[y * width + x] = (byte)desiredValue;
    		}
    	}

    	return outputImg;
    }







    void OnEndFrameRendering(ScriptableRenderContext context, Camera[] cameras)
    {
        if (captureAllowed)
        {
            //Create a new texture with the width and height of the screen
            RenderTexture.active = _camera.targetTexture;
            Texture2D texture = new Texture2D(_camera.targetTexture.width, _camera.targetTexture.height, TextureFormat.RGB24, false);

            //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
            texture.ReadPixels(new Rect(0, 0, _camera.targetTexture.width, _camera.targetTexture.height), 0, 0, false);
            texture.Apply();
            RenderTexture.active = null;


            float[] resizedImageArray = EncodeAndResizeImage(texture, imageDimensionX, imageDimensionY);
            byte[] resized8BitImage = UnitImageTo8BitImage(resizedImageArray, imageDimensionX, imageDimensionY);
            //byte[] resized8BitImageF1 = Filter1(resized8BitImage, imageDimensionX, imageDimensionY, 15);
            //byte[] resized8BitImage = BitmapFileTo1ChannelArray(exampleBitmapArray);

            string bmpsFolder = (currentMode == GameModes.Modes.Training)
            	?	"Training"
            	:	"Test";
            //WriteBytesToBitmap(resized8BitImage, @"C:\Users\bob\Documents\DeepLearningPython-master\Samples\ExportedBMPs\" + bmpsFolder + "\\" + numberOfCaptures + ".bmp");
            //WriteBytesToBitmap(resized8BitImage, @"Assets\Samples\ExportedBMPs\" + bmpsFolder + "\\" + numberOfCaptures + ".bmp");

            Texture2D resizedTexture = BitmapToTexture(resizedImageArray, imageDimensionX, imageDimensionY);

	        byte[] jpegData = resizedTexture.EncodeToJPG();

            string directory = (currentMode == GameModes.Modes.Training)
            	?	"Training"
            	:	"Test";

            string captureCategory;	// the name of the captured object (in the case of testing, the name of the "guessed" object)

            if (currentMode == GameModes.Modes.Test) {
            	neuralNetwork.DispatchSample(
	            	currentMode == GameModes.Modes.Training, 	// deci isTraining == false
	            	resized8BitImage
            	);

            	int objId = (int)neuralNetwork.Identify();
            	//Debug.Log("Cred ca este " + idObjs[objId]);
            	showResultText.GetComponent<Text>().text = idObjs[objId];

            	captureCategory = idObjs[objId];
            } else {
	            neuralNetwork.DispatchSample(
	            	currentMode == GameModes.Modes.Training, 	// deci isTraining == true
	            	resized8BitImage, 
	            	objIds[_UIScript.objName],
	            	numberOfCaptures
	            	);

	            captureCategory = _UIScript.objName;
	        }





            if (!captures.ContainsKey(captureCategory))
            {
                captures[captureCategory] = 0;
                /*System.IO.Directory.CreateDirectory("C:\\Users\\bob\\Documents\\Unity Projects\\Learning_Individual_etapa_1\\Assets\\Object captures\\Training\\" 
                	+ captureCategory
                	);
                System.IO.Directory.CreateDirectory("C:\\Users\\bob\\Documents\\Unity Projects\\Learning_Individual_etapa_1\\Assets\\Object captures\\Test\\" 
                	+ captureCategory
                	);*/

                System.IO.Directory.CreateDirectory("Assets\\Object captures\\Training\\"
                    + captureCategory
                    );
                System.IO.Directory.CreateDirectory("Assets\\Object captures\\Test\\"
                    + captureCategory
                    );
            }
            else
                captures[captureCategory]++;

            /*UnityEngine.Windows.File.WriteAllBytes("C:\\Users\\bob\\Documents\\Unity Projects\\Learning_Individual_etapa_1\\Assets\\Object captures\\" 
            	+ directory + "\\"
                + captureCategory + "\\" 
                + captures[captureCategory] 
                + ".jpg", jpegData
                );*/

            /*UnityEngine.Windows.File.WriteAllBytes("Assets\\Object captures\\"
                + directory + "\\"
                + captureCategory + "\\"
                + captures[captureCategory]
                + ".jpg", jpegData
                );*/







            //Reset the grab state
            captureAllowed = false;

            foreach (GameObject o in allGOs)
            {
                MeshRenderer mr = o.GetComponent<MeshRenderer>();
                if (mr != null)
                    mr.enabled = true;
            }
            captureBackground.GetComponent<MeshRenderer>().enabled = false;
            _camera.targetTexture = null;

        	numberOfCaptures++;
        }
    }








    void ReadImg()
    {
        //string filePath = @"C:\Users\bob\Documents\DeepLearningPython-master\Samples\TestSimple4\T0\1.bmp";
        string filePath = @"Assets\Object captures\Samples\TestSimple4\T0\1.bmp";
        exampleBitmapArray = System.IO.File.ReadAllBytes(filePath);
    }

    byte[] BitmapFileTo1ChannelArray(byte[] bitmapFileArray) {
    	byte[] rez = new byte[(bitmapFileArray.Length - 54) / 3];

        for (int i = 0; i < (bitmapFileArray.Length - 54) / 3; i++)
        {
            rez[i] = bitmapFileArray[54 + i * 3];
        }

        return rez;
    }

    void WriteBytesToBitmap(byte[] image8Bit, string filePath) {
    	byte[] bitmapArray = new byte[image8Bit.Length * 3 + 54];

    	for (int i = 0; i < 54; i++) {
    		bitmapArray[i] = exampleBitmapArray[i];
    	}

    	for (int i = 0; i < image8Bit.Length; i++) {
    		bitmapArray[54 + 3 * i] = image8Bit[i];
    		bitmapArray[54 + 3 * i + 1] = image8Bit[i];
    		bitmapArray[54 + 3 * i + 2] = image8Bit[i];
    	}

    	File.WriteAllBytes(filePath, bitmapArray);
    }
}
