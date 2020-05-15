using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace radiants.SpriteDigits
{
	[UnityEditor.CustomPropertyDrawer(typeof(DigitsReactiveProperty))]
	[UnityEditor.CustomPropertyDrawer(typeof(PaddingModeReactiveProperty))]
	[UnityEditor.CustomPropertyDrawer(typeof(HorizontalPivotReactiveProperty))]
	[UnityEditor.CustomPropertyDrawer(typeof(VerticalPivotReactiveProperty))]
	[UnityEditor.CustomPropertyDrawer(typeof(MaterialReactiveProperty))]
	public class ExtendInspectorDisplayDrawer : InspectorDisplayDrawer { }


}
