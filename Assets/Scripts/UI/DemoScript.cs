using UnityEngine;
using UnityEngine.UI;

namespace ModularInput
{
	public class DemoScript : MonoBehaviour
	{
		public Button start;
		public Button stop;

		// The context handles how the script will respond to uer input
		private StandardInputContext context;
		// InputManager gives us the current controller and notifies us when the controller changes
		private InputManager inputManager;
		// The controller is used to detect the user input
		private Controller controller;

		protected virtual void Awake()
		{
			this.inputManager = InputManager.Instance;
			this.inputManager.OnControllerChanged.AddListener(this.HandleControllerChange);
			this.context = ScriptableObject.CreateInstance<StandardInputContext>();
			this.context.Init("Demo Context");
			this.context.onInput += this.OnInput;
			this.controller = this.inputManager.Controller;
		}

		private void HandleControllerChange(Controller arg0)
		{
			this.controller = this.inputManager.Controller;
		}

		protected virtual void OnEnable()
		{
			this.inputManager.AddInputContext(this.context);
		}

		protected virtual void OnDisable()
		{
			this.inputManager.OnControllerChanged.RemoveListener(this.HandleControllerChange);
			this.inputManager.RemoveInputContext(this.context);
		}

		private bool OnInput()
		{
			if (this.controller.Validate() == true)
			{
				this.start.onClick.Invoke();
				return true;
			}
			else if (this.controller.Cancel() == true)
			{
				this.stop.onClick.Invoke();
				return true;
			}
			return false;
		}
	}
}

