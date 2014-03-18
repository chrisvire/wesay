using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.Text;
using Palaso.WritingSystems;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Foundation;
using WeSay.Project;
using WeSay.UI;

namespace WeSay.LexicalTools
{
	public static class HtmlRenderer
	{

		public static string Color;
		public static string HeadWordWritingSystemId;
		private static bool italicsOn;

		public static string ToHtml(LexEntry entry,
								   CurrentItemEventArgs currentItem,
								   LexEntryRepository lexEntryRepository)
		{
			string oldhtml = "<html><header><meta charset=\"UTF-8\"></head><body style='background:{5}' id='mainbody'><div style='min-height:20px; font-family:{0}; font-size:{1}pt; text-align:{3}' id='main' name='textArea' contentEditable='{4}'>{2}</div></body></html>";
			italicsOn = false;
			if (lexEntryRepository == null)
			{
				throw new ArgumentNullException("lexEntryRepository");
			}
			if (entry == null)
			{
				return string.Empty;
			}

			var html = new StringBuilder();
			html.Append("<html><header><meta charset=\"UTF-8\"></head><body style='background:{0}'><div style='min-height:20px' id='main' name='textArea' contentEditable='false'>");
			RenderHeadword(entry, html, lexEntryRepository);
			

			int senseNumber = 1;
			foreach (LexSense sense in entry.Senses)
			{
				RenderSense(entry, sense, senseNumber, currentItem, html);

				++senseNumber;
			}
			
#if GlossMeaning
			html.Append(RenderGhostedField("Gloss", currentItem, entry.Senses.Count + 1));
#else
			html.Append(RenderGhostedField(null,
				LexSense.WellKnownProperties.Definition,
										  currentItem,
										  entry.Senses.Count + 1));
#endif

			html.Append("</div></body></html>");
			return html.ToString();
		}

		private static void RenderSense(LexEntry entry, LexSense sense, int senseNumber, CurrentItemEventArgs currentItem, StringBuilder html)
		{
//html.Append(SwitchToWritingSystem(WritingSystems.AnalysisWritingSystemDefault.Id));
#if GlossMeaning
				if (entry.Senses.Count > 1 || (currentItem != null && currentItem.PropertyName == "Gloss"))
#else
			if (entry.Senses.Count > 1 ||
				(currentItem != null &&
				 currentItem.PropertyName == LexSense.WellKnownProperties.Definition))
#endif
			{
				html.Append(" " + senseNumber);
			}

			RenderPartOfSpeech(sense, currentItem, html);
			
#if GlossMeaning
				html.Append(" " + RenderField(sense.Gloss, currentItem));
#else
			// Render the Definition (meaning) field
			Field dfnField = WeSayWordsProject.Project.GetFieldFromDefaultViewTemplate(
	LexSense.WellKnownProperties.Definition);
			html.Append(" " + RenderField(sense.Definition, currentItem, 0, dfnField));
#endif

			RenderExampleSentences(currentItem, html, sense);

			html.Append(RenderGhostedField(sense, "Sentence", currentItem, null));
			html.Append(RenderGhostedField(sense, "Translation", currentItem, null));
			 
		}

		private static void RenderExampleSentences(CurrentItemEventArgs currentItem, StringBuilder rtf, LexSense sense)
		{
			foreach (LexExampleSentence exampleSentence in sense.ExampleSentences)
			{
				italicsOn = true;
				rtf.Append(RenderField(exampleSentence.Sentence, currentItem));
				italicsOn = false;
				rtf.Append(RenderField(exampleSentence.Translation, currentItem));
			}
		}

		private static void RenderPartOfSpeech(LexSense sense, CurrentItemEventArgs currentItem, StringBuilder html)
		{
			OptionRef posRef = sense.GetProperty<OptionRef>(
				LexSense.WellKnownProperties.PartOfSpeech
			);
			if (posRef == null)
			{
				return;
			}

			OptionsList list = WeSayWordsProject.Project.GetOptionsList(
				LexSense.WellKnownProperties.PartOfSpeech
			);
			if (list == null)
			{
				return;
			}

			Option posOption = list.GetOptionFromKey(posRef.Value);
			if (posOption == null)
			{
				return;
			}

			Field posField = WeSayWordsProject.Project.GetFieldFromDefaultViewTemplate(
				LexSense.WellKnownProperties.PartOfSpeech
			);
			if (posField == null)
			{
				return;
			}
			italicsOn = true;
			html.Append(RenderField(posOption.Name, currentItem, 0, posField));
			italicsOn = false;
		}

		private static void RenderHeadword(LexEntry entry,
										   StringBuilder html,
										   LexEntryRepository lexEntryRepository)
		{
			if (StartNewSpan(html, HeadWordWritingSystemId, true, false, 0))
			{
				LanguageForm headword = entry.GetHeadWord(HeadWordWritingSystemId);
				if (null != headword)
				{
					html.Append(headword.Form);

					int homographNumber = lexEntryRepository.GetHomographNumber(
						entry,
						WeSayWordsProject.Project.DefaultViewTemplate.HeadwordWritingSystem
						);
					if (homographNumber > 0)
					{
						html.Append("<sup>" + homographNumber.ToString() + "</sup>");
					}
				}
				else
				{
					html.Append("??? ");
				}
				html.Append(" </span>");
			}
		}

		private static bool StartNewSpan(StringBuilder html,
			String writingSystemId,
			bool boldText,
			bool underline,
			int fontSizeBoost)
		{
			if (!WritingSystems.Contains(writingSystemId))
			{
				return false;
				//that ws isn't actually part of our configuration, so can't get a special font for it
			}
			IWritingSystemDefinition ws = (IWritingSystemDefinition)WritingSystems.Get(writingSystemId);
			float fontSize = ws.DefaultFontSize + fontSizeBoost;
			var formattedSpan = string.Format(
				"<span style='font-family:{0}; font-size:{1}pt;font-weight:{2};font-style:{3};text-decoration:{4}'>",
				ws.DefaultFontName,
				fontSize.ToString(),
				boldText ? "bold": "normal",
				italicsOn ? "italic" : "normal",
				underline ? "underline" : "none");
			html.Append(formattedSpan);
			return true;
		}

		private static IWritingSystemRepository WritingSystems
		{
			get { return BasilProject.Project.WritingSystems; }
		}

		private static string RenderField(MultiText text, CurrentItemEventArgs currentItem)
		{
			return RenderField(text, currentItem, 0, null);
		}

		private static string RenderField(MultiText text,
										  CurrentItemEventArgs currentItem,
										  int sizeBoost,
										  Field field)
		{
			var htmlBuilder = new StringBuilder();
			if (text != null)
			{
				if (text.Count == 0 && currentItem != null && text == currentItem.DataTarget)
				{
					htmlBuilder.Append(RenderBlankPosition());
				}

				if (field == null) // show them all
				{
					foreach (string id in WritingSystems.FilterForTextIds(text.Forms.Select(f=>f.WritingSystemId)))
					{

						var form = text.Forms.First(f => f.WritingSystemId == id);
						RenderForm(text, currentItem, htmlBuilder, form, sizeBoost);
					}
				}
				else // show all forms turned on in the field
				{
					foreach (string id in field.WritingSystemIds.Intersect(text.Forms.Select(f=>f.WritingSystemId)))
					{
						var form = text.Forms.First(f => f.WritingSystemId == id);
						RenderForm(text, currentItem, htmlBuilder, form, sizeBoost);
					}
				}
			}
			return htmlBuilder.ToString();
		}

		//public static IList<LanguageForm> GetActualTextForms(MultiText text, IWritingSystemRepository writingSytems)
		//{
		//    var x = text.Forms.Where(f => !writingSytems.Get(f.WritingSystemId).IsVoice);
		//    return new List<LanguageForm>(x);
		//}

		private static void RenderForm(MultiText text,
									   CurrentItemEventArgs currentItem,
									   StringBuilder htmlBuilder,
									   LanguageForm form,
									   int sizeBoost)
		{
			bool underLineOn = IsCurrentField(text, form, currentItem);
			if (StartNewSpan(htmlBuilder, form.WritingSystemId, false, underLineOn, sizeBoost))
			{
				htmlBuilder.Append(form.Form); // + " ");
				htmlBuilder.Append(" </span>");
				
			}
		}

		private static string RenderGhostedField(PalasoDataObject parent,
												string property,
												 CurrentItemEventArgs currentItem,
												 int? number)
		{
			string html = string.Empty;
			if (currentItem != null && property == currentItem.PropertyName && parent==currentItem.Parent)
			{
				//REVIEW: is a ws switch needed for a blank? html += SwitchToWritingSystem(WritingSystems.AnalysisWritingSystemDefault.Id);
				if (number != null)
				{
					html += number.ToString();
				}
				html += RenderBlankPosition();
			}
			return html;
		}

		private static string RenderBlankPosition()
		{
			var htmlBuilder = new StringBuilder();
			if (StartNewSpan(htmlBuilder, HeadWordWritingSystemId, false, true, 0))
			{
				htmlBuilder.Append("        " + Convert.ToChar(160));
			    htmlBuilder.Append("</span");
			}
			return htmlBuilder.ToString();
		}

		private static bool IsCurrentField(MultiText text,
										   LanguageForm l,
										   CurrentItemEventArgs currentItem)
		{
			if (currentItem == null)
			{
				return false;
			}
			return (currentItem.DataTarget == text &&
					currentItem.WritingSystemId == l.WritingSystemId);
		}


	}
}