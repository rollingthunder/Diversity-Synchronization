﻿//#######################################################################
//Diversity Mobile Synchronization
//Project Homepage:  http://www.diversitymobile.net
//Copyright (C) 2011  Tobias Schneider, Lehrstuhl Angewndte Informatik IV, Universität Bayreuth
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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UBT.AI4.Bio.DivMobi.DatabaseConnector.Attributes;

namespace UBT.AI4.Bio.DivMobi.DataLayer.SyncAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple=true)]
    public class CheckForeingKeyConstraintAttribute : Attribute, ITarget
    {
        private String _Target = AttributeConstants.DEFAULT_TARGET;
        private String _UnmappedTable;
        private String _ExternColumn = null;

        public CheckForeingKeyConstraintAttribute(String unmappedTable)
        {
            _UnmappedTable = unmappedTable;
        }

        public String Target { get { return _Target; } set { _Target = value; } }

        public String ExternColumn { 
            get 
            {
                return _ExternColumn;
            } 
            set { _ExternColumn = value; } }

        public String UnmappedTable
        {
            get { return _UnmappedTable; }
        }
    }
}
