using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace radiants.SpriteDigits
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(RectTransform))]
	public class SpriteDigits : SpriteDigitsBase
	{
		#region Serialize/Observables

		[SerializeField]
		private LongReactiveProperty _Value = new LongReactiveProperty(0);
		public long Value
		{
			get { return _Value.Value; }
			set { _Value.Value = value; }
		}


		[SerializeField]
		private IntReactiveProperty _MaxDigitNum = new IntReactiveProperty(4);
		public int MaxDigitNum
		{
			get { return _MaxDigitNum.Value; }
			set { _MaxDigitNum.Value = value; }
		}

		[SerializeField]
		private PaddingModeReactiveProperty _PaddingMode = new PaddingModeReactiveProperty();
		public PaddingMode PaddingMode
		{
			get { return _PaddingMode.Value; }
			set { _PaddingMode.Value = value; }
		}

		#endregion


		#region Subscribes

		protected override void SubscribeObservables()
		{
			_MaxDigitNum.Subscribe(_num => PrepareNumberRenderers(_num))
				.AddTo(Disposables);

			Observable.CombineLatest(_Value, _Digits, _Size, _Spacing, _MaxDigitNum, _PaddingMode,
				(_1, _2, _3, _4, _5, _6) => Unit.Default)
				.CombineLatest(_HorizontalPivot, _VerticalPivot, (_1, _2, _3) => Unit.Default)
				.Subscribe(_ => ApplyNumbers())
				.AddTo(Disposables);
		}

		#endregion


		#region Child Sprites Management

		private List<SpriteRenderer> NumberRenderers = new List<SpriteRenderer>();

		private SpriteRenderer MinusRenderer = null;

		protected override void PrepareRenderers()
		{
			if (MinusRenderer == null)
				MinusRenderer = CreateChildRenderer();

			PrepareNumberRenderers(MaxDigitNum);
		}

		private void PrepareNumberRenderers(int digitNum)
		{
			if (NumberRenderers.Count >= digitNum) return;

			for (int i = NumberRenderers.Count; i < digitNum; i++)
			{
				NumberRenderers.Add(CreateChildRenderer());
			}
		}

		protected override void DestroyAllRenderers()
		{
			//Destroy all hidden child
			for (int i = NumberRenderers.Count - 1; i >= 0; --i)
			{
				DestroyObject(NumberRenderers[i].gameObject);
			}
			DestroyObject(MinusRenderer.gameObject);
		}

		protected override void ActForAllRenderers(Action<SpriteRenderer> action)
		{
			foreach (var number in NumberRenderers)
			{
				action?.Invoke(number);
			}
			action?.Invoke(MinusRenderer);
		}

		#endregion


		#region Apply Numbers

		protected override void ApplyNumbers()
		{
			if (Digits == null) return;
			if (!Digits.CheckNumbers()) return;

			long num = Value;
			bool displayMinus = false;

			if(num < 0)
			{
				//ignores minus value if minus sprite not found
				if (Digits.MinusDisplaySprite == null)
					num = 0;
				else
				{
					num = -num;
					displayMinus = true;
				}
			}

			int digitNum = GetDigitNumber(num);

			//counter-stop
			if(MaxDigitNum != -1 && MaxDigitNum < digitNum)
			{
				num = Power(10, MaxDigitNum) - 1;
				digitNum = MaxDigitNum;
			}

			int displayDigitNum = digitNum;
			if (MaxDigitNum == -1)
			{
				//unlimited digits
				PrepareNumberRenderers(digitNum);
			}
			else if (PaddingMode == PaddingMode.ZeroFill)
			{
				//zero fill
				displayDigitNum = MaxDigitNum;
			}

			//check sprite height
			float spriteHeight = Digits.NumberSprites[0].bounds.size.y;
			float size = Mathf.Min(Size, MyRectTransform.rect.height);
			float letterScale = size / spriteHeight;

			//set sprite and check sprites' total width
			float originalWidth = SetNumberSpriteToRenderers(num, displayDigitNum, displayMinus);
			float widthWithSpace = originalWidth * letterScale + (displayDigitNum - 1) * Spacing;
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
			SetupSpritePositions(letterScale, spacingScale, widthWithSpace, displayDigitNum, displayMinus);
		}

		private float SetNumberSpriteToRenderers(long num, int displayDigitNum, bool displayMinus)
		{
			//calc sprites' total width
			float width = 0;

			for (int i = 0; i < NumberRenderers.Count; i++)
			{
				var renderer = NumberRenderers[i];
				if(i >= displayDigitNum)
				{
					renderer.enabled = false;
					continue;
				}

				renderer.enabled = true;
				int count = (int)(num % 10);

				var sprite = Digits.NumberSprites[count];
				var spriteOriginalBounds = sprite.bounds.size;
				renderer.sprite = sprite;

				num /= 10;
				width += spriteOriginalBounds.x;
			}

			MinusRenderer.enabled = displayMinus;
			if (displayMinus)
			{
				var minusSprite = Digits.MinusDisplaySprite;
				MinusRenderer.sprite = minusSprite;
				if (minusSprite != null)
					width += minusSprite.bounds.size.x;
			}

			return width;
		}

		private void SetupSpritePositions(float letterScale, float spacingScale, float scaledWidth, int displayDigitNum, bool displayMinus)
		{
			Vector3 pivotOrigin = GetPivotOrigin(HorizontalPivot, VerticalPivot, MyRectTransform.rect, scaledWidth);
			Vector3 caret = pivotOrigin;

			for (int i = 0; i < displayDigitNum; ++i)
			{
				var renderer = NumberRenderers[i];
				var spriteBounds = renderer.sprite.bounds;

				//spriteBounds.min
				SetRendererPosition(ref caret, renderer.transform, HorizontalPivot, VerticalPivot, spriteBounds, letterScale, Spacing * spacingScale);
			}

			//set minus renderer if display
			if(displayMinus)
			{
				SetRendererPosition(ref caret, MinusRenderer.transform, HorizontalPivot, VerticalPivot, MinusRenderer.sprite.bounds,
					letterScale, Spacing * spacingScale);
			}
		}

		#endregion

		#region Math

		private static int GetDigitNumber(long num)
		{
			int digitNum = 0;

			while (num != 0)
			{
				++digitNum;
				num /= 10;
			}

			if (digitNum == 0) digitNum = 1;
			return digitNum;
		}

		private static long Power(int _base, int _power)
		{
			if (_power < 0) return 0;
			long ret = 1;
			for (int i = 0; i < _power; i++)
			{
				ret *= _base;
			}
			return ret;
		}

		#endregion
	}
}