// SpeechActParser.cs - 
//
// Copyright (C) 2006 GAIPS/INESC-ID
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
// 
// Company: GAIPS/INESC-ID
// Project: FearNot!
// Created: 06/04/2006
// Created by: Marco Vala
// Email to: marco.vala@tagus.ist.utl.pt
// 
// History:
//


using System;
using System.Xml;
using System.Xml.Schema;
using ThalamusFAtiMA.Speech.AMSummary;
using ThalamusFAtiMA.Utils;

namespace ThalamusFAtiMA.Speech
{
	/// <summary>
	/// Summary description for SpeechActParser.
	/// </summary>
	public class AMSummaryParser : XmlParser
	{
		class Singleton
		{
			internal static readonly AMSummaryParser Instance = new AMSummaryParser();

			// explicit static constructor to assure a single execution
			static Singleton()
			{
			}
		}

		AMSummaryParser()
		{
		}

		public static AMSummaryParser Instance
		{
			get
			{
				return Singleton.Instance;
			}
		}

		protected override void XmlErrorsHandler(object sender, ValidationEventArgs args) 
		{
			// TO DO: deal with xml errors
			Console.WriteLine("Validation error: " + args.Message);
		}


		/*protected override void ValidationErrorHandler(object sender, ValidationEventArgs args)
		{
			// TO DO: deal with xml errors
			Console.WriteLine("Validation error: " + args.Message);
		}*/

	    protected override object ParseElements(XmlDocument xml)
	    {
	        var summary = new AMSummary.AMSummary();
	        int chronologicalOrder = 1;

	        foreach (XmlNode xmlEvent in xml.DocumentElement.ChildNodes)
	        {
	            if (xmlEvent.Name.Equals("Event"))
	            {
                    var eventDescription = new EventDescription(chronologicalOrder++);
                    foreach (XmlNode node in xmlEvent.ChildNodes)
                    {
                        if (node.Name.Equals("Location"))
                        {
                            eventDescription.Location = node.InnerText;
                        }
                        else if (node.Name.Equals("Time"))
                        {
                            eventDescription.TimeCategory = node.InnerText;
                            eventDescription.TimeCount = node.Attributes["count"].Value;
                        }
                        else if (node.Name.Equals("Subject"))
                        {
                            eventDescription.Subject = node.InnerText;
                        }
                        else if (node.Name.Equals("Action"))
                        {
                            eventDescription.Action = node.InnerText;
                        }
                        else if (node.Name.Equals("Intention"))
                        {
                            eventDescription.Intention = node.InnerText;
                        }
                        else if (node.Name.Equals("Status"))
                        {
                            eventDescription.Status = node.InnerText;
                        }
                        else if (node.Name.Equals("Target"))
                        {
                            eventDescription.Target = node.InnerText;
                        }
                        else if (node.Name.Equals("Param"))
                        {
                            eventDescription.Parameters.Add(node.InnerText);
                        }
                        else if (node.Name.Equals("Emotion"))
                        {
                            var emotionDescription = new EmotionDescription();
                            if (node.Attributes["intensity"] != null)
                            {
                                emotionDescription.Intensity = node.Attributes["intensity"].Value;
                            }

                            if (node.Attributes["direction"] != null)
                            {
                                emotionDescription.Direction = node.Attributes["direction"].Value;
                            }
                            emotionDescription.Type = node.InnerText;
                            eventDescription.Emotion = emotionDescription;
                        }
                    }
                    summary.Events.Add(eventDescription);
	            }
	        }
            return summary;
	    }

	    protected override void ParseElements(XmlDocument xml, object elements)
	    {
	        throw new NotImplementedException();
	    }
	}
}
