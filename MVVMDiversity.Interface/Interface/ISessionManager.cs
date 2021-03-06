﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVVMDiversity.Model;

namespace MVVMDiversity.Interface
{
    public interface ISessionManager
    {
        void startSession();
        DBPaths createWorkingCopies(DBPaths paths);
        DBPaths createCleanWorkingCopies(DBPaths paths);
        bool endSession();

        bool canResumeSession();
        DBPaths resumeSession(DBPaths paths);

    }
}
