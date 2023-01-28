using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using MoonSharp.Interpreter;
using System.IO;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
	public static GameManager instance;
	public string version = "DEVELOPMENT";
	
	/* ----------------------------- Loading screen ----------------------------- */

	public GameObject bootTextObject;
	public GameObject topTextObject;
	public GameObject loadingScreenObject;
	public GameObject bootBackground;
	
	TextMeshProUGUI bootText;
	TextMeshProUGUI topText;
	
	AudioSource audsrc;

	AudioClip bootSound;
	
	string[] bootTextLines;

	string[] loadBars = {"-", "/", "|", "\\"};
	int loadBarFrame = 0;

	#if UNITY_EDITOR
	public bool fastBoot = true;
	#endif

	/* ------------------------------------ - ----------------------------------- */

	/* ---------------------------------- Game ---------------------------------- */
	bool inputServed;

	/* ------------------------------------ - ----------------------------------- */


    // Start is called before the first frame update
    void Start()
    {
		if (instance)
		{	
			Destroy(gameObject);
		} else
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
		

		topTextObject.SetActive(false);
		topText = topTextObject.GetComponent<TextMeshProUGUI>();

		bootTextObject.SetActive(false);
		bootText = bootTextObject.GetComponent<TextMeshProUGUI>();
		
		bootTextLines = bootText.text.Split('\n');		
		
		audsrc = GetComponent<AudioSource>();
		
		bootSound = Resources.Load<AudioClip>("Sound/boot1");
		
		StartCoroutine(IFirstLoad());
    }


	/* -------------------------------------------------------------------------- */
	/*                               Loading Screen                               */
	/* -------------------------------------------------------------------------- */
	#region LOADING_SCREEN
	#if UNITY_EDITOR
	private bool FastBoot() => fastBoot;
	#else
	private bool FastBoot() => false;
	#endif

	public void AdvanceBarFrame(string text, string identifier)
	{
		if (loadBarFrame >= loadBars.Length)
			loadBarFrame = 0;
		
		bootText.text = text.Replace(identifier, loadBars[loadBarFrame]);
		loadBarFrame++;
	}

	IEnumerator IFirstLoad()
	{
		if (!FastBoot()) {yield return new WaitForSeconds(1.0f);}
		if (bootSound)
			audsrc.PlayOneShot(bootSound);
				
		
		//AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game");
		
		if (!FastBoot()) {yield return new WaitForSeconds(5.5f);}
		topTextObject.SetActive(true);
		
		if (!FastBoot()) {yield return new WaitForSeconds(1.0f);}
		bootTextObject.SetActive(true);
		
		bootText.text = "";
		foreach(string line in bootTextLines)
		{
			if (line.Trim().StartsWith("#WAITSPACE"))
			{
				yield return new WaitForSeconds(FastBoot()?0.1f:Random.Range(1f, 3f));
				bootText.text += "\n";
				continue;
			}
			
			if (line.Trim().StartsWith("#WAIT"))
			{
				yield return new WaitForSeconds(FastBoot()?0.1f:Random.Range(1f, 3f));
				continue;
			}
			
			if (line.Trim().StartsWith("#CLEAR"))
			{
				bootText.text = "";
				continue;
			}
			
			if (line.Trim().StartsWith("#SCENE"))
			{
				//yield return StartCoroutine(IWaitForOperation(asyncLoad));
				bootText.text = bootText.text.Replace("@", "OK");
				continue;
			}
			
			if (line.Trim().StartsWith("#DATA"))
			{
				yield return StartCoroutine(ILoadData());
				continue;
			}

			if (line.Trim().StartsWith("#MODS"))
			{
				bootText.text = bootText.text.Replace("@", "NOT SUPPORTED YET"); // TODO: Replace with amount of loaded mods.
				continue;
			}
			
			if (line.Trim().StartsWith("#VERSION"))
			{
				bootText.text = bootText.text.Replace("@", version);
				continue;
			}
			
			if (line.Trim().StartsWith("#LANG"))
			{
				yield return StartCoroutine(ILoadLang());
				continue;
			}

			if (line.Contains("%")) // e.g: %1:OK
			{
				string[] s = line.Trim().Split(':');
				if (s.Length < 2)
					continue;
				s[0] = s[0].Replace("%", "");
				
				float time = 3.0f; 
				bool isFloat = float.TryParse(s[0], out time);
				
				yield return StartCoroutine(ITextLoadAnim("@", time, s[1]));
				continue;
			}
			
			if (line.Contains("@"))
			{
				bootText.text += line  + "\n";
				continue;
			}
			
			bootText.text += L(line)  + "\n";
			yield return new WaitForSeconds(FastBoot()?0.1f:Random.Range(0.2f, 1f));
		}
		
		yield return StartCoroutine(IWaitForInput(KeyCode.Return));
		StartCoroutine(IGameLoop());
		bootTextObject.SetActive(false);
		topTextObject.SetActive(false);
		loadingScreenObject.SetActive(false);
	}
	
	IEnumerator ITextLoadAnim(string identifier, float time, string finalText)
	{
		string buffer = bootText.text;
		float timer = 0f;
		
		while (timer < time)
		{
			AdvanceBarFrame(buffer, identifier);

			timer += 0.5f;
			yield return new WaitForSeconds(FastBoot()? 0.1f : 0.2f);
		}
		
		bootText.text = buffer.Replace(identifier, finalText);
	}
	
	IEnumerator IWaitForInput(KeyCode k)
	{
		while (!Input.GetKeyDown(k))
		{
			yield return null;
		}
	}
	
	IEnumerator IWaitForInputs(KeyCode[] keys)
	{
		bool pressed = false;
		
		while (!pressed)
		{
			foreach(KeyCode k in keys)
			{
				if (Input.GetKeyDown(k))
					pressed = true;
			}
			yield return null;
		}
	}
	
	IEnumerator IWaitForOperation(AsyncOperation op, string identifier = "@", string finalText = "OK")
	{
		string buffer = bootText.text;
		
		while(!op.isDone)
		{
			AdvanceBarFrame(buffer, identifier);
			yield return new WaitForSeconds(0.2f);
		}
		bootText.text = buffer.Replace(identifier, finalText);
		yield return null;
	}
	
	IEnumerator ILoadData(string identifier = "@", string finalText = "OK")
	{
		string buffer = bootText.text;
		bootText.text = buffer.Replace(identifier, "WORKING...");
		yield return StartCoroutine(IMountFolder(Application.streamingAssetsPath, true));

		bool error = true;
		switch(GameData.PostLoadVerify())
		{
			case 0:
				error = false;
				break;
			case 1:
				bootText.text = buffer.Replace(identifier, "ERR: LANG") + "\nNo localization file found for English, which is always required to exist.";
				break;
			case 2:
				bootText.text = buffer.Replace(identifier, "ERR: FACT") + "\nInsufficient faction data.";
				break;
		}

		if (error)
		{
			bootText.text += "\nPress ENTER to quit.";
			yield return StartCoroutine(IWaitForInput(KeyCode.Return));
			Quit();
		}

		bootText.text = buffer.Replace(identifier, finalText);
	}

	IEnumerator IBootError(string txt, string context = "None provided.")
	{
		bootText.text = $"FATAL:\n{txt}\nContext: {context}\n\n\nBoot cannot continue.\nPress ESC, SPACE or ENTER to Quit.";

		if (bootBackground)
		{
			RawImage img = bootBackground.GetComponent<RawImage>();
			img.color = Color.blue;
			bootText.color = Color.white;
		}
		yield return StartCoroutine(IWaitForInputs(new KeyCode[] {KeyCode.Space, KeyCode.Return, KeyCode.Escape}));
		Quit();
	}

	IEnumerator IMountFolder(string folder, bool mods = false)
	{
		if (!Directory.Exists(folder))
		{
			yield break;
		}

		// Folder should contain:
		// Graphics
		// Localization
		// Mods (*IF GAME FOLDER AND NOT A MOD FOLDER*)
		// Scripts
		//  - Contracts
		//  - Equipment
		//	- Events
		//	- Factions
		//	- Locations
		//	- Modifiers

		string fGraphics = folder + "/Graphics";
		string fLocalization = folder + "/Localization";
		string fMods = folder + "/Mods";
		string fScripts = folder + "/Scripts";

		string fContracts = fScripts + "/Contracts";
		string fEquipment = fScripts + "/Equipment";
		string fEvents = fScripts + "/Events";
		string fFactions = fScripts + "/Factions";
		string fLocations = fScripts + "/Locations";
		string fModifiers = fScripts + "/Modifiers";

		if (Directory.Exists(fLocalization))
		{
			Debug.Log($"Importing localization from {fLocalization}...");
			foreach(string file in Directory.GetFiles(fLocalization))
			{
				if (!file.EndsWith(".lua"))
					continue;
				if (!ValidateLua(file))
					continue;
				string name = Path.GetFileNameWithoutExtension(file).ToUpper();
				string errormsg = "";
				bool error = false;
				DynValue v = new DynValue();
				try
				{
					Script script = new Script();
					script.FillDefaults();
					v = script.DoString(File.ReadAllText(file));
				}
				catch(InterpreterException ex)
				{
					errormsg = ex.DecoratedMessage;
					error = true;
				}

				if (error)
					yield return StartCoroutine(IBootError(errormsg, $"\nFile: {file}\nFilename:{name}\nContext: Localization Parse"));

				
				if (v.Type == DataType.Table)
				{
					GameData.RegisterLanguage(name, v.Table);
				}
			}
			yield return null;
		}

		if (Directory.Exists(fGraphics))
		{
			Debug.Log($"Importing graphics from {fGraphics}...");
		}

		if (Directory.Exists(fContracts))
		{
			Debug.Log($"Importing contracts from {fContracts}...");
		}

		if (Directory.Exists(fEquipment))
		{
			Debug.Log($"Importing equipment from {fEquipment}...");
		}

		if (Directory.Exists(fEvents))
		{
			Debug.Log($"Importing events from {fEvents}...");
		}

		if (Directory.Exists(fFactions))
		{
			Debug.Log($"Importing factions from {fFactions}...");
			foreach(string file in Directory.GetFiles(fFactions))
			{
				if (!file.EndsWith(".lua"))
					continue;
				if (!ValidateLua(file))
					continue;
				bool error = false;
				string errormsg = "";
				DynValue factions = new DynValue();
				try
				{
					Script script = new Script();
					script.FillDefaults();
					factions = script.DoString(File.ReadAllText(file));
				} catch (InterpreterException ex)
				{
					error = true;
					errormsg = ex.DecoratedMessage;
				}

				if (error)
					yield return StartCoroutine(IBootError(errormsg, $"\nFile: {file}\nFilename:{name}\nContext: Faction Parse"));
				
				if (factions.Type == DataType.Table)
				{
					Table t = factions.Table;
					foreach(DynValue id in t.Keys)
					{
						DynValue faction = t.Get(id);

						if (faction.Type == DataType.Table)
						{
							GameData.RegisterFaction(id.String, faction.Table);
						}
					}
				}
			}
		}

		if (Directory.Exists(fLocations))
		{
			Debug.Log($"Importing locations from {fLocations}...");
		}

		if (Directory.Exists(fModifiers))
		{
			Debug.Log($"Importing modifiers from {fModifiers}...");
		}

		if (Directory.Exists(fMods))
		{
			Debug.Log($"Importing mods from {fMods}...");
			//TODO: Load mods.
		}
	}

	public static bool ValidateLua(string file)
	{
		if (!File.Exists(file))
			return false;

		if (!file.EndsWith(".lua"))
			return false;

		Script temp = new Script();
		try
		{
			temp.DoString(File.ReadAllText(file));
		}
		catch(InterpreterException ex)
		{
			Debug.Log($"{file} has failed validation due to lua errors:{ex.DecoratedMessage}");
			return false;
		}

		return true;
	}

	IEnumerator ILoadLang(string identifier = "@", string finalText = "OK")
	{
		string buffer = bootText.text;
		bool loaded = true;
		
		while(!loaded)
		{
			AdvanceBarFrame(buffer, identifier);
			yield return new WaitForSeconds(0.2f);
		}
		bootText.text = buffer.Replace(identifier, finalText);
		yield return null;
	}
	#endregion LOADING_SCREEN
	/* ------------------------------------ - ----------------------------------- */

	/* -------------------------------------------------------------------------- */
	/*                                    Game                                    */
	/* -------------------------------------------------------------------------- */


	IEnumerator IGameLoop()
	{
		// Current screen. Is a string for maybe future custom mod capabilities.
		string screen = "";
		bool exit = false;
		while(!exit)
		{
			switch(screen.ToLower().Trim())
			{
				case "menu":

				break;
			}
			yield return new WaitForEndOfFrame();
		}
		Quit();
	}

	enum InputType {
		Any,
		AnyKey,
		AnyNumber,
		AnyLetter,
		AnyString,
		ModeratedString,
	}
	IEnumerator IWaitForGameInput(InputType type)
	{
		bool pressed = false;
		switch(type)
		{
			case InputType.Any:
			case InputType.AnyKey:
				while (!Input.anyKey)
					yield return null;
			break;
			case InputType.AnyNumber:
				KeyCode[] nums = {
						KeyCode.Alpha1,
						KeyCode.Alpha2,
						KeyCode.Alpha3,
						KeyCode.Alpha4,
						KeyCode.Alpha5,
						KeyCode.Alpha6,
						KeyCode.Alpha7,
						KeyCode.Alpha8,
						KeyCode.Alpha9,
					};

				

				while(!pressed)
				{
					foreach(KeyCode key in nums)
					{
						if (Input.GetKeyDown(key))
							pressed = true;
					}
					yield return null;
				}
			break;
			case InputType.AnyLetter:
				KeyCode[] alpha = {
					KeyCode.A,
					KeyCode.B,
					KeyCode.C,
					KeyCode.D,
					KeyCode.E,
					KeyCode.F,
					KeyCode.G,
					KeyCode.H,
					KeyCode.I,
					KeyCode.J,
					KeyCode.K,
					KeyCode.L,
					KeyCode.M,
					KeyCode.N,
					KeyCode.O,
					KeyCode.P,
					KeyCode.Q,
					KeyCode.R,
					KeyCode.S,
					KeyCode.T,
					KeyCode.U,
					KeyCode.V,
					KeyCode.W,
					KeyCode.X,
					KeyCode.Y,
					KeyCode.Z
				};

				while(!pressed)
				{
					foreach(KeyCode key in alpha)
					{
						if (Input.GetKeyDown(key))
							pressed = true;
					}
					yield return null;
				}
			break;
		}
	}

	/* -------------------------------------------------------------------------- */
	/*                                   Static                                   */
	/* -------------------------------------------------------------------------- */
	public static void Quit()
	{
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}
	public static string L(string s) => GameData.L(s);
	public static void Log(string s) {Debug.Log("Lua: " + s);}

	/* ------------------------------------ - ----------------------------------- */
}
