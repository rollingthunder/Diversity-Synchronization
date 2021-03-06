﻿//#######################################################################
//Diversity Mobile Synchronization
//Project Homepage:  http://www.diversitymobile.net
//Copyright (C) 2011  Georg Rollinger
//
//This program is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 2 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License along
//with this program; if not, write to the Free Software Foundation, Inc.,
//51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
//#######################################################################

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVVMDiversity.Interface;
using System.IO;
using System.Xml;
using System.Net;
using Microsoft.Practices.Unity;
using MVVMDiversity.Model;
using log4net;

namespace MVVMDiversity.Services
{
    public class MapService : IMapService
    {
        private const string IMAGE_EXT = ".png";
        private const string META_EXT = ".xml";

        ILog _Log = LogManager.GetLogger(typeof(MapService));

        MapInfo _mapMetadata;

        public void saveMap(MapInfo metadata, string url, Action<MapInfo> finishedCallback)
        {
            lock (this)
            {
                _mapMetadata = metadata;
                var local = localPath();
                if(downloadFile(url, local + IMAGE_EXT) > 0)
                    writeSettingsToXML(local + META_EXT);
            }
            if (finishedCallback != null)
                finishedCallback(metadata);
        }



        private string localPath()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(ApplicationPathManager.getFolderPath(ApplicationFolder.Maps));
            builder.Append(@"\");
            builder.Append(_mapMetadata.Name);

            return builder.ToString();
        }

        private int downloadFile(String remoteFilename, String localFilename)
        {
            //Function will return the number of bytes processed
            //to the caller. Initialize to 0 here.
            int bytesProcessed = 0;

            //Assign values to these objects here so that they can
            //be referenced in the finally block
            Stream remoteStream = null;
            Stream localStream = null;
            WebResponse response = null;

            //Use a try/catch/finally block as both the WebRequest and Stream
            //classes throw exceptions upon error
            try
            {
                //Create a request for the specified remote file name
                WebRequest request = WebRequest.Create(remoteFilename);
                if (request != null)
                {
                    //Send the request to the server and retrieve the
                    //WebResponse object
                    response = request.GetResponse();
                    if (response != null)
                    {
                        //Once the WebResponse object has been retrieved,
                        //get the stream object associated with the response's data
                        remoteStream = response.GetResponseStream();

                        //Create the local file
                        localStream = File.Create(localFilename);

                        //Allocate a 1k buffer
                        byte[] buffer = new byte[1024];
                        int bytesRead;

                        //Simple do/while loop to read from stream until
                        //no bytes are returned
                        do
                        {
                            //Read data (up to 1k) from the stream
                            bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                            //Write the data to the local file
                            localStream.Write(buffer, 0, bytesRead);

                            //Increment total bytes processed
                            bytesProcessed += bytesRead;
                        } while (bytesRead > 0);
                    }
                }
            }
            catch (Exception e)
            {
                _Log.ErrorFormat("Error while downloading file: [{0}]", e);
            }
            finally
            {
                //Close the response and streams objects here
                //to make sure they're closed even if an exception
                //is thrown at some point
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
            }

            //Return total bytes processed to caller.
            return bytesProcessed;
        }

        private void writeSettingsToXML(String path)
        {
            XmlTextWriter writer = new XmlTextWriter(path, null);

            // console print out for testing purposes
            // XmlTextWriter writer = new XmlTextWriter(Console.Out);

            writer.Formatting = Formatting.Indented;

            // Starts a new document
            writer.WriteStartDocument();

            //Write comments
            writer.WriteComment("Map-Image Options");

            // Add elements to the file
            writer.WriteStartElement("ImageOptions", "");

            writer.WriteStartElement("Name", "");
            writer.WriteString(_mapMetadata.Name);
            writer.WriteEndElement();

            writer.WriteStartElement("Description", "");
            writer.WriteString(_mapMetadata.Description);
            writer.WriteEndElement();

            writer.WriteStartElement("SWLat", "");
            writer.WriteString(_mapMetadata.SWLat.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("SWLong", "");
            writer.WriteString(_mapMetadata.SWLong.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("SELat", "");
            writer.WriteString(_mapMetadata.SELat.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("SELong", "");
            writer.WriteString(_mapMetadata.SELong.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("NELat", "");
            writer.WriteString(_mapMetadata.NELat.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("NELong", "");
            writer.WriteString(_mapMetadata.NELong.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("ZoomLevel", "");
            writer.WriteString(_mapMetadata.ZoomLevel.ToString());
            writer.WriteEndElement();

            writer.WriteEndElement();

            // Ends the document
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();
        }

        private void readSettingsFromXML(String directory, String fileName)
        {
            if (Directory.Exists(directory))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directory);

                FileInfo[] fileInfo = dirInfo.GetFiles(fileName + META_EXT);

                if (fileInfo.Length > 0)
                {
                    XmlTextReader reader;
                    reader = new XmlTextReader(fileInfo[0].FullName);
                    reader.WhitespaceHandling = WhitespaceHandling.None;

                    while (reader.Read())
                    {
                        if (reader.LocalName.Equals("Name"))
                        {
                            _mapMetadata.Name = reader.ReadElementContentAsString();
                        }

                        if (reader.LocalName.Equals("Description"))
                        {
                            _mapMetadata.Description = reader.ReadElementContentAsString();
                        }

                        if (reader.LocalName.Equals("SWLat"))
                        {
                            _mapMetadata.SWLat = reader.ReadElementContentAsFloat();
                        }
                        if (reader.LocalName.Equals("SWLong"))
                        {
                            _mapMetadata.SWLong = reader.ReadElementContentAsFloat();
                        }

                        if (reader.LocalName.Equals("SELat"))
                        {
                            _mapMetadata.SELat = reader.ReadElementContentAsFloat();
                        }
                        if (reader.LocalName.Equals("SELong"))
                        {
                            _mapMetadata.SELong = reader.ReadElementContentAsFloat();
                        }

                        if (reader.LocalName.Equals("NELat"))
                        {
                            _mapMetadata.NELat = reader.ReadElementContentAsFloat();
                        }
                        if (reader.LocalName.Equals("NELong"))
                        {
                            _mapMetadata.NELong = reader.ReadElementContentAsFloat();
                        }
                    }
                }
            }
        }

        
    }
}
