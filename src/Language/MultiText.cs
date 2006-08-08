using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace WeSay.Language
{
	/// <summary>
	/// MultiText holds an array of strings, indexed by writing system ID.
	/// These are simple, single language Unicode strings.
	/// </summary>
	public class MultiText : INotifyPropertyChanged
	{
		/// <summary>
		/// For INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		protected System.Collections.Generic.Dictionary<string, string> _forms;

		public MultiText()
		{
			_forms = new Dictionary<string, string>();
		}

		public string this[string writingSystemId]
		{
			get
			{
				return GetAlternative(writingSystemId);
			}
			set
			{
				SetAlternative(writingSystemId, value);
			}
		}

		public string GetAlternative(string writingSystemId)
		{
			if (!_forms.ContainsKey(writingSystemId))
				return "";

			string form = _forms[writingSystemId];
			if (form == null)
			{
				return "";
			}
			else
				return form;
		}

		public void SetAlternative(string writingSystemId, string form)
		{
		   Debug.Assert(writingSystemId != null && writingSystemId.Length > 0, "The writing system id was empty.");
		   Debug.Assert(writingSystemId.Trim() == writingSystemId, "The writing system id had leading or trailing whitespace");

		   //enhance: check to see if there has actually been a change

		   if (form == null || form == "") // we don't use space to store empty strings.
		   {
			   if (_forms.ContainsKey(writingSystemId))
			   {
				   _forms.Remove(writingSystemId);
			   }
		   }
		   else
		   {
			   _forms[writingSystemId] = form;
		   }

		   NotifyPropertyChanged(writingSystemId);
		}

		private void NotifyPropertyChanged(string writingSystemId)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(writingSystemId));
			}
		}
	}
}
