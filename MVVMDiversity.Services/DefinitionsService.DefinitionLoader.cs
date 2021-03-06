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
using log4net;
using UBT.AI4.Bio.DivMobi.DataLayer.DataItems;
using UBT.AI4.Bio.DivMobi.ListSynchronization;
using UBT.AI4.Bio.DivMobi.DatabaseConnector.Serializable;
using UBT.AI4.Bio.DivMobi.DatabaseConnector.Restrictions;
using System.Data;
using MVVMDiversity.Model;

namespace MVVMDiversity.Services
{
	public partial class DefinitionsService
	{
        private class DefinitionLoader
        {
            private ILog _Log = LogManager.GetLogger(typeof(DefinitionLoader));

            DefinitionsService _owner;
            
            private List<Type> _defTypes = new List<Type>()
            {
                typeof(Property), 
                typeof(CollEventImageType_Enum),
                typeof(CollSpecimenImageType_Enum), 
                typeof(CollTaxonomicGroup_Enum), 
                typeof(LocalisationSystem), 
                typeof(CollCircumstances_Enum), 
                typeof(CollUnitRelationType_Enum),             
                typeof(CollIdentificationCategory_Enum),
            };    
       
            public DefinitionLoader (DefinitionsService owner)
	        {
                _owner = owner;
	        }

            public void loadCollectionDefinitions(AsyncOperationInstance progress)
            {
                progress.StatusDescription = "Services_Definitions_LoadingCollectionDefinitions";
                
                var uOptions = _owner.Settings.getOptions();
                //TODO
                var connectionProfile = uOptions.CurrentConnection;

              

                //TODO
                var projectID =  _owner.Profiles.ProjectID;

                var repSerializer = _owner.Connections.Repository;

                var mobSerializer = _owner.Connections.MobileDB;

                ObjectSyncList transferList = new ObjectSyncList();                
                String sql = @"SELECT * FROM [" + connectionProfile.InitialCatalog + "].[dbo].[AnalysisProjectList] (" + projectID + ")";
                IList<ISerializableObject> list = repSerializer.Connector.LoadList(typeof(Analysis), sql);
                transferList.addList(list);
                foreach (ISerializableObject iso in list)
                {
                    Analysis ana = (Analysis)iso;
                    IRestriction rana = RestrictionFactory.Eq(typeof(AnalysisResult), "_AnalysisID", ana.AnalysisID);
                    IList<ISerializableObject> resultList = repSerializer.Connector.LoadList(typeof(AnalysisResult), rana);
                    transferList.addList(resultList);
                }
                sql = @"SELECT AnalysisID,TaxonomicGroup,RowGUID FROM [" + connectionProfile.InitialCatalog + "].[dbo].[AnalysisTaxonomicGroupForProject] (" + projectID + ")";
                IList<AnalysisTaxonomicGroup> atgList = new List<AnalysisTaxonomicGroup>();
                IDbConnection connRepository = repSerializer.CreateConnection();
                connRepository.Open();
                IDbCommand com = connRepository.CreateCommand();
                com.CommandText = sql;
                IDataReader reader = null;
                try
                {
                    reader = com.ExecuteReader();
                    while (reader.Read())
                    {
                        AnalysisTaxonomicGroup atg = new AnalysisTaxonomicGroup();
                        atg.AnalysisID = reader.GetInt32(0);
                        atg.TaxonomicGroup = reader.GetString(1);
                        atg.Rowguid = Guid.NewGuid();
                        atgList.Add(atg);
                    }
                    connRepository.Close();
                }
                catch (Exception e)
                {                    
                    connRepository.Close();
                    _Log.ErrorFormat("Error loading Collection Definitions: [{0}]", e);
                                        
                    progress.failure("Services_Definitions_Error_MissingRights","");
                }


                foreach (AnalysisTaxonomicGroup atg in atgList)
                {
                    foreach (ISerializableObject iso in list)
                    {
                        if (iso.GetType().Equals(typeof(Analysis)))
                        {
                            Analysis ana = (Analysis)iso;
                            if (ana.AnalysisID == atg.AnalysisID)
                            {
                                transferList.addObject(atg);
                            }
                        }
                    }
                }

                
                float progressPerType = 100f / _defTypes.Count;
                progress.IsProgressIndeterminate = false;
                foreach (Type t in _defTypes)
                {
                    repSerializer.Progress = new ProgressInterval(progress, progressPerType, 1);
                    transferList.Load(t, repSerializer);                    
                }
                transferList.initialize(LookupSynchronizationInformation.downloadDefinitionsList(), LookupSynchronizationInformation.getReflexiveReferences(), LookupSynchronizationInformation.getReflexiveIDFields());


                List<ISerializableObject> orderedObjects = transferList.orderedObjects;

                foreach (ISerializableObject iso in orderedObjects)
                {
                    try
                    {
                        mobSerializer.Connector.InsertPlain(iso);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            if (iso.GetType().Equals(typeof(AnalysisTaxonomicGroup)))
                            {
                                AnalysisTaxonomicGroup atg = (AnalysisTaxonomicGroup)iso;
                                IRestriction r1 = RestrictionFactory.Eq(iso.GetType(), "_AnalysisID", atg.AnalysisID);
                                IRestriction r2 = RestrictionFactory.Eq(iso.GetType(), "_TaxonomicGroup", atg.TaxonomicGroup);
                                IRestriction r = RestrictionFactory.And().Add(r1).Add(r2);
                                ISerializableObject isoStored = mobSerializer.Connector.Load(iso.GetType(), r);
                                atg.Rowguid = isoStored.Rowguid;
                            }
                            else
                            {
                                IRestriction r = RestrictionFactory.Eq(iso.GetType(), "_guid", iso.Rowguid);
                                ISerializableObject isoStored = mobSerializer.Connector.Load(iso.GetType(), r);
                            }
                            mobSerializer.Connector.UpdatePlain(iso);
                        }
                        catch (Exception ex)
                        {
                            _Log.ErrorFormat("Exception while transferring [{0}]: [{1}]", iso, ex);
                        }
                    }
                }

               
                progress.success();
            }
        }
	}
}
