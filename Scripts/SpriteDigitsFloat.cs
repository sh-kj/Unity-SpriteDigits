using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace radiants.SpriteDigits
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(RectTransform))]
	public class SpriteDigitsFloat : SpriteDigitsBase
	{
		#region Serialize/Observables

		[SerializeField]
		private DoubleReactiveProperty _Value = new DoubleReactiveProperty(0d);
		public double Value
		{
			get { return _Value.Value; }
			set { _Value.Value = value; }
		}

		[SerializeField]
		private IntReactiveProperty _DisplayDecimalPlaces = new IntReactiveProperty(2);
		public int DisplayDecimalPlaces
		{
			get { return _DisplayDecimalPlaces.Value; }
			set { _DisplayDecimalPlaces.Value = value; }
		}

		#endregion

		#region Subscribes

		protected override void SubscribeObservables()
		{
			Observable.CombineLatest(_Value, _DisplayDecimalPlaces, _Digits, _Size, _Spacing, _HorizontalPivot, _VerticalPivot,
				(_1, _2, _3, _4, _5, _6, _7) => Unit.Default)
				.Subscribe(_ => ApplyNumbers())
				.AddTo(Disposables);
		}

		#endregion


		#region Child Sprites Management

		private List<SpriteRenderer> NumberRenderers = new List<SpriteRenderer>();

		private SpriteRenderer MinusRenderer = null;

		private SpriteRenderer DecimalPointRenderer = null;

		protected override void PrepareRenderers()
		{
			if (MinusRenderer == null)
				MinusRenderer = CreateChildRenderer();

			if (DecimalPointRenderer == null)
				DecimalPointRenderer = CreateChildRenderer();

			//minimum required
			PrepareNumberRenderers(DisplayDecimalPlaces + 1);
		}

		private void PrepareNumberRenderers(int requiredNumber)
		{
			if (NumberRenderers.Count >= requiredNumber) return;

			for (int i = NumberRenderers.Count; i < requiredNumber; i++)
			{
				NumberRenderers.Add(CreateChildRenderer());
			}
		}

		protected override void DestroyAllRenderers()
		{
			for (int i = NumberRenderers.Count - 1; i >= 0; i--)
			{
				DestroyObject(NumberRenderers[i].gameObject);
			}
			DestroyObject(MinusRenderer.gameObject);
			DestroyObject(DecimalPointRenderer.gameObject);
		}

		protected override void ActForAllRenderers(Action<SpriteRenderer> action)
		{
			foreach (var number in NumberRenderers)
			{
				action?.Invoke(number);
			}
			action?.Invoke(MinusRenderer);
			action?.Invoke(DecimalPointRenderer);
		}

		#endregion





		#region Apply Numbers

		protected override void ApplyNumbers()
		{
			if (Digits == null) return;
			if (!Digits.CheckNumbersFull()) return;

			//to avoid float accuracy issue, convert to decimal
			if(Value > (double)decimal.MaxValue)
			{
				Debug.LogWarning("Value is bigger than decimal.MaxValue. SpriteDigits cannot display it.");
				Value = (double)decimal.MaxValue;
			}
			decimal num = (decimal)Value;
			bool displayMinus = false;

			if (num < 0)
			{
				num = -num;
				displayMinus = true;
			}

			int digitsBeforePoint = CalcDigitsBeforeDecimalPoint(num);

			//prepare renderers dynamically
			PrepareNumberRenderers(DisplayDecimalPlaces + digitsBeforePoint);

			//check sprite height
			float spriteHeight = Digits.NumberSprites[0].bounds.size.y;
			float size = Mathf.Min(Size, MyRectTransform.rect.height);
			float letterScale = size / spriteHeight;

			//set sprite and check sprites' total width
			float originalWidth = SetNumberSpriteToRenderers(num, ref digitsBeforePoint, displayMinus);
			float widthWithSpace = originalWidth * letterScale + (digitsBeforePoint + DisplayDecimalPlaces) * Spacing;
			if (displayMinus) widthWithSpace += Spacing;

			//determine scale
			float spacingScale = 1f;
			if (MyRectTransform.rect.width < widthWithSpace)
			{
				spacingScale = MyRectTransform.rect.width / widthWithSpace;
				letterScale = letterScale * spacingScale;
				widthWithSpace = MyRectTransform.rect.width;
			}

			//set position
			SetupSpritePositions(letterScale, spacingScale, widthWithSpace, digitsBeforePoint, displayMinus);
		}


		private float SetNumberSpriteToRenderers(decimal num, ref int digitsBeforePoint, bool displayMinus)
		{
			//calc sprites' total width
			float width = 0;

			bool carryUp = false;

			//after point
			for (int i = 0; i < DisplayDecimalPlaces; i++)
			{
				int place = DisplayDecimalPlaces - i;
				int number = i == 0 ? RoundNumberOfPlaceAfterPoint(num, place) : GetNumberOfPlaceAfterPoint(num, place);

				//round carry-up
				if (carryUp) ++number;
				if (number > 9)
				{
					number -= 10;
					carryUp = true;
				}
				else
				{
					carryUp = false;
				}

				var sprite = Digits.NumberSprites[number];
				NumberRenderers[i].enabled = true;
				NumberRenderers[i].sprite = sprite;

				width += sprite.bounds.size.x;
			}

			//point
			DecimalPointRenderer.enabled = true;
			DecimalPointRenderer.sprite = Digits.DecimalPointSprite;
			width += Digits.DecimalPointSprite.bounds.size.x;

			//before point
			for (int i = 0; i < digitsBeforePoint; i++)
			{
				int number;
				if (DisplayDecimalPlaces == 0)
					number = RoundNumberOfPlaceBeforePoint(num, i);
				else
					number = GetNumberOfPlaceBeforePoint(num, i);

				//round carry-up
				if (carryUp) ++number;
				if (number > 9)
				{
					number -= 10;
					carryUp = true;
				}
				else
				{
					carryUp = false;
				}


				var sprite = Digits.NumberSprites[number];

				int rendererIndex = i + DisplayDecimalPlaces;
				NumberRenderers[rendererIndex].enabled = true;
				NumberRenderers[rendererIndex].sprite = sprite;

				width += sprite.bounds.size.x;
			}

			//carry final digit
			if(carryUp)
			{
				int finalIndex = DisplayDecimalPlaces + digitsBeforePoint;
				PrepareNumberRenderers(finalIndex+1);
				NumberRenderers[finalIndex].enabled = true;
				NumberRenderers[finalIndex].sprite = Digits.NumberSprites[1];
				width += Digits.NumberSprites[1].bounds.size.x;

				++digitsBeforePoint;
			}


			//disabled
			for (int i = DisplayDecimalPlaces + digitsBeforePoint; i < NumberRenderers.Count; i++)
			{
				NumberRenderers[i].enabled = false;
			}


			//minus
			MinusRenderer.enabled = displayMinus;
			if(displayMinus)
			{
				MinusRenderer.sprite = Digits.MinusDisplaySprite;
				width += Digits.MinusDisplaySprite.bounds.size.x;
			}

			return width;
		}

		private void SetupSpritePositions(float letterScale, float spacingScale, float scaledWidth, int digitsBeforePoint, bool displayMinus)
		{
			Vector3 pivotOrigin = GetPivotOrigin(HorizontalPivot, VerticalPivot, MyRectTransform.rect, scaledWidth);
			Vector3 caret = pivotOrigin;

			//after point
			for (int i = 0; i < DisplayDecimalPlaces; i++)
			{
				var renderer = NumberRenderers[i];
				var spriteBounds = renderer.sprite.bounds;
				SetRendererPosition(ref caret, renderer.transform, HorizontalPivot, VerticalPivot, spriteBounds, letterScale, Spacing * spacingScale);
			}

			//point
			SetRendererPosition(ref caret, DecimalPointRenderer.transform, HorizontalPivot, VerticalPivot, DecimalPointRenderer.sprite.bounds,
				letterScale, Spacing * spacingScale);

			//before point
			for (int i = 0; i < digitsBeforePoint; i++)
			{
				int index = i + DisplayDecimalPlaces;
				var renderer = NumberRenderers[index];
				var spriteBounds = renderer.sprite.bounds;
				SetRendererPosition(ref caret, renderer.transform, HorizontalPivot, VerticalPivot, spriteBounds, letterScale, Spacing * spacingScale);
			}

			//set minus renderer if display
			if (displayMinus)
			{
				SetRendererPosition(ref caret, MinusRenderer.transform, HorizontalPivot, VerticalPivot, MinusRenderer.sprite.bounds,
					letterScale, Spacing * spacingScale);
			}
		}

		#endregion

		#region Math

		private static int CalcDigitsBeforeDecimalPoint(decimal num)
		{
			int ret = 1;
			while(num >= 10)
			{
				++ret;
				num /= 10;
			}

			return ret;
		}

		private static int RoundNumberOfPlaceAfterPoint(decimal num, int place)
		{
			decimal n = num;
			n *= Power10(place - 1);
			n -= Math.Floor(n);
			return (int)Math.Round(n * 10);

		}
		private static int GetNumberOfPlaceAfterPoint(decimal num, int place)
		{
			decimal n = num;
			n *= Power10(place - 1);
			n -= Math.Floor(n);
			return (int)Math.Floor(n * 10);
		}
		private static int RoundNumberOfPlaceBeforePoint(decimal num, int digit)
		{
			decimal n = num;
			n /= Power10(digit + 1);
			n -= Math.Floor(n);
			return (int)Math.Round(n * 10);
		}
		private static int GetNumberOfPlaceBeforePoint(decimal num, int digit)
		{
			decimal n = num;
			n /= Power10(digit + 1);
			n -= Math.Floor(n);
			return (int)Math.Floor(n * 10);
		}

		private static decimal Power10(int _power)
		{
			if (_power < 0) return 1;
			decimal ret = 1;
			for (int i = 0; i < _power; i++)
			{
				ret *= 10;
			}
			return ret;
		}

		#endregion

	}
}