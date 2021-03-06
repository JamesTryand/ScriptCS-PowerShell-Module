/*
    Copyright (c) 2013 Code Owls LLC, All Rights Reserved.

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/


using System.IO;
using System.Management.Automation;
using System.Reflection;
using ScriptCs;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;

namespace CodeOwls.PowerShell.ScriptCS
{
    [Cmdlet(VerbsLifecycle.Invoke, "ScriptCS")]
    public class InvokeScriptCsCmdlet : PSCmdlet
    {
        private static readonly string[] DefaultReferences = new[]
                                                                 {
                                                                     "System", "System.Core", "System.Data",
                                                                     "System.Data.DataSetExtensions", "System.Xml",
                                                                     "System.Xml.Linq"
                                                                 };

        private static readonly string[] DefaultNamespaces = new[]
                                                                 {
                                                                     "System", "System.Collections.Generic",
                                                                     "System.Linq",
                                                                     "System.Text", "System.Threading.Tasks"
                                                                 };

        public InvokeScriptCsCmdlet()
        {
            References = DefaultReferences;
            Namespaces = DefaultNamespaces;
        }

        [Parameter(ValueFromPipeline = true)]
        public object[] Input { get; set; }

        [Parameter(Mandatory = true, Position = 0)]
        [Alias("CS")]
        public string Script { get; set; }

        [Parameter(Mandatory = false)]
        public string[] References { get; set; }
        
        [Parameter(Mandatory = false)]
        public string[] Namespaces { get; set; }

        private IScriptEngine _engine;

        protected override void BeginProcessing()
        {
            Script = "var pscmdlet = Require<CodeOwls.PowerShell.ScriptCS.CurrentCmdletContext>(); " + Script;

            var logger = new CmdletLogger( this );
            IScriptHostFactory factory = new ScriptHostFactory();
            
            _engine = new RoslynScriptEngine(factory, logger);
            _engine.BaseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);                                  
        }

        protected override void EndProcessing()
        {
        }

        protected override void ProcessRecord()
        {
            var currentCmdletScriptPack = new CurrentCmdletScriptPack(this);
            var scriptPackSession = new ScriptPackSession(new IScriptPack[] { currentCmdletScriptPack });
            scriptPackSession.InitializePacks();

            var result = _engine.Execute(Script, References, Namespaces, scriptPackSession);

            WriteObject(result);

            currentCmdletScriptPack.Terminate();
        }
    }
}
