/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2023
 *	
 *	"GVar.cs"
 * 
 *	This script is a data class for project-wide variables.
 * 
 */

using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * A data class for global and local variables.
	 * Variables are created in the Variables Manager asset file, and copied to the RuntimeVariables component when the game begins.
	 */
	[System.Serializable]
	public class GVar : ITranslatable
	{

		#region Variables

		/** Its editor name. */
		public string label;
		/** Its internal ID number. */
		public int id;
		/** Its variable type. */
		public VariableType type;
		[SerializeField] protected int val = 0;
		[SerializeField] protected float floatVal = 0f;
		[SerializeField] protected string textVal = "";
		[SerializeField] protected GameObject gameObjectVal;
		[SerializeField] protected GameObjectParameterReferences gameObjectSaveReferences = GameObjectParameterReferences.ReferenceSceneInstance;
		[SerializeField] protected Object objectVal;
		/** An array of labels, if a popup. */
		public string[] popUps;
		/** Its value, if a Vector3 */
		[SerializeField] protected Vector3 vector3Val = new Vector3 (0f, 0f, 0f);
		/** What it links to, if a Global or Compnent Variable.  A Variable can link to Options Data, or a Playmaker Variable. */
		public VarLink link = VarLink.None;
		/** If linked to a Playmaker Variable, the name of the PM variable. */
		public string pmVar;
		/** If True and the variable is linked to a custom script, the script will be referred to for the initial value. */
		public bool updateLinkOnStart = false;
		/** If True, the variable's value can be translated (if PopUp or String) */
		public bool canTranslate = true;
		/** The ID number of the shared popup labels, if more than zero */
		public int popUpID = 0;
		
		/** The translation ID number of the variable's string value (if type == VariableType.String), as generated by SpeechManager */
		public int textValLineID = -1;
		/** The translation ID number of the variables's PopUp values (if type == VariableType.PopUp), as generated by SpeechManager */
		public int popUpsLineID = -1;

		private float backupFloatVal;
		private int backupVal;

		protected string[] runtimeTranslations;

		#if UNITY_EDITOR
		public string description = "";
		public bool showInFilter;
		protected static string[] boolType = { "False", "True" };
		#endif

		#endregion


		#region Constructors

		public GVar ()
		{}
		
		
		/**
		 * The main Constructor.
		 * An array of ID numbers is required, to ensure its own ID is unique.
		 */
		public GVar (int[] idArray)
		{
			val = 0;
			floatVal = 0f;
			textVal = string.Empty;
			type = VariableType.Boolean;
			link = VarLink.None;
			pmVar = string.Empty;
			popUps = null;
			updateLinkOnStart = false;
			backupVal = 0;
			backupFloatVal = 0f;
			textValLineID = -1;
			popUpsLineID = -1;
			canTranslate = true;
			vector3Val = Vector3.zero;
			popUpID = 0;
			gameObjectVal = null;
			gameObjectSaveReferences = GameObjectParameterReferences.ReferenceSceneInstance;
			objectVal = null;

			AssignUniqueID (idArray);

			label = "Variable " + (id + 1).ToString ();

			#if UNITY_EDITOR
			description = string.Empty;
			#endif
		}
		

		/**
		 * A Constructor that copies all values from another variable.
		 * This way ensures that no connection remains to the asset file.
		 */
		public GVar (GVar assetVar)
		{
			val = assetVar.val;
			floatVal = assetVar.floatVal;
			textVal = assetVar.textVal;
			type = assetVar.type;
			id = assetVar.id;
			label = assetVar.label;
			link = assetVar.link;
			pmVar = assetVar.pmVar;
			popUps = assetVar.popUps;
			updateLinkOnStart = assetVar.updateLinkOnStart;
			backupVal = assetVar.val;
			backupFloatVal = assetVar.floatVal;
			textValLineID = assetVar.textValLineID;
			popUpsLineID = assetVar.popUpsLineID;
			canTranslate = assetVar.canTranslate;
			vector3Val = assetVar.vector3Val;
			popUpID = assetVar.popUpID;
			gameObjectVal = assetVar.gameObjectVal;
			gameObjectSaveReferences = assetVar.gameObjectSaveReferences;
			objectVal = assetVar.objectVal;
			if (type == VariableType.GameObject) textVal = (gameObjectVal) ? gameObjectVal.name : string.Empty;
			if (type == VariableType.UnityObject) textVal = (objectVal) ? objectVal.name : string.Empty;

			#if UNITY_EDITOR
			description = assetVar.description;
			#endif
		}

		#endregion


		#region PublicFunctions

		/**
		 * <summary>Sets the internal ID to a unique number based on an array of previously-used values</summary>
		 * <param name = "idArray">An array of previously-used ID values</param>
		 * <returns>The new ID number</returns>>
		 */
		public int AssignUniqueID (int[] idArray)
		{
			id = 0;

			if (idArray != null)
			{
				foreach (int _id in idArray)
				{
					if (id == _id)
					{
						id ++;
					}
				}
			}
			return id;
		}
		
		
		/**
		 * <summary>Sets its value to that of its linked variable (if appropriate).</summary>
		 * <param name = "_location">The variable's location</param>
		 * <param name = "_variables">The variable's Variables component, if location = VariableLocation.Component</param>
		 */
		public void Download (VariableLocation _location = VariableLocation.Global, Variables _variables = null)
		{
			if (_location == VariableLocation.Local) return;
			
			if (link == VarLink.PlaymakerVariable && !string.IsNullOrEmpty (pmVar))
			{
				if (!PlayMakerIntegration.IsDefinePresent ())
				{
					return;
				}

				if (_location != VariableLocation.Component) _variables = null;
				if (_location == VariableLocation.Component && _variables == null)
				{
					return;
				}

				switch (type)
				{
					case VariableType.Integer:
					case VariableType.PopUp:
						IntegerValue = PlayMakerIntegration.GetInt (pmVar, _variables);
						break;

					case VariableType.Boolean:
						BooleanValue = PlayMakerIntegration.GetBool (pmVar, _variables);
						break;

					case VariableType.String:
						TextValue = PlayMakerIntegration.GetString (pmVar, _variables);
						break;

					case VariableType.Float:
						FloatValue = PlayMakerIntegration.GetFloat (pmVar, _variables);
						break;

					case VariableType.Vector3:
						Vector3Value = PlayMakerIntegration.GetVector3 (pmVar, _variables);
						break;

					case VariableType.GameObject:
						GameObjectValue = PlayMakerIntegration.GetGameObject (pmVar, _variables);
						break;

					case VariableType.UnityObject:
						UnityObjectValue = PlayMakerIntegration.GetObject (pmVar, _variables);
						break;

					default:
						break;
				}
			}
			else if (link == VarLink.CustomScript && KickStarter.eventManager)
			{
				KickStarter.eventManager.Call_OnDownloadVariable (this, _variables);
			}
		}
		
		
		/**
		 * <summary>Sets the value of its linked variable to its value (if appropriate).</summary>
		 * <param name = "_location">The variable's location</param>
		 * <param name = "_variables">The variable's Variables component, if location = VariableLocation.Component</param>
		 */
		public void Upload (VariableLocation _location = VariableLocation.Global, Variables _variables = null)
		{
			if (_location == VariableLocation.Local) return;

			if (link == VarLink.PlaymakerVariable && !string.IsNullOrEmpty (pmVar))
			{
				if (!PlayMakerIntegration.IsDefinePresent ())
				{
					return;
				}

				if (_location != VariableLocation.Component) _variables = null;
				if (_location == VariableLocation.Component && _variables == null)
				{
					return;
				}

				switch (type)
				{
					case VariableType.Integer:
					case VariableType.PopUp:
						PlayMakerIntegration.SetInt (pmVar, val, _variables);
						break;

					case VariableType.Boolean:
						PlayMakerIntegration.SetBool (pmVar, (val == 1), _variables);
						break;

					case VariableType.String:
						PlayMakerIntegration.SetString (pmVar, textVal, _variables);
						break;

					case VariableType.Float:
						PlayMakerIntegration.SetFloat (pmVar, floatVal, _variables);
						break;

					case VariableType.Vector3:
						PlayMakerIntegration.SetVector3 (pmVar, vector3Val, _variables);
						break;

					case VariableType.GameObject:
						PlayMakerIntegration.SetGameObject (pmVar, gameObjectVal, _variables);
						break;

					case VariableType.UnityObject:
						PlayMakerIntegration.SetObject (pmVar, objectVal, _variables);
						break;

					default:
						break;
				}
			}
			else if (link == VarLink.OptionsData)
			{
				Options.SavePrefs ();
			}
			else if (link == VarLink.CustomScript && KickStarter.eventManager)
			{
				KickStarter.eventManager.Call_OnUploadVariable (this, _variables);
			}
		}
		

		/** Backs up its value. Necessary when skipping ActionLists that involve checking variable values. */
		public void BackupValue ()
		{
			backupVal = val;
			backupFloatVal = floatVal;
		}
		
		
		/** Restores its value from backup. Necessary when skipping ActionLists that involve checking variable values. */
		public void RestoreBackupValue ()
		{
			val = backupVal;
			floatVal = backupFloatVal;
		}
		
		
		/**
		 * <summary>Sets the value if its type is String.</summary>
		 * <param name = "newValue">The new value of the string</param>
		 * <param name = "newLineID">If >=0, the translation ID used by SpeechManager / RuntimeLanguages will be updated to this value</param>
		 */
		public void SetStringValue (string newValue, int newLineID = -1)
		{
			TextValue = newValue;

			if (type == VariableType.String && newLineID >= 0)
			{
				textValLineID = newLineID;
				CreateRuntimeTranslations ();
			}
		}
		

		/** Transfers translation data from RuntimeLanguages to the variable itself. This allows it to be transferred to other variables with the 'Variable: Copy' Action. */
		public void CreateRuntimeTranslations ()
		{
			if (!Application.isPlaying || KickStarter.runtimeLanguages == null) return;

			runtimeTranslations = null;

			if (HasTranslations ())
			{
				if (type == VariableType.String && canTranslate)
				{
					runtimeTranslations = KickStarter.runtimeLanguages.GetTranslations (textValLineID);
				}
				else if (type == VariableType.PopUp)
				{
					int translationID = -1;
					if (popUpID > 0)
					{
						PopUpLabelData popUpLabelData = KickStarter.variablesManager.GetPopUpLabelData (popUpID);
						if (popUpLabelData != null && popUpLabelData.CanTranslate ())
						{
							translationID = popUpLabelData.LineID;
							canTranslate = true;
						}
					}
					else if (canTranslate)
					{
						translationID = popUpsLineID;
					}
					runtimeTranslations = KickStarter.runtimeLanguages.GetTranslations (translationID);
				}
			}
		}


		/**
		 * <summary>Gets the variable's translations, if they exist.</summary>
		 * <returns>The variable's translations, if they exist, as an array.</summary>
		 */
		public string[] GetTranslations ()
		{
			return runtimeTranslations;
		}


		/**
		 * <summary>Copies the value of another variable onto itself.</summary>
		 * <param name = "oldVar">The variable to copy from</param>
		 * <param name = "oldLocation">The location of the variable to copy (Global, Local)</param>
		 */
		public void CopyFromVariable (GVar oldVar, VariableLocation oldLocation)
		{
			if (oldLocation == VariableLocation.Global)
			{
				oldVar.Download (oldLocation);
			}

			switch (type)
			{
				case VariableType.Float:
					{
						float oldValue = oldVar.FloatValue;

						if (oldVar.type == VariableType.Integer || oldVar.type == VariableType.Boolean || oldVar.type == VariableType.PopUp)
						{
							oldValue = (float) oldVar.IntegerValue;
						}
						else if (oldVar.type == VariableType.String)
						{
							float.TryParse (oldVar.textVal, out oldValue);
						}

						FloatValue = oldValue;
						break;
					}

				case VariableType.String:
					{
						string oldValue = oldVar.GetValue ();
						textVal = oldValue;

						if (oldVar.HasTranslations ())
						{
							runtimeTranslations = oldVar.GetTranslations ();
						}
						else
						{
							runtimeTranslations = null;
						}
						break;
					}

				case VariableType.Vector3:
					{
						Vector3 oldValue = oldVar.vector3Val;
						Vector3Value = oldValue;
						break;
					}

				case VariableType.GameObject:
					{
						GameObject oldValue = oldVar.GameObjectValue;
						GameObjectValue = oldValue;
						break;
					}

				case VariableType.UnityObject:
					{
						Object oldValue = oldVar.UnityObjectValue;
						UnityObjectValue = oldValue;
						break;
					}

				default:
					{
						int oldValue = oldVar.val;

						if (oldVar.type == VariableType.Float)
						{
							oldValue = (int)oldVar.floatVal;
						}
						else if (oldVar.type == VariableType.String)
						{
							float oldValueAsFloat = 0f;
							float.TryParse (oldVar.textVal, out oldValueAsFloat);
							oldValue = (int)oldValueAsFloat;
						}

						if (type == VariableType.PopUp && oldVar.HasTranslations ())
						{
							runtimeTranslations = oldVar.GetTranslations ();
						}
						else
						{
							runtimeTranslations = null;
						}

						IntegerValue = oldValue;
						break;
					}
			}
		}
		
		
		/**
		 * <summary>Returns the variable's value.</summary>
		 * <returns>The value, as a formatted string.</returns>
		 */
		public string GetValue (int languageNumber = 0)
		{
			if (!canTranslate)
			{
				languageNumber = 0;
			}

			switch (type)
			{
				case VariableType.Integer:
					return IntegerValue.ToString ();

				case VariableType.PopUp:
					return GetPopUpForIndex (val, languageNumber);

				case VariableType.Float:
					return FloatValue.ToString ();

				case VariableType.Boolean:
					return BooleanValue.ToString ();

				case VariableType.Vector3:
					return "(" + Vector3Value.x.ToString () + ", " + Vector3Value.y.ToString () + ", " + Vector3Value.z.ToString () + ")";

				case VariableType.String:
					if (languageNumber > 0)
					{
						if (runtimeTranslations == null)
						{
							CreateRuntimeTranslations ();
						}
						if (runtimeTranslations != null && runtimeTranslations.Length >= languageNumber)
						{
							return runtimeTranslations[languageNumber - 1];
						}
					}
					return textVal;

				case VariableType.GameObject:
					return (gameObjectVal) ? gameObjectVal.name : string.Empty;

				case VariableType.UnityObject:
					return (objectVal) ? objectVal.name : string.Empty;

				default:
					return string.Empty;
			}
		}


		/**
		 * <summary>Gets all possible PopUp values as a single string, where the values are separated by a ']' character.</summary>
		 * <returns>All possible PopUp values as a single string, where the values are separated by a ']' character.</returns>
		 */
		public string GetPopUpsString ()
		{
			if (popUpID > 0)
			{
				PopUpLabelData popUpLabelData = KickStarter.variablesManager.GetPopUpLabelData (popUpID);
				if (popUpLabelData != null)
				{
					return popUpLabelData.GetPopUpsString ();
				}
				return string.Empty;
			}

			string result = string.Empty;
			foreach (string popUp in popUps)
			{
				result += popUp + "]";
			}
			if (result.Length > 0)
			{
				return result.Substring (0, result.Length-1);
			}
			return string.Empty;
		}


		/**
		 * <summary>Checks if the Variable is translatable.</summary>
		 * <returns>True if the Variable is translatable</returns>
		 */
		public bool HasTranslations ()
		{
			if (type == VariableType.String || type == VariableType.PopUp)
			{
				return canTranslate;
			}
			return false;
		}


		/**
		 * <summary>Checks if this Variable is defined under the Variable Manager's list of Global Variables</summary>
		 * <returns>True if the variable is Global</returns> 
		 */
		public bool IsGlobalVariable ()
		{
			foreach (GVar gVar in KickStarter.runtimeVariables.globalVars)
			{
				if (gVar == this)
				{
					return true;
				}
			}
			return false;
		}


		/**
		 * <summary>Gets the number of possible values, if of the type PopUp</summary>
		 * <returns>The number of possible values, if of the type PopUp</returns>
		 */
		public int GetNumPopUpValues ()
		{
			if (popUpID <= 0)
			{
				if (popUps != null)
				{
					return popUps.Length;
				}
			}
			else if (KickStarter.variablesManager)
			{
				PopUpLabelData popUpLabelData = KickStarter.variablesManager.GetPopUpLabelData (popUpID);
				if (popUpLabelData != null)
				{
					return popUpLabelData.Length;
				}
			}
			return 0;
		}


		/**
		 * <summary>Gets the PopUp value of a given index and language, if of type PopUp</summary>
		 * <param name = "index">The index of the PopUp labels</param>
		 * <param name = "language">The language index</param>
		 * <returs>The PopUp value of a given index and language</returns>
		 */
		public string GetPopUpForIndex (int index, int language = 0)
		{
			if (index >= 0)
			{
				if (language > 0 && runtimeTranslations == null)
				{
					CreateRuntimeTranslations ();
				}
				if (language > 0 && runtimeTranslations != null && runtimeTranslations.Length >= language)
				{
					string popUpsString = runtimeTranslations[language-1];
					string[] popUpsNew = popUpsString.Split ("]"[0]);
					if (index < popUpsNew.Length)
					{
						return popUpsNew[index];
					}
				}
				else if (popUpID > 0)
				{
					PopUpLabelData popUpLabelData = KickStarter.variablesManager.GetPopUpLabelData (popUpID);
					if (popUpLabelData != null)
					{
						return popUpLabelData.GetValue (index);
					}
				}
				else if (popUps != null && index < popUps.Length)
				{
					return popUps[index];
				}
			}
			return string.Empty;
		}


		public void AssignPreset (PresetValue presetValue)
		{
			switch (type)
			{
				case VariableType.Float:
					FloatValue = presetValue.floatVal;
					break;

				case VariableType.String:
					TextValue = presetValue.textVal;
					break;

				case VariableType.Vector3:
					Vector3Value = presetValue.vector3Val;
					break;

				case VariableType.GameObject:
					GameObjectValue = presetValue.gameObjectVal;
					break;

				case VariableType.UnityObject:
					UnityObjectValue = presetValue.objectVal;
					break;

				default:
					IntegerValue = presetValue.val;
					break;
			}
		}


		public override string ToString ()
		{
			if (!string.IsNullOrEmpty (label))
			{
				return type + " Variable ID " + id + "; " + label;
			}
			return type + " Variable ID " + id;
		}

		#endregion


		#region GetSet

		/** Its value, if an integer. */
		public int IntegerValue
		{
			get
			{
				return val;
			}
			set
			{
				int originalValue = val;

				if (type == VariableType.PopUp)
				{
					value = Mathf.Clamp (value, 0, GetNumPopUpValues () - 1);
				}

				val = value;

				if (originalValue != val && Application.isPlaying && KickStarter.eventManager)
				{
					KickStarter.eventManager.Call_OnVariableChange (this);
				}
			}
		}


		/** Its value, if a PopUp */
		public string PopUpValue
		{
			get
			{
				return GetPopUpForIndex (IntegerValue, Options.GetLanguage ());
			}
		}


		/** Its value, if a boolean. */
		public bool BooleanValue
		{
			get
			{
				return (val == 1);
			}
			set
			{
				int originalValue = val;
				val = (value) ? 1 : 0;

				if (originalValue != val && Application.isPlaying && KickStarter.eventManager)
				{
					KickStarter.eventManager.Call_OnVariableChange (this);
				}
			}
		}


		/** Its value, if a float. */
		public float FloatValue
		{
			get
			{
				return floatVal;
			}
			set
			{
				float originalValue = floatVal;

				floatVal = value;

				if (!Mathf.Approximately (originalValue, floatVal) && Application.isPlaying && KickStarter.eventManager)
				{
					KickStarter.eventManager.Call_OnVariableChange (this);
				}
			}
		}


		/** Its value, if a string. */
		public string TextValue
		{
			get
			{
				return textVal;
			}
			set
			{
				string originalValue = textVal;

				textVal = value;

				if (originalValue != textVal && Application.isPlaying && KickStarter.eventManager)
				{
					KickStarter.eventManager.Call_OnVariableChange (this);
				}
			}
		}


		/** Its value, if a Vector3. */
		public Vector3 Vector3Value
		{
			get
			{
				return vector3Val;
			}
			set
			{
				Vector3 originalValue = vector3Val;

				vector3Val = value;

				if (originalValue != vector3Val && Application.isPlaying && KickStarter.eventManager)
				{
					KickStarter.eventManager.Call_OnVariableChange (this);
				}
			}
		}


		/** Its value, if a GameObject. */
		public GameObject GameObjectValue
		{
			get
			{
				return gameObjectVal;
			}
			set
			{
				GameObject originalValue = gameObjectVal;

				gameObjectVal = value;
				textVal = (gameObjectVal) ? gameObjectVal.name : string.Empty;

				if (originalValue != gameObjectVal && Application.isPlaying && KickStarter.eventManager)
				{
					KickStarter.eventManager.Call_OnVariableChange (this);
				}
			}
		}


		public bool SavePrefabReference
		{
			get
			{
				return (type == VariableType.GameObject && gameObjectSaveReferences == GameObjectParameterReferences.ReferencePrefab);
			}
		}


		/** Its value, if a Unity Object. */
		public Object UnityObjectValue
		{
			get
			{
				return objectVal;
			}
			set
			{
				Object originalValue = objectVal;

				objectVal = value;
				textVal = (objectVal) ? objectVal.name : string.Empty;

				if (originalValue != objectVal && Application.isPlaying && KickStarter.eventManager)
				{
					KickStarter.eventManager.Call_OnVariableChange (this);
				}
			}
		}

		#endregion


		#region ITranslatable

		public virtual string GetTranslatableString (int index)
		{
			if (type == VariableType.String)
			{
				return textVal;
			}
			else if (type == VariableType.PopUp)
			{
				return GetPopUpsString ();
			}
			return string.Empty;
		}


		public virtual int GetTranslationID (int index)
		{
			if (type == VariableType.String)
			{
				return textValLineID;
			}
			else
			{
				return popUpsLineID;
			}
		}
		

		public virtual AC_TextType GetTranslationType (int index)
		{
			return AC_TextType.Variable;
		}


		#if UNITY_EDITOR

		public void UpdateTranslatableString (int index, string updatedText)
		{
			if (type == VariableType.String)
			{
				textVal = updatedText;
			}
			else if (type == VariableType.PopUp)
			{
				if (popUpID > 0)
				{
					PopUpLabelData popUpLabelData = KickStarter.variablesManager.GetPopUpLabelData (popUpID);
					if (popUpLabelData != null)
					{
						popUpLabelData.UpdateTranslatableString (index, updatedText);
					}
				}
				else
				{
					string[] updatedLabels = updatedText.Split ("]"[0]);
					if (updatedLabels.Length > 0 && popUps.Length == updatedLabels.Length)
					{
						for (int i=0; i<updatedLabels.Length; i++)
						{
							popUps[i] = updatedLabels[i];
						}
					}
					else
					{
						ACDebug.LogWarning ("Cannot update PopUp labels for Variable '" + id + ": " + label + "' due to mismatching arrray.");
					}
				}
			}
		}


		public int GetNumTranslatables ()
		{
			return 1;
		}


		public virtual bool HasExistingTranslation (int index)
		{
			if (type == VariableType.String)
			{
				return textValLineID > -1;
			}
			else if (type == VariableType.PopUp)
			{
				return popUpsLineID > -1;
			}
			return false;
		}


		public virtual void SetTranslationID (int index, int _lineID)
		{
			if (type == VariableType.String)
			{
				textValLineID = _lineID;
			}
			else if (type == VariableType.PopUp)
			{
				popUpsLineID = _lineID;
			}
		}


		public string GetOwner (int index)
		{
			return string.Empty;
		}


		public bool OwnerIsPlayer (int index)
		{
			return false;
		}


		public virtual bool CanTranslate (int index)
		{
			if (canTranslate)
			{
				if (type == VariableType.String)
				{
					return !string.IsNullOrEmpty (textVal);
				}
				else if (type == VariableType.PopUp && popUpID <= 0)
				{
					return !string.IsNullOrEmpty (GetPopUpsString ());
				}
			}
			return false;
		}


		public string[] GenerateEditorPopUpLabels ()
		{
			string[] popUpLabels = new string[GetNumPopUpValues ()];
			for (int i=0; i<popUpLabels.Length; i++)
			{
				popUpLabels[i] = GetPopUpForIndex (i);
				if (string.IsNullOrEmpty (popUpLabels[i]))
				{
					popUpLabels[i] = "(Unnamed)";
				}
			}

			return popUpLabels;
		}

		#endif

		#endregion

		#if UNITY_EDITOR

		public void ShowGUI (VariableLocation location, bool canEdit, List<VarPreset> _varPresets = null, string apiPrefix = "", Variables _variables = null)
		{
			string labelPrefix = (canEdit) ? "Initial value:" : "Current value:";
			string helpText = (canEdit) ? "Its initial value" : "Its current value";

			if (!canEdit && HasTranslations () && Options.GetLanguage () > 0)
			{
				labelPrefix = "Original language value:";
			}

			if (canEdit)
			{
				label = CustomGUILayout.TextField ("Label:", label, apiPrefix + ".label", "Its editor name");
				type = (VariableType)CustomGUILayout.EnumPopup ("Type:", type, apiPrefix + ".type", "Its variable type");
			}
			else
			{
				EditorGUILayout.LabelField ("Label: " + label);
				EditorGUILayout.LabelField ("Type: " + type.ToString ());
			}

			switch (type)
			{
				case VariableType.Boolean:
					if (val != 1)
					{
						val = 0;
					}
					val = CustomGUILayout.Popup (labelPrefix, val, boolType, apiPrefix + ".BooleanValue", helpText);
					break;

				case VariableType.Float:
					floatVal = CustomGUILayout.FloatField (labelPrefix, floatVal, apiPrefix + ".FloatValue", helpText);
					break;

				case VariableType.Integer:
					val = CustomGUILayout.IntField (labelPrefix, val, apiPrefix + ".IntegerValue", helpText);
					break;

				case VariableType.PopUp:
					Object objectToRecord = null;
					if (location == VariableLocation.Global) objectToRecord = KickStarter.variablesManager;
					if (location == VariableLocation.Local) objectToRecord = KickStarter.localVariables;
					if (location == VariableLocation.Component) objectToRecord = _variables;

					VariablesManager.ShowPopUpLabelsGUI (this, canEdit, objectToRecord);

					if (GetNumPopUpValues () > 0)
					{
						string[] popUpLabels = GenerateEditorPopUpLabels ();
						val = CustomGUILayout.Popup (labelPrefix, val, popUpLabels, apiPrefix + ".IntegerValue", helpText);
					}
					else
					{
						val = 0;
					}

					if (popUpID > 0)
					{
						if (Application.isPlaying && canTranslate)
						{
							EditorGUILayout.LabelField ("Values can be translated");
						}
					}
					else
					{
						if (canEdit)
						{
							canTranslate = CustomGUILayout.Toggle ("Values can be translated?", canTranslate, apiPrefix + ".canTranslate", "If True, the variable's value can be translated");
						}
						else if (canTranslate)
						{
							EditorGUILayout.LabelField ("Values can be translated");
						}
					}
					break;

				case VariableType.String:
					textVal = CustomGUILayout.TextArea (labelPrefix, textVal, apiPrefix + ".TextValue");

					if (canEdit)
					{
						canTranslate = CustomGUILayout.Toggle ("Values can be translated?", canTranslate, apiPrefix + ".canTranslate", "If True, the variable's value can be translated");
					}
					else if (canTranslate)
					{
						EditorGUILayout.LabelField ("Values can be translated");
					}
					break;

				case VariableType.Vector3:
					vector3Val = CustomGUILayout.Vector3Field (labelPrefix, vector3Val, apiPrefix + ".Vector3Value", helpText);
					break;

				case VariableType.GameObject:
					gameObjectVal = (GameObject) CustomGUILayout.ObjectField <GameObject> (labelPrefix, gameObjectVal, (location != VariableLocation.Global), apiPrefix + ".GameObejctValue", helpText);
					if (location == VariableLocation.Local || location == VariableLocation.Component)
					{
						gameObjectSaveReferences = (GameObjectParameterReferences) CustomGUILayout.EnumPopup ("When saving data:", gameObjectSaveReferences, apiPrefix + ".gameObjectSaveReferences", "Whether to rely on the prefab's name (and search assets by this name), or the scene object's Constant ID number, when saving");
					}
					break;

				case VariableType.UnityObject:
					objectVal = CustomGUILayout.ObjectField <Object> (labelPrefix, objectVal, false);
					break;

				default:
					break;
			}

			switch (location)
			{
				case VariableLocation.Global:
					CustomGUILayout.TokenLabel ("[var:" + id.ToString () + "]");
					break;

				case VariableLocation.Local:
					CustomGUILayout.TokenLabel ("[localvar:" + id.ToString () + "]");
					break;

				case VariableLocation.Component:
					if (_variables)
					{
						ConstantID _constantID = _variables.GetComponent<ConstantID> ();
						if (_constantID && _constantID.constantID != 0)
						{
							CustomGUILayout.TokenLabel ("[compvar:" + _constantID.constantID.ToString () + ":" + id.ToString () + "]");
						}
					}
					break;
			}

			if (_varPresets != null)
			{
				EditorGUILayout.Space ();
				foreach (VarPreset _varPreset in _varPresets)
				{
					// Local
					string apiPrefix2 = (location == VariableLocation.Local) ?
										"AC.KickStarter.localVariables.GetPreset (" + _varPreset.ID + ").GetPresetValue (" + id + ")" :
										"AC.KickStarter.runtimeVariables.GetPreset (" + _varPreset.ID + ").GetPresetValue (" + id + ")";

					_varPreset.UpdateCollection (this);

					string label = "'" +
									(!string.IsNullOrEmpty (_varPreset.label) ? _varPreset.label : ("Preset #" + _varPreset.ID.ToString ())) +
									"' value:";

					PresetValue presetValue = _varPreset.GetPresetValue (this);
					switch (type)
					{
						case VariableType.Boolean:
							presetValue.val = CustomGUILayout.Popup (label, presetValue.val, boolType, apiPrefix2 + ".BooleanValue");
							break;

						case VariableType.Float:
							presetValue.floatVal = CustomGUILayout.FloatField (label, presetValue.floatVal, apiPrefix2 + ".FloatValue");
							break;

						case VariableType.Integer:
							presetValue.val = CustomGUILayout.IntField (label, presetValue.val, apiPrefix2 + ".IntegerValue");
							break;

						case VariableType.PopUp:
							if (popUpID > 0)
							{
								PopUpLabelData popUpLabelData = KickStarter.variablesManager.GetPopUpLabelData (popUpID);
								if (popUpLabelData != null)
								{
									presetValue.val = CustomGUILayout.Popup (label, presetValue.val, popUpLabelData.Labels, apiPrefix2 + ".IntegerValue");
								}
							}
							else
							{
								presetValue.val = CustomGUILayout.Popup (label, presetValue.val, popUps, apiPrefix2 + ".IntegerValue");
							}
							break;

						case VariableType.String:
							presetValue.textVal = CustomGUILayout.TextField (label, presetValue.textVal, apiPrefix2 + ".TextValue");
							break;

						case VariableType.Vector3:
							presetValue.vector3Val = CustomGUILayout.Vector3Field (label, presetValue.vector3Val, apiPrefix2 + ".Vector3Value");
							break;

						case VariableType.GameObject:
							presetValue.gameObjectVal = (GameObject) CustomGUILayout.ObjectField <GameObject> (label, presetValue.gameObjectVal, (location != VariableLocation.Global), apiPrefix2 + ".GameObjectVal");
							break;

						case VariableType.UnityObject:
							presetValue.objectVal = CustomGUILayout.ObjectField <Object> (label, presetValue.objectVal, false);
							break;

						default:
							break;
					}
				}
			}

			EditorGUILayout.Space ();
			if (canEdit)
			{
				switch (location)
				{
					case VariableLocation.Local:
						link = VarLink.None;
						break;

					case VariableLocation.Global:
					case VariableLocation.Component:
						link = (VarLink) CustomGUILayout.EnumPopup ("Link to:", link, apiPrefix + ".link", "What it links to");
						if (link == VarLink.PlaymakerVariable)
						{
							if (PlayMakerIntegration.IsDefinePresent ())
							{
								if (location == VariableLocation.Global)
								{
									pmVar = CustomGUILayout.TextField ("Playmaker Global Variable:", pmVar, apiPrefix + ".pmVar", "The name of the Playmaker variable to link to.");
								}
								else if (location == VariableLocation.Component)
								{
									if (_variables && PlayMakerIntegration.HasFSM (_variables.gameObject))
									{
										pmVar = CustomGUILayout.TextField ("Playmaker Local Variable:", pmVar, apiPrefix + ".pmVar", "The name of the Playmaker variable to link to. It is assumed to be placed on the same GameObject as this Variables component.");
									}
									else
									{
										EditorGUILayout.HelpBox ("A Playmaker FSM component must be present on the Variables GameObject.", MessageType.Info);
									}
								}

								if (!string.IsNullOrEmpty (pmVar))
								{
									updateLinkOnStart = CustomGUILayout.Toggle ("Use PM for initial value?", updateLinkOnStart, apiPrefix + ".updateLinkOnStart", "If True, then Playmaker will be referred to for the initial value");
								}
							}
							else
							{
								EditorGUILayout.HelpBox ("The 'PlayMakerIsPresent' Scripting Define Symbol must be listed in the\nPlayer Settings. It can be set in Edit -> Project Settings -> Player", MessageType.Warning);
							}
						}
						else if (link == VarLink.OptionsData)
						{
							if (location == VariableLocation.Global)
							{
								EditorGUILayout.HelpBox ("This Variable will be stored in PlayerPrefs, and not in saved game files.", MessageType.Info);
							}
							else
							{
								EditorGUILayout.HelpBox ("Component variables cannot be linked to Options Data - use Global variables instead.", MessageType.Warning);
							}
						}
						else if (link == VarLink.CustomScript)
						{
							updateLinkOnStart = CustomGUILayout.Toggle ("Script sets initial value?", updateLinkOnStart, apiPrefix + ".updateLinkOnStart", "If True, then a custom script will be referred to for the initial value");
							EditorGUILayout.HelpBox ("See the Manual's 'Variable linking' chapter for details on how to synchronise values.", MessageType.Info);
						}
						break;
				}
			}
			else
			{
				if (link != VarLink.None)
				{
					EditorGUILayout.LabelField ("Links to: " + link.ToString ());
					if (link == VarLink.PlaymakerVariable && !string.IsNullOrEmpty (pmVar))
					{
						EditorGUILayout.LabelField ("Linked PM variable: " + pmVar);
					}
					if (link == VarLink.PlaymakerVariable || link == VarLink.CustomScript)
					{
						if (updateLinkOnStart)
						{
							EditorGUILayout.LabelField ("Script sets initial value");
						}
					}
				}
			}

			if (canEdit)
			{
				description = CustomGUILayout.TextArea ("Internal description:", description, apiPrefix + ".description", "An Editor-only description to aid designers");
			}
			else
			{
				if (!string.IsNullOrEmpty (description))
				{
					EditorGUILayout.LabelField ("Internal description: " + description);
				}
			}
		}

		#endif

	}

}