﻿using Newtonsoft.Json.Linq;
using NLog;
using Thorium_Shared;
using Thorium_Shared.Net.ServicePoint;
using static Thorium_Shared.Net.ServerControlCommands;

namespace Thorium_Server
{
    public class ServerController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ThoriumServer server;

        private ServicePoint servicePoint = new ServicePoint();

        public ServerController(ThoriumServer server)
        {
            this.server = server;

            servicePoint.RegisterInvokationReceiver(new HttpServiceInvokationReceiver());
            servicePoint.RegisterInvokationReceiver(new TCPServiceInvokationReceiver(ThoriumServerConfig.ListeningPort));

            servicePoint.RegisterRoutine(new Routine(AddJob, AddJobHandler));
            servicePoint.RegisterRoutine(new Routine(ListClients, ListClientsHandler));
            servicePoint.RegisterRoutine(new Routine(ListJobs, ListJobsHandler));
            servicePoint.RegisterRoutine(new Routine(ListTasks, ListTasksHandler));
            servicePoint.RegisterRoutine(new Routine(AbortJob, AbortJobHandler));
            servicePoint.RegisterRoutine(new Routine(AbortTask, AbortTaskHandler));
        }

        public void Start()
        {
            servicePoint.Start();
        }

        public void Stop()
        {
            servicePoint.Stop();
        }

        JToken AddJobHandler(JToken arg)
        {
            JObject argObject = (JObject)arg;

            Job j = new Job(Utils.GetRandomID(), argObject.Get<string>("jobName"), (JObject)argObject["jobInformation"]);
            logger.Info("new Job Added: " + j.ID + ", " + j.Name + ", " + j.Information);
            server.JobManager.AddJob(j);
            JObject retval = new JObject
            {
                ["id"] = j.ID
            };
            return retval;
        }

        JToken ListClientsHandler(JToken arg)
        {
            JArray retval = new JArray();
            foreach(var c in server.ClientManager.ClientsSnapshot)
            {
                retval.Add(c.ID);
            }
            return retval;
        }

        JToken ListJobsHandler(JToken arg)
        {
            JArray retval = new JArray();
            foreach(var j in server.JobManager.Jobs)
            {
                retval.Add(j.Key);
            }
            return retval;
        }

        JToken ListTasksHandler(JToken arg)
        {
            JArray retval = new JArray();
            foreach(var t in server.TaskManager.Tasks)
            {
                retval.Add(t.ID);
            }
            return retval;
        }

        JToken AbortJobHandler(JToken arg)
        {
            JObject argObject = (JObject)arg;

            string id = argObject.Get<string>("id");
            foreach(var t in server.TaskManager.Tasks)
            {
                if(t.Job.ID == id)
                {
                    server.TaskManager.AbortTask(t.ID);
                }
            }
            return null;
        }

        JToken AbortTaskHandler(JToken arg)
        {
            JObject argObject = (JObject)arg;

            string id = argObject.Get<string>("id");
            server.TaskManager.AbortTask(id);

            return null;
        }
    }
}