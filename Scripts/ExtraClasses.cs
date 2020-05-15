﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace radiants.SpriteDigits
{
	#region Enums

	public enum PaddingMode
	{
		Pad,
		ZeroFill,
	}

	public enum HorizontalPivot
	{
		Left,
		Center,
		Right,
	}

	public enum VerticalPivot
	{
		Top,
		Center,
		Bottom,
	}

	#endregion




	[System.Serializable]
	public class DigitsReactiveProperty : ReactiveProperty<Digits>
	{ }

	[System.Serializable]
	public class PaddingModeReactiveProperty : ReactiveProperty<PaddingMode>
	{ }

	[System.Serializable]
	public class HorizontalPivotReactiveProperty : ReactiveProperty<HorizontalPivot>
	{
		public HorizontalPivotReactiveProperty() : base()
		{ }

		public HorizontalPivotReactiveProperty(HorizontalPivot _default) : base(_default)
		{ }
	}

	[System.Serializable]
	public class VerticalPivotReactiveProperty : ReactiveProperty<VerticalPivot>
	{
		public VerticalPivotReactiveProperty() : base()
		{ }

		public VerticalPivotReactiveProperty(VerticalPivot _default) : base(_default)
		{ }
	}

	[System.Serializable]
	public class MaterialReactiveProperty : ReactiveProperty<Material>
	{
		public MaterialReactiveProperty() : base() { }
		public MaterialReactiveProperty(Material value) : base(value) { }
	}



}