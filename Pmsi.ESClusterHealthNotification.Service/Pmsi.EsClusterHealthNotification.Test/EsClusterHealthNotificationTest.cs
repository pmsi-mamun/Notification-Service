using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pmsi.ElasticClusterInfo;
using Pmsi.Logger;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pmsi.EsClusterHealthNotification.Test
{
    [TestClass]
    public class EsClusterHealthNotificationTest
    {
        [TestMethod]
        public void get_elastic_cluster_info_method_test()
        {
            CLogger logger = new CLogger();
            logger.Initialize("C:\\temp", "BaseScraperTest - " + -1, true);
            ClusterInfo clusterinfo = new ClusterInfo();
            Dictionary<string,string> output = clusterinfo.GetElasticClusterInfo("http://67.202.79.81/_cluster/health?pretty=true", logger);
            
            Assert.AreNotEqual(output["status"],string.Empty);
        }

        [TestMethod]
        public void send_notification_method_test()
        {
            ClusterInfo cInfo = new ClusterInfo();
            string clusterhealth = "{\"cluster_name\":\"Pmsi.Live\",\"status\":\"green\",\"timed_out\":false,\"number_of_nodes\":4,\"number_of_data_nodes\":3,\"active_primary_shards\":89,\"active_shards\":178,\"relocating_shards\":0,\"initializing_shards\":0,\"unassigned_shards\":0}";
            Dictionary<string,string> output = JsonConvert.DeserializeObject<Dictionary<string, string>>(clusterhealth);
            bool response = cInfo.SendNotification(output, "smtp.gmail.com", 587, "notifications@pmsi-consulting.com", "n0t1fypmsi!", "notifications@pmsi-consulting.com", "mrashid@pmsi-consulting.com");

            Assert.AreEqual(response, true);
        }
    }
}
