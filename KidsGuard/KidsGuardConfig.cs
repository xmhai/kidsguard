using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace KidsComputerGuard
{
    public class KidsGuardConfig : ConfigurationSection
    {
        // to be consistent all the number related to time are in unit of second, timer is set to 1 second

        public static KidsGuardConfig GetConfig()
        {
            return (KidsGuardConfig)ConfigurationManager.GetSection("KidsGuard");
        }

        // Usage statistics update interval, default to 10 seconds
        [ConfigurationProperty("statisticsUpdateInterval", DefaultValue = "10", IsRequired = false)]
        public int StatisticsUpdateInterval
        {
            get
            {
                return (int)this["statisticsUpdateInterval"];
            }
            set
            {
                this["statisticsUpdateInterval"] = value;
            }
        }

        // save statistics to db interval, 60 seconds
        [ConfigurationProperty("statisticsSaveInterval", DefaultValue = "60", IsRequired = false)]
        public int StatisticsSaveInterval
        {
            get
            {
                return (int)this["statisticsSaveInterval"];
            }
            set
            {
                this["statisticsSaveInterval"] = value;
            }
        }

        // Session timeout default to 30 minutes (1800 seconds)
        [ConfigurationProperty("sessionTimeout", DefaultValue = "1800", IsRequired = false)]
        public int SessionTimeout
        {
            get
            {
                return (int)this["sessionTimeout"];
            }
            set
            {
                this["sessionTimeout"] = value;
            }
        }


        // Break time default to 5 minutes (300 seconds)
        [ConfigurationProperty("breakTime", DefaultValue = "300", IsRequired = false)]
        public int BreakTime
        {
            get
            {
                return (int)this["breakTime"];
            }
            set
            {
                this["breakTime"] = value;
            }
        }

        [ConfigurationProperty("BlockedApps")]
        [ConfigurationCollection(typeof(BlockedApp), AddItemName = "BlockedApp")]
        public BlockedAppCollection BlockedApps
        {
            get
            {
                return (BlockedAppCollection)this["BlockedApps"];
            }
        }

        // nested classes

        public class BlockedAppCollection : ConfigurationElementCollection
        {
            public BlockedApp this[int index]
            {
                get
                {
                    return base.BaseGet(index) as BlockedApp;
                }
                set
                {
                    if (base.BaseGet(index) != null)
                    {
                        base.BaseRemoveAt(index);
                    }
                    this.BaseAdd(index, value);
                }
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new BlockedApp();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((BlockedApp)element).Title;
            }
        }

        public class BlockedApp : ConfigurationElement
        {

            [ConfigurationProperty("title", IsRequired = true)]
            public string Title
            {
                get
                {
                    return (string)this["title"];
                }
            }

            [ConfigurationProperty("allowedTime", IsRequired = true)]
            public int AllowedTime
            {
                get
                {
                    return (int)this["allowedTime"];
                }
            }
        }
    }
}
