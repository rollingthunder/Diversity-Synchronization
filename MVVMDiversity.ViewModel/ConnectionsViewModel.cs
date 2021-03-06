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

using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using MVVMDiversity.Interface;
using MVVMDiversity.Messages;
using MVVMDiversity.Model;
using Microsoft.Practices.Unity;
using System;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using GalaSoft.MvvmLight.Threading;

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
    public class ConnectionsViewModel : PageViewModel
    {

        private ILog _Log = LogManager.GetLogger(typeof(ConnectionsViewModel));

        #region Dependencies
        [Dependency]
        public IUserOptionsService UserOptions { get; set; }

        [Dependency]
        public IUserProfileService UserProfile { get; set; }

        [Dependency]
        public IConnectionManagementService ConnectionManager { get; set; }
        
        [Dependency]
        public ISessionManager SessionMgr
        {
            get;
            set;
        }
        #endregion

        

        #region Properties

        /// <summary>
        /// The <see cref="IsRepositoryConnected" /> property's name.
        /// </summary>
        public const string IsRepositoryConnectedPropertyName = "IsRepositoryConnected";

        private bool _isRepConnected = false;

        /// <summary>
        /// 
        /// </summary>
        public bool IsRepositoryConnected
        {
            get
            {
                return _isRepConnected;
            }

            private set
            {
                if (_isRepConnected == value)
                {
                    return;
                }

                var oldValue = _isRepConnected;
                _isRepConnected = value;                

                // Verify Property Exists
                VerifyPropertyName(IsRepositoryConnectedPropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(IsRepositoryConnectedPropertyName);

                updateCanChangeCreds();
            }
        }

        /// <summary>
        /// The <see cref="IsDefinitionsConnected" /> property's name.
        /// </summary>
        public const string IsDefinitionsConnectedPropertyName = "IsDefinitionsConnected";

        private bool _isDefConnected = false;

        /// <summary>
        /// 
        /// </summary>
        public bool IsDefinitionsConnected
        {
            get
            {
                return _isDefConnected;
            }

            private set
            {
                if (_isDefConnected == value)
                {
                    return;
                }

                var oldValue = _isDefConnected;
                _isDefConnected = value;                

                // Verify Property Exists
                VerifyPropertyName(IsDefinitionsConnectedPropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(IsDefinitionsConnectedPropertyName);                
            }
        }

        /// <summary>
        /// The <see cref="IsMobileConnected" /> property's name.
        /// </summary>
        public const string IsMobileConnectedPropertyName = "IsMobileConnected";

        private bool _IsMobileConnected = false;

        /// <summary>
        /// 
        /// </summary>
        public bool IsMobileConnected
        {
            get
            {
                return _IsMobileConnected;
            }

            private set
            {
                if (_IsMobileConnected == value)
                {
                    return;
                }

                var oldValue = _IsMobileConnected;
                _IsMobileConnected = value;                

                // Verify Property Exists
                VerifyPropertyName(IsMobileConnectedPropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(IsMobileConnectedPropertyName);                
            }
        }

        /// <summary>
        /// The <see cref="IsMobileTaxaConnected" /> property's name.
        /// </summary>
        public const string IsMobileTaxaConnectedPropertyName = "IsMobileTaxaConnected";

        private bool _isMobTaxConnected = false;

        /// <summary>
        /// 
        /// </summary>
        public bool IsMobileTaxaConnected
        {
            get
            {
                return _isMobTaxConnected;
            }

            private set
            {
                if (_isMobTaxConnected == value)
                {
                    return;
                }

                var oldValue = _isMobTaxConnected;
                _isMobTaxConnected = value;               

                // Verify Property Exists
                VerifyPropertyName(IsMobileTaxaConnectedPropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(IsMobileTaxaConnectedPropertyName);                
            }
        }

        /// <summary>
        /// The <see cref="RepositoryCatalog" /> property's name.
        /// </summary>
        public const string RepositoryCatalogPropertyName = "RepositoryCatalog";

        private string _repCat = "Repository Catalog";

        /// <summary>
        /// 
        /// </summary>
        public string RepositoryCatalog
        {
            get
            {
                return _repCat;
            }

            set
            {
                if (_repCat == value)
                {
                    return;
                }

                var oldValue = _repCat;
                _repCat = value;              

                // Verify Property Exists
                VerifyPropertyName(RepositoryCatalogPropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(RepositoryCatalogPropertyName);               
            }
        }

        /// <summary>
        /// The <see cref="DefinitionsCatalog" /> property's name.
        /// </summary>
        public const string DefinitionsCatalogPropertyName = "DefinitionsCatalog";

        private string _defCat = "Definitions Catalog";

        /// <summary>
        /// 
        /// </summary>
        public string DefinitionsCatalog
        {
            get
            {
                return _defCat;
            }

            set
            {
                if (_defCat == value)
                {
                    return;
                }

                var oldValue = _defCat;
                _defCat = value;               

                // Verify Property Exists
                VerifyPropertyName(DefinitionsCatalogPropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(DefinitionsCatalogPropertyName);

            }
        }

        /// <summary>
        /// The <see cref="Username" /> property's name.
        /// </summary>
        public const string UsernamePropertyName = "UserName";

        private string _user = "";

        /// <summary>
        /// 
        /// </summary>
        public string UserName
        {
            get
            {
                return _user;
            }

            set
            {
                if (_user == value)
                {
                    return;
                }

                var oldValue = _user;
                _user = value;               

                // Verify Property Exists
                VerifyPropertyName(UsernamePropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(UsernamePropertyName);

                if(_connectRepository != null)
                    _connectRepository.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The <see cref="Password" /> property's name.
        /// </summary>
        public const string PasswordPropertyName = "Password";

        private string _pass = "";

        /// <summary>
        /// 
        /// </summary>
        public string Password
        {
            get
            {
                return _pass;
            }

            set
            {
                if (_pass == value)
                {
                    return;
                }

                var oldValue = _pass;
                _pass = value;
                
                // Verify Property Exists
                VerifyPropertyName(PasswordPropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(PasswordPropertyName);

                if(_connectRepository != null)
                    _connectRepository.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The <see cref="IsPasswordVisible" /> property's name.
        /// </summary>
        public const string IsPasswordVisiblePropertyName = "IsPasswordVisible";

        private bool _isPasswordVisible = false;

        /// <summary>
        /// 
        /// </summary>
        public bool IsPasswordVisible
        {
            get
            {
                return _isPasswordVisible;
            }

            set
            {
                if (_isPasswordVisible == value)
                {
                    return;
                }

                var oldValue = _isPasswordVisible;
                _isPasswordVisible = value;

                // Verify Property Exists
                VerifyPropertyName(IsPasswordVisiblePropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(IsPasswordVisiblePropertyName);
            }
        }      

        private bool _credsRequired = false;

        /// <summary>
        /// 
        /// </summary>
        private bool UserCredentialsRequired
        {
            get
            {
                return _credsRequired;
            }

            set
            {
                if (_credsRequired == value)
                {
                    return;
                }
               
                _credsRequired = value;

                updateCanChangeCreds();
            }
        }

        /// <summary>
        /// The <see cref="CanChangeCredentials" /> property's name.
        /// </summary>
        public const string CanChangeCredentialsPropertyName = "CanChangeCredentials";

        private bool _canChangeCreds = true;

        /// <summary>
        /// 
        /// </summary>
        public bool CanChangeCredentials
        {
            get
            {
                return _canChangeCreds;
            }

            private set
            {
                if (_canChangeCreds == value)
                {
                    return;
                }

               
                _canChangeCreds = value;                

                // Verify Property Exists
                VerifyPropertyName(CanChangeCredentialsPropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(CanChangeCredentialsPropertyName);               
            }
        }

        /// <summary>
        /// The <see cref="ConnectionsProfile" /> property's name.
        /// </summary>
        public const string ConnectionsProfilePropertyName = "ConnectionsProfile";

        private string _connProfile = "Profile Name";

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionsProfile
        {
            get
            {
                return _connProfile;
            }

            set
            {
                if (_connProfile == value)
                {
                    return;
                }

                var oldValue = _connProfile;
                _connProfile = value;                

                // Verify Property Exists
                VerifyPropertyName(ConnectionsProfilePropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(ConnectionsProfilePropertyName);              
            }
        }

        /// <summary>
        /// The <see cref="MobileDBPath" /> property's name.
        /// </summary>
        public const string MobileDBPathPropertyName = "MobileDBPath";

        private string _mobDB = "Mobile DB";

        /// <summary>
        ///
        /// </summary>
        public string MobileDBPath
        {
            get
            {
                return _mobDB;
            }

            set
            {
                if (_mobDB == value)
                {
                    return;
                }

                var oldValue = _mobDB;
                _mobDB = value;               

                // Verify Property Exists
                VerifyPropertyName(MobileDBPathPropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(MobileDBPathPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="MobileTaxaPath" /> property's name.
        /// </summary>
        public const string MobileTaxaPathPropertyName = "MobileTaxaPath";

        private string _mobTax = "Mobile Taxa";

        /// <summary>
        /// 
        /// </summary>
        public string MobileTaxaPath
        {
            get
            {
                return _mobTax;
            }

            set
            {
                if (_mobTax == value)
                {
                    return;
                }

                var oldValue = _mobTax;
                _mobTax = value;

                // Verify Property Exists
                VerifyPropertyName(MobileTaxaPathPropertyName);

                // Update bindings, no broadcast
                RaisePropertyChanged(MobileTaxaPathPropertyName);

               
            }
        }




        private DelegateCommand _connectRepository;
        public ICommand ConnectRepository { get { return _connectRepository; } }

        public ICommand DisConnectRepository { get; private set; }

        public ICommand ConnectMobile { get; private set; }
        public ICommand DisConnectMobile { get; private set; }
        
        #endregion

        private bool _repConnecting = false;
        private bool RepositoryConnecting
        {
            get
            {
                return _repConnecting;
            }
            set
            {
                _repConnecting = value;
                if(_connectRepository != null)
                    _connectRepository.RaiseCanExecuteChanged();

                updateCanChangeCreds();
            }
        }

       

        private bool _mobileConnecting;

        private DiversityUserOptions _settings;

        /// <summary>
        /// Initializes a new instance of the ConnectionsViewModel class.
        /// </summary>
        public ConnectionsViewModel(IConnectionManagementService connections, IUserOptionsService userOptions)
            : base("ConnectionsPage_NextText", "", "ConnectionsPage_Title", "ConnectionsPage_Description")
        {
            if (connections == null)
                throw new ArgumentNullException("connections");
            if (userOptions == null)
                throw new ArgumentNullException("userOptions");

            this.ConnectionManager = connections;
            this.UserOptions = userOptions;
            this.RepositoryConnecting = false;
            this._mobileConnecting = false;

            CanNavigateBack = false;

            MessengerInstance.Register<ConnectionStateChanged>(this, (msg) => 
            {                
                updateFromConnectionState(msg.Content);                
            });
            MessengerInstance.Register<Settings>(this, (msg) => { updateFromSettings(msg.Content); });
            MessengerInstance.Register<ExecuteAction>(this, (msg) =>
                {
                    if (msg.Content == Enums.Action.DisconnectRepositories)
                        disconnectFromRepository();
                });
            
            _connectRepository = new DelegateCommand(
                () =>
                {
                    connectToRepository();
                },
                () =>
                {
                    return !this.RepositoryConnecting && (loginInformationFilled() || !this.UserCredentialsRequired);
                });

            
            DisConnectRepository = new RelayCommand(
                () =>
                {
                    disconnectFromRepository();
                });

            ConnectMobile = new RelayCommand(
                () =>
                {
                    connectToMobile();                  
                },
                () => { return !this._mobileConnecting; });
            DisConnectMobile = new RelayCommand(
                () =>
                {
                    disconnectFromMobile();
                });

            NextPage = Page.ProjectSelection;
            PreviousPage = Page.Connections;

            if (ConnectionManager != null)
                updateFromConnectionState(ConnectionManager.State);
            else
                _Log.Info("Couldn't update ConnectionState: ConnectionManager N/A");

            MessengerInstance.Send<SettingsRequest>(new SettingsRequest());
               
        }

        private void disconnectFromMobile()
        {
            new Action(() =>
            {
                SessionMgr.endSession();
                ConnectionManager.disconnectFromMobileDB();
            }).BeginInvoke(null, null);
        }

        private void connectToMobile()
        {
            if (SessionMgr.canResumeSession())
                showYesNoBox("ConnectionsPage_ResumeSession_Title", "ConnectionsPage_ResumeSession_Description", System.Windows.MessageBoxResult.No,
                    (res) =>
                    {
                        if (res == System.Windows.MessageBoxResult.Yes)
                        {
                            new Action<DBPaths>(connectTo).BeginInvoke(SessionMgr.resumeSession(_settings.Paths), null, null);
                        }
                        else
                        {
                            SessionMgr.startSession();
                            new Action<DBPaths>(connectTo).BeginInvoke(SessionMgr.createWorkingCopies(_settings.Paths), null, null);
                        }
                    });
            else
            {
                SessionMgr.startSession();
                new Action<DBPaths>(connectTo).BeginInvoke(SessionMgr.createWorkingCopies(_settings.Paths), null, null);
            }
        }

        private void connectToRepository()
        {
            this.RepositoryConnecting = true;

            var uOptions = UserOptions.getOptions();
            uOptions.Username = UserName;
            UserOptions.setOptions(uOptions);

            string pass = Password;
            new Action(() =>
            {
                if (uOptions.UseSqlAuthentification)
                    ConnectionManager.connectRepositorySqlAuth(uOptions.CurrentConnection, UserName, pass);
                else
                    ConnectionManager.connectRepositoryWinAuth(uOptions.CurrentConnection);
                RepositoryConnecting = false;
            }).BeginInvoke((res) =>
            {

            }, null);
        }

        private void disconnectFromRepository()
        {
            new Action(() =>
            {                
                ConnectionManager.disconnectFromRepository();
            }).BeginInvoke(null, null);
        }

        private void connectTo(DBPaths paths)
        {
            if (paths != null)
            {                
                ConnectionManager.connectToMobileDB(paths);
            }
            else
                showWorkingCopyFailure();
        }

        private void showWorkingCopyFailure()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () =>
                {
                    showMessageBox("ConnectionsPage_CreateWorkingCopies_Title", "ConnectionsPage_CreateWorkingCopies_Description", null);
                });
        }

       

        protected override void OnNavigateNext()
        {
            if (UserProfile != null)
            {
                UserProfile.ProfileLoaded += new AsyncOperationFinishedHandler(UserProfile_ProfileLoaded);
                UserProfile.tryLoadProfile();
            }
        }

        void UserProfile_ProfileLoaded(AsyncOperationInstance operation)
        {
            if (UserProfile != null)
                UserProfile.ProfileLoaded -= UserProfile_ProfileLoaded;

            if (operation.State == OperationState.Succeeded)
            {
                string currentDB = null;
                string homeDB = null;
                if (UserProfile != null)
                {
                    homeDB = UserProfile.HomeDB;
                }
                if (UserOptions != null)
                {
                    var options = UserOptions.getOptions();
                    if (options != null && options.CurrentConnection != null)
                    {
                        currentDB = options.CurrentConnection.InitialCatalog;
                    }
                }
                if (currentDB != homeDB)
                {
                    nonHomeDBConnected();                    
                }
                else
                    base.OnNavigateNext();

               
            }
            else
                showError(operation);
        }

        private void nonHomeDBConnected()
        {
            showYesNoBox("ConnectionsPage_NonHomeDB_Title", "ConnectionsPage_NonHomeDB_Content", System.Windows.MessageBoxResult.No,
                (result) =>
                {
                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        MessengerInstance.Send<ExecuteAction>(Enums.Action.CleanDB);
                    }
                });            
        }

        

        private void updateFromSettings(DiversityUserOptions settings)
        {
            if (settings != null)
            {
                _settings = settings;
                IsPasswordVisible = settings.PasswordVisible;
                UserCredentialsRequired = settings.UseSqlAuthentification;
                if (settings.CurrentConnection != null)
                {
                    ConnectionsProfile = settings.CurrentConnection.Name;
                    RepositoryCatalog = settings.CurrentConnection.InitialCatalog;
                    DefinitionsCatalog = settings.CurrentConnection.TaxonNamesInitialCatalog;
                }
                
                if(!IsRepositoryConnected)
                    UserName = settings.Username;
                MobileDBPath = settings.Paths.MobileDB;
                MobileTaxaPath = settings.Paths.MobileTaxa;
            }
        }


        private void updateFromConnectionState(ConnectionState state)
        {
            var definitionsWasConnected = IsDefinitionsConnected;
            IsDefinitionsConnected = stateHasFlag(state, ConnectionState.ConnectedToRepTax);
            IsMobileConnected = stateHasFlag(state, ConnectionState.ConnectedToMobile);
            IsRepositoryConnected = stateHasFlag(state, ConnectionState.ConnectedToRepository);
            IsMobileTaxaConnected = stateHasFlag(state, ConnectionState.ConnectedToMobileTax);


            CanNavigateNext = (IsDefinitionsConnected && IsRepositoryConnected && IsMobileConnected && IsMobileTaxaConnected);

            //Connection to Repository failed
            if (!definitionsWasConnected && IsDefinitionsConnected && !IsRepositoryConnected)
            {
                showMessageBox("ConnectionsPage_ConnectionFailed_Title", "ConnectionsPage_ConnectionFailed_Content", null);
            }
        }

        private void updateCanChangeCreds()
        {
            CanChangeCredentials = !RepositoryConnecting && !IsRepositoryConnected && UserCredentialsRequired;
        }

        private bool loginInformationFilled()
        {
            bool passwordEntered = !string.IsNullOrEmpty(Password);
            bool userEntered = !string.IsNullOrEmpty(UserName);

            return userEntered && passwordEntered;                                    
        }

        private bool stateHasFlag(ConnectionState s, ConnectionState f)
        {
            return (s & f) == f;
        }       

       
    }
}