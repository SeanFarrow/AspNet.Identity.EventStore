﻿// Copyright 2014 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace AspNet.Identity.EventStore.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// A class to run and dispose the EventStore executable.
    /// </summary>
    internal class EventStoreRunner: IDisposable
    {
        bool _disposed;
bool _purgedata;
private string _additionalcommandlinearguments;
        Process _eventstoreprocess;
        public EventStoreRunner(bool PurgeData =false, string additionalCommandlineArguments=null)
        {
this._purgedata =PurgeData;
            this._additionalcommandlinearguments = additionalCommandlineArguments;
            string test = FindEventStorePath();
            var thread = new Thread(this.StartEventStore)
                             {
                                 IsBackground = true
                             };
        thread.Start();
        }

        private void StartEventStore()
        {
            var eventStorePath = FindEventStorePath();
            if (eventStorePath == null)
            {
                throw new FileNotFoundException("The EventStore binaries are not in the project directory structure.");
            }
 var startInfo = new ProcessStartInfo
        {
            FileName = eventStorePath,
            WindowStyle = ProcessWindowStyle.Normal,
            ErrorDialog = true,
            LoadUserProfile = true,
            CreateNoWindow = false,
            UseShellExecute = false
        };

            var cmdline = "--db=data";
if (String.IsNullOrEmpty(this._additionalcommandlinearguments))
{
    cmdline = cmdline + ' ' + this._additionalcommandlinearguments;
}

            startInfo.Arguments = cmdline;

                try
    {
        this._eventstoreprocess= new Process { StartInfo = startInfo };
 
        this._eventstoreprocess.Start();
        this._eventstoreprocess.WaitForExit();
    }
    catch
    {
this._eventstoreprocess.CloseMainWindow();
        this._eventstoreprocess.Dispose();
    }
        }

        private static string FindEventStorePath()
        {
            string eventStoreBinariesFolderAndExe = Path.Combine("EventStore Binaries", "EventStore.ClusterNode.exe");
            string assemblyPath = Assembly.GetExecutingAssembly().GetExecutingFolder();
            string finalEventStorePath =Path.Combine(assemblyPath, eventStoreBinariesFolderAndExe);
            bool eventStoreExeFound = default(bool);
            int backslashIndex = -1;
            do
            {
                if (!File.Exists(finalEventStorePath))
                {
                    backslashIndex =assemblyPath.LastIndexOf('\\');
                    assemblyPath = assemblyPath.Remove(backslashIndex);
                    finalEventStorePath = Path.Combine(assemblyPath, eventStoreBinariesFolderAndExe);
                }
                else
                {
                    eventStoreExeFound = true;
                }
            }
            while (!eventStoreExeFound);
            return finalEventStorePath;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this._eventstoreprocess.Kill();
                    this._eventstoreprocess.Dispose();
if (this._purgedata)
{
    Thread.Sleep(TimeSpan.FromSeconds(30));
    Directory.Delete(Path.Combine(Assembly.GetExecutingAssembly().GetExecutingFolder(), "data"), true);
    Directory.Delete(Path.Combine(Assembly.GetExecutingAssembly().GetExecutingFolder(), "data-logs"), true);
}
                    this._disposed = true;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}