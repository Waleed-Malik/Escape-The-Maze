using UnityEngine;
using System.Collections;
using UnityEngine.Events;


namespace qtools.qmaze.example1
{
	public class QFinishTrigger : MonoBehaviour 
	{
		public delegate void QFinishTriggerHandler();

		public event QFinishTriggerHandler triggerHandlerEvent;
		//UnityAction<void> unityEvent;
		QFPSMazeGame qfpsGameRef;

		void Start()
		{
			qfpsGameRef = GameObject.Find("QMazeGame")?.GetComponent<QFPSMazeGame>();
			// if(qfpsGameRef != null){
			// 	unityEvent += qfpsGameRef.finishHandler();
			// 	triggerHandlerEvent += qfpsGameRef.ActivateFinishTriggers;
			// }
		}

		void OnTriggerEnter () 
		{
			if (qfpsGameRef != null)
			{
				Debug.Log("Not Null");
				qfpsGameRef.finishHandler();
			}
			else
			{
				Debug.Log("Is Null");
			}
		}
	}
}