using Newtonsoft.Json;
using Pmsi.Logger;
using Pmsi.Utility.Notifications;
using Pmsi.WebScraper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pmsi.ElasticClusterInfo
{
    public class ClusterInfo
    {
        /// <summary>
        /// Returns Cluster information
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetElasticClusterInfo(string url, CLogger logger)
        {
            WebSession ws = new WebSession(logger);
            Dictionary<string, string> clusterInfo = new Dictionary<string, string>();
                ScrapeRequest sr = new ScrapeRequest();
                try
                {
                    sr.UseProxy = false;
                    sr.Method = "GET";
                    sr.TargetUrl.Url = url;

                    string json = ws.GetWebResponse(sr).Html;

                    clusterInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
                catch (Exception ex)
                {
                    logger.LogError(Severity.High, "HealthNotificationSvc", "Notify", string.Format("[{0}] Failed to notify elastic cluster health - {1}", Thread.CurrentThread.Name, ex));
                }


                return clusterInfo;
        }

        /// <summary>
        /// Send Cluster helth information via email
        /// </summary>
        /// <param name="clusterInfo"></param>
        /// <param name="smtpHost"></param>
        /// <param name="smtpPort"></param>
        /// <param name="smtpUser"></param>
        /// <param name="smtpPass"></param>
        /// <param name="notificationEmails"></param>
        public bool SendNotification(Dictionary<string, string> clusterInfo, string smtpHost, int smtpPort, string smtpUser, string smtpPass, string sender, string notificationEmails)
        {
            bool success = false;
            EmailNotification notification = new EmailNotification(smtpHost, smtpPort, smtpUser, smtpPass);
            List<string> to = notificationEmails.Split(';').ToList();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("<h1>Cluster Health is-> {0}</h1><br/>", clusterInfo["status"]));
            sb.AppendLine(string.Format("\n cluster_name -> {0}<br/>", clusterInfo["cluster_name"]));
            sb.AppendLine(string.Format("\n timed_out -> {0}<br/>", clusterInfo["timed_out"]));
            sb.AppendLine(string.Format("\n number_of_nodes -> {0}<br/>", clusterInfo["number_of_nodes"]));
            sb.AppendLine(string.Format("\n number_of_data_nodes -> {0}<br/>", clusterInfo["number_of_data_nodes"]));
            sb.AppendLine(string.Format("\n active_primary_shards -> {0}<br/>", clusterInfo["active_primary_shards"]));
            sb.AppendLine(string.Format("\n active_shards -> {0}", clusterInfo["active_shards"]));
            sb.AppendLine(string.Format("\n relocating_shards -> {0}<br/>", clusterInfo["relocating_shards"]));
            sb.AppendLine(string.Format("\n initializing_shards -> {0}<br/>", clusterInfo["initializing_shards"]));
            sb.AppendLine(string.Format("\n unassigned_shards -> {0}<br/>", clusterInfo["unassigned_shards"]));

            success = notification.Notify(to, sender, sb.ToString(), "Elastic Cluster Health information - "+DateTime.Now.ToString());

            return success;
        }    
    }
}
