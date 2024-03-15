#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.RAEtherNetIP;
using FTOptix.CommunicationDriver;
using FTOptix.OPCUAServer;
#endregion
using System.Linq;
using FTOptix.WebUI;
using FTOptix.S7TCP;
using System.Threading.Tasks;
using System.Collections.Generic;
using Tag = FTOptix.RAEtherNetIP.Tag;

public class RuntimeNetLogic1 : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        //Folder mastertagfolder = Project.Current.Get<Folder>("CommDrivers/RAEtherNet_IPDriver1/RAEtherNet_IPStation1/Tags/Controller Tags");
        //var vs = mastertagfolder.FindNodesByType<IUAVariable>().Where<IUAVariable>(v => v.BrowseName != "SymbolName");
        //foreach (var v in vs)
        //{
        //    variableSynchronizer.Add(v);
        //    Log.Info(v.BrowseName);
        //}

    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        //variableSynchronizer.Dispose();
    }

    [ExportMethod]
    public void Transfer()
    {
        //Task task = new Task(() =>
        //{
        var asynchronousTask = new LongRunningTask(() =>
        {
            lock (_lock)
            {
                //For transfering data between PLC stations
                int[] a = new int[5] { 1, 2, 3, 4, 5 };
                float b = 22.3f;
                bool c = true;
                //Read from PLC station 1
                var myNode = Project.Current.Get("CommDrivers/RAEtherNet_IPDriver1/RAEtherNet_IPStation1/Tags/Controller Tags/motors");
                try
                {
                    var reads = myNode.ChildrenRemoteRead();
                    foreach (var item in reads)
                    {
                        if (item.Value.Value.GetType() == typeof(Int32[]))
                        {
                            Log.Info("Tag " + item.Value + " has value " + item.Value);
                            a = (Int32[])item.Value.Value;
                            foreach (var i in (Int32[])item.Value)
                            {
                                //a[i] = (Int32[])item.Value[i];
                                Log.Info("Tag " + item.RelativePath + " has value " + i);
                            }
                        }
                        else if (item.Value.Value.GetType() == typeof(Single))
                        {
                            b = (Single)item.Value.Value;
                            Log.Info("Tag " + item.RelativePath + " has value " + item.Value);
                        }
                        else if (item.Value.Value.GetType() == typeof(Boolean))
                        {
                            c = (Boolean)item.Value.Value;
                            Log.Info("Tag " + item.RelativePath + " has value " + item.Value);
                        }
                        Log.Info("Tag " + item.RelativePath + " has value " + item.Value);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("ChildrenRemoteRead failed: " + ex.ToString());
                }

                //Write to PLC station 2
                var valuesToWrite = new List<RemoteChildVariableValue>()
                {
                    new RemoteChildVariableValue("a", a),
                    new RemoteChildVariableValue("b", b),
                    new RemoteChildVariableValue("c", c)
                };
                var station2Node = Project.Current.Get("CommDrivers/RAEtherNet_IPDriver1/RAEtherNet_IPStation2/Tags/Controller Tags/fromMotor");
                try
                {
                    station2Node.ChildrenRemoteWrite(valuesToWrite, 500);
                }
                catch (Exception ex)
                {
                    Log.Error("ChildrenRemoteWrite failed: " + ex.ToString());
                }
            }
        },LogicObject);
        asynchronousTask.Start();
    }

    //private RemoteVariableSynchronizer variableSynchronizer = new RemoteVariableSynchronizer();
    private Object _lock = new Object();
}
