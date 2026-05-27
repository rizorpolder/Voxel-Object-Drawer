using Sample;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	[CustomEditor(typeof(ObjectCreator))]
	public class ObjectCreatorEditor : UnityEditor.Editor
	{
		private ObjectCreator _target;

		public override void OnInspectorGUI()
		{
			_target = (ObjectCreator)target;


			var style = new GUIStyle(GUI.skin.button);
			style.normal.textColor = Color.green;

			if (GUILayout.Button("Generate Voxel Mesh", style))
			{
				_target.GenerateObject();
			}
		}
	}
}