using System;
using System.Configuration;

namespace HS.Microcore.Ninject
{
   
    /// <summary>
    /// Notice that REGEX_DEFAULT_MATCH_TIMEOUT can be set only once and will be determined when calling the first regex the default in infinite!!
    /// </summary>
    public class RegexTimeoutInitializer 
    {
        public void Init()
        {
            int regexDefaultMachTimeOutMs =(int) TimeSpan.FromSeconds(10).TotalMilliseconds;
            try
            {
                regexDefaultMachTimeOutMs = 10000;
            }
            catch (Exception e)
            {
            }

            AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT",TimeSpan.FromMilliseconds(regexDefaultMachTimeOutMs));
            Console.WriteLine($"REGEX_DEFAULT_MATCH_TIMEOUT is set to {regexDefaultMachTimeOutMs} ms");
        }
    }
}