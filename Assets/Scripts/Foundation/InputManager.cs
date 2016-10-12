using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ModularInput
{
	public enum TargetPlatform
	{
		Standalone,
		XboxOne,
		UWP,
	}

	[Serializable]
	public class ControllerEvent : UnityEvent<Controller> { };

	public class InputManager : Singleton<InputManager>
	{
		public InputContext[] contexts;
		public TargetPlatform targetPlatform;

		[Header(""), Tooltip("Tells the input manager to track when the user switches from keyboard + mouse to a controller")]
		public bool trackController;
		public ControllerEvent OnControllerChanged;

		public Controller Controller { get { return this.controller; } }

		private List<InputContext> m_highPriorityContext	= new List<InputContext>();
		private List<InputContext> m_lowPriorityContext		= new List<InputContext>();

		private bool isUsingKeyboard;
		private Controller controller;

		private bool cancelClick;
		private bool detailsClick;
		private bool pauseClick;
		private bool validateClick;

		public bool CancelClick { get { return this.cancelClick; } }
		public bool DetailsClick { get { return this.detailsClick; } }
		public bool PauseClick { get { return this.pauseClick; } }
		public bool ValidateClick { get { return this.validateClick; } }

		protected void Awake()
		{
			InputManager.instance = this;

			try
			{
				Input.GetAxis("Joy1 Axis 1");
			}
			catch (Exception e)
			{
				this.trackController = false;
			}
			if (this.targetPlatform == TargetPlatform.Standalone)
			{
				this.controller = Activator.CreateInstance(typeof(StandaloneController)) as Controller;
				this.isUsingKeyboard = true;
			}
			else if (this.targetPlatform == TargetPlatform.XboxOne)
			{
				this.controller = Activator.CreateInstance(typeof(XboxOneController)) as Controller;
				this.isUsingKeyboard = false;
			}
			else if (this.targetPlatform == TargetPlatform.UWP)
			{
				this.controller = Activator.CreateInstance(typeof(StandaloneController)) as Controller;
				this.isUsingKeyboard = true;
			}

			foreach (var context in this.contexts)
			{
				context.Init();
				this.AddInputContext(context);
			}
		}

		// If there are any high priority contexts, the input won't propagate to the low priority contexts
		protected void Update()
		{
			if(m_highPriorityContext.Count > 0)
			{
				for (int i = this.m_highPriorityContext.Count - 1; i >= 0; --i)
					if (this.m_highPriorityContext[i].OnInput() == true)
						break;
			}
			else if(m_lowPriorityContext.Count > 0)
			{
				for (int i = this.m_lowPriorityContext.Count - 1; i >= 0; --i)
					if (this.m_lowPriorityContext[i].OnInput() == true)
						break;
			}

			if (this.trackController == true)
				this.DetectController();

			this.cancelClick = false;
			this.detailsClick = false;
			this.pauseClick = false;
			this.validateClick = false;
		}

		private void DetectController()
		{
			if (this.isUsingKeyboard == true)
			{
				if (Input.anyKeyDown == true)
				{
					if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton1) ||
						Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.JoystickButton3) ||
						Input.GetKeyDown(KeyCode.JoystickButton4) || Input.GetKeyDown(KeyCode.JoystickButton5) ||
						Input.GetKeyDown(KeyCode.JoystickButton6) || Input.GetKeyDown(KeyCode.JoystickButton7) ||
						Input.GetKeyDown(KeyCode.JoystickButton8) || Input.GetKeyDown(KeyCode.JoystickButton9) ||
						Input.GetKeyDown(KeyCode.JoystickButton10) || Input.GetKeyDown(KeyCode.JoystickButton11) ||
						Input.GetKeyDown(KeyCode.JoystickButton12) || Input.GetKeyDown(KeyCode.JoystickButton13) ||
						Input.GetKeyDown(KeyCode.JoystickButton14) || Input.GetKeyDown(KeyCode.JoystickButton15) ||
						Input.GetKeyDown(KeyCode.JoystickButton16) || Input.GetKeyDown(KeyCode.JoystickButton17) ||
						Input.GetKeyDown(KeyCode.JoystickButton18) || Input.GetKeyDown(KeyCode.JoystickButton19))
					{
						this.isUsingKeyboard = false;
						this.ChangeController<XboxOneController>();
					}
				}
				else if (Input.GetAxis("Joy1 Axis 1") != 0F || Input.GetAxis("Joy1 Axis 2") != 0F ||
						 Input.GetAxis("Joy1 Axis 4") != 0F || Input.GetAxis("Joy1 Axis 5") != 0F ||
						 Input.GetAxis("Joy1 Axis 6") != 0F || Input.GetAxis("Joy1 Axis 7") != 0F)
				{
					this.isUsingKeyboard = false;
					this.ChangeController<XboxOneController>();
				}
			}
			else if (Input.anyKeyDown == true)
			{
				if (!Input.GetKeyDown(KeyCode.JoystickButton0) && !Input.GetKeyDown(KeyCode.JoystickButton1) &&
					!Input.GetKeyDown(KeyCode.JoystickButton2) && !Input.GetKeyDown(KeyCode.JoystickButton3) &&
					!Input.GetKeyDown(KeyCode.JoystickButton4) && !Input.GetKeyDown(KeyCode.JoystickButton5) &&
					!Input.GetKeyDown(KeyCode.JoystickButton6) && !Input.GetKeyDown(KeyCode.JoystickButton7) &&
					!Input.GetKeyDown(KeyCode.JoystickButton8) && !Input.GetKeyDown(KeyCode.JoystickButton9) &&
					!Input.GetKeyDown(KeyCode.JoystickButton10) && !Input.GetKeyDown(KeyCode.JoystickButton11) &&
					!Input.GetKeyDown(KeyCode.JoystickButton12) && !Input.GetKeyDown(KeyCode.JoystickButton13) &&
					!Input.GetKeyDown(KeyCode.JoystickButton14) && !Input.GetKeyDown(KeyCode.JoystickButton15) &&
					!Input.GetKeyDown(KeyCode.JoystickButton16) && !Input.GetKeyDown(KeyCode.JoystickButton17) &&
					!Input.GetKeyDown(KeyCode.JoystickButton18) && !Input.GetKeyDown(KeyCode.JoystickButton19))
				{
					this.isUsingKeyboard = true;
					this.ChangeController<StandaloneController>();
				}
			}
		}

		private void ChangeController<T>() where T : Controller
		{
			Debug.Log("Changing the controller to a " + typeof(T).Name + ".");
			this.controller = Activator.CreateInstance(typeof(T)) as Controller;
			this.OnControllerChanged.Invoke(this.Controller);
		}

		public void AddInputContext(InputContext ic)
		{
			if (ic.priority == ContextPriority.High)
				this.m_highPriorityContext.Add(ic);
			else
				this.m_lowPriorityContext.Add(ic);
			ic.OnContextAdded();
		}

		public void RemoveInputContext(InputContext ic)
		{
			if (ic.priority == ContextPriority.High)
				this.m_highPriorityContext.Remove(ic);
			else
				this.m_lowPriorityContext.Remove(ic);
			ic.OnContextRemoved();
		}

		public void ClickCancel()
		{
			this.cancelClick = true;
		}

		public void ClickDetails()
		{
			this.detailsClick = true;
		}

		public void ClickPause()
		{
			this.pauseClick = true;
		}

		public void ClickValidate()
		{
			this.validateClick = true;
		}

	}
}

