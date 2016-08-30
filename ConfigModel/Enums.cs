using System;
using System.Collections.Generic;
using System.Text;

namespace ContactRoute.Config
{
    /// <summary>
    /// The mode of the configuration model. The selected mode will change the behavoir of what is displayed, read and write to the config file(s)
    /// </summary>
    public enum ConfigMode
    {
        /// <summary>
        /// This is the default mode. User values will be read and write from/to the user file and other values to the general config. This mode should display all the option
        /// </summary>
        Default,
        /// <summary>
        /// This is the system mode. All values will be read or writen to the default file. This allows the configurator to put default user values. This mode should display all the options.
        /// </summary>
        System,
        /// <summary>
        /// This is the user mode. Is the same a the default mode but will only write to the user file and only display the user option. This is to allow the user to change his settings.
        /// </summary>
        User,
    }
}
