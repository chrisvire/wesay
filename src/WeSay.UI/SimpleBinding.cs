using System;
using System.ComponentModel;
using WeSay.Foundation;

namespace WeSay.UI
{
	/// <summary>
	/// This simple binding class connects a IValueHolder<TValueType> (e.g. OptionRef)
	/// with a Widget (e.g. SingleOptionControl)
	/// Changes in either one are reflected in the other.
	/// </summary>
	public class SimpleBinding<TValueType>
	 {
		public event EventHandler<CurrentItemEventArgs> CurrentItemChanged = delegate
		{
		};

		private IValueHolder<TValueType> _dataTarget;
		private IBindableControl<TValueType> _widget;
		private bool _inMidstOfChange;

		public SimpleBinding(IValueHolder<TValueType> dataTarget, IBindableControl<TValueType> widgetTarget)
		{
			_dataTarget = dataTarget;
			_dataTarget.PropertyChanged += new PropertyChangedEventHandler(OnDataPropertyChanged);
			_widget = widgetTarget;
			_widget.ValueChanged += new EventHandler(OnWidgetValueChanged);
			_widget.GoingAway += new EventHandler(_target_HandleDestroyed);
		  //  _widget.Enter += new EventHandler(OnTextBoxEntered);
		}

		void _target_HandleDestroyed(object sender, EventArgs e)
		{
			TearDown();
		}

//      todo: Make some kind of "infocus" event  void OnTextBoxEntered(object sender, EventArgs e)
//        {
//            CurrentItemChanged(sender, new CurrentItemEventArgs(DataTarget, _writingSystemId));
//        }


		void OnWidgetValueChanged(object sender, EventArgs e)
		{
			SetDataTargetValue(_widget.Value);
		}

		/// <summary>
		/// Drop our connections to everything so garbage collection can happen and we aren't
		/// a zombie responding to data change events.
		/// </summary>
		private void TearDown()
		{
			//Debug.WriteLine(" BindingTearDown boundTo: " + this._widget.Name);

			if (_dataTarget == null)
			{
				return; //teardown was called twice
			}

			_dataTarget.PropertyChanged -= new PropertyChangedEventHandler(OnDataPropertyChanged);
			_dataTarget = null;
			_widget.ValueChanged -= new EventHandler(OnWidgetValueChanged);
			_widget.GoingAway -= new EventHandler(_target_HandleDestroyed);
			_widget = null;
		}

		/// <summary>
		/// Respond to a change in the data object that we are attached to.
		/// </summary>
		protected virtual void OnDataPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_inMidstOfChange )
				return;

			try
			{
				_inMidstOfChange = true;
				_widget.Value = GetTargetValue();
			}
			finally
			{
				_inMidstOfChange = false;
			}
		}

		protected TValueType GetTargetValue()
		{
			IValueHolder<TValueType> holder = _dataTarget;
			if (holder == null)
				throw new ArgumentException("Binding can't handle that type of target.");
			return holder.Value;
		}

		protected virtual void SetDataTargetValue(TValueType value)
		{
			if (_inMidstOfChange)
				return;

			try
			{
				_inMidstOfChange = true;

				if (_dataTarget == null)
				{
					throw new ArgumentException("Binding found data target null.");
				}
				_dataTarget.Value = value;

			}
			finally
			{
				_inMidstOfChange = false;
			}
		}

		public INotifyPropertyChanged DataTarget
		{
			get
			{
				return _dataTarget;
			}
		}

	}
}