﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UBT.AI4.Bio.DivMobi.DatabaseConnector.Serializable;
using System.ComponentModel;

namespace MVVMDiversity.Interface
{
    public delegate void SelectionBuildProgressChangedHandler(int rootCount, int currentRoot, IISOViewModel currentVM);

    public interface ITreeViewModel : INotifyPropertyChanged
    {
        IEnumerable<INodeViewModel> Roots {get;}

        bool TruncateDataItems { get; set; }
        
        void addGenerator(IISOViewModel vm);
        void removeGenerator(IISOViewModel vm);

        event SelectionBuildProgressChangedHandler SelectionBuildProgressChanged;
        IList<ISerializableObject> buildSelection();
       
    }
}
