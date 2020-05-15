using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace radiants.SpriteDigits
{
	[CreateAssetMenu(menuName = "radiants/SpriteDigits/Make Digits ScriptableObject")]
	public class Digits : ScriptableObject
	{
		[SerializeField]
		public List<Sprite> NumberSprites = new List<Sprite>(10);

		[SerializeField]
		public Sprite DecimalPointSprite;

		[SerializeField]
		public Sprite MinusDisplaySprite;

		public bool CheckNumbers()
		{
			if(NumberSprites.Count != 10)
			{
				Debug.LogError("Digits NumberSprites' length must be 10.");
				return false;
			}

			foreach(var spr in NumberSprites)
			{
				if(spr == null)
				{
					Debug.LogError("Digits NumberSprites has null reference. This may cause problem.");
					return false;
				}
			}
			return true;
		}

		public bool CheckNumbersFull()
		{
			if (!CheckNumbers()) return false;

			if (DecimalPointSprite == null) return false;
			if (MinusDisplaySprite == null) return false;
			return true;
		}

	}
}