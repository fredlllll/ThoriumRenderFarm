﻿using System;
using System.Threading;
using Thorium.Shared;
using NLog;
using Thorium.Config;
using Thorium.Threading;

namespace Thorium.Client
{
    public class ThoriumClient : RestartableThreadClass
    {
        private static dynamic config = ConfigFile.GetClassConfig();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ServerInterface serverInterface;
        private ClientController clientController;

        //TODO: make id created by a pattern, so single machines can be identified
        public string Id { get; } = Utils.Utils.GetRandomGUID();

        private AutoResetEvent hasTaskEvent = new AutoResetEvent(false);
        private LightweightTask currentTask = null;

        public ThoriumClient() : base(false)
        {
            serverInterface = new ServerInterface(config.ServerHost, config.ServerListeningPort, this);
            clientController = new ClientController(this);
        }

        public override void Start()
        {
            serverInterface.InvokeRegister();
            base.Start();
        }

        public override void Stop(int joinTimeoutms = -1)
        {
            string id = currentTask?.Id;
            if(id != null)
            {
                serverInterface.InvokeAbandonTask(id, "Client Stopped");
            }
            serverInterface.InvokeUnregister();

            base.Stop(joinTimeoutms);
        }

        public void AssignTask(LightweightTask lightweightTask)
        {
            currentTask = lightweightTask;
            hasTaskEvent.Set();
        }

        protected override void Run()
        {
            DateTime lastTimeJobCompleted = DateTime.UtcNow;
            try
            {
                while(true)
                {
                    logger.Info("getting job...");
                    hasTaskEvent.WaitOne();
                    logger.Info("got task: " + currentTask.Id);

                    AExecutioner executioner = currentTask.GetExecutioner();
                    try
                    {
                        logger.Info("executing task");
                        var result = executioner.Execute();
                        switch(result.FinalAction)
                        {
                            case FinalAction.TurnIn:
                                serverInterface.InvokeTurnInTask(currentTask.Id, result.AdditionalInformation);
                                break;
                            case FinalAction.Abandon:
                                serverInterface.InvokeAbandonTask(currentTask.Id, result.AdditionalInformation);
                                break;
                            case FinalAction.Fail:
                                serverInterface.InvokeFailTask(currentTask.Id, result.AdditionalInformation);
                                break;
                        }

                        logger.Info("done task");
                    }
                    catch(Exception execEx) when(!(execEx is ThreadInterruptedException))
                    {
                        logger.Info("task failed: " + execEx);
                        serverInterface.InvokeFailTask(currentTask.Id, execEx.ToString());
                    }

                    lastTimeJobCompleted = DateTime.UtcNow;
                }
            }
            catch(ThreadInterruptedException)
            {
                //bye bye
                logger.Info("worker thread interrupted. exiting");
            }
            catch(Exception ex)
            {
                logger.Info("exception");
                logger.Info(ex);
            }
            logger.Info("leaving worker thread");
            Stop();
        }
    }
}
