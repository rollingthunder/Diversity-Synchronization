﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVVMDiversity.ViewModel;
using System.ComponentModel;

namespace MVVMDiversity.Interface
{
    public interface INodeViewModel : INotifyPropertyChanged
    {
        string Name { get; }
        ISOIcon Icon { get; }



        bool IsExpanded { get; set; }
        bool IsGenerator { get; }
        IEnumerable<INodeViewModel> Children { get; }
        IEnumerable<IISOViewModel> Properties { get; }


    }
}
