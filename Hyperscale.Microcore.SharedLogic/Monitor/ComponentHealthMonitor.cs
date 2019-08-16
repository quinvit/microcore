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
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Metrics.Health;
using App.Metrics.Health.Builder;

namespace Hyperscale.Microcore.SharedLogic.Monitor
{
    public sealed class ComponentHealthMonitor : IDisposable
    {
        private Func<ValueTask<HealthCheckResult>> _healthFunction;
        private Func<Dictionary<string, string>> _getHealthData;
        private readonly string _component;
        private readonly HealthBuilder healthBuilder = new HealthBuilder();
        private bool _active;

        public ComponentHealthMonitor(string component, Func<ValueTask<HealthCheckResult>> func)
        {
            _component = component;
            SetHealthFunction(func);
        }

        public void Activate()
        {
            if (!_active)
            {
                healthBuilder.HealthChecks.AddCheck(_component, () => CheckFunction());
                _active = true;
            }
        }

        public void Deactivate()
        {
            if (_active)
            {
                _active = false;
            }
        }

        public void SetHealthFunction(Func<ValueTask<HealthCheckResult>> func)
        {
            if(func != null)
            {
                _healthFunction = func;
                return;
            }

            throw new ArgumentNullException(nameof(func));
        }

        private ValueTask<HealthCheckResult> CheckFunction()
        {
            return _healthFunction.Invoke();
        }

        public void SetHealthData(Func<Dictionary<string, string>> getHealthData)
        {
            _getHealthData = getHealthData;
        }

        public Dictionary<string, string> GetHealthData()
        {
            if (_getHealthData == null)
                return new Dictionary<string, string>();
            else
                return _getHealthData();
        }

        public void Dispose()
        {
            Deactivate();
        }
    }
}
