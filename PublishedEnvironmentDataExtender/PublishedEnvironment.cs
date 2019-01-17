using PublishedEnvironmentDataExtender.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Tridion.ContentManager;
using Tridion.Web.UI.Core.Extensibility;
using ItemType = Tridion.ContentManager.ItemType;


namespace PublishedEnvironmentDataExtender
{
    public class PublishedEnvironment : DataExtender
    {
        public override string Name
        {
            get
            {
                Type ns = this.GetType();
                return String.Concat(ns.Namespace, ".", ns.Name);
            }
        }

        public override XmlTextReader ProcessRequest(XmlTextReader reader, PipelineContext context)
        {
            return reader;
        }

        public override XmlTextReader ProcessResponse(XmlTextReader reader, PipelineContext context)
        {
            XmlTextReader xReader = reader;
            string command = context.Parameters["command"] as String;
            if (command == "GetList")
            {
                try
                {
                    xReader = ProcessPublishedContext(reader, context);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("ProcessResponseEXCEPTION " + "ProcessResponse" + ex.Message + "Stack Trace" + ex.StackTrace);
                }
            }
            return xReader;
        }
        
        private XmlTextReader ProcessPublishedContext(XmlTextReader xReader, PipelineContext context)
        {
            TextWriter sWriter = new StringWriter();
            XmlTextWriter xWriter = new XmlTextWriter(sWriter);

            xReader.MoveToContent();

            while (!xReader.EOF)
            {
                switch (xReader.NodeType)
                {
                    case XmlNodeType.Element:
                        xWriter.WriteStartElement(xReader.Prefix, xReader.LocalName, xReader.NamespaceURI);
                        xWriter.WriteAttributes(xReader, false);
                        try
                        {
                            if (IsValidItem(xReader))
                            {
                                using (var client = new CoreServiceUtility())
                                {

                                    string id = xReader.GetAttribute("ID"); // Get the id of item from List 
                                    string catman = "catman-";
                                    string bptman = "bptman-";
                                    if (id.StartsWith(catman))
                                    {
                                        id = id.ToString().Replace(catman, string.Empty);
                                    }
                                    else if (id.StartsWith(bptman))
                                    {
                                        id = id.ToString().Replace(bptman, string.Empty);
                                    }
                                    TcmUri uri = new TcmUri(id);
									// Validate the the types of items
                                    if (uri.ItemType == ItemType.Component || uri.ItemType == ItemType.Page || uri.ItemType == ItemType.StructureGroup || uri.ItemType == ItemType.Category)
                                    {
                                        string publishedCotexts = null;
                                        List<string> publishedPurposeList = new List<string>();
                                        try
                                        {
                                            var publishInfo = client.GetListPublishInfo(id);
                                            //Validate item is published
                                            if (publishInfo.Count() > 0)
                                            {
                                                foreach (var info in publishInfo)
                                                {
                                                    if (!String.IsNullOrEmpty(info.TargetPurpose))
                                                    {
														//Validate item is published from current publication
                                                        if(client.IsPublished(id, info.TargetPurpose))
                                                        {
                                                            publishedPurposeList.Add(info.TargetPurpose);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Trace.TraceError($"TargetInfo is NUll");
                                                    }
                                                }

                                                if (publishedPurposeList.Count > 0)
                                                {
                                                    publishedCotexts = String.Join(",", publishedPurposeList.Distinct());
                                                    xWriter.WriteAttributeString("PublishedTo", publishedCotexts); // Add the data into the list
                                                }
                                                else
                                                {
                                                    xWriter.WriteAttributeString("PublishedTo", "-");
                                                }
                                            }
                                            else {
                                                xWriter.WriteAttributeString("PublishedTo", "-");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Trace.TraceError("Exception :" + ex.Message + "Stack Trace :" + ex.StackTrace);
                                        }
                                    }
                                    else {
                                        xWriter.WriteAttributeString("PublishedTo", "N/A");
                                        xReader.MoveToElement();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("Exception: " + ex.Message + "Stack Trace :" + ex.StackTrace);
                        }
                        if (xReader.IsEmptyElement)
                        {
                            xWriter.WriteEndElement();
                        }
                        break;

                    case XmlNodeType.EndElement:
                        xWriter.WriteEndElement();
                        break;

                    case XmlNodeType.CDATA:
                        xWriter.WriteCData(xReader.Value);
                        break;

                    case XmlNodeType.Comment:
                        xWriter.WriteComment(xReader.Value);
                        break;

                    case XmlNodeType.DocumentType:
                        xWriter.WriteDocType(xReader.Name, null, null, null);
                        break;

                    case XmlNodeType.EntityReference:
                        xWriter.WriteEntityRef(xReader.Name);
                        break;

                    case XmlNodeType.ProcessingInstruction:
                        xWriter.WriteProcessingInstruction(xReader.Name, xReader.Value);
                        break;

                    case XmlNodeType.SignificantWhitespace:
                        xWriter.WriteWhitespace(xReader.Value);
                        break;

                    case XmlNodeType.Text:
                        xWriter.WriteString(xReader.Value);
                        break;

                    case XmlNodeType.Whitespace:
                        xWriter.WriteWhitespace(xReader.Value);
                        break;
                }
                xReader.Read();
            };
            xWriter.Flush();
            xReader = new XmlTextReader(new StringReader(sWriter.ToString()));
            xReader.MoveToContent();  
            return xReader;
        }

        /// <summary>
        /// Check if node is item node 
        /// </summary>
        /// <param name="xReader"></param>
        /// <returns>True if Item node exist</returns>
        public static bool IsValidItem(XmlTextReader xReader)
        {
            if (xReader.LocalName == "Item")
                return true;
            else
                return false;
        }
    }
}
