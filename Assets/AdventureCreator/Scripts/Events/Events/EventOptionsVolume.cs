﻿using UnityEngine;

namespace AC
{

	public class EventOptionsVolume : EventBase
	{

		[SerializeField] private VolumeType volumeType;
		private enum VolumeType { Music, SFX, Speech };

		public override string[] EditorNames { get { return new string[] { "Options/Change volume/Music", "Options/Change volume/SFX", "Options/Change volume/Speech" }; } }
		protected override string EventName { get { return "OnChangeVolume"; } }
		protected override string ConditionHelp { get { return "Whenever the " + volumeType.ToString ().ToLower () + " volume is changed."; } }
		

		public override void Register ()
		{
			EventManager.OnChangeVolume += OnChangeVolume;
		}


		public override void Unregister ()
		{
			EventManager.OnChangeVolume -= OnChangeVolume;
		}


		private void OnChangeVolume (SoundType soundType, float volume)
		{
			if (soundType.ToString () == volumeType.ToString ())
			{
				Run (new object[] { volume });
			}
		}


		protected override ParameterReference[] GetParameterReferences ()
		{
			return new ParameterReference[]
			{
				new ParameterReference (ParameterType.Float, "Volume"),
			};
		}


#if UNITY_EDITOR

		protected override bool HasConditions (bool isAssetFile) { return false; }


		public override void AssignVariant (int variantIndex)
		{
			volumeType = (VolumeType) variantIndex;
		}

#endif

	}

}