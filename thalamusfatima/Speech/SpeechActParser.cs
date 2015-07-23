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
using ThalamusFAtiMA.Utils;

namespace ThalamusFAtiMA.Speech
{
	/// <summary>
	/// Summary description for SpeechActParser.
	/// </summary>
	public class SpeechActParser : XmlParser
	{
		class Singleton
		{
			internal static readonly SpeechActParser Instance = new SpeechActParser();

			// explicit static constructor to assure a single execution
			static Singleton()
			{
			}
		}

		SpeechActParser()
		{
		}

		public static SpeechActParser Instance
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
			
			SpeechActParameters s = new SpeechActParameters();
            
            String aux = xml.DocumentElement.Attributes["type"].InnerXml;
            if (aux != null)
            {
                s.ActionType = aux;
            }

            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
            {
                if (node.Name.Equals("Sender"))
                {
                    s.Subject = node.InnerText; 
                }
                else if (node.Name.Equals("Receiver"))
                {
                    s.Target = node.InnerText;
                }
                else if (node.Name.Equals("Type"))
                {
                    s.Meaning = node.InnerText;
                }
                else if (node.Name.Equals("Context"))
                {
                    s.AddContextVariable(node.Attributes["id"].InnerXml, node.InnerText);
                }
                else if (node.Name.Equals("Parameters"))
                {
                    foreach(XmlNode auxNode in node.ChildNodes)
                    {
                        s.AddParameter(auxNode.InnerText);
                    }
                }
                else if(node.Name.Equals("Utterance"))
                {
                    s.Utterance = node.InnerText;
                }
                else if(node.Name.Equals("Emotion"))
                {
                   // s.Emotion = Emotion.ParseEmotion(node);
                }
                else if (node.Name.Equals("AMSummary"))
                {
                    s.AMSummary = node.InnerXml;
                }
            }
			
			return s;
		}

       

		protected override void ParseElements(XmlDocument xml,object result) 
		{
		}
	}
}
