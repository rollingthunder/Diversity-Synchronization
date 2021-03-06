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

using MVVMDiversity.Model;
using System.Collections.Generic;
using MVVMDiversity.Interface;
using Microsoft.Practices.Unity;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using log4net;
using UBT.AI4.Bio.DivMobi.DatabaseConnector.Serializable;
using MVVMDiversity.Messages;
using System.Collections.ObjectModel;
using System.Collections;
using System.Linq;
using GalaSoft.MvvmLight.Threading;
using System;

namespace MVVMDiversity.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class SelectFieldDataViewModel : PageViewModel
    {
        ILog _Log = LogManager.GetLogger(typeof(SelectFieldDataViewModel));

        #region Dependencies

        private IFieldDataService _fd;
        [Dependency]
        public IFieldDataService FieldData 
        {
            get
            {
                return _fd;
            }
            set
            {
                if (value != null)
                {
                    if (_fd != null)
                        detachFromFDSvc();
                    _fd = value;
                    if (_fd != null)
                        attachToFDSvc();

                    VerifyPropertyName(SearchTypesPropertyName);

                    RaisePropertyChanged(SearchTypesPropertyName);
                }
            }
        }

        private void attachToFDSvc()
        {
            FieldData.SearchFinished += new AsyncOperationFinishedHandler<IList<ISerializableObject>>(SearchFinished);
        }        

        private void detachFromFDSvc()
        {
            FieldData.SearchFinished -= SearchFinished;
        }

        [Dependency]
        public IUserProfileService UserProfileSvc { get; set; }

        [Dependency]
        public IISOViewModelStore ISOStore { get; set; }
       
        #endregion   
     
        

        /// <summary>
        /// Initializes a new instance of the SelectFieldDataViewModel class.
        /// </summary>
        public SelectFieldDataViewModel()
            : base("SelectFD_Next","SelectFD_Previous","SelectFD_Title","SelectFD_Description")
        {
            NextPage = Messages.Page.FinalSelection;
            PreviousPage = Messages.Page.Actions;

            CanNavigateBack = true;
            CanNavigateNext = true;

            QueryDatabase = new RelayCommand(executeSearch,
                () => ConfiguredSearch != null);

            AddToSelection = new RelayCommand<IList>((selection) =>
            {
                if(SelectionTree == null)
                    SelectionTree = new AsyncTreeViewModel(ISOStore);

                if (_queryResult != null && selection != null)
                {
                    var typedSelection = Enumerable.Cast<IISOViewModel>(selection);                   

                    foreach (var generator in typedSelection)
                    {
                        if (!_selection.Contains(generator)) 
                            _selection.Add(generator);                        
                    }
                    if (typedSelection.Count() > 0)
                    {
                        RaiseSelectionChanged();                       
                    }
                }

            });

            RemoveFromSelection = new RelayCommand<IList>((selection) =>
                {
                    if(_queryResult != null && selection != null)
                    {
                        var typedSelection = Enumerable.Cast<IISOViewModel>(selection);                        

                        foreach (var generator in typedSelection)
                        {                            
                            _selection.Remove(generator);                            
                        }
                        if (selection.Count > 0)
                        {
                            RaiseSelectionChanged();                            
                        }
                    }
                });
            MessengerInstance.Register<ConnectionStateChanged>(this, (msg) =>
                {
                    //Repository Disconnected
                    if ((msg.Content & ConnectionState.ConnectedToRepository) != ConnectionState.ConnectedToRepository)
                        clearSelection();
                });

            MessengerInstance.Register<Settings>(this, (msg) => updateFromSettings(msg.Content));
            MessengerInstance.Send<SettingsRequest>(new SettingsRequest());
        }

        private void clearSelection()
        {
            ISOStore.Clear();

            _queryResult = null;
            RaiseQueryResultChanged();
            QueryResultTree = null;

            _selection = new HashSet<IISOViewModel>();
            RaiseSelectionChanged();
            SelectionTree = null;
        }

        void executeSearch()
        {
            if (FieldData != null)
            {
                if (UserProfileSvc != null)
                {                    
                    CurrentOperation = FieldData.startSearch(ConfiguredSearch, UserProfileSvc.ProjectID);
                }
                else
                    _Log.Error("UserProfileService N/A");
            }
            else
                _Log.Error("FieldDataService N/A");
        }

        void SearchFinished(AsyncOperationInstance<IList<ISerializableObject>> operation, IList<ISerializableObject> result)
        {
            List<IISOViewModel> selectionList = buildQueryResult(result);
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    _queryResult = selectionList;

                    QueryResultTree = new AsyncTreeViewModel(ISOStore);

                    RaiseQueryResultChanged();

                    CurrentOperation = null;                    
                });
                                    
        }

        private void updateFromSettings(DiversityUserOptions diversityUserOptions)
        {
            TruncateDataItems = diversityUserOptions.TruncateDataItems;
        }

        private List<IISOViewModel> buildQueryResult(IList<ISerializableObject> result)
        {
            CurrentOperation = new AsyncOperationInstance(true, null)
            {
                StatusDescription = "SelectFD_Status_FillingResults",
                Progress = 0,
                IsProgressIndeterminate = false,
            };       

            List<IISOViewModel> list = new List<IISOViewModel>(result.Count());
            if (result.Count() == 0)
            {
                sendNotification("SelectFD_ResultEmpty");
                return list;
            }

            ProgressInterval localProgress = null;
            localProgress = new ProgressInterval(CurrentOperation, 100f, result.Count());           

            var conversionQuery = from obj in result
                                  select ISOStore.addOrRetrieveVMForISO(obj);

            foreach(var line in conversionQuery)
            {
                list.Add(line);
                if (localProgress != null)
                    if (localProgress.IsCancelRequested)
                        break;
                    else
                        localProgress.advance();
            }
            return list;
        }

        private void RaiseQueryResultChanged()
        {
            // Verify Property Exists
            VerifyPropertyName(QueryResultPropertyName);

            // Update bindings, no broadcast
            RaisePropertyChanged(QueryResultPropertyName);

            if (QueryResult != null && QueryResult.Count() == 0)
                MessengerInstance.Send<StatusNotification>("SelectFD_ResultEmpty");
        }

        private void RaiseSelectionChanged()
        {
            VerifyPropertyName(SelectionPropertyName);
            RaisePropertyChanged(SelectionPropertyName);
        }

        #region Properties
        public ICommand QueryDatabase { get; private set; }      

        public ICommand AddToSelection { get; private set; }

        public ICommand RemoveFromSelection { get; private set; }

        /// <summary>
        /// The <see cref="SearchTypes" /> property's name.
        /// </summary>
        public const string SearchTypesPropertyName = "SearchTypes";       

        /// <summary>
        /// Available Search types
        /// </summary>
        public IEnumerable<SearchSpecification> SearchTypes
        {
            get
            {
                return (FieldData != null) ? FieldData.SearchTypes : null;
            }            
        }

        /// <summary>
        /// The <see cref="ConfiguredSearch" /> property's name.
        /// </summary>
        public const string ConfiguredSearchPropertyName = "ConfiguredSearch";

        private SearchSpecification _configuredSearch = null;

        /// <summary>
        /// 
        /// </summary>
        public SearchSpecification ConfiguredSearch
        {
            get
            {
                return _configuredSearch;
            }

            set
            {
                if (_configuredSearch == value)
                {
                    return;
                }

                var oldValue = _configuredSearch;
                _configuredSearch = value;                

                // Verify Property Exists
                VerifyPropertyName(ConfiguredSearchPropertyName);
                

                // Update bindings, no broadcast
                RaisePropertyChanged(ConfiguredSearchPropertyName);
                
            }
        }


        /// <summary>
        /// The <see cref="QueryResult" /> property's name.
        /// </summary>
        public const string QueryResultPropertyName = "QueryResult";

        private List<IISOViewModel> _queryResult = null;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IISOViewModel> QueryResult
        {
            get
            {
                if (_queryResult != null)
                    foreach (var item in _queryResult)
                        yield return item;
                else
                    yield return null;
            }            
        }

        /// <summary>
        /// The <see cref="QueryResultTree" /> property's name.
        /// </summary>
        public const string QueryResultTreePropertyName = "QueryResultTree";

        private AsyncTreeViewModel _qrTree = null;

        /// <summary>
        /// 
        /// </summary>
        public AsyncTreeViewModel QueryResultTree
        {
            get
            {
                return _qrTree;
            }

            set
            {
                if (_qrTree == value)
                {
                    return;
                }

                var oldValue = _qrTree;
                _qrTree = value;


                // Verify Property Exists
                VerifyPropertyName(QueryResultTreePropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(QueryResultTreePropertyName);

            }
        }

        /// <summary>
        /// The <see cref="Selection" /> property's name.
        /// </summary>
        public const string SelectionPropertyName = "Selection";

        private ICollection<IISOViewModel> _selection = new HashSet<IISOViewModel>();

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IISOViewModel> Selection
        {
            get
            {
                if(_selection != null)
                    foreach (var gen in _selection)
                        yield return gen;
            }           
        }

        /// <summary>
        /// The <see cref="SelectionTree" /> property's name.
        /// </summary>
        public const string SelectionTreePropertyName = "SelectionTree";

        private AsyncTreeViewModel _selectionTree = null;

        /// <summary>
        /// 
        /// </summary>
        public AsyncTreeViewModel SelectionTree
        {
            get
            {
                return _selectionTree;
            }

            private set
            {
                if (_selectionTree == value)
                {
                    return;
                }

                if (_selectionTree != null)
                    _selectionTree.SuspendUpdates = true;

                _selectionTree = value;                

                // Verify Property Exists
                VerifyPropertyName(SelectionTreePropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(SelectionTreePropertyName);                
            }
        }

        /// <summary>
        /// The <see cref="TruncateDataItems" /> property's name.
        /// </summary>
        public const string TruncateDataItemsPropertyName = "TruncateDataItems";

        private bool _truncate = false;

        /// <summary>
        /// Determines, whether adding an ISO to the selection also adds all ISOs below this one in the tree.
        /// </summary>
        public bool TruncateDataItems
        {
            get
            {
                return _truncate;
            }

            set
            {
                if (_truncate == value)
                {
                    return;
                }

                
                _truncate = value;               

                // Verify Property Exists
                VerifyPropertyName(TruncateDataItemsPropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(TruncateDataItemsPropertyName);
                
                if (QueryResultTree != null)
                    QueryResultTree.TruncateDataItems = _truncate;
                if (SelectionTree != null)
                    SelectionTree.TruncateDataItems = _truncate;
            }
        }

        #endregion

        public void QuerySelectionChanged(IEnumerable<IISOViewModel> added, IEnumerable<IISOViewModel> removed)
        {
            foreach (var i in added)
                QueryResultTree.addGenerator(i);
            foreach (var i in removed)
                QueryResultTree.removeGenerator(i);
        }

        public void SelectionChanged(IEnumerable<IISOViewModel> added, IEnumerable<IISOViewModel> removed)
        {
            foreach (var i in added)
                SelectionTree.addGenerator(i);
            foreach (var i in removed)
                SelectionTree.removeGenerator(i);
        }

        protected override void OnNavigateNext()
        {
            QueryResultTree.SuspendUpdates = true;
            SelectionTree.SuspendUpdates = true;

            MessengerInstance.Send<Messages.Selection>(new Selection(_selection,TruncateDataItems));

            base.OnNavigateNext();
        }
        

        
    }
}