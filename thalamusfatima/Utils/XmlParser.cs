// XmlParser.cs - 
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
using System.IO;
using System.Xml;
using System.Xml.Schema;

// beta
namespace ThalamusFAtiMA.Utils
{
	public abstract class XmlParser
	{
		private bool xmlErrors;

		protected XmlParser()
		{
			this.xmlErrors = false;
		}

		#region Errors Handling

		protected abstract void XmlErrorsHandler(object sender, ValidationEventArgs args);

		private void XmlErrorsCallBack(object sender, ValidationEventArgs args)
		{
			this.xmlErrors = true;
			this.XmlErrorsHandler(sender, args);
		}

		#endregion

		#region Parsing
		
		protected abstract object ParseElements(XmlDocument xml);

		protected abstract void ParseElements(XmlDocument xml, object elements);

		public object Parse(string text)
		{
			try
			{
				// convert text to a xml document
				XmlDocument xml = new XmlDocument();
				xml.Load(new XmlTextReader(new StringReader(text)));

				// parse elements
				return this.ParseElements(xml);
			}
			catch (Exception e)
			{
				// TODO: error log
				Console.WriteLine(e);

				return null;
			}
		}

		public void Parse(string text, object elements)
		{
			try
			{
				// convert text to a xml document
				XmlDocument xml = new XmlDocument();
				xml.Load(new XmlTextReader(new StringReader(text)));

				// parse elements
				this.ParseElements(xml, elements);
			}
			catch (Exception e)
			{
				// TODO: error log
				Console.WriteLine(e);
			}
		}

		public object ParseFile(string filename)
		{
			try
			{
				// load xml file
				XmlDocument xml = new XmlDocument();
				xml.Load(filename);

				// parse elements
				return this.ParseElements(xml);
			}
			catch (Exception e)
			{
				// TODO: error log
				Console.WriteLine(e);

				return null;
			}
		}

		public void ParseFile(string filename, object elements)
		{
			try
			{
				// load xml file
				XmlDocument xml = new XmlDocument();
				xml.Load(filename);

				// parse elements
				this.ParseElements(xml, elements);
			}
			catch (Exception e)
			{
				// TODO: error log
				Console.WriteLine(e);
			}
		}

		public object ParseFile(string filename, string schemaNamespace, string schemaFilename)
		{
            this.xmlErrors = false;

            // load schema
            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add(schemaNamespace, schemaFilename);

            // prepare xml validator
            XmlReader reader = XmlReader.Create(filename);
            reader.Settings.Schemas.Add(schema);
            reader.Settings.Schemas.ValidationEventHandler += new ValidationEventHandler(this.XmlErrorsCallBack);

            // load and validate xml file
            XmlDocument xml = new XmlDocument();
            xml.Load(reader);

			// validation ok -> parse elements
			if (xmlErrors == false)
			{
				return this.ParseElements(xml);
			}
			else
			{
				return null;
			}
		}

		public void ParseFile(string filename, object elements, string schemaNamespace, string schemaFilename)
		{
			this.xmlErrors = false;

			// load schema
            XmlSchemaSet schema = new XmlSchemaSet();
			schema.Add(schemaNamespace, schemaFilename);

			// prepare xml validator
            XmlReader reader = XmlReader.Create(filename);
            reader.Settings.Schemas.Add(schema);
			reader.Settings.Schemas.ValidationEventHandler += new ValidationEventHandler(this.XmlErrorsCallBack);
				
			// load and validate xml file
			XmlDocument xml = new XmlDocument();
            xml.Load(reader);

			// validation ok -> parse elements
			if (xmlErrors == false)
			{
				this.ParseElements(xml, elements);
			}
		}

		#endregion
	}
}
