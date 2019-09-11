#region Copyright 
// Copyright 2017 Gygya Inc.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.  
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;
using Hyperscale.Common.Contracts.Exceptions;
using Hyperscale.Microcore.Interfaces.Configuration;
using Hyperscale.Microcore.Interfaces.SystemWrappers;

namespace Hyperscale.Microcore.SharedLogic.SystemWrappers
{
    [ConfigurationRoot("dataCenters", RootStrategy.ReplaceClassNameWithPath)]
    public class DataCentersConfig : IConfigObject
    {
        public string Current { get; set; }
    }

    public class EnvironmentInstance : IEnvironment
    {
        private readonly string _region;
        private Func<DataCentersConfig> GetDataCentersConfig { get; }

        public EnvironmentInstance(Func<DataCentersConfig> getDataCentersConfig)
        {
            GetDataCentersConfig = getDataCentersConfig;
            Zone = Environment.GetEnvironmentVariable("ZONE") ?? Environment.GetEnvironmentVariable("DC");
            _region = Environment.GetEnvironmentVariable("REGION");
            DeploymentEnvironment = Environment.GetEnvironmentVariable("ENV");
            ConsulAddress = Environment.GetEnvironmentVariable("CONSUL");

            if (string.IsNullOrEmpty(Zone))
            {
                Zone = "dc1";
            }

            if (string.IsNullOrEmpty(_region))
            {
                Zone = "vn";
            }

            if (string.IsNullOrEmpty(DeploymentEnvironment))
            {
                Zone = "dev";
            }
        }

        public string Zone { get; }
        public string Region => _region ?? GetDataCentersConfig().Current; // if environmentVariable %REGION% does not exist, take the region from DataCenters configuration (the region was previously called "DataCenter")
        public string DeploymentEnvironment { get; }        
        public string ConsulAddress { get; }
    }
}