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
using UBT.AI4.Bio.DivMobi.ListSynchronization;
using UBT.AI4.Bio.DivMobi.DataLayer.DataItems;
using UBT.AI4.Bio.DivMobi.DatabaseConnector.Serializer;
using log4net;
using UBT.AI4.Bio.DivMobi.DatabaseConnector.Serializable;
using System.ComponentModel;
using MVVMDiversity.Model;

namespace MVVMDiversity.Services
{
    public partial class FieldDataService
    {
        private class Synchronizer
        {
            ILog _Log = LogManager.GetLogger(typeof(Synchronizer));
            FieldDataService _owner;
            public Synchronizer(FieldDataService owner)
            {
                _owner = owner;
            }

            private int _projectID;
            string _userNr;
            IList<ISerializableObject> _selection;
            BackgroundOperation _progress;
            AnalyzeSyncObjectList _ansl;            
            Serializer _mobileDB;
            Serializer _repository;
            Serializer _sync;



            public void uploadFieldDataWorker(string userNr, int projectID)
            {


                getSerializers();

                _userNr = userNr;
                _projectID = projectID;
                

                if (_mobileDB != null && _repository != null && _sync != null)
                {
                    configureUploadANSL();                   

                    syncData();
                }
                else
                    _Log.Error("At least one Serializer is missing!");                 
                
            }            

            public void downloadFieldDataWorker(IList<ISerializableObject> selection)
            {
                getSerializers();

                _selection = selection;

                if (_mobileDB != null && _repository != null && _sync != null)
                {
                    configureDownloadANSL(selection);
                    
                    syncData();
                }
                else
                    _Log.Error("At least one Serializer is missing!");
            }

            private void configureDownloadANSL(IList<ISerializableObject> selection)
            {
                var syncList = new ObjectSyncList();
                syncList.addList(selection);
                syncList.initialize(LookupSynchronizationInformation.getFieldDataList(), LookupSynchronizationInformation.getReflexiveReferences(), LookupSynchronizationInformation.getReflexiveIDFields());

                _ansl = new AnalyzeSyncObjectList(syncList, _mobileDB, _repository, _sync);
            }

            private void getSerializers()
            {
                if (_owner.Connections != null)
                {
                    _mobileDB = _owner.Connections.MobileDB;
                    _repository = _owner.Connections.Repository;
                    _sync = _owner.Connections.Synchronization;
                }
                else
                    _Log.Error("ConnectionsProvider N/A");

                
            }

            private void configureUploadANSL()
            {
                var syncList = new ObjectSyncList();
                foreach (Type t in uploadTypes())
                {
                    syncList.Load(t, _mobileDB);
                }
                syncList.initialize(LookupSynchronizationInformation.getFieldDataList(), LookupSynchronizationInformation.getReflexiveReferences(), LookupSynchronizationInformation.getReflexiveIDFields());
                var snsb = new SNSBPictureTransfer(_userNr, _projectID, _mobileDB, ApplicationPathManager.getFolderPath(ApplicationFolder.Pictures));
                _ansl = new AnalyzeSyncObjectList(syncList, _mobileDB, _repository, _sync, snsb);
            }

            private void syncData()
            {
                _ansl.analyzeAll();

                var ignoredStates = new List<SyncStates_Enum>() 
                {
                    SyncStates_Enum.ConflictState,
                    SyncStates_Enum.ConflictResolvedState,
                    SyncStates_Enum.SynchronizedState,
                    SyncStates_Enum.UpdateState,
                    SyncStates_Enum.DeletedState,
                    SyncStates_Enum.PrematureState
                };

                //Alles außer InsertState auf ignore setzen
                foreach (var state in ignoredStates)
                {
                    var objectsOfState = _ansl.getObjectsOfState(state);
                    foreach (var objectOfState in objectsOfState)
                        objectOfState.State = SyncStates_Enum.IgnoreState;
                }

                _ansl.synchronizeAll();
            }

            private IList<Type> uploadTypes()
            {
                return new List<Type>()
                {
                    typeof(CollectionAgent),
                    typeof(CollectionEvent),
                    typeof(CollectionEventImage),
                    typeof(CollectionEventLocalisation),
                    typeof(CollectionEventProperty),
                    typeof(CollectionSpecimen),
                    typeof(CollectionSpecimenImage),
                    typeof(Identification),
                    typeof(IdentificationUnit),
                    typeof(IdentificationUnitAnalysis),
                    typeof(IdentificationUnitGeoAnalysis),
                    typeof(CollectionEventSeries),
                    typeof(CollectionProject),
                    //Bis hier: Korrepondiert zu DBVersion 31
                };
            }
            
        }
    }
}

