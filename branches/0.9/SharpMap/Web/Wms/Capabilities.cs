// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
namespace SharpMap.Web.Wms
{
	/// <summary>
	/// Class for generating the WmsCapabilities Xml
	/// </summary>
	public class Capabilities
	{
		/// <summary>
		/// The Wms Service Description stores metadata parameters for a WMS service
		/// </summary>
		public struct WmsServiceDescription
		{
			/// <summary>
			/// Initializes a WmsServiceDescription object
			/// </summary>
			/// <param name="title">Mandatory Human-readable title for pick lists</param>
			/// <param name="onlineResource">Top-level web address of service or service provider.</param>
			public WmsServiceDescription(string title, string onlineResource)
			{
				Title = title;
				OnlineResource = onlineResource;
				Keywords = null;
				Abstract = "";
				ContactInformation = new WmsContactInformation();
				Fees = "";
				AccessConstraints = "";
				LayerLimit = 0;
				MaxWidth = 0;
				MaxHeight = 0;
			}
			/// <summary>
			/// Mandatory Human-readable title for pick lists
			/// </summary>
			public string Title;
			/// <summary>
			/// Optional narrative description providing additional information
			/// </summary>
			public string Abstract;
			/// <summary>
			/// Optional list of keywords or keyword phrases describing the server as a whole to help catalog searching
			/// </summary>
			public string[] Keywords;
			/// <summary>
			/// Mandatory Top-level web address of service or service provider.
			/// </summary>
			public string OnlineResource;
			/// <summary>
			/// Optional WMS contact information
			/// </summary>
			public WmsContactInformation ContactInformation;
			/// <summary>
			/// The optional element "Fees" may be omitted if it do not apply to the server. If
			/// the element is present, the reserved word "none" (case-insensitive) shall be used if there are no
			/// fees, as follows: "none".
			/// </summary>
			public string Fees;
			/// <summary>
			/// <para>The optional element "AccessConstraints" may be omitted if it do not apply to the server. If
			/// the element is present, the reserved word "none" (case-insensitive) shall be used if there are no
			/// access constraints, as follows: "none".</para>
			/// <para>When constraints are imposed, no precise syntax has been defined for the text content of these elements, but
			/// client applications may display the content for user information and action.</para>
			/// </summary>
			public string AccessConstraints;
			/// <summary>
			/// Maximum number of layers allowed (0=no restrictions)
			/// </summary>
			public uint LayerLimit;
			/// <summary>
			/// Maximum width allowed in pixels (0=no restrictions)
			/// </summary>
			public uint MaxWidth;
			/// <summary>
			/// Maximum height allowed in pixels (0=no restrictions)
			/// </summary>
			public uint MaxHeight;
		}

		/// <summary>
		/// Stores contact metadata about WMS service
		/// </summary>
		public struct WmsContactInformation
		{
			/// <summary>
			/// Primary contact person
			/// </summary>
			public ContactPerson PersonPrimary;
			/// <summary>
			/// Position of contact person
			/// </summary>
			public string Position;
			/// <summary>
			/// Address
			/// </summary>
			public ContactAddress Address;
			/// <summary>
			/// Telephone
			/// </summary>
			public string VoiceTelephone;
			/// <summary>
			/// Fax number
			/// </summary>
			public string FacsimileTelephone;
			/// <summary>
			/// Email address
			/// </summary>
			public string ElectronicMailAddress;
			/// <summary>
			/// Information about a contact person for the service.
			/// </summary>
			public struct ContactPerson
			{
				/// <summary>
				/// Primary contact person
				/// </summary>
				public string Person;
				/// <summary>
				/// Organisation of primary person
				/// </summary>
				public string Organisation;
			}
			/// <summary>
			/// Information about a contact address for the service.
			/// </summary>
			public struct ContactAddress
			{
				/// <summary>
				/// Type of address (usually "postal").
				/// </summary>
				public string AddressType;
				/// <summary>
				/// Contact address
				/// </summary>
				public string Address;
				/// <summary>
				/// Contact City
				/// </summary>
				public string City;
				/// <summary>
				/// State or province of contact
				/// </summary>
				public string StateOrProvince;
				/// <summary>
				/// Zipcode of contact
				/// </summary>
				public string PostCode;
				/// <summary>
				/// Country of contact address
				/// </summary>
				public string Country;
			}
		}

		private const string wmsNamespaceURI = "http://www.opengis.net/wms";
		private const string xlinkNamespaceURI = "http://www.w3.org/1999/xlink";
		
		/// <summary>
		/// Generates a capabilities file from a map object for use in WMS services
		/// </summary>
		/// <remarks>The capabilities document uses the v1.3.0 OpenGIS WMS specification</remarks>
		/// <param name="map">The map to create capabilities for</param>
		/// <param name="serviceDescription">Additional description of WMS</param>
		/// <returns>Returns XmlDocument describing capabilities</returns>
		public static XmlDocument GetCapabilities(SharpMap.Map map, WmsServiceDescription serviceDescription)
		{
			XmlDocument capabilities = new XmlDocument();
		
			
			//Set XMLSchema
			//capabilities.Schemas.Add(GetCapabilitiesSchema());

			//Instantiate an XmlNamespaceManager object.
			//System.Xml.XmlNamespaceManager xmlnsManager = new System.Xml.XmlNamespaceManager(capabilities.NameTable);
			//xmlnsManager.AddNamespace(xlinkNamespaceURI, "urn:Books");
		
			//Insert XML tag
			capabilities.InsertBefore(capabilities.CreateXmlDeclaration("1.0", "UTF-8", string.Empty), capabilities.DocumentElement);
			capabilities.AppendChild(capabilities.CreateComment("Capabilities generated by SharpMap v. " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()));
			//Create root node
			XmlNode RootNode = capabilities.CreateNode(XmlNodeType.Element, "WMS_Capabilities", wmsNamespaceURI);
			RootNode.Attributes.Append(CreateAttribute("version", "1.3.0", capabilities));
			
			XmlAttribute attr = capabilities.CreateAttribute("xmlns", "xsi", "http://www.w3.org/2000/xmlns/");
			attr.InnerText = "http://www.w3.org/2001/XMLSchema-instance";
			RootNode.Attributes.Append(attr);

			RootNode.Attributes.Append(CreateAttribute("xmlns:xlink", xlinkNamespaceURI, capabilities));
			XmlAttribute attr2 = capabilities.CreateAttribute("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
			attr2.InnerText = "http://schemas.opengis.net/wms/1.3.0/capabilities_1_3_0.xsd";
			RootNode.Attributes.Append(attr2);

			//Build Service node
			RootNode.AppendChild(GenerateServiceNode(ref serviceDescription, capabilities));

#if !CFBuild
			//Build Capability node
			RootNode.AppendChild(GenerateCapabilityNode(map, capabilities));
#endif
			capabilities.AppendChild(RootNode);

			//TODO: Validate output against schema

			return capabilities;
		}

		private static XmlNode GenerateServiceNode(ref WmsServiceDescription serviceDescription, XmlDocument capabilities)
		{
			//XmlNode ServiceNode = capabilities.CreateNode(XmlNodeType.Element, "Service", "");
			XmlElement ServiceNode = capabilities.CreateElement("Service", wmsNamespaceURI);
			ServiceNode.AppendChild(CreateElement("Name", "WMS", capabilities, false, wmsNamespaceURI));
			ServiceNode.AppendChild(CreateElement("Title", serviceDescription.Title, capabilities, false, wmsNamespaceURI)); //Add WMS Title
			if (!String.IsNullOrEmpty(serviceDescription.Abstract)) //Add WMS abstract
				ServiceNode.AppendChild(CreateElement("Abstract", serviceDescription.Abstract, capabilities, false, wmsNamespaceURI));
			if (serviceDescription.Keywords.Length > 0) //Add keywords
			{
				XmlElement KeywordListNode = capabilities.CreateElement("KeywordList", wmsNamespaceURI);
				foreach (string keyword in serviceDescription.Keywords)
					KeywordListNode.AppendChild(CreateElement("Keyword", keyword, capabilities, false, wmsNamespaceURI));
				ServiceNode.AppendChild(KeywordListNode);
			}
			//Add Online resource
			XmlElement OnlineResourceNode = GenerateOnlineResourceElement(capabilities, serviceDescription.OnlineResource);
			ServiceNode.AppendChild(OnlineResourceNode);

			//Add ContactInformation info
			XmlElement ContactInfoNode = GenerateContactInfoElement(capabilities, serviceDescription.ContactInformation);
			if(ContactInfoNode.HasChildNodes)
				ServiceNode.AppendChild(ContactInfoNode);

			if (serviceDescription.Fees != null && serviceDescription.Fees != string.Empty)
				ServiceNode.AppendChild(CreateElement("Fees", serviceDescription.Fees, capabilities, false, wmsNamespaceURI));
			if (serviceDescription.AccessConstraints != null && serviceDescription.AccessConstraints != string.Empty)
				ServiceNode.AppendChild(CreateElement("AccessConstraints", serviceDescription.AccessConstraints, capabilities, false, wmsNamespaceURI));
			if(serviceDescription.LayerLimit>0)
				ServiceNode.AppendChild(CreateElement("LayerLimit", serviceDescription.LayerLimit.ToString(), capabilities, false, wmsNamespaceURI));
			if (serviceDescription.MaxWidth > 0)
				ServiceNode.AppendChild(CreateElement("MaxWidth", serviceDescription.MaxWidth.ToString(), capabilities, false, wmsNamespaceURI));
			if (serviceDescription.MaxHeight > 0)
				ServiceNode.AppendChild(CreateElement("MaxHeight", serviceDescription.MaxHeight.ToString(), capabilities, false, wmsNamespaceURI));
			return ServiceNode;
		}

#if !CFBuild //v2.0 no System.Web
		private static XmlNode GenerateCapabilityNode(SharpMap.Map map, XmlDocument capabilities)
		{
			string OnlineResource = "";
			OnlineResource = System.Web.HttpContext.Current.Server.HtmlEncode("http://" + System.Web.HttpContext.Current.Request.Url.Host + System.Web.HttpContext.Current.Request.Url.AbsolutePath);
			XmlNode CapabilityNode = capabilities.CreateNode(XmlNodeType.Element, "Capability", wmsNamespaceURI);
			XmlNode RequestNode = capabilities.CreateNode(XmlNodeType.Element, "Request", wmsNamespaceURI);
			XmlNode GetCapabilitiesNode = capabilities.CreateNode(XmlNodeType.Element, "GetCapabilities", wmsNamespaceURI);
			//Set format of supported capabilities mime types (only text/xml supported)
			GetCapabilitiesNode.AppendChild(CreateElement("Format", "text/xml", capabilities, false, wmsNamespaceURI));
			GetCapabilitiesNode.AppendChild(GenerateDCPTypeNode(capabilities, OnlineResource));
			RequestNode.AppendChild(GetCapabilitiesNode);

			XmlNode GetMapNode = capabilities.CreateNode(XmlNodeType.Element, "GetMap", wmsNamespaceURI);
			//Add supported fileformats. Return the ones that GDI+ supports
			foreach (System.Drawing.Imaging.ImageCodecInfo encoder in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
				GetMapNode.AppendChild(CreateElement("Format", encoder.MimeType, capabilities, false, wmsNamespaceURI));
			
			GetMapNode.AppendChild(GenerateDCPTypeNode(capabilities,OnlineResource));

			RequestNode.AppendChild(GetMapNode);
			CapabilityNode.AppendChild(RequestNode);
			XmlElement exceptionNode = capabilities.CreateElement("Exception",wmsNamespaceURI);
			exceptionNode.AppendChild(CreateElement("Format", "text/xml", capabilities, false, wmsNamespaceURI));
			CapabilityNode.AppendChild(exceptionNode); //Add supported exception types

			//Build layerlist
			
			XmlNode LayerRootNode = capabilities.CreateNode(XmlNodeType.Element, "Layer", wmsNamespaceURI);
			LayerRootNode.AppendChild(CreateElement("Title", "SharpMap", capabilities, false, wmsNamespaceURI));
			LayerRootNode.AppendChild(CreateElement("CRS", "EPSG:" + map.Layers[0].SRID.ToString(), capabilities, false, wmsNamespaceURI)); //TODO
			LayerRootNode.AppendChild(GenerateBoundingBoxElement(map.GetExtents(), map.Layers[0].SRID, capabilities));
			//This should be changed when Transformation library is complete
			XmlElement geoBox = capabilities.CreateElement("EX_GeographicBoundingBox", wmsNamespaceURI);
			geoBox.Attributes.Append(CreateAttribute("minx", "-180", capabilities));
			geoBox.Attributes.Append(CreateAttribute("miny", "-90", capabilities));
			geoBox.Attributes.Append(CreateAttribute("maxx", "180", capabilities));
			geoBox.Attributes.Append(CreateAttribute("maxy", "90", capabilities));
			LayerRootNode.AppendChild(geoBox);

            foreach (SharpMap.Layers.ILayer layer in map.Layers)
				LayerRootNode.AppendChild(GetWmsLayerNode(layer, capabilities));

			CapabilityNode.AppendChild(LayerRootNode);
			
			return CapabilityNode;
		}
#endif
		private static XmlNode GenerateDCPTypeNode(XmlDocument capabilities, string OnlineResource)
		{
			XmlNode DcpType = capabilities.CreateNode(XmlNodeType.Element, "DCPType", wmsNamespaceURI);
			XmlNode HttpType = capabilities.CreateNode(XmlNodeType.Element, "HTTP", wmsNamespaceURI);
			XmlElement resource = GenerateOnlineResourceElement(capabilities, OnlineResource);

			XmlNode GetNode = capabilities.CreateNode(XmlNodeType.Element, "Get", wmsNamespaceURI);
			XmlNode PostNode = capabilities.CreateNode(XmlNodeType.Element, "Post", wmsNamespaceURI);
			GetNode.AppendChild(resource.Clone());
			PostNode.AppendChild(resource);
			HttpType.AppendChild(GetNode);
			HttpType.AppendChild(PostNode);
			DcpType.AppendChild(HttpType);
			return DcpType;
		}

		private static XmlElement GenerateOnlineResourceElement(XmlDocument capabilities, string OnlineResource)
		{
			XmlElement resource = capabilities.CreateElement("OnlineResource", wmsNamespaceURI);
			XmlAttribute attrType = capabilities.CreateAttribute("xlink", "type", xlinkNamespaceURI);
			attrType.Value = "simple";
			resource.Attributes.Append(attrType);
			XmlAttribute href = capabilities.CreateAttribute("xlink","href", xlinkNamespaceURI);
			href.Value = OnlineResource;
			resource.Attributes.Append(href);
			XmlAttribute xmlns = capabilities.CreateAttribute("xmlns:xlink");
			xmlns.Value = xlinkNamespaceURI;
			resource.Attributes.Append(xmlns);
			return resource;
		}

		private static XmlElement GenerateContactInfoElement(XmlDocument capabilities, WmsContactInformation info)
		{
			XmlElement infoNode = capabilities.CreateElement("ContactInformation", wmsNamespaceURI);
			
			//Add primary person
			XmlElement cpp = capabilities.CreateElement("ContactPersonPrimary", wmsNamespaceURI);
			if (info.PersonPrimary.Person != null && info.PersonPrimary.Person != String.Empty)
				cpp.AppendChild(CreateElement("ContactPerson", info.PersonPrimary.Person, capabilities, false, wmsNamespaceURI));
			if (info.PersonPrimary.Organisation!=null && info.PersonPrimary.Organisation!=String.Empty)
				cpp.AppendChild(CreateElement("ContactOrganization", info.PersonPrimary.Organisation, capabilities, false, wmsNamespaceURI));
			if (cpp.HasChildNodes)
				infoNode.AppendChild(cpp);

			if (info.Position != null && info.Position != string.Empty)
				infoNode.AppendChild(CreateElement("ContactPosition", info.Position, capabilities, false, wmsNamespaceURI));

			//Add address
			XmlElement ca = capabilities.CreateElement("ContactAddress", wmsNamespaceURI);
			if (info.Address.AddressType != null && info.Address.AddressType != string.Empty)
				ca.AppendChild(CreateElement("AddressType", info.Address.AddressType, capabilities, false, wmsNamespaceURI));
			if (info.Address.Address!=null && info.Address.Address != string.Empty)
				ca.AppendChild(CreateElement("Address", info.Address.Address, capabilities, false, wmsNamespaceURI));
			if (info.Address.City!=null && info.Address.City != string.Empty)
				ca.AppendChild(CreateElement("City", info.Address.City, capabilities, false, wmsNamespaceURI));
			if (info.Address.StateOrProvince!=null && info.Address.StateOrProvince != string.Empty)
				ca.AppendChild(CreateElement("StateOrProvince", info.Address.StateOrProvince, capabilities, false, wmsNamespaceURI));
			if (info.Address.PostCode !=null && info.Address.PostCode != string.Empty)
				ca.AppendChild(CreateElement("PostCode", info.Address.PostCode, capabilities, false, wmsNamespaceURI));
			if (info.Address.Country!=null && info.Address.Country != string.Empty)
				ca.AppendChild(CreateElement("Country", info.Address.Country, capabilities, false, wmsNamespaceURI));
			if (ca.HasChildNodes)
				infoNode.AppendChild(ca);

			if (info.VoiceTelephone!=null && info.VoiceTelephone != string.Empty)
				infoNode.AppendChild(CreateElement("ContactVoiceTelephone", info.VoiceTelephone, capabilities, false, wmsNamespaceURI));
			if (info.FacsimileTelephone!=null && info.FacsimileTelephone != string.Empty)
				infoNode.AppendChild(CreateElement("ContactFacsimileTelephone", info.FacsimileTelephone, capabilities, false, wmsNamespaceURI));
			if (info.ElectronicMailAddress!=null && info.ElectronicMailAddress != string.Empty)
				infoNode.AppendChild(CreateElement("ContactElectronicMailAddress", info.ElectronicMailAddress, capabilities, false, wmsNamespaceURI));

			return infoNode;
		}

		private static XmlNode GetWmsLayerNode(SharpMap.Layers.ILayer layer, XmlDocument doc)
		{
			XmlNode LayerNode = doc.CreateNode(XmlNodeType.Element, "Layer", wmsNamespaceURI);
			LayerNode.AppendChild(CreateElement("Name", layer.LayerName, doc, false, wmsNamespaceURI));
			LayerNode.AppendChild(CreateElement("Title", layer.LayerName, doc, false, wmsNamespaceURI));
			//If this is a grouplayer, add childlayers recursively
			if (layer.GetType() == typeof(SharpMap.Layers.LayerGroup))
				foreach (SharpMap.Layers.Layer childlayer in ((SharpMap.Layers.LayerGroup)layer).Layers)
					LayerNode.AppendChild(GetWmsLayerNode(childlayer, doc));		
			
			LayerNode.AppendChild(GenerateBoundingBoxElement(layer.Envelope, layer.SRID, doc));

			return LayerNode;
		}

		private static XmlElement GenerateBoundingBoxElement(SharpMap.Geometries.BoundingBox bbox, int SRID, XmlDocument doc)
		{
			XmlElement xmlBbox = doc.CreateElement("BoundingBox", wmsNamespaceURI);
			xmlBbox.Attributes.Append(CreateAttribute("minx", bbox.Left.ToString(SharpMap.Map.numberFormat_EnUS), doc));
			xmlBbox.Attributes.Append(CreateAttribute("miny", bbox.Bottom.ToString(SharpMap.Map.numberFormat_EnUS), doc));
			xmlBbox.Attributes.Append(CreateAttribute("maxx", bbox.Right.ToString(SharpMap.Map.numberFormat_EnUS), doc));
			xmlBbox.Attributes.Append(CreateAttribute("maxy", bbox.Top.ToString(SharpMap.Map.numberFormat_EnUS), doc));
			xmlBbox.Attributes.Append(CreateAttribute("CRS", "EPSG:" + SRID.ToString(), doc));
			return xmlBbox;
		}

		private static XmlAttribute CreateAttribute(string name, string value, XmlDocument doc)
		{
			XmlAttribute attr = doc.CreateAttribute(name);
			attr.Value = value;
			return attr;
		}

		private static XmlNode CreateElement(string name, string value, XmlDocument doc, bool IsXml, string namespaceURI)
		{
			XmlNode node = doc.CreateNode(XmlNodeType.Element, name, namespaceURI);
			if (IsXml)
				node.InnerXml = value;
			else
				node.InnerText = value;
			return node;
		}

		internal static XmlDocument CreateXml()
		{
			XmlDocument capabilities = new XmlDocument();
			//Set XMLSchema
			capabilities.Schemas = new System.Xml.Schema.XmlSchemaSet();
			capabilities.Schemas.Add(GetCapabilitiesSchema());

			return capabilities;
		}

		private static System.Xml.Schema.XmlSchema GetCapabilitiesSchema()
		{
			//Get XML Schema
			System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SharpMap.Web.Wms.Schemas._1._3._0.capabilities_1_3_0.xsd");
			System.Xml.Schema.XmlSchema schema = System.Xml.Schema.XmlSchema.Read(stream, new System.Xml.Schema.ValidationEventHandler(ValidationError));
			stream.Close();
			return schema;
		}

		private static void ValidationError(object sender, System.Xml.Schema.ValidationEventArgs arguments)
		{
		}
	}
}
