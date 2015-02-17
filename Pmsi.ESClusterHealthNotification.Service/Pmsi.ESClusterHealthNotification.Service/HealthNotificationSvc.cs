using Newtonsoft.Json;
using Pmsi.ESClusterHealthNotification.Service.Properties;
using Pmsi.Logger;
using Pmsi.ElasticClusterInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pmsi.ESClusterHealthNotification.Service
{
    public partial class HealthNotificationSvc : ServiceBase
    {
        CLogger _logger;

        public HealthNotificationSvc()
        {
            InitializeComponent();
        }

        Thread thr = null;
        protected override void OnStart(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
            InitializeLoggers();
            thr = new Thread(new ThreadStart(Notify));
            thr.Name = "Notify Thread";

            _logger.LogInformation(Severity.Normal, "ScraperBotSvc", "OnStart", string.Format("[{0}] Starting scraper herd service...", Thread.CurrentThread.Name));
            thr.Start();
        }

        /// <summary>
        /// Send notification if cluster health is yellow/red/time hour is 22
        /// </summary>
        void Notify()
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
            ClusterInfo clusterInfo = new ClusterInfo();
            string status = string.Empty;
            List<string> checkList = Settings.Default.StatusList.Split(',').ToList();

            while(true)
            {
                status = string.Empty;
                try
                {
                    var info = clusterInfo.GetElasticClusterInfo(Settings.Default.requestUrl,_logger);
                    if (info.ContainsKey("status") && checkList.Any(info["status"].Equals))
                        clusterInfo.SendNotification(info, Settings.Default.SmtpHost, Settings.Default.SmtpPort, Settings.Default.SmtpUser, Settings.Default.SmtpPass, Settings.Default.Sender, Settings.Default.NotificationEmails);                    
                    else if (DateTime.Now.Hour.Equals(Settings.Default.DNotificationTime))
                        clusterInfo.SendNotification(info, Settings.Default.SmtpHost, Settings.Default.SmtpPort, Settings.Default.SmtpUser, Settings.Default.SmtpPass, Settings.Default.Sender, Settings.Default.NotificationEmails);                    

                }
                catch (Exception ex)
                {
                    _logger.LogError(Severity.High, "HealthNotificationSvc", "Notify", string.Format("[{0}] Failed to notify elastic cluster health - {1}", Thread.CurrentThread.Name, ex));
                }

                Thread.Sleep(new TimeSpan(0, 0, Settings.Default.SleepTimeInSec));
            }
            OnStop();
        }


        protected override void OnStop()
        {
            try
            {
                thr.Abort();
                _logger.LogInformation(Severity.Normal, "HealthNotificationSvc", "OnStop", string.Format("[{0}] Exiting cluster health notification service!", Thread.CurrentThread.Name));
            }
            catch (Exception ex)
            {
                _logger.LogError(Severity.High, "HealthNotificationSvc", "OnStop", string.Format("[{0}] Eternal Destruction Error - {1}", Thread.CurrentThread.Name, ex));
            }
             
        }

        /// <summary>
        /// Initialize logger
        /// </summary>
        void InitializeLoggers()
        {
            string logFilePath = Settings.Default.LogLocation;
            if (string.IsNullOrEmpty(logFilePath)) logFilePath = AppDomain.CurrentDomain.BaseDirectory + "Logs";

            _logger = new CLogger();
            _logger.Initialize(logFilePath, "EsCluster Health Notification", true, 1);
        }
       
    }
}
