using MapsExt.Editor;
using UnityEngine;
namespace GMCObjectsEditor.EditorActionHandlers
{
	public class TeleporterActionHandler : EditorActionHandler
	{
		public override bool CanRotate()
		{
			return false;
		}

		public override bool CanResize(int resizeDirection)
		{
			return false;
		}

		public override bool Resize(Vector3 sizeDelta, int resizeDirection)
		{
			return false;
		}
	}
}
